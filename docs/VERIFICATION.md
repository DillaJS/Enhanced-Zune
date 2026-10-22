# Verification and limits

The preserved `RC7-RECORDED-VERIFICATION.json` is historical evidence from packaging, not a new execution of every check during repository preparation. It records installer reconstruction/recovery scenarios and separate repeat, bridge and audio checks, with their stated limits.

The stage-one preparation report sits outside Git alongside this repository. It records a fresh installer compilation from this checkout, comparison against the delivered RC7 checksum, directly runnable audio tests, and a tracked-file/privacy audit. It does not claim a new install, listening test, device check or Windows prerequisite installation.

## What reproducibility establishes

A byte-identical installer rebuild shows that the specified compiler inputs and frozen payload reproduce the delivered EXE. It does not establish that every payload component has visible editable source, that every original artifact has cleared redistribution rights, or that the resulting application is safe on every machine.

## Important unresolved limits

- Full Windows feature servicing and clean-machine behavior require appropriate disposable-system tests.
- The reference PC historically had strong-name verification overrides. Setup does not install those overrides; reference-PC success is not clean-runtime certification.
- Manual playback/visual feedback is distinct from automated fixture checks.
- Historical runtime changes are not all source-rebuildable from this repository.
- This is a release candidate, not a Microsoft-supported product.

Read [KNOWN-LIMITATIONS.md](../KNOWN-LIMITATIONS.md) for the preserved detailed release notes. No check during stage one modifies installed Zune.
