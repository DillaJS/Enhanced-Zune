using System;using System.Drawing;using System.Windows.Forms;using EnhancedZune;
class RenderSetup {
 [STAThread]static void Main(string[] args){Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);using(SetupForm f=new SetupForm(new string[0])){f.ShowInTaskbar=false;f.Opacity=0;f.Show();Application.DoEvents();using(Bitmap b=new Bitmap(f.Width,f.Height)){f.DrawToBitmap(b,new Rectangle(0,0,b.Width,b.Height));b.Save(args[0]);}f.Size=new Size(760,620);f.PerformLayout();Application.DoEvents();using(Bitmap b=new Bitmap(f.Width,f.Height)){f.DrawToBitmap(b,new Rectangle(0,0,b.Width,b.Height));b.Save(args[1]);}f.Close();}}
}
