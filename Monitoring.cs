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
partial class Pedal {
 MonitorButton monitorPad;
 void InstallMonitor(Panel head,Label name,Label clock){monitorPad=new MonitorButton{Size=new Size(36,30),Accent=Skin.Mint};monitorPad.Click+=(s,e)=>{loopers[0].SetInputMonitor(!loopers[0].InputMonitor);monitorPad.Lit=loopers[0].InputMonitor;monitorPad.Invalidate();status.Text=monitorPad.Lit?"Input monitoring on · use headphones; avoid doubling hardware direct monitoring":"Input monitoring off";};head.Controls.Add(monitorPad);monitorPad.BringToFront();head.Layout+=(s,e)=>monitorPad.Location=new Point(Math.Max(name.Right+8,clock.Left-44),2);}
}
}
