using System;namespace LoopPedal {
sealed class BloomReverb {
 readonly int[] lengths={2801,3253,3691,4253,4783,5501,6311,7489};readonly float[][] lines=new float[8][],diffuse=new float[4][];readonly int[] at=new int[8],da=new int[4];readonly float[] damp=new float[8],gain=new float[8],targetGain=new float[8],work=new float[8];readonly double[] phase=new double[8];
 readonly float[] preL=new float[96032],preR=new float[96032];int preAt,tick;public float left,right,alpha=.35f,alphaTarget=.35f,diff=.65f,motion=.25f;double attack=.35,outDcL,outDcR;
 public BloomReverb(){int[] sizes={347,613,991,1559};for(int i=0;i<8;i++){lines[i]=new float[lengths[i]+64];phase[i]=i*.73;}for(int i=0;i<4;i++)diffuse[i]=new float[sizes[i]];}
 public static double seconds(float size){return 1+23*size*size;}
 public void process(float l,float r,float size,float tone,float rise,float diffusion,float movement){
  if((tick++&127)==0){alphaTarget=(float)(1-Math.Exp(-2*Math.PI*400*Math.Pow(35,tone)/48000));for(int i=0;i<8;i++)targetGain[i]=(float)Math.Pow(.001,lengths[i]/(48000.0*seconds(size)));}
  alpha+=(alphaTarget-alpha)*.002f;diff+=((.25f+diffusion*.55f)-diff)*.001f;motion+=(movement-motion)*.001f;double wanted=.02+rise*1.78;attack+=Math.Max(-.000015,Math.Min(.000015,(wanted-attack)*.0004));
  float mid=(l+r)*.5f;for(int i=0;i<4;i++){float old=diffuse[i][da[i]],v=old-mid*diff;diffuse[i][da[i]]=mid+v*diff;da[i]=(da[i]+1)%diffuse[i].Length;mid=v;}
  preL[preAt]=mid+(l-r)*.28f;preR[preAt]=mid+(r-l)*.28f;float sum=0;left=right=0;
  for(int i=0;i<8;i++){phase[i]+=.000005*(1+i*.17);if(phase[i]>Math.PI*2)phase[i]-=Math.PI*2;float tap=MasterFx.Read(lines[i],at[i]-lengths[i]-Math.Sin(phase[i])*motion*12);damp[i]+=(tap-damp[i])*alpha;work[i]=damp[i];sum+=work[i];gain[i]+=(targetGain[i]-gain[i])*.002f;left+=tap*((i&1)==0?1:-1);right+=tap*((i&2)==0?1:-1);}
  for(int i=0;i<8;i++){double fraction=(i+1)/8.0,delay=(.008+attack*fraction)*48000;float a=MasterFx.Read(preL,preAt-delay),b=MasterFx.Read(preR,preAt-delay*.973-13);float excitation=(a+b*.7f)*(float)(fraction*fraction)*.32f;lines[i][at[i]]=(work[i]-sum*.25f)*gain[i]+excitation*((i&1)==0?1:-1);at[i]=(at[i]+1)%lines[i].Length;}
  preAt=(preAt+1)%preL.Length;left*=.22f;right*=.22f;outDcL+=(left-outDcL)*.00065428;outDcR+=(right-outDcR)*.00065428;left=(float)(left-outDcL);right=(float)(right-outDcR);
 }
 public void clear(){foreach(float[] line in lines)Array.Clear(line,0,line.Length);foreach(float[] line in diffuse)Array.Clear(line,0,line.Length);Array.Clear(preL,0,preL.Length);Array.Clear(preR,0,preR.Length);Array.Clear(damp,0,damp.Length);outDcL=outDcR=0;left=right=0;}
}
}
