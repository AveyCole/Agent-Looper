using System;
using System.Linq;
using NAudio.Wave;
using NAudio.CoreAudioApi;
namespace LoopPedal {
sealed class CaptureFormat {
 public WaveFormat Format;public AudioClientShareMode Mode;
 public static CaptureFormat Find(MMDevice device){var client=device.AudioClient;var native=client.MixFormat;if(client.IsFormatSupported(AudioClientShareMode.Shared,native.AsStandardWaveFormat()))return new CaptureFormat{Format=native,Mode=AudioClientShareMode.Shared};foreach(int rate in new[]{48000,44100,96000})foreach(int channels in new[]{native.Channels,2,1}.Distinct())foreach(int bits in new[]{32,24,16}){WaveFormat format=channels>2?(WaveFormat)new WaveFormatExtensible(rate,bits,channels):new WaveFormat(rate,bits,channels);if(client.IsFormatSupported(AudioClientShareMode.Exclusive,format))return new CaptureFormat{Format=format,Mode=AudioClientShareMode.Exclusive};}if(client.IsFormatSupported(AudioClientShareMode.Shared,native))return new CaptureFormat{Format=native,Mode=AudioClientShareMode.Shared};throw new NotSupportedException("No usable audio format: "+device.FriendlyName);}
}
sealed class CompatibleCapture:WasapiCapture {
 public CompatibleCapture(MMDevice device,CaptureFormat plan):base(device,true,40){ShareMode=plan.Mode;WaveFormat=plan.Format;}
 protected override AudioClientStreamFlags GetAudioClientStreamFlags(){return AudioClientStreamFlags.None;}
}
// Decode the endpoint's negotiated format rather than asking a USB driver to accept stereo PCM16.
sealed class NativeCapture {
 readonly WaveFormat format;readonly int channels;readonly double step;NAudio.Dsp.WdlResampler resampler;float[] decoded=new float[0],output=new float[0];
 public NativeCapture(WaveFormat f){format=f.AsStandardWaveFormat();channels=f.Channels;step=f.SampleRate/(double)DSP.Rate;if(f.SampleRate!=DSP.Rate){resampler=new NAudio.Dsp.WdlResampler();resampler.SetMode(false,0,true,128,32);resampler.SetFeedMode(true);resampler.SetRates(f.SampleRate,DSP.Rate);}}
 public float[] Convert(byte[] bytes,int length,out int count){int sampleCount=length/(format.BitsPerSample/8);if(decoded.Length<sampleCount)decoded=new float[sampleCount];int bits=format.BitsPerSample;bool floating=format.Encoding==WaveFormatEncoding.IeeeFloat;
 if(floating&&bits==32)System.Buffer.BlockCopy(bytes,0,decoded,0,sampleCount*4);else for(int i=0,offset=0;i<sampleCount;i++,offset+=bits/8){if(floating&&bits==64)decoded[i]=(float)BitConverter.ToDouble(bytes,offset);else if(!floating&&bits==16)decoded[i]=BitConverter.ToInt16(bytes,offset)/32768f;else if(!floating&&bits==24){int v=bytes[offset]|(bytes[offset+1]<<8)|(bytes[offset+2]<<16);if((v&0x800000)!=0)v|=unchecked((int)0xff000000);decoded[i]=v/8388608f;}else if(!floating&&bits==32)decoded[i]=(float)(BitConverter.ToInt32(bytes,offset)/2147483648.0);else if(!floating&&bits==8)decoded[i]=(bytes[offset]-128)/128f;else throw new NotSupportedException("Input format: "+format);}
 if(format.SampleRate==DSP.Rate){count=sampleCount;return decoded;}int frames=sampleCount/channels;int capacity=((int)Math.Ceiling((frames+256)/step)+256)*channels;if(output.Length<capacity)output=new float[capacity];float[] prepared;int preparedOffset;int wanted=resampler.ResamplePrepare(frames,channels,out prepared,out preparedOffset);if(wanted!=frames)throw new Exception("Unexpected resampler frame count");Array.Copy(decoded,0,prepared,preparedOffset,sampleCount);count=resampler.ResampleOut(output,0,frames,output.Length/channels,channels)*channels;return output;} public float[] Flush(out int count){if(resampler==null){count=0;return new float[0];}float[] prepared;int offset;resampler.ResamplePrepare(256,channels,out prepared,out offset);var tail=new float[((int)Math.Ceiling(512/step)+512)*channels];count=resampler.ResampleOut(tail,0,0,tail.Length/channels,channels)*channels;return tail;}
}
}







