using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace LoopPedal {
sealed class StudioMenuColors:ProfessionalColorTable {
 public override Color ToolStripDropDownBackground{get{return Skin.Ink;}}public override Color MenuBorder{get{return Skin.Line;}}public override Color MenuItemBorder{get{return Skin.Mint;}}public override Color MenuItemSelected{get{return Color.FromArgb(31,55,49);}}public override Color MenuItemSelectedGradientBegin{get{return MenuItemSelected;}}public override Color MenuItemSelectedGradientEnd{get{return MenuItemSelected;}}public override Color ImageMarginGradientBegin{get{return Skin.Ink;}}public override Color ImageMarginGradientMiddle{get{return Skin.Ink;}}public override Color ImageMarginGradientEnd{get{return Skin.Ink;}}public override Color SeparatorDark{get{return Skin.Line;}}public override Color SeparatorLight{get{return Skin.Line;}}
}
sealed class StudioMenuRenderer:ToolStripProfessionalRenderer {
 public StudioMenuRenderer():base(new StudioMenuColors()){}
 protected override void Initialize(ToolStrip strip){base.Initialize(strip);if(strip is ToolStripDropDown)Skin.RoundFrame(strip,10);}
 protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e){Skin.Border(e.Graphics,new RectangleF(.5f,.5f,e.ToolStrip.Width-1,e.ToolStrip.Height-1),Skin.Line,10);}
 protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e){if(!e.Item.Selected||!e.Item.Enabled)return;e.Graphics.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;using(var path=Skin.Rounded(new RectangleF(3,1,Math.Max(1,e.Item.Width-6),Math.Max(1,e.Item.Height-2)),6))using(var brush=new SolidBrush(Color.FromArgb(31,55,49)))e.Graphics.FillPath(brush,path);}

 protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e){e.TextColor=!e.Item.Enabled?Skin.Muted:e.Item.Selected?Skin.Mint:Color.WhiteSmoke;base.OnRenderItemText(e);}
}
class StudioChoice:Control {
 public readonly List<object> Items=new List<object>();int selected=-1;bool hover;ToolStripDropDown popup;
 public FlatStyle FlatStyle{get;set;}public ComboBoxStyle DropDownStyle{get;set;}public int DropDownWidth{get;set;}
 public event EventHandler SelectedIndexChanged;
 public int SelectedIndex{get{return selected>=0&&selected<Items.Count?selected:-1;}set{if(value<-1||value>=Items.Count)throw new ArgumentOutOfRangeException("value");int old=SelectedIndex;selected=value;Invalidate();if(old!=value&&SelectedIndexChanged!=null)SelectedIndexChanged(this,EventArgs.Empty);}}
 public object SelectedItem{get{return SelectedIndex>=0?Items[SelectedIndex]:null;}set{SelectedIndex=Items.IndexOf(value);}}
 public StudioChoice(){Skin.RoundFrame(this,8);Height=29;TabStop=true;Cursor=Cursors.Hand;SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);BackColor=Skin.Panel;ForeColor=Color.WhiteSmoke;AccessibleRole=AccessibleRole.ComboBox;}
 protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;using(var b=new SolidBrush(Enabled?BackColor:Skin.Ink))g.FillRectangle(b,ClientRectangle);Skin.Border(g,new RectangleF(.5f,.5f,Width-1,Height-1),Focused||hover?Skin.Mint:Skin.Line);Skin.Text(g,SelectedItem==null?"—":SelectedItem.ToString(),Font,Enabled?ForeColor:Skin.Muted,new Rectangle(10,0,Math.Max(1,Width-38),Height),ContentAlignment.MiddleLeft);int x=Width-18,y=Height/2;using(var b=new SolidBrush(Enabled?Skin.Muted:Skin.Line))g.FillPolygon(b,new[]{new Point(x-4,y-2),new Point(x+4,y-2),new Point(x,y+2)});}
 protected override void OnMouseEnter(EventArgs e){hover=true;Invalidate();base.OnMouseEnter(e);}protected override void OnMouseLeave(EventArgs e){hover=false;Invalidate();base.OnMouseLeave(e);}
 protected override void OnMouseDown(MouseEventArgs e){if(e.Button==MouseButtons.Left){Focus();Open();}base.OnMouseDown(e);}
 void Open(){if(!Enabled||Items.Count==0)return;if(popup!=null&&!popup.IsDisposed){popup.Close();return;}int width=Math.Max(Width,DropDownWidth);var list=new ChoicePopup(this){Size=new Size(width,Math.Min(8,Items.Count)*30+8),Font=Font};var host=new ToolStripControlHost(list){Margin=Padding.Empty,Padding=Padding.Empty,AutoSize=false,Size=list.Size};popup=new ToolStripDropDown{Renderer=new StudioMenuRenderer(),Padding=Padding.Empty,Margin=Padding.Empty,AutoClose=true,DropShadowEnabled=true};popup.Items.Add(host);popup.Closed+=(s,e)=>{popup=null;Invalidate();};popup.Show(this,new Point(0,Height+3));list.Focus();}
 protected override bool IsInputKey(Keys key){return key==Keys.Down||key==Keys.Up||base.IsInputKey(key);}
 protected override void OnKeyDown(KeyEventArgs e){if(e.KeyCode==Keys.Space||e.KeyCode==Keys.Enter||e.KeyCode==Keys.Down){Open();e.Handled=true;}else if(e.KeyCode==Keys.Up&&SelectedIndex>0){SelectedIndex--;e.Handled=true;}base.OnKeyDown(e);}
 sealed class ChoicePopup:Control {
  readonly StudioChoice owner;int top,highlight;
  public ChoicePopup(StudioChoice choice){Skin.RoundFrame(this,10);owner=choice;highlight=Math.Max(0,choice.SelectedIndex);top=Math.Max(0,highlight-6);BackColor=Skin.Ink;ForeColor=Color.WhiteSmoke;TabStop=true;SetStyle(ControlStyles.AllPaintingInWmPaint|ControlStyles.UserPaint|ControlStyles.OptimizedDoubleBuffer,true);AccessibleRole=AccessibleRole.List;}
  protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.Clear(Skin.Ink);int rows=(Height-8)/30;for(int row=0;row<rows&&top+row<owner.Items.Count;row++){int index=top+row;var r=new Rectangle(4,4+row*30,Width-8,30);if(index==highlight)using(var path=Skin.Rounded(r,6))using(var b=new SolidBrush(Color.FromArgb(31,55,49)))g.FillPath(b,path);if(index==owner.SelectedIndex)using(var b=new SolidBrush(Skin.Mint))g.FillEllipse(b,11,r.Y+12,5,5);Skin.Text(g,owner.Items[index].ToString(),Font,index==highlight?Skin.Mint:Color.WhiteSmoke,new Rectangle(25,r.Y,Width-40,30),ContentAlignment.MiddleLeft);}Skin.Border(g,new RectangleF(.5f,.5f,Width-1,Height-1),Skin.Line,10);if(owner.Items.Count>rows){int thumb=Math.Max(18,(Height-8)*rows/owner.Items.Count),y=4+(Height-8-thumb)*top/Math.Max(1,owner.Items.Count-rows);using(var b=new SolidBrush(Skin.Muted))g.FillRectangle(b,Width-5,y,2,thumb);}}
  protected override void OnMouseMove(MouseEventArgs e){int index=top+(e.Y-4)/30;if(index>=0&&index<owner.Items.Count){highlight=index;Invalidate();}base.OnMouseMove(e);}
  protected override void OnMouseDown(MouseEventArgs e){if(e.Button==MouseButtons.Left){int index=top+(e.Y-4)/30;if(e.Y>=4&&index>=0&&index<owner.Items.Count){owner.SelectedIndex=index;if(owner.popup!=null)owner.popup.Close();}}base.OnMouseDown(e);}
  protected override void OnMouseWheel(MouseEventArgs e){top=Math.Max(0,Math.Min(Math.Max(0,owner.Items.Count-(Height-8)/30),top-Math.Sign(e.Delta)*2));Invalidate();base.OnMouseWheel(e);}
  protected override bool IsInputKey(Keys key){return true;}
  protected override void OnKeyDown(KeyEventArgs e){if(e.KeyCode==Keys.Escape){if(owner.popup!=null)owner.popup.Close();return;}if(e.KeyCode==Keys.Enter||e.KeyCode==Keys.Space){owner.SelectedIndex=highlight;if(owner.popup!=null)owner.popup.Close();return;}if(e.KeyCode==Keys.Down)highlight=Math.Min(owner.Items.Count-1,highlight+1);if(e.KeyCode==Keys.Up)highlight=Math.Max(0,highlight-1);if(e.KeyCode==Keys.Home)highlight=0;if(e.KeyCode==Keys.End)highlight=owner.Items.Count-1;int rows=(Height-8)/30;if(highlight<top)top=highlight;if(highlight>=top+rows)top=highlight-rows+1;Invalidate();e.Handled=true;}
 }
}
class StudioNumber:Control {
 readonly TextBox editor;decimal minimum,maximum=100,current,increment=1;int places;bool hover,setting;
 public event EventHandler ValueChanged;public BorderStyle BorderStyle{get;set;}
 public decimal Minimum{get{return minimum;}set{minimum=value;if(maximum<value)maximum=value;Value=current;}}
 public decimal Maximum{get{return maximum;}set{maximum=value;if(minimum>value)minimum=value;Value=current;}}
 public decimal Value{get{return current;}set{decimal next=Math.Max(minimum,Math.Min(maximum,value));bool changed=next!=current;current=next;RefreshText();Invalidate();if(changed&&ValueChanged!=null)ValueChanged(this,EventArgs.Empty);}}
 public decimal Increment{get{return increment;}set{increment=value;}}public int DecimalPlaces{get{return places;}set{places=Math.Max(0,Math.Min(10,value));RefreshText();}}
 public StudioNumber(){Skin.RoundFrame(this,8);Height=29;TabStop=true;BackColor=Skin.Panel;ForeColor=Color.WhiteSmoke;SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);AccessibleRole=AccessibleRole.SpinButton;editor=new TextBox{BorderStyle=BorderStyle.None,BackColor=BackColor,ForeColor=ForeColor,Text="0",Font=new Font("Consolas",10)};Controls.Add(editor);editor.Leave+=(s,e)=>ParseText();editor.KeyDown+=(s,e)=>{if(e.KeyCode==Keys.Enter){ParseText();editor.SelectAll();e.Handled=e.SuppressKeyPress=true;}else if(e.KeyCode==Keys.Escape){RefreshText();e.Handled=e.SuppressKeyPress=true;}else if(e.KeyCode==Keys.Up||e.KeyCode==Keys.Down){ParseText();Value+=e.KeyCode==Keys.Up?increment:-increment;e.Handled=e.SuppressKeyPress=true;}};editor.GotFocus+=(s,e)=>Invalidate();editor.LostFocus+=(s,e)=>Invalidate();}
 void ParseText(){if(setting)return;decimal value;if(decimal.TryParse(editor.Text,NumberStyles.Number,CultureInfo.CurrentCulture,out value))Value=value;else RefreshText();}
 void RefreshText(){if(editor==null)return;setting=true;editor.Text=current.ToString("F"+places,CultureInfo.CurrentCulture);setting=false;}
 protected override void OnResize(EventArgs e){if(editor!=null)editor.SetBounds(8,Math.Max(4,(Height-editor.PreferredHeight)/2),Math.Max(12,Width-32),editor.PreferredHeight);base.OnResize(e);}
 protected override void OnBackColorChanged(EventArgs e){if(editor!=null)editor.BackColor=BackColor;base.OnBackColorChanged(e);}protected override void OnForeColorChanged(EventArgs e){if(editor!=null)editor.ForeColor=ForeColor;base.OnForeColorChanged(e);}
 protected override void OnPaint(PaintEventArgs e){e.Graphics.Clear(BackColor);Skin.Border(e.Graphics,new RectangleF(.5f,.5f,Width-1,Height-1),ContainsFocus||hover?Skin.Mint:Skin.Line);using(var p=new Pen(Skin.Line))e.Graphics.DrawLine(p,Width-24,1,Width-24,Height-2);using(var b=new SolidBrush(Skin.Muted)){int x=Width-12;e.Graphics.FillPolygon(b,new[]{new Point(x-3,Height/2-4),new Point(x+3,Height/2-4),new Point(x,Height/2-7)});e.Graphics.FillPolygon(b,new[]{new Point(x-3,Height/2+4),new Point(x+3,Height/2+4),new Point(x,Height/2+7)});}}
 protected override void OnMouseDown(MouseEventArgs e){if(e.Button==MouseButtons.Left){if(e.X>Width-24){ParseText();Value+=e.Y<Height/2?increment:-increment;}else{editor.Focus();editor.SelectAll();}}base.OnMouseDown(e);}
 protected override void OnMouseWheel(MouseEventArgs e){ParseText();Value+=Math.Sign(e.Delta)*increment;var handled=e as HandledMouseEventArgs;if(handled!=null)handled.Handled=true;base.OnMouseWheel(e);}
 protected override void OnMouseEnter(EventArgs e){hover=true;Invalidate();base.OnMouseEnter(e);}protected override void OnMouseLeave(EventArgs e){hover=false;Invalidate();base.OnMouseLeave(e);}
}
}
