using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace LoopPedal {
sealed class LiveStretch:IDisposable {
 const string Library="librubberband-2.dll";
 [DllImport(Library,CallingConvention=CallingConvention.Cdecl)]static extern IntPtr rubberband_new(uint rate,uint channels,int options,double time,double pitch);
 [DllImport(Library,CallingConvention=CallingConvention.Cdecl)]static extern void rubberband_delete(IntPtr state);
 [DllImport(Library,CallingConvention=CallingConvention.Cdecl)]static extern void rubberband_reset(IntPtr state);
 [DllImport(Library,CallingConvention=CallingConvention.Cdecl)]static extern int rubberband_get_engine_version(IntPtr state);
 [DllImport(Library,CallingConvention=CallingConvention.Cdecl)]static extern uint rubberband_get_preferred_start_pad(IntPtr state);
 [DllImport(Library,CallingConvention=CallingConvention.Cdecl)]static extern uint rubberband_get_start_delay(IntPtr state);
 [DllImport(Library,CallingConvention=CallingConvention.Cdecl)]static extern uint rubberband_get_samples_required(IntPtr state);
 [DllImport(Library,CallingConvention=CallingConvention.Cdecl)]static extern void rubberband_set_max_process_size(IntPtr state,uint frames);
 [DllImport(Library,CallingConvention=CallingConvention.Cdecl)]static extern void rubberband_set_time_ratio(IntPtr state,double value);
 [DllImport(Library,CallingConvention=CallingConvention.Cdecl)]static extern void rubberband_set_pitch_scale(IntPtr state,double value);
 [DllImport(Library,CallingConvention=CallingConvention.Cdecl)]static extern void rubberband_set_formant_option(IntPtr state,int value);
 [DllImport(Library,CallingConvention=CallingConvention.Cdecl)]static extern void rubberband_process(IntPtr state,IntPtr channels,uint frames,int final);
 [DllImport(Library,CallingConvention=CallingConvention.Cdecl)]static extern int rubberband_available(IntPtr state);
 [DllImport(Library,CallingConvention=CallingConvention.Cdecl)]static extern uint rubberband_retrieve(IntPtr state,IntPtr channels,uint frames);
 static readonly object initialize=new object();readonly object gate=new object();readonly AutoResetEvent wake=new AutoResetEvent(false);readonly Thread worker;readonly float[] ring=new float[16384];
 float[] source;double stretch,pitch;bool reverse,formants;long offset,seekFrame,startFrame;int revision,read,count;bool seekPending=true;volatile bool running=true;public volatile string Error;public int EngineVersion;public long Produced,ReadHits,ReadMisses;
 public LiveStretch(float[] audio,double time,double semitones,bool backwards,bool preserve,long frame,long layerOffset){source=audio;stretch=time;pitch=semitones;reverse=backwards;formants=preserve;offset=layerOffset;seekFrame=startFrame=frame;worker=new Thread(Work){IsBackground=true,Priority=ThreadPriority.AboveNormal,Name="Agent Looper live R3"};worker.Start();}
 public void Configure(float[] audio,double time,double semitones,bool backwards,bool preserve,long frame,long layerOffset){lock(gate){bool restart=!object.ReferenceEquals(source,audio)||reverse!=backwards||offset!=layerOffset;source=audio;stretch=time;pitch=semitones;reverse=backwards;formants=preserve;offset=layerOffset;if(restart)SeekLocked(frame);}wake.Set();}
 public void Seek(long frame){lock(gate)SeekLocked(frame);wake.Set();}
 void SeekLocked(long frame){seekFrame=startFrame=frame;read=count=0;seekPending=true;revision++;}
 public bool TryRead(long frame,out float left,out float right){left=right=0;if(!running)return false;lock(gate){if(frame<startFrame&&!seekPending){SeekLocked(frame);wake.Set();}if(frame>startFrame&&count>0){int skip=(int)Math.Min(count,frame-startFrame);read=(read+skip)%8192;count-=skip;startFrame+=skip;}if(count==0||frame!=startFrame){ReadMisses++;return false;}left=ring[read*2];right=ring[read*2+1];read=(read+1)%8192;count--;startFrame++;ReadHits++;if(count==512)wake.Set();return true;}}
 void Work(){using(var floatingPoint=AudioFloatScope.Enter())WorkCore();}
 void WorkCore(){IntPtr state=IntPtr.Zero,inputPointers=IntPtr.Zero,outputPointers=IntPtr.Zero;var pins=new GCHandle[4];try{
  var inL=new float[1024];var inR=new float[1024];var outL=new float[1024];var outR=new float[1024];var arrays=new[]{inL,inR,outL,outR};for(int i=0;i<4;i++)pins[i]=GCHandle.Alloc(arrays[i],GCHandleType.Pinned);inputPointers=Marshal.AllocHGlobal(IntPtr.Size*2);outputPointers=Marshal.AllocHGlobal(IntPtr.Size*2);for(int i=0;i<2;i++){Marshal.WriteIntPtr(inputPointers,i*IntPtr.Size,pins[i].AddrOfPinnedObject());Marshal.WriteIntPtr(outputPointers,i*IntPtr.Size,pins[i+2].AddrOfPinnedObject());}
  lock(initialize)state=rubberband_new(DSP.Rate,2,0x34010001,stretch,Math.Pow(2,pitch/12));if(state==IntPtr.Zero)throw new Exception("Could not create live R3 processor.");EngineVersion=rubberband_get_engine_version(state);rubberband_set_max_process_size(state,256);
  long produceFrame=0;double sourceFrame=0,previousTime=-1,previousPitch=-1000;bool previousFormants=false;int activeRevision=-1,pad=0,drop=0;
  while(running){float[] audio;double time,semitones;bool backwards,preserve,reset;long position,layerOffset;int version,queued;
   lock(gate){audio=source;time=stretch;semitones=pitch;backwards=reverse;preserve=formants;position=seekFrame;layerOffset=offset;version=revision;reset=seekPending;queued=count;if(reset)seekPending=false;}
   if(reset){rubberband_reset(state);rubberband_set_time_ratio(state,time);rubberband_set_pitch_scale(state,Math.Pow(2,semitones/12));produceFrame=position;int frames=Math.Max(1,audio.Length/2);sourceFrame=(((position-layerOffset)/time)%frames+frames)%frames;pad=(int)rubberband_get_preferred_start_pad(state);drop=(int)rubberband_get_start_delay(state);activeRevision=version;previousTime=time;previousPitch=semitones;previousFormants=!preserve;}
   if(previousTime!=time){rubberband_set_time_ratio(state,time);previousTime=time;}if(previousPitch!=semitones){rubberband_set_pitch_scale(state,Math.Pow(2,semitones/12));previousPitch=semitones;}if(previousFormants!=preserve){rubberband_set_formant_option(state,preserve?0x01000000:0);previousFormants=preserve;}
   if(queued>=1024){wake.WaitOne(8);continue;}
   int available=rubberband_available(state);if(available<=0){int need=Math.Max(1,Math.Min(256,(int)rubberband_get_samples_required(state)));int n=Math.Max(1,audio.Length/2);for(int i=0;i<need;i++){if(pad>0){inL[i]=inR[i]=0;pad--;}else{long p=(long)sourceFrame;if(backwards)p=n-1-p;inL[i]=(float)Engine.Edge(audio,p,0);inR[i]=(float)Engine.Edge(audio,p,1);sourceFrame=(sourceFrame+1)%n;}}rubberband_process(state,inputPointers,(uint)need,0);continue;}
   int got=(int)rubberband_retrieve(state,outputPointers,(uint)Math.Min(256,available));int skip=Math.Min(drop,got);drop-=skip;if(skip==got)continue;
   lock(gate){if(activeRevision!=revision)continue;if(count==0){read=0;startFrame=produceFrame;}for(int i=skip;i<got;i++){if(count>=8192)break;int write=(read+count)%8192;ring[write*2]=outL[i];ring[write*2+1]=outR[i];count++;produceFrame++;Produced++;}}
  }
 }catch(Exception e){Error="Live R3 · "+e.Message;}finally{if(state!=IntPtr.Zero)lock(initialize)rubberband_delete(state);if(inputPointers!=IntPtr.Zero)Marshal.FreeHGlobal(inputPointers);if(outputPointers!=IntPtr.Zero)Marshal.FreeHGlobal(outputPointers);foreach(var pin in pins)if(pin.IsAllocated)pin.Free();}}
 public void RequestStop(){running=false;wake.Set();}
 public void Dispose(){RequestStop();if(Thread.CurrentThread!=worker)worker.Join(1500);}
}
}
