using NAudio.Wave;
using System.Text.Json;
namespace CrossfadePrototype;
sealed class FakeOutput : IWavePlayer {
 public PlaybackState PlaybackState {get;private set;}=PlaybackState.Stopped;
 public float Volume {get;set;}=1;
 public WaveFormat OutputWaveFormat=>WaveFormat.CreateIeeeFloatWaveFormat(Mixer.Rate,2);
 public event EventHandler<StoppedEventArgs>? PlaybackStopped {add{} remove{}}
 public int Plays,Pauses;
 public void Init(IWaveProvider wave){}
 public void Play(){PlaybackState=PlaybackState.Playing;Plays++;}
 public void Pause(){PlaybackState=PlaybackState.Paused;Pauses++;}
 public void Stop(){PlaybackState=PlaybackState.Stopped;}
 public void Dispose(){}
}
static class IntegrationTests {
 static readonly List<string> passed=new();
 static void Check(bool value,string label){if(!value)throw new Exception(label);passed.Add(label);Console.WriteLine("PASS "+label);}
 static void Read(Mixer m,double seconds){var b=new float[(int)Math.Round(seconds*Mixer.Rate)*2];m.Read(b,0,b.Length);}
 static int Main(string[] args){
  string report=Path.GetFullPath(args[0]);Directory.CreateDirectory(report);
  try{
   Tests.Run(report);
   string source=Path.Combine(report,"Crossfade-off.wav"); // Generated tones only, never user music.
   using(var m=new Mixer(new ITrack[]{new IdentifiedTrack(10,source)})) {
    var device=new FakeOutput();var playback=new PlaybackOutput(device,m);
    playback.SetPaused(true);Check(device.Plays==0,"starting paused never starts the output device");
    playback.SetPaused(false);Read(m,.1);playback.SetPaused(true);
    Check(device.PlaybackState==PlaybackState.Paused&&device.Pauses==1,"pause freezes the actual output queue");
    var position=m.Snapshot().Position;Read(m,1);Check(m.Snapshot().Position==position,"paused output cannot consume or repeat track samples");
    playback.SetPaused(true);Check(device.Pauses==1,"repeated pause state does not churn the device");
    playback.Seek(10,3);Check(device.PlaybackState==PlaybackState.Stopped&&device.Plays==1&&m.Snapshot().Position==3,"seeking while paused does not restart audio");
    playback.SetPaused(false);Read(m,.1);Check(device.PlaybackState==PlaybackState.Playing&&m.Snapshot().Position>3,"resume continues from the paused seek position");
    playback.SetPaused(false);Check(device.Plays==2,"repeated playing state does not restart the output queue");
    m.QueueAfter(10,new IdentifiedTrack(20,source));m.Configure(true,2);playback.Seek(10,9);Read(m,.1);Check(m.Snapshot().Fading,"regression fixture is inside an overlap");
    playback.SetPaused(true);position=m.Snapshot().Position;Read(m,1);Check(m.Snapshot().Fading&&m.Snapshot().Position==position,"pausing during overlap freezes both tracks");
    playback.SetPaused(false);Read(m,.1);Check(m.Snapshot().Position>position,"overlap resumes without replaying its start");
   }
   using(var m=new Mixer(new ITrack[]{new IdentifiedTrack(10,source)})){
    m.Configure(true,2);m.SetVolume(1);m.QueueAfter(10,new IdentifiedTrack(20,source));
    m.SeekId(10,9);Read(m,.1);Check(m.Snapshot().Fading,"late seek starts a queued overlap");
    m.QueueAfter(10,new IdentifiedTrack(20,source));Check(m.Snapshot().Fading,"duplicate queue notification does not restart an active fade");
    Read(m,1);Check(m.CurrentId==20&&m.Snapshot().Position>.99,"incoming track retains played overlap and its stable playback ID");
    m.QueueAfter(20,new IdentifiedTrack(30,source));m.SeekId(20,9.5);Read(m,.6);Check(m.CurrentId==30,"subsequent queue notification appends to the current track");
    m.SeekId(30,1);Check(m.CurrentId==30&&m.Snapshot().Position==1&&!m.Snapshot().Fading,"ID-based seek cancels overlap");
    m.QueueAfter(30,null);Read(m,20);Check(m.Snapshot().Ended,"cleared queue ends normally");
    m.SeekId(30,0);Read(m,.1);Check(m.CurrentId==30&&m.Snapshot().Position>0,"repeat one can restart a completed current track");
   }
   using(var m=new Mixer(new ITrack[]{new IdentifiedTrack(10,source)})){
    m.Configure(true,2);m.QueueAfter(10,new IdentifiedTrack(20,source));m.SeekId(10,9);Read(m,.1);m.QueueAfter(10,null);Check(!m.Snapshot().Fading,"repeat-one queue cancellation cancels the overlap");
   }
   var files=Directory.GetFiles(Path.Combine(AppContext.BaseDirectory,"Fixtures")).Where(p=>p.EndsWith(".mp3")||p.EndsWith(".wma")).Prepend(source).ToArray();
   Tests.Decode(Path.Combine(report,"Decoder-Results.json"),files);Check(true,"WAV/MP3/WMA and mixed-format rendering passed");
   File.WriteAllText(Path.Combine(report,"Integration-Mixer-Results.json"),JsonSerializer.Serialize(new{passed=passed.Count,checks=passed},new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine("PASS original mixer suite plus integration queue and decoder checks. No audio device opened.");return 0;
  }catch(Exception e){File.WriteAllText(Path.Combine(report,"Failure.txt"),e.ToString());Console.WriteLine(e);return 1;}
 }
}
