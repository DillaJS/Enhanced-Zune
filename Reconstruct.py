from pathlib import Path
import json,hashlib,sys,shutil
ROOT=Path(__file__).resolve().parent
PROJECT=ROOT.parent.parent
sys.path.insert(0,str(PROJECT/'work/profiling/python-packages'))
import pefile
sys.path.insert(0,str(ROOT.parent/'Zune-Community-Package'))
from ReleasePolicy import verify_source
sha=lambda b:hashlib.sha256(b).hexdigest().upper()
install=Path('C:/Program Files/Zune')
stock=ROOT.parent/'Zune-Community-Package/installer-extracted'
policy=verify_source(install)
evidence=ROOT/'private-evidence';evidence.mkdir(exist_ok=True)
snapshot={str(p.relative_to(install)):sha(p.read_bytes()) for p in install.rglob('*') if p.is_file()}
(evidence/'working-installation-before.json').write_text(json.dumps(snapshot,indent=2))
mapping=json.loads((ROOT.parent/'Zune-Community-Package/installer-file-map.json').read_text())
comparison=[]
for row in mapping:
    a,b=stock/row['id'],install/row['name']
    if not a.exists() or not b.exists():continue
    x,y=a.read_bytes(),b.read_bytes()
    comparison.append(dict(name=row['name'],stock_id=row['id'],stock_sha256=sha(x),current_sha256=sha(y),stock_size=len(x),current_size=len(y),different=x!=y))
(evidence/'stock-comparison.json').write_text(json.dumps(comparison,indent=2))
changed=[x for x in comparison if x['different']]
known={r['name'].lower() for r in mapping}
extra=[dict(name=p.name,size=p.stat().st_size,sha256=sha(p.read_bytes())) for p in install.iterdir() if p.is_file() and p.name.lower() not in known]
(evidence/'extra-files.json').write_text(json.dumps(extra,indent=2))
for row in changed:
    shutil.copy2(install/row['name'],evidence/row['name'])
for name in ['ZuneScalingSettings.dll','ZuneBackgroundUpdater.exe','Zune.exe.config']:
    if (install/name).exists():shutil.copy2(install/name,evidence/name)
(evidence/'policy.json').write_text(json.dumps(policy,indent=2))
print(json.dumps({'changed':changed,'extra':extra,'policy':policy},indent=2))

local=Path.home()/'AppData/Local/Microsoft/Zune/LocalArtistPhotos/ZuneLocalArtistPhotos.dll'
assert local.is_file(), 'Current artist photo component missing'
shutil.copy2(local,evidence/local.name)
(evidence/'external-component-before.json').write_text(json.dumps({'path':str(local),'sha256':sha(local.read_bytes())},indent=2))
