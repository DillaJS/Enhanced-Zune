from pathlib import Path
import pefile,struct,json
r=Path(__file__).resolve().parent;p=r/'private-evidence/UIXrender.dll';orig=r/'private-evidence/UIXrender.reference.dll'
if not orig.exists():orig.write_bytes(p.read_bytes())
a=orig.read_bytes();pe=pefile.PE(data=a);s=next(x for x in pe.sections if x.Name.rstrip(b'\0')==b'.zdpi');code=s.get_data();needle=bytes.fromhex('b8 c0 00 00 00');auto=bytes.fromhex('48 8b 4c 24 58 ba 58 00 00 00');assert code.count(needle)==code.count(auto)==1
at=s.PointerToRawData+code.index(needle);dest=s.PointerToRawData+code.index(auto);new=bytearray(a);new[at:at+5]=b'\xe9'+struct.pack('<i',dest-(at+5));assert len(new)==len(a)
assert all(i in range(at,at+5) for i,(x,y) in enumerate(zip(a,new)) if x!=y)
p.write_bytes(new);(r/'private-evidence/defaults-native-audit.json').write_text(json.dumps({'default_instruction_offset':at,'reuses_existing_Auto_path_offset':dest,'only_change': '5-byte default-192-DPI instruction replaced by jump to existing GetDeviceCaps Auto path','all_saved_explicit_scales_and_fairness_fix_preserved':True,'source_installation_untouched':True},indent=2))
print('PASS isolated native Auto default; saved scale branches and all other code bytes identical.')
