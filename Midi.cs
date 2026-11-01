using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Xml;

namespace LoopPedal {
sealed class MidiInput:IDisposable {
 delegate void Callback(IntPtr handle,uint message,IntPtr instance,UIntPtr data,UIntPtr timestamp);
 [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Auto)]struct Caps{public ushort manufacturer,product;public uint version;[MarshalAs(UnmanagedType.ByValTStr,SizeConst=32)]public string name;public uint support;}
 [DllImport("winmm.dll")]static extern uint midiInGetNumDevs();
 [DllImport("winmm.dll",CharSet=CharSet.Auto)]static extern uint midiInGetDevCaps(UIntPtr device,out Caps caps,uint size);
 [DllImport("winmm.dll")]static extern uint midiInOpen(out IntPtr handle,uint device,Callback callback,IntPtr instance,uint flags);
 [DllImport("winmm.dll")]static extern uint midiInStart(IntPtr handle);
 [DllImport("winmm.dll")]static extern uint midiInStop(IntPtr handle);
 [DllImport("winmm.dll")]static extern uint midiInReset(IntPtr handle);
 [DllImport("winmm.dll")]static extern uint midiInClose(IntPtr handle);
 readonly ConcurrentQueue<int> queue=new ConcurrentQueue<int>();readonly Callback callback;IntPtr handle;volatile bool closing;
 public static string[] Names(){var names=new List<string>();for(uint i=0;i<midiInGetNumDevs();i++){Caps c;if(midiInGetDevCaps((UIntPtr)i,out c,(uint)Marshal.SizeOf(typeof(Caps)))==0)names.Add(c.name);else names.Add("MIDI "+(i+1));}return names.ToArray();}
 public MidiInput(int device){callback=Receive;uint error=midiInOpen(out handle,(uint)device,callback,IntPtr.Zero,0x30000);if(error!=0)throw new Exception("MIDI input unavailable ("+error+").");error=midiInStart(handle);if(error!=0){Dispose();throw new Exception("Could not start MIDI input ("+error+").");}}
 void Receive(IntPtr h,uint message,IntPtr instance,UIntPtr data,UIntPtr time){if(closing||message!=0x3c3)return;int packed=unchecked((int)data.ToUInt64());int kind=packed&0xf0;if((kind==0x80||kind==0x90||kind==0xb0)&&queue.Count<2048)queue.Enqueue(packed);}
 public bool TryRead(out int data){return queue.TryDequeue(out data);}
 public void Dispose(){closing=true;if(handle!=IntPtr.Zero){midiInStop(handle);midiInReset(handle);midiInClose(handle);handle=IntPtr.Zero;}}
}
sealed class MidiBinding {
 public int Kind,Channel,Number;
 public bool Matches(int kind,int channel,int number){return Kind==kind&&Channel==channel&&Number==number;}
 public override string ToString(){return (Kind==0x90?"NOTE ":"CC ")+Number+" / CH "+(Channel+1);}
}
sealed class MidiMap {
 public readonly Dictionary<string,MidiBinding> Bindings=new Dictionary<string,MidiBinding>();
 public string LearnTarget,DeviceName="";public Action<string> Trigger,Learned;public Action<string,int> Value;
 public static bool Continuous(string action){return action!=null&&(action.StartsWith("FX.")||action.Contains(".Layer.")||action=="MASTER");}
 readonly HashSet<int> highCC=new HashSet<int>();readonly Dictionary<int,long> lastNote=new Dictionary<int,long>();
 public void Handle(int packed){int kind=packed&0xf0,channel=packed&15,number=(packed>>8)&127,value=(packed>>16)&127,key=(channel<<8)|number;
 if(kind!=0xb0&&kind!=0x90&&kind!=0x80)return;
 bool controlLearn=Continuous(LearnTarget);if(controlLearn&&kind!=0xb0)return;
 if(string.IsNullOrEmpty(LearnTarget)){foreach(var map in Bindings)if(map.Value.Matches(kind,channel,number)&&Continuous(map.Key)){if(kind==0xb0&&Value!=null)Value(map.Key,value);return;}}
 if(!controlLearn){if(kind==0xb0){bool previous=highCC.Contains(key);if(value>=64)highCC.Add(key);else highCC.Remove(key);if(value<64||previous)return;}else if(kind==0x90&&value>0){long now=DateTime.UtcNow.Ticks,previous;if(lastNote.TryGetValue(key,out previous)&&now-previous<TimeSpan.TicksPerMillisecond*35)return;lastNote[key]=now;}else return;}
 if(!string.IsNullOrEmpty(LearnTarget)){string action=LearnTarget;var duplicate=new List<string>();foreach(var map in Bindings)if(map.Value.Matches(kind,channel,number))duplicate.Add(map.Key);foreach(var old in duplicate)Bindings.Remove(old);Bindings[action]=new MidiBinding{Kind=kind,Channel=channel,Number=number};LearnTarget=null;if(Learned!=null)Learned(action);return;}
 foreach(var map in Bindings)if(map.Value.Matches(kind,channel,number)){if(Trigger!=null)Trigger(map.Key);return;}
 }
 public void ResetEdges(){highCC.Clear();lastNote.Clear();}
 public void Save(string path){using(var writer=XmlWriter.Create(path,new XmlWriterSettings{Indent=true})){writer.WriteStartElement("StrataMidi");writer.WriteAttributeString("device",DeviceName??"");foreach(var map in Bindings){writer.WriteStartElement("Binding");writer.WriteAttributeString("action",map.Key);writer.WriteAttributeString("kind",map.Value.Kind.ToString());writer.WriteAttributeString("channel",map.Value.Channel.ToString());writer.WriteAttributeString("number",map.Value.Number.ToString());writer.WriteEndElement();}writer.WriteEndElement();}}
 public void Load(string path){var document=new XmlDocument{XmlResolver=null};document.Load(path);var root=document.DocumentElement;if(root==null||root.Name!="StrataMidi")return;Bindings.Clear();DeviceName=root.GetAttribute("device");foreach(XmlNode node in root.ChildNodes){var e=node as XmlElement;if(e==null||e.Name!="Binding")continue;int kind,channel,number;if(int.TryParse(e.GetAttribute("kind"),out kind)&&int.TryParse(e.GetAttribute("channel"),out channel)&&int.TryParse(e.GetAttribute("number"),out number)&&(kind==0x90||kind==0xb0)&&channel>=0&&channel<16&&number>=0&&number<128)Bindings[e.GetAttribute("action")]=new MidiBinding{Kind=kind,Channel=channel,Number=number};}}
}
}

