# Agent Looper 1.2.6 — Windows

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
