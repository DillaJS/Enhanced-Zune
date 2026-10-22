# Build RC6.2

This historical branch builds **RC6.2**, not RC7. See the [branch README](README.md) for the exact version and command, and [REBUILD.md](REBUILD.md) for preserved engineering instructions.

Requirements: x64 Windows, .NET SDK 8.0.425, Windows .NET Framework 4.x references, and the matching release's frozen `build/payload.zip`. Run `./Build-Setup.ps1` and compare the EXE's SHA-256 to `910D70A40DFC17190FCE2FF3531B10BC2BF249167CA5C3D9BC7ED63797AFD67F`.

The build compiles the installer; it does not install Zune or regenerate every historical runtime patch. Private original-file/old-payload-dependent tests are separate and should not be run against a working installation. RC7's editable playback implementation and audio tests are available on main, not in this older snapshot.
