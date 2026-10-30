param([string]$OutputName = 'Agent-Looper.exe')
$ErrorActionPreference = 'Stop'
$buildOutput = Join-Path $PSScriptRoot $OutputName
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /platform:x64 /target:winexe /optimize+ /out:"$buildOutput" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:"$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\netstandard.dll" /reference:"$PSScriptRoot\NAudio.Asio.dll" /reference:"$PSScriptRoot\NAudio.Core.dll" /reference:"$PSScriptRoot\NAudio.Wasapi.dll" "$PSScriptRoot\LoopPedal.cs" "$PSScriptRoot\Controls.cs" "$PSScriptRoot\AudioBackend.cs" "$PSScriptRoot\AudioChecks.cs" "$PSScriptRoot\NativeCapture.cs" "$PSScriptRoot\QualityRender.cs" "$PSScriptRoot\SampleEditor.cs" "$PSScriptRoot\Midi.cs" "$PSScriptRoot\MidiUi.cs" "$PSScriptRoot\StudioChecks.cs" "$PSScriptRoot\StudioControls.cs" "$PSScriptRoot\LiveStretch.cs"
if ($LASTEXITCODE -ne 0) { throw 'Build failed' }







