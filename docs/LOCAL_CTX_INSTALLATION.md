# CTX Local Installation
If a language model and its agent lose context, this is the tool you need.

## Objective

Install CTX locally in `C:\ctx` so the CLI can be used without depending on the repository workspace.

For Linux/macOS published installs, the default install root is `$HOME/.local/share/ctx`.

## Command

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\publish-local.ps1
```

Alternative source-install bootstrap:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\install-ctx.ps1 -Mode source -SourceRepoPath C:\\sources\\ctx-open
```

Single-entry bootstrap with install/update/repair detection:

```powershell
powershell -ExecutionPolicy Bypass -File .\install.ps1
```

Important:

- even if you run `install.ps1` from a cloned repository checkout, the default path is still the published portable bundle
- running the bootstrap from a clone does not imply `source` mode
- use `-Mode source` only when you intentionally want to compile CTX from that checkout

Recommended default:

- use `install.ps1` for user-facing local installation
- use `scripts/install-ctx.ps1` only as the lower-level engine
- keep `scripts/publish-local.ps1` for repo-local publish/refresh workflows

To request machine-wide PATH exposure on Windows:

```powershell
powershell -ExecutionPolicy Bypass -File .\install.ps1 -PathScope Machine
```

## Expected result

- CLI published to `C:\ctx\bin`
- MCP server published to `C:\ctx\mcp`
- viewer published to `C:\ctx\viewer`
- `ctx` available in a new terminal session
- `ctx-mcp` available for MCP-capable agents
- `ctx-viewer` available to launch the local viewer
- helper prompt copied into `C:\ctx\prompts`
- context docs copied into `C:\ctx\docs`
- install metadata written to `C:\ctx\ctx-install.json`
- installer output includes `CTX_INSTALL_ROOT`, `CTX_BIN_PATH`, and `CTX_MCP_PATH`

## Locations

- CLI: `C:\ctx\bin\Ctx.Cli.exe`
- CLI launcher: `C:\ctx\bin\ctx.cmd`
- MCP server: `C:\ctx\mcp\Ctx.Mcp.exe`
- MCP launcher: `C:\ctx\bin\ctx-mcp.cmd`
- viewer: `C:\ctx\viewer\Ctx.Viewer.exe`
- viewer launcher: `C:\ctx\bin\ctx-viewer.cmd`

Linux/macOS equivalents:

- install root: `$HOME/.local/share/ctx`
- CLI launcher: `$HOME/.local/share/ctx/bin/ctx`
- MCP launcher: `$HOME/.local/share/ctx/bin/ctx-mcp`
- MCP server: `$HOME/.local/share/ctx/mcp/Ctx.Mcp`
- viewer launcher: `$HOME/.local/share/ctx/bin/ctx-viewer`

## Verification

```powershell
ctx helper
ctx version
Test-Path C:\ctx\bin\ctx-mcp.cmd
ctx-viewer
```

MCP agent setup is documented in [CTX_MCP_AGENT_SETUP.md](C:/sources/ctx-open/docs/CTX_MCP_AGENT_SETUP.md).

Viewer-specific verification:

```powershell
Invoke-RestMethod http://127.0.0.1:5271/api/overview
Invoke-RestMethod http://127.0.0.1:5271/api/mcp-status
```

Expected local viewer behavior:

- the header shows `local-version` when the viewer runs from `C:\ctx\viewer`
- the top bar shows an `MCP Server` status capsule
- `/api/mcp-status` reports `healthy: true` when the MCP launcher, MCP executable, and at least one `Ctx.Mcp` process are present

Recommended first verification flow:

```powershell
ctx helper
ctx status
ctx next
```

## PATH note

The single-entry bootstrap can control PATH scope directly:

- `-PathScope Auto`
- `-PathScope User`
- `-PathScope Machine`
- `-PathScope None`

The legacy publish script adds `C:\ctx\bin` to the current user's `PATH`.

If the current terminal does not pick that up automatically, open a new terminal.
