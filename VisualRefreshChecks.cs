using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
namespace LoopPedal {
partial class Pedal {
 sealed class PaintScope:Scope {public int Invalidations;protected override void OnInvalidated(InvalidateEventArgs e){Invalidations++;base.OnInvalidated(e);}public int Paints;protected override void OnPaint(PaintEventArgs e){Paints++;base.OnPaint(e);}}
 sealed class PaintEditor:SampleEditor {public int Invalidations;protected override void OnInvalidated(InvalidateEventArgs e){Invalidations++;base.OnInvalidated(e);}public int Paints;protected override void OnPaint(PaintEventArgs e){Paints++;base.OnPaint(e);}}
 void VerifyScrollAndImport(){
  var bus=loopers[0];bus.ClearLoop();bus.SetDestructive(false);RefreshTracks();var original=bankScopes[0];var inline=bankInlineEditors[0];var parent=original.Parent;
  var probe=new PaintScope{Dock=DockStyle.Fill,Samples=original.Samples,Phase=original.Phase,Accent=original.Accent};var edit=new PaintEditor{Dock=DockStyle.Fill,Samples=inline.Samples,Phase=inline.Phase,GetStart=inline.GetStart,GetLength=inline.GetLength,GetStretch=inline.GetStretch,GetReverse=inline.GetReverse,ChangeRegion=inline.ChangeRegion,Accent=inline.Accent};
  parent.Controls.Remove(original);parent.Controls.Remove(inline);parent.Controls.Add(probe);parent.Controls.Add(edit);bankScopes[0]=probe;bankInlineEditors[0]=edit;
  try{RefreshTracks();Application.DoEvents();var audio=new float[96000];for(int i=0;i<48000;i++)audio[i*2]=audio[i*2+1]=(float)(.3*Math.Sin(i*.05));int before=probe.Invalidations;InstallImportedAudio(bus,audio,"Refresh test");if(probe.Invalidations<=before||!object.ReferenceEquals(probe.Samples(),audio))throw new Exception("Normal import repaint: before="+before+" after="+probe.Invalidations+" visible="+probe.Visible+" handle="+probe.IsHandleCreated+" source="+object.ReferenceEquals(probe.Samples(),audio));
   bus.ClearLoop();bus.SetDestructive(true);RefreshTracks();Application.DoEvents();before=edit.Invalidations;InstallImportedAudio(bus,audio,"Destructive import");if(edit.Invalidations<=before||edit.Samples()==null)throw new Exception("Destructive import did not immediately repaint");before=edit.Invalidations;var old=edit.Samples();InstallImportedAudio(bus,audio,"Second import");if(edit.Invalidations<=before||object.ReferenceEquals(edit.Samples(),old))throw new Exception("Replacement/merged import did not repaint");
  }finally{parent.Controls.Remove(probe);parent.Controls.Remove(edit);probe.Dispose();edit.Dispose();parent.Controls.Add(original);parent.Controls.Add(inline);bankScopes[0]=original;bankInlineEditors[0]=inline;bus.ClearLoop();bus.SetDestructive(false);RefreshTracks();}
  foreach(var list in bankTracks)if(list.Parent.BackColor==SystemColors.Control||list.BackColor==SystemColors.Control)throw new Exception("White loop scroll background");for(int i=0;i<5;i++)bus.CommitLayer(new Layer{Name="Scroll "+i,Original=new float[96000],Audio=new float[96000]});RefreshTracks();Application.DoEvents();var host=bankTracks[0].Parent.Parent as StudioScrollHost;if(host==null||host.Region==null||host.Region.IsVisible(0,0))throw new Exception("Rounded custom loop scroll host");var rail=host.VerticalRail;if(!rail.Visible||rail.Maximum()<=0)throw new Exception("Layer scrollbar for long lists");if(((ScrollableControl)bankTracks[0]).AutoScroll)throw new Exception("Native scrollbar enabled");var flags=BindingFlags.Instance|BindingFlags.NonPublic;typeof(StudioScrollRail).GetMethod("OnKeyDown",flags).Invoke(rail,new object[]{new KeyEventArgs(Keys.End)});if(-StudioScrolling.Position(bankTracks[0]).Y!=rail.Maximum())throw new Exception("Custom layer End scroll");typeof(StudioScrollRail).GetMethod("OnKeyDown",flags).Invoke(rail,new object[]{new KeyEventArgs(Keys.Home)});if(StudioScrolling.Position(bankTracks[0]).Y!=0)throw new Exception("Custom layer Home scroll");bus.ClearLoop();bus.SetDestructive(false);SetSingleMode(false);RefreshTracks();

 }
}
}

