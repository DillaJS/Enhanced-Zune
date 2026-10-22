# Licensing and sources

## Project contributions: 0BSD

Our own code and original contributions are licensed under [0BSD](LICENSE), with scope defined in [LICENSE-SCOPE.md](LICENSE-SCOPE.md). The license does not require credit to the project owner or separate permission to use, modify or redistribute those contributions.

## Microsoft Zune

The original application, framework, branding, fonts, resources and Microsoft-origin portions of derivatives retain their existing rights. Original installation media and full original Microsoft DLLs are not bundled as separate release inputs. Shipping binary deltas does not by itself establish rights over reconstructed material. Microsoft's [general software terms](https://www.microsoft.com/en-us/legal/terms-of-use) refer to the accompanying product license; those website terms are not a replacement Zune-specific license.

## Inherited dark modification

Development began with an already modified dark `ZuneShellResources.dll`. Creator, original download source and redistribution license remain unidentified. The project owner did not create this work. Attribution and resource-level evidence are retained in [CREDITS.md](CREDITS.md) and [the audit](docs/ATTRIBUTION-AUDIT.md). No 0BSD grant or permission from that unknown creator is implied.

## Dependencies

- NAudio 2.2.1: upstream MIT license.
- Bundled .NET/Windows Desktop runtime 8.0.31: upstream licenses and third-party notices.
- Build-time Mono.Cecil package references retain their upstream terms; the scoped patcher projects pin their packages.

Required audio-runtime notices are included in `ZuneCrossfade-Licenses.txt`. Dependency licenses do not license Microsoft Zune or the inherited dark material. Screenshots are demonstrations; depicted album covers, photos and branding are not supplied as licensed reusable assets.

## Preserved release archives

Installers and frozen payloads retain their preserved bytes. The GitHub engineering-source packages add current license/credits documentation and normalize one RC6.2 test-fixture path; compiled installer inputs are unchanged. Original local archives remain untouched. Older documentation inside the packages records the earlier licensing stage; the current `LICENSE` and `LICENSE-SCOPE.md` supply the project's own 0BSD grant. Source availability varies by component; see [source coverage](docs/SOURCE-COVERAGE.md).
