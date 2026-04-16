# CTX - Release 1.0.5

Release date: 2026-04-16

Version:

- `1.0.5`

Summary:

- Stable patch release for CTX 1.0.
- Freezes the first public release-aware installer/bootstrap line.

Highlights:

- CTX now ships a single-entry bootstrap for Windows (`install.ps1`) and Linux/macOS (`install.sh`) with install, update, and repair detection.
- The installer now resolves the latest published version and matching portable asset from GitHub Releases instead of relying on a checked-in version string as the public update source of truth.
- The CLI helper now loads a dynamic project-context prompt from `prompts/CTX_HELPER_PROMPT.md` so operators and agents re-anchor on the active repo or install root before planning.
- Distribution packaging now copies the helper prompt and canonical CTX docs into the install bundle so the installed environment carries its own operating guidance.
- Public docs now describe the release branch strategy and the public-safe installer flow without leaking private workspace paths or temporary live-demo URLs.
