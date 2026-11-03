using System;namespace LoopPedal {
static class LfoMath {
 public static readonly string[] TYPES={"Sine","Triangle","Input envelope","Square","Saw","Sample & hold"};
 public static readonly string[] POLARITIES={"Bipolar","Positive","Negative"};
 public static double value(PerformanceTimeline.Mod m,long time,double period,float envelope){double cycle=Math.Floor(time/period+m.phase),p=time/period+m.phase-cycle;double raw=m.type==2?envelope*2-1:m.type==1?1-4*Math.Abs(p-.5):m.type==3?(p<.5?1:-1):m.type==4?p*2-1:m.type==5?random(cycle,m.seed*101):Math.Sin(p*2*Math.PI);return m.polarity==1?(raw+1)*.5:m.polarity==2?(raw-1)*.5:raw;}
 public static double random(double cycle,int seed){double n=Math.Sin(cycle*12.9898+seed*78.233)*43758.5453;return (n-Math.Floor(n))*2-1;}
}
}
