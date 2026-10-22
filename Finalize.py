"""Verify private RC7 build evidence and create local delivery archives. No upload."""
from pathlib import Path
import json,hashlib,zipfile,shutil
R=Path(__file__).resolve().parent
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest().upper()
load=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
exe=R/'dist/Enhanced-Zune-Setup-1.0.0-rc.7.exe'
quality=load(R/'QUALITY-RESULTS.json');assert quality['status']=='passed'
assert quality['installer_sha256']==quality['different_build_directory_sha256']==sha(exe)
for name in ['LEGACY-RESULTS.json','COMPLETION-RESULTS.json','PREREQUISITE-RESULTS.json','WINDOWS-COMPONENT-RESULTS.json','SELF-CONTAINED-HOST-RESULTS.json']:assert load(R/name)['status']=='passed',name
transaction=load(Path((R/'latest-test-path.txt').read_text(encoding='utf-8-sig').strip())/'results.json');assert transaction['status']=='passed'
assert 'ALL 31 BRIDGE CHECKS PASSED' in (R/'tests/bridge/results.txt').read_text(encoding='utf-8-sig')
assert 'ALL 20 repeat-one regression checks passed' in (R/'tests/repeat/results.txt').read_text(encoding='utf-8-sig')
audio=load(R/'tests/audio-output/Mixer-Test-Results.json');assert audio['passed']==22
output=load(R/'tests/audio-output/Integration-Mixer-Results.json');assert output['passed']==19
trace=(R/'tests/self-contained-host/host-trace.txt').read_text(encoding='utf-8-sig');assert 'Executing as a self-contained app' in trace
before=load(R/'private-evidence/working-installation-before.json');installed=Path('C:/Program Files/Zune')
assert before=={str(p.relative_to(installed)):sha(p) for p in installed.rglob('*') if p.is_file()},'Installed Zune changed during packaging'
assert sha(R.parent/'Zune-Public-Release-RC6-2/dist/Enhanced-Zune-Setup-1.0.0-rc.6.2.exe')=='910D70A40DFC17190FCE2FF3531B10BC2BF249167CA5C3D9BC7ED63797AFD67F'
manifest=load(R/'build/payload/manifest.json');assert len(manifest['files'])==12 and manifest['version']=='1.0.0-rc.7'
assert (R/'build/payload.zip').read_bytes() in exe.read_bytes()
verification={'version':'1.0.0-rc.7','installer_sha256':sha(exe),'installer_size':exe.stat().st_size,'status':'passed recorded packaging checks','installer_rebuild_byte_identical':True,'isolated_transaction_scenarios':transaction['tests'],'legacy_recovery':'RC1 through RC6 passed','repeat_one_checks':20,'bridge_checks':31,'mixer_checks':22,'queue_and_pause_output_checks':19,'crossfade_defaults':'off; 5 second initial duration; 0–10 second range','real_clr2_defaults_and_preference_bounds_checked':True,'self_contained_runtime':'8.0.31; WAV/MP3/WMA decoder startup and parent-exit cleanup checked with global .NET environment roots hidden','working_installation_files_preserved':len(before),'old_rc62_installer_preserved':True,'new_runtime_payloads':['ZuneCrossfadeBridge.dll','ZuneCrossfadeHost.exe','ZuneCrossfade-Licenses.txt'],'prior_runtime_changes_scoped_to':['ZuneShell.dll','ZuneScalingSettings.dll','ZuneShellResources.dll / MANAGEMENTGENERAL.UIX'],'all_other_prior_public_runtime_payloads_preserved':True,'private_backgrounds_or_photos_bundled':False,'uploaded_or_published':False,'actual_new_installer_run_on_working_zune':False,'listening':'User confirmed the separate test version; no agent playback/visual test or fresh clean-machine acceptance performed.','os_servicing':'Decision and mocked-script checks passed; this build did not install Windows components or reboot.'}
(R/'VERIFICATION.json').write_text(json.dumps(verification,indent=2,ensure_ascii=False),encoding='utf-8')
docs=['README.md','CHANGELOG.md','FEATURES.md','REBUILD.md','KNOWN-LIMITATIONS.md','LICENSING-AND-SOURCES.md','VERIFICATION.json','ZuneCrossfade-Licenses.txt']
for name in docs:shutil.copy2(R/name,R/'dist'/name)
shutil.copy2(R/'build/payload/manifest.json',R/'dist/release-manifest.json')
source_names=['Setup.cs','SetupUI.cs','MediaInstall.cs','Prerequisites.cs','WindowsComponents.cs','WindowsComponents.ps1','RuntimeTrust.cs','Slots.cs','BackgroundResources.cs','installer.manifest','Build-Setup.ps1','Build-Payload.py','Build-RC7-Payload.py','PayloadCodec.py','Reconstruct.py','Finalize.py','Tests.cs','Preference-Tests.cs','Render-Setup.cs','Quality-Checks.ps1','Test-Setup.ps1','Legacy-Tests.cs','Test-Legacy.ps1','Completion-Tests.cs','Test-Completion.ps1','Prerequisite-Tests.cs','Test-Prerequisites.ps1','Test-WindowsComponents.ps1','Media-Tests.cs','Test-Media.ps1']
files=[(R/n,n) for n in source_names+docs]+[(R/'build/payload.zip','build/payload.zip')]
for p in sorted((R/'Enhancements').rglob('*')):
 if not p.is_file() or any(part in ['bin','obj'] for part in p.relative_to(R/'Enhancements').parts):continue
 if p.suffix.lower() not in ['.cs','.csproj','.json','.uix','.uib','.mp3','.wma','.md','.ps1']:continue
 files.append((p,p.relative_to(R).as_posix()))
assert all(not n.endswith(('.snk','.dll','.exe')) for p,n in files)
source=R/'Enhanced-Zune-1.0.0-rc.7-Engineering-Source.zip'
with zipfile.ZipFile(source,'w',zipfile.ZIP_DEFLATED,compresslevel=6) as z:
 for p,n in files:
  info=zipfile.ZipInfo(n,(2026,10,20,0,0,0));info.compress_type=zipfile.ZIP_DEFLATED;z.writestr(info,p.read_bytes())
with zipfile.ZipFile(source) as z:
 assert z.testzip() is None
 assert z.read('build/payload.zip')==(R/'build/payload.zip').read_bytes()
 assert all(z.read(n)==p.read_bytes() for p,n in files)
for p in [exe,source]:assert p.stat().st_size>0
(R/'dist/SHA256SUMS.txt').write_text(''.join(sha(p)+'  '+p.name+'\n' for p in [exe,source]),encoding='utf-8')
(R/'DELIVERY.json').write_text(json.dumps({'installer':str(exe),'source':str(source),'installer_sha256':sha(exe),'source_sha256':sha(source),'both_archives_created_locally':True,'uploaded':False},indent=2),encoding='utf-8')
print(json.dumps({'installer':str(exe),'installer_bytes':exe.stat().st_size,'source':str(source),'source_bytes':source.stat().st_size,'source_files':len(files),'installed_files_preserved':len(before)},indent=2))
