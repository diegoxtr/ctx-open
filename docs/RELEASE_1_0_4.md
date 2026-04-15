# CTX - Release 1.0.4

Release date: 2026-04-15

Version:

- `1.0.4`

Summary:

- Stable patch release for CTX 1.0.
- Freezes the current public-safe baseline after the live-demo hardening pass, public/private viewer parity sync, and commit-focus lineage auto-selection converged into one coherent release.

Highlights:

- The viewer now auto-selects the primary commit-focus node so historical commit views immediately explain themselves through lineage highlight.
- Historical graph export is more resilient with abbreviated commit IDs, safer legacy-snapshot handling, and controlled JSON failures instead of raw server errors.
- The GitHub Codespaces live-demo flow is now documented and hardened with explicit entrypoints, copy/paste demo repository paths, SDK bootstrap recovery, and clearer public/private alignment.
