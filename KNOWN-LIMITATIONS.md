# Known limitations

This is a private test candidate. A successful prerequisite check does not prove a clean Zune installation or playback works on every Windows build.

- Actual missing-.NET, missing-media, Windows N and Windows 11 26H1 standalone installation/reboot scenarios need disposable-system tests. Automated tests cover decisions/routing, native file inspection, read-only detection on Windows 11 25H2, and the production servicing script with all Windows queries/mutations replaced by mocks. Actual feature installation/download/UAC/reboot remains untested.
- The .NET check uses Microsoft's installed-product registry record. Media checks inspect required x64 Windows DLLs and the optional-feature state where reported. They do not fully validate codecs, COM registration, file signatures or servicing health. A Windows version that does not report the player feature is evaluated using runtime files.
- Setup installs supported Windows optional components only after the user chooses Install missing components and approves the operation. Microsoft instructions may need Internet access or matching Windows media. Media Feature Pack always needs a restart, even without a prompt. Other pending Windows restarts are advisory.
- Original media must match verified x64 Zune 4.8.2345. Windows x86/ARM64 are unsupported. No original Microsoft setup/DLL files are bundled in this package. The separate ZIP or EXE must match the supported package hash; other versions fail clearly.
- Earlier-release updates require Restore originals, then Install / verify, with a valid existing backup. Unknown local modifications are protected. Restore exports custom slot backgrounds; users may need to reselect them afterward.
- The reference PC has existing strong-name verification overrides. Setup does not install them; a successful run here is not clean-runtime certification. Laptop launch/clean install/UAC/reboot/device acceptance and distribution-rights review remain open.
- The brief unenhanced placeholder during rapid skipping remains deliberately unchanged. Artist photos are desktop-only and are not synchronized as device artist metadata. Discontinued online Zune services are not restored.

- Updates from RC1 through RC6 use verified Restore originals, then Install / verify.
- Assisted servicing depends on Windows offering the applicable capability/feature and having a usable source. Managed update policies are respected; setup does not bypass them. A restart may require reopening setup and a second component-install pass.
- Setup may need Internet access to install prerequisites. No Windows payloads are bundled, and build 28000+ .NET is installed separately through Microsoft's instructions.
- Automatic Zune launch is suppressed when setup itself was elevated, to avoid running Zune as administrator. Close setup and use Start in that case. Automatic launching itself has not been exercised against the working Zune; launch policy and completion screens were tested without opening it.

- The user confirmed the local Mixview fixes before RC6 packaging. Local album seeding still requires a local library album ID.

- ZIP and EXE extraction, all 56 media hashes, altered downloads, file locks and cleanup were tested without running Microsoft setup. A full new installation on a disposable Windows system remains untested. The browser download depends on the community archive staying available. Downloads can also be selected manually.


## Crossfading

Crossfading applies to readable local music and falls back to native playback for unsupported/failed decoding, videos and streams. It is optional and off by default. Repeat one uses native repeating without overlap. This follows WMP-style controls; exact proprietary fade-curve equivalence is not claimed. The separate audio helper may appear separately in Windows per-app volume controls. Bundled runtime versions are pinned; changing them requires rebuilding and revalidating the package. The user confirmed the earlier isolated playback build locally; packaging checks here do not constitute a fresh clean-machine listening test.
