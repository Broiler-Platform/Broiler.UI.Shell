# Broiler.UI.Shell Human Review

**Status:** PENDING

Broiler.UI.Shell contains platform-neutral shell dialog contracts, print/scan
host ports, Standard shell dialog implementations, tests, and samples. No
attributable human approval of a current revision is recorded here. This file
must remain `PENDING` until a human reviewer selects a decision and signs a
review for an exact commit.

## Required review scope

- Core dependency boundaries (`Broiler.UI` / `Broiler.Graphics` only, no native UI handles).
- File-system enumeration, path validation, and cancellation in `Broiler.UI.FileDialog`.
- Print/scan job settings validation, capability clamping, and host cancellation in
  `Broiler.UI.PrintDialog` and `Broiler.UI.ScanDialog`.
- Keyboard/pointer navigation, focus/modality, theme/contrast, and DPI behavior across
  `AboutDialog`, `ColorDialog`, `FileDialog`, `FontDialog`, `PrintDialog`, and `ScanDialog`.
- Package metadata, public API/XML documentation, dependencies, licenses, and static analysis.

## Human decision

The reviewer must identify the commit, commands/evidence, limitations, decision,
name, and date. AI tools may assemble evidence, but must not choose the decision,
sign the attestation, or change `PENDING` to an approval.
