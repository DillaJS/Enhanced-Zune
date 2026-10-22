using System;using System.IO;using System.IO.Compression;using System.Linq;using System.Text;using System.Collections.Generic;using System.Diagnostics;using System.ComponentModel;using System.Windows.Forms;
namespace EnhancedZune {
 public sealed class PreparedMedia:IDisposable {
  public string SetupPath;internal string Staging;internal List<FileStream> Pins=new List<FileStream>();
  public void Dispose(){foreach(FileStream p in Pins)p.Dispose();Pins.Clear();if(Staging!=null){MediaPackage.Cleanup(Staging);Staging=null;}}
 }
 public static class MediaPackage {
  public const string DownloadUrl="https://www.zuneupdate.com/resources/zune-software.zip";
  const long PackageLength=283639032,CabOffset=36352,CabLength=283595405;
  const string PackageHash="EF5E5DEBD76CA7DF2F7B6BF7095E1354EE4F9BAFDBBD29C96CB1CA87695FDDEE";
  internal static string TestRoot;
  static string Root{get{return TestRoot??Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"EnhancedZune","Installation files");}}
  internal static void Cleanup(string path){try{
   string root=Engine.Full(Root)+Path.DirectorySeparatorChar,full=Engine.Full(path);Guid id;
   if(!full.StartsWith(root,StringComparison.OrdinalIgnoreCase)||!Guid.TryParseExact(Path.GetFileName(full),"N",out id)||Path.GetDirectoryName(full)!=Engine.Full(Root))return;
   Engine.NoLinks(full);if(Directory.Exists(full))DeleteTree(full);
  }catch{/* Retain staging if Windows still has files open. */}}
  static void DeleteTree(string path){Engine.NoLinks(path);foreach(string child in Directory.GetDirectories(path)){Engine.NoLinks(child);DeleteTree(child);}foreach(string file in Directory.GetFiles(path)){Engine.NoLinks(file);File.Delete(file);}Directory.Delete(path);}
  static void CopyExact(Stream source,Stream output,long length){byte[] buffer=new byte[65536];while(length>0){int n=source.Read(buffer,0,(int)Math.Min(buffer.Length,length));if(n<=0)throw new InvalidDataException("The Zune download is incomplete. Download it again.");output.Write(buffer,0,n);length-=n;}}
  internal static void CheckCab(Stream stream){
   stream.Position=CabOffset;using(BinaryReader r=new BinaryReader(stream,Encoding.UTF8,true)){
    if(r.ReadUInt32()!=0x4643534d||r.ReadUInt32()!=0||r.ReadUInt32()!=CabLength)throw new InvalidDataException("Unrecognized Zune archive.");
    r.ReadUInt32();uint table=r.ReadUInt32();r.ReadUInt32();byte minor=r.ReadByte(),major=r.ReadByte();ushort folders=r.ReadUInt16(),files=r.ReadUInt16(),flags=r.ReadUInt16();
    if(major!=1||minor!=3||folders!=1||files!=168||flags!=0||table<36||table>=CabLength)throw new InvalidDataException("Unrecognized Zune archive layout.");
    stream.Position=CabOffset+table;HashSet<string> seen=new HashSet<string>(StringComparer.OrdinalIgnoreCase);long total=0;
    for(int i=0;i<files;i++){
     uint size=r.ReadUInt32();r.ReadUInt32();ushort folder=r.ReadUInt16();r.ReadUInt16();r.ReadUInt16();ushort attributes=r.ReadUInt16();List<byte> bytes=new List<byte>();byte c;
     while((c=r.ReadByte())!=0){if(bytes.Count>=240)throw new InvalidDataException("Archive path is too long.");bytes.Add(c);}
     string name=((attributes&128)!=0?Encoding.UTF8:Encoding.GetEncoding(1252)).GetString(bytes.ToArray());string[] parts=name.Replace('/','\\').Split('\\');
     if(folder!=0||String.IsNullOrEmpty(name)||Path.IsPathRooted(name)||name.Contains(':')||parts.Any(p=>p==".."||p=="."||p.Length==0||p.TrimEnd(' ','.')!=p)||!seen.Add(name)||stream.Position>CabOffset+CabLength)throw new InvalidDataException("Unsafe or duplicate installation archive path.");
     total+=size;if(total>400000000)throw new InvalidDataException("Installation archive is too large.");
    }
   }
  }
  public static PreparedMedia Prepare(string input,Manifest manifest,Action<string> progress){
   input=Engine.Full(input);Engine.NoLinks(input);PreparedMedia result=new PreparedMedia();
   try{string media;
    using(FileStream original=new FileStream(input,FileMode.Open,FileAccess.Read,FileShare.Read)){
     if(Path.GetFileName(input).Equals("zunesetup.exe",StringComparison.OrdinalIgnoreCase)&&original.Length<10000000){media=Path.GetDirectoryName(input);}
     else {
      Directory.CreateDirectory(Root);Engine.NoLinks(Root);result.Staging=Path.Combine(Root,Guid.NewGuid().ToString("N"));Directory.CreateDirectory(result.Staging);string package=input;
      if(Path.GetExtension(input).Equals(".zip",StringComparison.OrdinalIgnoreCase)){
       progress("Reading the Zune ZIP download…");using(ZipArchive zip=new ZipArchive(original,ZipArchiveMode.Read,true)){
        if(zip.Entries.Count!=1||zip.Entries[0].FullName!="ZunePackage.exe"||zip.Entries[0].Length!=PackageLength)throw new InvalidDataException("Choose the original zune-software.zip from the Download Zune button.");
        package=Path.Combine(result.Staging,"ZunePackage.exe");using(Stream source=zip.Entries[0].Open())using(FileStream dest=new FileStream(package,FileMode.CreateNew,FileAccess.Write)){CopyExact(source,dest,PackageLength);if(source.ReadByte()!=-1)throw new InvalidDataException("Oversized Zune package.");}
       }
      }
      using(FileStream source=new FileStream(package,FileMode.Open,FileAccess.Read,FileShare.Read)){
       progress("Verifying the original Microsoft Zune package…");if(source.Length!=PackageLength)throw new InvalidDataException("The Zune package is incomplete or unrecognized. Download it again using Download Zune.");
       using(var hash=System.Security.Cryptography.SHA256.Create())if(BitConverter.ToString(hash.ComputeHash(source)).Replace("-","")!=PackageHash)throw new InvalidDataException("The Zune package differs from the supported original release. Nothing was run.");
       CheckCab(source);string cab=Path.Combine(result.Staging,"installation.cab");source.Position=CabOffset;using(FileStream dest=new FileStream(cab,FileMode.CreateNew,FileAccess.Write)){CopyExact(source,dest,CabLength);}
       string extracted=Path.Combine(result.Staging,"Extracted");Directory.CreateDirectory(extracted);progress("Extracting Zune installation files—no manual extraction needed…");
       var start=new ProcessStartInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),"expand.exe"),"\""+cab+"\" -F:* \""+extracted+"\""){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};
       using(Process p=Process.Start(start)){var stdout=p.StandardOutput.ReadToEndAsync();var stderr=p.StandardError.ReadToEndAsync();if(!p.WaitForExit(120000)){p.Kill();p.WaitForExit();throw new IOException("Extracting Zune took too long. No installer was run. Please retry.");}System.Threading.Tasks.Task.WaitAll(stdout,stderr);if(p.ExitCode!=0)throw new IOException("Windows could not extract the Zune installation files. No installer was run.");}
       media=Path.Combine(extracted,"x64");
      }
     }
     progress("Checking all original installation files…");Verify(media,manifest,result.Pins);result.SetupPath=Path.Combine(media,"zunesetup.exe");return result;
    }
   }catch{result.Dispose();throw;}
  }
  internal static void Verify(string media,Manifest manifest,List<FileStream> pinned){
   Engine.NoLinks(media);HashSet<string> allowed=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
   foreach(MediaFile f in manifest.original_media){string file=Engine.Full(Path.Combine(media,f.name.Replace('/',Path.DirectorySeparatorChar)));if(!file.StartsWith(Engine.Full(media)+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))throw new InvalidDataException("Invalid original media manifest.");allowed.Add(file);Engine.NoLinks(file);FileStream s=new FileStream(file,FileMode.Open,FileAccess.Read,FileShare.Read);pinned.Add(s);if(s.Length!=f.size)throw new IOException("Original installation media is incomplete or unrecognized: "+f.name);using(var hash=System.Security.Cryptography.SHA256.Create())if(BitConverter.ToString(hash.ComputeHash(s)).Replace("-","")!=f.sha256)throw new IOException("Original installation media differs from the verified Microsoft release: "+f.name);}
   CheckTree(media,allowed);
  }
  static void CheckTree(string path,HashSet<string> allowed){Engine.NoLinks(path);foreach(string sub in Directory.GetDirectories(path)){Engine.NoLinks(sub);CheckTree(sub,allowed);}foreach(string file in Directory.GetFiles(path)){Engine.NoLinks(file);if(!allowed.Contains(Engine.Full(file)))throw new IOException("Unexpected file inside original installation media: "+Path.GetFileName(file));}}
 }
 public static class MediaInstall {
  internal static ProcessStartInfo OriginalSetupStartInfo(string setup){return new ProcessStartInfo(setup,"-local"){UseShellExecute=true,Verb="runas",WorkingDirectory=Path.GetDirectoryName(setup)};}
  public static void Run(Form parent,Action<bool> busy,Action<string> log,Action completed,Action<string> progress){
   using(OpenFileDialog d=new OpenFileDialog{Filter="Zune download or extracted setup|*.zip;*.exe|Zune ZIP download|*.zip|Zune package or setup|*.exe",Title="Choose zune-software.zip or ZunePackage.exe—no extraction needed"}){
    if(d.ShowDialog(parent)!=DialogResult.OK)return;if(File.Exists(Path.Combine(SetupForm.Detect(),"Zune.exe")))throw new IOException("Zune is already installed. Use Install / verify instead.");if(!Prerequisites.Ensure(parent))return;
    string input=d.FileName;PreparedMedia prepared=null;busy(true);BackgroundWorker extraction=new BackgroundWorker();
    extraction.DoWork+=delegate{using(Bundle bundle=new Bundle())prepared=MediaPackage.Prepare(input,bundle.M,progress);};
    extraction.RunWorkerCompleted+=delegate(object sender,RunWorkerCompletedEventArgs e){
     if(e.Error!=null){busy(false);log(e.Error.Message);return;}
     if(MessageBox.Show(parent,"The original Microsoft Zune installation files are verified and ready. Microsoft's setup will now open. After it finishes, this installer will apply the enhancements.\r\n\r\nWindows components may require Internet access or a restart.","Install Zune",MessageBoxButtons.OKCancel)!=DialogResult.OK){prepared.Dispose();busy(false);log("Installation canceled. No original setup was run.");return;}
     progress("Installing original Zune—complete Microsoft's setup, then return here…");BackgroundWorker install=new BackgroundWorker();int exit=-1;
     install.DoWork+=delegate{try{using(Process p=Process.Start(OriginalSetupStartInfo(prepared.SetupPath))){p.WaitForExit();exit=p.ExitCode;}}finally{prepared.Dispose();}};
     install.RunWorkerCompleted+=delegate(object s,RunWorkerCompletedEventArgs done){busy(false);if(done.Error!=null){log(done.Error.Message);return;}if(exit==3010){log("Microsoft setup requires a Windows restart. Restart, then run Enhanced Zune setup again.");return;}if(exit!=0){log("Microsoft setup returned "+exit+". Enhancements were not applied. Review Microsoft's message before retrying.");return;}completed();};install.RunWorkerAsync();
    };extraction.RunWorkerAsync();
   }
  }
 }
}
