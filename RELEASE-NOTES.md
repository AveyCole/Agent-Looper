# Agent Looper 1.2.1 — Windows

- Independent A/B Reverb, Delay and Grain sends, plus independent filters. Effect types, detailed settings, Freeze and routing remain shared.
- Dedicated FX page, centered knob labels, and serial effect routing ordered by the visible tabs. Hold a tab for one second, then drag it to reorder.
- Headphone button for input monitoring; actionable ASIO busy/unavailable messages and automatic retry.
- Distinct curved Start/End waveform grips. Crossing the grips reverses the selected region; Escape cancels a Windows drag.
- Stopped layers and silent FX skip unnecessary processing. Native audio workers retire away from the UI, and rebuilt FX editors release their resources.
- Fixed-size pooled recording blocks replace growing capture buffers. Destructive overdubs started mid-loop receive splice smoothing before merging. Raw samples are not normalized or boosted.

Extract the complete portable ZIP and run **Agent-Looper.exe**. Close older copies before using a single-client ASIO driver. Keep DLLs, tools and licenses together. Requires Windows x64 and .NET Framework 4.7.2 or later.

Validation passed: capture/playback unity, mono Input 2 destructive sums, exact capture block boundaries, automatic overdub, quantized/timed dual capture, history, independent sends/filters in all routing modes, waveform/reverse gestures, float-quality checks, fullscreen UI, and stopped-audio silence. The reproduced destructive splice jump fell from 1.4 to below 0.0046 per sample. Hardware listening remains necessary to confirm behavior with a particular interface.

This update distributes Windows software only. No Android APK, app bundle or signing key is included. See `SHA256SUMS.txt` for the portable ZIP checksum.
