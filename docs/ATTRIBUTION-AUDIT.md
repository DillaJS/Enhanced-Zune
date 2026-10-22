> Historical attribution audit. Its factual findings remain unchanged. For the current project license and publication workflow, see [LICENSE-SCOPE.md](../LICENSE-SCOPE.md) and [REDISTRIBUTION.md](REDISTRIBUTION.md). Earlier hold recommendations below do not assign new rights or override the owner's later preparation instructions.

# Attribution audit â€” 2026-10-22

## Scope and evidence

Read-only comparison of retained Microsoft-stock and pre-existing third-party dark reference DLLs, the saved 15 October Light-versus-Dark comparison, later project correction records, and manifest-verified RC6.2/RC7 resource outputs. Installed Zune was not opened or changed. No image or DLL is exported into the provenance documentation.

| Reference | SHA-256 | Evidence |
| --- | --- | --- |
| Microsoft stock | `073E43F83B2E5AE9FC4C16516F681A374D757FF3574E51CFEEE1D9205783B854` | Saved comparison recorded a valid Microsoft Authenticode signature and equality to installer-extracted stock; bytes rechecked in this audit. |
| Inherited third-party dark baseline | `BCE5801AAAF06666BE238E5F85068BBC2E45ADBE1C27B211727F2BD1E42CF691` | Retained experiment baseline matches this hash; earlier rollback scripts and manifests corroborate continuity. Creator and license remain unknown. |

## What predates Enhanced Zune

Both references contain 1,856 resources. There are 1,313 differing payloads: 1,289 PNGs, 23 icon-group metadata entries and one version metadata entry. The saved pixel comparison breaks the PNG changes into 995 re-encodings with identical decoded pixels, 19 transparent-RGB-only changes, and **275 visibly changed PNGs**.

The 275 include 132 visible RGB inversions with preserved alpha, 12 inversions with alpha changes, 129 other RGB edits, one mixed unchanged/inverted image, and one dimension change. All 11 built-in backgrounds change visibly. Only two backgrounds are exact whole-canvas inversions; nine have additional RGB differences. Describing all inherited backgrounds as our newly created negative images is inaccurate.

All 241 UIX resources and the single UIB table are identical between those references. All 156 icon-image payloads are identical. The earlier dark baseline therefore does not establish an earlier implementation of our later UIX text/style/model logic. Conversely, unchanged Microsoft markup remains Microsoft's work, not ours. Microsoft version strings survive in the modified DLL and do not identify its editor.

## What remains in the actual release inputs

The resource DLLs inspected for each version exactly match their shipping payload-manifest hashes. The following counts concern the 275 historically visible dark-reference changes, allowing renamed resources and aliases:

| Retention of historical dark content | RC6.2 | RC7 |
| --- | ---: | ---: |
| Historical reference entries whose exact dark PNG bytes occur in the release | 172 | 172 |
| Additional reference entries with matching visible RGBA/geometry but different encoding/transparent RGB | 2 | 2 |
| No exact or visible match established | 101 | 101 |

These count historical reference entries, not unique artwork copyrights or distinct shipped resources. Duplicate source images and aliases can share one match. The 101 unmatched entries are **not** classified as independently original project artwork; they may have been replaced or transformed. Derivative lineage cannot be disproved by a changed hash.

The project record `Zune-Dark-Theme-Reference-Correction` deliberately replaced a rejected generation step with byte-exact Microsoft/inherited source pairs. All **58 resources in its 29 pairs** remain byte-exact in both releases: 29 Microsoft-reference assets and 29 inherited-dark assets. This includes all 11 inherited dark backgrounds. The technical integration/pair selection is subsequent project work; the inherited source images are not.

The release PNG inventory has 901 entries whose bytes match only the dark reference, 53 matching only stock, and 821 with no exact match in either reference. **901 does not mean 901 distinct third-party artistic edits:** it includes re-encoded stock art. Likewise, 821 does not mean 821 independently created assets.

## Resource-level evidence

- [Original comparison inventory](provenance/historical-light-vs-inherited-dark.csv): every changed historical resource, hashes, group and pixel classification.
- [RC6.2 inherited visible entries](provenance/RC6-2-inherited-visible-resources.csv) and [RC7 entries](provenance/RC7-inherited-visible-resources.csv): all 275 entries with matching release resource names.
- [Release PNG byte provenance](provenance/release-png-byte-provenance.csv): per-resource reference matches for both release versions.
- [Machine-readable audit](provenance/attribution-results.json): counts, release hashes and all 58 source-pair preservation checks.

These CSV/JSON files contain resource names and measurements, not the images themselves. They establish content continuity, not legal permission or the identity of an unknown author.

## Creator search outcome

No reliable creator, original download URL or redistribution license was found. Local rollback/manifests prove the same modified DLL was used; they do not identify who edited it. Retained baseline/desktop backup files have no download-origin alternate stream. No source credit was recovered from their retained companion notes. The deleted Downloads copies are not needed for this comparison because a matching baseline survived in the workspace.

Web searches covered the exact DLL hash and combinations of `ZuneShellResources.dll`, Dark Zune, dark mode/theme, GitHub and download terms. Results about other Zune mods, registry changes, Windows XP themes, phone skins and independent players are not evidence that their creators made this DLL. No message was sent to any creator or community member. Unknown identity is stated explicitly rather than guessed.

## Rights conclusion

No redistribution authorization for the inherited dark modification was established. Keep its DLL, images, derivative artwork and payloads that embed/reconstruct them on hold. RC6.2 being already shared does not resolve that gap. Read [REDISTRIBUTION.md](REDISTRIBUTION.md) for the recommended alternative and source references.
