from pathlib import Path
import json,ctypes
r=Path(__file__).resolve().parent
ns={'__file__':str(r.parent/'Zune-Community-Package/Read-Installer.py')}
exec(Path(ns['__file__']).read_text().split('files=[]')[0],ns)
rows,s=ns['rows'],ns['string']
tables=[s(x,1) for x in rows('SELECT Name FROM _Tables')]
result={'has_dialog_table':'Dialog' in tables,'has_control_table':'Control' in tables,'properties':{s(x,1):s(x,2) for x in rows('SELECT Property,Value FROM Property')}}
if 'LaunchCondition' in tables:result['launch_conditions']=[[s(x,1),s(x,2)] for x in rows('SELECT Condition,Description FROM LaunchCondition')]
if 'InstallUISequence' in tables:result['ui_sequence']=[[s(x,1),s(x,2)] for x in rows('SELECT Action,Condition FROM InstallUISequence')]
if 'Control' in tables:result['license_controls']=[s(x,1) for x in rows('SELECT Text FROM Control') if any(v in s(x,1).lower() for v in ['license','eula'])]
ns['msi'].MsiCloseHandle(ns['db'])
(r/'private-evidence/original-media-metadata.json').write_text(json.dumps(result,indent=2))
print(json.dumps(result,indent=2))
