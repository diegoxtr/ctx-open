# CTX 1.0.14 Release Notes

Release date:

- `2026-05-12`

Version:

- `1.0.14`

## Summary

CTX 1.0.14 is a packaging hotfix for the local Viewer MCP server controls.

The public 1.0.13 bundles include `ctx-mcp` and `Ctx.Mcp`, but the packaged Viewer could still look for them under the default install root instead of the active portable or installed layout. This release makes Viewer MCP discovery follow the actual install root.

## Fixed

- Packaged Viewer MCP detection now honors `CTX_INSTALL_ROOT`.
- If `CTX_INSTALL_ROOT` is not set, the Viewer infers the install root from the bundled `viewer/` directory and validates sibling `bin/ctx-mcp` plus `mcp/Ctx.Mcp` paths.
- Portable `ctx-viewer` launchers now set `CTX_INSTALL_ROOT` before starting the Viewer.
- Installed Windows and local publish Viewer launchers now set `CTX_INSTALL_ROOT`.

## Validation Checklist

- `ctx version` reports `1.0.14`.
- `dotnet build Ctx.sln --no-restore` passes.
- `dotnet test Ctx.Tests\Ctx.Tests.csproj --no-restore` passes.
- `scripts/build-distribution.ps1` regenerates portable bundles.
- `ctx-win-x64.zip` includes `bin/ctx-mcp.cmd`, `mcp/Ctx.Mcp.exe`, and a `bin/ctx-viewer.cmd` launcher that sets `CTX_INSTALL_ROOT`.
- Static pages reference `v1.0.14`.
- `git ls-files .ctx` is empty for the public repository root.
- Private repository paths are absent from public docs and source.
