using System.ComponentModel;
using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Management;
using Microsoft.Win32;

namespace EnhancedZune {
 public sealed class PrerequisiteState {
  public int Build;
  public string Edition="", Error="";
  public bool Net35, PlayerRuntime, FormatRuntime, MediaFoundation, RestartPending;
  public int? PlayerFeatureState;
  public string FeatureNote="";
  public bool Ready {get{return Error.Length==0&&Net35&&PlayerRuntime&&FormatRuntime&&MediaFoundation&&PlayerFeatureState!=2&&PlayerFeatureState!=3;}}
  public bool CanAssist {get{return Error.Length==0&&((!Net35&&!StandaloneNet35)||!PlayerRuntime||!FormatRuntime||!MediaFoundation||PlayerFeatureState==2||PlayerFeatureState==3);}}
  public bool Windows11 {get{return Build>=22000;}}
  public bool StandaloneNet35 {get{return Build>=28000;}}
  public bool NEdition {get{return Edition.EndsWith("N",StringComparison.OrdinalIgnoreCase);}}
  public string NetGuide {get{return Windows11?"https://learn.microsoft.com/en-us/dotnet/framework/install/dotnet-35-windows-11":"https://learn.microsoft.com/en-us/dotnet/framework/install/dotnet-35-windows";}}
  public string MediaGuide {get{return NEdition?"https://support.microsoft.com/en-us/windows/experience/platform-variants/media-feature-pack-for-windows-n":"https://support.microsoft.com/en-us/windows/apps/windowsmediaplayer/troubleshoot-windows-media-player-errors";}}
  static string Found(bool value){return value?"Detected":"Missing";}
  public string Report(){StringBuilder s=new StringBuilder();
   s.AppendLine((Windows11?"Windows 11":"Windows 10")+" — "+Edition+", build "+Build);s.AppendLine();
   s.AppendLine(".NET Framework 3.5: "+Found(Net35));
   s.AppendLine("Windows Media Player Legacy runtime: "+Found(PlayerRuntime));
   s.AppendLine("Windows Media Format runtime: "+Found(FormatRuntime));
   s.AppendLine("Windows Media Foundation: "+Found(MediaFoundation));s.AppendLine();
   s.AppendLine("Windows Media Player feature: "+(PlayerFeatureState==1?"Enabled":PlayerFeatureState==2?"Disabled":PlayerFeatureState==3?"Absent":"Not reported; runtime files checked instead"));
   if(FeatureNote.Length>0)s.AppendLine(FeatureNote);
   s.AppendLine();
   s.AppendLine(StandaloneNet35?"For this Windows version, install .NET Framework 3.5 using Microsoft's version-specific standalone installer. NetFx3 is no longer a Windows optional feature.":"If .NET Framework 3.5 is missing, enable .NET Framework 3.5 (includes .NET 2.0 and 3.0) in Windows Features. Windows Update or matching Windows installation media may be required.");s.AppendLine();
   s.AppendLine("If media components are missing, add Windows Media Player Legacy through Windows Optional Features. The newer Microsoft Store Media Player app does not replace these legacy components.");
   if(NEdition)s.AppendLine("This is a Windows N edition. Install Microsoft's Media Feature Pack, then restart Windows even if Windows does not ask you to. Recheck Windows Media Player Legacy afterward.");
   s.AppendLine();s.AppendLine(RestartPending?"Windows reports a pending restart. Restart before retrying installation; this may relate to another Windows update.":"Restart Windows if a component installation requests it. After installing the Media Feature Pack, always restart.");
   if(Error.Length>0){s.AppendLine();s.AppendLine("Some checks could not finish: "+Error);}
   s.AppendLine();s.AppendLine("Use Install missing components to let Windows add the supported optional components. This can require administrator approval, Internet access and a restart. These checks identify common missing prerequisites. If installation or playback still fails, use the Microsoft troubleshooting guide.");
   s.AppendLine();s.AppendLine(Ready?"Required component checks passed. Close this window and retry setup.":"Install the missing components using the Microsoft guides, then choose Recheck. Setup has not started Microsoft's original installer.");return s.ToString();
  }
 }
 public static class Prerequisites {
  public static bool RestartRequiredBySetup=false;
  public static PrerequisiteState Detect(){PrerequisiteState s=new PrerequisiteState();List<string> errors=new List<string>();
   try{using(RegistryKey lm=RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,RegistryView.Registry64)){
    using(RegistryKey os=lm.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion")){if(os==null||!Int32.TryParse(Convert.ToString(os.GetValue("CurrentBuildNumber","")),out s.Build))throw new IOException("Windows version could not be read");s.Edition=Convert.ToString(os.GetValue("EditionID",""));}
    using(RegistryKey net=lm.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5")){s.Net35=net!=null&&Convert.ToInt32(net.GetValue("Install",0))==1;}
    foreach(string key in new[]{@"SOFTWARE\Microsoft\Windows\CurrentVersion\Component Based Servicing\RebootPending",@"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired"})using(RegistryKey pending=lm.OpenSubKey(key)){if(pending!=null)s.RestartPending=true;}
   }}catch(Exception e){errors.Add(e.Message);}
   string system=Environment.GetFolderPath(Environment.SpecialFolder.System);
   s.PlayerRuntime=NativeFile(Path.Combine(system,"wmp.dll"),errors);
   s.FormatRuntime=NativeFile(Path.Combine(system,"wmvcore.dll"),errors);
   s.MediaFoundation=NativeFile(Path.Combine(system,"mfplat.dll"),errors);
   try{EnumerationOptions options=new EnumerationOptions{Timeout=TimeSpan.FromSeconds(3),ReturnImmediately=true,Rewindable=false};using(ManagementObjectSearcher query=new ManagementObjectSearcher("root\\CIMV2","SELECT InstallState FROM Win32_OptionalFeature WHERE Name='WindowsMediaPlayer'",options))using(ManagementObjectCollection rows=query.Get()){foreach(ManagementObject row in rows)using(row){s.PlayerFeatureState=Convert.ToInt32(row["InstallState"]);}}}catch(Exception){s.FeatureNote="Windows did not provide the optional-feature state. Runtime file detection is still available.";}
   s.Error=String.Join("; ",errors);return s;
  }
  // Inspect the native x64 runtime, not a player shortcut or a Store app.
  static bool NativeFile(string path,List<string> errors){try{if(!File.Exists(path))return false;using(BinaryReader r=new BinaryReader(File.Open(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete))){if(r.ReadUInt16()!=0x5a4d)return false;r.BaseStream.Position=0x3c;int pe=r.ReadInt32();if(pe<0||pe>r.BaseStream.Length-6)return false;r.BaseStream.Position=pe;return r.ReadUInt32()==0x4550&&r.ReadUInt16()==0x8664;}}catch(Exception e){errors.Add(Path.GetFileName(path)+": "+e.Message);return false;}}
  public static bool Ensure(Form owner){PrerequisiteState s=Detect();if(s.Ready&&!RestartRequiredBySetup)return true;using(PrerequisiteForm f=new PrerequisiteForm())f.ShowDialog(owner);return !RestartRequiredBySetup&&Detect().Ready;}
 }
 public sealed class PrerequisiteForm:Form {
  readonly TextBox report=new TextBox();readonly Label summary=new Label();readonly FlowLayoutPanel actions=new FlowLayoutPanel();readonly Button install=new Button();PrerequisiteState state;bool busy,preview;
  public PrerequisiteForm(){Text="Zune prerequisites";Font=new Font("Segoe UI",11);AutoScaleMode=AutoScaleMode.Dpi;Size=new Size(880,760);MinimumSize=new Size(740,560);StartPosition=FormStartPosition.CenterParent;
   TableLayoutPanel layout=new TableLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(22),ColumnCount=1,RowCount=4};layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));Controls.Add(layout);
   layout.Controls.Add(new Label{Text="Required Windows components",Font=new Font("Segoe UI",19),AutoSize=true,Margin=new Padding(0,0,0,12)});
   summary.AutoSize=true;summary.MaximumSize=new Size(780,0);summary.Font=new Font("Segoe UI",14,FontStyle.Bold);summary.Padding=new Padding(12);summary.Margin=new Padding(0,0,0,12);layout.Controls.Add(summary);
   report.Multiline=true;report.ReadOnly=true;report.ScrollBars=ScrollBars.Vertical;report.Dock=DockStyle.Fill;report.BackColor=Color.White;layout.Controls.Add(report);
   actions.Dock=DockStyle.Fill;actions.AutoSize=true;actions.WrapContents=true;actions.Padding=new Padding(0,12,0,0);layout.Controls.Add(actions);
   install.Text="Install missing components…";install.AutoSize=true;install.BackColor=Color.FromArgb(205,0,120);install.ForeColor=Color.White;install.FlatStyle=FlatStyle.Flat;install.Margin=new Padding(0,0,10,8);install.Click+=delegate{InstallMissing();};actions.Controls.Add(install);
   Add(".NET instructions",delegate{Open(state.NetGuide);});Add("Media instructions",delegate{Open(state.MediaGuide);});Add("Recheck",RefreshChecks);Add("Close",Close);
   FormClosing+=delegate(object sender,FormClosingEventArgs e){if(busy){e.Cancel=true;MessageBox.Show(this,"Please let Windows finish installing components before closing setup.");}};RefreshChecks();
  }
  void Status(string text,bool success){summary.Text=text;summary.ForeColor=success?Color.FromArgb(20,105,45):Color.FromArgb(130,70,0);summary.BackColor=success?Color.FromArgb(229,245,233):Color.FromArgb(255,243,218);}
  void RefreshChecks(){state=Prerequisites.Detect();ShowState();}
  internal void PreviewState(PrerequisiteState sample){preview=true;state=sample;ShowState();}
  void ShowState(){report.Text=state.Report();if(Prerequisites.RestartRequiredBySetup)report.Text="Restart Windows before continuing, then open setup again.\r\n\r\n"+report.Text;report.SelectionStart=0;report.SelectionLength=0;report.ScrollToCaret();install.Enabled=state.CanAssist&&!Prerequisites.RestartRequiredBySetup;install.BackColor=install.Enabled?Color.FromArgb(205,0,120):SystemColors.Control;
   Status(Prerequisites.RestartRequiredBySetup?"Restart Windows to continue":state.Ready?"Required components are ready":"Windows components need attention",state.Ready&&!Prerequisites.RestartRequiredBySetup);
  }
  void InstallMissing(){if(preview)return;RefreshChecks();if(!install.Enabled)return;
   string note="Windows will install missing supported optional components for .NET Framework 3.5 and Windows Media Player Legacy. On Windows N, this includes the Media Feature Pack.\r\n\r\nThis can require administrator approval, Internet access and several minutes. Windows Update settings will stay unchanged. Setup will not restart your PC automatically.";
   if(state.StandaloneNet35&&!state.Net35)note+="\r\n\r\n.NET Framework 3.5 needs Microsoft's separate version-specific installer on this Windows version. This button handles media components; use .NET instructions for that remaining step.";
   if(MessageBox.Show(this,note,"Install missing Windows components?",MessageBoxButtons.OKCancel,MessageBoxIcon.Information)!=DialogResult.OK)return;
   busy=true;actions.Enabled=false;Status("Installing — approve the Windows prompt and keep setup open",false);BackgroundWorker w=new BackgroundWorker();int exit=-1;
   w.DoWork+=delegate{using(Process p=Process.Start(new ProcessStartInfo(System.Reflection.Assembly.GetExecutingAssembly().Location,"--windows-components"){UseShellExecute=true,Verb="runas"})){p.WaitForExit();exit=p.ExitCode;}};
   w.RunWorkerCompleted+=delegate(object sender,RunWorkerCompletedEventArgs e){busy=false;actions.Enabled=true;if(exit==3010)Prerequisites.RestartRequiredBySetup=true;RefreshChecks();if(exit!=0&&state.RestartPending){Prerequisites.RestartRequiredBySetup=true;RefreshChecks();}if(e.Error!=null){Status("Windows component installation did not start",false);MessageBox.Show(this,e.Error.Message,"Windows components");return;}if(exit!=0&&exit!=3010){Status("Windows components could not be completed — use the Microsoft instructions",false);return;}if(exit==0&&!state.Ready)Status(state.StandaloneNet35&&!state.Net35?"Complete .NET installation using .NET instructions, then Recheck":"Some components still need attention — use the Microsoft instructions",false);};w.RunWorkerAsync();
  }
  void Open(string url){try{Process.Start(new ProcessStartInfo(url){UseShellExecute=true});}catch(Exception e){MessageBox.Show(this,"Could not open the Microsoft guide.\r\n"+url+"\r\n\r\n"+e.Message);}}
  void Add(string text,Action action){Button b=new Button{Text=text,AutoSize=true,Margin=new Padding(0,0,10,8)};b.Click+=delegate{action();};actions.Controls.Add(b);}
 }
}


