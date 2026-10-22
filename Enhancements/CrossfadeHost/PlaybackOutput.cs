using NAudio.Wave;

namespace CrossfadePrototype;

// Pausing the provider alone leaves the driver running. Freeze the actual output
// queue as well, and never restart it as a side effect of seeking while paused.
sealed class PlaybackOutput {
    readonly IWavePlayer output;
    readonly Mixer mixer;
    bool paused=true;
    public PlaybackOutput(IWavePlayer output, Mixer mixer) { this.output=output; this.mixer=mixer; }
    public void SetPaused(bool value) {
        paused=value;
        if(value) {
            if(output.PlaybackState==PlaybackState.Playing) output.Pause();
            mixer.Pause(true);
        } else {
            mixer.Pause(false);
            if(output.PlaybackState!=PlaybackState.Playing) output.Play();
        }
    }
    public void Seek(int id,double seconds) {
        output.Stop();
        mixer.SeekId(id,seconds);
        mixer.Pause(paused);
        if(!paused) output.Play();
    }
}
