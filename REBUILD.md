# Rebuild RC7

## Reproduce the installer

Extract the matching engineering-source ZIP on x64 Windows. Use .NET SDK **8.0.425** and the Windows .NET Framework 4.x references expected by `Build-Setup.ps1`. Run that script. It compiles `dist/Enhanced-Zune-Setup-1.0.0-rc.7.exe` from the installer C# source, manifest and exact included `build/payload.zip`. A recorded build from a second directory must match the delivered EXE byte for byte.

This step does not need original Microsoft DLLs, Python, Mono.Cecil, a signing key, or a separate original-Zune download. Installing still requires legitimate original Zune files.

## New enhancement source

`Enhancements/CrossfadeHost` contains the complete audio host/mixer/output-control source and its pinned NuGet lockfile. Publish with `dotnet publish Host.csproj -c Release`; the project targets win-x64, bundles runtime 8.0.31, uses NAudio 2.2.1 and compresses the self-contained single-file executable. Changing toolchain/runtime versions requires fresh package and playback validation.

`Enhancements/Bridge` contains the CLR 2 bridge, General settings model and UI markup. The release uses `HKCU\Software\EnhancedZune\Playback`, off by default. No test profile or user identity is embedded. It expects the audio helper beside Zune.exe.

`Enhancements/RepeatOne` contains the full source of the repaired repeat feature. `RepeatOnePatch` and `CrossfadePatch` contain scoped Mono.Cecil patch source. They consume supplied legitimate pre-patch files and a signing key argument. The private signing key is not distributed. Rebuilding patched signed runtime components with a different key changes their hashes/identities and requires updating references, payload hashes and verification. The frozen payload is sufficient for an exact installer rebuild without doing that.

`Enhancements/Tests` contains deterministic mixer/output tests, generated-tone MP3/WMA fixtures, a CLR 2 simulated bridge/transport test and repeat-one tests. They do not require opening Zune or listening. `dotnet run --project Enhancements/Tests/Tests.csproj -- <report-folder>` runs audio rendering/output-controller tests without opening an audio device. Test fixtures contain synthetic tones, not user music.

## Frozen historical components and private fixtures

The engineering archive includes installer source, newly added enhancement source and audited frozen binary deltas. It does not contain the complete historical enhancement worktree or Microsoft Zune source. It does not include full Microsoft Zune DLLs, original installation media, personal backgrounds, user photos, private signing keys, live registry dumps or private test snapshots.

`Build-Payload.py` / `Build-RC7-Payload.py` regenerate the payload only in the private engineering workspace with legitimate original-media fixtures, the prior RC6 payload, verified enhancement outputs and Python/pefile. `PayloadCodec.py` contains the delta encoder. Installer transaction tests similarly require private legally obtained original-file fixtures. These paths are deliberately not represented as a standalone full-runtime source build.

`VERIFICATION.json` records this release's actual checks and limits. Prior-release test output is not reused as evidence that RC7's new payload was installed or restored successfully.
