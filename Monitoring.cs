using System;using System.Drawing;using System.Drawing.Drawing2D;using System.Windows.Forms;
namespace LoopPedal {
sealed class InputMonitorBuffer {
 readonly float[] data=new float[16384];int read,count;float lastL,lastR,recovery;
 public void Clear(){read=count=0;lastL=lastR=recovery=0;}
 public void Write(float l,float r){if(count==8192){read=(read+1)%8192;count--;recovery=0;}int at=(read+count)%8192;data[at*2]=l;data[at*2+1]=r;count++;}
 public void Mix(float[] output,int frames){for(int i=0;i<frames;i++){if(count>0){lastL=data[read*2];lastR=data[read*2+1];read=(read+1)%8192;count--;recovery+=(1-recovery)*.02f;}else{lastL*=.98f;lastR*=.98f;recovery*=.98f;if(recovery<1e-7f)lastL=lastR=recovery=0;}output[i*2]+=lastL*recovery;output[i*2+1]+=lastR*recovery;}}
}
sealed partial class Engine {
 public volatile bool InputMonitor;public volatile float InputMonitorGain=1;
 public void SetInputMonitor(bool enabled){lock(Gate){InputMonitor=enabled;foreach(var input in Inputs)if(enabled)input.EnableMonitor();}}
}
sealed class MonitorButton:PadButton {
 public MonitorButton(){Text="";AccessibleName="Input monitoring";AccessibleDescription="Headphones: enable or disable live input monitoring";}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);DrawHeadphones(e.Graphics,new PointF(Width/2f,Height/2f),Lit?Skin.Mint:Skin.Muted);}
 internal static void DrawHeadphones(Graphics g,PointF center,Color color){g.SmoothingMode=SmoothingMode.AntiAlias;using(var pen=new Pen(color,2)){pen.StartCap=pen.EndCap=LineCap.Round;g.DrawArc(pen,center.X-9,center.Y-9,18,18,180,180);g.DrawLine(pen,center.X-9,center.Y,center.X-9,center.Y+6);g.DrawLine(pen,center.X+9,center.Y,center.X+9,center.Y+6);}using(var brush=new SolidBrush(color)){g.FillRectangle(brush,center.X-11,center.Y-1,5,9);g.FillRectangle(brush,center.X+6,center.Y-1,5,9);}}
}
sealed class RestartButton:PadButton {
 public RestartButton(){Text="";AccessibleName="Return to start";AccessibleDescription="Return this looper to the beginning without changing play or stop";}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);float x=Width/2f,y=Height/2f;using(var b=new SolidBrush(Accent)){e.Graphics.FillRectangle(b,x-8,y-6,2,12);e.Graphics.FillPolygon(b,new[]{new PointF(x+6,y-6),new PointF(x-4,y),new PointF(x+6,y+6)});}}
}
partial class Pedal {
 MonitorButton monitorPad;
 void InstallRestart(int bank,Panel head,Label name,Label clock){var button=new RestartButton{Name="Restart"+bank,Size=new Size(34,30),Accent=bank==0?Skin.Mint:Skin.Gold};button.Click+=(s,e)=>{try{SelectBank(bank);loopers[bank].Restart();RefreshTracks(true);}catch(Exception ex){status.Text=ex.Message;}};RegisterMidi((bank==0?"A.":"B.")+"Restart",button);head.Controls.Add(button);button.BringToFront();head.Layout+=(s,e)=>button.Location=new Point(Math.Max(name.Right+8,clock.Left-42),2);}

 void InstallMonitor(Panel head,Label name,Label clock){monitorPad=new MonitorButton{Size=new Size(36,30),Accent=Skin.Mint};monitorPad.Click+=(s,e)=>{loopers[0].SetInputMonitor(!loopers[0].InputMonitor);monitorPad.Lit=loopers[0].InputMonitor;monitorPad.Invalidate();status.Text=monitorPad.Lit?"Input monitoring on · use headphones; avoid doubling hardware direct monitoring":"Input monitoring off";};head.Controls.Add(monitorPad);monitorPad.BringToFront();head.Layout+=(s,e)=>monitorPad.Location=new Point(Math.Max(name.Right+8,clock.Left-84),2);}
}
}
