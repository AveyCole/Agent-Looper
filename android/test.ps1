param([switch]$Android)
$ErrorActionPreference='Stop'
$base=$PSScriptRoot
$jdk=(Get-ChildItem "$base\.toolchain\jdk" -Directory | Select-Object -First 1).FullName
$out="$base\build\tests"
New-Item -ItemType Directory -Force -Path $out | Out-Null
$env:JAVA_HOME=$jdk
$env:PATH="$jdk\bin;$env:PATH"
$env:JAVA_TOOL_OPTIONS="-Djava.io.tmpdir=$base\build\temp"
$source="$base\app\src\main\java\com\aveycole\agentlooper"
& "$jdk\bin\javac.exe" -encoding UTF-8 -d $out "$source\LoopEngine.java" "$source\AudioFiles.java" "$base\tests\EngineTest.java"
if($LASTEXITCODE -ne 0){throw 'Core test compile failed'}
& "$jdk\bin\java.exe" -cp $out EngineTest
if($LASTEXITCODE -ne 0){throw 'Engine test failed'}
if($Android){
 $jar=(Get-ChildItem "$base\.toolchain\platform-35_r02.zip-extracted" -Filter android.jar -Recurse | Select-Object -First 1).FullName
 $bt=Split-Path (Get-ChildItem "$base\.toolchain\build-tools_r35_windows.zip-extracted" -Filter aapt2.exe -Recurse | Select-Object -First 1).FullName
 New-Item -ItemType Directory -Force -Path "$out\android","$out\dex" | Out-Null
 & "$bt\aapt2.exe" link -o "$out\test-base.apk" -I $jar --manifest "$base\tests\AndroidManifest.xml"
 & "$jdk\bin\javac.exe" -encoding UTF-8 -source 8 -target 8 -classpath "$jar;$base\build\classes.jar" -d "$out\android" "$base\tests\AndroidSmokeTest.java"
 if($LASTEXITCODE -ne 0){throw 'Android test compile failed'}
 & "$jdk\bin\jar.exe" cf "$out\test.jar" -C "$out\android" .
 & "$bt\d8.bat" --min-api 26 --lib $jar --classpath "$base\build\classes.jar" --output "$out\dex" "$out\test.jar"
 Copy-Item "$out\test-base.apk" "$out\test-unsigned.apk" -Force
 & "$jdk\bin\jar.exe" uf "$out\test-unsigned.apk" -C "$out\dex" classes.dex
 & "$bt\zipalign.exe" -f 4 "$out\test-unsigned.apk" "$out\test-aligned.apk"
 & "$bt\apksigner.bat" sign --ks "$base\.toolchain\development.jks" --ks-pass pass:android --ks-key-alias agentlooper --out "$out\test.apk" "$out\test-aligned.apk"
 if($LASTEXITCODE -ne 0){throw 'Test APK signing failed'}
 New-Item -ItemType Directory -Force -Path "$out\source","$out\source-dex" | Out-Null
 & "$bt\aapt2.exe" link -o "$out\source-base.apk" -I $jar --manifest "$base\tests\SourceManifest.xml"
 & "$jdk\bin\javac.exe" -encoding UTF-8 -source 8 -target 8 -classpath $jar -d "$out\source" "$base\tests\SourceActivity.java"
 & "$jdk\bin\jar.exe" cf "$out\source.jar" -C "$out\source" .
 & "$bt\d8.bat" --min-api 29 --lib $jar --output "$out\source-dex" "$out\source.jar"
 Copy-Item "$out\source-base.apk" "$out\source-unsigned.apk" -Force
 & "$jdk\bin\jar.exe" uf "$out\source-unsigned.apk" -C "$out\source-dex" classes.dex
 & "$bt\zipalign.exe" -f 4 "$out\source-unsigned.apk" "$out\source-aligned.apk"
 & "$bt\apksigner.bat" sign --ks "$base\.toolchain\development.jks" --ks-pass pass:android --ks-key-alias agentlooper --out "$out\source.apk" "$out\source-aligned.apk"
 if($LASTEXITCODE -ne 0){throw 'Test source APK failed'}
}
