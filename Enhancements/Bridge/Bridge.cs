using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading;

namespace ZuneCrossfade {
 // CLR 2 bridge. Only deferred Zune callbacks touch its UI/native transport.
 public static class Bridge {
  static readonly BindingFlags F=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
  static readonly object statusGate=new object();
  static readonly CultureInfo C=CultureInfo.InvariantCulture;
  static object transport,player;
  static Process host;
  static StreamWriter input;
  static bool active,loading,busy,failed,shutdown;
  static int loadedId=-1,alignedId=-1,readyId=-1,hostId=-1,failedId=-1,nextId=-1,queuedId=-2;
  static string nextUri;
  static double hostPosition;
  static int clockId=-1;
  static double clockPosition;
  static DateTime mismatchSince;
  static DateTime heartbeat,started;
  static bool hostError;
  static string lastState;
  static readonly string log=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),@"EnhancedZune\Crossfade\bridge.log");
  static Bridge(){ AppDomain.CurrentDomain.ProcessExit+=delegate{shutdown=true;StopHost();}; }
  static object Read(object x,string name){if(x==null)return null;PropertyInfo p=x.GetType().GetProperty(name,F);if(p!=null)return p.GetValue(x,null);FieldInfo f=x.GetType().GetField(name,F);return f==null?null:f.GetValue(x);}
  static object Invoke(object p,string method,params object[] args){return p.GetType().GetMethod(method,F).Invoke(p,args);}
  static string N(double x){return x.ToString("R",C);}
  static string Encode(string x){return Convert.ToBase64String(Encoding.UTF8.GetBytes(x));}
  static void Log(string message){try{Directory.CreateDirectory(Path.GetDirectoryName(log));File.AppendAllText(log,DateTime.Now.ToString("s")+" "+message+Environment.NewLine);}catch{}}
  static bool One(){try {foreach(Assembly a in AppDomain.CurrentDomain.GetAssemblies()){Type t=a.GetType("ZuneRestoration.RepeatFeature");if(t!=null)return (bool)t.GetMethod("IsOne").Invoke(null,null);}}catch{}return false;}
  static string LocalPath(string value){
   if(String.IsNullOrEmpty(value))return null;
   Uri uri;string path=value;
   if(Uri.TryCreate(value,UriKind.Absolute,out uri)){if(!uri.IsFile||uri.IsUnc)return null;path=uri.LocalPath;}
   if(!Path.IsPathRooted(path)||!File.Exists(path))return null;
   string ext=Path.GetExtension(path).ToLowerInvariant();
   return ext==".mp3"||ext==".wma"||ext==".wav"||ext==".m4a"||ext==".aac"?path:null;
  }
  static int DesiredVolume(){return Math.Max(0,Math.Min(100,Convert.ToInt32(Read(Read(transport,"_volume"),"Value"))));}
  static bool Muted(){return Convert.ToBoolean(Read(Read(transport,"_muted"),"Value"));}
  static bool Paused(){object playing=Read(transport,"Playing");return playing is bool?!(bool)playing:Convert.ToInt32(Read(player,"TransportState"))!=2;}
  static void Write(string s){if(input==null)throw new IOException("Crossfade host is unavailable.");input.WriteLine(s);input.Flush();}
  static void Reader(object state){
   Process p=(Process)state;
   try{string line;while((line=p.StandardOutput.ReadLine())!=null){
    string[] a=line.Split('|');lock(statusGate){
     if(!Object.ReferenceEquals(host,p))return;
     heartbeat=DateTime.UtcNow;
     if(a[0]=="READY")readyId=Int32.Parse(a[1],C);
     else if(a[0]=="POS"){hostId=Int32.Parse(a[1],C);hostPosition=Double.Parse(a[2],C);}
     else if(a[0]=="ERROR"){hostError=true;Log("Host error: "+Encoding.UTF8.GetString(Convert.FromBase64String(a[1])));}
    }
   }}catch(Exception ex){Log("Host reader: "+ex.Message);}finally{lock(statusGate){if(Object.ReferenceEquals(host,p))hostError=true;}}
  }
  static void StopHost(){
   Process p=host;host=null;StreamWriter writer=input;input=null;
   try{if(writer!=null){writer.WriteLine("QUIT");writer.Flush();writer.Close();}}catch{}
   if(p!=null){try{if(!p.HasExited&&!p.WaitForExit(150))p.Kill();}catch{}try{p.Dispose();}catch{}}
   lock(statusGate){readyId=hostId=-1;hostError=false;}
   loading=false;loadedId=alignedId=clockId=-1;queuedId=-2;lastState=null;mismatchSince=DateTime.MinValue;
  }
  static void Restore(){
   // Restore the UI's selected volume, never the native value (which was silenced).
   try{if(player!=null&&active){Invoke(player,"set_Volume",DesiredVolume());Invoke(player,"set_Mute",Muted());}}catch(Exception ex){Log("Volume restore: "+ex.Message);}
   active=false;StopHost();
  }
  static void Failure(Exception ex){Log("Native fallback: "+ex.GetBaseException().Message);failedId=loadedId;failed=true;Restore();}
  public static bool SuppressNativeVolume(object t){return active;}
  public static void Register(object t){transport=t;player=Read(t,"_playbackWrapper");}
  static void Start(int id,string path){
   StopHost();loadedId=id;loading=true;started=DateTime.UtcNow;
   string exe=Path.Combine(Path.GetDirectoryName(typeof(Bridge).Assembly.Location),"ZuneCrossfadeHost.exe");
   ProcessStartInfo info=new ProcessStartInfo(exe,"--parent "+Process.GetCurrentProcess().Id);
   info.UseShellExecute=false;info.CreateNoWindow=true;info.RedirectStandardInput=true;info.RedirectStandardOutput=true;
   host=Process.Start(info);input=host.StandardInput;heartbeat=DateTime.UtcNow;
   Thread thread=new Thread(Reader);thread.IsBackground=true;thread.Start(host);
   Write("LOAD|"+id+"|"+Encode(path)+"|"+Settings.Seconds);
   Log("Loading local track ID "+id);
  }
  static void Queue(int currentId){
   int id=One()||nextId==currentId?-1:nextId;string path=id<0?null:LocalPath(nextUri);
   if(path==null)id=-1;
   if(id==queuedId)return;
   Write("QUEUE|"+currentId+"|"+id+(id<0?"":"|"+Encode(path)));queuedId=id;
  }
  public static void Tick(object t){
   if(busy||shutdown)return;busy=true;
   try{
    Register(t);
    if(!Settings.Enabled){if(host!=null||active)Restore();failed=false;failedId=-1;return;}
    if(player==null)return;
    int id=Convert.ToInt32(Read(player,"CurrentUriID"));
    object track=Read(t,"CurrentTrack");
    string path=LocalPath(Read(player,"CurrentUri") as string);
    bool music=track!=null&&Convert.ToBoolean(Read(track,"IsMusic"))&&!Convert.ToBoolean(Read(track,"IsStreaming"));
    if(Convert.ToBoolean(Read(Read(t,"_fastforwarding"),"Value"))||Convert.ToBoolean(Read(Read(t,"Rewinding"),"Value")))music=false;
    if(!music||path==null){if(host!=null||active)Restore();return;}
    // URI callbacks can precede Zune's playlist update. Wait for agreement.
    if(Convert.ToInt32(Read(track,"PlaybackID"))!=id)return;
    if(failed&&id==failedId)return;
    if(failed){failed=false;failedId=-1;}
    if(host==null){int playState=Convert.ToInt32(Read(player,"TransportState"));if(playState!=2&&playState!=3)return;Start(id,path);return;}
    int ready,current;double position;bool error;
    lock(statusGate){ready=readyId;current=hostId;position=hostPosition;error=hostError;}
    if(error||host.HasExited||(DateTime.UtcNow-heartbeat).TotalSeconds>3)throw new IOException("The audio host stopped responding.");
    if(loading){
     if(id!=loadedId){Restore();Start(id,path);return;}
     if(ready!=id){if((DateTime.UtcNow-started).TotalSeconds>5)throw new IOException("Decoder startup timed out.");return;}
     // The host is decoded and ready before native audio is silenced.
     active=true;Invoke(player,"set_Volume",0);
     Write("GO|"+id+"|"+N(Convert.ToInt64(Read(player,"Position"))/10000000d)+"|"+(Paused()?"1":"0")+"|"+N(Muted()?0:DesiredVolume()/100d));
     clockId=id;clockPosition=Convert.ToInt64(Read(player,"Position"))/10000000d;
     loading=false;alignedId=id;queuedId=-2;Queue(id);Log("Crossfading active, track ID "+id);
    } else if(id!=alignedId){
     if(current==id){
      // Continue the already audible incoming track instead of replaying its intro.
      Invoke(player,"SeekToAbsolutePosition",(long)(Math.Max(0,position-.08)*10000000));
      loadedId=alignedId=id;queuedId=-2;mismatchSince=DateTime.MinValue;Queue(id);
     }else {
      // The status reporter and device buffer can trail the native end event.
      if(mismatchSince==DateTime.MinValue)mismatchSince=DateTime.UtcNow;
      if((DateTime.UtcNow-mismatchSince).TotalMilliseconds<350)return;
      Restore();Start(id,path);return;
     }
    } else {
     Queue(id);
     // The repaired native repeat-one path can restart the same URI directly,
     // without a new playback ID or a shell Seek call. Restart the audio host
     // after its EOF when the native clock has begun that next repeat.
     double nativePosition=Convert.ToInt64(Read(player,"Position"))/10000000d;
     bool restarted=clockId==id&&nativePosition+.25<clockPosition;
     clockId=id;clockPosition=nativePosition;
     if(One()&&restarted&&!Paused())
      Write("SEEK|"+id+"|"+N(nativePosition));
    }
    string state="STATE|"+(Paused()?"1":"0")+"|"+N(Muted()?0:DesiredVolume()/100d)+"|"+(One()?"0":"1")+"|"+Settings.Seconds;
    if(state!=lastState){Write(state);lastState=state;}
   }catch(Exception ex){Failure(ex);}finally{busy=false;}
  }
  public static void SettingsChanged(){failed=false;failedId=-1;if(transport!=null)Tick(transport);}
  public static void SetUri(object p,string uri,long start,int id){
   // Explicit selections belong to Zune. Never carry an overlap across a manual skip.
   try{if(host!=null||active)Restore();nextId=-1;nextUri=null;failed=false;failedId=-1;}catch{}
   Invoke(p,"SetUri",uri,start,id);
  }
  public static void SetNextUri(object p,string uri,long start,int id){Invoke(p,"SetNextUri",uri,start,id);nextId=id;nextUri=uri;}
  public static void CancelNext(object p){Invoke(p,"CancelNext");nextId=-1;nextUri=null;}
  public static void SetVolume(object p,int value){Invoke(p,"set_Volume",active?0:value);if(active)try{Write("STATE|"+(Paused()?"1":"0")+"|"+N(Muted()?0:value/100d)+"|"+(One()?"0":"1")+"|"+Settings.Seconds);lastState=null;}catch(Exception ex){Failure(ex);}}
  public static void SetMute(object p,bool value){Invoke(p,"set_Mute",value);if(active)try{Write("STATE|"+(Paused()?"1":"0")+"|"+N(value?0:DesiredVolume()/100d)+"|"+(One()?"0":"1")+"|"+Settings.Seconds);lastState=null;}catch(Exception ex){Failure(ex);}}
  public static void Seek(object p,long value){Invoke(p,"SeekToAbsolutePosition",value);if(active)try{clockId=Convert.ToInt32(Read(p,"CurrentUriID"));clockPosition=value/10000000d;Write("SEEK|"+clockId+"|"+N(clockPosition));}catch(Exception ex){Failure(ex);}}
  public static void SeekRelative(object p,long value){long position=Convert.ToInt64(Read(p,"Position"));Invoke(p,"SeekToRelativePosition",value);if(active)try{clockId=Convert.ToInt32(Read(p,"CurrentUriID"));clockPosition=Math.Max(0,position+value)/10000000d;Write("SEEK|"+clockId+"|"+N(clockPosition));}catch(Exception ex){Failure(ex);}}
  public static void Play(object p){Invoke(p,"Play");if(active)try{Write("PAUSE|0");}catch(Exception ex){Failure(ex);}}
  public static void Pause(object p){Invoke(p,"Pause");if(active)try{Write("PAUSE|1");}catch(Exception ex){Failure(ex);}}
  public static void Stop(object p){try{Restore();}catch{}Invoke(p,"Stop");}
  public static void Close(object p){try{Restore();}catch{}Invoke(p,"Close");}
 }
}
