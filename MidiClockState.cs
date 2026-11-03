using System;namespace LoopPedal {
sealed class MidiClockState {
 public volatile int mode;public double bpm=120;public long last;readonly long[] pulses=new long[25];int count,index,pending,waiting;long serial;
 public void receive(int value,long now){lock(this){if(mode!=1)return;if(value==250||value==251){waiting=value;count=index=0;}else if(value==252){pending=252;waiting=0;}else if(value==248){if(waiting!=0){pending=waiting;waiting=0;serial++;}if(last!=0&&(now<=last||now-last>500000000L)){count=index=0;}pulses[index]=now;int old=(index+1)%25;if(count>=24){long span=now-pulses[old];if(span>0){double next=60000000000.0/span;if(next>=20&&next<=300)bpm=count==24?next:bpm+(next-bpm)*.15;}}index=(index+1)%25;count=Math.Min(1000,count+1);last=now;}}}
 public int command(){lock(this){int value=pending;pending=0;return value;}}
 public bool locked(long now){return mode==1&&count>=24&&now-last<750000000L;}
 public void reset(int next){lock(this){mode=next;pending=waiting=count=index=0;last=0;}}
}
}
