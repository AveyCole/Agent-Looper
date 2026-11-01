using System;
namespace LoopPedal {
sealed class MasterFilter {
 double smooth,lp1L,lp1R,lp2L,lp2R,hp1L,hp1R,hp2L,hp2R,lowAlpha=1,highAlpha,lowMix,highMix,targetLow=1,targetHigh;int tick;
 public void Process(float l,float r,float position,out float left,out float right){
 if(position==0&&smooth==0&&lowMix==0&&highMix==0){lp1L=lp2L=hp1L=l;lp1R=lp2R=hp1R=r;hp2L=hp2R=0;left=l;right=r;return;}
 smooth+=(Math.Max(-1,Math.Min(1,position))-smooth)*.0025;if(Math.Abs(smooth)<1e-7)smooth=0;
 if((tick++&31)==0){double low=Math.Max(0,-smooth),high=Math.Max(0,smooth);targetLow=1-Math.Exp(-2*Math.PI*20000*Math.Pow(.0015,low)/48000);targetHigh=1-Math.Exp(-2*Math.PI*20*Math.Pow(600,high)/48000);}
 // Coefficients and dry/filter crossfades evolve every sample, including the
 // center crossing. The cached cutoff calculation never steps the signal path.
 lowAlpha+=(targetLow-lowAlpha)*.003;highAlpha+=(targetHigh-highAlpha)*.003;
 double lowGoal=Math.Min(1,Math.Max(0,-smooth)/.04),highGoal=Math.Min(1,Math.Max(0,smooth)/.04);
 lowMix+=(lowGoal-lowMix)*.003;highMix+=(highGoal-highMix)*.003;
 if(lowMix<1e-12)lowMix=0;if(highMix<1e-12)highMix=0;

 lp1L+=lowAlpha*(l-lp1L);lp1R+=lowAlpha*(r-lp1R);lp2L+=lowAlpha*(lp1L-lp2L);lp2R+=lowAlpha*(lp1R-lp2R);
 hp1L+=highAlpha*(l-hp1L);hp1R+=highAlpha*(r-hp1R);double hl=l-hp1L,hr=r-hp1R;hp2L+=highAlpha*(hl-hp2L);hp2R+=highAlpha*(hr-hp2R);
 left=(float)(l+(lp2L-l)*lowMix+(hl-hp2L-l)*highMix);right=(float)(r+(lp2R-r)*lowMix+(hr-hp2R-r)*highMix);
 }
 public void Clear(){smooth=lp1L=lp1R=lp2L=lp2R=hp1L=hp1R=hp2L=hp2R=lowMix=highMix=0;tick=0;lowAlpha=targetLow=1;highAlpha=targetHigh=0;}
 public static string Label(float position){if(Math.Abs(position)<.01)return "OFF";double hz=position<0?20000*Math.Pow(.0015,-position):20*Math.Pow(600,position);return (position<0?"LP ":"HP ")+(hz>=1000?(hz/1000).ToString("0.0")+"k":hz.ToString("0"));}
}
}
