using System;using System.IO;using System.Linq;using System.IO.Compression;using System.Collections.Generic;using System.Web.Script.Serialization;using EnhancedZune;
class MediaTests {
 static void Check(bool value,string text){if(!value)throw new Exception(text);Console.WriteLine("PASS "+text);}
 static void Reject(Action action,string text){bool failed=false;try{action();}catch(IOException){failed=true;}catch(InvalidDataException){failed=true;}Check(failed,text);}
 static int Main(string[] args){try{
  string root=Path.GetFullPath(args[0]);Directory.CreateDirectory(root);MediaPackage.TestRoot=Path.Combine(root,"staging");var manifest=new JavaScriptSerializer().Deserialize<Manifest>(File.ReadAllText(args[3]));
  string zipHash=Engine.FileHash(args[1]),exeHash=Engine.FileHash(args[2]);string staged;
  foreach(string input in new[]{args[1],args[2]}){
   using(PreparedMedia p=MediaPackage.Prepare(input,manifest,Console.WriteLine)){
    staged=p.Staging;var launch=MediaInstall.OriginalSetupStartInfo(p.SetupPath);Check(launch.FileName==p.SetupPath&&launch.Arguments=="-local"&&launch.WorkingDirectory==Path.GetDirectoryName(p.SetupPath)&&launch.UseShellExecute&&launch.Verb=="runas","original setup launch uses -local, x64 working directory and existing elevation");Check(File.Exists(p.SetupPath)&&Directory.GetFiles(Path.GetDirectoryName(p.SetupPath),"*",SearchOption.AllDirectories).Length==56,"complete verified x64 media from "+Path.GetExtension(input));
    Reject(delegate{using(var write=new FileStream(p.SetupPath,FileMode.Open,FileAccess.Write)){}},"verified setup stays locked against replacement while ready");
    string extra=Path.Combine(Path.GetDirectoryName(p.SetupPath),"unexpected.txt");File.WriteAllText(extra,"test");var pins=new List<FileStream>();try{Reject(delegate{MediaPackage.Verify(Path.GetDirectoryName(p.SetupPath),manifest,pins);},"unexpected extracted files rejected");}finally{foreach(var pin in pins)pin.Dispose();File.Delete(extra);}
   }
   Check(!Directory.Exists(staged),"temporary extraction removed after disposal");
  }
  using(PreparedMedia existing=MediaPackage.Prepare(args[4],manifest,delegate{}))Check(existing.Staging==null&&existing.SetupPath==Path.GetFullPath(args[4]),"previous complete-media selection still supported");
  string invalid=Path.Combine(root,"bad.exe");File.WriteAllText(invalid,"not an installer");Reject(delegate{using(var bad=MediaPackage.Prepare(invalid,manifest,delegate{})){}},"truncated or unrelated executable rejected");
  using(var sparse=new FileStream(invalid,FileMode.Create,FileAccess.Write)){sparse.SetLength(283639032);}Reject(delegate{using(var bad=MediaPackage.Prepare(invalid,manifest,delegate{})){}},"same-size altered package rejected by hash");
  string malicious=Path.Combine(root,"bad.zip");using(var zip=ZipFile.Open(malicious,ZipArchiveMode.Create)){using(var writer=new StreamWriter(zip.CreateEntry("../ZunePackage.exe").Open()))writer.Write("test");}
  Reject(delegate{using(var bad=MediaPackage.Prepare(malicious,manifest,delegate{})){}},"unexpected ZIP entries rejected before extraction");
  Check(!Directory.GetDirectories(MediaPackage.TestRoot).Any(),"failed imports leave no staging directories");
  Check(zipHash==Engine.FileHash(args[1])&&exeHash==Engine.FileHash(args[2]),"user ZIP and EXE unchanged");
  File.WriteAllText(Path.Combine(root,"results.json"),"{\"status\":\"passed\",\"zip_and_exe_extract_verified\":true,\"required_files\":56,\"tampered_downloads_rejected\":true,\"staging_cleanup_verified\":true,\"original_setup_executed\":false,\"installed_zune_modified\":false}");return 0;
 }catch(Exception e){Console.Error.WriteLine(e);return 1;}}
}
