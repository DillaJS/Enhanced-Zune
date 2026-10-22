# Restoration included in RC5

The installed files, not old experiment folders, were the authority. `RESOURCE-CHANGES.json` supplies the exhaustive changed resource identifiers; `release-manifest.json` supplies exact program hashes. The private source snapshot records all installed files for preservation checks.

| Area | Included behavior | Evidence / provenance |
|---|---|---|
| Responsiveness | Native renderer fairness fix returns to the message loop after a complete render pass, addressing the previously measured long input stalls. Premium rendering and animations remain available. No claim of faster startup or zero latency. | Native 4K fix, preserved in the current scaling renderer |
| Native DPI | Application DPI compatibility option avoids Windows bitmap enlargement. Native text rendering is retained. | Native 4K / independent scaling investigations; installed renderer and UIX |
| Interface scaling | Auto (Windows), 100%, 150%, 200%, 250%, 300%, 350%, 400%. Native save/discard/cancel handling and restart notice. Auto is a startup desktop-DPI choice, not live per-monitor DPI switching. | Interface Scaling / Finish / Fifty-Percent-Steps |
| Artwork quality | Basic, Balanced, Premium decode bounds for visible local album-art consumers, notifications, Now Playing/Mini Player covers, and initial metadata-dialog covers. Keeps native layout sizes. Does not invent detail absent from a source image. | Album Art Investigation / Quality Slider; Artwork Coverage Audit |
| Display settings | Single column, in order: Background; Dark theme; Artist photos; Screen graphics; Album artwork quality; Now playing; Interface scaling. | Display Order, replacing the rejected compact layout |
| Dark theme | One checkbox: **Dark negative versions of the original backgrounds, and adjusted text and controls for dark backgrounds in general**. Selected text/control palettes and main-screen highlight levels switch together. Pink accents and original pinkish text are protected. Native button-caption colors are retained where required. | Interface Colors staged fixes; Theme Corrections; current UIX hooks |
| Protected surfaces | Original white dialogs, warnings, context menus and dropdowns retained. Device, Quickplay, Now Playing and Mini Player remain protected from broad text-theme inversion. Explicit artwork-placeholder exceptions are retained. | White-dialog rollback; Device Theme Exclusion; placeholder correction |
| Disc | Disc text, highlights and themed controls follow Dark theme. Original text metrics, row heights and layout remain. | Disc Interface Fix / Disc Fixes Reapplied |
| Artwork placeholders | All 13 NOART resources have the accepted enhanced light/dark versions. Includes disc/burn, metadata, wide/multiple-album and Now Playing forms. The tiny Now Playing logo uses its own original proportions. Edit Album Info artwork follows Dark theme while the dialog stays white. | All Placeholders; Placeholder View Correction |
| Custom backgrounds | Three persistent custom slots, JPEG/PNG picker, removal, native background fitting, validation/size limits and original save/cancel behavior. DLL-backed persistence, queued background operations and one combined Restart / Later prompt after saving are included. No imported user pictures are shipped. | DLL Custom Backgrounds / Automatic Restart / Automatic Discard; current helper |
| Built-in backgrounds | Exact final installed built-in background resources, including later original/white restorations. No abandoned 4K experiment is substituted and no alternate asset package is invented. | Final installed resource payloads; Original/White Backgrounds Restore |
| Window frame | Accepted pink-border correction and neutral gray window-control states, including normal, hover and pressed shades. | Pink Border; Neutral Window Controls |
| Enhanced controls/assets | Final accepted playback/play/pause/skip, slideshow and picture controls, collection actions and hearts, volume/mute, rating, scrollbars, selection controls, search, toolbar computer/disc/playlist artwork, drag/status/attention symbols, HD badge, sync/device artwork and other accepted resource upgrades. Exact current assets and state opacity are preserved. | Current resource hashes; Asset Upgrade reports; Collection Button Theme corrections |
| Other device families | Unfilled/disconnected toolbar icons receive color-only light/dark pairs across generic, mobile and supported device families. Filled/connected/color/syncing variants remain neutral. Unseen hardware icons were not redrawn. | Toolbar Outline Palettes: 56 outline sources, 320 filled sources preserved |
| Page navigation | PAGE.PREV / PAGE.NEXT and their states have color-only light/dark variants, restricted to appropriate main-screen owners. | Page Arrow Palettes: 12 sources |
| More / Smart DJ | Existing enhanced More arrow retained; More and inline Smart DJ receive light/dark pairs. Inline Smart DJ now uses the accepted sharp reconstruction at its original logical size. Loading and Now Playing header images were corrected separately. | More SmartDJ Palettes |
| Marketplace cleanup | Original Marketplace interface and accepted visual resources remain; stock service endpoint code retained. No helper redirections, forced commerce flags, helper application or community-service dependencies included. Does not restore online commerce or sign-in services. | Marketplace Cleanup; source release-policy pass; stock ZuneService hash |
| Maintenance | One graphical EXE with compatibility/integrity checks, administrator worker, verified originals, staged reconstruction, journaled recovery, safe repeat installation and repair. | New RC2 installer source and isolated tests |

## Deliberately excluded

- Abandoned Collection animation experiments, rejected two-column Display layout, neutral-gray placeholder trial and canceled device enhancement candidates.
- Recorders, debuggers, instrumentation, injected observer DLLs, developer-only compatibility experiments, `.bak` / `TEST*` files, old installation snapshots and historical installer packages.
- Marketplace/webservice helper components, host redirects, forced service flags and unverified downloads.
- Music-file artwork embedding: investigated only; CHANGE IMAGE retains its existing Zune behavior.
- Startup optimization: discussed, then explicitly left unchanged.
- User-profile tile selection changes, unused transport-control experiments, and any inventory-only preview that was never installed.
- Developer library, database, custom imported pictures and personal registry preferences.

The existing final dark visual assets are retained alongside their already-approved palette variants. This release does not independently redesign or add a second theme package.

## Changes included since the first laptop build

- New accounts default to automatic Windows scaling, the original white interface and Basic album-art decoding. Saved valid choices are retained. Optional display reset uses these defaults.
- Dark theme uses the existing original/negative background and control assets, with the corrected description. Added approved pairs cover remaining shell/navigation/device-outline/playback/transport assets and the selected Other PNG artwork. Filled device images remain neutral; SONGSFORYOU.PNG uses the Microsoft original.
- Enhanced small Smart DJ link icons in both themes; corrected loading-logo size; connected the Collection loading logo to its theme pair; enhanced the actual Now Playing Smart DJ header at its original logical size.
- Fixed the cropped/oversized footer heart when starting on Quickplay; enhanced Quickplay footer hearts without enlarging their controls.
- Added the three requested sync pairs, preserving the existing enhanced dark versions and supplying the enhanced Microsoft original or exact negatives for the white interface.
- Custom background changes queue until settings are saved. One combined Restart / Later notice covers changes saved together. Empty custom slots preview the existing white Zero background.
- Enhanced placeholder defaults were promoted to the original resource names as applied on the reference PC. The broader asset-default migration was stopped at the user's request; no further speculative substitutions are included.

The brief blurry placeholder flash during rapid Next in Now Playing remains unresolved. Native model/resource checks do not establish live appearance on every screen or device.

## Current additions included in RC5

Local artist-photo folders can be managed from Display settings. Artist folders are created for library artists, photos live directly in each folder, and Windows logical filename ordering chooses the primary image used by Quickplay. Entering Now Playing refreshes the current artist; final Quickplay/photo-loop repairs are included. Device artist-photo transfer is not supplied.

Mixview uses available local albums and artist photos, randomizes surrounding entries and retains the selected album as a fallback. Its artwork follows quality levels. The existing enhanced info controls are forced visible, labeled Go to, and navigate to collection items or the corresponding artist. Repeat cycles through off, repeat list and repeat current item, with matching visuals/tooltips. Genuine sign-in conditions remain in control of profile access.

Prerequisites checks missing .NET 3.5 and media components, reports available player feature state, offers Microsoft guides and Recheck, and handles the build 28000+ standalone .NET guidance and Windows N restart instructions. Original Zune setup is not started while required checks fail. No network restriction or retired-server patch is included.

## RC4 installer experience

Large colored/text status banners, a Finish button, and optional normal-user launch after successful install/repair. Explicit Windows-managed installation of supported missing prerequisites, without bundling Windows media packages, changing update policies or automatically restarting. A separate official .NET installer is still needed on Windows 11 build 28000+.

## RC5 Now Playing Mixview

Enhanced native button artwork, unchanged logical size, current-track availability refreshed on track changes, and current local album seeding of the existing native overlay. No replacement button or online catalog requirement for local albums.

RC6: Download Zune and Install downloaded Zune provide verified automatic ZIP/EXE extraction. Includes confirmed local Mixview visibility, icon scaling and shared callback repairs.


## RC7 playback

Optional crossfading is off by default, with an on/off control and a 0–10 second slider in Settings → Software → General. It applies to supported local audio, follows pause, seek, mute and volume, and falls back to native playback on audio-host failure. Repeat one replays the current item without overlap and permits explicit selection of another item. The required audio runtime is bundled.
