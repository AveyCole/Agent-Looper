using System;
using System.Drawing;
using System.Windows.Forms;
namespace LoopPedal {
static class StudioScrolling {
 public static bool Enabled(ScrollableControl c){return c is StudioFlowPanel||c.AutoScroll;}
 public static Point Position(ScrollableControl c){var flow=c as StudioFlowPanel;return flow!=null&&flow.Owner!=null?flow.Owner.ScrollPosition:c.AutoScrollPosition;}
 public static void Set(ScrollableControl c,Point p){var flow=c as StudioFlowPanel;if(flow!=null&&flow.Owner!=null)flow.Owner.ScrollTo(p.X,p.Y);else c.AutoScrollPosition=p;}
}
class StudioFlowPanel:FlowLayoutPanel {
 internal StudioScrollHost Owner;
 public new bool AutoScroll{get{return false;}set{base.AutoScroll=false;}}
 protected override void OnMouseWheel(MouseEventArgs e){if(Owner!=null){Owner.ScrollTo(-Owner.ScrollPosition.X,-Owner.ScrollPosition.Y-e.Delta/120*60);var handled=e as HandledMouseEventArgs;if(handled!=null)handled.Handled=true;}else base.OnMouseWheel(e);}
}
sealed class StudioScrollHost:Panel {
 public readonly FlowLayoutPanel Content;readonly Panel viewport=new Panel();readonly StudioScrollRail vertical=new StudioScrollRail(),horizontal=new StudioScrollRail{Horizontal=true};bool pending,arranging,shownV,shownH;int scrollX,scrollY;
 public Point ScrollPosition{get{return new Point(-scrollX,-scrollY);}}
 public StudioScrollHost(FlowLayoutPanel content){Content=content;Skin.RoundFrame(this,12);Dock=DockStyle.Fill;BackColor=content.BackColor==SystemColors.Control?Skin.Ink:content.BackColor;content.BackColor=BackColor;Margin=content.Margin;viewport.Dock=DockStyle.Fill;viewport.BackColor=BackColor;Controls.Add(viewport);vertical.Dock=DockStyle.Right;vertical.Width=14;horizontal.Dock=DockStyle.Bottom;horizontal.Height=14;vertical.Visible=horizontal.Visible=false;Controls.Add(vertical);Controls.Add(horizontal);viewport.Controls.Add(content);content.Dock=DockStyle.None;content.Margin=Padding.Empty;content.AutoScroll=false;content.AutoSize=true;content.AutoSizeMode=AutoSizeMode.GrowAndShrink;var flow=content as StudioFlowPanel;if(flow!=null)flow.Owner=this;
 vertical.Position=()=>scrollY;horizontal.Position=()=>scrollX;vertical.Maximum=()=>Math.Max(0,Content.Height-viewport.ClientSize.Height);horizontal.Maximum=()=>Math.Max(0,Content.Width-viewport.ClientSize.Width);vertical.Page=()=>viewport.ClientSize.Height;horizontal.Page=()=>viewport.ClientSize.Width;vertical.SetPosition=value=>ScrollTo(scrollX,value);horizontal.SetPosition=value=>ScrollTo(value,scrollY);
 viewport.SizeChanged+=(a,b)=>Arrange();content.Layout+=(a,b)=>Schedule();content.SizeChanged+=(a,b)=>Schedule();content.ControlAdded+=(a,b)=>{Wire(b.Control);Schedule();};content.ControlRemoved+=(a,b)=>Schedule();HandleCreated+=(a,b)=>Arrange();foreach(Control child in content.Controls)Wire(child);Arrange();
 }
 void Wire(Control control){control.Enter+=(a,b)=>EnsureVisible(control);control.ControlAdded+=(a,b)=>Wire(b.Control);foreach(Control child in control.Controls)Wire(child);}
 void EnsureVisible(Control c){if(!c.IsHandleCreated||!viewport.IsHandleCreated)return;var point=viewport.PointToClient(c.Parent.PointToScreen(c.Location));int x=scrollX,y=scrollY;if(point.Y<0)y+=point.Y;else if(point.Y+c.Height>viewport.ClientSize.Height)y+=point.Y+c.Height-viewport.ClientSize.Height;if(point.X<0)x+=point.X;else if(point.X+c.Width>viewport.ClientSize.Width)x+=point.X+c.Width-viewport.ClientSize.Width;ScrollTo(x,y);}
 void Arrange(){if(arranging)return;arranging=true;try{Content.MinimumSize=new Size(Math.Max(1,viewport.ClientSize.Width),0);Content.PerformLayout();ScrollTo(scrollX,scrollY);}finally{arranging=false;}Schedule();}
 void Schedule(){if(pending||!IsHandleCreated||IsDisposed)return;pending=true;BeginInvoke((Action)(()=>{pending=false;if(IsDisposed)return;bool v=vertical.Maximum()>0,h=horizontal.Maximum()>0;if(v!=shownV||h!=shownH){shownV=v;shownH=h;vertical.Visible=v;horizontal.Visible=h;PerformLayout();Arrange();}ScrollTo(scrollX,scrollY);}));}
 public void ScrollTo(int x,int y){scrollX=Math.Max(0,Math.Min(horizontal.Maximum(),x));scrollY=Math.Max(0,Math.Min(vertical.Maximum(),y));Content.Location=new Point(-scrollX,-scrollY);vertical.Invalidate();horizontal.Invalidate();}
 public StudioScrollRail VerticalRail{get{return vertical;}}
}
sealed class StudioScrollRail:Control {
 public bool Horizontal;public Func<int> Position,Maximum,Page;public Action<int> SetPosition;bool dragging,hover;int down,origin;
 public StudioScrollRail(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);TabStop=true;AccessibleRole=AccessibleRole.ScrollBar;BackColor=Skin.Panel;Cursor=Cursors.Hand;}
 int Max{get{return Maximum==null?0:Maximum();}}int Pos{get{return Position==null?0:Position();}}int Length{get{return Horizontal?ClientSize.Width:ClientSize.Height;}}
 internal Rectangle Thumb{get{int length=Math.Max(1,Length-6),page=Page==null?1:Math.Max(1,Page());int size=Math.Min(length,Math.Max(28,(int)((long)length*page/(Max+(long)page))));int offset=Max==0?0:(int)((long)(length-size)*Pos/Max);return Horizontal?new Rectangle(3+offset,3,size,Math.Max(1,Height-6)):new Rectangle(3,3+offset,Math.Max(1,Width-6),size);}}
 void ScrollTo(int value){if(SetPosition!=null)SetPosition(Math.Max(0,Math.Min(Max,value)));Invalidate();}
 protected override void OnPaint(PaintEventArgs e){e.Graphics.Clear(BackColor);var thumb=Thumb;using(var rail=new SolidBrush(Skin.Ink))e.Graphics.FillRectangle(rail,Horizontal?new Rectangle(3,5,Math.Max(1,Width-6),4):new Rectangle(5,3,4,Math.Max(1,Height-6)));using(var fill=new SolidBrush(dragging?Skin.Mint:hover?Color.FromArgb(125,165,154):Color.FromArgb(64,89,84)))e.Graphics.FillRectangle(fill,thumb);}
 protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);if(e.Button!=MouseButtons.Left)return;Focus();if(Thumb.Contains(e.Location)){dragging=true;down=Horizontal?e.X:e.Y;origin=Pos;Capture=true;}else ScrollTo(Pos+((Horizontal?e.X:e.Y)<(Horizontal?Thumb.Left:Thumb.Top)?-1:1)*(Page==null?80:Page()));}
 protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);if(dragging){int travel=Math.Max(1,Length-6-(Horizontal?Thumb.Width:Thumb.Height));ScrollTo(origin+(int)((long)((Horizontal?e.X:e.Y)-down)*Max/travel));}}
 protected override void OnMouseUp(MouseEventArgs e){dragging=false;Capture=false;Invalidate();base.OnMouseUp(e);}
 protected override void OnMouseEnter(EventArgs e){hover=true;Invalidate();base.OnMouseEnter(e);}protected override void OnMouseLeave(EventArgs e){hover=false;Invalidate();base.OnMouseLeave(e);}
 protected override void OnMouseWheel(MouseEventArgs e){ScrollTo(Pos-e.Delta/120*60);base.OnMouseWheel(e);}
 protected override bool IsInputKey(Keys keyData){return keyData==Keys.Up||keyData==Keys.Down||keyData==Keys.Left||keyData==Keys.Right||keyData==Keys.Home||keyData==Keys.End||keyData==Keys.PageUp||keyData==Keys.PageDown||base.IsInputKey(keyData);}
 protected override void OnKeyDown(KeyEventArgs e){if(e.KeyCode==Keys.Home)ScrollTo(0);else if(e.KeyCode==Keys.End)ScrollTo(Max);else if(e.KeyCode==Keys.Up||e.KeyCode==Keys.Left)ScrollTo(Pos-30);else if(e.KeyCode==Keys.Down||e.KeyCode==Keys.Right)ScrollTo(Pos+30);else if(e.KeyCode==Keys.PageUp)ScrollTo(Pos-Page());else if(e.KeyCode==Keys.PageDown)ScrollTo(Pos+Page());else{base.OnKeyDown(e);return;}e.Handled=true;}
}
}


