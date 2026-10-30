using System;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using NAudio.Wave;

namespace LoopPedal {
static class QualityRender {
 static readonly SemaphoreSlim jobs=new SemaphoreSlim(2,2);
 public static float[] Render(float[] input,double stretch,double semitones,bool reverse,CancellationToken cancel,bool formants=false){
  cancel.ThrowIfCancellationRequested();if(input.Length==0)return input;
  if(reverse){var backwards=new float[input.Length];for(int i=0,n=input.Length/2;i<n;i++){backwards[i*2]=input[(n-1-i)*2];backwards[i*2+1]=input[(n-1-i)*2+1];}input=backwards;}
  if(Math.Abs(stretch-1)<1e-12&&Math.Abs(semitones)<1e-12)return input;
  string executable=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"tools","rubberband-r3.exe");if(!File.Exists(executable))throw new FileNotFoundException("High-quality processing requires tools/rubberband-r3.exe.");
  jobs.Wait(cancel);string folder=null;try{
   folder=Path.Combine(Path.GetTempPath(),"AgentLooper-render-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);string source=Path.Combine(folder,"source.wav"),target=Path.Combine(folder,"rendered.wav");int count=Math.Max(1,(int)(input.Length/2*stretch));int repeats=Math.Max(3,(int)Math.Ceiling(24576.0/Math.Max(1,input.Length/2)));if(repeats%2==0)repeats++;DSP.Wav(source,input,repeats,cancel);
   string arguments="-3 -q --ignore-clipping --centre-focus --pitch-hq --time "+stretch.ToString("R",CultureInfo.InvariantCulture)+" --pitch "+semitones.ToString("R",CultureInfo.InvariantCulture)+(formants?" --formant":"")+" \""+source+"\" \""+target+"\"";
   var info=new ProcessStartInfo(executable,arguments){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};
   using(var process=Process.Start(info))using(cancel.Register(()=>{try{if(!process.HasExited)process.Kill();}catch{}})){
    var errors=process.StandardError.ReadToEndAsync();var messages=process.StandardOutput.ReadToEndAsync();process.WaitForExit();cancel.ThrowIfCancellationRequested();string error=errors.GetAwaiter().GetResult();messages.GetAwaiter().GetResult();if(process.ExitCode!=0)throw new Exception("R3 pitch render · "+error);
   }
   using(var reader=new WaveFileReader(target)){if(reader.WaveFormat.BitsPerSample!=32||reader.WaveFormat.AsStandardWaveFormat().Encoding!=WaveFormatEncoding.IeeeFloat)throw new Exception("R3 did not preserve floating-point audio.");reader.Position=(long)(repeats/2)*count*reader.WaveFormat.BlockAlign;var result=new float[count*2];byte[] bytes=new byte[65536];int offset=0;while(offset<result.Length){cancel.ThrowIfCancellationRequested();int wanted=Math.Min(bytes.Length,(result.Length-offset)*4);int read=reader.Read(bytes,0,wanted);if(read==0)throw new Exception("R3 render ended before the loop's requested duration.");System.Buffer.BlockCopy(bytes,0,result,offset*4,read);offset+=read/4;}return result;}
  }finally{if(folder!=null){foreach(string name in new[]{"source.wav","rendered.wav"})try{string path=Path.Combine(folder,name);if(File.Exists(path))File.Delete(path);}catch{}try{Directory.Delete(folder);}catch{}}jobs.Release();}
 }
}
}

