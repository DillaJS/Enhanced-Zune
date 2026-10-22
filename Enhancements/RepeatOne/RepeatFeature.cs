using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
namespace ZuneRestoration {
 public static class RepeatFeature {
  private static readonly BindingFlags F=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
  private static int mode;private static bool changing;
  private static readonly Hashtable originals=new Hashtable(),numbered=new Hashtable();
  private static readonly string cache=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),@"Microsoft\Zune\Repeat One");
  private static string Key(object value,string name){Type notify=value.GetType().Assembly.GetType("Microsoft.Iris.Markup.NotifyService");return notify==null?name:(string)notify.GetMethod("CanonicalizeString",F).Invoke(null,new object[]{name});}
  private static object Read(object value,string name){if(value==null)return null;PropertyInfo p=value.GetType().GetProperty(name,F);if(p!=null)return p.GetValue(value,null);FieldInfo field=value.GetType().GetField(name,F);if(field!=null)return field.GetValue(value);MethodInfo get=value.GetType().GetMethod("GetProperty",F,null,new Type[]{typeof(string)},null);return get==null?null:get.Invoke(value,new object[]{Key(value,name)});}
  private static void Put(object value,string name,object data){PropertyInfo p=value.GetType().GetProperty(name,F);if(p!=null){p.SetValue(value,data,null);return;}value.GetType().GetMethod("SetProperty",F,null,new Type[]{typeof(string),typeof(object)},null).Invoke(value,new object[]{Key(value,name),data});}
  private static void Log(Exception error){try{Directory.CreateDirectory(cache);File.AppendAllText(Path.Combine(cache,"errors.txt"),DateTime.Now.ToString("s")+" "+error+Environment.NewLine);}catch{}}
  public static bool IsOne(){return mode==2;}
  public static string DescriptionText(){return mode==0?"Repeat off":mode==1?"Repeat all":"Repeat one";}
  public static bool OnChanged(object transport){if(changing)return true;try{object choice=Read(transport,"Repeating");bool value=Convert.ToBoolean(Read(choice,"Value"));if(value){mode=1;}else if(mode==1){mode=2;changing=true;try{Put(choice,"Value",true);}finally{changing=false;}CancelQueuedNext(transport);}else{mode=0;}return false;}catch(Exception ex){Log(ex);return false;}}
  public static bool ShouldReplay(bool original,bool endOfMedia){return original||(IsOne()&&endOfMedia);}
  // Prepared tracks include explicit user selections, not just automatic-next
  // tracks. Preserve that state and let Zune's normal transition code run.
  public static bool CancelAutomaticNext(object transport){return false;}
  private static void CancelQueuedNext(object transport){object player=Read(transport,"_playbackWrapper");if(player!=null)player.GetType().GetMethod("CancelNext",F).Invoke(player,null);}
  public static object ChooseNext(object transport,object selected,object originalNext){
   if(!IsOne())return originalNext;
   if(selected!=null)return selected;
   object playlist=Read(transport,"_playlistCurrent");
   return Read(playlist,"CurrentTrack");
  }
  public static void UpdateDescription(object transport){try{object choice=Read(transport,"Repeating");if(!Convert.ToBoolean(Read(choice,"Value")))mode=0;else if(mode==0)mode=1;Put(choice,"Description",DescriptionText());UpdateVisuals();}catch(Exception ex){Log(ex);}}
  private static object SharedImages(){Assembly iris=Assembly.Load("UIX");Type system=iris.GetType("Microsoft.Iris.Markup.MarkupSystem",true),pass=iris.GetType("Microsoft.Iris.Markup.LoadPass",true);object loaded=system.GetMethod("Load",F,null,new Type[]{typeof(string),typeof(uint)},null).Invoke(null,new object[]{"res://ZuneShellResources!ImageStyle.uix",(uint)100});for(int i=1;i<=4;i++)loaded.GetType().GetMethod("Load",F,null,new Type[]{pass},null).Invoke(loaded,new object[]{Enum.ToObject(pass,i)});foreach(object schema in (IEnumerable)Read(loaded,"ExportTable"))if((string)Read(schema,"Name")=="Images")return Read(schema,"SharedInstance");return null;}
  public static void UpdateVisuals(){try{if(mode!=2&&originals.Count==0)return;object images=SharedImages();if(images==null)return;foreach(string name in new string[]{"TransportRepeatingOn","NowPlayingTransportRepeatingOn"}){object style=Read(images,name);if(style==null)continue;Hashtable saved=originals[style] as Hashtable;if(saved==null){saved=new Hashtable();foreach(string prop in new string[]{"Image","HoverImage","PressedImage"})saved[prop]=Read(style,prop);originals[style]=saved;}foreach(string prop in new string[]{"Image","HoverImage","PressedImage"}){object original=saved[prop];if(original!=null)Put(style,prop,mode==2?NumberedImage(original):original);}}}catch(Exception ex){Log(ex);}}
  private static object NumberedImage(object original){string source=Read(original,"Source") as string;if(String.IsNullOrEmpty(source))return original;object cached=numbered[source];if(cached!=null)return cached;int bang=source.IndexOf('!');if(bang<0)return original;string resource=source.Substring(bang+1).ToUpperInvariant();using(Stream input=typeof(RepeatFeature).Assembly.GetManifestResourceStream("RepeatSource."+resource)){if(input==null)return original;Directory.CreateDirectory(cache);string path=Path.Combine(cache,resource+".one-v1.png");using(Bitmap baseImage=new Bitmap(input))using(Bitmap canvas=new Bitmap(baseImage.Width,baseImage.Height,System.Drawing.Imaging.PixelFormat.Format32bppArgb)){using(Graphics graphics=Graphics.FromImage(canvas)){graphics.DrawImageUnscaled(baseImage,0,0);Color color=Color.FromArgb(255,236,0,140);int alpha=0;for(int y=0;y<baseImage.Height;y++)for(int x=0;x<baseImage.Width;x++){Color sample=baseImage.GetPixel(x,y);if(sample.A>alpha){alpha=sample.A;color=Color.FromArgb(255,sample.R,sample.G,sample.B);}}graphics.SmoothingMode=SmoothingMode.AntiAlias;float scale=canvas.Width/92f;using(Pen pen=new Pen(color,3.5f*scale)){pen.StartCap=LineCap.Round;pen.EndCap=LineCap.Round;pen.LineJoin=LineJoin.Round;graphics.DrawLines(pen,new PointF[]{new PointF(43f*scale,44f*scale),new PointF(48f*scale,40f*scale),new PointF(48f*scale,55f*scale)});graphics.DrawLine(pen,44f*scale,55f*scale,52f*scale,55f*scale);}}canvas.Save(path,System.Drawing.Imaging.ImageFormat.Png);}Assembly iris=original.GetType().Assembly;Type image=iris.GetType("Microsoft.Iris.Drawing.UriImage",true);object result=Activator.CreateInstance(image);Put(result,"Source","file://"+path);numbered[source]=result;return result;}}
 }
}


