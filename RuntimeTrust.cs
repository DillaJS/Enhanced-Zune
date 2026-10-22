using System;using System.Collections.Generic;using Microsoft.Win32;
namespace EnhancedZune {
 public static class RuntimeTrust {
  public static string[] Overrides(){List<string> found=new List<string>();foreach(RegistryHive hive in new[]{RegistryHive.LocalMachine,RegistryHive.CurrentUser})foreach(RegistryView view in new[]{RegistryView.Registry64,RegistryView.Registry32}){try{using(RegistryKey root=RegistryKey.OpenBaseKey(hive,view))using(RegistryKey key=root.OpenSubKey(@"SOFTWARE\Microsoft\StrongName\Verification")){if(key!=null&&key.GetSubKeyNames().Length>0)found.Add(hive+" / "+view+": "+key.GetSubKeyNames().Length+" verification override(s)");}}catch(UnauthorizedAccessException){found.Add(hive+" / "+view+": could not inspect verification policy");}}return found.ToArray();}
 }
}
