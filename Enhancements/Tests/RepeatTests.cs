using System;using System.Collections.Generic;using ZuneRestoration;
public sealed class FakeChoice {
 bool value;public event EventHandler Changed;
 public bool Value {get{return value;}set{if(this.value==value)return;this.value=value;if(Changed!=null)Changed(this,EventArgs.Empty);}}
}
public sealed class Track {public string Name;public Track(string n){Name=n;}}
public sealed class Playlist {public Track CurrentTrack;public int Count;public Track NextTrack;}
public sealed class Native {
 public object Queued,Playing;public int CancelCalls;
 public void CancelNext(){Queued=null;CancelCalls++;}
}
public sealed class Transport {
 public FakeChoice Repeating=new FakeChoice();public Playlist _playlistCurrent=new Playlist();public object _lastKnownPreparedTrack;
 public Native _playbackWrapper=new Native();public int NativeChanges,RecursiveChanges;
 public Transport(){Repeating.Changed+=delegate{if(RepeatFeature.OnChanged(this)){RecursiveChanges++;return;}NativeChanges++;};}
 public void Click(){Repeating.Value=!Repeating.Value;}
 public void Prepare(object selected){_playbackWrapper.Queued=RepeatFeature.ChooseNext(this,selected,_playlistCurrent.NextTrack);}
 public void End(){if(_playbackWrapper.Queued!=null){_lastKnownPreparedTrack=_playbackWrapper.Queued;_playbackWrapper.Playing=_lastKnownPreparedTrack;_playlistCurrent.CurrentTrack=(Track)_lastKnownPreparedTrack;}}
}
class Tests {
 static int checks;static void Check(bool c,string label){if(!c)throw new Exception(label);checks++;Console.WriteLine("PASS "+label);}
 static int Main(){try{
  var a=new Track("A");var b=new Track("B");var c=new Track("C");var t=new Transport();t._playlistCurrent.CurrentTrack=a;t._playlistCurrent.NextTrack=b;t._playlistCurrent.Count=8;t._playbackWrapper.Playing=a;
  Check(!RepeatFeature.IsOne()&&RepeatFeature.DescriptionText()=="Repeat off","initial state stays repeat off");
  Check(Object.ReferenceEquals(RepeatFeature.ChooseNext(t,a,b),b),"repeat off keeps the normal next track");
  t.Click();Check(!RepeatFeature.IsOne()&&t.Repeating.Value&&RepeatFeature.DescriptionText()=="Repeat all","first click remains repeat all");
  t._playbackWrapper.Queued=b;t._lastKnownPreparedTrack=b;t.Click();
  Check(RepeatFeature.IsOne()&&t.Repeating.Value&&RepeatFeature.DescriptionText()=="Repeat one","second click enables repeat one with correct hover text");
  Check(t._playbackWrapper.Queued==null&&t._playbackWrapper.CancelCalls==1,"entering repeat one cancels the previously queued different track");
  Check(Object.ReferenceEquals(t._lastKnownPreparedTrack,b),"entering repeat one preserves the prepared user selection");
  Check(t.NativeChanges==2&&t.RecursiveChanges==1,"three-state checkbox handles its recursive value event once");
  t._lastKnownPreparedTrack=a;t.Prepare(null);
  Check(Object.ReferenceEquals(t._playbackWrapper.Queued,a),"playlist prefetch queues the current track instead of the next row");
  t.End();Check(Object.ReferenceEquals(t._playbackWrapper.Playing,t._playlistCurrent.CurrentTrack)&&Object.ReferenceEquals(t._playbackWrapper.Playing,a),"playlist repeat keeps audio and displayed track on the same item");
  for(int i=0;i<4;i++){t.Prepare(null);t.End();}Check(Object.ReferenceEquals(t._playbackWrapper.Playing,a),"multiple playlist repeats do not advance silently");
  t._lastKnownPreparedTrack=b;t.Prepare(b);
  Check(Object.ReferenceEquals(t._playbackWrapper.Queued,b),"manual selection repeats the newly chosen item, not the old current item");
  Check(Object.ReferenceEquals(t._lastKnownPreparedTrack,b),"manual selection's prepared track is not cleared");
  Check(!RepeatFeature.CancelAutomaticNext(t)&&Object.ReferenceEquals(t._lastKnownPreparedTrack,b),"old automatic-next guard cannot block an explicit selection");
  t._playbackWrapper.Playing=b;t._playlistCurrent.CurrentTrack=b;t._playlistCurrent.NextTrack=c;t.Prepare(null);t.End();
  Check(Object.ReferenceEquals(t._playbackWrapper.Playing,b)&&Object.ReferenceEquals(t._playlistCurrent.CurrentTrack,b),"repeat follows the manual selection on its next cycle");
  Check(RepeatFeature.ShouldReplay(false,true),"single-item or missed-prefetch natural end retains replay fallback");
  Check(!RepeatFeature.ShouldReplay(false,false),"manual stop and selection do not trigger end-of-media replay");
  t.Click();Check(!RepeatFeature.IsOne()&&!t.Repeating.Value&&RepeatFeature.DescriptionText()=="Repeat off","third click disables repeating and restores hover text");
  t.Prepare(null);Check(Object.ReferenceEquals(t._playbackWrapper.Queued,c),"disabling repeat one restores normal playlist progression");
  Check(!RepeatFeature.ShouldReplay(false,true)&&RepeatFeature.ShouldReplay(true,true),"normal off/all replay decisions remain intact");
  t.Click();t.Click();t._playlistCurrent=null;Check(RepeatFeature.ChooseNext(t,null,c)==null,"an empty or disposed playlist cannot queue an unrelated track");
  Console.WriteLine("ALL "+checks+" repeat-one regression checks passed against the patched helper. No Zune UI/audio opened.");return 0;
 }catch(Exception e){Console.WriteLine(e);return 1;}}
}
