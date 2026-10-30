# Agent Looper

Native Windows and Android dual audio looper.

- [Windows portable download](Agent-Looper-Windows.zip)
- [Android APK v0.2.0](downloads/Agent-Looper-Android.apk)
- [Android instructions and source](android/README.md)

Both versions now include Wait for Input and per-bank Warp. Windows WARP highlights while pending or active; it currently uses the same highlight for both states.

![Windows interface](docs/screenshot.png)

[Android interface](docs/android.png)

## Windows

Run **Agent-Looper.exe** from the extracted portable folder. Keep all included DLLs and the tools folder beside it. Use one Agent Looper instance at a time. Older builds are preserved. Windows x64 and .NET Framework 4.7.2 or later are required.

The interface uses custom dark dropdowns and numeric controls. Filter, level, pan, and master controls respond throughout a drag. Layer editors and the Audio panel collapse independently.

## Waveform editing

Each layer has visible start/end handles, including when Edit is collapsed. Drag the left handle to trim the beginning while keeping the end fixed; drag the right handle to shorten or extend the loop. Extension beyond the take adds silence. Numeric Start/Length fields follow the handles. Audio updates with a crossfade when the drag is released; Escape cancels the preview. Original takes stay untouched.

Wheel zooms around the pointer. Shift+wheel pans horizontally. Double-click fits the full take and selected region. Click the waveform to focus it if your Windows mouse settings route wheel events to the focused control. S/E selects a start/end handle for arrow-key adjustment; Shift+arrows makes finer changes.

Waveforms are taller, filled min/max envelopes. Quiet material is scaled visually without changing recorded samples or playback gain. Silence is still drawn as silence.

## Two loopers and timing

A and B have independent Record, Overdub, Stop/Play, Undo, layer editors, and loop lengths. Both can record together. Click a bank heading or transport to select it; Import A/B and Export A/B act on that bank. Keys 1/2 select A/B, R records/finishes, D overdubs, and Space starts/stops.

Free records manually. Sync captures the selected number of measures using BPM and time signature, with a five-minute maximum. Count-in is optional. Overdub captures one cycle of the bank's master loop. The first layer defines its bank's master cycle; editing that layer's duration updates the cycle. Other layers can retain independent lengths.

**AUTO OD** is optional per bank. Finish the first free take to enter overdubbing automatically, or start an overdub on an existing loop. Each completed cycle becomes a separate layer without a recording gap. Stop or finish Overdub to stop recording. Automatic continuation is bounded at approximately 64 layers; stop and re-arm after changing the master cycle length to use the new capture period.

For Overdub, **START: MEASURE** follows the first loop's actual restart, including free loops that do not match the BPM grid. B follows A when A has a loop; otherwise a bank follows its own loop. Ordinary new recordings retain measure-grid scheduling.

Start options are independent of Free/Sync: Now, Measure, or the other looper's next loop boundary. On B, choose **START: A LOOP** to quantize Record, Overdub, or Play to A. Record a loop in A first. An armed operation waits if A is paused; playback restarts B at its own beginning on A's boundary. Cancel an armed operation with its transport/Record button.

B's input options are **LIVE**, **A**, or **LIVE + A**. A feeds B after A's layer processing and master gain, before the final combined output. Live + A records separate live-input and A-bus takes. B is never fed back into its own A-bus source. Changing a source or start option finishes/cancels an existing take before applying the change.

## Click and MIDI

**CLICK** enables a monitor metronome throughout recording. Count-in and recording clicks are excluded from the software capture buffers, A-to-B recording feed, and WAV exports. Raw hardware loopback/master inputs can contain a click returned through the hardware mixer; choose the instrument channels or use hardware cue routing to avoid recording that physical return.

Open **MIDI** and select an input port. Right-click any transport button and choose **MIDI Learn**, then press a controller button. The MIDI window also offers target selection, Learn and Clear. Note On and CC rising edges are supported, with channel-specific assignments. Learning does not execute the action. Assignments and the selected port are saved in Agent-Looper-MIDI.xml beside the executable. A note/CC can control one transport action at a time.

## Audio quality

Capture and playback use 48 kHz, 32-bit float. Default gains are unity, without automatic normalization or saturation. Long takes retain their own duration instead of folding/summing into older short loops. Boundary crossfades preserve loop duration without fading through silence. Several audible layers still sum together; reduce gains if their combined level clips.

Pitch and stretch default to **R3 LIVE · MUSIC**: native Rubber Band R3 processes on a worker thread and updates while dragging. Pitch and duration are independent. There is a short processing/lookahead delay; CPU requirements grow with active processed layers. **FORMANTS** preserves the spectral/formant envelope for vocal material and can intentionally change isolated-tone levels. Unchanged audio bypasses processing exactly.

**R3 RENDER** uses Rubber Band 4 R3 offline two-pass processing with linked stereo, wrapped loop context, and float WAV throughout; pitch/stretch updates apply after releasing the knob and completing the render. Filter, level and pan remain live in either mode. Exports render current pitch/stretch settings offline, including layers in Live mode.

RePitch 2 is a vocal-editing plugin requiring a compatible host. PitchNet uses neural vocal resynthesis. R3 was selected as the general-purpose engine for mixed synth, drum, and instrument loops:

- RePitch 2: https://www.synchroarts.com/repitch
- PitchNet: https://github.com/SessionLoops/PitchNet
- R3 integration recommendations: https://breakfastquay.com/rubberband/integration.html
- Official R3 executable/source: https://breakfastquay.com/rubberband/

Input monitoring is off. Audio expands device selection, including all 14 Zoom ASIO inputs. Changes apply automatically and preserve both banks' layers. Stereo/mono selections for the same physical channel replace each other to prevent duplicate capture. Model:Cycles uses its negotiated native format. Windows audio can combine devices; their independent clocks may drift.

Import accepts mono/stereo PCM or float WAV, including high-resolution files. Export writes 48 kHz, 32-bit float WAV with headroom. Export covers the longest layer/master cycle of the selected bank. Closing clears the in-memory session; export first. Editable session saving is not included.

## Build and checks

Build: powershell -ExecutionPolicy Bypass -File build.ps1 -OutputName Agent-Looper.exe

The native R3 helper and licenses are in tools; source provenance is in RubberBand.README.txt. No network is used by the running app.

Live processing uses the official MSYS2 x64 Rubber Band 3.3 package and its FFTW, libsamplerate and GCC runtime dependencies. Native license notices are included in licenses/native. Package provenance is recorded there as well.

Additional verified checks: --measure-overdub-test, --live-filter-test, --automatic-overdub-test, --native-live-test, --realtime-ui-test. --realtime-hardware-test passed on the Zoom ASIO clock with native live pitch/stretch feeding B and hardware output suppressed. These checks do not substitute for an audible listening test on your monitoring setup.

Verified tests: --recording-test, --quality-test, --dual-test, --studio-routing-test, --midi-test, --click-test, --pitch-test, --handles-test, --dual-ui-test. These verify unity capture, no self-summing, exact quantized starts/playback, A-to-B isolation, click exclusion, note/CC mapping/persistence, R3 pitch/duration, handle edits/zoom, and independent UI controls. --dual-hardware-test also passed with real simultaneous one-second/three-second Zoom ASIO captures and silent output. MIDI protocol tests were synthetic; no physical controller button was pressed by the tests.

## Wait for Input and Warp

WAIT INPUT arms Record/Overdub until a selected input crosses the adjacent dBFS threshold (default −40; range −72 to −6). NOW starts on the exact crossing sample. Quantization and count-in begin after detection, preserving their scheduled boundary. Cancel with Record or Stop; no silent layer is created. Auto OD continues without waiting for a new signal every pass. No pre-roll is added: lower the threshold for soft attacks.

WARP fits every layer in one bank to the current master cycle and common downbeat while preserving pitch. Windows prepares high-quality Rubber Band renders, then publishes all layers together at the next loop restart after rendering completes. Paused banks apply immediately. Subsequent live pitch/trim edits use the locked cycle; disabling Warp restores independent lengths and stretch settings. Original takes are retained. This aligns whole loop durations, not individual detected beats/transients. A and B still have independent cycles.

Additional checks passed: --input-trigger-test, --warp-test, --warp-ui-test, including exact threshold onset, count-in/quantization interaction, Auto OD, atomic layer alignment and live edits while warped.