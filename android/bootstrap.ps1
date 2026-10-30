$ErrorActionPreference='Stop'
$root=Join-Path $PSScriptRoot '.toolchain'
New-Item -ItemType Directory -Force -Path $root | Out-Null
$jdkMeta=Invoke-RestMethod 'https://api.adoptium.net/v3/assets/latest/17/hotspot?architecture=x64&image_type=jdk&os=windows&vendor=eclipse'
$jdk=$jdkMeta[0].binary.package
Invoke-WebRequest $jdk.link -OutFile "$root\jdk.zip"
if((Get-FileHash "$root\jdk.zip" -Algorithm SHA256).Hash -ne $jdk.checksum){throw 'JDK checksum mismatch'}
Expand-Archive "$root\jdk.zip" -DestinationPath "$root\jdk" -Force
foreach($asset in @('platform-35_r02.zip','build-tools_r35_windows.zip')){
 Invoke-WebRequest "https://dl.google.com/android/repository/$asset" -OutFile "$root\$asset"
 Expand-Archive "$root\$asset" -DestinationPath "$root\$asset-extracted" -Force
}
Write-Output 'Build tools ready. Run .\build.ps1'
