# Rebuild and evidence

The engineering archive contains the installer source, build script, tests and audited frozen patch payload. It is **installer engineering source**, not a replacement for Microsoft's source or the complete historical enhancement worktree.

To reproduce the EXE, use x64 Windows with .NET SDK **8.0.425** and the .NET Framework 4.x runtime/reference files used by `Build-Setup.ps1`. Run that script from the extracted engineering archive. It compiles a Windows Forms EXE with the embedded `build/payload.zip`, a deterministic compiler configuration, fixed manifest/version, and normalized source paths. No original Microsoft DLLs are needed for this installer-compilation step. Installing the result still requires the user's verified original Zune files.

A build from a different directory was actually compared and was byte-identical on the recorded toolchain. Reproducibility across different compiler/runtime-reference versions has not been certified.

Regenerating the patch payload is a separate operation. `Reconstruct.py` and `Build-Payload.py` use the private approved working snapshot, legitimate original media, stock extraction map, source release policy, Python and pefile. Those private Microsoft binaries and personal snapshots are deliberately omitted from the engineering archive. The exact final program hashes, complete resource identifiers, privacy normalization and public candidate hashes are recorded in the release package.

The transaction tests use disposable copies of legitimately extracted originals. `Test-Setup.ps1` expects this project's private `Zune-Community-Package/installer-extracted` and file map; those fixtures are not redistributable test data. Preference tests redirect the test process's HKCU into a unique temporary key and use a separate settings-backup directory. They do not change the real Zune settings. UI render tests and the EXE's read-only `--check-ui` engineering mode do not invoke installation.

`QUALITY-RESULTS.json`, `ISOLATED-TEST-RESULTS.json`, `SETUP-SMOKE-TEST.json`, `PRESERVATION-RESULTS.json` and `RELEASE-POLICY-RESULTS.json` distinguish completed checks from laptop/OS tests still required.


RC3 refresh notes: Reconstruct.py snapshots the current installation read-only. The owned helper defaults are patched in the private snapshot by DefaultsPatch, and Patch-Native-Default.py changes only the missing-value branch in the native DPI stub. Test-Runtime-Defaults.cs exercises candidate managed defaults against a process-private registry with the normal SettingsRegistryPath and an isolated Management/commit-list model. Test-Native-Default.py executes the candidate native DPI stub with a process-private registry. These do not launch or modify the reference Zune.

Test-Setup.ps1 uses the retained private RC2 payload as a legacy fixture; Test-Legacy.ps1 additionally exercises both RC1 and RC2 to exercise same-EXE verified recovery. That old fixture and Microsoft's original-file fixtures are not shipped in the engineering archive. The frozen payload is sufficient to rebuild the installer itself without regenerating enhancements.


RC3 adds Prerequisites.cs to the installer compilation and references the Windows .NET Framework System.Management assembly for a read-only optional-feature query. Test-Prerequisites.ps1 records decision/routing tests, live read-only detection and an off-screen UI render. Missing-component and 26H1 decisions are simulated; actual OS installations remain pending.

The frozen payload now includes the final ZuneLocalArtistPhotos.dll as a third added helper. The release-only helper normalization redirects exactly two loader sites from a per-user code location to the helper installation directory, using the original CLR 2.0 core-library scope. Artist photo data remains in user-controlled folders. The working per-user helper is snapshotted and hash-verified unchanged. The defaults/loader method audit is retained privately. Test-Runtime-Defaults.cs verifies five preference modes and installation-relative photo loading using the candidate DLLs.

To run available tests in the private project, run Quality-Checks.ps1, Test-Prerequisites.ps1 and Test-Legacy.ps1. The last two require their included sources; legacy fixtures are private prior payloads. Reusing transaction evidence is only appropriate when installer engine/payload bytes have not changed since the passing transaction run. Finalization checks the current EXE and embedded payload hashes, reference file preservation and privacy before producing delivery archives.


RC4 preserves RC3 runtime patch/helper bytes. Its manifest version and recovery metadata change; the EXE adds WindowsComponents.cs plus an embedded WindowsComponents.ps1 resource. Include both in deterministic rebuilds. The script is loaded from the EXE, not an editable temporary script. It runs only from the explicit elevated --windows-components flow. A named setup mutex prevents overlapping enhancement/Windows-component operations.

Test-Completion.ps1 exercises launch policy and renders completion, restore, restart and failure screens in preview mode without starting Zune. Test-WindowsComponents.ps1 executes the exact production script in non-elevated child PowerShell processes with all Windows registry/feature/capability functions replaced by test functions. It does not service Windows. Test-Legacy.ps1 verifies RC1/RC2 recovery and RC3 identical-runtime/no-op/recovery. The recorded script SHA-256 identifies the script actually tested.


RC5 freezes the scoped Now Playing Mixview resource changes and final signed photo component from the audited Zune-NowPlaying-Mixview stage. Other runtime components retain their RC4 candidate bytes. Recovery metadata now includes RC1 through RC4. The component method audit preserved a final post-compiled EnsureWatcher repair; do not rebuild that component from the older source and omit the preservation step.
