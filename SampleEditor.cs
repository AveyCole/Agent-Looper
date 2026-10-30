using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LoopPedal {
class SampleEditor:Scope {
 public Func<double> GetStart,GetLength,GetStretch;
 public Func<bool> GetReverse;
 public Action<double,double,bool> ChangeRegion;
 float[] peakSource,peaks,minima,maxima;float visualGain=1;int peakWidth;double peakSpan,peakOrigin;bool manualView;double manualSpan,manualOrigin,dragOrigin;
 int dragging,keyboardHandle=2,mouseOrigin;double initialStart,initialLength,dragSpan;
 double StartValue{get{return GetStart==null?0:GetStart();}}
 double LengthValue{get{return GetLength==null?1:GetLength();}}
 double StretchValue{get{return GetStretch==null?1:GetStretch();}}
 double Duration{get{var samples=Samples==null?null:Samples();return Math.Max(1.0/DSP.Rate,samples==null?1:samples.Length/(2.0*DSP.Rate));}}
 double FitSpan{get{return Math.Max(1,StartValue+LengthValue)*1.12;}}public double ViewSpan{get{return dragging!=0?dragSpan:manualView?Math.Min(manualSpan,FitSpan):FitSpan;}}public double ViewOrigin{get{return dragging!=0?dragOrigin:manualView?Math.Min(manualOrigin,Math.Max(0,FitSpan-ViewSpan)):0;}}
 int PlotWidth{get{return Math.Max(1,Width-24);}}
 public int HandlePixel(bool end){return 12+(int)Math.Round((StartValue+(end?LengthValue:0)-ViewOrigin)/ViewSpan*PlotWidth);}
 public SampleEditor(){TabStop=true;AccessibleRole=AccessibleRole.Graphic;AccessibleName="Loop start and end handles";Cursor=Cursors.Default;}
 int Hit(int x){int left=Math.Abs(x-HandlePixel(false)),right=Math.Abs(x-HandlePixel(true));if(Math.Min(left,right)>12)return 0;return left<right?1:2;}
 protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;g.Clear(Skin.Ink);double span=ViewSpan,origin=ViewOrigin,start=StartValue,end=start+LengthValue;var source=Samples==null?null:Samples();int buckets=Math.Max(1,PlotWidth);
  if(!object.ReferenceEquals(source,peakSource)||peaks==null||peakWidth!=buckets||Math.Abs(peakSpan-span)>1e-12||Math.Abs(peakOrigin-origin)>1e-12){peakSource=source;peakWidth=buckets;peakSpan=span;peakOrigin=origin;peaks=new float[buckets];minima=new float[buckets];maxima=new float[buckets];float peak=0;if(source!=null){int frames=source.Length/2;for(int bucket=0;bucket<buckets;bucket++){int first=Math.Min(frames,(int)((origin+bucket*span/buckets)*frames)),last=Math.Min(frames,(int)((origin+(bucket+1)*span/buckets)*frames));for(int i=first;i<last;i++){minima[bucket]=Math.Min(minima[bucket],Math.Min(source[i*2],source[i*2+1]));maxima[bucket]=Math.Max(maxima[bucket],Math.Max(source[i*2],source[i*2+1]));peaks[bucket]=Math.Max(Math.Abs(minima[bucket]),Math.Abs(maxima[bucket]));peak=Math.Max(peak,peaks[bucket]);}}}visualGain=peak>1e-9f?Math.Min(1000000,.9f/peak):1;}
  int x1=HandlePixel(false),x2=HandlePixel(true);using(var b=new SolidBrush(Color.FromArgb(22,Accent)))g.FillRectangle(b,x1,0,Math.Max(1,x2-x1),Height);using(var p=new Pen(Skin.Line))for(int x=12;x<Width-12;x+=32)g.DrawLine(p,x,0,x,Height);
  using(var selected=new Pen(Accent,1.4f))using(var outside=new Pen(Color.FromArgb(65,80,87),1.2f)){for(int bucket=0;bucket<buckets;bucket++){int x=12+bucket;double position=origin+(bucket+.5)*span/buckets;float top=Height/2-maxima[bucket]*visualGain*(Height*.42f),bottom=Height/2-minima[bucket]*visualGain*(Height*.42f);if(bottom-top<1){top-=.5f;bottom+=.5f;}g.DrawLine(position>=start&&position<=end?selected:outside,x,top,x,bottom);}}
  double phase=Phase==null?0:Phase();if(GetReverse!=null&&GetReverse())phase=1-phase;int cursor=12+(int)((start+LengthValue*phase-origin)/span*PlotWidth);using(var p=new Pen(Color.FromArgb(180,Color.White)))g.DrawLine(p,cursor,7,cursor,Height-7);
  PaintHandle(g,x1,dragging==1||Focused&&keyboardHandle==1);PaintHandle(g,x2,dragging==2||Focused&&keyboardHandle==2);
  if(dragging!=0){string text=dragging==1?"START "+(start*Duration).ToString("0.000")+" s":"LENGTH "+(LengthValue*Duration*StretchValue).ToString("0.000")+" s";int x=Math.Max(20,Math.Min(Width-130,(dragging==1?x1:x2)+9));using(var b=new SolidBrush(Skin.Ink))g.FillRectangle(b,x,0,108,17);using(var f=new Font("Consolas",8))Skin.Text(g,text,f,Accent,new Rectangle(x,0,108,17),ContentAlignment.MiddleLeft);}
 }
 void PaintHandle(Graphics g,int x,bool active){if(x<0||x>=Width)return;using(var fill=new SolidBrush(active?Color.White:Accent))g.FillRectangle(fill,x-5,2,10,Height-4);using(var p=new Pen(Skin.Ink,1))for(int dy=-4;dy<=4;dy+=4)g.DrawLine(p,x-2,Height/2+dy,x+2,Height/2+dy);}
 void Apply(int handle,double value,bool commit){double start=StartValue,end=start+LengthValue;double minimum=Math.Max(1.0/(Duration*DSP.Rate),.001/(Duration*StretchValue));if(handle==1)start=Math.Max(0,Math.Min(Math.Min(1-Math.Min(1,.001/Duration),end-minimum),value));else end=Math.Max(start+minimum,Math.Min(start+Math.Max(300,Duration*StretchValue)/(Duration*StretchValue),value));if(ChangeRegion!=null)ChangeRegion(start,end-start,commit);Invalidate();}
 protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);if(e.Button!=MouseButtons.Left)return;Focus();int hit=Hit(e.X);if(hit==0)return;Focus();keyboardHandle=hit;dragSpan=ViewSpan;dragOrigin=ViewOrigin;initialStart=StartValue;initialLength=LengthValue;mouseOrigin=e.X;dragging=hit;Capture=true;Cursor=Cursors.SizeWE;Invalidate();}
 protected override void OnMouseMove(MouseEventArgs e){if(dragging!=0){double position=(dragging==1?initialStart:initialStart+initialLength)+(e.X-mouseOrigin)*dragSpan/PlotWidth;Apply(dragging,position,false);}else Cursor=Hit(e.X)!=0?Cursors.SizeWE:Cursors.Default;base.OnMouseMove(e);}
 protected override void OnMouseUp(MouseEventArgs e){if(e.Button==MouseButtons.Left&&dragging!=0)EndDrag();base.OnMouseUp(e);}
 void EndDrag(){dragging=0;Capture=false;Cursor=Cursors.Default;if(ChangeRegion!=null)ChangeRegion(StartValue,LengthValue,true);Invalidate();}
 protected override void OnMouseCaptureChanged(EventArgs e){if(dragging!=0&&!Capture)EndDrag();base.OnMouseCaptureChanged(e);}
 protected override void OnMouseLeave(EventArgs e){if(dragging==0)Cursor=Cursors.Default;base.OnMouseLeave(e);}
 protected override void OnMouseWheel(MouseEventArgs e){var handled=e as HandledMouseEventArgs;if(handled!=null)handled.Handled=true;if(dragging!=0)return;double span=ViewSpan,origin=ViewOrigin;if((ModifierKeys&Keys.Shift)!=0){manualSpan=span;manualOrigin=Math.Max(0,Math.Min(Math.Max(0,FitSpan-span),origin-e.Delta/120.0*span*.15));manualView=true;}else{double fraction=Math.Max(0,Math.Min(1,(e.X-12)/(double)PlotWidth)),anchor=origin+fraction*span;double minimum=Math.Min(FitSpan,32.0/Math.Max(1,Duration*DSP.Rate));manualSpan=Math.Max(minimum,Math.Min(FitSpan,span*Math.Pow(1.4,-e.Delta/120.0)));manualOrigin=Math.Max(0,Math.Min(Math.Max(0,FitSpan-manualSpan),anchor-fraction*manualSpan));manualView=manualSpan<FitSpan-1e-12;}Invalidate();base.OnMouseWheel(e);}
 protected override void OnMouseDoubleClick(MouseEventArgs e){if(dragging!=0){dragging=0;Capture=false;if(ChangeRegion!=null)ChangeRegion(initialStart,initialLength,false);}manualView=false;Invalidate();base.OnMouseDoubleClick(e);}
 protected override bool IsInputKey(Keys key){return key==Keys.Left||key==Keys.Right||key==Keys.Home||key==Keys.End||base.IsInputKey(key);}
 protected override void OnKeyDown(KeyEventArgs e){if(e.KeyCode==Keys.Escape&&dragging!=0){dragging=0;Capture=false;if(ChangeRegion!=null)ChangeRegion(initialStart,initialLength,false);Invalidate();e.Handled=true;}else if(e.KeyCode==Keys.S||e.KeyCode==Keys.E){keyboardHandle=e.KeyCode==Keys.S?1:2;Invalidate();e.Handled=true;}else if(e.KeyCode==Keys.Left||e.KeyCode==Keys.Right){double step=(e.Shift?.001:.01)/Duration;Apply(keyboardHandle,(keyboardHandle==1?StartValue:StartValue+LengthValue)+(e.KeyCode==Keys.Left?-step:step),true);e.Handled=true;}else if(e.KeyCode==Keys.Home||e.KeyCode==Keys.End){Apply(keyboardHandle,e.KeyCode==Keys.Home?0:1,true);e.Handled=true;}base.OnKeyDown(e);}
}
}





