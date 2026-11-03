using System;
namespace LoopPedal {
static class ExtraFx {
 public static readonly string[] NAMES={"SHIFT","SPLICE","REVERSE","SHIMMER","WIDTH","PS1 MODE","JITTER","DETUNE","GRAIN TONE","ENVELOPE"};
 public static readonly string[] PRESETS={"HALL","ROOM","STUDIO","SPACE"};
 public static string name(int k){return k>=36?AtmosFx.NAMES[k-36]:NAMES[k-26];}
 public static float get(MasterFx f,int k){if(k>=36)return AtmosFx.get(f,k);switch(k){case 26:return (f.CrystalPitch+12)/24;case 27:return (f.CrystalWindow-.04f)/.21f;case 28:return f.CrystalReverse;case 29:return f.CrystalMix;case 30:return f.CrystalWidth;case 31:return f.PsxPreset/3f;case 32:return f.GrainJitter;case 33:return f.GrainDetune/50;case 34:return f.GrainTone;default:return f.GrainShape;}}
 public static void set(MasterFx f,int k,float value){if(k>=36){AtmosFx.set(f,k,value);return;}float v=Math.Max(0,Math.Min(1,value));switch(k){case 26:f.CrystalPitch=v*24-12;break;case 27:f.CrystalWindow=.04f+v*.21f;break;case 28:f.CrystalReverse=v;break;case 29:f.CrystalMix=v;break;case 30:f.CrystalWidth=v;break;case 31:f.PsxPreset=(int)Math.Round(v*3);break;case 32:f.GrainJitter=v;break;case 33:f.GrainDetune=v*50;break;case 34:f.GrainTone=v;break;case 35:f.GrainShape=v;break;}}
 public static float initial(int k){if(k>=36)return AtmosFx.initial(k);return new float[]{1f,(.128f-.04f)/.21f,1,.82f,1,0,0,0,1,.5f}[k-26];}
 public static string label(MasterFx f,int k){if(k>=36)return AtmosFx.label(f,k);if(k==26)return f.CrystalPitch.ToString("+0.0;-0.0;0");if(k==27)return Math.Round(f.CrystalWindow*1000)+"ms";if(k==31)return PRESETS[f.PsxPreset];if(k==33)return Math.Round(f.GrainDetune)+"ct";if(k==34)return f.GrainTone>.999f?"OPEN":Math.Round(40*Math.Pow(500,f.GrainTone))+"Hz";return Math.Round(get(f,k)*100)+"%";}
 public static float shape(float w,float value){return value<.5f?w+(w*(2-w)-w)*(1-value*2):w+(w*w-w)*(value*2-1);}
 public static int[] controls(int tab,MasterFx f){return tab<0?new int[0]:tab==0?(f.ReverbMode==4?new int[]{43,44,1,19,45,46,47}:f.ReverbMode==3?new int[]{1,19,36,37,38}:f.ReverbMode==2?new int[]{31,1,19}:new int[]{1,19}):tab==1?(f.DelayMode==4?new int[]{3,4,39,40,41,42}:f.DelayMode==3?new int[]{3,4,26,27,28,29,30}:new int[]{3,4}):f.GrainMode==0?new int[]{20,25,6,7,8,32,33,34,35}:f.GrainMode==1?new int[]{20,25,11,12,13,14,15,16,17,32,33,34,35}:new int[]{20,10,11,12,13,14,15,16,17,32,33,34,35};}
}

}
