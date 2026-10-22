> **Attribution correction (2026-10-22):** Development used a pre-existing third-party dark `ZuneShellResources.dll`. Its creator/source/license are unknown. The project did not create that original dark modification. Microsoft originals, inherited dark assets, and subsequent project changes are distinct layers. Current mixed-provenance release artifacts are **not cleared for redistribution**; see `CREDITS.md` and `docs/REDISTRIBUTION.md` in the staged repository. Earlier wording such as "our dark assets" describes retained/integrated content, not exclusive authorship.

# Enhanced Zune 4.8

Community enhancements for the original **x64 Zune 4.8.2345** desktop application on Windows 10/11. This is an enhancement project, not a replacement Zune application or a restoration of Microsoft's discontinued online services. It is not affiliated with Microsoft.

## What changed?

- Interface scaling, clearer controls, an optional dark theme and three custom background slots.
- Artwork-quality levels and local artist photos for desktop views.
- Local Mixview improvements and collection navigation.
- A renderer responsiveness repair, repeat-one playback, and optional crossfading.
- An installer with compatibility checks, verified backups, recovery, and prerequisite guidance.

See [how the changes work](docs/HOW-IT-WORKS.md), [source coverage](docs/SOURCE-COVERAGE.md), and [known limitations](KNOWN-LIMITATIONS.md). These are scoped improvements; claims of universally eliminated lag or restored online services would be inaccurate.

## Downloads and installation

The first publicly shared installer is **1.0.0-rc.6.2**. The project owner reports that the website still distributes that version. **RC6.2 has the repeat-one defect**: repeat one can block explicit track selection and, in some playlist cases, let audio advance while the previous item's information remains displayed.

**1.0.0-rc.7 is the latest preserved local update candidate, not the version currently published on the website.** Its changes include the repeat-one repair and optional crossfading. This repository preparation has not published RC7 or created a new installer version. At present this is a local staging repository; there is no public repository URL yet. See [publication status](docs/PUBLICATION-STATUS.md).

After publication, obtain `Enhanced-Zune-Setup-1.0.0-rc.7.exe` from this project's Releases page and compare its SHA-256 with the release checksums. Original Microsoft Zune installation media is not bundled. Setup supports the verified original-media download/install flow; missing Windows components may require Windows servicing, an Internet connection, or a restart.

Close Zune before installing or restoring. Existing earlier enhancement releases require **Restore originals**, followed by **Install / verify**, using the matching verified backup. Do not manually delete the backup or bypass an unknown-file warning. See [the original RC7 usage notes](docs/RC7-ORIGINAL-README.md).

## Source and rebuilds

Start with [BUILDING.md](BUILDING.md). The installer C# and newly authored repeat-one/crossfade implementation are browsable here. The matching frozen payload is distributed separately as a release asset to keep large binaries out of Git.

**This is not complete editable source for every historical enhancement.** Some earlier changes remain frozen binary deltas. Rebuilding the installer around those deltas is different from regenerating every patched component from source. [The coverage table](docs/SOURCE-COVERAGE.md) describes that distinction explicitly.

The original engineering package recorded a byte-identical installer rebuild. [Verification and its limits](docs/VERIFICATION.md) explain what that establishes and what it does not.

## Project history and participation

Git history begins with an import of the preserved RC7 engineering source, followed by repository documentation. Earlier development commits were not preserved here; no reconstructed timeline is presented as original history. See [CHANGELOG.md](CHANGELOG.md).

For problems, include the enhancement version, Windows version, actions that reproduce the issue, and whether crossfading is enabled. Remove usernames, music paths, device IDs and other personal information before sharing logs. Do not upload Microsoft installation files, private music, or signing keys. See [CONTRIBUTING.md](CONTRIBUTING.md).

## Licensing

Microsoft Zune remains proprietary. The repository's mixed source, patch and artwork provenance does **not** justify a blanket open-source license. Licensing of project-authored material remains to be decided before publication; dependency notices are retained. See [LICENSE.md](LICENSE.md) and [LICENSING-AND-SOURCES.md](LICENSING-AND-SOURCES.md).
