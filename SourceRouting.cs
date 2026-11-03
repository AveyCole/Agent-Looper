namespace LoopPedal { static class SourceRouting {
        public static bool UsesLive(int mode){return mode==0||(mode&1)==0;}
        public static bool Other(int mode){return mode>=1&&mode<=4;}
        public static string Label(int bank,int mode){string peer=bank==0?"B":"A";string[] names={"Live","Looper "+peer+" · pre-FX","Live + "+peer+" · pre-FX","Looper "+peer+" · post-FX","Live + "+peer+" · post-FX","Self · post-FX (resample)","Live + Self · post-FX","A+B · post-FX","Live + A+B · post-FX"};return names[mode];}
} }
