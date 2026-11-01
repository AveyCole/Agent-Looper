using System;using System.Runtime.InteropServices;
namespace LoopPedal {
// Avoid inaudible subnormal feedback/filter states monopolizing real-time threads.
struct AudioFloatScope:IDisposable {
 [DllImport("msvcrt.dll",CallingConvention=CallingConvention.Cdecl)]static extern uint _controlfp(uint value,uint mask);
 readonly uint previous;readonly bool changed;
 AudioFloatScope(uint before){previous=before;changed=(before&0x03000000)!=0x01000000;if(changed)_controlfp(0x01000000,0x03000000);}
 public static AudioFloatScope Enter(){return new AudioFloatScope(_controlfp(0,0));}
 public void Dispose(){if(changed)_controlfp(previous,0x03000000);}
}
}
