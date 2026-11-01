using System;
namespace LoopPedal {
// Eight-line orthogonal feedback network with input diffusion and damped, time-scaled decay.
sealed class CloudDiffusion {
 readonly float[][] lines=new float[8][],diffuse=new float[4][];readonly int[] at=new int[8],da=new int[4];readonly float[] damp=new float[8],gain=new float[8],work=new float[8];readonly float[] preL=new float[1201],preR=new float[1201];int preAt,tick;float previousSize=-1;
 public CloudDiffusion(){int[] lengths={2503,2791,3079,3469,3907,4217,4639,5003};for(int i=0;i<8;i++)lines[i]=new float[lengths[i]];int[] d={149,241,353,547};for(int i=0;i<4;i++)diffuse[i]=new float[d[i]];}
 public void Process(float l,float r,float size,out float left,out float right){if((tick++&255)==0&&size!=previousSize){previousSize=size;double seconds=1.2+Math.Max(0,Math.Min(1,size))*8;for(int i=0;i<8;i++)gain[i]=(float)Math.Pow(.001,lines[i].Length/(48000.0*seconds));}float pl=preL[preAt],pr=preR[preAt];preL[preAt]=l;preR[preAt]=r;preAt=(preAt+1)%preL.Length;float input=(pl+pr)*.5f;for(int i=0;i<4;i++){float delayed=diffuse[i][da[i]],y=delayed-input*.65f;diffuse[i][da[i]]=input+y*.65f;da[i]=(da[i]+1)%diffuse[i].Length;input=y;}left=right=0;for(int i=0;i<8;i++){float tap=lines[i][at[i]];damp[i]+=(tap-damp[i])*.23f;work[i]=damp[i];left+=tap*((i&1)==0?1:-1);right+=tap*((i&2)==0?1:-1);}for(int width=1;width<8;width*=2)for(int baseIndex=0;baseIndex<8;baseIndex+=2*width)for(int j=0;j<width;j++){float a=work[baseIndex+j],b=work[baseIndex+j+width];work[baseIndex+j]=a+b;work[baseIndex+j+width]=a-b;}float side=(pl-pr)*.2f;for(int i=0;i<8;i++){lines[i][at[i]]=work[i]*.35355339f*gain[i]+input*.35f+side*((i&1)==0?1:-1);at[i]=(at[i]+1)%lines[i].Length;}left*=.18f;right*=.18f;}
 public void Clear(){foreach(var a in lines)Array.Clear(a,0,a.Length);foreach(var a in diffuse)Array.Clear(a,0,a.Length);Array.Clear(damp,0,8);Array.Clear(preL,0,preL.Length);Array.Clear(preR,0,preR.Length);}
}
}
