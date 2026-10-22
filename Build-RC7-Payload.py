"""Build RC7 from the verified prior release plus the scoped playback changes.
Requires private, legitimate original-media fixtures; never writes to live Zune.
Rebuilding the installer from its frozen payload does not require these fixtures.
"""
from pathlib import Path
import sys,json,hashlib,gzip,struct,ctypes,shutil,zipfile
R=Path(__file__).resolve().parent;P=R.parent.parent
sys.path.insert(0,str(P/'work/profiling/python-packages'))
import pefile
sys.path.insert(0,str(R.parent/'Zune-Community-Package'))
from ReleasePolicy import verify_source,verify_package
verify_source()
sha=lambda b:hashlib.sha256(b).hexdigest().upper()
e=R/'private-evidence';out=R/'build';payload=out/'payload';payload.mkdir(exist_ok=True)
previous=R.parent/'Zune-Public-Release-RC6-2/build/payload'
m=json.loads((previous/'manifest.json').read_text(encoding='utf-8-sig'))
old_manifest=json.loads(json.dumps(m))

def resources(data):
 p=pefile.PE(data=data)
 return {(t.id if t.id is not None else str(t.name),str(n.name or n.id),l.id):(p.get_offset_from_rva(l.data.struct.OffsetToData),l.data.struct.Size) for t in p.DIRECTORY_ENTRY_RESOURCE.entries for n in t.directory.entries for l in n.directory.entries}

# Only General settings are replaced; prior public slot sanitization, defaults,
# installation-relative photo loading, and every other enhancement are retained.
base=(e/'ZuneShellResources.dll').read_bytes();old_resources=resources(base)
test=(P/'outputs/Zune-Crossfade-Integration/Zune-Test/ZuneShellResources.dll').read_bytes()
at,size=resources(test)[(10,'MANAGEMENTGENERAL.UIX',1033)];general=test[at:at+size]
target=out/'ZuneShellResources.clean.dll';target.write_bytes(base)
k=ctypes.WinDLL('kernel32',use_last_error=True)
k.BeginUpdateResourceW.argtypes=[ctypes.c_wchar_p,ctypes.c_bool];k.BeginUpdateResourceW.restype=ctypes.c_void_p
k.UpdateResourceW.argtypes=[ctypes.c_void_p,ctypes.c_void_p,ctypes.c_wchar_p,ctypes.c_ushort,ctypes.c_void_p,ctypes.c_uint];k.UpdateResourceW.restype=ctypes.c_bool
k.EndUpdateResourceW.argtypes=[ctypes.c_void_p,ctypes.c_bool];k.EndUpdateResourceW.restype=ctypes.c_bool
h=k.BeginUpdateResourceW(str(target),False);assert h
assert k.UpdateResourceW(h,10,'MANAGEMENTGENERAL.UIX',1033,ctypes.create_string_buffer(general),len(general))
assert k.EndUpdateResourceW(h,False),ctypes.WinError(ctypes.get_last_error())
clean=target.read_bytes();new_resources=resources(clean);changed=[]
for key,(at,size) in old_resources.items():
 bpos,bsize=new_resources[key];a=base[at:at+size];b=clean[bpos:bpos+bsize]
 if a!=b:changed.append(key)
assert changed==[(10,'MANAGEMENTGENERAL.UIX',1033)],changed
a,b=pefile.PE(data=base),pefile.PE(data=clean)
for section in a.sections:
 if section.Name.rstrip(b'\0')!=b'.rsrc':assert section.get_data()==next(s for s in b.sections if s.Name==section.Name).get_data()
pos,count=new_resources[(10,'CUSTOMBACKGROUND.SLOTS.V1',1033)]
assert clean[pos:pos+count]==b'ZUNE DLL CUSTOM BACKGROUNDS 1\n\n\n'

# Use the existing audited delta encoder, kept in the included build source.
from PayloadCodec import delta,header
mapping={r['name']:r['id'] for r in json.loads((R.parent/'Zune-Community-Package/installer-file-map.json').read_text())}
stock=R.parent/'Zune-Community-Package/installer-extracted'
for row in m['files']:
 if row['name'] in ['ZuneShell.dll','ZuneShellResources.dll','ZuneScalingSettings.dll']:
  data=clean if row['name']=='ZuneShellResources.dll' else (e/row['name']).read_bytes()
  if row['kind']=='new':patch=gzip.compress(data,mtime=0)
  else:
   original=(stock/mapping[row['name']]).read_bytes();assert sha(original)==row['original_sha256'];patch=delta(original,data)
  row.update(patched_sha256=sha(data),patched_size=len(data),patch_sha256=sha(patch))
  (payload/row['patch']).write_bytes(patch)
 else:shutil.copy2(previous/row['patch'],payload/row['patch'])
for name,source in [('ZuneCrossfadeBridge.dll',e/'ZuneCrossfadeBridge.dll'),('ZuneCrossfadeHost.exe',out/'host/ZuneCrossfadeHost.exe'),('ZuneCrossfade-Licenses.txt',R/'ZuneCrossfade-Licenses.txt')]:
 data=source.read_bytes();patch=gzip.compress(data,mtime=0);filename=name+'.gz';(payload/filename).write_bytes(patch)
 m['files'].append(dict(name=name,original_sha256='',patched_sha256=sha(data),original_size=0,patched_size=len(data),patch=filename,patch_sha256=sha(patch),kind='new'))
m['version']='1.0.0-rc.7'
m['crossfade']={'default_enabled':False,'duration_seconds':[0,10],'self_contained_host':True,'runtime_version':'8.0.31','naudio_version':'2.2.1','preference_key':r'HKCU\Software\EnhancedZune\Playback'}
slots={f'FRAMEBACKGROUNDCUSTOM{i:02}.PNG' for i in range(1,4)}|{'CUSTOMBACKGROUND.SLOTS.V1'}
m['shell_resources']={('#'+str(t) if isinstance(t,int) else t)+'|'+('#'+n if n.isdecimal() else n)+'|'+str(l):sha(clean[at:at+size]) for (t,n,l),(at,size) in new_resources.items() if n not in slots}
pe=pefile.PE(data=clean);m['shell_sections']={s.Name.rstrip(b'\0').decode():sha(s.get_data()) for s in pe.sections if s.Name.rstrip(b'\0')!=b'.rsrc'}

m['shell_header']=header(clean)
(payload/'manifest.json').write_text(json.dumps(m,indent=2),encoding='utf-8')
for old in previous.glob('*release.json'):shutil.copy2(old,payload/old.name)
(payload/'rc6-release.json').write_text(json.dumps(old_manifest,indent=2),encoding='utf-8')
with zipfile.ZipFile(out/'payload.zip','w',zipfile.ZIP_STORED) as z:
 for p in sorted(payload.iterdir()):
  info=zipfile.ZipInfo(p.name,(2026,10,20,0,0,0));info.compress_type=zipfile.ZIP_STORED;z.writestr(info,p.read_bytes())
audit={'only_previous_runtime_changes':['ZuneShell.dll','ZuneScalingSettings.dll','ZuneShellResources.dll'],'only_ui_resource_changed':'MANAGEMENTGENERAL.UIX','all_other_public_runtime_payloads_preserved':True,'private_backgrounds_absent':True,'components':len(m['files']),'crossfade_off_by_default':True,'runtime_bundled':True,'payload_bytes':(out/'payload.zip').stat().st_size,'release_policy':verify_package(payload)}
(R/'PAYLOAD-REFRESH-AUDIT.json').write_text(json.dumps(audit,indent=2),encoding='utf-8')
print(json.dumps({k:v for k,v in audit.items() if k!='release_policy'},indent=2))
