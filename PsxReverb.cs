using System;
namespace LoopPedal {

/** SPU-style wet path: preset geometries, SAME/DIFF reflections, four comb taps,
 * two allpasses, signed 16-bit RAM and 22.05 kHz processing. 48 kHz I/O uses a new
 * bandlimited resampler; this is an original adaptation, not bit-exact emulation.
 * Topology/preset data: https://psx-spx.consoledev.net/soundprocessingunitspu/ */
sealed class PsxReverb {
 private readonly float[] ram=new float[32768],inputL=new float[64],inputR=new float[64];
 private int at,inputAt,phase;private float wall=-.575f,tone=.35f,previousL,previousR,nextL,nextR,lowL,lowR,dcL,dcR;
 private int silent;private float energy,tailGate=1;private bool asleep;
 public float left,right;
 private static readonly int[][] PROGRAMS=new int[][]{new int[]{0x01A5,0x0139,0x6000,0x5000,0x4C00,0xB800,0xBC00,0xC000,0x6000,0x5C00,0x15BA,0x11BB,0x14C2,0x10BD,0x11BC,0x0DC1,0x11C0,0x0DC3,0x0DC0,0x09C1,0x0BC4,0x07C1,0x0A00,0x06CD,0x09C2,0x05C1,0x05C0,0x041A,0x0274,0x013A,0x8000,0x8000},new int[]{0x007D,0x005B,0x6D80,0x54B8,0xBED0,0x0000,0x0000,0xBA80,0x5800,0x5300,0x04D6,0x0333,0x03F0,0x0227,0x0374,0x01EF,0x0334,0x01B5,0x0000,0x0000,0x0000,0x0000,0x0000,0x0000,0x0000,0x0000,0x01B4,0x0136,0x00B8,0x005C,0x8000,0x8000},new int[]{0x00E3,0x00A9,0x6F60,0x4FA8,0xBCE0,0x4510,0xBEF0,0xA680,0x5680,0x52C0,0x0DFB,0x0B58,0x0D09,0x0A3C,0x0BD9,0x0973,0x0B59,0x08DA,0x08D9,0x05E9,0x07EC,0x04B0,0x06EF,0x03D2,0x05EA,0x031D,0x031C,0x0238,0x0154,0x00AA,0x8000,0x8000},new int[]{0x033D,0x0231,0x7E00,0x5000,0xB400,0xB000,0x4C00,0xB000,0x6000,0x5400,0x1ED6,0x1A31,0x1D14,0x183B,0x1BC2,0x16B2,0x1A32,0x15EF,0x15EE,0x1055,0x1334,0x0F2D,0x11F6,0x0C5D,0x1056,0x0AE1,0x0AE0,0x07A2,0x0464,0x0232,0x8000,0x8000}};
 private static readonly int[] SIZES={0xADE0/2,0x26C0/2,0x6FE0/2,0xF6C0/2};
 private int program,workSize=0xADE0/2;private float modeGate=1;
 private static readonly float[][] KERNEL=makeKernel();
 private static float[][] makeKernel(){float[][] table=new float[256][];for(int p=0;p<256;p++){table[p]=new float[48];double sum=0;for(int t=0;t<48;t++){double x=t-23.5-p/256.0;double value=(Math.Abs(x)<1e-9?.4:Math.Sin(Math.PI*.4*x)/(Math.PI*x))*(.5-.5*Math.Cos(2*Math.PI*t/47));table[p][t]=(float)value;sum+=value;}for(int t=0;t<48;t++)table[p][t]/=(float)sum;}return table;}
 public static double seconds(float size,int preset){return (SIZES[Math.Max(0,Math.Min(3,preset))]/22256.0)* .13*Math.Log(.001)/Math.Log(.25+.65*Math.Max(0,Math.Min(1,size)));}
 private float read(int offset){int i=(at+offset)%workSize;if(i<0)i+=workSize;return ram[i];}
 private void write(int offset,float value){int i=(at+offset)%workSize;if(i<0)i+=workSize;ram[i]=(float)(Math.Floor(Math.Max(-1,Math.Min(32767f/32768,value))*32768+.5)/32768);}
 private float coefficient(int k){int value=PROGRAMS[program][k];return (value>=32768?value-65536:value)/32768f;}
 private int address(int k){return PROGRAMS[program][k]*4;}
 private void tick(float l,float r){
  float iir=coefficient(2),wallScale=wall*Math.Abs(coefficient(7))/.5f;wallScale=Math.Max(-.94f,Math.Min(.94f,wallScale));l*=coefficient(30);r*=coefficient(31);
  float old=read(address(10)-1);write(address(10),old+(l+read(address(16))*wallScale-old)*iir);
  old=read(address(11)-1);write(address(11),old+(r+read(address(17))*wallScale-old)*iir);
  old=read(address(18)-1);write(address(18),old+(l+read(address(25))*wallScale-old)*iir);
  old=read(address(19)-1);write(address(19),old+(r+read(address(24))*wallScale-old)*iir);
  float a=read(address(12))*coefficient(3)+read(address(14))*coefficient(4)+read(address(20))*coefficient(5)+read(address(22))*coefficient(6);
  float b=read(address(13))*coefficient(3)+read(address(15))*coefficient(4)+read(address(21))*coefficient(5)+read(address(23))*coefficient(6);
  float apf=coefficient(8),dl=read(address(26)-address(0)),dr=read(address(27)-address(0));a-=dl*apf;b-=dr*apf;write(address(26),a);write(address(27),b);a=a*apf+dl;b=b*apf+dr;
  apf=coefficient(9);dl=read(address(28)-address(1));dr=read(address(29)-address(1));a-=dl*apf;b-=dr*apf;write(address(28),a);write(address(29),b);
  previousL=nextL;previousR=nextR;nextL=(a*apf+dl)*.65f;nextR=(b*apf+dr)*.65f;if(++at==workSize)at=0;
 }
 public void process(float l,float r,float decay,float colour,int preset){
  preset=Math.Max(0,Math.Min(3,preset));if(preset!=program){modeGate+=(0-modeGate)*.003f;if(modeGate<.0001f){clear();program=preset;workSize=SIZES[preset];modeGate=0;}}else modeGate+=(1-modeGate)*.003f;
  if(Math.Abs(l)+Math.Abs(r)>1e-8){silent=0;asleep=false;}else if(asleep){left=right=0;return;}else silent++;
  wall+=(-(.25f+.65f*Math.Max(0,Math.Min(1,decay)))-wall)*.0004f;
  tone+=((.055f+.78f*Math.Max(0,Math.Min(1,colour)))-tone)*.0008f;
  inputL[inputAt]=l;inputR[inputAt]=r;phase+=22050;
  if(phase>=48000){phase-=48000;float[] kernel=KERNEL[Math.Min(255,phase*256/22050)];float il=0,ir=0;for(int t=0;t<48;t++){int i=(inputAt-t+64)&63;il+=inputL[i]*kernel[t];ir+=inputR[i]*kernel[t];}tick(il,ir);}
  inputAt=(inputAt+1)&63;float fraction=phase/48000f;
  lowL+=(previousL+(nextL-previousL)*fraction-lowL)*tone;lowR+=(previousR+(nextR-previousR)*fraction-lowR)*tone;
  dcL+=(lowL-dcL)*.0013f;dcR+=(lowR-dcR)*.0013f;left=lowL-dcL;right=lowR-dcR;
  // Console RAM is quantized; fade its inaudible limit-cycle residue to true silence.
  energy+=((Math.Abs(left)+Math.Abs(right))*.5f-energy)*.0002f;
  float target=silent>48000&&energy<.00006f?0:1;tailGate+=(target-tailGate)*.0008f;
  left*=tailGate*modeGate;right*=tailGate*modeGate;if(target==0&&tailGate<.00001f){clear();tailGate=0;asleep=true;}

 }
 public void clear(){Array.Clear(ram,0,ram.Length);Array.Clear(inputL,0,inputL.Length);Array.Clear(inputR,0,inputR.Length);at=inputAt=phase=silent=0;energy=0;tailGate=1;asleep=false;left=right=previousL=previousR=nextL=nextR=lowL=lowR=dcL=dcR=0;}
}

}
