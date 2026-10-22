# Enhanced Zune 4.8 — RC6.2 historical source snapshot

This branch imports the preserved engineering source for **the first publicly shared installer**, RC6.2. It contains the known Repeat One selection/playlist defect and does not include RC7's optional crossfading or repeat repair. For the newer candidate and main documentation, see [the main branch](https://github.com/DillaJS/Enhanced-Zune/tree/main).

This is a source snapshot imported during repository preparation, not a claim that original development happened in this Git branch or at its commit date. Earlier local preparation history is preserved in its ancestry.

## Build

Use x64 Windows, .NET SDK **8.0.425**, and the Windows .NET Framework 4.x references expected by `Build-Setup.ps1`. Obtain the matching RC6.2 payload asset, place it at `build/payload.zip`, then run `./Build-Setup.ps1`. Output: `dist/Enhanced-Zune-Setup-1.0.0-rc.6.2.exe`.

The installer sources and frozen payload match the preserved release inputs. A test-fixture path was made portable in `Test-Media.ps1`; it is not compiled into the installer. The engineering-source download includes the same payload. See [REBUILD.md](REBUILD.md) for the historical fixture requirements.

## Source, credits and license

Editable installer source is available here. Older runtime enhancements are preserved as binary patches, not complete editable source. The repeat-one repair and crossfade source belong to the later RC7 snapshot on main. Shared technical documents may discuss that later work; they are not claims that RC6.2 includes it.

Our own contributions use [0BSD](LICENSE), scoped by [LICENSE-SCOPE.md](LICENSE-SCOPE.md). Microsoft created original Zune; development used a pre-existing third-party dark resource modification whose creator remains unknown. The project owner did not create that inherited dark artwork. See [CREDITS.md](CREDITS.md).

[Releases](https://github.com/DillaJS/Enhanced-Zune/releases) · [Installation](INSTALLING.md) · [Changelog](CHANGELOG.md) · [Issues](https://github.com/DillaJS/Enhanced-Zune/issues)
