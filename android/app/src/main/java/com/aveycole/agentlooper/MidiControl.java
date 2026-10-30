package com.aveycole.agentlooper;
import android.content.*;
import android.media.midi.*;
import android.os.*;
import java.io.*;
import java.util.*;

final class MidiControl implements AutoCloseable {
    final MainActivity host;
    final MidiManager manager;
    final android.content.SharedPreferences prefs;
    final HashMap<Integer,Integer> bindings=new HashMap<>();
    final boolean[] down=new boolean[16*256];
    MidiDevice device;
    MidiOutputPort port;
    int learning=-1,status,first=-1;
    final Handler handler=new Handler(Looper.getMainLooper());
    MidiControl(MainActivity h){host=h;manager=(MidiManager)h.getSystemService(Context.MIDI_SERVICE);prefs=h.getSharedPreferences("midi",0);for(int target=0;target<10;target++){int key=prefs.getInt("target"+target,-1);if(key>=0)bindings.put(key,target);}}
    void menu(){if(manager==null){host.engine.message="MIDI unavailable";return;}MidiDeviceInfo[] devices=manager.getDevices();ArrayList<MidiDeviceInfo> inputs=new ArrayList<>();ArrayList<String> names=new ArrayList<>();names.add("OFF");for(MidiDeviceInfo d:devices)if(d.getOutputPortCount()>0){inputs.add(d);names.add(d.getProperties().getString(MidiDeviceInfo.PROPERTY_NAME,"MIDI "+d.getId()));}host.choice("MIDI input",names.toArray(new String[0]),-1,n->{close();if(n>0)manager.openDevice(inputs.get(n-1),opened->{if(opened==null){host.engine.message="MIDI open failed";return;}device=opened;for(MidiDeviceInfo.PortInfo info:opened.getInfo().getPorts())if(info.getType()==MidiDeviceInfo.PortInfo.TYPE_OUTPUT){port=opened.openOutputPort(info.getPortNumber());break;}if(port!=null){port.connect(receiver);host.engine.message="Hold a transport to learn MIDI";}},handler);});}
    void learn(int target){if(port==null){host.engine.message="Select a MIDI input first";menu();return;}host.choice("MIDI · "+(target/5==0?"A":"B")+" "+new String[]{"RECORD","OVERDUB","PLAY / STOP","UNDO","AUTO OD"}[target%5],new String[]{"LEARN","CLEAR"},-1,n->{if(n==0){learning=target;Arrays.fill(down,false);host.engine.message="Press a MIDI note or CC";}else{bindings.values().removeIf(v->v==target);prefs.edit().remove("target"+target).apply();host.engine.message="MIDI assignment cleared";}});}
    final MidiReceiver receiver=new MidiReceiver(){@Override public void onSend(byte[] bytes,int offset,int count,long timestamp){for(int i=offset;i<offset+count;i++){int v=bytes[i]&255;if(v>=248)continue;if(v>=128){status=v;first=-1;if(v>=240)status=0;continue;}if(status==0)continue;int kind=status&240;if(kind==192||kind==208)continue;if(first<0){first=v;continue;}int number=first;first=-1;int channel=status&15;if(kind==128)kind=144;if(kind!=144&&kind!=176)continue;int key=(kind<<16)|(channel<<8)|number;boolean pressed=(status&240)!=128&&v>0;int edge=channel*256+number+(kind==176?128:0);boolean rising=pressed&&!down[edge];down[edge]=pressed;if(!rising)continue;handler.post(()->event(key));}}};
    void event(int key){if(learning>=0){int target=learning;learning=-1;Integer old=bindings.get(key);if(old!=null)prefs.edit().remove("target"+old).apply();bindings.values().removeIf(v->v==target);bindings.put(key,target);prefs.edit().putInt("target"+target,key).apply();host.engine.message="MIDI assigned";return;}Integer target=bindings.get(key);if(target!=null)host.transport(target/5,target%5);}
    @Override public void close(){try{if(port!=null)port.close();if(device!=null)device.close();}catch(IOException ignored){}port=null;device=null;status=0;first=-1;Arrays.fill(down,false);}
}
