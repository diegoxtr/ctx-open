# Changelog

All notable changes to CTX will be documented in this file.

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
