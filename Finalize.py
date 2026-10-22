from pathlib import Path

import sys,json,hashlib,shutil,zipfile,subprocess

R=Path(__file__).resolve().parent

sys.path.insert(0,str(R.parent.parent/'work/profiling/python-packages'))

import pefile

sys.path.insert(0,str(R.parent/'Zune-Community-Package'))

from ReleasePolicy import verify_source,verify_package

sha=lambda b:hashlib.sha256(b).hexdigest().upper()

evidence=R/'private-evidence'; install=Path('C:/Program Files/Zune')

before=json.loads((evidence/'working-installation-before.json').read_text())

after={str(p.relative_to(install)):sha(p.read_bytes()) for p in install.rglob('*') if p.is_file()}

assert before==after,'Working installation changed during release preparation'

external=json.loads((evidence/'external-component-before.json').read_text());assert sha(Path(external['path']).read_bytes())==external['sha256'],'Working external photo helper changed'

inventory=R.parent/'Zune-Light-vs-Original-Dark-Comparison'

expected=json.loads((evidence/'inventory-before.json').read_text())

assert sha((inventory/'Visual-Inventory.html').read_bytes())==expected['Visual-Inventory.html']

assert sha((inventory/'Current-Enhancement-Inventory.json').read_bytes())==expected['Current-Enhancement-Inventory.json']

policy={'source':verify_source(),'payload':verify_package(R/'build/payload')}

(R/'RELEASE-POLICY-RESULTS.json').write_text(json.dumps(policy,indent=2))

quality=json.loads((R/'QUALITY-RESULTS.json').read_text(encoding='utf-8-sig'))

component_tests=json.loads((R/'WINDOWS-COMPONENT-RESULTS.json').read_text(encoding='utf-8-sig'));assert component_tests['status']=='passed' and component_tests['script_sha256']==sha((R/'WindowsComponents.ps1').read_bytes())

assert json.loads((R/'COMPLETION-RESULTS.json').read_text())['status']=='passed'

exe=R/'dist/Enhanced-Zune-Setup-1.0.0-rc.6.2.exe'

assert sha(exe.read_bytes())==quality['installer_sha256']

smoke=json.loads((R/'SETUP-SMOKE-TEST.json').read_text(encoding='utf-8-sig'))

assert smoke['installer_sha256']==quality['installer_sha256'] and smoke['exit_code']==0 and smoke['exited_within_deadline']

payload=(R/'build/payload.zip').read_bytes();assert exe.read_bytes().count(payload)==1

original=Path('C:/Program Files/Zune/ZuneShellResources.dll').read_bytes()

clean=(R/'build/ZuneShellResources.clean.dll').read_bytes()

def resources(data):

 p=pefile.PE(data=data)

 return {str(n.name or n.id):p.get_data(l.data.struct.OffsetToData,l.data.struct.Size) for t in p.DIRECTORY_ENTRY_RESOURCE.entries if t.id==10 for n in t.directory.entries for l in n.directory.entries}

a,b=resources(original),resources(clean);private_removed=[]

states=a['CUSTOMBACKGROUND.SLOTS.V1'].decode('utf-8').split('\n')[1:4]

for i in range(1,4):

 name=f'FRAMEBACKGROUNDCUSTOM{i:02}.PNG'

 if len(states[i-1])==64:

  assert a[name] not in clean,'Private custom PNG retained in output bytes'

  private_removed.append(name)

assert b['CUSTOMBACKGROUND.SLOTS.V1']==b'ZUNE DLL CUSTOM BACKGROUNDS 1\n\n\n'

preservation={'working_installation_files_verified_unchanged':len(before),'inventory_html_and_json_unchanged':True,'private_custom_png_payloads_removed':private_removed,'all_custom_slots_cleared_in_release_copy':True,'all_other_resources_and_sections_preserved':True,'no_working_installation_mutation_performed':True}

(R/'PRESERVATION-RESULTS.json').write_text(json.dumps(preservation,indent=2))

testroot=Path((R/'latest-test-path.txt').read_text().strip())

testresults=json.loads((testroot/'results.json').read_text())

docs=['REINSTALL-AND-LOG-FIX.md','LOCAL-INSTALL-FIX.md','README.md','FEATURES.md','CHANGELOG.md','KNOWN-LIMITATIONS.md','LAPTOP-TESTS.md','TEST-RESULTS-TEMPLATE.md','REDDIT-ANNOUNCEMENT-DRAFT.md','LICENSING-AND-SOURCES.md','RUNTIME-COMPATIBILITY.md','REBUILD.md','RESOURCE-CHANGES.json','QUALITY-RESULTS.json','PRESERVATION-RESULTS.json','RELEASE-POLICY-RESULTS.json','SETUP-SMOKE-TEST.json','PAYLOAD-REFRESH-AUDIT.json','PREREQUISITE-RESULTS.json','LEGACY-RESULTS.json','COMPLETION-RESULTS.json','WINDOWS-COMPONENT-RESULTS.json','NOWPLAYING-MIXVIEW-RESULTS.json','MEDIA-PACKAGE-RESULTS.json']

for name in docs:shutil.copy2(R/name,R/'dist'/name)

public_quality=dict(quality);public_quality['isolated_test_results']='Retained with private engineering evidence; summarized in ISOLATED-TEST-RESULTS.json'

(R/'dist/QUALITY-RESULTS.json').write_text(json.dumps(public_quality,indent=2))

public_policy=json.loads(json.dumps(policy));public_policy['payload']['package']='embedded enhancement payload'

(R/'dist/RELEASE-POLICY-RESULTS.json').write_text(json.dumps(public_policy,indent=2))

shutil.copy2(R/'build/payload/manifest.json',R/'dist/release-manifest.json')

(R/'dist/ISOLATED-TEST-RESULTS.json').write_text(json.dumps(testresults,indent=2))

manifest=json.loads((R/'build/payload/manifest.json').read_text())

delivery={'installer_revision':'1.0.0-rc.6.2','version':manifest['version'],'status':'PRIVATE LAPTOP TEST CANDIDATE — NOT PUBLICLY CLEARED','installer':exe.name,'installer_sha256':sha(exe.read_bytes()),'installer_size':exe.stat().st_size,'patched_microsoft_files':[x['name'] for x in manifest['files'] if x['kind']=='delta'],'enhancement_helpers':[x['name'] for x in manifest['files'] if x['kind']=='new'],'dependency_hashes_checked':len(manifest['dependencies']),'all_runtime_payloads_embedded':True,'original_microsoft_installer_bundled':False,'full_microsoft_dlls_bundled':False,'private_backgrounds_bundled':False,'uploaded_or_published':False,'working_installation_modified':False,'remaining_release_gates':['Clean-runtime loading without the reference PCs global strong-name override','Laptop upgrade/clean-install/UAC/restart/rollback acceptance','Windows 10 and Windows 11 validation matrix','Derivative patch/artwork distribution rights review'],'isolated_tests_passed':testresults['tests']}

(R/'dist/DELIVERY.json').write_text(json.dumps(delivery,indent=2))

sums=''.join(f'{sha(p.read_bytes())}  {p.name}\n' for p in sorted((R/'dist').iterdir()) if p.is_file() and p.name!='SHA256SUMS.txt')

(R/'dist/SHA256SUMS.txt').write_text(sums)

def archive(path,files):

 with zipfile.ZipFile(path,'w',zipfile.ZIP_DEFLATED,compresslevel=9) as z:

  for source,name in files:

   info=zipfile.ZipInfo(name,(2026,10,19,0,0,0));info.compress_type=zipfile.ZIP_DEFLATED;z.writestr(info,source.read_bytes())

package=R/'Enhanced-Zune-1.0.0-rc.6.2-Laptop-Test-Package.zip'

archive(package,[(p,p.name) for p in sorted((R/'dist').iterdir()) if p.is_file()])

source_names=['Media-Tests.cs','Test-Media.ps1','Setup.cs','SetupUI.cs','MediaInstall.cs','Prerequisites.cs','WindowsComponents.cs','WindowsComponents.ps1','Completion-Tests.cs','Test-Completion.ps1','Test-WindowsComponents.ps1','Prerequisite-Tests.cs','Test-Prerequisites.ps1','Legacy-Tests.cs','Test-Legacy.ps1','RuntimeTrust.cs','Slots.cs','BackgroundResources.cs','installer.manifest','Build-Setup.ps1','Tests.cs','Preference-Tests.cs','Render-Setup.cs','Quality-Checks.ps1','Test-Setup.ps1','Build-Payload.py','Reconstruct.py','Inspect-Media.py','Finalize.py','Patch-Native-Default.py','Test-Native-Default.py','Test-Runtime-Defaults.cs']

sourcezip=R/'Enhanced-Zune-1.0.0-rc.6.2-Engineering-Source.zip'

archive(sourcezip,[(R/n,n) for n in source_names]+[(R/'build/payload.zip','build/payload.zip'),(R/'README.md','README.md'),(R/'REBUILD.md','REBUILD.md'),(R/'RUNTIME-COMPATIBILITY.md','RUNTIME-COMPATIBILITY.md'),(R/'LICENSING-AND-SOURCES.md','LICENSING-AND-SOURCES.md')])

(R/'PACKAGE-CHECKSUMS.txt').write_text(f'{sha(package.read_bytes())}  {package.name}\n{sha(sourcezip.read_bytes())}  {sourcezip.name}\n')

(R/'DELIVERY.json').write_text(json.dumps(delivery,indent=2))

print(json.dumps({'installer':str(exe),'installer_sha256':delivery['installer_sha256'],'installer_bytes':exe.stat().st_size,'laptop_package':str(package),'source_archive':str(sourcezip),'preservation':preservation},indent=2))



