# Changelog

All notable changes to CTX will be documented in this file.

## [1.0.5] - 2026-04-16

### Added

- Single-entry installer bootstraps: `install.ps1` for Windows and `install.sh` for Linux/macOS.
- Cross-platform install engines in `scripts/install-ctx.ps1` and `scripts/install-ctx.sh`.
- Dynamic helper prompt loading from `prompts/CTX_HELPER_PROMPT.md`.
- Distribution manifests for install layout and GitHub Releases asset resolution.

### Changed

- The CLI `helper`/`--help` flow now prints project-specific bootstrap guidance by resolving the active repo or install root.
- Distribution packaging now copies the helper prompt and canonical CTX docs into install bundles.
- Installer documentation now follows a release-aware flow: `main` for integration, `release/x.y.z` for stabilization, and GitHub Releases as the public install/update source of truth.

### Notes

- This patch release freezes the first release-aware installer/bootstrap line for CTX 1.0, including dynamic helper guidance, public-safe distribution manifests, and GitHub Releases-based update detection.

## [1.0.4] - 2026-04-15

### Added

- Public live-demo docs now include explicit landing, notes, quickstart, and current session entrypoints plus copy/paste demo repository paths.
- The public release line now documents an explicit Codespaces recovery block for resumed sessions that do not auto-start the viewer.

### Changed

- The viewer now auto-selects the primary commit-focus node so opening a historical commit immediately highlights the cognitive lineage without an extra graph click.
- The public viewer surface is aligned again with the private workspace UI, including state badges and commit-focus behavior parity.
- The Codespaces bootstrap no longer depends on Python to resolve the pinned SDK and is easier to recover in live-demo sessions.

### Fixed

- Historical graph export is more resilient with abbreviated commit IDs, safer legacy-snapshot handling, and controlled JSON failures instead of raw server errors.
- The GitHub Codespaces live-demo flow is hardened against stale sessions, SDK mismatches, and missing recovery instructions.

### Notes

- This patch release freezes the current public-safe baseline after the live-demo hardening pass, viewer parity sync, and commit-focus lineage highlight behavior converged into one releaseable state.

## [1.0.3] - 2026-04-14

### Added

- GitHub-native live demo delivery scaffolding with Codespaces startup flow, tri-language live-demo docs, and a Pages landing surface.
- `ctx preflight --operation ...` as a stronger runbook-aware preflight for critical operations.

### Changed

- The working graph now focuses on immediate tactical lineage instead of mixing sibling tactical lines by default.
- Graph inspection now supports full lineage highlighting from the selected node, with better node badges, tighter spacing, and improved long-edge lane routing.
- `ctx-open` is now aligned with the current public-safe baseline and carries the live-demo/public landing surface.

### Fixed

- Historical tasks without goals are normalized under a legacy parent goal in the current working state, avoiding new orphan tasks.
- Graph columns now stack tall cards by measured height and avoid the worst node overlap cases from the earlier fixed-gap layout.
- The GitHub Codespaces live-demo scaffold now starts the viewer on workspace start and includes SSH support for remote operations instead of depending on a manual attach step.

### Notes

- This patch release freezes the current stable line after the viewer graph readability pass, runbook preflight hardening, and the first GitHub-native demo/public delivery flow converged into one releaseable state.

## [1.0.2] - 2026-04-14

### Added

- `OperationalRunbook` as a compact operational memory layer, versioned through `RepositorySnapshot` and surfaced in packets, `ctx check`, and the viewer.
- `CognitiveTrigger` as the persistent origin model for cognitive lines, with compact packet integration and a dedicated `Origin` surface in the viewer.
- Canonical guide parity across English, Spanish, and Chinese for the `docs/` surface.

### Changed

- The viewer right rail now uses isolated tabs for `Details`, `Origin`, `Playbook`, and `Hypotheses`.
- Viewer collapse behavior now uses side tab rails instead of narrow collapsed panels.
- Trigger creation now distinguishes between direct new work lines and inherited continuation.

### Fixed

- Viewer `Origin` rendering no longer fails on missing date-format helpers.
- Published viewer now handles `favicon.ico` requests explicitly and avoids misleading console noise.
- Published viewer root and right-rail behavior are aligned with the current local installation flow.

### Notes

- This patch release freezes the current stable CTX line after the viewer UX, runbook/origin model, and tri-language documentation surfaces converged into a coherent baseline.

## [1.0.1] - 2026-04-10

### Fixed

- Viewer commit selection now keeps the graph focused on the commit closure thread instead of expanding into the full modified cognitive snapshot.
- Portable release binaries are rebuilt from the viewer commit-focus fixes.

### Notes

- This patch release refreshes the stable 1.0 line with the latest viewer history/graph closeout behavior.

## [1.0.0] - 2026-04-10

### Added

- Stable release baseline for CTX 1.0

### Notes

- This release marks CTX as a stable baseline suitable for production evaluation.

## [0.1.1-internal] - 2026-04-08

### Added

- Minimal web viewer for CTX history and graph inspection
- Timeline lanes by branch for the bundled viewer
- Structured commit detail panel with diff sections and parent navigation
- Bundled sample repository at `examples/viewer-demo` for viewer validation
- Support for branch names containing `/` in filesystem persistence

### Notes

- This release freezes the first usable local viewer workflow for CTX.
- The product can now demonstrate repository history, graph lineage and a persistent sample project end-to-end.

## [0.1.0-internal] - 2026-04-07

### Added

- Clean Architecture .NET 8 solution for CTX
- Typed cognitive domain model for goals, tasks, hypotheses, decisions, evidence, conclusions, runs, packets, commits, and working context
- Filesystem-backed `.ctx/` repository structure
- CLI commands for repository initialization, artifact creation, status, commit, log, diff, branching, checkout, merge, context building, and operational inspection
- OpenAI and Anthropic provider abstraction with deterministic offline fallback
- Cognitive diffing and conflict-aware merge engine
- Automated test suite for core workflows
- Proprietary licensing, contribution assignment, and product planning documentation
- Pilot testing and installation guides in Spanish
- `ctx version` command for internal release identification

### Notes

- This is an internal baseline release intended for controlled technical validation.
- The product is functional and testable, but not yet declared V1.
