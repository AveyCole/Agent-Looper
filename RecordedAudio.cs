using System;using System.Collections.Concurrent;using System.Threading;
namespace LoopPedal {
// Fixed blocks avoid copying an ever-growing take on the ASIO callback.
static class CaptureBlocks {
 public const int Samples=16384;const int Target=32,Maximum=64;
 static readonly ConcurrentQueue<float[]> spare=new ConcurrentQueue<float[]>();static int count,filling;
 static readonly WaitCallback refill=state=>Fill();public static long EmergencyAllocations;
 static CaptureBlocks(){for(int i=0;i<Target;i++){spare.Enqueue(new float[Samples]);count++;}filling=1;ThreadPool.QueueUserWorkItem(refill);}
 public static void Warm(){}
 static void Fill(){try{while(Volatile.Read(ref count)<Target){var block=new float[Samples];spare.Enqueue(block);Interlocked.Increment(ref count);}}finally{Interlocked.Exchange(ref filling,0);}}
 public static float[] Rent(){float[] block;if(spare.TryDequeue(out block))Interlocked.Decrement(ref count);else{Interlocked.Increment(ref EmergencyAllocations);block=new float[Samples];}if(Volatile.Read(ref count)<Target/2&&Interlocked.CompareExchange(ref filling,1,0)==0)ThreadPool.QueueUserWorkItem(refill);return block;}
 public static void Return(float[] block){if(Interlocked.Increment(ref count)<=Maximum)spare.Enqueue(block);else Interlocked.Decrement(ref count);}
}
sealed class RecordedAudio:IDisposable {
 readonly float[][] blocks;public int Count{get;private set;}
 public RecordedAudio(int maximum){blocks=new float[Math.Max(1,(maximum+CaptureBlocks.Samples-1)/CaptureBlocks.Samples)][];blocks[0]=CaptureBlocks.Rent();}
 public float this[int index]{get{if(index<0||index>=Count)throw new ArgumentOutOfRangeException();return blocks[index/CaptureBlocks.Samples][index%CaptureBlocks.Samples];}}
 public void Add(float value){int block=Count/CaptureBlocks.Samples,index=Count%CaptureBlocks.Samples;if(blocks[block]==null)blocks[block]=CaptureBlocks.Rent();blocks[block][index]=value;Count++;}
 public void CopyTo(int sourceIndex,float[] output,int outputIndex,int count){if(sourceIndex<0||count<0||sourceIndex+count>Count)throw new ArgumentOutOfRangeException();while(count>0){int block=sourceIndex/CaptureBlocks.Samples,offset=sourceIndex%CaptureBlocks.Samples,take=Math.Min(count,CaptureBlocks.Samples-offset);Array.Copy(blocks[block],offset,output,outputIndex,take);sourceIndex+=take;outputIndex+=take;count-=take;}}
 public float[] ToArray(){var output=new float[Count];CopyTo(0,output,0,Count);return output;}
 public void Dispose(){for(int i=0;i<blocks.Length;i++)if(blocks[i]!=null){CaptureBlocks.Return(blocks[i]);blocks[i]=null;}}
}
sealed partial class Engine {public Engine(){CaptureBlocks.Warm();}}
}
