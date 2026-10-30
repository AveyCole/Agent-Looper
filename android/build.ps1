param([string]$JavaHome,[string]$AndroidJar,[string]$BuildTools)
$ErrorActionPreference='Stop'
$base=$PSScriptRoot
if(-not $JavaHome){$JavaHome=(Get-ChildItem "$base\.toolchain\jdk" -Directory | Select-Object -First 1).FullName}
if(-not $AndroidJar){$AndroidJar=(Get-ChildItem "$base\.toolchain\platform-35_r02.zip-extracted" -Filter android.jar -Recurse | Select-Object -First 1).FullName}
if(-not $BuildTools){$BuildTools=Split-Path (Get-ChildItem "$base\.toolchain\build-tools_r35_windows.zip-extracted" -Filter aapt2.exe -Recurse | Select-Object -First 1).FullName}
if(-not (Test-Path "$JavaHome\bin\javac.exe")){throw 'Set -JavaHome to a JDK 17 or newer'}
$env:JAVA_HOME=$JavaHome
$env:PATH="$JavaHome\bin;$env:PATH"
$out=Join-Path $base 'build'
New-Item -ItemType Directory -Force -Path $out,"$out\classes","$out\dex","$out\temp" | Out-Null
$env:JAVA_TOOL_OPTIONS="-Djava.io.tmpdir=$out\temp"
function Check([string]$step){if($LASTEXITCODE -ne 0){throw "$step failed (exit $LASTEXITCODE)"}}
& "$BuildTools\aapt2.exe" compile --dir "$base\app\src\main\res" -o "$out\resources.zip"; Check 'Resources'
& "$BuildTools\aapt2.exe" link -o "$out\base.apk" -I $AndroidJar --manifest "$base\app\src\main\AndroidManifest.xml" --min-sdk-version 26 --target-sdk-version 35 --auto-add-overlay "$out\resources.zip"; Check 'Resource linking'
$sources=@(Get-ChildItem "$base\app\src\main\java" -Filter '*.java' -Recurse | ForEach-Object {$_.FullName})
& "$JavaHome\bin\javac.exe" -encoding UTF-8 -source 8 -target 8 -Xlint:deprecation -classpath $AndroidJar -d "$out\classes" $sources; Check 'Java compile'
& "$JavaHome\bin\jar.exe" cf "$out\classes.jar" -C "$out\classes" .; Check 'Class archive'
& "$BuildTools\d8.bat" --min-api 26 --lib $AndroidJar --output "$out\dex" "$out\classes.jar"; Check 'Dex'
Copy-Item "$out\base.apk" "$out\unsigned.apk" -Force
& "$JavaHome\bin\jar.exe" uf "$out\unsigned.apk" -C "$out\dex" classes.dex; Check 'Dex packaging'
& "$BuildTools\zipalign.exe" -f -p 4 "$out\unsigned.apk" "$out\aligned.apk"; Check 'Alignment'
$key=Join-Path $base '.toolchain\development.jks'
if(-not(Test-Path $key)){& "$JavaHome\bin\keytool.exe" -genkeypair -keystore $key -storepass android -keypass android -alias agentlooper -keyalg RSA -keysize 2048 -validity 10000 -dname 'CN=Agent Looper Development';Check 'Development signing key'}
& "$BuildTools\apksigner.bat" sign --ks $key --ks-pass pass:android --ks-key-alias agentlooper --out "$out\Agent-Looper-Android.apk" "$out\aligned.apk"; Check 'APK signing'
& "$BuildTools\apksigner.bat" verify --verbose "$out\Agent-Looper-Android.apk"; Check 'APK verification'
Write-Output "APK: $out\Agent-Looper-Android.apk"
