using System;using System.IO;using System.Text;using System.Linq;using System.Collections.Generic;using System.Runtime.InteropServices;using System.Threading;using System.Windows.Forms;using System.Drawing;
namespace LoopPedal {
static class VstNative {
 const string D="AgentVst3.dll";
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]internal static extern int av_version();
 [DllImport(D,CallingConvention=CallingConvention.Cdecl,CharSet=CharSet.Unicode)]internal static extern IntPtr av_open(string path,StringBuilder error,int capacity);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]internal static extern void av_close(IntPtr p);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]internal static extern int av_latency(IntPtr p);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]internal static extern void av_name(IntPtr p,StringBuilder name,int size);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]internal static extern uint av_revision(IntPtr p);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]internal static extern int av_process(IntPtr p,[In,Out]float[] audio,int frames,double tempo,int playing,int numerator,int denominator);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]internal static extern int av_editor(IntPtr p,IntPtr hwnd,out int width,out int height,int embedded);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]internal static extern int av_editor_fit(IntPtr p,int availableWidth,int availableHeight,out int width,out int height);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]internal static extern void av_editor_close(IntPtr p);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]internal static extern int av_state(IntPtr p,[Out]byte[] data,int capacity);
 [DllImport(D,CallingConvention=CallingConvention.Cdecl)]internal static extern int av_restore(IntPtr p,byte[] data,int size);
}
sealed class VstInstance:IDisposable {
 internal IntPtr Handle;internal readonly object Gate=new object();public readonly int Latency;public readonly string Name;internal Form Editor;internal Control EmbeddedHost;byte[] cached;uint cachedRevision;static readonly List<VstInstance> live=new List<VstInstance>();
 public VstInstance(string path,byte[] state){if(VstNative.av_version()!=1)throw new InvalidOperationException("Extract the full updated ZIP: VST3 bridge version mismatch.");var error=new StringBuilder(2048);Handle=VstNative.av_open(path,error,error.Capacity);if(Handle==IntPtr.Zero)throw new InvalidOperationException(error.ToString());try{var name=new StringBuilder(1024);VstNative.av_name(Handle,name,name.Capacity);Name=name.ToString();if(state!=null&&state.Length>0&&VstNative.av_restore(Handle,state,state.Length)==0)throw new InvalidOperationException("Plugin could not restore its saved state.");Latency=VstNative.av_latency(Handle);live.Add(this);}catch{VstNative.av_close(Handle);Handle=IntPtr.Zero;throw;}}
 public bool Process(float[] audio,double tempo,bool playing=true,int numerator=4,int denominator=4){if(Handle==IntPtr.Zero||!Monitor.TryEnter(Gate))return false;try{return VstNative.av_process(Handle,audio,VstRack.Block,tempo,playing?1:0,numerator,denominator)!=0;}finally{Monitor.Exit(Gate);}}
 public byte[] Snapshot(){lock(Gate){if(Handle==IntPtr.Zero)return new byte[0];uint revision=VstNative.av_revision(Handle);if(cached!=null&&revision==cachedRevision)return cached;int n=VstNative.av_state(Handle,null,0);if(n<=0||n>33554440)return new byte[0];var result=new byte[n];VstNative.av_state(Handle,result,n);cached=result;cachedRevision=revision;return result;}}
 public void DetachEmbedded(Control host){if(object.ReferenceEquals(EmbeddedHost,host)){VstNative.av_editor_close(Handle);EmbeddedHost=null;}}
 public bool Embed(Control host){if(Editor!=null&&!Editor.IsDisposed)return false;int width,height;if(VstNative.av_editor(Handle,host.Handle,out width,out height,1)==0)throw new InvalidOperationException("This plugin does not provide a Windows editor.");EmbeddedHost=host;host.Size=new Size(width,height);return true;}
 public void ShowEditor(Form owner,Action changed){if(Editor!=null&&!Editor.IsDisposed){Editor.Activate();return;}if(EmbeddedHost!=null){EmbeddedHost=null;VstNative.av_editor_close(Handle);}var window=new Form{Text=Name+" · VST3",BackColor=Skin.Ink,StartPosition=FormStartPosition.CenterParent,ClientSize=new Size(640,480),FormBorderStyle=FormBorderStyle.FixedToolWindow,ShowInTaskbar=false};var panel=new Panel{Dock=DockStyle.Fill};window.Controls.Add(panel);window.Show(owner);int width,height;if(VstNative.av_editor(Handle,panel.Handle,out width,out height,0)==0){window.Close();throw new InvalidOperationException("This plugin does not provide a Windows editor.");}window.ClientSize=new Size(width,height);Editor=window;window.FormClosing+=(s,e)=>{VstNative.av_editor_close(Handle);Editor=null;if(changed!=null)changed();};}
 public void Dispose(){if(Handle==IntPtr.Zero)return;if(Editor!=null)Editor.Close();lock(Gate){VstNative.av_close(Handle);Handle=IntPtr.Zero;}live.Remove(this);}
 [DllImport("user32.dll")]static extern IntPtr GetFocus();[DllImport("user32.dll")]static extern bool IsChild(IntPtr parent,IntPtr child);
 public static bool PluginKeyboardFocus(){var focus=GetFocus();if(focus==IntPtr.Zero)return false;foreach(var instance in live){var host=instance.EmbeddedHost;if(host!=null&&!host.IsDisposed&&host.IsHandleCreated&&(focus==host.Handle||IsChild(host.Handle,focus)))return true;}return false;}
 public static void Shutdown(){foreach(var instance in live.ToArray())instance.Dispose();}
}
sealed class VstSlot {
 internal Func<float[],bool> TestProcess;internal int TestLatency;public int Latency{get{return TestProcess!=null?TestLatency:Instance==null?0:Instance.Latency;}}
 public string Path,Name,Error;public byte[] State=new byte[0];public volatile bool Bypass;public VstInstance Instance;
 public VstSlot Copy(){return new VstSlot{Path=Path,Name=Name,State=Instance!=null?Instance.Snapshot():(byte[])State.Clone(),Bypass=Bypass,Error=Error};}
 public void Prepare(){if(Instance!=null||TestProcess!=null)return;try{Instance=new VstInstance(Path,State);Name=Instance.Name;Error=null;}catch(Exception ex){Error=ex.Message;}}
 public void Write(BinaryWriter d){d.Write(Path??"");d.Write(Name??"VST3");d.Write(Bypass);d.Write(State.Length);d.Write(State);}
 public static VstSlot Read(BinaryReader d){var slot=new VstSlot{Path=d.ReadString(),Name=d.ReadString(),Bypass=d.ReadBoolean()};int n=d.ReadInt32();if(n<0||n>33554440)throw new IOException("Invalid plugin state length");slot.State=d.ReadBytes(n);if(slot.State.Length!=n)throw new EndOfStreamException();return slot;}
}
sealed class VstDelay {
 readonly float[] data;int at;public VstDelay(int max){data=new float[Math.Max(1,max+1)*2];}
 public void Process(float[] audio,int delay){if(delay<=0)return;int size=data.Length/2;for(int i=0;i<VstRack.Block;i++){int read=(at-delay+size)%size;float l=data[read*2],r=data[read*2+1];data[at*2]=audio[i*2];data[at*2+1]=audio[i*2+1];audio[i*2]=l;audio[i*2+1]=r;at=(at+1)%size;}}
}
sealed class VstRack {
 public const int Block=64;public readonly VstSlot[] Slots=new VstSlot[3];public bool HasPlugins{get{return Slots.Any(s=>s!=null);}}public string Error{get{return string.Join(" · ",Slots.Where(s=>s!=null&&s.Error!=null).Select(s=>s.Name+": "+s.Error));}}
 readonly float[] input=new float[Block*2],output=new float[Block*2],source=new float[Block*2],stage=new float[Block*2],dry=new float[Block*2];readonly float[][] node={new float[Block*2],new float[Block*2],new float[Block*2]},amount={new float[Block],new float[Block],new float[Block]};readonly float[] reset=new float[Block];readonly float[] mix=new float[3];VstDelay[] local,align;VstDelay baseDelay;int at;int[] latency=new int[3];
 public VstRack Copy(){var rack=new VstRack();for(int i=0;i<3;i++)if(Slots[i]!=null)rack.Slots[i]=Slots[i].Copy();return rack;}
 public void Prepare(){for(int i=0;i<3;i++)if(Slots[i]!=null)Slots[i].Prepare();int max=0;for(int i=0;i<3;i++){latency[i]=Slots[i]==null?0:Slots[i].Latency;max=Math.Max(max,latency[i]);}local=new VstDelay[3];align=new VstDelay[3];for(int i=0;i<3;i++){local[i]=new VstDelay(latency[i]);align[i]=new VstDelay(max);}baseDelay=new VstDelay(max);}
 public void Step(MasterFx fx,float l,float r,out float resultL,out float resultR){resultL=output[at*2];resultR=output[at*2+1];input[at*2]=l;input[at*2+1]=r;if(++at==Block){at=0;Render(fx);}}
 void Run(MasterFx fx,int id,float[] src){Array.Copy(src,node[id],src.Length);var slot=Slots[id];if(slot==null){for(int i=0;i<Block;i++){float l,r;fx.BuiltinVstNode(id,src[i*2],src[i*2+1],amount[0][i],amount[1][i],amount[2][i],out l,out r);node[id][i*2]+=l;node[id][i*2+1]+=r;}return;}Array.Copy(src,dry,src.Length);local[id].Process(dry,latency[id]);bool ok=slot.TestProcess!=null?slot.TestProcess(node[id]):slot.Instance!=null&&slot.Instance.Process(node[id],fx.Tempo,fx.VstPlaying,fx.VstNumerator,fx.VstDenominator);if(!ok){Array.Copy(dry,node[id],dry.Length);if(slot.Instance!=null)slot.Error="Processing unavailable; bypassed";}for(int i=0;i<Block;i++){float target=slot.Bypass||!ok?0:amount[id][i];mix[id]+=(target-mix[id])*.003f;node[id][i*2]=dry[i*2]+(node[id][i*2]-dry[i*2])*mix[id];node[id][i*2+1]=dry[i*2+1]+(node[id][i*2+1]-dry[i*2+1])*mix[id];}}
 void Render(MasterFx fx){for(int i=0;i<Block;i++)fx.AdvanceVst(out amount[0][i],out amount[1][i],out amount[2][i],out reset[i]);int routing=fx.VstRouting,order=fx.VstOrder;if(routing==1){Array.Copy(input,stage,input.Length);foreach(int id in FxOrder.Items(order)){Run(fx,id,stage);Array.Copy(node[id],stage,stage.Length);}Array.Copy(stage,output,stage.Length);}else{if(routing==2){Run(fx,2,input);Array.Copy(node[2],source,source.Length);}else Array.Copy(input,source,input.Length);int max= routing==2?Math.Max(latency[0],latency[1]):Math.Max(latency[2],Math.Max(latency[0],latency[1]));Array.Copy(source,stage,stage.Length);baseDelay.Process(stage,max);Array.Copy(stage,output,stage.Length);for(int id=0;id<(routing==2?2:3);id++){Run(fx,id,source);align[id].Process(node[id],max-latency[id]);for(int k=0;k<output.Length;k++)output[k]+=node[id][k]-stage[k];}}
 // Route/reset gates apply to the effect difference, preserving latency-aligned dry sound.
 for(int i=0;i<Block;i++){float gate=reset[i];output[i*2]=input[i*2]+(output[i*2]-input[i*2])*gate;output[i*2+1]=input[i*2+1]+(output[i*2+1]-input[i*2+1])*gate;}}
 public void Write(BinaryWriter d){for(int i=0;i<3;i++){d.Write(Slots[i]!=null);if(Slots[i]!=null)Slots[i].Write(d);}}
 public static VstRack Read(BinaryReader d){var rack=new VstRack();for(int i=0;i<3;i++)if(d.ReadBoolean())rack.Slots[i]=VstSlot.Read(d);return rack.HasPlugins?rack:null;}
}
sealed partial class MasterFx {
 internal bool VstPlaying=true;internal int VstNumerator=4,VstDenominator=4;public VstRack Plugins;public int VstRouting{get{return activeRoute;}}public int VstOrder{get{return activeOrder;}}
  VstRack renderedRack;int switchRemaining;float pluginLastL,pluginLastR,pluginCorrectionL,pluginCorrectionR;bool firstSwitchSample;
 void BeginPluginSwitch(VstRack rack){if(object.ReferenceEquals(renderedRack,rack))return;renderedRack=rack;switchRemaining=960;firstSwitchSample=true;}
 void FinishPluginSwitch(float l,float r,ref float wl,ref float wr){if(switchRemaining>0){if(firstSwitchSample){pluginCorrectionL=pluginLastL-l-wl;pluginCorrectionR=pluginLastR-r-wr;firstSwitchSample=false;}float gain=switchRemaining/960f;gain*=gain;wl+=pluginCorrectionL*gain;wr+=pluginCorrectionR*gain;switchRemaining--;}pluginLastL=l+wl;pluginLastR=r+wr;}
 public void AdvanceVst(out float reverb,out float delay,out float grain,out float gate){AdvanceFx();reverb=rs;delay=ds;grain=gs;gate=resetGate*routeGate;}
 public void BuiltinVstNode(int id,float l,float r,float rev,float del,float gr,out float wl,out float wr){rs=rev;ds=del;gs=gr;Node(id,l,r);wl=effectWetL[id];wr=effectWetR[id];}
 public void CopyPluginsTo(MasterFx target){target.Plugins=Plugins==null?null:Plugins.Copy();}
 public void PreparePlugins(){if(Plugins!=null)Plugins.Prepare();}
 public void PluginAudio(float l,float r,out float wl,out float wr){float resultL,resultR;Plugins.Step(this,l,r,out resultL,out resultR);wl=resultL-l;wr=resultR-r;}
}
}
