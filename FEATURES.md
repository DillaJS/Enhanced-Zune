# Features

## Display and customization

- Automatic Windows-DPI startup scaling, or 100%, 150%, 200%, 250%, 300%, 350% and 400% interface scaling.
- Optional dark palette with text/control adjustments; inherited dark artwork is credited separately. Selected dialogs and protected views retain native styling.
- Three persistent custom background slots, with image validation, queued save/discard behavior and combined restart notices.
- Clearer controls, placeholders and selected artwork assets; original layout and screen-graphics choices remain available.
- Basic, Balanced and Premium artwork decode-quality levels. A higher setting cannot restore detail absent from the source image.

## Local artist photos and Mixview

- Artist-photo folders managed through Display settings; photos live directly inside each artist's folder.
- Windows logical filename ordering chooses the primary photo used by Quickplay; Now Playing refreshes the current artist when entered.
- Local Mixview album/photo selection, randomized surrounding items, quality-aware artwork, and Go to navigation into the collection.
- Artist-photo transfer to Zune devices is not supplied. Online catalog, sign-in and subscription services are not restored.

## Responsiveness

The renderer fairness fix returns to the Windows message loop after a complete render pass, addressing the investigated input stalls. It is not an Internet-blocking change and does not claim to solve every startup or machine-specific slowdown.

## Playback

- Repeat cycles through off, repeat list and repeat current item, with a numbered indicator and matching tooltips.
- **RC6.2 has a Repeat One defect. RC7 repairs it** so explicitly selecting another item works and playlist audio remains aligned with the displayed item.
- **RC7:** optional crossfading under Software → General, off by default, with a 0–10 second overlap setting. Supported local music only; pause, seek, mute and volume are synchronized. Unsupported/failed host playback returns to native audio. Repeat One does not overlap a different track.
- Crossfading follows WMP-style controls; identical proprietary fade curves are not claimed.

## Installer and recovery

- Checks supported original x64 Zune files and expected hashes; protects unrecognized modifications.
- Verifies and stages changes, backs up affected original program files, and keeps a recovery journal.
- Repair/restoration and interrupted-operation recovery; custom-background-aware checks and exports.
- Windows media/.NET prerequisite detection and explicit servicing assistance, without automatic Windows restarts.
- Supports the known original Zune ZIP/EXE and extracts/validates its media; original Microsoft setup runs separately in local mode.

Read [installation instructions](INSTALLING.md), [known limitations](KNOWN-LIMITATIONS.md), [source coverage](docs/SOURCE-COVERAGE.md), and [credits](CREDITS.md). The [historical feature inventory](docs/HISTORICAL-FEATURE-INVENTORY.md) preserves more detailed development notes; it is not a claim that all underlying artwork originated here.
