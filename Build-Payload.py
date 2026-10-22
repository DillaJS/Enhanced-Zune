"""Private local build. Never writes to the source Zune installation."""
from pathlib import Path
import sys,json,hashlib,gzip,struct,ctypes,shutil,zipfile
R=Path(__file__).resolve().parent; P=R.parent.parent
sys.path.insert(0,str(P/'work/profiling/python-packages'))
import pefile
sys.path.insert(0,str(R.parent/'Zune-Community-Package'))
from ReleasePolicy import verify_source
verify_source()
sha=lambda x:hashlib.sha256(x).hexdigest().upper()
source=R/'private-evidence';out=R/'build';out.mkdir(exist_ok=True)
payload=out/'payload';payload.mkdir(exist_ok=True)
rows=json.loads((source/'stock-comparison.json').read_text())
stock=R.parent/'Zune-Community-Package/installer-extracted'
def resources(data):
    pe=pefile.PE(data=data);result={}
    for t in getattr(pe,'DIRECTORY_ENTRY_RESOURCE',[]).entries:
        for n in t.directory.entries:
            for l in n.directory.entries:
                at=pe.get_offset_from_rva(l.data.struct.OffsetToData);size=l.data.struct.Size
                result[(t.id if t.id is not None else str(t.name),str(n.name or n.id),l.id)]=(at,size)
    return result
# Remove personal slot payloads from the COPY, using the same existing fallback
# and metadata structure. Every other resource must remain byte identical.
shell=source/'ZuneShellResources.dll';clean=out/'ZuneShellResources.clean.dll'
shutil.copy2(shell,clean)
raw=shell.read_bytes();rr=resources(raw);changes={}
at,n=rr[(10,'BUTTONDARK.FRAMEBACKGROUND06.PNG',1033)];fallback=raw[at:at+n]
for i in range(1,4):changes[(10,f'FRAMEBACKGROUNDCUSTOM{i:02}.PNG',1033)]=fallback
changes[(10,'CUSTOMBACKGROUND.SLOTS.V1',1033)]=b'ZUNE DLL CUSTOM BACKGROUNDS 1\n\n\n'
k=ctypes.WinDLL('kernel32',use_last_error=True)
k.BeginUpdateResourceW.argtypes=[ctypes.c_wchar_p,ctypes.c_bool];k.BeginUpdateResourceW.restype=ctypes.c_void_p
k.UpdateResourceW.argtypes=[ctypes.c_void_p,ctypes.c_void_p,ctypes.c_wchar_p,ctypes.c_ushort,ctypes.c_void_p,ctypes.c_uint];k.UpdateResourceW.restype=ctypes.c_bool
k.EndUpdateResourceW.argtypes=[ctypes.c_void_p,ctypes.c_bool];k.EndUpdateResourceW.restype=ctypes.c_bool
h=k.BeginUpdateResourceW(str(clean),False);assert h
for (t,name,lang),b in changes.items():assert k.UpdateResourceW(h,t,name,lang,ctypes.create_string_buffer(b),len(b))
assert k.EndUpdateResourceW(h,False)
new=clean.read_bytes();nr=resources(new)
assert set(nr)==set(rr)
for key,(at,n) in rr.items():
    off,size=nr[key];assert new[off:off+size]==changes.get(key,raw[at:at+n]),key
a,b=pefile.PE(data=raw),pefile.PE(data=new)
for x,y in zip(a.sections,b.sections):
    if x.Name.rstrip(b'\0')!=b'.rsrc':assert x.get_data()==y.get_data()
(source/'privacy-sanitization.json').write_text(json.dumps({'source_hash':sha(raw),'clean_hash':sha(new),'only_changed_resource_names':[x[1] for x in changes],'all_other_resource_payloads_identical':True,'all_nonresource_sections_identical':True},indent=2))
def delta(old,new):
    # Copy/XOR commands reference corresponding original resource offsets. New
    # material is encoded as insert commands. Not a redistribution license.
    stream=bytearray(b'ZRDELTA1'+struct.pack('<QQ',len(old),len(new)))
    def emit(at,data):
        if not data:return
        if at is None or at>=len(old):stream.extend(b'\x02'+struct.pack('<I',len(data))+data);return
        if at+len(data)>len(old):
            n=len(old)-at;emit(at,data[:n]);emit(None,data[n:]);return
        before=old[at:at+len(data)]
        if before==data:stream.extend(b'\x00'+struct.pack('<QI',at,len(data)))
        else:stream.extend(b'\x01'+struct.pack('<QI',at,len(data))+bytes(x^y for x,y in zip(before,data)))
    aa,bb=resources(old),resources(new);pos=0
    for key,(at,n) in sorted(bb.items(),key=lambda x:x[1][0]):
        if at<pos:continue
        if at>pos:emit(pos,new[pos:at])
        data=new[at:at+n]
        if key in aa:
            off,count=aa[key];common=min(count,n);emit(off,data[:common]);emit(None,data[common:])
        else:emit(None,data)
        pos=at+n
    emit(pos,new[pos:]);stream.append(255)
    return gzip.compress(stream,compresslevel=9,mtime=0)
files=[];reslog=[]
for row in rows:
    if not row['different']:continue
    old=(stock/row['stock_id']).read_bytes();name=row['name']
    current=(clean if name=='ZuneShellResources.dll' else source/name).read_bytes()
    patch=delta(old,current);fn=name+'.gz';(payload/fn).write_bytes(patch)
    files.append(dict(name=name,original_sha256=sha(old),patched_sha256=sha(current),original_size=len(old),patched_size=len(current),patch=fn,patch_sha256=sha(patch),kind='delta'))
    aa,bb=resources(old),resources(current);diff=[]
    for key,(at,n) in bb.items():
        off,count=aa.get(key,(0,0))
        if key not in aa or old[off:off+count]!=current[at:at+n]:diff.append('/'.join(map(str,key)))
    reslog.append(dict(name=name,changed_resources=diff))
for name in ['ZuneScalingSettings.dll','ZuneBackgroundUpdater.exe','ZuneLocalArtistPhotos.dll']:
    data=(source/name).read_bytes();patch=gzip.compress(data,mtime=0);fn=name+'.gz';(payload/fn).write_bytes(patch)
    files.append(dict(name=name,original_sha256='',patched_sha256=sha(data),original_size=0,patched_size=len(data),patch=fn,patch_sha256=sha(patch),kind='new'))
# Verify the entire original dependency set; allow only untouched files from the
# exact supported media. No whole-installation transplant or extra files.
deps=[dict(name=x['name'],sha256=x['stock_sha256']) for x in rows if not x['different'] and x['name'].lower().endswith(('.exe','.dll','.manifest','.config'))]
msi=R.parent.parent/'work/Zune-Download-Extraction/extracted/x64/packages/zune-x64.msi'
m=dict(format='ZRDELTA1',version='1.0.0-rc.6',zune_version='4.8.2345.0',architecture='x64',files=files,dependencies=deps,original_msi_sha256=sha(msi.read_bytes()),original_msi_size=msi.stat().st_size,private_candidate=True)
media=msi.parent.parent
m['original_media']=[dict(name=p.relative_to(media).as_posix(),sha256=sha(p.read_bytes()),size=p.stat().st_size) for p in sorted(media.rglob('*')) if p.is_file()]
shellbytes=clean.read_bytes();pe=pefile.PE(data=shellbytes)
m['shell_resources']={('#'+str(t) if isinstance(t,int) else t)+'|'+('#'+n if n.isdecimal() else n)+'|'+str(l):sha(shellbytes[at:at+size]) for (t,n,l),(at,size) in resources(shellbytes).items() if (t,n,l) not in changes}
m['shell_sections']={s.Name.rstrip(b'\0').decode():sha(s.get_data()) for s in pe.sections if s.Name.rstrip(b'\0')!=b'.rsrc'}
def header(data):
    p=pefile.PE(data=data);h=bytearray(data[:p.OPTIONAL_HEADER.SizeOfHeaders]);nt=p.DOS_HEADER.e_lfanew;opt=nt+24
    directories=112 if p.OPTIONAL_HEADER.Magic==0x20b else 96
    for off in [nt+8,opt+8,opt+56,opt+64,opt+directories+20,opt+directories+32,opt+directories+36]:h[off:off+4]=b'\0'*4
    for s in p.sections:
        if s.Name.rstrip(b'\0')==b'.rsrc':
            at=s.get_file_offset()
            for off in [at+8,at+16]:h[off:off+4]=b'\0'*4
    return sha(h)
m['shell_header']=header(shellbytes)
(payload/'manifest.json').write_text(json.dumps(m,indent=2))
for folder,metadata in [('Zune-Public-Release-RC5','previous-release.json'),('Zune-Public-Release-RC4','earlier-release.json'),('Zune-Public-Release-RC3','oldest-release.json'),('Zune-Public-Release-RC2','oldest2-release.json'),('Zune-Public-Release','oldest3-release.json')]:
    shutil.copy2(R.parent/folder/'build/payload/manifest.json',payload/metadata)
(R/'RESOURCE-CHANGES.json').write_text(json.dumps(reslog,indent=2))
with zipfile.ZipFile(out/'payload.zip','w',zipfile.ZIP_STORED) as z:
    for p in sorted(payload.iterdir()):
        info=zipfile.ZipInfo(p.name,(2026,10,16,0,0,0));info.compress_type=zipfile.ZIP_STORED;z.writestr(info,p.read_bytes())
print(json.dumps({'files':len(files),'dependencies':len(deps),'payload_bytes':(out/'payload.zip').stat().st_size,'privacy':'all slots cleared in release copy only'},indent=2))
