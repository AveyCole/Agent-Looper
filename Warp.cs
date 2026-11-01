using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
namespace LoopPedal {
sealed partial class Engine {
 public bool WarpLayers,WarpPending;public int WarpFrames,WarpRevision;long warpAt;Layer[] warpLayers;float[][] warpAudio;readonly Queue<LiveStretch> retiredWarp=new Queue<LiveStretch>();
 public double EffectiveStretch(Layer layer){return WarpLayers?WarpFrames/(double)Math.Max(1,(int)Math.Round(layer.Original.Length/2*layer.TrimLength)):layer.Stretch;}
 public void StageWarp(Layer[] layers,float[][] audio,int frames){lock(Gate){WarpFrames=frames;warpLayers=layers;warpAudio=audio;long phase=((Frame-LoopOrigin)%frames+frames)%frames;warpAt=Frame+frames-phase;WarpPending=true;if(!Playing)CommitWarpIfDue();}}
 void CommitWarpIfDue(){if(!WarpPending||Playing&&Frame<warpAt)return;WarpLayers=true;WarpPending=false;LoopFrames=WarpFrames;for(int i=0;i<warpLayers.Length;i++){var layer=warpLayers[i];if(!Layers.Contains(layer))continue;if(layer.Stream!=null){retiredWarp.Enqueue(layer.Stream);layer.Stream=null;}layer.LiveWet=0;layer.PreviousAudio=layer.Audio;layer.PreviousPhaseDelta=LoopOrigin-layer.Offset;layer.Transition=960;layer.Audio=warpAudio[i];layer.LivePlaybackFrames=WarpFrames;}warpLayers=null;warpAudio=null;}
 public void RetireStream(LiveStretch stream){if(stream==null)return;stream.RequestStop();lock(Gate)retiredWarp.Enqueue(stream);}
 public void RetireWarpStreams(bool wait=false){LiveStretch[] old;lock(Gate){if(retiredWarp.Count==0)return;old=retiredWarp.ToArray();retiredWarp.Clear();}foreach(var stream in old)stream.RequestStop();if(wait){foreach(var stream in old)stream.Dispose();}else System.Threading.ThreadPool.QueueUserWorkItem(_=>{foreach(var stream in old)stream.Dispose();});}
 public void CancelWarp(){lock(Gate){WarpRevision++;WarpPending=false;WarpLayers=false;warpLayers=null;warpAudio=null;}}
}
partial class Pedal {
 PadButton[] warpChips=new PadButton[2];
 public void VerifyWarpUi(){PreviewLayers();var bus=loopers[0];bus.Playing=false;int cycle=bus.LoopFrames;var layer=bus.Layers[0];layer.TrimLength=.5;layer.Pitch=5;bus.Layers[1].Stretch=1.5;ToggleWarp(0);DateTime deadline=DateTime.UtcNow.AddSeconds(20);while(!bus.WarpLayers&&DateTime.UtcNow<deadline){System.Windows.Forms.Application.DoEvents();Thread.Sleep(10);}if(!bus.WarpLayers||bus.Layers.Any(t=>t.Audio.Length/2!=cycle)||layer.Pitch!=5||bus.Layers[1].Stretch!=1.5)throw new Exception("Warp UI/render did not preserve settings and common duration: "+status.Text);layer.Pitch=-3;Process(layer);if(layer.Stream==null||layer.LivePlaybackFrames!=cycle||bus.LoopFrames!=cycle)throw new Exception("Live edit broke warp duration");ToggleWarp(0);if(bus.WarpLayers||layer.Pitch!=-3||layer.TrimLength!=.5||layer.LivePlaybackFrames!=cycle/2)throw new Exception("Unlock did not restore independent duration");}
 async void ToggleWarp(int bank){var bus=loopers[bank];if(bus.Destructive)return;SelectBank(bank);if(bus.WarpLayers||bus.WarpPending){bus.CancelWarp();foreach(var layer in bus.Layers.ToArray())Process(layer);return;}if(bus.Layers.Count==0){status.Text="Record a loop first.";return;}if(bus.Recording)bus.Finish(false);Layer[] layers;int[] revisions;int cycle;int revision=++bus.WarpRevision;lock(bus.Gate){layers=bus.Layers.ToArray();revisions=layers.Select(x=>x.Revision).ToArray();cycle=bus.LoopFrames;}if(cycle<1)return;status.Text="Warping layers…";try{var audio=await Task.Run(()=>layers.Select(t=>{var source=DSP.Region(t.Original,t.TrimStart,t.TrimLength);var rendered=DSP.Render(source,cycle/(double)(source.Length/2),t.Pitch,t.Reverse,default(CancellationToken),t.PreserveFormants);if(rendered.Length!=cycle*2)Array.Resize(ref rendered,cycle*2);return rendered;}).ToArray());if(revision!=bus.WarpRevision)return;for(int i=0;i<layers.Length;i++)if(layers[i].Revision!=revisions[i]){status.Text="Layer changed. Tap Warp again.";return;}bus.StageWarp(layers,audio,cycle);status.Text=bus.WarpPending?"Warp armed for next loop.":"";RefreshTracks(true);}catch(Exception e){status.Text=e.Message;}}
}
}
