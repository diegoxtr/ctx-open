# CTX 1.0.10 Release Notes

发布日期：

- `2026-04-27`

版本：

- `1.0.10`

## Summary

CTX 1.0.10 adds the local stdio MCP server line, improves CTX Viewer graph and history coherence, and aligns the public installer layout for CLI, viewer, and MCP usage.

## Highlights

- `Ctx.Mcp` local stdio MCP server.
- Read-only MCP inspection by default, with guarded write mode when explicitly enabled.
- CTX Viewer starts in Split view for first-time sessions.
- Viewer history loads the latest 20 commits first and can load older commits on demand.
- Commit graphs stay centered on the cognitive task captured by the selected commit.
- Local viewer builds show `local-version`.
- Installers include `ctx-mcp` and print parseable install paths.

## Validation

- `ctx version` reports `1.0.10`.
- `dotnet build Ctx.sln --no-restore` passes.
- `dotnet test Ctx.Tests\Ctx.Tests.csproj --no-restore` passes.
- `Ctx.Mcp` builds and is available through `ctx-mcp`.
- The public repository does not include private `.ctx` state.
