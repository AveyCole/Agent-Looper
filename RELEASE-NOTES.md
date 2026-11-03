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
