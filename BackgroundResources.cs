using System;using System.IO;using System.Text;using System.Collections;using System.Security.Cryptography;using System.Runtime.InteropServices;using System.Drawing;
namespace ZuneRestoration {
 internal static class BackgroundResources {
  internal const string Meta="CUSTOMBACKGROUND.SLOTS.V1";
  internal const string Magic="ZUNE DLL CUSTOM BACKGROUNDS 1";
  internal static string Name(int i){return "FRAMEBACKGROUNDCUSTOM"+(i+1).ToString("00")+".PNG";}
  internal static string Uri(int i){return "res://ZuneShellResources!FrameBackgroundCustom"+(i+1).ToString("00")+".png";}
  internal static string Digest(byte[] b){using(SHA256 s=SHA256.Create())return BitConverter.ToString(s.ComputeHash(b)).Replace("-","");}
  internal static string UpdateRoot {get{return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),@"Microsoft\Zune\BackgroundResourceUpdates");}}
  internal static byte[] ReadBounded(string path,int limit){using(FileStream s=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read)){if(s.Length<1||s.Length>limit)throw new InvalidDataException("Invalid file size.");byte[] b=new byte[(int)s.Length];int at=0;while(at<b.Length){int n=s.Read(b,at,b.Length-at);if(n==0)throw new EndOfStreamException();at+=n;}return b;}}
  internal static void ValidatePng(byte[] b){
   if(b.Length<24||b.Length>40*1024*1024||b[0]!=137||b[1]!=80||b[2]!=78||b[3]!=71)throw new InvalidDataException("Invalid slot PNG.");
   int w=BE(b,16),h=BE(b,20);if(w<1||h<1||w>4096||h>4096||(long)w*h>8388608)throw new InvalidDataException("Slot image exceeds safe texture dimensions.");
   using(MemoryStream ms=new MemoryStream(b))using(Image image=Image.FromStream(ms,true,true)){if(image.RawFormat.Guid!=System.Drawing.Imaging.ImageFormat.Png.Guid||image.Width!=w||image.Height!=h)throw new InvalidDataException("Invalid PNG contents.");using(Bitmap check=new Bitmap(image)){} }
  }
  static int BE(byte[] b,int p){return b[p]<<24|b[p+1]<<16|b[p+2]<<8|b[p+3];}
  [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern IntPtr LoadLibraryEx(string path,IntPtr file,uint flags);
  [DllImport("kernel32.dll")]static extern bool FreeLibrary(IntPtr module);
  [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern IntPtr FindResourceEx(IntPtr module,IntPtr type,IntPtr name,ushort language);
  [DllImport("kernel32.dll")]static extern IntPtr LoadResource(IntPtr module,IntPtr resource);
  [DllImport("kernel32.dll")]static extern IntPtr LockResource(IntPtr resource);
  [DllImport("kernel32.dll")]static extern uint SizeofResource(IntPtr module,IntPtr resource);
  delegate bool TypeCallback(IntPtr module,IntPtr type,IntPtr parameter);
  delegate bool NameCallback(IntPtr module,IntPtr type,IntPtr name,IntPtr parameter);
  delegate bool LanguageCallback(IntPtr module,IntPtr type,IntPtr name,ushort language,IntPtr parameter);
  [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern bool EnumResourceTypes(IntPtr module,TypeCallback callback,IntPtr parameter);
  [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern bool EnumResourceNames(IntPtr module,IntPtr type,NameCallback callback,IntPtr parameter);
  [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern bool EnumResourceLanguages(IntPtr module,IntPtr type,IntPtr name,LanguageCallback callback,IntPtr parameter);
  static string Identifier(IntPtr p){return ((ulong)p.ToInt64()>>16)==0?"#"+p.ToInt64():Marshal.PtrToStringUni(p);}
  internal static Hashtable ReadAll(string path){
   IntPtr module=LoadLibraryEx(Path.GetFullPath(path),IntPtr.Zero,34);if(module==IntPtr.Zero)throw new IOException("Cannot read resource DLL: "+Marshal.GetLastWin32Error());
   Hashtable result=new Hashtable(StringComparer.Ordinal);Exception failure=null;
   try{
    LanguageCallback lang=delegate(IntPtr m,IntPtr t,IntPtr n,ushort l,IntPtr data){try{IntPtr res=FindResourceEx(m,t,n,l);uint size=SizeofResource(m,res);if(res==IntPtr.Zero||size>64*1024*1024)throw new InvalidDataException("Invalid DLL resource.");byte[] b=new byte[size];Marshal.Copy(LockResource(LoadResource(m,res)),b,0,b.Length);result[Identifier(t)+"|"+Identifier(n)+"|"+l]=b;return true;}catch(Exception e){failure=e;return false;}};
    NameCallback names=delegate(IntPtr m,IntPtr t,IntPtr n,IntPtr data){return EnumResourceLanguages(m,t,n,lang,IntPtr.Zero);};
    TypeCallback types=delegate(IntPtr m,IntPtr t,IntPtr data){return EnumResourceNames(m,t,names,IntPtr.Zero);};
    if(!EnumResourceTypes(module,types,IntPtr.Zero)||failure!=null)throw new IOException("Resource enumeration failed.",failure);
    GC.KeepAlive(lang);GC.KeepAlive(names);GC.KeepAlive(types);return result;
   }finally{FreeLibrary(module);}
  }
  internal static string Key(string name){return "#10|"+name+"|1033";}
  internal static string[] SlotFlags(string path){
   IntPtr module=LoadLibraryEx(Path.GetFullPath(path),IntPtr.Zero,34),name=IntPtr.Zero;
   try{if(module==IntPtr.Zero)throw new IOException("Cannot read custom slot metadata.");name=Marshal.StringToHGlobalUni(Meta);IntPtr res=FindResourceEx(module,new IntPtr(10),name,1033);if(res==IntPtr.Zero)return new string[]{"","",""};uint size=SizeofResource(module,res);if(size>512)throw new InvalidDataException("Invalid custom metadata size.");byte[] bytes=new byte[size];Marshal.Copy(LockResource(LoadResource(module,res)),bytes,0,bytes.Length);string[] lines=Encoding.ASCII.GetString(bytes).Split('\n');if(lines.Length!=4||lines[0]!=Magic)throw new InvalidDataException("Invalid custom metadata.");string[] flags=new string[3];for(int i=0;i<3;i++){flags[i]=lines[i+1];if(flags[i]!=""){if(flags[i].Length!=64)throw new InvalidDataException("Invalid slot checksum.");foreach(char c in flags[i])if(!((c>='0'&&c<='9')||(c>='A'&&c<='F')))throw new InvalidDataException("Invalid slot checksum.");}}return flags;}
   finally{if(name!=IntPtr.Zero)Marshal.FreeHGlobal(name);if(module!=IntPtr.Zero)FreeLibrary(module);}
  }
  internal static void DeleteTransaction(string folder){
   string full=Path.GetFullPath(folder),parent=Path.GetDirectoryName(full);Guid id;try{id=new Guid(Path.GetFileName(full));}catch{return;}
   if(id.ToString("N")!=Path.GetFileName(full)||!String.Equals(parent,Path.GetFullPath(UpdateRoot).TrimEnd(Path.DirectorySeparatorChar),StringComparison.OrdinalIgnoreCase)||(File.GetAttributes(full)&FileAttributes.ReparsePoint)!=0)return;
   foreach(string name in new string[]{"slot1.png","slot2.png","slot3.png","request.bin","ready.txt","result.txt","restart.tmp","restart.txt","abort.txt","previous-background.txt","previous-color.txt","launch-ok.txt","launch-failed.txt"})try{File.Delete(Path.Combine(full,name));}catch{}
   try{Directory.Delete(full,false);}catch{}
  }
  internal static string[] States(Hashtable resources){
   byte[] data=resources[Key(Meta)] as byte[];if(data==null)return new string[]{"","",""};
   string[] lines=Encoding.ASCII.GetString(data).Split('\n');if(lines.Length!=4||lines[0]!=Magic)throw new InvalidDataException("Custom slot metadata is invalid.");
   string[] state=new string[3];for(int i=0;i<3;i++){state[i]=lines[i+1];if(state[i]!=""){if(state[i].Length!=64||state[i]!=Digest((byte[])resources[Key(Name(i))]))throw new InvalidDataException("Custom slot checksum mismatch.");}}return state;
  }
  internal static byte[] Metadata(string[] state){return Encoding.ASCII.GetBytes(Magic+"\n"+String.Join("\n",state));}
  [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern IntPtr BeginUpdateResource(string file,bool delete);
  [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern bool UpdateResource(IntPtr update,IntPtr type,string name,ushort language,byte[] data,uint size);
  [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern bool EndUpdateResource(IntPtr update,bool discard);
  internal static void Build(string original,string candidate,int[] changes,byte[][] images){
   Hashtable before=ReadAll(original);string[] state=States(before);byte[] fallback=(byte[])before[Key("FRAMEBACKGROUND04.PNG")];ValidatePng(fallback);
   Hashtable expected=new Hashtable(before);
   for(int i=0;i<3;i++){if(changes[i]<-1||changes[i]>1)throw new InvalidDataException("Invalid slot operation.");if(changes[i]<0)continue;byte[] png=changes[i]==0?fallback:images[i];ValidatePng(png);expected[Key(Name(i))]=png;state[i]=changes[i]==0?"":Digest(png);}
   expected[Key(Meta)]=Metadata(state);
   File.Copy(original,candidate,false);IntPtr update=BeginUpdateResource(candidate,false);if(update==IntPtr.Zero)throw new IOException("Cannot stage background resources.");
   try{for(int i=0;i<3;i++)if(changes[i]>=0){byte[] bytes=(byte[])expected[Key(Name(i))];if(!UpdateResource(update,new IntPtr(10),Name(i),1033,bytes,(uint)bytes.Length))throw new IOException("Updating slot failed.");}byte[] meta=(byte[])expected[Key(Meta)];if(!UpdateResource(update,new IntPtr(10),Meta,1033,meta,(uint)meta.Length))throw new IOException("Updating metadata failed.");if(!EndUpdateResource(update,false))throw new IOException("Completing resource update failed.");update=IntPtr.Zero;}finally{if(update!=IntPtr.Zero)EndUpdateResource(update,true);}
   Hashtable actual=ReadAll(candidate);if(actual.Count!=expected.Count)throw new InvalidDataException("Unexpected resource count.");foreach(DictionaryEntry e in expected){byte[] bytes=actual[e.Key] as byte[];if(bytes==null||Digest(bytes)!=Digest((byte[])e.Value))throw new InvalidDataException("Resource preservation check failed: "+e.Key);}States(actual);VerifySections(original,candidate);
  }
  static Hashtable Sections(string path){using(BinaryReader b=new BinaryReader(File.OpenRead(path))){b.BaseStream.Position=60;int pe=b.ReadInt32();b.BaseStream.Position=pe+6;int count=b.ReadUInt16();b.BaseStream.Position=pe+20;int optional=b.ReadUInt16();long table=pe+24+optional;Hashtable result=new Hashtable();for(int i=0;i<count;i++){b.BaseStream.Position=table+40*i;string name=Encoding.ASCII.GetString(b.ReadBytes(8)).TrimEnd('\0');b.BaseStream.Position=table+40*i+16;uint length=b.ReadUInt32(),start=b.ReadUInt32();if(name==".rsrc")continue;b.BaseStream.Position=start;result[name]=Digest(b.ReadBytes(checked((int)length)));}return result;}}
  static void VerifySections(string a,string b){Hashtable x=Sections(a),y=Sections(b);if(x.Count!=y.Count)throw new InvalidDataException("Section count mismatch.");foreach(DictionaryEntry e in x)if(!Object.Equals(e.Value,y[e.Key]))throw new InvalidDataException("Non-resource section changed.");}
  internal static string CheckRequest(string request){
   string file=Path.GetFullPath(request),folder=Path.GetDirectoryName(file),parent=Path.GetDirectoryName(folder);Guid id;
   try{id=new Guid(Path.GetFileName(folder));}catch{throw new InvalidDataException("Invalid transaction directory.");}
   if(id.ToString("N")!=Path.GetFileName(folder)||!String.Equals(parent,Path.GetFullPath(UpdateRoot).TrimEnd(Path.DirectorySeparatorChar),StringComparison.OrdinalIgnoreCase)||Path.GetFileName(file)!="request.bin")throw new InvalidDataException("Request must be in the custom-background update directory.");
   if((File.GetAttributes(folder)&FileAttributes.ReparsePoint)!=0||(File.GetAttributes(file)&FileAttributes.ReparsePoint)!=0)throw new InvalidDataException("Linked requests are not accepted.");return folder;
  }
  internal static void ReadRequest(string request,out string hash,out int[] changes,out byte[][] images){
   string folder=CheckRequest(request);changes=new int[3];images=new byte[3][];
   using(BinaryReader b=new BinaryReader(new MemoryStream(ReadBounded(request,4096)))){if(b.ReadString()!=Magic)throw new InvalidDataException("Invalid request.");hash=b.ReadString();if(hash.Length!=64)throw new InvalidDataException("Invalid base hash.");for(int i=0;i<3;i++){changes[i]=b.ReadInt32();if(changes[i]<-1||changes[i]>1)throw new InvalidDataException("Invalid operation.");string digest=b.ReadString();if(changes[i]==1){string png=Path.Combine(folder,"slot"+(i+1)+".png");if((File.GetAttributes(png)&FileAttributes.ReparsePoint)!=0)throw new InvalidDataException("Linked PNG rejected.");images[i]=ReadBounded(png,40*1024*1024);if(Digest(images[i])!=digest)throw new InvalidDataException("Staged image changed.");ValidatePng(images[i]);}}if(b.BaseStream.Position!=b.BaseStream.Length)throw new InvalidDataException("Unexpected request data.");}
  }
 }
}

