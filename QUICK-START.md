# Agent Looper 1.8.0 (Windows)

It is happening again...

FX type buttons now retain their screen positions while parameter panels collapse/expand or change size, so repeated double taps can cycle types at the same coordinates. Intentional serial-routing drag reorder still works.

## New FX types

Open FX and double-tap/click the effect type tab to cycle its types. Double-tapping a knob still resets that knob to its default.

**Tape Echo** is in the Delay selector after Crystal. TIME supports existing free/tempo divisions; FEEDBACK sets repeat decay. DRIVE adds soft saturation in the repeat path. WEAR adds wow and gentle level instability; FLUTTER adds faster pitch movement. TAPE TONE controls the repeat lowpass. Time changes slew through a bandlimited read head for tape-style pitch dives.

**Bloom** is in the Reverb selector after PS1 SPU. DECAY reaches 24 seconds. ATTACK spreads the rising reflection buildup from 20–1800 ms. DIFFUSION controls input scattering, MOTION adds subtle movement, and TONE colors the tail.

**Tuned** follows Bloom in the Reverb selector. ROOT selects C1–C6; SCALE chooses Harmonic, Minor, Major, Pentatonic, Fifths or Chromatic voicings. DECAY controls ringing; DAMPING darkens it. SPREAD places voices across stereo, EXCITE controls input drive, and DETUNE adds up to 25 cents of stereo detuning. It adds a tuned resonant return to your source, using the Reverb send level.

All new knobs support live changes, MIDI Learn, automation, modulation, default reset and project/scene saves. A/B send amounts stay independent; detailed effect settings retain the shared behavior of the existing FX.

## Automation

Use the visible **AUTO REC** button. In **Start / Finish** mode it begins immediately, and **FINISH** ends the take and sets its length to the actual elapsed recording time. There is no preset stop time. This is the default for new projects. Previously saved projects retain their selected mode; choose Start / Finish under Performance → Automation → Record length to change it.

Move effect or layer knobs during the take. Each touched control gets a separate lane; untouched lanes keep their existing curves. Free clips repeat at their own recorded lengths, independently of the audio loops. Follow A / Follow B remain available for takes synchronized to either looper. Beat-based and fixed-second lengths remain optional. Each lane can also have its own length mode.

Record changes to Finish with an elapsed-time readout; a quantized take shows Cancel while armed. The Windows automation buttons are centered in a fixed bottom bar, with status beneath them. The automation transport stays available on the FX page. Android uses a fixed footer in portrait and header controls in landscape. The adjacent AUTO button pauses/resumes automation without stopping audio. Open the ⋯ button or Performance → Automation for lane settings, bypass/delete or clear.

Windows: right-click the automation transport buttons for MIDI Learn. Android: hold them to assign a note or CC. Knob MIDI assignments also record during a take. Manual movement temporarily overrides a playing lane.

## Modulation

Performance → Modulation provides four assignable slots. Choose an effect or layer target, Sine / Triangle / Input envelope, depth, and tempo-beat or free-second rate. Slots aimed at the same parameter add together. Input envelope follows the selected live input level. Pitch/stretch use the existing live algorithms; Windows prepares native R3 processors off the audio callback.

## Scenes

Performance → Scenes has eight named slots. Capture the current loops, layer settings, FX settings, transport states and automation. Recall now or at A's next loop boundary; recall is immediate if A is stopped. A short transition crossfade smooths the change. Finish an audio recording before capturing/recalling scenes. Scenes are included in project saves. Reverb/delay tails and frozen grain buffer audio are not embedded in project files.

## Overdub feedback

Looper ⋯ → Recording → Overdub feedback. **100%** retains the old loop unchanged; lower values reduce older audio once per overdub pass. **0%** replaces old material with the new take. Works in layered and destructive modes, including automatic overdub. Feedback decay is separate from level automation and is included in Undo/Redo.

## MIDI clock

Timing & sync → MIDI clock. Choose an input device for **Follow external** or an output device for **Send clock**. Send and Follow are separate clock modes. MIDI timing clock uses 24 pulses per quarter note, with Start, Continue and Stop. Follow drives BPM and transport; tempo-synced FX, beat-based automation and LFO rates use that BPM. Free audio clips keep their chosen playback length. Clock loss keeps the last tempo; it does not suddenly alter your loops. Hardware sync remains dependent on the device driver and output latency.

## Projects and menus

The main menu is organized into Project, Performance, Timing & sync, and Settings. Audio import/export remain under Project. Windows now saves editable **.agw** projects; Android uses **.agl** projects. WAV export is audio only. Windows and Android project formats are currently separate.

Save and Save As include loops, layers, FX, feedback, automation, modulators and scenes. New/Load only ask to save if the project changed since the last successful save. Playback position does not count as an edit, and edits made while a save is writing remain unsaved. Android project-file export also acknowledges the successful save.

## Running

Windows: extract the whole ZIP, then run Agent-Looper.exe. Keep the DLLs and tools folder beside it. F11 toggles immersive fullscreen; Escape leaves fullscreen. Windows source is in source/. Run source/build.ps1 to rebuild beside the runtime DLLs as Agent-Looper-Rebuilt.exe. Android: install the signed APK as an update to your current app. Windows version 1.8.0. Android remains 1.6.1 / code 53.

These builds are local/private. No APK was published to GitHub. macOS is unchanged.


## LFO modulation

Click/tap the main LFO square to open a rounded picker with four numbered sources. Each square previews its selected waveform. Drag a numbered square onto a knob to add a destination. Repeating on another knob keeps previous assignments. Each LFO supports up to 32 destinations, sharing its waveform, depth, rate, polarity and phase.

Double-click/tap a numbered square to open a rounded settings panel. Depth, Rate and Phase use labelled live sliders with visible numbers.
Windows: TARGETS… lists destinations to remove and offers Add destination.
Android: Destinations lists assignments; tap one to remove it.
Disable restores all target baselines; Unlink removes all assignments. Settings and destinations save with projects. Older projects load; projects saved by this update require the updated app.

Windows: extract the entire ZIP and run Agent-Looper.exe. Android APK remains private. Nothing published.


## External VST3 effects

Right-click an FX tab and choose Replace with VST3… . Select an installed 64-bit stereo effect. Browse supports both .vst3 files and bundle folders.

The plugin editor opens directly inside the FX controls area. Resizable/scalable editors fit the available space where the plugin supports it; POP OUT remains available for larger fixed-size editors. Double-clicking an external-effect tab also pops out its editor. Return to the built-in effect from the tab's right-click menu.

The slot level becomes wet/dry mix and retains its MIDI/LFO/automation assignment. Native plugin parameters are edited in the plugin UI; direct host automation/LFO assignment of those external parameters is not yet supported. BYPASS retains latency alignment.

Replacements apply to the selected A SENDS or B SENDS bank, with separate plugin instances. Existing built-in detailed settings and serial order remain shared. Projects and scenes store plugin paths, bypass and parameter state. Plugins must remain installed; their binaries and effect tails are not embedded in the project. Missing plugins retain their saved state and pass dry audio with an error shown.

This preview hosts plugins inside the app process; a plugin crash can close the app. There is no plugin crash sandbox. Stereo effects only: instruments, sidechains and plugin MIDI input are not included. Multi-effect bundles currently use the first effect class. Changes to plugin latency or bus layout require Reload plugin from the menu.

The rack adds 64 samples (about 1.33 ms), plus reported plugin latency. Wet/dry and parallel branches within a rack are aligned. Separate A/B banks are not automatically delay-compensated against each other.

Extract the ENTIRE ZIP and run Agent-Looper.exe. Keep AgentVst3.dll, other DLLs, tools and licenses together. New projects saved by this version require this update; earlier Windows projects remain readable.

Validated with TAL-Reverb-2, TAL-Chorus-LX and ValhallaSupermassive; editor attachment/resizing/tab switches/popout/reattachment; latency fixtures for all routing modes; project/save/dirty/missing-plugin checks; 60 seconds of simulated silent three-plugin processing; existing performance/session/audio checks. Physical interface listening has not been repeated. Android and Mac are unchanged. Nothing published.
