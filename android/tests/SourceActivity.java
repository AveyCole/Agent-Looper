package com.aveycole.agentlooper.source;
import android.app.Activity;
import android.os.Bundle;
import android.media.*;
public final class SourceActivity extends Activity {
    volatile boolean running=true;
    Thread thread;
    @Override public void onCreate(Bundle state){super.onCreate(state);android.widget.TextView label=new android.widget.TextView(this);label.setText("Agent Looper test source · 440 Hz");label.setTextSize(24);setContentView(label);thread=new Thread(()->{AudioTrack output=new AudioTrack.Builder().setAudioAttributes(new AudioAttributes.Builder().setUsage(AudioAttributes.USAGE_MEDIA).setContentType(AudioAttributes.CONTENT_TYPE_MUSIC).setAllowedCapturePolicy(AudioAttributes.ALLOW_CAPTURE_BY_ALL).build()).setAudioFormat(new AudioFormat.Builder().setSampleRate(48000).setEncoding(AudioFormat.ENCODING_PCM_FLOAT).setChannelMask(AudioFormat.CHANNEL_OUT_STEREO).build()).setBufferSizeInBytes(8192).setTransferMode(AudioTrack.MODE_STREAM).build();output.play();float[] samples=new float[512];long frame=0;while(running){for(int i=0;i<256;i++,frame++)samples[i*2]=samples[i*2+1]=(float)(.2*Math.sin(2*Math.PI*440*frame/48000));output.write(samples,0,samples.length,AudioTrack.WRITE_BLOCKING);}output.stop();output.release();});thread.start();}
    @Override public void onDestroy(){running=false;super.onDestroy();}
}
