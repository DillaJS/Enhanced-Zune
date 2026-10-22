using System;using System.IO;using System.Reflection;using System.Runtime.InteropServices;using System.Runtime.Serialization;using Microsoft.Win32;
class DefaultsTest{
 const BindingFlags F=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance;
 [DllImport("advapi32.dll",CharSet=CharSet.Unicode)]static extern int RegCreateKeyEx(IntPtr key,string sub,int reserved,string cls,int options,int sam,IntPtr security,out IntPtr result,out int disposition);
 [DllImport("advapi32.dll")]static extern int RegOverridePredefKey(IntPtr key,IntPtr replacement);
 [DllImport("advapi32.dll")]static extern int RegCloseKey(IntPtr key);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode)]static extern bool SetDllDirectory(string p);
 static void Check(bool ok,string text){if(!ok)throw new Exception(text);Console.WriteLine("PASS "+text);}
 static void NoOp(object ignored){}
 static int Main(string[] args){string name=@"Software\EnhancedZune\IsolatedTests\Runtime-"+Guid.NewGuid().ToString("N");IntPtr key;int disposition;IntPtr hkcu=new IntPtr(unchecked((int)0x80000001));if(RegCreateKeyEx(hkcu,name,0,null,0,0xf003f,IntPtr.Zero,out key,out disposition)!=0)return 2;
 try{
  Check(RegOverridePredefKey(hkcu,key)==0,"process-private registry enabled");string mode=args.Length>1?args[1]:"missing";string root=Path.GetFullPath(args[0]);
  using(RegistryKey k=Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Zune\Shell")){
   if(mode=="saved"){k.SetValue("AlbumArtworkQuality",1,RegistryValueKind.DWord);k.SetValue("InterfaceScalingChoice",3,RegistryValueKind.DWord);k.SetValue("BackgroundInterfaceTheme",1,RegistryValueKind.DWord);}
   if(mode=="invalid"){k.SetValue("AlbumArtworkQuality",99,RegistryValueKind.DWord);k.SetValue("InterfaceScalingChoice",99,RegistryValueKind.DWord);}
   if(mode=="legacy-dark"||mode=="legacy-white")k.SetValue("CollectionButtonTheme",mode=="legacy-dark"?0:1,RegistryValueKind.DWord);
  }
SetDllDirectory(@"C:\Program Files\Zune");AppDomain.CurrentDomain.AssemblyResolve+=delegate(object sender,ResolveEventArgs e){string f=new AssemblyName(e.Name).Name+".dll",p=Path.Combine(root,f);if(!File.Exists(p))p=Path.Combine(@"C:\Program Files\Zune",f);return File.Exists(p)?Assembly.LoadFrom(p):null;};
  Assembly.LoadFrom(Path.Combine(root,"UIX.dll")); Assembly.LoadFrom(Path.Combine(root,"ZuneShell.dll")); Assembly controls=Assembly.LoadFrom(Path.Combine(root,"UIXcontrols.dll")); controls.GetType("UIXControls.RegistryHelper",true).GetProperty("SettingsRegistryPath",F).SetValue(null,@"HKEY_CURRENT_USER\Software\Microsoft\Zune\Shell",null); Assembly helper=Assembly.LoadFrom(Path.Combine(root,"ZuneScalingSettings.dll"));string componentFolder=(string)helper.GetType("ZuneLocalTest.Bridge",true).GetMethod("ReleaseComponentDirectory",F).Invoke(null,null);Check(Path.GetFullPath(componentFolder)==root,"photo loader uses candidate installation directory");Assembly component=Assembly.LoadFrom(Path.Combine(componentFolder,"ZuneLocalArtistPhotos.dll"));Check(component.GetType("ZuneLocalTest.Photos",true).GetMethod("Watch",F)!=null,"packaged photo component loads without a per-user installation");Type art=helper.GetType("ZuneRestoration.ArtworkSettings",true),theme=helper.GetType("ZuneRestoration.InterfaceTheme",true),session=helper.GetType("ZuneRestoration.DisplaySession",true);object settings=FormatterServices.GetUninitializedObject(session);GC.SuppressFinalize(settings); FieldInfo mf=session.GetField("management",F),af=session.GetField("action",F);object management=FormatterServices.GetUninitializedObject(mf.FieldType);GC.SuppressFinalize(management);mf.SetValue(settings,management);af.SetValue(settings,Delegate.CreateDelegate(af.FieldType,typeof(DefaultsTest).GetMethod("NoOp",F)));
  MethodInfo read=art.GetMethod("ReadStartup",F),color=theme.GetMethod("ReadIndex",F);
  int artwork=mode=="saved"?1:0,scale=mode=="saved"?2:0,themeIndex=mode=="saved"||mode=="legacy-dark"?1:0;
  Check((int)read.Invoke(null,null)==artwork,"runtime artwork mode="+mode+" result="+artwork);
  Check((int)color.Invoke(null,null)==themeIndex,"runtime theme mode="+mode+" result="+themeIndex);
  Check((int)session.GetProperty("ScalingIndex",F).GetValue(settings,null)==scale,"display scaling mode="+mode+" result="+scale);
  Check((int)session.GetProperty("ArtworkIndex",F).GetValue(settings,null)==artwork,"display artwork mode="+mode+" result="+artwork);
  return 0;
 }catch(Exception e){Console.WriteLine(e);return 1;}finally{RegOverridePredefKey(hkcu,IntPtr.Zero);RegCloseKey(key);Registry.CurrentUser.DeleteSubKeyTree(name);}}
}




