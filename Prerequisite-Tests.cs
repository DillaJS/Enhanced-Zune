using System;using System.IO;using System.Collections.Generic;using System.Drawing;using System.Windows.Forms;using System.Web.Script.Serialization;using EnhancedZune;
class PrerequisiteTests {
 static List<string> passed=new List<string>();
 static PrerequisiteState Good(int build=26200){return new PrerequisiteState{Build=build,Edition="Professional",Net35=true,PlayerRuntime=true,FormatRuntime=true,MediaFoundation=true};}
 static void Check(bool ok,string name){if(!ok)throw new Exception(name);passed.Add(name);Console.WriteLine("PASS "+name);}
 [STAThread]static int Main(string[] args){try{
  Check(Good().Ready,"Present runtimes accepted without requiring a player shortcut");
  var s=Good();s.Net35=false;Check(!s.Ready,"Missing .NET 3.5 stops installation even when setup itself runs on .NET 4");
  s=Good();s.PlayerRuntime=false;Check(!s.Ready,"Missing legacy player runtime stops installation");
  s=Good();s.FormatRuntime=false;Check(!s.Ready,"Missing media format runtime stops installation");
  s=Good();s.MediaFoundation=false;Check(!s.Ready,"Missing Media Foundation stops installation");
  s=Good();s.PlayerFeatureState=2;Check(!s.Ready,"Disabled player feature rejected even with residual runtime files");
  s=Good();s.PlayerFeatureState=3;Check(!s.Ready,"Absent player feature rejected");
  s=Good();s.PlayerFeatureState=1;Check(s.Ready,"Enabled player feature and runtimes accepted");
  s=Good();s.Error="Access denied";Check(!s.Ready,"Incomplete detection cannot falsely pass");
  Check(!Good(26200).StandaloneNet35&&Good(28000).StandaloneNet35&&Good(29000).StandaloneNet35,"Build 28000 standalone .NET boundary and later builds");
  Check(Good(19045).NetGuide.EndsWith("dotnet-35-windows")&&Good(22000).NetGuide.EndsWith("dotnet-35-windows-11"),"Windows 10 and 11 guide routing");
  s=Good();s.Edition="ProfessionalN";Check(s.NEdition&&s.MediaGuide.Contains("media-feature-pack-for-windows-n")&&s.Report().Contains("always restart"),"N edition Media Feature Pack instructions include required restart");
  s=Good();s.RestartPending=true;Check(s.Ready&&s.Report().Contains("pending restart"),"Unrelated pending Windows restart is advisory, not a false missing-prerequisite failure");
  Check(Good(28000).Report().Contains("standalone installer")&&!Good(26200).Report().Contains("standalone installer"),"No obsolete NetFx3 installation instruction on 26H1");
  s=Good(28000);s.Net35=false;Check(!s.CanAssist,"26H1 missing .NET alone directs users to the official standalone installer without a pointless elevated operation");s.PlayerRuntime=false;Check(s.CanAssist,"26H1 can still install missing media components");s.Error="Version unavailable";Check(!s.CanAssist,"Incomplete detection cannot start automatic servicing");var live=Prerequisites.Detect();Application.EnableVisualStyles();using(var f=new PrerequisiteForm()){f.ShowInTaskbar=false;f.Opacity=0;f.Show();Application.DoEvents();using(var b=new Bitmap(f.Width,f.Height)){f.DrawToBitmap(b,new Rectangle(0,0,b.Width,b.Height));b.Save(Path.Combine(args[0],"prerequisites-ui.png"));}var missing=Good();missing.Net35=false;missing.PlayerRuntime=false;f.PreviewState(missing);Application.DoEvents();using(var b=new Bitmap(f.Width,f.Height)){f.DrawToBitmap(b,new Rectangle(0,0,b.Width,b.Height));b.Save(Path.Combine(args[0],"prerequisites-missing-ui.png"));}f.Close();}
  File.WriteAllText(Path.Combine(args[0],"PREREQUISITE-RESULTS.json"),new JavaScriptSerializer().Serialize(new{status="passed",tests=passed,live_detection=live,live_ready=live.Ready,live_report=live.Report(),scope="Decision/routing tests and read-only detection on the reference PC. Missing-component Windows installations and Windows 11 26H1 are simulated decisions, not OS integration certification."}));return 0;
 }catch(Exception e){Console.Error.WriteLine(e);return 1;}}
}
