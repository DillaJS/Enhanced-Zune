import ctypes,winreg,uuid,json
from pathlib import Path
import pefile
r=Path(__file__).resolve().parent
adv=ctypes.WinDLL('advapi32',use_last_error=True);adv.RegOverridePredefKey.argtypes=[ctypes.c_void_p,ctypes.c_void_p];adv.RegOverridePredefKey.restype=ctypes.c_long
rootname='Software\\EnhancedZune\\IsolatedTests\\NativeAuto-'+uuid.uuid4().hex
root=winreg.CreateKeyEx(winreg.HKEY_CURRENT_USER,rootname,0,winreg.KEY_ALL_ACCESS)
k=ctypes.WinDLL('kernel32',use_last_error=True);k.SetDllDirectoryW('C:/Program Files/Zune')
u=ctypes.WinDLL('user32',use_last_error=True);u.GetDC.argtypes=[ctypes.c_void_p];u.GetDC.restype=ctypes.c_void_p;u.ReleaseDC.argtypes=[ctypes.c_void_p,ctypes.c_void_p]
g=ctypes.WinDLL('gdi32');g.GetDeviceCaps.argtypes=[ctypes.c_void_p,ctypes.c_int]
lib=ctypes.WinDLL(str(r/'private-evidence/UIXrender.dll'));pe=pefile.PE(str(r/'private-evidence/UIXrender.dll'));s=next(s for s in pe.sections if s.Name.rstrip(b'\0')==b'.zdpi')
func=ctypes.WINFUNCTYPE(ctypes.c_int,ctypes.c_void_p)(lib._handle+s.VirtualAddress)
hdc=u.GetDC(None);assert hdc;expected=g.GetDeviceCaps(hdc,88);results=[]
try:
 assert adv.RegOverridePredefKey(ctypes.c_void_p(-2147483647),ctypes.c_void_p(int(root)))==0
 key=winreg.CreateKeyEx(winreg.HKEY_CURRENT_USER,'Software\\Microsoft\\Zune\\Shell',0,winreg.KEY_ALL_ACCESS)
 for val,wanted in [(None,expected),(0,expected),(1,96),(5,192),(13,384),(99,expected)]:
  if val is None:
   try:winreg.DeleteValue(key,'InterfaceScalingChoice')
   except FileNotFoundError:pass
  else:winreg.SetValueEx(key,'InterfaceScalingChoice',0,winreg.REG_DWORD,val)
  actual=func(hdc);assert actual==wanted,(val,actual,wanted);results.append({'saved_value':val,'actual_dpi':actual,'expected':wanted})
 key.Close()
finally:
 adv.RegOverridePredefKey(ctypes.c_void_p(-2147483647),None);root.Close();u.ReleaseDC(None,hdc)
 def remove(path):
  with winreg.OpenKey(winreg.HKEY_CURRENT_USER,path,0,winreg.KEY_ALL_ACCESS) as key:
   children=[]
   while True:
    try:children.append(winreg.EnumKey(key,len(children)))
    except OSError:break
  for child in children:remove(path+'\\'+child)
  winreg.DeleteKey(winreg.HKEY_CURRENT_USER,path)
 remove(rootname)
(r/'private-evidence/native-default-tests.json').write_text(json.dumps({'process_private_registry':True,'working_Zune_untouched':True,'checks':results},indent=2));print('PASS native DPI default/Auto/explicit/invalid paths executed against candidate in an isolated process.')
