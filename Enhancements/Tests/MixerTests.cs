using NAudio.Wave;
using System.Text.Json;

namespace CrossfadePrototype;

sealed class Signal : ITrack {
    public string Name { get; }
    public long Length { get; }
    public long Position { get; private set; }
    readonly float value;
    readonly double frequency;
    public bool Disposed { get; private set; }
    public Signal(string name, double duration, float value, double frequency = 0) { Name = name; Length = (long)(duration * Mixer.Rate); this.value = value; this.frequency = frequency; }
    public int Read(float[] data, int count) {
        int frames = (int)Math.Min(count / 2, Length - Position);
        for (int i = 0; i < frames; i++) {
            float sample = frequency == 0 ? value : value * (float)Math.Sin(2 * Math.PI * frequency * (Position + i) / Mixer.Rate);
            data[2 * i] = data[2 * i + 1] = sample;
        }
        Position += frames; return frames * 2;
    }
    public void Seek(long frame) => Position = Math.Clamp(frame, 0, Length);
    public void Dispose() => Disposed = true;
}

static class Tests {
    public static void Decode(string report, string[] files) {
        var results = new List<object>();
        foreach (string file in files) {
            using var m = new Mixer(new ITrack[] { new FileTrack(file), new FileTrack(file) });
            m.Configure(true, 1); var samples = Render(m);
            Check(samples.All(float.IsFinite), "Finite samples decoded: " + Path.GetFileName(file));
            results.Add(new { file, seconds = (double)samples.Length / 2 / Mixer.Rate, finite = true });
        }
        using (var m = new Mixer(files.Select(f => (ITrack)new FileTrack(f)).ToArray())) {
            m.Configure(true, 1); float[] samples = Render(m);
            Check(samples.Length > 0 && samples.All(float.IsFinite), "Mixed-format playlist renders correctly");
            results.Add(new { playlist = true, seconds = (double)samples.Length / 2 / Mixer.Rate });
        }
        File.WriteAllText(report, JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true }));
    }
    static readonly List<string> passed = new();
    static void Check(bool value, string name) { if (!value) throw new Exception("FAIL: " + name); passed.Add(name); }
    static float[] Render(Mixer mixer, int chunk = 1024) {
        List<float> data = new(); float[] buffer = new float[chunk]; int n;
        while ((n = mixer.Read(buffer, 0, buffer.Length)) > 0) { data.AddRange(buffer.Take(n)); if (data.Count > 12_000_000) throw new Exception("Renderer did not reach the end."); }
        return data.ToArray();
    }
    static Mixer Pair() { var m = new Mixer(new ITrack[] { new Signal("A", 4, .6f), new Signal("B", 4, -.6f) }); m.SetVolume(1); return m; }
    static void Approx(float a, float b, string name) => Check(Math.Abs(a - b) < .00002f, name);
    public static void Run(string directory) {
        Directory.CreateDirectory(directory);
        using (var m = Pair()) {
            Check(!m.Snapshot().Enabled, "Crossfading defaults to off");
            var a = Render(m); Check(a.Length == 8 * Mixer.Rate * 2, "Off preserves both full track durations");
            Approx(a[4 * Mixer.Rate * 2 - 2], .6f, "Off preserves outgoing track end");
            Approx(a[4 * Mixer.Rate * 2], -.6f, "Off starts next track without overlap");
        }
        float[] baseline;
        using (var m = Pair()) {
            m.Configure(true, 1); baseline = Render(m);
            Check(baseline.Length == 7 * Mixer.Rate * 2, "One-second overlap shortens total duration by exactly one second");
            Approx(baseline[3 * Mixer.Rate * 2], .6f, "Fade starts at full outgoing and zero incoming gain");
            Approx(baseline[4 * Mixer.Rate * 2 - 2], -.6f, "Fade ends at zero outgoing and full incoming gain");
            Approx(baseline[7 * Mixer.Rate * 2 - 2], -.6f, "Incoming track continues after overlap without replaying its intro");
            Check(baseline.All(float.IsFinite), "Rendered output contains no NaN or infinity");
        }
        using (var m = Pair()) { m.Configure(true, 1); Check(baseline.SequenceEqual(Render(m, 110)), "Fade timing is independent of output block size"); }
        using (var m = Pair()) {
            m.Configure(true, 1); var b = new float[1024]; m.Read(b, 0, b.Length); var before = m.Snapshot().Position;
            m.Pause(true); m.Read(b, 0, b.Length); Check(b.All(x => x == 0) && m.Snapshot().Position == before, "Pause outputs silence without consuming either track");
            m.Pause(false); m.Read(b, 0, b.Length); Check(m.Snapshot().Position > before, "Resume continues from the paused position");
            m.Seek(3.5); m.Read(b, 0, b.Length); Check(m.Snapshot().Fading, "Seek near the end can start a bounded overlap");
            m.Seek(1); Check(!m.Snapshot().Fading, "Seeking cancels the current overlap");
            m.Next(); Check(m.Snapshot().Index == 1 && m.Snapshot().Position == 0, "Manual next cancels overlap and starts selected track from its beginning");
        }
        using (var m = Pair()) {
            m.Configure(true, 0); Check(Render(m).Length == 8 * Mixer.Rate * 2, "Zero duration produces no overlap");
        }
        using (var m = Pair()) {
            m.Configure(true, 1); var b = new float[3 * Mixer.Rate * 2 + 100]; m.Read(b, 0, b.Length);
            Check(m.Snapshot().Fading, "Overlap begins during playback");
            m.Configure(false, 1); Render(m); Check(m.Snapshot().Ended, "Disabling during a fade completes the active transition safely");
        }
        using (var m = new Mixer(new ITrack[] { new Signal("Short A", .1, .5f), new Signal("Short B", .1, .5f), new Signal("Short C", .1, .5f) })) {
            m.SetVolume(1); m.Configure(true, 10); var b = Render(m);
            Check(b.Length == (int)(.2 * Mixer.Rate * 2), "Very short tracks clamp overlap to half the shorter track");
            Check(b.All(x => Math.Abs(x - .5f) < .00002), "Identical signals retain level through the linear fade");
        }
        using (var m = new Mixer(new ITrack[] { new Signal("Empty", 0, 0), new Signal("Valid", 1, .2f) })) Check(Render(m).Length == Mixer.Rate * 2, "Empty track cannot stall playback");
        var disposable = new Signal("D", 1, 1); var owner = new Mixer(new ITrack[] { disposable }); owner.Dispose(); owner.Dispose(); Check(disposable.Disposed, "Disposal releases owned decoders and is idempotent");
        foreach (bool enabled in new[] { false, true }) {
            using var m = new Mixer(new ITrack[] { new Signal("A", 5, .25f, 220), new Signal("B", 5, .25f, 330) }); m.Configure(enabled, 2); m.SetVolume(1);
            using var writer = new WaveFileWriter(Path.Combine(directory, enabled ? "Crossfade-on-2-seconds.wav" : "Crossfade-off.wav"), m.WaveFormat);
            float[] b = new float[1024]; int n; while ((n = m.Read(b, 0, b.Length)) > 0) writer.WriteSamples(b, 0, n);
        }
        File.WriteAllText(Path.Combine(directory, "Mixer-Test-Results.json"), JsonSerializer.Serialize(new { passed = passed.Count, checks = passed }, new JsonSerializerOptions { WriteIndented = true }));
    }
}
