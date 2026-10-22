using System;
using Microsoft.Iris;
using Microsoft.Win32;
using ZuneUI;
namespace ZuneCrossfade {
 public static class Settings {
  const string Key=@"Software\EnhancedZune\Playback";
  static bool enabled=Read("Enabled",0)==1;
  static int seconds=Read("Seconds",5);
  static Session current;
  static int Read(string name,int fallback){try{using(RegistryKey k=Registry.CurrentUser.OpenSubKey(Key)){return k==null?fallback:Convert.ToInt32(k.GetValue(name,fallback));}}catch{return fallback;}}
  public static bool Enabled{get{return enabled;}}
  public static int Seconds{get{return Math.Max(0,Math.Min(10,seconds));}}
  static Session Current{get{Management m=ZuneShell.DefaultInstance.Management;if(current==null||!Object.ReferenceEquals(current.Management,m))current=new Session(m);return current;}}
  public static bool SelectedEnabled{get{return Current.Enabled;}}
  public static int SelectedSeconds{get{return Current.Seconds;}}
  public static string Description(int value){return value+" second"+(value==1?"":"s")+" of overlap between local audio tracks.";}
  public static void Track(BooleanChoice toggle,Choice duration){Session s=Current;toggle.ChosenChanged+=delegate(object o,EventArgs e){s.Change(toggle.Value,duration.ChosenIndex);};duration.ChosenChanged+=delegate(object o,EventArgs e){s.Change(toggle.Value,duration.ChosenIndex);};}
  sealed class Session {
   public readonly Management Management;
   readonly ProxySettingDelegate action;
   bool? pendingEnabled;int? pendingSeconds;
   public Session(Management m){Management=m;action=new ProxySettingDelegate(Commit);}
   void Reset(){if(!Management.CommitList.ContainsKey(action)){pendingEnabled=null;pendingSeconds=null;}}
   public bool Enabled{get{Reset();return pendingEnabled.HasValue?pendingEnabled.Value:enabled;}}
   public int Seconds{get{Reset();return pendingSeconds.HasValue?pendingSeconds.Value:Settings.Seconds;}}
   public void Change(bool on,int duration){Reset();pendingEnabled=on;pendingSeconds=Math.Max(0,Math.Min(10,duration));Management.CommitList[action]=new object[]{on,pendingSeconds.Value};}
   void Commit(object value){object[] values=(object[])value;enabled=(bool)values[0];seconds=(int)values[1];using(RegistryKey k=Registry.CurrentUser.CreateSubKey(Key)){k.SetValue("Enabled",enabled?1:0,RegistryValueKind.DWord);k.SetValue("Seconds",seconds,RegistryValueKind.DWord);}pendingEnabled=null;pendingSeconds=null;Bridge.SettingsChanged();}
  }
 }
}
