using System;using System.Threading;
namespace LoopPedal {
static class SamplerPlayback {
 public static double Time(Layer l){return l.PlaybackMode==1?l.Stretch/Math.Pow(2,l.Pitch/12):l.Stretch;}
 public static double Pitch(Layer l){return l.PlaybackMode==1?l.Pitch-12*Math.Log(l.Stretch,2):l.Pitch;}
 public static double Snap(double value,bool snap){value=Math.Max(-12,Math.Min(12,value));return snap?Math.Round(value,MidpointRounding.AwayFromZero):value;}
 public static void SetPitch(Layer l,double value){l.Pitch=Snap(value,l.SnapPitch);if(l.PlaybackMode==1)l.Stretch=1;}
 public static void SetStretch(Layer l,double value){value=Math.Max(.5,Math.Min(2,value));if(l.PlaybackMode==1){l.Stretch=1;l.Pitch=Snap(-12*Math.Log(value,2),l.SnapPitch);}else l.Stretch=value;}
 public static void Mode(Layer l,int mode){double time=Time(l),pitch=Pitch(l);if(mode==1&&l.PlaybackMode!=1)time/=Math.Pow(2,pitch/12);l.PlaybackMode=mode;if(mode==1)SetStretch(l,time);else{l.Stretch=Math.Max(.5,Math.Min(2,time));l.Pitch=Snap(pitch,l.SnapPitch);}}
 public static double Phase(Layer l,long frame){long phase=frame-l.Offset;return l.PlaybackMode==1&&l.Sampler.IsAt(phase)?l.Sampler.Normalized:Engine.Phase(phase,l.PlaybackFrames);}
 public static void Update(Engine b,Layer l){l.LivePlaybackFrames=Math.Max(1,(int)Math.Round(l.Audio.Length/2*Time(l)));if(!b.WarpLayers&&b.Layers.Count>0&&object.ReferenceEquals(b.Layers[0],l)){b.LoopFrames=l.LivePlaybackFrames;b.LoopOrigin=l.Offset;}}
 public static float[] Render(float[] source,double stretch,double pitch,bool reverse,int mode,double cycleMs,CancellationToken cancel){if(mode==0)return DSP.Render(source,stretch,pitch,reverse,cancel);if(reverse){var backwards=new float[source.Length];for(int i=0,n=source.Length/2;i<n;i++){backwards[i*2]=source[(n-1-i)*2];backwards[i*2+1]=source[(n-1-i)*2+1];}source=backwards;}int count=Math.Max(1,(int)Math.Round(source.Length/2*stretch/(mode==1?Math.Pow(2,pitch/12):1)));var output=new float[count*2];var voice=new SamplerVoice();for(int i=0;i<count;i++){if((i&4095)==0)cancel.ThrowIfCancellationRequested();double l,r;voice.Read(source,i,stretch,pitch,mode,cycleMs,out l,out r);output[i*2]=(float)l;output[i*2+1]=(float)r;}return output;}
}
sealed class SamplerVoice {
 float[] source;long last=long.MinValue,lastHop=long.MinValue;int previousMode,previousCycle;double position,rate=1,lastL,lastR,correctionL,correctionR;int fade;readonly double[] starts=new double[2],ratios=new double[2];
 public bool IsAt(long frame){return source!=null&&(frame==last||frame==last+1);}public double Normalized{get{return source==null?0:Mod(position,Math.Max(1,source.Length/2))/Math.Max(1,source.Length/2);}}
 static double Mod(double x,int n){return (x%n+n)%n;}
 static double Edge(float[] audio,double position,int channel){int n=Math.Max(1,audio.Length/2),fade=Math.Min(160,n/4);position=Mod(position,n);if(fade==0||position>=fade&&position<n-fade)return DSP.Sample(audio,position,channel);double a,b,t;if(position<fade){a=n-1-position;b=position;t=(position+fade)/(2*fade);}else{double d=n-position;a=position;b=d-1;t=(fade-d)/(2*fade);}double w=.5-.5*Math.Cos(Math.PI*t);return DSP.Sample(audio,a,channel)*(1-w)+DSP.Sample(audio,b,channel)*w;}
 public void Read(float[] audio,long frame,double stretch,double pitch,int mode,double cycleMs,out double left,out double right){int cycle=Math.Max(32,(int)Math.Round(cycleMs*48));double ratio=Math.Pow(2,pitch/12),targetRate=ratio/stretch;bool reset=!object.ReferenceEquals(source,audio)||frame!=last&&frame!=last+1||previousMode!=mode||previousCycle!=cycle;if(reset){position=frame*targetRate;rate=targetRate;lastHop=long.MinValue;}source=audio;previousMode=mode;previousCycle=cycle;
  if(mode==1){left=Edge(audio,position,0);right=Edge(audio,position,1);if(frame!=last){rate+=(targetRate-rate)*.003;position=Mod(position+rate,Math.Max(1,audio.Length/2));}}else{long hop=(long)Math.Floor(frame/(double)cycle);if(lastHop==long.MinValue||hop<lastHop||hop-lastHop>2)lastHop=hop-2;while(lastHop<hop){long next=++lastHop;int at=(int)((next%2+2)%2);starts[at]=next*cycle/stretch+(1/stretch-ratio)*cycle;ratios[at]=ratio;}left=right=0;for(int k=0;k<2;k++){long anchor=(hop-k)*cycle;int age=(int)(frame-anchor),at=(int)(((hop-k)%2+2)%2);double weight=.5-.5*Math.Cos(Math.PI*age/cycle),pos=starts[at]+age*ratios[at];left+=Edge(audio,pos,0)*weight;right+=Edge(audio,pos,1)*weight;}}
  if(reset&&last!=long.MinValue){correctionL=lastL-left;correctionR=lastR-right;fade=960;}if(fade>0){double t=fade/960.0;t=t*t*(3-2*t);left+=correctionL*t;right+=correctionR*t;fade--;}lastL=left;lastR=right;last=frame;
 }
}
}
