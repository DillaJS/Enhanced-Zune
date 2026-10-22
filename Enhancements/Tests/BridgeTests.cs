using System;using System.IO;using System.Reflection;using System.Diagnostics;using System.Threading;using System.Globalization;
namespace ZuneCrossfade {public static class Settings {public static bool Enabled;public static int Seconds=5;}}
namespace ZuneRestoration {public static class RepeatFeature {public static bool One;public static bool IsOne(){return One;}}}
public sealed class ChoiceValue {public object ValueData;public object Value {get{return ValueData;}}public ChoiceValue(object v){ValueData=v;}}
public sealed class Track {public bool IsMusic=true,IsStreaming;public int PlaybackID=1;}
public sealed class Player {
 public int Volume=63;public bool Mute;public long Position=200000000,Duration=600000000;public int CurrentUriID=1;public string CurrentUri;public int TransportState=2;public int SetCount,NextCount,SeekCount;
 public void SetUri(string u,long s,int id){CurrentUri=u;Position=s;CurrentUriID=id;SetCount++;}
 public void SetNextUri(string u,long s,int id){NextCount++;} public void CancelNext(){}
 public void Play(){TransportState=2;}public void Pause(){TransportState=3;}public void Stop(){TransportState=1;}public void Close(){TransportState=1;}
 public void SeekToAbsolutePosition(long x){Position=x;SeekCount++;}public void SeekToRelativePosition(long x){Position+=x;SeekCount++;}
 public void set_Volume(int x){Volume=x;}public void set_Mute(bool x){Mute=x;}
}
public sealed class Transport {public Player _playbackWrapper;public bool Playing=true;public ChoiceValue _volume=new ChoiceValue(63);public ChoiceValue _muted=new ChoiceValue(false);public Track CurrentTrack=new Track();public ChoiceValue FastForwarding=new ChoiceValue(false),Rewinding=new ChoiceValue(false);}
class BridgeTests {
 static int checks;
 static BindingFlags f=BindingFlags.NonPublic|BindingFlags.Static;
 static void Check(bool c,string label){if(!c)throw new Exception(label);checks++;Console.WriteLine("PASS "+label);}
 static object Field(string n){return typeof(ZuneCrossfade.Bridge).GetField(n,f).GetValue(null);}
 static void Send(string s){typeof(ZuneCrossfade.Bridge).GetMethod("Write",f).Invoke(null,new object[]{s});Thread.Sleep(120);}
 static void Active(Transport t){for(int i=0;i<30&&!ZuneCrossfade.Bridge.SuppressNativeVolume(t);i++){ZuneCrossfade.Bridge.Tick(t);Thread.Sleep(50);}Check(ZuneCrossfade.Bridge.SuppressNativeVolume(t),"host ready activates custom playback");}
 static int FakeHost(){
  object gate=new object();int id=-1;double position=0;bool quit=false;
  Thread reporter=new Thread(delegate(){while(!quit){lock(gate){Console.WriteLine("POS|"+id+"|"+position.ToString(CultureInfo.InvariantCulture)+"|0|0");Console.Out.Flush();}Thread.Sleep(30);}});reporter.IsBackground=true;reporter.Start();
  string line;while((line=Console.ReadLine())!=null){string[] a=line.Split('|');lock(gate){
   File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"commands.log"),line+Environment.NewLine);
   if(a[0]=="LOAD"){id=Int32.Parse(a[1]);Console.WriteLine("READY|"+id);Console.Out.Flush();}
   if(a[0]=="GO"||a[0]=="SEEK"||a[0]=="TEST"){id=Int32.Parse(a[1]);position=Double.Parse(a[2],CultureInfo.InvariantCulture);}
   if(a[0]=="QUIT"){quit=true;break;}
  }}return 0;
 }
 static int Main(string[] args){if(args.Length>0&&args[0]=="--parent")return FakeHost();try{
  string path=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"read-only-test.mp3");if(File.Exists(path))File.SetAttributes(path,FileAttributes.Normal);File.WriteAllBytes(path,new byte[16]);File.SetAttributes(path,FileAttributes.ReadOnly);
  string commandLog=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"commands.log");File.WriteAllText(commandLog,"");
  var p=new Player();p.CurrentUri=path;var t=new Transport();t._playbackWrapper=p;
  ZuneCrossfade.Bridge.Tick(t);Check(Field("host")==null&&p.Volume==63,"disabled mode launches no host and preserves native volume");
  ZuneCrossfade.Bridge.SetUri(p,path,200000000,1);Check(p.SetCount==1&&p.Position==200000000,"disabled selection uses native transport");
  ZuneCrossfade.Settings.Enabled=true;ZuneCrossfade.Bridge.Tick(t);Check(p.Volume==63,"native audio stays on during decoder startup");Active(t);
  Check(p.Volume==0,"native audio silenced only after readiness");
  ZuneCrossfade.Bridge.SetNextUri(p,path,0,2);ZuneCrossfade.Bridge.Tick(t);Check((int)Field("queuedId")==2&&p.NextCount==1,"native queue remains intact and next track is prefetched");
  Send("TEST|2|5.25");p.CurrentUriID=2;t.CurrentTrack.PlaybackID=2;p.Position=0;ZuneCrossfade.Bridge.Tick(t);
  Check(p.Position>50000000&&p.Position<53000000&&(int)Field("alignedId")==2,"natural transition preserves incoming intro offset");
  ZuneCrossfade.Bridge.Seek(p,120000000);Check(p.Position==120000000,"explicit seeking still updates native transport");
  ZuneCrossfade.Bridge.Pause(p);Check(p.TransportState==3,"pause synchronized");
  t.Playing=false;p.TransportState=2;ZuneCrossfade.Bridge.Tick(t);Check(((string)Field("lastState")).StartsWith("STATE|1|"),"delayed native playing status cannot undo the UI pause");
  t.Playing=true;ZuneCrossfade.Bridge.Play(p);Check(p.TransportState==2,"resume synchronized");
  t._volume.ValueData=41;ZuneCrossfade.Bridge.SetVolume(p,41);Check(p.Volume==0,"volume changes keep native graph silent");
  t._muted.ValueData=true;ZuneCrossfade.Bridge.SetMute(p,true);Check(p.Mute,"mute synchronized");t._muted.ValueData=false;ZuneCrossfade.Bridge.SetMute(p,false);
  ZuneRestoration.RepeatFeature.One=true;ZuneCrossfade.Bridge.Tick(t);Check((int)Field("queuedId")==-1,"repeat one cancels overlap queue");
  Send("TEST|-1|0");p.Position=0;int nativeSeeks=p.SeekCount;ZuneCrossfade.Bridge.Tick(t);Thread.Sleep(100);
  Check((int)Field("hostId")==2&&p.SeekCount==nativeSeeks,"native same-ID repeat restarts ended host without seeking the native player");
  Send("TEST|-1|0");t.Playing=false;p.Position=0;ZuneCrossfade.Bridge.Tick(t);Thread.Sleep(100);
  Check((int)Field("hostId")==-1,"paused native repeat cannot restart ended audio");t.Playing=true;
  ZuneRestoration.RepeatFeature.One=false;
  ZuneCrossfade.Settings.Enabled=false;ZuneCrossfade.Bridge.Tick(t);Check(p.Volume==41&&Field("host")==null&&!ZuneCrossfade.Bridge.SuppressNativeVolume(t),"turning off restores user volume and closes host");
  ZuneCrossfade.Settings.Enabled=true;Active(t);((Process)Field("host")).Kill();Thread.Sleep(100);ZuneCrossfade.Bridge.Tick(t);Check(p.Volume==41&&Field("host")==null,"host crash returns to native audio");
  ZuneCrossfade.Bridge.Tick(t);Check(Field("host")==null,"failed track does not cause a restart loop");
  ZuneCrossfade.Bridge.SetUri(p,path,0,3);t.CurrentTrack.PlaybackID=3;Active(t);
  t.CurrentTrack.IsMusic=false;ZuneCrossfade.Bridge.Tick(t);Check(p.Volume==41&&Field("host")==null,"non-music content falls back to native playback");
  t.CurrentTrack.IsMusic=true;ZuneCrossfade.Bridge.Stop(p);ZuneCrossfade.Bridge.Tick(t);Check(Field("host")==null&&p.TransportState==1,"stopping cannot relaunch the audio host");
  Check(new FileInfo(path).Length==16&&(File.GetAttributes(path)&FileAttributes.ReadOnly)!=0,"source file remains unchanged and read-only");
  string commands=File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"commands.log"));
  foreach(string s in new string[]{"GO|1|20|0|0.63","SEEK|2|12","PAUSE|1","PAUSE|0","STATE|0|0.41|1|5","STATE|0|0|1|5","QUEUE|2|-1"})Check(commands.Contains(s),"host received "+s);
  Console.WriteLine("ALL "+checks+" BRIDGE CHECKS PASSED (fake host, no audio, no Zune UI or settings writes).");return 0;
 }catch(Exception e){Console.WriteLine(e);return 1;}finally{ZuneCrossfade.Settings.Enabled=false;try{typeof(ZuneCrossfade.Bridge).GetMethod("Restore",f).Invoke(null,null);}catch{}}}
}
