using System.Diagnostics;
using System.Globalization;
using System.Text;
using NAudio.Wave;

namespace CrossfadePrototype;

public sealed class IdentifiedTrack : ITrack {
    readonly ITrack track;
    public int Id { get; }
    public IdentifiedTrack(int id, string path) { Id = id; track = new FileTrack(path); }
    public string Name => track.Name;
    public long Length => track.Length;
    public long Position => track.Position;
    public int Read(float[] b, int count) => track.Read(b, count);
    public void Seek(long frame) => track.Seek(frame);
    public void Dispose() => track.Dispose();
}

static class Program {
    static readonly object gate = new(), stdout = new();
    static Mixer? mixer;
    static WaveOutEvent? output;
    static PlaybackOutput? playback;
    static bool quitting;
    static int parent;
    static string? outputError;
    static string Decode(string s) => Encoding.UTF8.GetString(Convert.FromBase64String(s));
    static int Int(string s) => int.Parse(s, CultureInfo.InvariantCulture);
    static double Num(string s) => double.Parse(s, CultureInfo.InvariantCulture);
    static void Say(string s) { lock(stdout) { Console.WriteLine(s); Console.Out.Flush(); } }
    static void Stop() {
        // Stop outside the mixer lock, allowing the output thread to finish its read.
        output?.Stop(); output?.Dispose(); output = null; playback=null;
        lock(gate) { mixer?.Dispose(); mixer = null; }
    }
    static void Receive(string line) {
        string[] a = line.Split('|');
        switch(a[0]) {
            case "LOAD": {
                Stop(); var track = new IdentifiedTrack(Int(a[1]), Decode(a[2]));
                lock(gate) { mixer = new Mixer(new ITrack[]{track}); mixer.Configure(true, Num(a[3])); mixer.Pause(true); }
                Say("READY|"+a[1]); break;
            }
            case "GO": {
                lock(gate) {
                    if(mixer == null) break;
                    mixer.SeekId(Int(a[1]), Num(a[2])); mixer.SetVolume((float)Num(a[4]));
                }
                if(output == null) { output = new WaveOutEvent { DesiredLatency=80, NumberOfBuffers=3 }; output.PlaybackStopped += (_,e) => { if(e.Exception!=null) outputError=e.Exception.Message; }; output.Init(mixer!); playback=new PlaybackOutput(output,mixer!); }
                playback!.SetPaused(a[3]=="1"); Say("ACTIVE|"+a[1]); break;
            }
            case "QUEUE": {
                IdentifiedTrack? track = a[2]=="-1" ? null : new IdentifiedTrack(Int(a[2]), Decode(a[3]));
                lock(gate) { if(mixer == null) track?.Dispose(); else mixer.QueueAfter(Int(a[1]),track); }
                break;
            }
            case "STATE": {
                lock(gate) {
                    if(mixer == null) break;
                    mixer.SetVolume((float)Num(a[2])); mixer.Configure(a[3]=="1",Num(a[4]));
                }
                playback?.SetPaused(a[1]=="1");
                break;
            }
            case "PAUSE": playback?.SetPaused(a[1]=="1"); break;
            case "SEEK": {
                playback?.Seek(Int(a[1]),Num(a[2])); break;
            }
            case "QUIT": quitting=true; break;
            default: throw new InvalidDataException("Unknown bridge message.");
        }
    }
    static int Main(string[] args) {
        if(args.Length!=2 || args[0]!="--parent" || !int.TryParse(args[1],out parent)) return 2;
        var reporter = new Thread(() => {
            while(!quitting) {
                try {
                    if(outputError!=null) { Say("ERROR|"+Convert.ToBase64String(Encoding.UTF8.GetBytes(outputError))); Environment.Exit(1); }
                    using var p = Process.GetProcessById(parent);
                    if(p.HasExited) Environment.Exit(0);
                    lock(gate) {
                        if(mixer != null) {
                            var s=mixer.Snapshot();
                            Say("POS|"+mixer.CurrentId+"|"+s.Position.ToString("F5",CultureInfo.InvariantCulture)+"|"+(s.Fading?"1":"0")+"|"+(s.Ended?"1":"0"));
                        } else Say("IDLE");
                    }
                } catch { Environment.Exit(0); }
                Thread.Sleep(100);
            }
        }) { IsBackground=true, Name="Zune crossfade host status" };
        reporter.Start();
        try {
            string? line;
            while(!quitting && (line=Console.ReadLine())!=null) {
                try { Receive(line); }
                catch(Exception e) { Stop(); Say("ERROR|"+Convert.ToBase64String(Encoding.UTF8.GetBytes(e.GetBaseException().Message))); }
            }
        } finally { quitting=true; Stop(); }
        return 0;
    }
}
