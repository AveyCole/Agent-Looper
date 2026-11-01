using System;using System.Drawing;using System.Drawing.Drawing2D;using System.Windows.Forms;
namespace LoopPedal {
sealed class VerticalSwitch:Control {
 public Func<string[]> Labels;public Func<int> GetIndex;public Func<float> GetPosition;public Action<int> SetIndex;public string Caption;bool drag;ScrollableControl scrollParent;int screenStart,scrollStart;
 public VerticalSwitch(){Size=new Size(44,180);TabStop=true;SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);AccessibleRole=AccessibleRole.Slider;}
 int Count{get{return Labels().Length;}}
 protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;g.Clear(Skin.Panel);using(var font=new Font("Segoe UI",6.5f,FontStyle.Bold))Skin.Text(g,Caption,font,Skin.Muted,new Rectangle(0,0,Width,22),ContentAlignment.MiddleCenter);string[] labels=Labels();int selected=GetIndex();using(var line=new Pen(Skin.Line,3))g.DrawLine(line,8,32,8,152);for(int i=0;i<labels.Length;i++){float y=32+i*120f/(labels.Length-1);using(var dot=new SolidBrush(i==selected?Skin.Mint:Skin.Muted))g.FillEllipse(dot,6,y-2,4,4);using(var font=new Font("Consolas",6.8f))Skin.Text(g,labels[i],font,i==selected?Skin.Mint:Skin.Muted,new Rectangle(18,(int)y-12,Width-18,24),ContentAlignment.MiddleCenter);}float at=32+Math.Max(0,Math.Min(1,GetPosition()))*120;using(var fill=new LinearGradientBrush(new Rectangle(2,(int)at-6,12,12),Color.FromArgb(94,113,118),Color.FromArgb(36,45,52),90))g.FillRectangle(fill,2,at-6,12,12);using(var pen=new Pen(Skin.Mint,1))g.DrawRectangle(pen,2,at-6,12,12);using(var pen=new Pen(Color.WhiteSmoke,1))g.DrawLine(pen,5,at,11,at);}
 void Apply(int y){SetIndex(Math.Max(0,Math.Min(Count-1,(int)Math.Round((y-32)/120.0*(Count-1)))));Invalidate();}
 ScrollableControl FindScroll(){for(Control p=Parent;p!=null;p=p.Parent){var sc=p as ScrollableControl;if(sc!=null&&StudioScrolling.Enabled(sc))return sc;}return null;}
 protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);if(e.Button!=MouseButtons.Left)return;if(e.Y>=20&&e.Y<=166&&e.X>=2&&e.X<Width-2){Focus();drag=true;Capture=true;Apply(e.Y);}else{scrollParent=FindScroll();if(scrollParent!=null){screenStart=PointToScreen(e.Location).Y;scrollStart=-StudioScrolling.Position(scrollParent).Y;Capture=true;}}}
 protected override void OnMouseMove(MouseEventArgs e){if(drag)Apply(e.Y);else if(scrollParent!=null)StudioScrolling.Set(scrollParent,new Point(-StudioScrolling.Position(scrollParent).X,Math.Max(0,scrollStart+screenStart-PointToScreen(e.Location).Y)));base.OnMouseMove(e);}
 protected override void OnMouseUp(MouseEventArgs e){drag=false;scrollParent=null;Capture=false;base.OnMouseUp(e);}
 protected override void OnMouseWheel(MouseEventArgs e){int current=GetIndex();if(current<0)current=(int)Math.Round(GetPosition()*(Count-1));SetIndex(Math.Max(0,Math.Min(Count-1,current-Math.Sign(e.Delta))));Invalidate();}
 protected override bool IsInputKey(Keys key){return key==Keys.Up||key==Keys.Down||base.IsInputKey(key);}
 protected override void OnKeyDown(KeyEventArgs e){if(e.KeyCode==Keys.Up||e.KeyCode==Keys.Down){int current=GetIndex();if(current<0)current=(int)Math.Round(GetPosition()*(Count-1));SetIndex(Math.Max(0,Math.Min(Count-1,current+(e.KeyCode==Keys.Down?1:-1))));Invalidate();e.Handled=true;}base.OnKeyDown(e);}
}
}
