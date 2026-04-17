# CTX - Release 1.0.7

Release date: 2026-04-17

Version:

- `1.0.7`

Summary:

- Stable patch hotfix for CTX 1.0.
- Restores end-to-end consistency between the published release metadata and the product version reported by the installed binary.

Highlights:

- `ctx version`, viewer version surfaces, and runtime product-version checks now report `1.0.7` instead of the stale `1.0.4` constant that slipped into `1.0.6`.
- The public `win-x64` portable asset was rebuilt from the release branch with the corrected product version constant.
- The public README, changelog, live-demo landing copy, and release references now point to the corrected hotfix baseline.
