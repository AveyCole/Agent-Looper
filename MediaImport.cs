using System;
using System.IO;
using System.Collections.Generic;
using NAudio.Wave;
namespace LoopPedal {
static class MediaImport {
 public static float[] Read(string path){
  if(string.Equals(Path.GetExtension(path),".wav",StringComparison.OrdinalIgnoreCase)){
   try{using(var wav=new WaveFileReader(path))return Decode(wav,false);}catch(FormatException){}catch(NotSupportedException){}
  }
  WaveStream reader;
  try{reader=new MediaFoundationReader(path,new MediaFoundationReader.MediaFoundationReaderSettings{RequestFloatOutput=true,SingleReaderObject=true});}
  catch{try{reader=new MediaFoundationReader(path,new MediaFoundationReader.MediaFoundationReaderSettings{SingleReaderObject=true});}catch(Exception ex){throw new InvalidDataException("Cannot read an audio track. Choose a supported audio file or video with audio.",ex);}}
  using(reader)return Decode(reader,true);
 }
 static float[] Decode(WaveStream reader,bool encoded){
  var format=reader.WaveFormat;var extra=format as WaveFormatExtraData;if(format.Encoding==WaveFormatEncoding.Extensible&&extra!=null&&extra.ExtraSize>=22){byte[] guid=new byte[16];Array.Copy(extra.ExtraData,2,guid,0,16);var sub=new Guid(guid);if(sub==new Guid("00000003-0000-0010-8000-00aa00389b71"))format=WaveFormat.CreateIeeeFloatWaveFormat(format.SampleRate,format.Channels);else if(sub==new Guid("00000001-0000-0010-8000-00aa00389b71"))format=new WaveFormat(format.SampleRate,format.BitsPerSample,format.Channels);else throw new NotSupportedException("Unsupported WAV encoding.");}if(format.Channels<1||format.Channels>2)throw new NotSupportedException("Choose mono or stereo audio.");
  var decoder=new NativeCapture(format);var samples=new List<float>();int limit=DSP.Rate*600*2;int padding=encoded?DSP.Rate/10:0;long sourceFrames=0;
  byte[] bytes=new byte[Math.Max(format.BlockAlign,65536/format.BlockAlign*format.BlockAlign)];int read;
  while((read=reader.Read(bytes,0,bytes.Length))>0){if(read%format.BlockAlign!=0)throw new InvalidDataException("Incomplete audio frame.");sourceFrames+=read/format.BlockAlign;if(sourceFrames>format.SampleRate*600L+(encoded?format.SampleRate/10:0))throw new InvalidDataException("Import limit: 10 minutes. Choose a shorter clip.");int count;var data=decoder.Convert(bytes,read,out count);Append(samples,data,count,format.Channels,limit+padding*2);}
  int tailCount;var tail=decoder.Flush(out tailCount);Append(samples,tail,tailCount,format.Channels,limit+padding*2+2048);
  int expected=Math.Min(limit,(int)Math.Round(sourceFrames*(double)DSP.Rate/format.SampleRate)*2);if(expected==0)throw new InvalidDataException("The audio track is empty.");
  var audio=new float[expected];samples.CopyTo(0,audio,0,Math.Min(samples.Count,audio.Length));return audio;
 }
 static void Append(List<float> result,float[] data,int count,int channels,int limit){if(result.Count+count/channels*2>limit)throw new InvalidDataException("Import limit: 10 minutes.");for(int i=0;i<count;i+=channels){float l=data[i],r=data[i+(channels==1?0:1)];if(float.IsNaN(l)||float.IsInfinity(l)||float.IsNaN(r)||float.IsInfinity(r))throw new InvalidDataException("Invalid audio samples.");result.Add(l);result.Add(r);}}
 public static void Check(string directory){
  var report=new List<string>();try{foreach(string file in new[]{"tone.wav","tone.mp3","tone.m4a","tone.flac","tone.mp4"}){var audio=Read(Path.Combine(directory,file));if(Math.Abs(audio.Length/2-96000)>4800)throw new Exception(file+" duration: "+audio.Length/2);double l=0,r=0;int crosses=0;for(int i=12000;i<84000;i++){l+=audio[i*2]*audio[i*2];r+=audio[i*2+1]*audio[i*2+1];if(audio[(i-1)*2]<=0&&audio[i*2]>0)crosses++;}l=Math.Sqrt(l/72000);r=Math.Sqrt(r/72000);if(Math.Abs(l-.3/Math.Sqrt(2))>.025||Math.Abs(r-.1/Math.Sqrt(2))>.025||Math.Abs(crosses/1.5-440)>4)throw new Exception(file+" level/stereo/frequency: "+l+", "+r+", "+crosses/1.5);report.Add("PASS "+file+": duration, 44.1→48 kHz, stereo level, 440 Hz");}
   foreach(string file in new[]{"ten.wav","ten.mp3"}){var audio=Read(Path.Combine(directory,file));if(audio.Length!=DSP.Rate*600*2)throw new Exception("Ten-minute duration: "+file+" "+audio.Length/2);double end=0;for(int i=audio.Length-96000;i<audio.Length;i+=2)end+=audio[i]*audio[i];if(end<10)throw new Exception("Missing end audio: "+file);report.Add("PASS "+file+": complete ten-minute import");audio=null;GC.Collect();}
   foreach(string file in new[]{"silent.mp4","corrupt.mp3","over.mp3"}){bool failed=false;try{Read(Path.Combine(directory,file));}catch{failed=true;}if(!failed)throw new Exception("Expected failure: "+file);report.Add("PASS clean rejection: "+file);}
  }catch(Exception ex){report.Add("FAIL "+ex);Environment.ExitCode=1;}File.WriteAllLines(Path.Combine(directory,"windows-results.txt"),report);
 }
}
}
