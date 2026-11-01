using System;
namespace LoopPedal {
// Original granular cloud/delay: bounded overlap, stereo scatter, diffused feedback.
sealed class CloudsFx {
 readonly float[] left=new float[192000],right=new float[192000];
 readonly CloudDiffusion diffusion=new CloudDiffusion();
 sealed class Voice {public int Age,Length,Band;public double Position,Rate;public float L=1,R=1,Texture;}
 readonly Voice[] voices=new Voice[32];int head,untilGrain;uint seed=19830917;
 double windowFrames=12000;float feedback,colourL,colourR,lofiL,lofiR;
 public CloudsFx(){for(int i=0;i<voices.Length;i++)voices[i]=new Voice();}
 float Random(){seed^=seed<<13;seed^=seed>>17;seed^=seed<<5;return (seed&0x7fffffff)/2147483648f;}
 static float Clamp(float x,float min,float max){return Math.Max(min,Math.Min(max,x));}
 static float Bound(float x){return x/(1+Math.Abs(x)*.35f);}
 public void Process(float l,float r,MasterFx fx,out float wetL,out float wetR){
  int mode=fx.GrainMode;float cloudy=Clamp(fx.Cloudy,0,1),texture=Clamp(fx.Texture,0,1);
  feedback+=(Clamp(fx.CloudFeedback,-.92f,.92f)-feedback)*.001f;
  if(--untilGrain<=0){int length=(int)(Clamp(fx.GrainSize,.01f,.4f)*48000);float density=mode==2?Clamp((2+cloudy*10)*Clamp(fx.Density,1,12)/4,1,24):Clamp(fx.Density,1,12);
   untilGrain=Math.Max(32,(int)(length/density*(.85f+Random()*.3f)));
   foreach(var v in voices)if(v.Age>=v.Length){v.Age=0;v.Length=length;
    v.Rate=Math.Pow(2,Clamp(fx.GrainPitch,-12,12)/12.0);v.Band=Math.Max(8,Math.Min(32,(int)Math.Ceiling(v.Rate*16)));
    double delay=mode==2?Clamp(fx.CloudWindow,.04f,1.5f)*48000:Clamp(fx.Position,0,1.5f)*48000;
    double scatter=Random()*Clamp(fx.Spray,0,1)*(mode==2?1200+cloudy*18000:24000);
    v.Position=head-length*Math.Max(1,v.Rate)-64-delay-scatter;
    if(fx.GrainReverse){v.Position+=length*v.Rate;v.Rate=-v.Rate;}
    float pan=(Random()*2-1)*Clamp(fx.Spread,0,1);v.L=(float)Math.Sqrt(1-pan);v.R=(float)Math.Sqrt(1+pan);v.Texture=texture;break;
   }
  }
  float gl=0,gr=0,wl=0,wr=0;
  foreach(var v in voices)if(v.Age<v.Length){float phase=v.Age/(float)v.Length;
   float hann=MasterFx.GrainWindow[(int)(phase*2047)],triangle=1-Math.Abs(phase*2-1);
   float window=triangle+(hann-triangle)*v.Texture;
   gl+=MasterFx.GrainRead(left,v.Position,v.Band)*window*v.L;
   gr+=MasterFx.GrainRead(right,v.Position,v.Band)*window*v.R;
   wl+=window*v.L;wr+=window*v.R;v.Age++;v.Position+=v.Rate;
  }
  gl/=Math.Max(1,wl);gr/=Math.Max(1,wr);
  // Keep Window tied to a coherent echo; granular taps add the surrounding cloud.
  windowFrames+=(Clamp(fx.CloudWindow,.04f,1.5f)*48000-windowFrames)*.001;
  if(mode==2){float cloudMix=cloudy*.45f;gl=MasterFx.Read(left,head-windowFrames)*(1-cloudMix)+gl*cloudMix;gr=MasterFx.Read(right,head-windowFrames)*(1-cloudMix)+gr*cloudMix;}

  float dl,dr;diffusion.Process(gl,gr,.15f+cloudy*.65f,out dl,out dr);
  float diffuse=mode==2?cloudy*.8f:cloudy*.2f;
  gl=gl*(1-diffuse*.45f)+dl*diffuse;gr=gr*(1-diffuse*.45f)+dr*diffuse;
  // Keep modulation and feedback bounded; lo-fi is an explicit performance option.
  colourL+=(gl-colourL)*(.08f+(1-texture)*.65f);colourR+=(gr-colourR)*(.08f+(1-texture)*.65f);
  float processedL=gl*(1-texture*.5f)+colourL*texture*.5f,processedR=gr*(1-texture*.5f)+colourR*texture*.5f;
  lofiL+=(processedL-lofiL)*.18f;lofiR+=(processedR-lofiR)*.18f;
  if(fx.CloudLoFi){processedL=(float)Math.Round(lofiL*256)/256;processedR=(float)Math.Round(lofiR*256)/256;}
  if(!fx.Freeze){left[head]=l+Bound(processedR)*feedback;right[head]=r+Bound(processedL)*feedback;head=(head+1)%left.Length;}
  wetL=processedL;wetR=processedR;
 }
 public void CopyFrozenTo(CloudsFx target){Array.Copy(left,target.left,left.Length);Array.Copy(right,target.right,right.Length);target.head=head;target.windowFrames=windowFrames;target.seed=seed;}
 public void Clear(){Array.Clear(left,0,left.Length);Array.Clear(right,0,right.Length);diffusion.Clear();foreach(var v in voices)v.Age=v.Length;head=untilGrain=0;windowFrames=12000;feedback=colourL=colourR=lofiL=lofiR=0;seed=19830917;}
}
}
