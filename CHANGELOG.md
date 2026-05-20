# Changelog

All notable changes to CTX will be documented in this file.

## [Unreleased]

## [1.0.21] - 2026-05-20

### Added

- `ctx runbook update <runbookId>` for maintaining operational runbooks without hand-editing `.ctx` files.
- `ctx_runbook_update` MCP write tool for agent-facing runbook maintenance.
- Viewer chrome controls for topbar collapse, project-tab collapse, graph intro minimization, Compare Graph auto-fit, and expanded canvas mode.

### Changed

- Viewer desktop layout now uses measured fixed chrome surfaces so the graph workspace, side rails, footer, parked epic rail, and summary strip remain reachable while resizing.
- Viewer graph canvas can resize freely on desktop and keeps parked epic state aligned to the graph window.
- MCP parity docs now classify `runbook add/update` as implemented and keep attach/detach plus prompt-list follow-ups explicit.

### Validation

- Release branch validation must confirm `ctx version` reports `1.0.21`, Viewer chrome/graph contract coverage passes, `ctx_runbook_update` appears in MCP tool coverage, public/private consistency guard passes, and no local `.ctx` state or machine-specific paths are staged.

## [1.0.20] - 2026-05-18

### Added

- Viewer commit details can now start `Compare with parent` directly.
- Viewer `/api/diff` now returns snapshot-context overlay data for target commits.
- Compare Graph now includes `Snapshot Context` nodes around changed entities.

### Changed

- Viewer mobile layouts now stack controls and panels cleanly on phone-width screens.
- Mobile Viewer panel order is now Trace Graph, Details, then History.

### Validation

- Release branch validation must confirm `ctx version` reports `1.0.20`, Viewer contract coverage verifies parent comparison and snapshot context, tests pass, GitHub Pages assets resolve, and no local `.ctx` state or machine-specific paths are staged.

## [1.0.19] - 2026-05-13

### Fixed

- Viewer `Parked Epics` rail now filters to actual `Parked` epics instead of listing completed or active epics.

### Validation

- Release branch validation must confirm `ctx version` reports `1.0.19`, Viewer contract coverage verifies the parked-epic filter, tests pass, GitHub Pages assets resolve, and no local `.ctx` state or machine-specific paths are staged.

## [1.0.18] - 2026-05-13

### Changed

- Viewer live sync now fingerprints the full working cognitive state, including project metadata, dirty/head state, goals, epics, tasks, hypotheses, decisions, evidence, and conclusions.
- Working-context refresh detection now updates when non-task cognitive entities change, so Viewer reloads stay aligned with the actual `.ctx` state.

### Validation

- Release branch validation must confirm `ctx version` reports `1.0.18`, Viewer `/api/working-context/signal` returns a stable fingerprint over the enriched cognitive state, tests pass, GitHub Pages assets resolve, and no local `.ctx` state or machine-specific paths are staged.

## [1.0.17] - 2026-05-13

### Fixed

- Viewer Compare Graph now uses the preserved commit diff when comparing a cognitive commit directly with its parent, avoiding an unnecessary reconstructed diff path.
- Viewer Compare Graph contract coverage now verifies the repository guard, parent-direct preserved diff path, and fallback diff path.

### Validation

- Release branch validation must confirm `ctx version` reports `1.0.17`, parent/child Viewer Compare Graph returns preserved diff JSON from `/api/diff`, tests pass, GitHub Pages assets resolve, and no local `.ctx` state or machine-specific paths are staged.

## [1.0.16] - 2026-05-13

### Fixed

- Viewer Compare Graph now has the matching `/api/diff` backend endpoint, so History row comparisons no longer fail with `Comparison failed: 404`.
- The Viewer diff endpoint resolves abbreviated commit ids before delegating to the cognitive diff service.

### Validation

- Release branch validation must confirm `ctx version` reports `1.0.16`, Viewer Compare Graph returns diff JSON from `/api/diff`, tests pass, GitHub Pages assets resolve, and no local `.ctx` state or machine-specific paths are staged.

## [1.0.15] - 2026-05-12

### Added

- `ctx update` and alias `ctx -update` for a read-only GitHub Release check against the latest public `ctx-open` release.
- Viewer native module entrypoint plus focused API, storage, workspace, history, and utility modules.
- Viewer contract tests for Compare Graph behavior, selectable diff nodes, graph scrolling, zoom, cognitive diff summaries, and module boundaries.
- Public release inventory for the post-1.0.13 release pass.

### Changed

- CLI command coverage now tracks `update`.
- English, Spanish, and Chinese command references document `ctx update`.

### Fixed

- Public GitHub Pages landing now ships `docs.html` and uses deployed screenshot asset paths.

### Validation

- Release branch validation must confirm `ctx version` reports `1.0.15`, `ctx update` is read-only, tests pass, Viewer module assets load, Compare Graph remains inspectable, GitHub Pages screenshot assets resolve, and no local `.ctx` state or machine-specific paths are staged.

## [1.0.14] - 2026-05-12

### Changed

- Distribution packaging now carries the full public `docs/` tree and `ctx --help` points to the installed `docs/TECHNICAL_INDEX.md` documentation index.
- Portable and installed Viewer launchers now declare `CTX_INSTALL_ROOT`, keeping packaged MCP discovery anchored to the active install layout.

### Fixed

- Packaged Viewer MCP detection now honors `CTX_INSTALL_ROOT` and can infer the portable install root from the bundled `viewer/`, `bin/`, and `mcp/` layout.
- Packaged Viewer MCP controls can now find the bundled `ctx-mcp` launcher and `Ctx.Mcp` binary outside `C:\ctx`.

### Validation

- Release branch validation must confirm `ctx version` reports `1.0.14`, tests pass, local Markdown/HTML links resolve, JSON snippets parse, static pages target `v1.0.14`, and no local `.ctx` state or machine-specific paths are staged.

## [1.0.13] - 2026-05-07

### Added

- First-class epics plus `ctx epic add/update/promote/list/show` for future planning that stays out of `ctx next` until promoted.
- `ctx gaps` and `ctx roadmap` for read-only planning gaps, blocked work, deferred ideas, and parked epics.
- MCP parity for epics and updated CLI-to-MCP parity documentation.
- VS Code MCP video guide and public release-version update helper.
- Viewer MCP `Start` / `Stop` controls, release bell, graph zoom, progressive rendering, footer summaries, and parked-epic rail navigation.

### Changed

- `ctx next` now keeps blocked tasks out of executable recommendations while preserving them through gaps and roadmap surfaces.
- `ctx plan` now exposes one authoritative `runbookSuggestions` list, with task-attached runbooks promoted ahead of generic matches.
- `ctx preflight --operation <operation>` now supports arbitrary operation tokens.
- Viewer Browse now opens the local folder picker before scanning and documents the browser path boundary.
- Public MCP setup now includes Gemini CLI and Devin configuration examples.

### Fixed

- Dense Viewer graphs no longer restart rendering endlessly after completion or node selection.
- Parked epic cards now hydrate History pages when needed and scroll to the origin commit.
- Public docs now include the MCP parity, roadmap/gaps, VS Code MCP guide, and live-demo references they link to.
- Live-demo screenshot paths and archived Spanish release links now resolve locally.
- Portable distribution bundles now include the documented launchers before install.

### Validation

- Release branch validation must confirm `ctx version` reports `1.0.13`, tests pass, local Markdown/HTML links resolve, JSON snippets parse, static pages target `v1.0.13`, and no local `.ctx` state or machine-specific paths are staged.

## [1.0.12] - 2026-04-30

### Added

- `ctx prompt list` and alias `ctx prompts` for chronological prompt-like trigger extraction.
- `ctx plan` for a compact planning packet that mirrors the MCP `ctx_plan` startup surface.
- `ctx operational review` for repeated issue detection and runbook improvement guidance.
- Claude-specific MCP setup blocks on the static MCP page, including Claude Code, Claude Desktop, project `.mcp.json`, and a `ctx_plan` smoke test prompt.

### Changed

- Static landing copy now highlights local MCP connection as a first-class path for agents.
- MCP setup guidance now emphasizes read-only first connection, `ctx_plan` validation, and explicit write-mode opt-in.
- `ctx preflight` now includes repeated operational issue guidance when recurrence reaches the configured threshold.
- Example docs now avoid machine-specific checkout paths where relative or placeholder paths are sufficient.

### Fixed

- Stale hardcoded local checkout references in active example documentation were converted to relative paths or neutral placeholders.
- Static MCP release links now target the 1.0.12 release line.

### Validation

- Release branch validation must confirm `ctx version` reports `1.0.12`, tests pass, JSON snippets parse, and no `.ctx` state is staged.

## [1.0.11] - 2026-04-28

### Added

- Public local MCP quickstart page and GitHub Pages entrypoint for configuring `ctx-mcp`.
- README explanation of CTX as structured cognitive memory beyond provider-level model memory.
- README MCP exposure section describing local stdio transport, read-only default mode, guarded write mode, and current boundaries.

### Changed

- Public live-demo and MCP setup pages now point at the current release line.
- Public README now presents the operator start model, MCP exposure, and durable memory model before deeper reference material.

### Fixed

- Public changelog release ordering now keeps `1.0.8` and `1.0.7` as separate release sections.
- Public release links no longer point at stale `v1.0.9`/`v1.0.10` pages after the post-`1.0.10` documentation pass.

### Validation

- Release branch validation must confirm `ctx version` reports `1.0.11`, `Ctx.Mcp` builds, unit tests pass, install scripts parse, and no local `.ctx` state is staged.

## [1.0.10] - 2026-04-27

### Added

- `Ctx.Mcp`, a local stdio MCP server for MCP-capable agents.
- Read-only MCP tools for CTX status, audit, next-step guidance, context packets, graph inspection, thread reconstruction, preflight, check, and closeout.
- Guarded write-mode MCP tools for creating CTX goals, tasks, hypotheses, evidence, decisions, conclusions, and cognitive commits when the server is explicitly started with `--mode write`.
- Cross-client MCP setup documentation for generic MCP clients, Claude/Anthropic Desktop, VS Code/Copilot-style MCP configuration, DeepSeek-backed clients, and Codex.
- Viewer `/api/mcp-status` plus a compact `MCP Server` status capsule for local installs.
- Release handoff documentation for the 1.0.10 release branch.

### Changed

- First-time CTX Viewer sessions now open in Split view by default.
- Viewer commit history now loads in 20-commit pages, with button and near-bottom scroll continuation for older commits.
- Commit-focus graph expansion is now task-centered so broad goal anchors do not pull unrelated sibling tasks or historical conclusions into the selected commit view.
- Viewer primary graph routing now favors the meaningful cognitive route instead of presenting misleading direct support shortcuts as the main path.
- Locally published viewer builds now show `local-version` in the header instead of looking like an official release tag.
- `ctx next` now returns applicable operational playbooks through `runbookSuggestions` plus overflow titles in `additionalRunbooksAvailable`.
- Local publish and distribution scripts now include `ctx-mcp` and expose parseable install paths, including `CTX_INSTALL_ROOT`, `CTX_BIN_PATH`, and `CTX_MCP_PATH`.

### Fixed

- Structured JSON diffing now compares values semantically instead of treating collection reference inequality as a change.
- `ctx audit` now reports tasks without goals, and `ctx task update --goal` can repair orphaned task lines without direct file edits.
- Linux/macOS installation layout now installs the MCP server alongside CLI and viewer assets.

### Validation

- Release branch validation must confirm `ctx version` reports `1.0.10`, `Ctx.Mcp` builds, unit tests pass, install scripts parse, and no local `.ctx` state is staged.

## [1.0.9] - 2026-04-22

### Changed

- The public install bootstrap now prefers published portable release assets by default instead of falling back to source builds.
- The installation guide and README now state clearly that normal users should not need the .NET 8 SDK for the standard published install flow.
- Public onboarding, live-demo copy, screenshots, examples notes, and Chinese documentation were aligned to the current state-driven operator model.

### Fixed

- The Linux/macOS `install.sh` and Windows `install.ps1` auto mode no longer steer ordinary users into an unintended source-build path.

### Notes

- This hotfix exists to restore the intended public installation experience: download a published binary bundle, expose `ctx`, and start using CTX without compiling the project from source.

## [1.0.8] - 2026-04-21

### Changed

- The public `ctx` bare entrypoint now behaves as a state-driven startup helper instead of a flat command dump.
- Public onboarding documentation now distinguishes between an existing CTX repository and a new cognitive project.

### Added

- A public state-machine-oriented operator loop across the helper prompt, README, CLI command guide, installation guide, and autonomous operation protocol.
- A public `State-driven CTX startup` runbook example documenting the next-command loop for agents.

### Notes

- This patch release freezes the first public line where CTX startup guidance is consistently state-driven across CLI, docs, and helper surfaces.

## [1.0.7] - 2026-04-17

### Fixed

- The published public binary now reports the correct product version instead of retaining the stale `1.0.4` constant in `ctx version` and other version surfaces.

### Notes

- This hotfix exists to restore end-to-end installer and version-reporting consistency after `1.0.6` shipped with the correct release metadata but an outdated hardcoded product version inside the binary.

## [1.0.6] - 2026-04-17

### Added

- `ctx bootstrap map` and `ctx bootstrap apply` as public bootstrap surfaces for provisional cognitive extraction from articles and projects.
- Public documentation for bootstrap cognitive indexing, bootstrap testing development, and branch-like hypothesis semantics.
- Public bootstrap agriculture example packs for `v1`, `v2`, `v3`, and `v4`, including source texts, plans, and real-testing notes.

### Changed

- The domain model, CLI, and viewer now support branch-like hypothesis semantics through hypothesis lifecycle state, role, lineage grouping, inter-hypothesis relations, and evidence sharing.
- The viewer now exposes an `Interpretations` detail surface plus an optional `Show interpretation relations` overlay so competing hypotheses can stay visible without overwhelming the main trace graph.
- The public helper prompt and examples now explicitly preserve the publication boundary while keeping all published examples sanitized and replayable.

### Notes

- This patch release freezes the first public line where bootstrap indexing, coexistence-first hypothesis handling, and interpretation-aware viewer surfaces ship together as a coherent public-safe baseline.

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
- The public viewer surface is aligned again with the current workspace UI, including state badges and commit-focus behavior parity.
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
