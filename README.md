# Enhanced Zune 4.8

Enhancements for the original **Microsoft Zune 4.8.2345 x64** desktop application on **Windows 10 and 11**. Familiar Zune, with display, customization, responsiveness and playback improvementsâ€”not a new player or a restoration of discontinued online services.

[Download RC7](https://github.com/DillaJS/Enhanced-Zune/releases/tag/v1.0.0-rc.7) آ· [Installation](INSTALLING.md) آ· [Features](FEATURES.md) آ· [Build from source](BUILDING.md) آ· [Report a problem](https://github.com/DillaJS/Enhanced-Zune/issues/new/choose)

![Enhanced Zune Collection with the optional dark theme](docs/images/collection-dark.png)

## What's improved?

- **Display:** interface scaling from 100% to 400%, clearer controls and artwork, an optional dark theme, and three custom-background slots.
- **Artwork:** Basic, Balanced and Premium decode-quality choices; local artist photos; improved local Mixview artwork and navigation.
- **Responsiveness:** a renderer fairness repair addressing the investigated input stalls, while retaining graphics choices and animations.
- **Playback:** repeat-current-item controls; RC7 repairs repeat-one selection/playlist behavior and adds optional crossfading with a 0â€“10 second control.
- **Installation:** original-media extraction/verification, Windows prerequisite assistance, verified program-file backups, repair and recovery.

See the [feature list](FEATURES.md), [screenshots and comparisons](docs/SCREENSHOTS.md), and [technical explanations](docs/HOW-IT-WORKS.md). There is no claim of universally eliminated lag, restored sign-in/Marketplace services, or device artist-photo synchronization.

## Versions

| Version | Status and important difference |
| --- | --- |
| **RC6.2** | First publicly shared version. Includes a known Repeat One defect: explicit track selection can be blocked, or playlist audio can advance while old metadata stays visible. Preserved as the historical baseline. |
| **RC7** | Newer published release candidate. Repairs Repeat One and adds optional crossfading, off by default. [RC7 downloads](https://github.com/DillaJS/Enhanced-Zune/releases/tag/v1.0.0-rc.7). |

The existing website still offers RC6.2 according to the project owner. Preparing this repository does not update that website. See [changelog](CHANGELOG.md) and [release status](docs/PUBLICATION-STATUS.md). Both versions are release candidates.

## Install or update

Use the appropriate installer from [Releases](https://github.com/DillaJS/Enhanced-Zune/releases), and check its SHA-256 against that release's `SHA256SUMS.txt`. Original Microsoft installation media is not bundled.

Close Zune first. When updating a previous enhancement version, use **Restore originals**, then **Install / verify**, keeping the verified backup. Do not manually delete or bypass an unknown-file warning. Read [INSTALLING.md](INSTALLING.md) for first installation, updates, prerequisites and recovery.

Crossfading in RC7 is under **Settings â†’ Software â†’ General â†’ Crossfading**. It starts off, supports local audio, and follows pause, seek, volume and mute. It does not overlap a different track in Repeat One mode. The helper's runtime is bundled; a separate .NET 8 installation is not needed to use it.

## Source transparency

This repository contains installer source and the editable repeat-one repair/crossfade implementation, build scripts, tests and documentation. **Some older enhancements remain frozen binary patches, not complete editable source.** Recompiling the installer around those preserved patches is different from regenerating every patched component from source. [SOURCE-COVERAGE.md](docs/SOURCE-COVERAGE.md) identifies the available source and gaps.

The documented toolchain reproduces the preserved RC7 installer byte for byte. See [BUILDING.md](BUILDING.md) and [verification limits](docs/VERIFICATION.md). Reproducibility is useful evidence, not a blanket safety guarantee.

Git history preserves the local repository-preparation checkpoints. Release snapshots are imported from preserved packages; no invented earlier development history or backdated commits are presented.

## Credits and license

Microsoft and its licensors created Zune and its original assets. Our development started from a **pre-existing third-party dark-themed resource DLL, creator unknown**. The project owner did not create that original dark modification. Our later fixes, integration and additions are distinguished in [CREDITS.md](CREDITS.md) and the [provenance audit](docs/ATTRIBUTION-AUDIT.md).

**Our own contributions use [0BSD](LICENSE): use, modify and redistribute them without requesting permission or giving the project owner credit.** This does not license Microsoft's components, inherited dark-theme work, dependencies or other third-party assets. See [LICENSE-SCOPE.md](LICENSE-SCOPE.md) and [licensing disclosures](LICENSING-AND-SOURCES.md).

This is an independent community project, not affiliated with or supported by Microsoft. Bug reports and feature requests are welcome through [Issues](https://github.com/DillaJS/Enhanced-Zune/issues); remove personal information before attaching logs or screenshots.
