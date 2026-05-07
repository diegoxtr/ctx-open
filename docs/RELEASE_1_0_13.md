# CTX 1.0.13 Release Notes

Release date:

- `2026-05-07`

Version:

- `1.0.13`

## Summary

CTX 1.0.13 focuses on planning discipline, MCP parity, and Viewer usability.

The release keeps the agent start path simple: install CTX, connect an MCP-capable client, call `ctx_plan`, read the returned `runbookSuggestions`, and keep future ideas in `ctx roadmap` or parked epics instead of polluting `ctx next`.

## Highlights

- First-class epics preserve future planning without making parked work executable.
- `ctx gaps`, `ctx roadmap`, `ctx_gaps`, `ctx_roadmap`, and MCP epic tools make planning state visible to agents.
- Viewer MCP controls, release bell, graph zoom, progressive rendering, and parked-epic navigation make the local UI more useful on large repositories.
- Documentation now explains MCP parity, roadmap/gaps behavior, task-attached runbooks, and browser path limits for repository Browse.

## Added

- First-class `Epic` domain entity for durable future planning.
- CLI commands: `ctx epic add`, `ctx epic update`, `ctx epic promote`, `ctx epic list`, and `ctx epic show`.
- MCP parity for planning review through `ctx_gaps` and `ctx_roadmap`.
- MCP parity for epics through `ctx_epic_add`, `ctx_epic_update`, `ctx_epic_promote`, `ctx_epic_list`, and `ctx_epic_show`.
- `ctx gaps` for read-only unresolved planning gaps, blocked work, and deferred candidates.
- `ctx roadmap` for read-only future planning lanes.
- `docs/CTX_MCP_TOOL_PARITY.md` to track practical CLI-to-MCP parity.
- `docs/ROADMAP_AND_GAPS_DESIGN.md` plus localized references for future planning semantics.
- `docs/VS_CODE_MCP_VIDEO_GUIDE.md` as a recording-ready walkthrough for connecting VS Code / Copilot Chat to the local CTX MCP server.
- `scripts/update-release-version.ps1` to update public release version surfaces before artifact rebuilds.
- CTX Viewer graph zoom controls, progressive graph rendering, app footer, release notification bell, MCP `Start` / `Stop` controls, and a compact parked-epics rail.

## Changed

- `ctx next` no longer recommends `Blocked` tasks as executable work; blocked and parked work remain visible through `ctx gaps` and `ctx roadmap`.
- `ctx plan` exposes one authoritative top-level `runbookSuggestions` list for the current turn.
- Task-attached runbooks are promoted ahead of generic goal or trigger fallback matches.
- `ctx preflight --operation <operation>` accepts arbitrary operation tokens and selects matching runbooks dynamically.
- Viewer Browse opens the local folder picker first and validates `.ctx` only after the operator selects a folder.
- Viewer docs now state the browser security boundary clearly: browser directory APIs expose handles or relative paths, while the local backend owns real repository-path loading.
- The public landing and MCP setup pages now target CTX 1.0.13.
- The public MCP setup page now includes Gemini CLI and Devin configuration examples, with a clear Devin runtime-boundary warning.
- Release preflight now checks the packaging contract for console-referenced files: if `ctx helper`, install docs, or release notes point to an installed file, the portable bundle and install scripts must ship it.

## Fixed

- Viewer dense graph rendering no longer restarts endlessly after completion or on node selection.
- Parked epic cards now resolve the origin commit, hydrate History pages when needed, select the commit, scroll the History panel, and highlight the target row.
- Viewer node selection updates detail panels and path highlighting without triggering a full graph redraw.
- Viewer Browse no longer performs a repository scan before the user selects a local folder.
- Public README references now include the MCP parity and roadmap/gaps documents they point to.
- Public technical index now points to public live-demo and UNJu talk pages instead of private slide-source files.
- Static live-demo image paths now resolve against the shipped public screenshot assets.
- Portable distribution bundles now include the documented `ctx`, `ctx-mcp`, `ctx-agent-acp`, and optional `ctx-viewer` launchers instead of relying only on post-install wrapper generation.
- Published install flows now include `docs/CLI_COMMANDS.md` in the installed docs set so `ctx helper` does not point operators at a missing CLI reference.

## Validation Checklist

- `ctx version` reports `1.0.13`.
- `dotnet build Ctx.sln --no-restore` passes.
- `dotnet test Ctx.Tests\Ctx.Tests.csproj --no-restore` passes.
- MCP parity tests cover the newly public epic surfaces.
- JSON snippets in MCP, ACP, and CLI docs parse.
- Static pages reference `v1.0.13`.
- Markdown and live-demo local links resolve.
- `git ls-files .ctx` is empty for the public repository root.
- Private repository paths are absent from public docs and source.
- Installed packages include `prompts/CTX_HELPER_PROMPT.md`, `prompts/CTX_AGENT_PROMPT.md`, `docs/CLI_COMMANDS.md`, `docs/CTX_VIEWER_GUIDE.md`, and `docs/CTX_AUTONOMOUS_OPERATION_PROTOCOL.md`.
- Release artifacts are regenerated from this branch before tagging or publishing.
