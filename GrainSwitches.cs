using System;
namespace LoopPedal {
/** Existing grain parameters exposed as detented front-panel switches. */
static class GrainSwitches {
 public static readonly float[] DENSITY={2,4,8,12},POSITION={0,.15f,.5f,1.5f},WINDOW={.04f,.15f,.5f,1.5f};
 public static int count(int kind){return kind<23?2:4;}
 public static float[] values(MasterFx f,int kind){return kind==23?DENSITY:f.GrainMode==2?WINDOW:POSITION;}
 public static string title(MasterFx f,int kind){return kind==21?"DIR":kind==22?"LO-FI":kind==23?"DENSITY":f.GrainMode==2?"WINDOW":"POSITION";}
 public static string[] labels(MasterFx f,int kind){return kind==21?new string[]{"FWD","REV"}:kind==22?new string[]{"OFF","ON"}:kind==23?new string[]{"2","4","8","12"}:new string[]{f.GrainMode==2?"40":"0","150","500","1.5s"};}
 public static float current(MasterFx f,int kind){return kind==21?(f.GrainReverse?1:0):kind==22?(f.CloudLoFi?1:0):kind==23?f.Density:f.GrainMode==2?f.CloudWindow:f.Position;}
 public static int index(MasterFx f,int kind){if(kind<23)return (int)current(f,kind);float value=current(f,kind);float[] options=values(f,kind);for(int i=0;i<options.Length;i++)if(Math.Abs(value-options[i])<.0001f)return i;return -1;}
 public static float normalized(MasterFx f,int kind){if(kind<23)return current(f,kind);float value=current(f,kind);float[] v=values(f,kind);if(value<=v[0])return 0;for(int i=1;i<v.Length;i++)if(value<=v[i])return (i-1+(value-v[i-1])/(v[i]-v[i-1]))/(v.Length-1);return 1;}
 public static void setIndex(MasterFx f,int kind,int index){index=Math.Max(0,Math.Min(count(kind)-1,index));if(kind==21)f.GrainReverse=index==1;else if(kind==22)f.CloudLoFi=index==1;else if(kind==23)f.Density=DENSITY[index];else if(f.GrainMode==2)f.CloudWindow=WINDOW[index];else f.Position=POSITION[index];}
 public static void setNormalized(MasterFx f,int kind,float value){setIndex(f,kind,(int)Math.Round(Math.Max(0,Math.Min(1,value))*(count(kind)-1)));}
}
}
