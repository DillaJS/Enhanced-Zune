> **Attribution correction (2026-10-22):** Development used a pre-existing third-party dark `ZuneShellResources.dll`. Its creator/source/license are unknown. The project did not create that original dark modification. Microsoft originals, inherited dark assets, and subsequent project changes are distinct layers. Current mixed-provenance release artifacts are **not cleared for redistribution**; see `CREDITS.md` and `docs/REDISTRIBUTION.md` in the staged repository. Earlier wording such as "our dark assets" describes retained/integrated content, not exclusive authorship.

# What source is available?

This table describes the preserved RC7 engineering package. It distinguishes implementation source from frozen shipping inputs. It does not claim that all surviving development folders have already been recovered or audited.

| Area | Available here | Important gap |
| --- | --- | --- |
| Installer and recovery | `Setup.cs`, `SetupUI.cs`, `BackgroundResources.cs`, `Slots.cs`, `RuntimeTrust.cs` | Some full transaction tests require private original-file and prior-release fixtures. |
| Original-media handling and prerequisites | `MediaInstall.cs`, `Prerequisites.cs`, `WindowsComponents.cs`, `WindowsComponents.ps1` | Original Microsoft setup/media and Windows feature payloads are not included. |
| Crossfade audio engine | `Enhancements/CrossfadeHost` including pinned host dependencies | The host alone is not a complete Zune integration build. |
| Crossfade bridge/settings | `Enhancements/Bridge`, `Enhancements/CrossfadePatch` | Compatible Zune references, pre-patch inputs and signing workflow are required to regenerate shipping signed components. |
| Repeat-one repair | `Enhancements/RepeatOne`, `Enhancements/RepeatOnePatch` | Original reference/resource inputs and signing workflow are required. |
| Playback tests | `Enhancements/Tests` | Only the audio/output project is directly runnable as documented; other fixtures need their original harness. |
| Earlier renderer, scaling, themes, artwork, photos and Mixview changes | Frozen patch payload, component hashes, feature/provenance notes | No complete standalone editable build of these historical changes is provided by this snapshot. Further surviving source may be recovered in future work. |
| Microsoft Zune | User-supplied originals used by the installer | Microsoft source and full Microsoft DLLs are not included or relicensed. |

## Why the payload exists

The installer embeds a ZIP of compressed added components and binary deltas, with hashes and compatibility metadata. Compiling this installer from source reproduces its packaging around these fixed bytes. It does not prove that every embedded helper was regenerated from the visible source.

The payload contains executable material and derivative resource changes, not only plain-text patch instructions. Separating it as a release asset is repository organization, not a redistribution-rights determination. See [licensing status](../LICENSE.md).

## How history will be represented

The first commit imports the preserved RC7 source. Later commits describe repository preparation and future changes. Earlier release notes can be added when supported by preserved packages; invented earlier commits or guessed dates will not be used.

## Source availability is separate from ownership

The Microsoft resource base, the inherited third-party dark edits and our subsequent code/asset work have different provenance. A source or patch file being visible here is not a license grant over its inputs or embedded content. Compiled `ManagementGeneral.uib` and derivative `ManagementGeneral.uix`, frozen payloads and historical patch material require rights review before any code-only publication. See [CREDITS.md](../CREDITS.md) and [the audit](ATTRIBUTION-AUDIT.md).
