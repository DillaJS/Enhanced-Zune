using System;using System.IO;using System.Text;using System.Reflection;using System.Diagnostics;using System.Security.Principal;using System.Windows.Forms;using System.ComponentModel;using System.Drawing;
namespace EnhancedZune {
 public static class WindowsComponents {
  public static bool Elevated {get{return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);}}
  public static string Script {get{using(Stream s=Assembly.GetExecutingAssembly().GetManifestResourceStream("WindowsComponents.ps1")){if(s==null)throw new IOException("Windows component helper is missing.");using(StreamReader r=new StreamReader(s))return r.ReadToEnd();}}}
  public static int Run(Action<string> log){using(var mutex=new System.Threading.Mutex(false,@"Global\EnhancedZune.Setup")){if(!mutex.WaitOne(0))throw new IOException("Another Enhanced Zune operation is running. Let it finish before installing Windows components.");try{return RunCore(log);}finally{mutex.ReleaseMutex();}}}
  static int RunCore(Action<string> log){if(!Elevated)throw new IOException("Administrator approval is required to add Windows components.");string shell=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),@"WindowsPowerShell\v1.0\powershell.exe");
   ProcessStartInfo start=new ProcessStartInfo(shell,"-NoLogo -NoProfile -NonInteractive -EncodedCommand "+Convert.ToBase64String(Encoding.Unicode.GetBytes(Script))){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};
   using(Process process=new Process{StartInfo=start}){process.OutputDataReceived+=delegate(object sender,DataReceivedEventArgs e){if(e.Data!=null)log(e.Data);};process.ErrorDataReceived+=delegate(object sender,DataReceivedEventArgs e){if(e.Data!=null)log(e.Data);};process.Start();process.BeginOutputReadLine();process.BeginErrorReadLine();process.WaitForExit();return process.ExitCode;}
  }
 }
 public sealed class WindowsComponentForm:Form {
  readonly Label title=new Label();readonly TextBox log=new TextBox();readonly Button close=new Button();bool running=true;public int Code=1;
  public WindowsComponentForm(bool preview=false){Text="Install Windows components";Font=new Font("Segoe UI",11);AutoScaleMode=AutoScaleMode.Dpi;Size=new Size(800,600);MinimumSize=new Size(660,440);StartPosition=FormStartPosition.CenterScreen;
   TableLayoutPanel layout=new TableLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(24),ColumnCount=1,RowCount=4};layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));Controls.Add(layout);
   title.Text="Installing Windows components…";title.Font=new Font("Segoe UI",18,FontStyle.Bold);title.ForeColor=Color.FromArgb(0,85,145);title.AutoSize=true;layout.Controls.Add(title);
   var instruction=new Label{Text="Keep this window open. Windows may need several minutes to download and install components.",AutoSize=true,MaximumSize=new Size(700,0),Margin=new Padding(0,12,0,16)};layout.Controls.Add(instruction);title.MaximumSize=new Size(700,0);Resize+=delegate{title.MaximumSize=new Size(Math.Max(300,ClientSize.Width-64),0);instruction.MaximumSize=title.MaximumSize;};if(preview)running=false;
   log.Multiline=true;log.ReadOnly=true;log.ScrollBars=ScrollBars.Vertical;log.Dock=DockStyle.Fill;layout.Controls.Add(log);close.Text="Close";close.AutoSize=true;close.Enabled=false;close.Margin=new Padding(0,16,0,0);close.Click+=delegate{Close();};layout.Controls.Add(close);
   FormClosing+=delegate(object sender,FormClosingEventArgs e){if(running){e.Cancel=true;MessageBox.Show(this,"Windows is still servicing components. Please let it finish; setup will not interrupt this operation.");}};
   Shown+=delegate{if(preview)return;BackgroundWorker worker=new BackgroundWorker();int result=1;worker.DoWork+=delegate{result=WindowsComponents.Run(Append);};worker.RunWorkerCompleted+=delegate(object sender,RunWorkerCompletedEventArgs e){running=false;Code=e.Error==null?result:1;if(e.Error!=null)Append(e.Error.Message);if(Code==0){Close();return;}title.Text=Code==3010?"Restart Windows to continue":"Windows components need attention";title.ForeColor=Code==3010?Color.FromArgb(145,80,0):Color.FromArgb(170,25,25);close.Enabled=true;};worker.RunWorkerAsync();};
  }
  void Append(string value){if(InvokeRequired){BeginInvoke(new Action<string>(Append),value);return;}log.AppendText(value+Environment.NewLine);}
 }
 public static class CompletionPolicy {
  public static bool MayLaunch(bool success,string operation,bool restartRequired,bool elevated){return success&&(operation=="install"||operation=="repair")&&!restartRequired&&!elevated;}
 }
}
