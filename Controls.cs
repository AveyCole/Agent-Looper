using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LoopPedal {
static class Skin {
 public static void RoundFrame(Control control,int radius=10){Action update=()=>{if(control.IsDisposed||control.Width<2||control.Height<2)return;int d=Math.Min(radius*2,Math.Min(control.Width,control.Height));using(var path=new GraphicsPath()){path.AddArc(0,0,d,d,180,90);path.AddArc(control.Width-d,0,d,d,270,90);path.AddArc(control.Width-d,control.Height-d,d,d,0,90);path.AddArc(0,control.Height-d,d,d,90,90);path.CloseFigure();var old=control.Region;control.Region=new Region(path);if(old!=null)old.Dispose();}};control.SizeChanged+=(sender,args)=>update();update();}
 public static GraphicsPath Rounded(RectangleF r,float radius=8){var path=new GraphicsPath();float d=Math.Min(radius*2,Math.Min(r.Width,r.Height));path.AddArc(r.Left,r.Top,d,d,180,90);path.AddArc(r.Right-d,r.Top,d,d,270,90);path.AddArc(r.Right-d,r.Bottom-d,d,d,0,90);path.AddArc(r.Left,r.Bottom-d,d,d,90,90);path.CloseFigure();return path;}
 public static void Border(Graphics g,RectangleF rect,Color color,float radius=8){g.SmoothingMode=SmoothingMode.AntiAlias;using(var path=Rounded(rect,radius))using(var pen=new Pen(color))g.DrawPath(pen,path);}
 public static Color Ink=Color.FromArgb(12,15,19),Panel=Color.FromArgb(23,28,34),Line=Color.FromArgb(43,51,59),Muted=Color.FromArgb(135,148,159),Mint=Color.FromArgb(133,242,195),Red=Color.FromArgb(255,112,105),Gold=Color.FromArgb(244,199,118);
 public static Color[] Colors={Mint,Gold,Color.FromArgb(165,161,255),Red,Color.FromArgb(111,196,255)};
 public static void Text(Graphics g,string s,Font f,Color c,Rectangle r,ContentAlignment align){TextRenderer.DrawText(g,s,f,r,c,TextFormatFlags.NoPadding|TextFormatFlags.EndEllipsis|(align==ContentAlignment.MiddleCenter?TextFormatFlags.HorizontalCenter:TextFormatFlags.Left)|TextFormatFlags.VerticalCenter);}
}
class PadButton:Button {
 bool hover,down;public Color Accent=Skin.Muted;public bool Lit;public bool Pedal;
 public PadButton(){Skin.RoundFrame(this,8);SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);FlatStyle=FlatStyle.Flat;FlatAppearance.BorderSize=0;UseVisualStyleBackColor=false;Cursor=Cursors.Hand;BackColor=Skin.Panel;ForeColor=Color.WhiteSmoke;}
 protected override void OnMouseEnter(EventArgs e){hover=true;Invalidate();base.OnMouseEnter(e);}protected override void OnMouseLeave(EventArgs e){hover=false;Invalidate();base.OnMouseLeave(e);}protected override void OnMouseDown(MouseEventArgs e){down=true;Invalidate();base.OnMouseDown(e);}protected override void OnMouseUp(MouseEventArgs e){down=false;Invalidate();base.OnMouseUp(e);}
 protected override void OnGotFocus(EventArgs e){base.OnGotFocus(e);Invalidate();}protected override void OnLostFocus(EventArgs e){base.OnLostFocus(e);Invalidate();}
 protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.SmoothingMode=SmoothingMode.None;g.Clear(down?Color.FromArgb(48,57,65):hover?Color.FromArgb(37,45,53):BackColor);if(Lit)using(var brush=new SolidBrush(Color.FromArgb(24,Accent)))g.FillRectangle(brush,ClientRectangle);Skin.Border(g,new RectangleF(.5f,.5f,Width-1,Height-1),Skin.Line);if(Focused&&ShowFocusCues)Skin.Border(g,new RectangleF(4.5f,4.5f,Width-9,Height-9),Skin.Muted,5);if(Pedal){g.SmoothingMode=SmoothingMode.AntiAlias;using(var b=new SolidBrush(Lit?Accent:Color.FromArgb(55,64,71)))g.FillEllipse(b,18,18,7,7);using(var p=new Pen(Color.FromArgb(38,46,53)))for(int y=Height-22;y<Height-8;y+=4)g.DrawLine(p,18,y,Width-18,y);}Skin.Text(g,Text,Font,Lit||hover?Accent:ForeColor,new Rectangle(10,Pedal?15:0,Width-20,Height-(Pedal?35:0)),ContentAlignment.MiddleCenter);}
}
class Knob:Control {
 public int Min,Max,Value,Default; public string Caption;public Color Accent;public Func<int,string> Format;public Action<int> Commit;public bool Live;int startY,startValue;bool drag,dialDown;ScrollableControl scrollParent;int scrollStart,screenStart;
 public Knob(){Size=new Size(112,136);TabStop=true;Cursor=Cursors.Hand;SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);AccessibleRole=AccessibleRole.Slider;}
 protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.TranslateTransform((Width-112)/2,0);g.SmoothingMode=SmoothingMode.AntiAlias;float ratio=(float)(Value-Min)/(Max-Min);var r=new Rectangle(28,30,56,56);using(var pen=new Pen(Skin.Line,4)){pen.StartCap=pen.EndCap=LineCap.Round;g.DrawArc(pen,r,135,270);}using(var pen=new Pen(Accent,4)){pen.StartCap=pen.EndCap=LineCap.Round;g.DrawArc(pen,r,135,Math.Max(.1f,270*ratio));}using(var b=new LinearGradientBrush(new Rectangle(35,37,42,42),Color.FromArgb(58,66,74),Color.FromArgb(23,28,34),90))g.FillEllipse(b,35,37,42,42);double a=(135+270*ratio)*Math.PI/180;using(var p=new Pen(Accent,2))g.DrawLine(p,56+(float)Math.Cos(a)*9,58+(float)Math.Sin(a)*9,56+(float)Math.Cos(a)*17,58+(float)Math.Sin(a)*17);Skin.Text(g,Caption,Font,Skin.Muted,new Rectangle((Width-112)/2,2,112,22),ContentAlignment.MiddleCenter);using(var f=new Font("Consolas",11))Skin.Text(g,Format(Value),f,Color.WhiteSmoke,new Rectangle((Width-112)/2,95,112,23),ContentAlignment.MiddleCenter);}
 void Change(int v){int old=Value;Value=Math.Max(Min,Math.Min(Max,v));if(Live&&Value!=old&&Commit!=null)Commit(Value);AccessibleName=Caption+" "+Format(Value);Invalidate();}
 internal bool DialContains(int x,int y){int dx=x-Width/2,dy=y-58;return dx*dx+dy*dy<=30*30;}
 ScrollableControl FindScrollParent(){for(Control p=Parent;p!=null;p=p.Parent){var sc=p as ScrollableControl;if(sc!=null&&StudioScrolling.Enabled(sc))return sc;}return null;}
 protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);if(e.Button!=MouseButtons.Left)return;dialDown=DialContains(e.X,e.Y);if(!dialDown){scrollParent=FindScrollParent();if(scrollParent!=null){scrollStart=-StudioScrolling.Position(scrollParent).Y;screenStart=PointToScreen(e.Location).Y;Capture=true;}return;}Focus();drag=true;Capture=true;startY=e.Y;startValue=Value;}
 protected override void OnMouseMove(MouseEventArgs e){if(drag)Change(startValue+(startY-e.Y)*(Max-Min)/150);else if(scrollParent!=null){int y=scrollStart+screenStart-PointToScreen(e.Location).Y;StudioScrolling.Set(scrollParent,new Point(-StudioScrolling.Position(scrollParent).X,Math.Max(0,y)));}else Cursor=DialContains(e.X,e.Y)?Cursors.Hand:Cursors.Default;base.OnMouseMove(e);}
 protected override void OnMouseUp(MouseEventArgs e){if(drag){drag=false;Capture=false;if(!Live)Commit(Value);}if(scrollParent!=null){scrollParent=null;Capture=false;}base.OnMouseUp(e);}
 protected override void OnMouseWheel(MouseEventArgs e){if(!DialContains(e.X,e.Y)){var parent=FindScrollParent();if(parent!=null)StudioScrolling.Set(parent,new Point(-StudioScrolling.Position(parent).X,Math.Max(0,-StudioScrolling.Position(parent).Y-Math.Sign(e.Delta)*60)));return;}Change(Value+Math.Sign(e.Delta)*Math.Max(1,(Max-Min)/60));if(!Live)Commit(Value);base.OnMouseWheel(e);}
 protected override void OnDoubleClick(EventArgs e){if(dialDown){Change(Default);if(!Live)Commit(Value);}base.OnDoubleClick(e);}
 protected override bool IsInputKey(Keys k){return k==Keys.Left||k==Keys.Right||k==Keys.Up||k==Keys.Down||base.IsInputKey(k);}
 protected override void OnKeyDown(KeyEventArgs e){if(e.KeyCode==Keys.Left||e.KeyCode==Keys.Down){Change(Value-Math.Max(1,(Max-Min)/100));if(!Live)Commit(Value);e.Handled=true;}if(e.KeyCode==Keys.Right||e.KeyCode==Keys.Up){Change(Value+Math.Max(1,(Max-Min)/100));if(!Live)Commit(Value);e.Handled=true;}base.OnKeyDown(e);}
}
class Scope:Control {
 public Func<float[]> Samples;public Func<double> Phase;public Color Accent=Skin.Mint;public Func<float> Level;public bool Ring=false;float[] cachedAudio,cachedPeaks;int cachedBuckets;float visualGain=1;
 public Scope(){Skin.RoundFrame(this,10);SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);}
 protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;g.Clear(Skin.Ink);int w=Width,h=Height;using(var p=new Pen(Color.FromArgb(25,33,39)))for(int x=0;x<w;x+=32)g.DrawLine(p,x,0,x,h);double phase=Phase==null?0:Phase();var a=Samples==null?null:Samples();int left=Ring?180:12;if(Ring){int size=Math.Min(128,h-24);var r=new Rectangle(24,(h-size)/2,size,size);using(var p=new Pen(Skin.Line,3))g.DrawEllipse(p,r);using(var p=new Pen(Accent,5))g.DrawArc(p,r,-90,(float)Math.Max(.2,phase*360));using(var f=new Font("Consolas",17))Skin.Text(g,(phase*100).ToString("00")+"%",f,Accent,r,ContentAlignment.MiddleCenter);}
 int buckets=Math.Max(1,w-left-12);if(!object.ReferenceEquals(a,cachedAudio)||cachedPeaks==null||cachedBuckets!=buckets){cachedAudio=a;cachedBuckets=buckets;cachedPeaks=new float[buckets];if(a!=null){int frames=a.Length/2;for(int bucket=0;bucket<buckets;bucket++){int begin=(int)((long)bucket*frames/buckets),end=(int)((long)(bucket+1)*frames/buckets);for(int i=begin;i<end;i++)cachedPeaks[bucket]=Math.Max(cachedPeaks[bucket],Math.Max(Math.Abs(a[i*2]),Math.Abs(a[i*2+1])));}}float peak=0;foreach(var value in cachedPeaks)peak=Math.Max(peak,value);visualGain=peak>1e-9f?Math.Min(1000000,.9f/peak):1;}using(var pen=new Pen(Accent,1.4f)){for(int bucket=0;bucket<buckets;bucket++){int x=left+bucket;float size=Math.Max(1,Math.Min(1,cachedPeaks[bucket]*visualGain)*(h*.42f));g.DrawLine(pen,x,h/2-size,x,h/2+size);}}if(a!=null){int x=left+(int)(phase*Math.Max(1,w-left-12));using(var p=new Pen(Color.White,1))g.DrawLine(p,x,12,x,h-12);}if(Level!=null){float level=Level();for(int i=0;i<34;i++)using(var b=new SolidBrush(i/34f<level?(i>29?Skin.Red:Accent):Skin.Line))g.FillRectangle(b,w-12-i*7,h-12,4,4);}}
}
}







