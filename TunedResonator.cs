using System;namespace LoopPedal {
sealed class TunedResonator {
 static readonly int[][] SCALES={new int[]{0,12,19,24,28,31,36,40},new int[]{0,3,7,10,12,15,19,22},new int[]{0,4,7,11,12,16,19,23},new int[]{0,2,4,7,9,12,14,16},new int[]{0,7,12,19,24,31,36,43},new int[]{0,1,2,3,4,5,6,7}};
 readonly float[][] l=new float[8][],r=new float[8][];readonly double[] delayL=new double[8],delayR=new double[8],wantedL=new double[8],wantedR=new double[8];readonly float[] lowL=new float[8],lowR=new float[8],feedback=new float[8],wantedFeedback=new float[8],excitation=new float[8];
 public TunedResonator(){for(int i=0;i<8;i++){l[i]=new float[2048];r[i]=new float[2048];}}
 int at,tick;public float left,right,alpha=.3f,alphaTarget=.3f,spread=.7f,excite=.35f;double inputDcL,inputDcR,outDcL,outDcR;bool primed;
 public static double seconds(float size){return .12+11.88*size*size;}
 public void process(float il,float ir,float size,float tone,float root,int scale,float width,float drive,float detune){
  if((tick++&127)==0){double cutoff=600*Math.Pow(24,tone);alphaTarget=(float)(1-Math.Exp(-2*Math.PI*cutoff/48000));int mode=Math.Max(0,Math.Min(5,scale));for(int i=0;i<8;i++){double midi=24+Math.Floor(root*60+.5)+SCALES[mode][i];while(midi>120)midi-=12;double hz=440*Math.Pow(2,(midi-69)/12);double omega=2*Math.PI*hz/48000,phaseDelay=Math.Atan2((1-alphaTarget)*Math.Sin(omega),1-(1-alphaTarget)*Math.Cos(omega))/omega;double period=48000/hz;wantedL[i]=Math.Max(4,period*Math.Pow(2,detune*25/1200)-phaseDelay);wantedR[i]=Math.Max(4,period*Math.Pow(2,-detune*25/1200)-phaseDelay);wantedFeedback[i]=(float)Math.Pow(.001,period/(48000*seconds(size)));excitation[i]=(float)(Math.Sqrt(1-wantedFeedback[i]*wantedFeedback[i])*.3);if(!primed){delayL[i]=wantedL[i];delayR[i]=wantedR[i];}}primed=true;}
  alpha+=(alphaTarget-alpha)*.002f;spread+=(width-spread)*.002f;excite+=(drive-excite)*.002f;inputDcL+=(il-inputDcL)*.00065428;inputDcR+=(ir-inputDcR)*.00065428;float inL=(float)(Math.Tanh((il-inputDcL)*(1+excite*6))/(1+excite*3)),inR=(float)(Math.Tanh((ir-inputDcR)*(1+excite*6))/(1+excite*3));left=right=0;
  for(int i=0;i<8;i++){delayL[i]+=Math.Max(-.04,Math.Min(.04,(wantedL[i]-delayL[i])*.001));delayR[i]+=Math.Max(-.04,Math.Min(.04,(wantedR[i]-delayR[i])*.001));float a=MasterFx.Read(l[i],at-delayL[i]),b=MasterFx.Read(r[i],at-delayR[i]);lowL[i]+=(a-lowL[i])*alpha;lowR[i]+=(b-lowR[i])*alpha;feedback[i]+=(wantedFeedback[i]-feedback[i])*.002f;l[i][at]=(inL*.8f+inR*.2f)*excitation[i]+lowL[i]*feedback[i];r[i][at]=(inR*.8f+inL*.2f)*excitation[i]+lowR[i]*feedback[i];float pan=(i/7f*2-1)*spread;left+=a*(1-pan*.65f);right+=b*(1+pan*.65f);}
  at=(at+1)&2047;left=(float)Math.Tanh(left*.65);right=(float)Math.Tanh(right*.65);outDcL+=(left-outDcL)*.00019633;outDcR+=(right-outDcR)*.00019633;left=(float)(left-outDcL);right=(float)(right-outDcR);
 }
 public void clear(){foreach(float[] a in l)Array.Clear(a,0,a.Length);foreach(float[] a in r)Array.Clear(a,0,a.Length);Array.Clear(lowL,0,lowL.Length);Array.Clear(lowR,0,lowR.Length);inputDcL=inputDcR=outDcL=outDcR=0;left=right=0;primed=false;}
}
}
