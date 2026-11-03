# Native VST3 bridge

The prebuilt AgentVst3.dll is next to Agent-Looper.exe. Rebuilding the C# app does not rebuild this DLL.

To rebuild the bridge, unpack the LLVM MinGW 20251021 UCRT x86_64 compiler from the official release linked in licenses/VST3/PROVENANCE.txt. The required Steinberg SDK sources are included under sdk/.

Run this script from PowerShell, passing absolute paths:

    ./build.ps1 -Compiler C:/path/to/llvm-mingw/bin/x86_64-w64-mingw32-clang++.exe -OutputPath C:/path/to/AgentVst3.dll

Do not replace the running app's bridge DLL. The wrapper exports ABI version 1. The process is x64 and hosts stereo float32 VST3 audio effects at 48 kHz, in blocks of 64 samples.
