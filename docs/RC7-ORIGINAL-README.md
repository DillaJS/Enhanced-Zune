> **Attribution correction (2026-10-22):** Development used a pre-existing third-party dark `ZuneShellResources.dll`. Its creator/source/license are unknown. The project did not create that original dark modification. Microsoft originals, inherited dark assets, and subsequent project changes are distinct layers. Current mixed-provenance release artifacts are **not cleared for redistribution**; see `CREDITS.md` and `docs/REDISTRIBUTION.md` in the staged repository. Earlier wording such as "our dark assets" describes retained/integrated content, not exclusive authorship.

# Enhanced Zune 1.0.0 RC7

For original x64 Zune 4.8.2345 on Windows 10/11. This package contains enhancements and optional crossfading; original Microsoft Zune installation media is not bundled.

Run `Enhanced-Zune-Setup-1.0.0-rc.7.exe`. If Zune is absent, the setup offers the verified original-media download/install flow. Windows Media Player Legacy and .NET Framework 3.5 prerequisite handling is retained. Close Zune before installing or restoring.

**Upgrading RC6/RC6.2:** use this setup's **Restore originals** with the existing verified backup, then **Install / verify**. Unknown manual modifications remain protected and may require their own matching backup. The installer does not guess or overwrite them.

## Crossfading

Open **Settings → Software → General → Crossfading**. It starts **off**. Enable it, choose 0–10 seconds of overlap, then click **OK** or **Apply**. It applies to supported local music files. Videos/streams stay with native playback. Repeat one does not overlap another track. Selecting another track explicitly starts that selection.

The Zune player continues to manage the playlist and UI while a separate local audio component renders overlaps. Pause/resume, seeking and volume are synchronized. Decoder/host failures return to native audio. This follows WMP-style on/off and overlap controls; no claim is made of an identical proprietary fade curve.

A self-contained x64 .NET runtime is embedded in the audio helper. Users do not install a separate .NET 8 runtime. The helper has no startup task or service and exits with Zune.

Profile/sign-in emulation and internet blocking are not included. Existing library files, artist-photo data and personal files are outside the installation transaction.

## Source and verification

The matching engineering-source ZIP rebuilds this exact installer from its included frozen payload, using the recorded toolchain. It also includes the newly authored crossfade bridge/audio engine, repeat-one repair, UI markup and tests. Historical enhancement patches and Microsoft internals are not represented as complete open-source code; see REBUILD.md and LICENSING-AND-SOURCES.md.

Checks distinguish isolated installation/recovery and synthetic audio/bridge tests from actual OS or listening checks. The user confirmed the separate playback test locally. This packaging pass does not reinstall or control the working Zune.
