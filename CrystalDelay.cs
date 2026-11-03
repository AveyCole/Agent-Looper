using System;
namespace LoopPedal {
/** Variable-pitch forward/reverse crystal splices. Voices keep their settings until
 * the envelope ends; changing splice length never cuts an active voice. */
sealed class CrystalDelay {
 private static readonly int SIZE=65536;
 private readonly float[] historyL=new float[SIZE],historyR=new float[SIZE];
 private readonly int[] age=new int[8],length=new int[8];private readonly double[] start=new double[8],rates=new double[8],rightRates=new double[8];
 private readonly float[] widths=new float[8];private int at,until,slot,previous=-1;private uint seed=13273;
 private float inL,inR,softL,softR,lowL,lowR,dcL,dcR,mix=.82f,width=1,pitch=12,window=.128f;
 public float left,right;
 private static readonly float[][] KERNEL=makeKernel();
 private static float[][] makeKernel(){float[][] k=new float[256][];for(int p=0;p<256;p++){k[p]=new float[16];double sum=0;for(int t=0;t<16;t++){double x=t-7-p/256.0;double v=(Math.Abs(x)<1e-9?.44:Math.Sin(Math.PI*.44*x)/(Math.PI*x))*(.5+.5*Math.Cos(Math.PI*x/8));k[p][t]=(float)v;sum+=v;}for(int t=0;t<16;t++)k[p][t]/=(float)sum;}return k;}
 private static float read(float[] data,double position){position=(position%SIZE+SIZE)%SIZE;int i=(int)position,p=Math.Min(255,(int)((position-i)*256));float[] k=KERNEL[p];float y=0;int tap=(i-7+SIZE)&(SIZE-1);for(int t=0;t<16;t++){y+=data[tap]*k[t];tap=(tap+1)&(SIZE-1);}return y;}

 private float random(){seed^=seed<<13;seed^=seed>>17;seed^=seed<<5;return (seed&0x7fffffff)/2147483648f;}
 private float linear(float[] data,double position){position=(position%SIZE+SIZE)%SIZE;int i=(int)position;float f=(float)(position-i);return data[i]+(data[(i+1)&(SIZE-1)]-data[i])*f;}
 private double anchor(double rate,int count){double wanted=at-512-(rate>0?count*rate:0),best=wanted,errorBest=1e20;if(previous<0||age[previous]>=length[previous])return wanted;double energy=0;for(int j=0;j<64;j+=4){float x=linear(historyL,start[previous]+(age[previous]+j)*rates[previous]);energy+=x*x;}if(energy<1e-9)return wanted;for(int shift=-384;shift<=384;shift+=4){double error=Math.Abs(shift)*energy*1e-6;for(int j=0;j<64;j+=4){double reference=linear(historyL,start[previous]+(age[previous]+j)*rates[previous]),value=linear(historyL,wanted+shift+j*rate),d=reference-value;error+=d*d;}if(error<errorBest){errorBest=error;best=wanted+shift;}}return best;}
 public void process(float l,float r,MasterFx fx){
  mix+=(fx.CrystalMix-mix)*.001f;width+=(fx.CrystalWidth-width)*.001f;pitch+=(fx.CrystalPitch-pitch)*.001f;window+=(fx.CrystalWindow-window)*.001f;
  inL+=(l-inL)*.52f;inR+=(r-inR)*.52f;softL+=(inL-softL)*.52f;softR+=(inR-softR)*.52f;historyL[at]=softL;historyR[at]=softR;
  if(--until<=0){int count=Math.Max(1920,Math.Min(12000,(int)(window*48000)));until=Math.Max(256,count/4);
   int chosen=-1;for(int k=0;k<8;k++){int v=(slot+k)&7;if(age[v]>=length[v]){chosen=v;break;}}
   if(chosen>=0){int v=chosen;double rate=Math.Pow(2,Math.Max(-12,Math.Min(12,pitch))/12.0);bool rev=fx.CrystalReverse>=.9999f||fx.CrystalReverse>.0001f&&random()<fx.CrystalReverse;rate*=rev?-1:1;rates[v]=rate*(1+.001*width);rightRates[v]=rate*(1-.001*width);start[v]=anchor(rates[v],count);age[v]=0;length[v]=count;widths[v]=width;previous=v;slot=(v+1)&7;}
  }
  float a=0,b=0,weight=0;for(int v=0;v<8;v++)if(age[v]<length[v]){float w=MasterFx.GrainWindow[(int)((long)age[v]*2047/length[v])]*.5f;a+=read(historyL,start[v]+age[v]*rates[v])*w;b+=read(historyR,start[v]-301*widths[v]+age[v]*rightRates[v])*w;weight+=w;age[v]++;}
  float norm=1/Math.Max(1,weight);a*=norm;b*=norm;at=(at+1)&(SIZE-1);lowL+=(a-lowL)*.62f;lowR+=(b-lowR)*.62f;dcL+=(lowL-dcL)*.002f;dcR+=(lowR-dcR)*.002f;
  left=l*(1-mix)+(lowL-dcL)*mix;right=r*(1-mix)+(lowR-dcR)*mix;
 }
 public void clear(){Array.Clear(historyL,0,historyL.Length);Array.Clear(historyR,0,historyR.Length);Array.Clear(age,0,age.Length);Array.Clear(length,0,length.Length);Array.Clear(start,0,start.Length);at=until=slot=0;previous=-1;left=right=inL=inR=softL=softR=lowL=lowR=dcL=dcR=0;}
}

}
