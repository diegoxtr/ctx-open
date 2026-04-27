# CTX 1.0.10 Release Notes

Release date:

- `2026-04-27`

Version:

- `1.0.10`

## Summary

CTX 1.0.10 prepares the public line for local MCP-capable agents, improves CTX Viewer coherence and startup behavior, and tightens installer/distribution validation around the CLI, viewer, and MCP server layout.

## Highlights

- Local stdio MCP server included in the solution as `Ctx.Mcp`.
- MCP read-only mode for safe inspection and guarded write mode for intentional cognitive artifact creation.
- CTX Viewer starts in Split view for first-time sessions.
- Viewer history loads in 20-commit pages with explicit and scroll-based continuation.
- Commit-focus graphs now stay centered on the cognitive task captured by the selected commit.
- Local viewer builds display `local-version` instead of a public release tag.
- Installers and local publish now include `ctx-mcp` and expose parseable install paths.

## Added

- `Ctx.Mcp` project.
- `ctx-mcp` launcher in local publish and distribution layouts.
- `/api/mcp-status` and the top-bar `MCP Server` status capsule in CTX Viewer.
- MCP setup documentation for generic MCP clients, Claude/Anthropic Desktop, VS Code/Copilot-style clients, DeepSeek-backed clients, and Codex.
- `runbookSuggestions` and `additionalRunbooksAvailable` in `ctx next`.

## Changed

- Viewer graph routing now prefers the meaningful cognitive route over misleading direct support shortcuts.
- Commit-focus expansion no longer expands sibling historical tasks just because they share a broad goal.
- History hydration now reads the latest 20 commits first and loads older commits on demand.
- Local publish and distribution scripts now install CLI, viewer, and MCP assets consistently across Windows, Linux, and macOS.

## Fixed

- Semantic JSON diffing avoids false modified entries caused by collection reference inequality.
- `ctx audit` reports tasks without goals and `ctx task update --goal` can repair them.
- Linux/macOS installers include the MCP server path and print `CTX_INSTALL_ROOT`, `CTX_BIN_PATH`, and `CTX_MCP_PATH`.

## Validation Checklist

- `ctx version` reports `1.0.10`.
- `dotnet build Ctx.sln --no-restore` passes.
- `dotnet test Ctx.Tests\Ctx.Tests.csproj --no-restore` passes.
- `Ctx.Mcp` builds and exposes MCP tools through the installed `ctx-mcp` launcher.
- `node --check Ctx.Viewer\wwwroot\app.js` passes when Node is available.
- PowerShell and Bash install scripts parse.
- `git ls-files .ctx` is empty for the public repository root.
- `git status --short` does not include private `.ctx` state.
