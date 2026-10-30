# Agent Looper for Android

Native Android touch version of Agent Looper. Android 8.0+; other-app audio capture requires Android 10+. This is the development APK (0.2.0), signed with a local development key, not a Play Store release.

## Inputs and monitoring

**AUDIO → INPUT** offers Microphone, Other apps, Mic + apps, and Android-exposed external audio devices. USB audio requires a compatible USB-C host connection/interface. **CHANNELS** selects a mono channel or stereo pair from the channels Android reports. No Windows ASIO driver is used. One physical input device is active at a time. Stereo output can be selected independently.

Other apps opens Android's capture-consent dialog. The app records audio only: it does not create a screen/video recorder or save screenshots. Android may describe this permission as screen recording/sharing. Only apps whose playback policy allows capture can be recorded; protected content, calls, or apps that opt out may be silent. Stop app capture using AUDIO → END APP CAPTURE or the Android capture indicator.

**AUDIO → INPUT MONITOR** enables mic/USB listening with a feedback warning. **MONITOR LEVEL** adjusts that signal. Use headphones and avoid doubling hardware direct monitoring. Monitoring is off initially and switches off when the audio service ends. Other-app playback already reaches your output, so it is not duplicated by the monitor. Monitoring is mixed after the recording and internal routing paths.

An audio foreground service keeps the engine running while you switch to another app or lock the screen. Stop the session from the Agent Looper notification. Swiping Agent Looper away from Recent Apps also stops the audio service, capture, playback and monitoring. Merely switching apps leaves it running. No account, analytics, Internet permission, or audio upload is included.

## Looping

A/B are stacked in portrait and side by side on wide screens. Tap the bank name to choose the FILES destination. Record finishes the current take; Overdub adds a layer; Stop/Play toggles playback. Undo removes the latest layer. Optional AUTO OD rolls into separate passes at the master loop's sample boundary. Free takes are limited to 60 seconds. Each bank is limited to 32 layers and available heap memory.

Tap FREE/SYNC or START to choose measure-based capture and start quantization. BPM opens timing settings: 2–7 quarter-note beats per measure, 1–16 measures, and optional one/two-measure count-in. CLICK continues during recording. Software recordings and the A→B bus exclude the count-in and click. Physical speaker pickup or hardware loopback can still contain anything you hear externally.

START: MEASURE on an overdub follows the first loop's actual restart; B follows A if A has a loop. START: A/B LOOP waits for that peer's next restart, including while the reference is paused. B can record LIVE, A, or LIVE + A. A's post-layer-FX signal feeds B before metronome and input monitoring. There is no B→A feedback path.

## Wait for Input and Warp

Tap FREE/SYNC or START to open the bank options, enable **WAIT FOR INPUT**, and set the trigger threshold (−72 to −6 dBFS; default −40). Record now arms without collecting silence or sounding count-in. The threshold-crossing sample starts the take in NOW mode. With count-in or quantization, the trigger starts that waiting schedule; it does not bypass the chosen boundary. Overdub can use the same mode. Cancel an armed take with Record/Overdub or Stop. Auto OD continues without rearming every cycle. There is no pre-roll buffer, so use a sufficiently low threshold to retain soft attacks.

**WARP** locks all layers in one bank to that bank's current master cycle and downbeat at its next restart. Pitch is preserved, using independent granular stretching even if a layer's free mode is Repitch. Trim/pitch edits cannot change that locked cycle. Turning Warp off restores the individual free stretch/length/pitch settings; the original samples are retained. This fits each whole layer to a common cycle; it does not detect/reposition every drum transient or align A and B to the same duration. Large ratios can produce granular artifacts. Paused banks apply Warp immediately.

## Layer editing

EDIT expands live stretch, pitch, filter, level, and pan controls. Drag knobs vertically; hold a knob without moving to reset it. M/S mute and solo. Reverse, half/double length, and FULL operate on the original take without overwriting it. Drag either waveform handle to trim or extend with silence. Pinch to zoom; drag the waveform's middle to pan; dragging outside controls scrolls the page.

Two processing modes:

- **GRANULAR · INDEPENDENT**: stereo-linked, four-overlap Hann grains with waveform-alignment search. Independent live pitch (±12 semitones) and duration (0.5–2×). This is an original lightweight processor with audible granular/transient artifacts on some material, not Koala's implementation or desktop Rubber Band R3.
- **REPITCH · TAPE**: pitch and duration are coupled by playback rate, like speeding/slowing a sampler or record. This is the same general approach described for Koala's Repitch mode, not shared Koala source code.

Koala documents Modern, Retro, Beats, Repitch and Cyclic modes, but its manual does not identify a reusable third-party Modern engine: https://manual.koalasampler.com/mobile/4-sample/ . Agent Looper does not claim to reproduce all five.

Audio is stored and processed at 48 kHz stereo float32, with unity default gains and no normalization. Playback clips only at the final hardware output. Float WAV exports preserve summed headroom. Quiet waveforms are enlarged visually without increasing playback gain. Mic preprocessing is requested off where Android supports unprocessed capture; actual ADC precision, routing, latency, and vendor processing depend on the phone/interface. Bluetooth adds latency. Neither USB multichannel exposure nor Zoom L6 compatibility has been physically verified on your phone.

## Files and MIDI

FILES imports mono/stereo PCM16/24/32 or float32 WAV (up to 60 seconds), exports the selected bank as float32 WAV, and saves/opens editable `.agl` sessions using Android's document picker. Rate conversion uses a windowed-sinc interpolator. Restored sessions open paused. Sessions are local; Windows and Android session formats are separate. Completed sessions are also saved in app-private storage when the app leaves the screen or a transport finishes. Save/export before uninstalling.

MIDI selects an Android MIDI input. Hold a transport, choose LEARN, then send a note or CC. Channel-specific rising edges trigger actions; learning does not execute them. Assignments persist locally. BLE MIDI device pairing is handled by Android or the controller's app; it is not implemented here. Physical MIDI hardware has not been tested.

## Build

Use Android Studio with the included Gradle project (AGP 8.7.3, JDK 17, compile/target API 35), or the dependency-free Windows `build.ps1` packaging path. The latter accepts `-JavaHome`, `-AndroidJar`, and `-BuildTools` and otherwise finds local tools in `.toolchain`. `bootstrap.ps1` downloads the pinned official tools into that ignored directory. Run `build.ps1` to produce `build/Agent-Looper-Android.apk`. Keep the private `.toolchain/development.jks` if you need future APKs to update this installation. The signing key is deliberately excluded from source archives.

`test.ps1` runs host JVM DSP/session/file regressions. `test.ps1 -Android` also builds a separate instrumentation APK and synthetic playback-source APK. No tests or tone generator are included in the user APK. Android 15 emulator verification covered launch, foreground mic/output, advancing clocks, nonzero loop playback, held-touch filter changes, overdub/finish/undo, playback-capture consent, own-output exclusion, and a full one-second 440 Hz recording from another foreground app. This is not a physical-phone listening or latency test.

Android API references: https://developer.android.com/media/platform/av-capture and https://developer.android.com/develop/background-work/services/fgs/service-types .
