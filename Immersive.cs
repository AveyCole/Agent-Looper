using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
namespace LoopPedal {
partial class Pedal {
 bool immersive;
 Rectangle windowedBounds;
 FormWindowState windowedState;
 FormBorderStyle windowedBorder;
 Size windowedMinimum,windowedMaximum;
 PadButton exitImmersive;
 void InstallImmersiveControls(Panel header){
  exitImmersive=(PadButton)Btn("EXIT FULLSCREEN · ESC",()=>SetImmersive(false));
  exitImmersive.Name="ExitFullscreen";exitImmersive.AccessibleName="Exit fullscreen";
  exitImmersive.Size=new Size(180,34);exitImmersive.Accent=Skin.Mint;exitImmersive.Visible=false;
  header.Controls.Add(exitImmersive);Action arrange=()=>exitImmersive.Location=new Point(Math.Max(0,header.ClientSize.Width-exitImmersive.Width),4);
  header.SizeChanged+=(s,e)=>arrange();arrange();KeyPreview=true;
 }
 void SetImmersive(bool enabled){
  if(enabled==immersive)return;
  if(enabled){
   var screen=Screen.FromControl(this);windowedState=WindowState;windowedBounds=WindowState==FormWindowState.Normal?Bounds:RestoreBounds;
   windowedBorder=FormBorderStyle;windowedMinimum=MinimumSize;windowedMaximum=MaximumSize;
   SuspendLayout();try{immersive=true;WindowState=FormWindowState.Normal;MinimumSize=Size.Empty;MaximumSize=Size.Empty;FormBorderStyle=FormBorderStyle.None;Bounds=screen.Bounds;exitImmersive.Visible=true;exitImmersive.BringToFront();}finally{ResumeLayout(true);}
  }else{
   SuspendLayout();try{immersive=false;exitImmersive.Visible=false;WindowState=FormWindowState.Normal;FormBorderStyle=windowedBorder;
    var bounds=windowedBounds;if(!Screen.AllScreens.Any(s=>s.WorkingArea.IntersectsWith(bounds))){var area=Screen.FromControl(this).WorkingArea;bounds.Location=new Point(area.Left+Math.Max(0,(area.Width-bounds.Width)/2),area.Top+Math.Max(0,(area.Height-bounds.Height)/2));}
    Bounds=bounds;MinimumSize=windowedMinimum;MaximumSize=windowedMaximum;WindowState=windowedState==FormWindowState.Minimized?FormWindowState.Normal:windowedState;
   }finally{ResumeLayout(true);}
  }
 }
 protected override bool ProcessCmdKey(ref Message message,Keys keyData){
  if(keyData==Keys.F11){if(currentPopup!=null)currentPopup.Close();SetImmersive(!immersive);return true;}
  if(keyData==Keys.Escape&&immersive){if(currentPopup!=null)currentPopup.Close();SetImmersive(false);return true;}
  return base.ProcessCmdKey(ref message,keyData);
 }
 public void VerifyImmersive(){
  var normal=Bounds;var border=FormBorderStyle;var minimum=MinimumSize;int count=loopers.Sum(e=>e.Layers.Count);bool playing=loopers[0].Playing;var message=new Message();
  ProcessCmdKey(ref message,Keys.F11);Application.DoEvents();
  if(!immersive||FormBorderStyle!=FormBorderStyle.None||Bounds!=Screen.FromControl(this).Bounds||!exitImmersive.Visible)throw new Exception("Immersive screen bounds / exit button");
  using(var image=new Bitmap(ClientSize.Width,ClientSize.Height)){DrawToBitmap(image,new Rectangle(Point.Empty,ClientSize));image.Save(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"fullscreen-preview.png"));}
  recPads[0].Focus();if(!ProcessCmdKey(ref message,Keys.Escape))throw new Exception("Escape not handled");Application.DoEvents();
  if(immersive||Bounds!=normal||FormBorderStyle!=border||MinimumSize!=minimum||exitImmersive.Visible)throw new Exception("Windowed restore after Escape");
  SetImmersive(true);exitImmersive.PerformClick();Application.DoEvents();if(immersive||Bounds!=normal)throw new Exception("Visible fullscreen exit");
  WindowState=FormWindowState.Maximized;Application.DoEvents();SetImmersive(true);SetImmersive(false);Application.DoEvents();if(WindowState!=FormWindowState.Maximized)throw new Exception("Maximized state restore");
  WindowState=FormWindowState.Normal;Bounds=normal;Application.DoEvents();
  if(loopers.Sum(e=>e.Layers.Count)!=count||loopers[0].Playing!=playing)throw new Exception("Fullscreen changed audio state");
 }
}
}
