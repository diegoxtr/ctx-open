# CTX - Release 1.0.9

Release date:

- `2026-04-22`

Version:

- `1.0.9`

## Summary

This hotfix makes the public install flow match the intended user experience.

Normal users should install CTX from published portable release assets, not by compiling the repository from source.

## Changed

- The public install bootstrap now prefers published portable assets by default.
- The README and installation guide now state explicitly that the normal install flow should not require the .NET 8 SDK.
- Public onboarding copy, screenshots, example notes, live-demo surfaces, and Chinese documentation were aligned with the current state-driven operator model.

## Fixed

- `install.sh` and `install.ps1` no longer default ordinary users into a source-build path when `MODE=auto`.

## Notes

- Source mode is still available for developers and unreleased validation.
- Public installation should now be understood as:
  - download published release bundle
  - install
  - expose `ctx`
  - start operating
