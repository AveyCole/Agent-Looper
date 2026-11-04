# Agent Looper 1.9.0 — Apple Silicon Mac

Native Apple Silicon Mac update matching the published Windows 1.9.0 build, retaining VST3 and Audio Unit hosting.

- **MAC AUDIO · Chrome / Desktop**: record a selected playback device alongside hardware inputs, including silence. Requires macOS 14.2+ and system-audio recording permission. No screen video is captured.
- Per-layer **Snap pitch to semitones**.
- **Repitch · linked speed / pitch**: twice the duration is one octave lower.
- **Cyclic · S950-style** live stretch with 5–100 ms D-time. Original stereo cyclic overlap-add DSP inspired by vintage samplers; not Akai code or bit-exact hardware emulation.
- 2×2 LFO source picker with clearer outline; send labels follow the effect type/plugin name.
- Playback settings persist in projects, scenes and history and apply to export. Older projects load; newly saved .agw files require 1.9+.

Playback settings: EDIT → Playback or a layer's ⋯ → Playback. Desktop inputs: AUDIO. Desktop capture includes Agent Looper when it plays through the selected device; desktop software monitoring is disabled to avoid an immediate feedback loop. The selected device's first stream/channel pair is captured.

Validated native pitch/duration, live sweeps, cyclic stereo/D-time, snapping, history/scenes/export; project/session roundtrips; actual UI edits and Save/Load; AU pop-out/reattachment; real VST3/AUv2/AUv3 audio/state/export; and real desktop silent/tone capture without microphone input. Simultaneous external-interface+desktop timing, multichannel hardware and physical MIDI were not retested. See VALIDATION.txt.

VST3/AUv2 native editors remain available. AUv3 defaults to parameter controls because Koala FX's remote custom editor is blank on this Mac; custom remote views remain experimental. Effects must be installed separately as Mac Apple Silicon/Universal builds.

Extract the complete ZIP and open **Agent Looper.app**. Apple Silicon, macOS 12+ (desktop capture 14.2+). Locally ad-hoc signed, not Apple notarized. Complete source, dependency sources/licenses and offline build script included.

Windows reference: f1278d12a12f9d3a7bbb36e59b3b9c89a09dab35; shipped Windows ZIP checksum and AssemblyVersion 1.9.0.0 verified.

SHA-256 (`Agent-Looper-M1-Mac.zip`):
```
0964a941599a33aa54644fd6e044efe244bf1486b0bd8faaae0e82735b292c4f
```

[Mac release and checksum](https://github.com/AveyCole/Agent-Looper/releases/tag/v1.9.0-macos-arm64).

# Agent Looper 1.9.0 — Windows

- Clearer popup borders and a 2×2 LFO source picker.
- **PC AUDIO · Chrome / Desktop** inputs capture a selected Windows playback device, including silence. Available alongside ASIO inputs when the devices support simultaneous use.
- Per-layer **Snap pitch to semitones**.
- **Repitch · linked speed / pitch**: the pitch and stretch controls follow one another. Twice the duration is one octave lower.
- **Cyclic · S950-style** live stretch with a 5–100 ms D-time control for vintage metallic textures. This is original stereo cyclic overlap-add DSP inspired by vintage samplers, not Akai code or bit-exact hardware emulation. Existing R3 modes remain available.

Find playback settings under **EDIT → Playback**, or a layer's **⋯ → Playback**. Find PC loopback in **AUDIO → input list**. Loopback includes this app if it plays through the same shared device.

Settings persist in projects, scenes and history, and apply to export. Older projects load; newly saved projects require 1.9.0 or newer. Mac 1.9.0 reads the new Windows project format.

Validated: linked frequency/duration and live sweeps; cyclic pitch, duration, stereo and D-time; project/scene round-trips; actual menu actions; 2×2 picker; real Zoom L6 WASAPI loopback record/finish including silence; session UI, LFO, performance and VST3 regressions. Packaged source rebuilt and tested. Mixed ASIO+loopback timing has not been tested.

Extract the full ZIP and keep its DLLs, tools and licenses beside Agent-Looper.exe. Rebuild the portable package with source/build.ps1. No APK or Android source is included.

# Agent Looper 1.8.1 — Windows

Send-knob labels now show the selected effect type or plugin name. Labels update when cycling types, replacing a VST3, restoring a built-in effect, or switching A/B sends. Long names stay within the control's label area.

The send controls retain their existing MIDI, LFO and automation assignments. Existing Windows 1.8.0 projects remain compatible.

Validated with the Windows session UI checks and real VST3 replacement/editor tests, including the plugin-name send label. Download the complete Windows ZIP and keep its DLLs, tools and licenses beside Agent-Looper.exe. No APK or Android source is included in the Windows package.

# Agent Looper 1.8.0 — Apple Silicon Mac · VST3 + Audio Units

Adds Windows 1.8.0 VST3 effect hosting to the native Apple Silicon Mac app, plus Audio Unit effects (AUv2/AUv3).

- Independent three-slot replacements for A/B, wet/dry sends, bypass, reload and restore built-ins.
- Embedded VST3/AUv2 editors, pop-out and reattachment.
- AUv3 defaults to editable parameter controls. Koala FX's remote custom editor is blank on this Mac; custom AUv3 remote views remain experimental.
- Plugin state in projects/scenes, missing-plugin dry fallback, rack latency alignment, and independent offline WAV export.
- All existing looping, internal resampling, automation, LFOs and MIDI clock retained.

Tested real ValhallaSupermassive VST3/AUv2 and Koala FX AUv3 audio/state/export, actual GUI-edited AUv3 save/restore, editor pop-out/reattachment, native routing/latency and existing regressions. Full details in VALIDATION.txt.

Effects are installed separately and must be Mac Apple Silicon/Universal builds. This hosts effects inside the standalone app; Agent Looper itself is not a DAW plugin. No instruments, sidechains, plugin MIDI input or third-party parameter automation. VST3 runs without crash isolation. Reload after bus/latency changes; no cross-bank latency compensation.

Extract the complete ZIP and open Agent Looper.app. Apple Silicon, macOS 12+. Locally ad-hoc signed; not Apple notarized. Complete source, dependency sources/licenses and offline build script included. Older projects load; newly saved .agw projects require the 1.8 reader.

Windows reference: 8e4f8a627b14de890edf3a301a220dc5185cf679.

[Mac release and checksum](https://github.com/AveyCole/Agent-Looper/releases/tag/v1.8.0-macos-arm64).

# Agent Looper 1.8.0 — Windows VST3 preview

- Right-click an FX tab → **Replace with VST3…**. The menu searches the standard system and per-user VST3 folders whenever opened. Browse accepts plugin files and bundle folders outside those locations. This is directory discovery, without an exhaustive plugin-validation scan or custom-folder manager.
- Plugin editors appear directly in the FX controls area. Supported editors resize/scale to fit; **POP OUT** remains available for fixed or oversized editors.
- Each bank has its own plugin replacements. Slot levels act as wet/dry mix and retain existing MIDI, LFO and automation assignments. Bypass, replacement and restoration of built-in effects are available in the tab menu.
- Plugin paths and parameter state save with Windows projects and scenes. Missing plugins retain their state and pass dry audio. Plugins must be installed separately.
- Stereo float32 VST3 effects at 48 kHz, with a 64-sample rack buffer. Wet/dry and parallel branches are latency-aligned within a rack. Separate A/B banks are not automatically delay-compensated against each other.

First-preview limits: plugins run in the app process, with no crash sandbox. Instruments, sidechains, plugin MIDI input and host automation of external plugin parameters are not included. Plugin latency or bus changes require Reload plugin. Multi-effect bundles use the first effect class.

Extract the complete ZIP and retain **AgentVst3.dll**. Windows 1.8.0 reads older Windows projects; newly saved projects require the updated reader, available in Mac 1.8.0. No Android binaries, signing material or third-party effect plugins are included.

Validation passed for real TAL-Reverb-2, TAL-Chorus-LX and ValhallaSupermassive audio; editor attachment/resizing/tab switching/popout/reattachment; project/save/dirty/missing-plugin handling; deterministic latency checks for all routing modes; a simulated silent three-plugin soak; existing audio/UI checks; managed and native source rebuilds. Physical-interface listening was not repeated.

# Agent Looper 1.7.2 — Apple Silicon Mac

- Four multi-target LFOs with waveform picker, drag-to-knob linking, source/wave/polarity/timing controls and labelled Depth, Rate and Phase sliders.
- Recorded knob automation: free Start/Finish, Follow A/B, beats or seconds; per-lane controls and playback toggle.
- Eight named scenes, immediate or next-A-loop recall with a 20 ms transition.
- Windows-compatible editable `.agw` projects; New/Save/Save as/Load and Save/Discard/Cancel protection. Older Mac sessions still open.
- MIDI clock Follow/Send, 24 ppqn, Start/Continue/Stop and output selection; automation MIDI Learn.
- Overdub feedback with per-pass layer decay, partial destructive capture, Undo and export support.
- PS1 SPU, Bloom, Tuned resonator, Crystal delay, Tape Echo, and expanded grain controls.
- Existing two-bank pre/post-FX, self and mix resampling retained.

Reference: shipped Windows 1.7.2 ZIP at a90d6eeb87bf2833033527fd09e40b27f5cc9a12; checksum and bundled version verified. Native/Swift regressions, `.agw` byte round-trips, MIDI clock scheduler and Mac UI recording/settings/save checks passed. Physical external MIDI/controllers and multichannel interfaces remain untested.

[Mac 1.7.2 release and checksum](https://github.com/AveyCole/Agent-Looper/releases/tag/v1.7.2-macos-arm64). Requires Apple Silicon and macOS 12+. Full source, dependency sources/licenses, validation and offline rebuild script included. Locally signed, not Apple notarized.

# Agent Looper 1.7.2 — Windows

- Rounded LFO picker: click **LFO** for four waveform squares. Drag a source onto knobs; each LFO can control multiple destinations. Double-click a numbered square for labelled Depth, Rate and Phase sliders, with visible values. Waveforms include Sine, Triangle, Square, Saw, Sample & hold and Input envelope; polarity and tempo/free timing are adjustable. TARGETS… adds/removes individual assignments.
- Record automation for effect and layer controls from the centered bottom transport. Free Start/Finish recording sets its length; follow A/B, beat and second lengths are also available. MIDI clock can follow or send.
- Editable Windows projects, scenes and saved modulation/automation. Earlier projects load; newly saved projects need this updated version.
- Expanded effects: PS1 reverb modes, Crystal Delay, Tape Echo, Bloom and Tuned, plus additional granular/cloud controls.

Extract the complete portable ZIP and run Agent-Looper.exe. Windows x64 and .NET Framework 4.7.2+. Mac 1.7.2 is available separately below. No Android binaries or signing material are included.

Validation: multi-destination DSP and save/load, baseline restoration and unlinking; picker/settings/slider and actual knob drop handlers; performance/UI regression checks; stopped-audio/realtime stability checks. Packaged Windows source rebuild and ZIP integrity passed. Physical-device listening was not repeated.

# Agent Looper 1.2.12 — Mac / Windows parity

- Added EDIT → Truncate to selection, preserving selected float PCM and pitch/stretch/reverse/filter/gain/pan settings. Undo/Redo restores/reapplies the crop.
- Added Return to start on both bank headers, MIDI-assignable as A.Restart / B.Restart. Corrects stale origins, preserves Play/Stop, cancels pending playback and transitions smoothly while playing; peer/audio clocks stay unchanged.
- Retained all nine internal/live recording sources and FX behavior. Other-bank sources are disabled in Single mode; input-free internal playback/resampling does not request unused microphone access.

Reference: published Windows 1.2.12 ZIP at dd05c718f48fe23e972100ce59cd977220bc2258; ZIP checksum and bundled version verified. New native tests and the existing full regression suite passed. GUI truncation, cropped full-take bounds, both return buttons, layout and MIDI Learn checked. External physical MIDI was not retested.

[Mac 1.2.12 release and checksum](https://github.com/AveyCole/Agent-Looper/releases/tag/v1.2.12-macos-arm64). Complete corresponding source, dependencies/licenses, validation and rebuild script are included. Requires Apple Silicon and macOS 12+. Locally signed, not Apple notarized.

## Windows 1.2.12

- **Truncate to selection:** directly in EDIT. Keeps the selected float audio and existing pitch, stretch, reverse, filter, level and pan settings. Undo/Redo restores/reapplies the crop.
- **Return to start:** new MIDI-assignable button beside the headphones, plus one for B. Stopped remains stopped; playing continues from the beginning. Corrects stale bank-origin alignment after an overdub or layer replacement.
- **Internal resampling parity with Mac 1.2.8:** either bank can record the other pre/post-FX, itself post-FX, or A+B post-FX, with a Live + variant for each. Internal-only capture requires no hardware input. Live/internal layers stay separate in Layers mode and sum in Destructive. Click and monitoring are excluded.

Use looper ⋯ → Input to select the recording source. Self post-FX + DUB bakes the current processing into a pass. Destination FX still process playback; reduce its sends to avoid processing the captured effects again. Repeated resampling overdubs can increase level.

Validation: source/FX sample comparisons for both banks and all new modes; destructive summing; input-trigger and monitor/click isolation; automatic passes; sample-accurate quantized routing; selected-region truncate samples/settings/Undo/Redo; restart alignment and UI checks passed. External hardware listening was not repeated. Windows still exports WAV rather than editable project files.

Extract the complete portable ZIP and run Agent-Looper.exe. Requires Windows x64 and .NET Framework 4.7.2+. Mac remains at 1.2.8. No APK, Android app bundle or signing key is published.

## Mac 1.2.8 — resampling

- Both banks can capture the other bank before or after effects.
- Self post-FX resampling captures a bank’s own processed loop as an overdub; A+B post-FX captures the combined internal mix.
- Every internal source has a Live + variant. Separate live/internal layers in Layers mode, summed in Destructive.
- Internal-only recording needs no selected hardware input. Input trigger follows the chosen source; click/monitoring are excluded. Source selections persist in Mac sessions.
- Tested against independently rendered post-FX stereo samples for both banks and all new sources, including phase, destructive summing/Undo and isolation. Existing regressions and GUI source-menu checks passed.

**B records A with effects:** B ⋯ → Input → Looper A · post-FX. **A resamples itself:** A ⋯ → Input → Self · post-FX (resample), then DUB. Captured effects are baked in; destination effects still process playback. Repeated automatic resampling overdubs can increase level.

[Mac 1.2.8 download](https://github.com/AveyCole/Agent-Looper/releases/download/v1.2.8-macos-arm64/Agent-Looper-M1-Mac.zip) · [Release and checksum](https://github.com/AveyCole/Agent-Looper/releases/tag/v1.2.8-macos-arm64)

Windows remains at 1.2.7. Requires macOS 12+ on Apple Silicon. Locally signed; not Apple notarized. Complete corresponding source, dependency sources/licenses and rebuild script are in the ZIP.

## Previous Mac 1.2.7

Native arm64 Mac port based on the supplied Windows 1.2.7 source. Requires macOS 12 or later on M1 and newer.

- Windows-style single/dual interface, destructive/layered recording, timing, input triggers, auto overdub, undo/redo and A-to-B routing.
- Independent Rubber Band R3 pitch/stretch, render modes, Warp and waveform region editing.
- Full FX page with all reverb/delay/grain types, independent A/B sends, filters and Freeze, serial/parallel/split routing and MIDI Learn.
- Core Audio hardware/channel selection, native HAL microphone capture, bundled offline media decoder and Float32 WAV export.
- Mac sessions, complete corresponding source, dependency sources/licenses and rebuild script included in the ZIP.

Validated by regression tests, UI checks and built-in microphone recording/export on the M1 Mac. External MIDI/interfaces and older macOS releases were not physically tested. Locally signed, not Apple notarized.

[Mac download](https://github.com/AveyCole/Agent-Looper/releases/download/v1.2.7-macos-arm64/Agent-Looper-M1-Mac.zip) · [Mac release and checksum](https://github.com/AveyCole/Agent-Looper/releases/tag/v1.2.7-macos-arm64)

## Windows 1.2.7

- Removed the tagline from the app interface. Taglines remain on the website and documentation.
- Retained the Windows icon matching Android.
- Audio processing and features are unchanged.

Windows UI checks and Android interface checks passed. No APK is published in this repository.

## Previous: 1.2.6 — Windows

**It is happening again...**

- The primary tagline appears beneath the app wordmark and across the website and documentation.
- The alternate line, “That loop you like is going to come back in style”, appears in a secondary website section.
- Windows now matches Android's mint loop icon and dark palette. The executable includes icon sizes from 16 to 256 pixels for Explorer, the taskbar and title bar.
- Updated screenshot assets and browser favicon use the current branding.
- Audio processing is unchanged from 1.2.5.

Windows UI checks and Android portrait/landscape branding checks passed. The Windows build remains portable; extract the complete ZIP and run Agent-Looper.exe. No Android APK is distributed in this repository.

## Previous: 1.2.5 — Windows

- **Stronger CLOUDY:** a wider change from defined echoes/grains to a dense, diffuse cloud. Cloud Delay progressively replaces the clear echo with grains, and density changes across the full control range. Live changes are smoothed, including during Freeze. The grain voice limit is unchanged.
- **Independent A/B Freeze:** hold different grain captures for the two loopers, or release either separately. Detailed effect parameters and routing remain shared; sends and filters remain independent.
- **Expanded grain controls:** 2–1,000 ms Grain Size, up to 2 seconds of Spray, and 0–200% Spread. Existing parameter values and double-click defaults are retained.
- **Smoother Freeze release:** resumed input/feedback is spliced to the frozen tail over 50 ms, removing a reproduced delayed click. Eight seconds of grain history cover long grains, maximum pitch, position and spray together.
- **Feedback stability:** sub-audible DC is removed before the stronger diffusion path to prevent buildup at high feedback.
- Includes the earlier pooled recording buffers, destructive-overdub splice fixes, idle-audio improvements, MIDI controls, custom UI and dedicated fullscreen FX page.

Existing projects/parameter settings retain their numeric values; the CLOUDY character at those values is intentionally stronger.

## Run

Extract the complete portable ZIP and run **Agent-Looper.exe**. Keep DLLs, tools and licenses together. Requires Windows x64 and .NET Framework 4.7.2 or later. Close older copies before using a single-client ASIO driver.

## Validation

Passed Windows real-time regression checks: stopped-output silence, mono Input 2 destructive recording at unity, fixed recording-block boundaries, arbitrary-position overdub splices, independent A/B sends, filters and frozen audio, copied frozen exports, expanded grain extremes, live frozen CLOUDY sweeps, and maximum positive/negative feedback with biased input. Hardware listening remains necessary for a particular interface and mixer setup.

This update distributes Windows software only. No Android APK, app bundle or signing key is included. See `SHA256SUMS.txt` for the portable ZIP checksum.
