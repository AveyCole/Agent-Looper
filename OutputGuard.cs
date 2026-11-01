using System;
namespace LoopPedal {
sealed class OutputGuard {
 double gain=1;
 public void Process(float l,float r,out float left,out float right){if(float.IsNaN(l)||float.IsInfinity(l))l=0;if(float.IsNaN(r)||float.IsInfinity(r))r=0;double peak=Math.Max(Math.Abs(l),Math.Abs(r));double wanted=peak>.95?.95/peak:1;gain+=(wanted-gain)*(wanted<gain?.15:.00008);if(1-gain<1e-7)gain=1;double after=peak*gain,scale=gain;if(after>.95){double excess=(after-.95)/.05;double ceiling=.95+.05*excess/(1+excess);scale*=ceiling/after;}left=(float)(l*scale);right=(float)(r*scale);}
}
}
