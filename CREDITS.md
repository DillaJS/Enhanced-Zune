# Credits and attribution

## Microsoft and its original contributors

Microsoft and its licensors created the original Zune desktop application, Iris/UIX framework, branding, fonts, original interface and assets. These are not creations of Enhanced Zune and are not made open source by this project.

## Pre-existing third-party Dark Zune modification — creator unknown

**Enhanced Zune development began with an already modified, third-party dark-themed `ZuneShellResources.dll`, not Microsoft's untouched resource DLL. The project owner did not create that original dark modification and claims no credit for it.**

We acknowledge and thank the unknown creator(s) of those pre-existing dark adaptations. The original download location, creator identity and applicable redistribution license have not been established. If you can identify the exact original package, please provide a primary source or file evidence; a visually similar theme is not enough to establish authorship.

The saved inherited DLL is identified by SHA-256 `BCE5801AAAF06666BE238E5F85068BBC2E45ADBE1C27B211727F2BD1E42CF691`. The Microsoft-stock comparison DLL is `073E43F83B2E5AE9FC4C16516F681A374D757FF3574E51CFEEE1D9205783B854`.

The inherited DLL already contained 275 PNG resources with visible color/alpha/geometry changes relative to stock, including all 11 built-in desktop backgrounds. Many additional PNG byte changes were re-encoding only. Later resizing, recoloring, pairing or enhancement of inherited content does not erase its provenance or establish exclusive ownership.

## Enhanced Zune project's subsequent work

The project's contribution is the subsequent integration and fixes: renderer responsiveness, scaling, artwork-quality controls, theme-selection and text/control behavior, custom-background management, local artist-photo behavior, Mixview/navigation changes, playback controls, repeat-one repair, crossfading, and installer/recovery work, to the extent supported by the preserved source and project records.

This describes later work on top of Microsoft Zune and the inherited dark baseline. It does not claim that the project created the original dark artwork, all underlying graphics, or every resource in its payload. See [source coverage](docs/SOURCE-COVERAGE.md) and the [resource provenance audit](docs/ATTRIBUTION-AUDIT.md).

## Dependencies and tools

The optional crossfade helper uses NAudio 2.2.1 and the Microsoft .NET runtime. Their upstream notices remain in `ZuneCrossfade-Licenses.txt`. Those licenses do not license Microsoft Zune, inherited dark artwork, or unrelated project content. AI-assisted development does not change these attribution boundaries.

## Permission status

Acknowledgment is not a permission grant. Redistribution of the inherited dark assets is **not cleared**. No blanket open-source license is assigned to this mixed-provenance package. See [licensing and distribution plan](docs/REDISTRIBUTION.md).
