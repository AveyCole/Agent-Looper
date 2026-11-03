using System;namespace LoopPedal {
sealed class TapeEcho {
 public float left,right;double seconds,wowPhase,flutterPhase,lastOffset;float drive=.3f,wear=.25f,tone=.65f,flutter=.2f,lpL,lpR,dcL,dcR,alpha=.35f;int tick;bool primed;
 public void process(float[] l,float[] r,int at,double target,float d,float w,float t,float f){
  if(!primed){seconds=target;primed=true;}drive+=(d-drive)*.001f;wear+=(w-wear)*.0002f;tone+=(t-tone)*.001f;flutter+=(f-flutter)*.001f;
  double delta=Math.Max(-.000012,Math.Min(.000012,(target-seconds)*.0007));seconds+=delta;
  wowPhase+=.00006937683777;flutterPhase+=.0009293878267;if(wowPhase>Math.PI*2)wowPhase-=Math.PI*2;if(flutterPhase>Math.PI*2)flutterPhase-=Math.PI*2;
  double offset=Math.Sin(wowPhase)*(2+238*wear)*wear+Math.Sin(flutterPhase)*(6*flutter);double speed=1-delta*48000+offset-lastOffset;lastOffset=offset;
  int band=Math.Max(8,Math.Min(32,(int)Math.Ceiling(Math.Abs(speed)*16)));double pos=at-seconds*48000+offset;
  float a=MasterFx.GrainRead(l,pos,band),b=MasterFx.GrainRead(r,pos+.37*flutter,band);
  if((tick++&127)==0)alpha=(float)(1-Math.Exp(-2*Math.PI*600*Math.Pow(25,tone)/48000));
  lpL+=(a-lpL)*alpha;lpR+=(b-lpR)*alpha;float gain=(float)(1-wear*.06*(.5+.5*Math.Sin(wowPhase)));
  float k=1+drive*7;float x=(float)(Math.Tanh(lpL*k)/k)*gain,y=(float)(Math.Tanh(lpR*k)/k)*gain;dcL+=(x-dcL)*.00039262f;dcR+=(y-dcR)*.00039262f;left=x-dcL;right=y-dcR;
 }
 public void clear(){left=right=lpL=lpR=dcL=dcR=0;primed=false;}
}
}
