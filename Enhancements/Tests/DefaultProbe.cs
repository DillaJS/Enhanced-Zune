using System;using System.IO;using System.Reflection;using System.Runtime.InteropServices;using Microsoft.Win32;
class DefaultProbe {
 [DllImport("advapi32.dll")]static extern int RegOverridePredefKey(IntPtr root,IntPtr replacement);
 [DllImport("advapi32.dll",CharSet=CharSet.Unicode)]static extern int RegOpenKeyEx(IntPtr root,string name,uint options,int access,out IntPtr key);
 [DllImport("advapi32.dll")]static extern int RegCloseKey(IntPtr key);
 static int Main(string[] args){string temporary=@"Software\EnhancedZune\ReleaseTests\"+Guid.NewGuid().ToString("N");RegistryKey key=null;IntPtr native=IntPtr.Zero;bool redirected=false;
  try{
   key=Registry.CurrentUser.CreateSubKey(temporary);
   IntPtr hkcu=new IntPtr(unchecked((int)0x80000001));if(RegOpenKeyEx(hkcu,temporary,0,0xf003f,out native)!=0||RegOverridePredefKey(hkcu,native)!=0)throw new Exception("Registry isolation failed");redirected=true;
   if(args[1]=="enabled")using(RegistryKey k=Registry.CurrentUser.CreateSubKey(@"Software\EnhancedZune\Playback")){k.SetValue("Enabled",1);k.SetValue("Seconds",10);}
   if(args[1]=="invalid")using(RegistryKey k=Registry.CurrentUser.CreateSubKey(@"Software\EnhancedZune\Playback")){k.SetValue("Enabled","invalid");k.SetValue("Seconds",99);}
   AppDomain.CurrentDomain.AssemblyResolve+=delegate(object s,ResolveEventArgs e){string p=Path.Combine(@"C:\Program Files\Zune",new AssemblyName(e.Name).Name+".dll");return File.Exists(p)?Assembly.LoadFrom(p):null;};
   Type settings=Assembly.LoadFrom(Path.GetFullPath(args[0])).GetType("ZuneCrossfade.Settings",true);
   bool enabled=(bool)settings.GetProperty("Enabled").GetValue(null,null);int seconds=(int)settings.GetProperty("Seconds").GetValue(null,null);
   if(enabled!=(args[1]=="enabled")||seconds!=(args[1]=="empty"?5:10))throw new Exception("Wrong defaults or preference bounds");
   Console.WriteLine("PASS real CLR2 settings: "+args[1]+", enabled="+enabled+", seconds="+seconds+"; private registry only.");return 0;
  }catch(Exception e){Console.WriteLine(e);return 1;}finally{if(redirected)RegOverridePredefKey(new IntPtr(unchecked((int)0x80000001)),IntPtr.Zero);if(native!=IntPtr.Zero)RegCloseKey(native);if(key!=null)key.Close();try{Registry.CurrentUser.DeleteSubKeyTree(temporary);}catch{}}
 }
}
