param([string]$Compiler,[string]$OutputPath)
$ErrorActionPreference='Stop'
if(!$Compiler){$Compiler=Join-Path $PSScriptRoot '.toolchain/llvm-mingw-20251021-ucrt-x86_64/bin/x86_64-w64-mingw32-clang++.exe'}
if(!$OutputPath){$OutputPath=Join-Path $PSScriptRoot '../AgentVst3.dll'}
$sdk=Join-Path $PSScriptRoot 'sdk'
if(!(Test-Path $sdk)){$sdk=Join-Path $PSScriptRoot '.toolchain/sdk'}
$sources=@('pluginterfaces/base/funknown.cpp','pluginterfaces/base/coreiids.cpp','pluginterfaces/base/ustring.cpp','pluginterfaces/base/conststringtable.cpp','base/source/baseiids.cpp','base/source/fbuffer.cpp','base/source/fdebug.cpp','base/source/fobject.cpp','base/source/fstreamer.cpp','base/source/fstring.cpp','base/source/updatehandler.cpp','base/thread/source/flock.cpp','public.sdk/source/common/threadchecker_win32.cpp','public.sdk/source/common/commonstringconvert.cpp','public.sdk/source/common/commoniids.cpp','public.sdk/source/common/memorystream.cpp','public.sdk/source/vst/vstinitiids.cpp','public.sdk/source/vst/hosting/module.cpp','public.sdk/source/vst/hosting/module_win32.cpp','public.sdk/source/vst/hosting/plugprovider.cpp','public.sdk/source/vst/hosting/connectionproxy.cpp','public.sdk/source/vst/hosting/hostclasses.cpp','public.sdk/source/vst/hosting/pluginterfacesupport.cpp','public.sdk/source/vst/hosting/parameterchanges.cpp','public.sdk/source/vst/hosting/eventlist.cpp','public.sdk/source/vst/utility/stringconvert.cpp')|ForEach-Object{Join-Path $sdk $_}
& $Compiler --target=x86_64-w64-mingw32 -Wno-pragma-pack -std=c++17 -O2 -shared -static -DNDEBUG -DRELEASE=1 -DUNICODE -D_UNICODE -I $sdk "$PSScriptRoot/AgentVst.cpp" @sources -o "$OutputPath" -lole32 -luuid -lshell32 -luser32 -ladvapi32
if($LASTEXITCODE -ne 0){throw 'VST3 bridge build failed'}
