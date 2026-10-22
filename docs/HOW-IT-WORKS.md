# How the enhancements work

This explains the shipped design at different evidence levels. Source links point to editable implementation where present; older components are explicitly marked as frozen. It is not a claim that every fix has been reconstructed here.

## Repeat one: choosing what plays next

Zune originally exposed a repeating boolean. `Enhancements/RepeatOne/RepeatFeature.cs` adds three modes and updates descriptions and numbered repeat artwork. `ChooseNext` preserves an explicit new selection; otherwise repeat one chooses the current playlist item. `ShouldReplay` accounts for end-of-media. The scoped IL patch is in `Enhancements/RepeatOnePatch/Program.cs`.

The repair matters because a prepared track may be a user's explicit selection, not merely an automatic next track. Clearing that state could prevent track changes or leave displayed metadata inconsistent with audio. The current implementation preserves the normal transition path instead of blocking all prepared tracks. Entering repeat one cancels a queued automatic successor.

## Crossfading: overlapping local audio

`Enhancements/CrossfadeHost/Engine.cs` decodes supported local files and mixes the end of one track with the start of the next. `PlaybackOutput.cs` controls the actual output device. `Enhancements/Bridge/Bridge.cs` synchronizes native Zune playlist state with a separate local audio process through redirected input/output messages; it also checks process health and restores native audio on failure.

The feature starts off. The General settings model and markup expose an on/off choice and 0–10 second duration. The bridge follows explicit selections, seek, volume and mute. Repeat one avoids overlapping another track. Unsupported media remains on native playback. This follows WMP-style controls, not a verified reproduction of Microsoft's proprietary fade curve.

The pause repair pauses the output queue as well as mixer progression. Merely stopping sample progression while leaving a queued output device playing was insufficient. The simulated-device checks exercise paused seeking and overlap pause/resume without playing sound.

## Installation and recovery

`Setup.cs` verifies expected original/patched file hashes before changes, reconstructs into staging files, saves original files and a recovery journal, and records progress so failed/interrupted transactions can be recovered. Unknown modifications are rejected rather than guessed away. Custom-background-aware handling is in `BackgroundResources.cs` and `Slots.cs`.

`MediaInstall.cs` checks supported original download/extraction inputs and launches original Microsoft setup in local mode. That avoids the obsolete online package lookup encountered in testing while retaining Microsoft's setup flow. Original media is separate from the enhancement installer.

`Prerequisites.cs` and `WindowsComponents.*` detect or guide missing Windows media/.NET components. Windows servicing is an explicit user action; setup does not automatically reboot. The recorded checks include mocks and decision tests, not proof of every Windows-version servicing scenario.

## Responsiveness repair — historical frozen component

The retained feature inventory describes a renderer fairness repair: return to the Windows message loop after a complete render pass, allowing pending input to be processed rather than extending the rendering workload indefinitely. The shipping change is retained in the native renderer/scaling patch. This snapshot does not include a complete editable native build of that repair or a new benchmark.

Do not confuse that repair with retired-server blocking. The user reported no perceived benefit in the temporary networking comparison. No firewall/hosts-file change or retired-server patch is shipped.

## Visuals, artist photos and Mixview — historical frozen components

Earlier changes combine UIX/resource edits and managed/native helpers: scaling choices, palette/control assets, decode-quality choices, custom background slots, local artist-photo lookup, and local Mixview navigation. Artist images live directly under artist folders; Windows logical filename ordering chooses the primary image. Refresh and loop repairs address desktop views; device photo synchronization is not supplied.

Mixview uses local album/artist information and randomized surrounding entries, with a selected-album fallback. Controls navigate into the collection instead of relying on discontinued catalog pages. The payload carries these fixes, but this repository does not currently offer their complete editable construction pipeline. The retained [historical feature inventory](HISTORICAL-FEATURE-INVENTORY.md) gives broader provenance; it contains old release-stage wording and is not a fresh certification.

## Background and restart safeguards

The custom-background design validates image input, limits sizes, queues changes until settings are saved, and combines relevant restart notices. Installer code recognizes the custom resource slots rather than treating permitted personal background edits as arbitrary modifications. Restore exports customized slots. The runtime picker/helper side is historical frozen material; the installer side is visible in source.

The broad objective was to preserve music, library data and personal files while changing a bounded program-file set. Safeguards and tests reduce risk; they are not a guarantee against every machine-specific failure. See [verification](VERIFICATION.md).
