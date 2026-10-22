using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace CrossfadePrototype;

public interface ITrack : IDisposable {
    string Name { get; }
    long Length { get; }
    long Position { get; }
    int Read(float[] buffer, int count);
    void Seek(long frame);
}

public sealed class FileTrack : ITrack {
    readonly AudioFileReader reader;
    ISampleProvider source = null!;
    public string Name { get; }
    public long Length { get; }
    public long Position { get; private set; }
    public FileTrack(string path) {
        Name = Path.GetFileNameWithoutExtension(path);
        reader = new AudioFileReader(path);
        if (reader.WaveFormat.Channels is not (1 or 2)) { reader.Dispose(); throw new NotSupportedException("The prototype supports mono and stereo files."); }
        Length = (long)Math.Round(reader.TotalTime.TotalSeconds * Mixer.Rate);
        ResetSource();
    }
    void ResetSource() {
        source = reader;
        if (source.WaveFormat.Channels == 1) source = new MonoToStereoSampleProvider(source);
        if (source.WaveFormat.SampleRate != Mixer.Rate) source = new WdlResamplingSampleProvider(source, Mixer.Rate);
    }
    public int Read(float[] buffer, int count) {
        int n = source.Read(buffer, 0, (int)Math.Min(count, Math.Max(0, Length - Position) * 2));
        if ((n & 1) != 0) throw new InvalidDataException("Decoder returned an incomplete stereo frame.");
        Position += n / 2;
        if (n == 0) Position = Length;
        return n;
    }
    public void Seek(long frame) {
        frame = Math.Clamp(frame, 0, Length);
        reader.CurrentTime = TimeSpan.FromSeconds((double)frame / Mixer.Rate);
        Position = frame;
        ResetSource();
    }
    public void Dispose() => reader.Dispose();
}

public sealed class Mixer : ISampleProvider, IDisposable {
    public const int Rate = 48000;
    public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(Rate, 2);
    readonly object gate = new();
    readonly List<ITrack> tracks;
    readonly float[] a = new float[2048], b = new float[2048];
    int index;
    bool enabled, paused, disposed;
    double seconds = 5;
    long fadeFrames, fadePosition;
    bool fading;
    float masterVolume = .5f;
    public Mixer(ITrack[] tracks) { this.tracks = tracks.ToList(); }
    public void Configure(bool enable, double overlapSeconds) {
        if (!double.IsFinite(overlapSeconds) || overlapSeconds < 0 || overlapSeconds > 10) throw new ArgumentOutOfRangeException(nameof(overlapSeconds));
        lock (gate) { enabled = enable; seconds = overlapSeconds; }
    }
    public void SetVolume(float volume) {
        if (!float.IsFinite(volume) || volume < 0 || volume > 1) throw new ArgumentOutOfRangeException(nameof(volume));
        lock (gate) masterVolume = volume;
    }
    public void Pause(bool pause) { lock (gate) paused = pause; }
    public (int Index, string Name, double Position, double Duration, bool Fading, bool Enabled, bool Ended) Snapshot() {
        lock (gate) {
            if (index >= tracks.Count) return (index, "Finished", 0, 0, false, enabled, true);
            var t = tracks[index]; return (index, t.Name, (double)t.Position / Rate, (double)t.Length / Rate, fading, enabled, false);
        }
    }
    void CancelFade() {
        if (fading && index + 1 < tracks.Count) tracks[index + 1].Seek(0);
        fading = false; fadePosition = fadeFrames = 0;
    }
    public void Seek(double position) {
        if (!double.IsFinite(position)) throw new ArgumentOutOfRangeException(nameof(position));
        lock (gate) { if (index < tracks.Count) { CancelFade(); tracks[index].Seek((long)(Math.Max(0, position) * Rate)); } }
    }
    public void Next() {
        lock (gate) { CancelFade(); if (index < tracks.Count) index++; if (index < tracks.Count) tracks[index].Seek(0); }
    }
    public int Read(float[] buffer, int offset, int count) {
        if (offset < 0 || count < 0 || offset + count > buffer.Length || (count & 1) != 0) throw new ArgumentOutOfRangeException(nameof(count));
        lock (gate) {
            if (disposed) return 0;
            if (paused) { Array.Clear(buffer, offset, count); return count; }
            int written = 0;
            while (written < count && index < tracks.Count) {
                var current = tracks[index];
                long remaining = Math.Max(0, current.Length - current.Position);
                if (remaining == 0) { Advance(); continue; }
                long overlap = 0;
                if (enabled && seconds > 0 && index + 1 < tracks.Count)
                    overlap = Math.Min((long)Math.Round(seconds * Rate), Math.Min(current.Length / 2, tracks[index + 1].Length / 2));
                if (!fading && overlap > 0 && remaining <= overlap) {
                    tracks[index + 1].Seek(0);
                    fadeFrames = remaining; fadePosition = 0; fading = true;
                }
                int frames = Math.Min((count - written) / 2, a.Length / 2);
                frames = (int)Math.Min(frames, remaining);
                if (!fading && overlap > 0 && remaining > overlap) frames = (int)Math.Min(frames, remaining - overlap);
                if (fading) frames = (int)Math.Min(frames, fadeFrames - fadePosition);
                int n = current.Read(a, frames * 2);
                if (n == 0) { Advance(); continue; }
                int incoming = fading ? tracks[index + 1].Read(b, n) : 0;
                for (int frame = 0; frame < n / 2; frame++) {
                    float gainB = fading ? (float)((double)(fadePosition + frame) / Math.Max(1, fadeFrames - 1)) : 0;
                    for (int channel = 0; channel < 2; channel++) {
                        int sample = frame * 2 + channel;
                        float mix = a[sample] * (1 - gainB) + (sample < incoming ? b[sample] * gainB : 0);
                        buffer[offset + written + sample] = Math.Clamp(mix * masterVolume, -1, 1);
                    }
                }
                if (fading) fadePosition += n / 2;
                written += n;
            }
            return written;
        }
    }
    public int CurrentId { get { lock(gate) { return index < tracks.Count ? ((IdentifiedTrack)tracks[index]).Id : -1; } } }
    void Advance() {
        if(index+1 < tracks.Count) {
            tracks[index].Dispose(); tracks.RemoveAt(index);
        } else index++;
        fading=false; fadePosition=fadeFrames=0;
    }
    public void QueueAfter(int afterId, IdentifiedTrack? next) {
        lock(gate) {
            int after = tracks.FindIndex(t => ((IdentifiedTrack)t).Id == afterId);
            if (after < index || after < 0) { next?.Dispose(); return; }
            if (after+1 < tracks.Count && next != null && ((IdentifiedTrack)tracks[after+1]).Id == next.Id) { next.Dispose(); return; }
            if (after == index) CancelFade();
            for(int i=tracks.Count-1;i>after;i--) { tracks[i].Dispose(); tracks.RemoveAt(i); }
            if(next != null) tracks.Add(next);
        }
    }
    public void SeekId(int id, double seconds) {
        lock(gate) {
            int target=tracks.FindIndex(t => ((IdentifiedTrack)t).Id == id);
            if(target < 0) throw new InvalidOperationException("Requested track is not queued.");
            CancelFade(); index=target; tracks[index].Seek((long)(Math.Max(0,seconds)*Rate));
        }
    }
    public void Dispose() { lock (gate) { if (disposed) return; disposed = true; foreach (var t in tracks) t.Dispose(); } }
}
