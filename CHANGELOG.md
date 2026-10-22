> Both versions build on a pre-existing third-party dark resource modification, creator unknown. Our own subsequent contributions use 0BSD; inherited material retains separate rights. See [CREDITS.md](CREDITS.md) and [LICENSE-SCOPE.md](LICENSE-SCOPE.md).

# Changelog

## 1.0.0 RC7 — local update candidate, not yet publicly released

- Repair repeat one so clicking a different item works and playlist audio remains consistent with the displayed track.
- Add optional crossfading under Settings → Software → General, off by default, with 0–10 seconds of overlap.
- Include the tested pause/resume and paused-seeking corrections. Repeat one plays without overlap.
- Bundle the audio component and its runtime; no separate .NET 8 installation is required for crossfading.
- Retain original-Zune installation, prerequisite, backup, rollback and custom-background protections. Add verified RC6 recovery support.
- Include the new enhancement source and tests alongside reproducible installer engineering source.

## 1.0.0 RC6.2 — first publicly shared installer

- First version uploaded/shared with users, according to the project owner.
- Still the version on the website as reported by the owner on 2026-10-22. No independent website check is claimed here.
- Known repeat-one defect: explicit selection of another track can be blocked; some playlist transitions can advance audio while retaining the previous item's displayed information.
- The repeat-one repair and optional crossfading belong to the later RC7 candidate, not this public baseline.
- Earlier internal RC numbers do not imply earlier public releases. The original RC6.2 publication date is not asserted.
