using System;namespace LoopPedal {
static class AtmosFx {
 public static readonly string[] NAMES={"ATTACK","DIFFUSION","MOTION","DRIVE","WEAR","TAPE TONE","FLUTTER","ROOT","SCALE","SPREAD","EXCITE","DETUNE"};
 public static readonly string[] SCALES={"HARMONIC","MINOR","MAJOR","PENTATONIC","FIFTHS","CHROMATIC"};
 public static string targetName(int k){return (k<39?"BLOOM · ":k<43?"TAPE · ":"TUNED · ")+NAMES[k-36];}
 public static float get(MasterFx f,int k){switch(k){case 36:return f.BloomAttack;case 37:return f.BloomDiffusion;case 38:return f.BloomMotion;case 39:return f.TapeDrive;case 40:return f.TapeWear;case 41:return f.TapeTone;case 42:return f.TapeFlutter;case 43:return f.ResonRoot;case 44:return f.ResonScale/5f;case 45:return f.ResonSpread;case 46:return f.ResonExcite;case 47:return f.ResonDetune;default:return 0;}}
 public static void set(MasterFx f,int k,float v){v=Math.Max(0,Math.Min(1,v));switch(k){case 36:f.BloomAttack=v;break;case 37:f.BloomDiffusion=v;break;case 38:f.BloomMotion=v;break;case 39:f.TapeDrive=v;break;case 40:f.TapeWear=v;break;case 41:f.TapeTone=v;break;case 42:f.TapeFlutter=v;break;case 43:f.ResonRoot=v;break;case 44:f.ResonScale=(int)Math.Floor(v*5+.5);break;case 45:f.ResonSpread=v;break;case 46:f.ResonExcite=v;break;case 47:f.ResonDetune=v;break;}}
 public static void reset(MasterFx f){f.BloomAttack=.1854f;f.BloomDiffusion=.75f;f.BloomMotion=.3f;f.TapeDrive=.3f;f.TapeWear=.25f;f.TapeTone=.65f;f.TapeFlutter=.2f;f.ResonRoot=.4f;f.ResonScale=1;f.ResonSpread=.7f;f.ResonExcite=.35f;f.ResonDetune=.2f;}
 public static void copy(MasterFx f,MasterFx n){n.BloomAttack=f.BloomAttack;n.BloomDiffusion=f.BloomDiffusion;n.BloomMotion=f.BloomMotion;n.TapeDrive=f.TapeDrive;n.TapeWear=f.TapeWear;n.TapeTone=f.TapeTone;n.TapeFlutter=f.TapeFlutter;n.ResonRoot=f.ResonRoot;n.ResonScale=f.ResonScale;n.ResonSpread=f.ResonSpread;n.ResonExcite=f.ResonExcite;n.ResonDetune=f.ResonDetune;}
 public static float initial(int k){return new float[]{.1854f,.75f,.3f,.3f,.25f,.65f,.2f,.4f,.2f,.7f,.35f,.2f}[k-36];}
 public static string label(MasterFx f,int k){if(k==36)return Math.Round((.02+f.BloomAttack*1.78)*1000)+"ms";if(k==41)return Math.Round(600*Math.Pow(25,f.TapeTone))+"Hz";if(k==43){int note=24+(int)Math.Floor(f.ResonRoot*60+.5);return new string[]{"C","C#","D","D#","E","F","F#","G","G#","A","A#","B"}[note%12]+(note/12-1);}if(k==44)return SCALES[Math.Max(0,Math.Min(5,f.ResonScale))];if(k==47)return Math.Round(f.ResonDetune*25)+"ct";return Math.Round(get(f,k)*100)+"%";}
}
}
