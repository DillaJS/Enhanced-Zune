# Building the preserved RC7 installer

## Exact installer rebuild

Use x64 Windows, **.NET SDK 8.0.425**, and the Windows .NET Framework 4.x reference/runtime files expected by `Build-Setup.ps1`. The recorded script deliberately pins the compiler path. Changing compiler/reference inputs can change the output.

1. Obtain `Enhanced-Zune-1.0.0-rc.7-payload.zip` from the matching release assets. Check its hash against `release-manifest.json`.
2. Create `build` in the repository root and copy that file to `build/payload.zip`. Do not extract the inner payload.
3. From PowerShell in the repository root, run `./Build-Setup.ps1`.
4. Compare `dist/Enhanced-Zune-Setup-1.0.0-rc.7.exe` with the installer hash in `release-manifest.json`.

This compiles the installer only. It does not launch it, modify installed Zune, regenerate all historical patches, or install prerequisites. The original engineering ZIP already includes the same payload if preferred.

## Editable playback implementation

`Enhancements/CrossfadeHost` contains the audio helper. It pins NAudio 2.2.1 and runtime 8.0.31. Restore the host with `dotnet restore Enhancements/CrossfadeHost/Host.csproj --locked-mode`, then publish with `dotnet publish Enhancements/CrossfadeHost/Host.csproj -c Release --no-restore`. Package restore may access NuGet. Review pinned dependencies before using changed versions.

The simulated audio/output tests run with:

```powershell
dotnet run --project Enhancements/Tests/Tests.csproj -- ./artifacts/audio-tests
```

They generate tone files and use a simulated output device; they do not open Zune or exercise a real audio device. That command covers the tests included by `Tests.csproj`, not every other `.cs` fixture in the directory. Historical bridge/repeat fixtures need their original CLR 2 test harness and supplied references; see [REBUILD.md](REBUILD.md).

## Separate, incomplete historical payload regeneration

`Build-RC7-Payload.py`, `Reconstruct.py`, `Finalize.py` and several transaction-test scripts reference the original private engineering workspace. They are retained for inspection, **not** advertised as standalone public build commands. Some inspect an installed Zune or require Microsoft files and prior-release fixtures. Do not run them against a working installation to try to fix missing dependencies.

The public installer rebuild does not require those commands. Full regeneration of modified signed components additionally requires compatible original inputs, reference assemblies, resources and a signing key. A replacement key changes identities/hashes. Original private keys and Microsoft binaries are not supplied.

For full historical detail, read [REBUILD.md](REBUILD.md) and [source coverage](docs/SOURCE-COVERAGE.md).
