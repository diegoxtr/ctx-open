# Run CTX MCP Locally

This guide is the short public setup path for running the CTX MCP server on your own machine.

GitHub Pages is static. It cannot run CTX or inspect your local `.ctx` repository. The MCP server runs locally through your MCP-capable agent or IDE.

## 1. Install CTX

Install CTX from the latest release, then identify the install root for your platform.

| Platform | Default install root | MCP launcher |
|---|---|---|
| Windows | `C:\ctx` | `C:\ctx\bin\ctx-mcp.cmd` |
| Linux | `$HOME/.local/share/ctx` | `$HOME/.local/share/ctx/bin/ctx-mcp` |
| macOS | `$HOME/.local/share/ctx` | `$HOME/.local/share/ctx/bin/ctx-mcp` |

## 2. Choose The Repository

The MCP server binds to one CTX repository at startup. Use a folder that contains `.ctx`.

Examples:

```powershell
C:\sources\ctx-open
```

```bash
$HOME/sources/ctx-open
```

Prefer one MCP server entry per repository. Do not use one server to silently switch between unrelated repos.

## 3. Verify The Launcher

Windows:

```powershell
Test-Path C:\ctx\bin\ctx-mcp.cmd
Test-Path C:\ctx\mcp\Ctx.Mcp.exe
```

Linux/macOS:

```bash
test -x "$HOME/.local/share/ctx/bin/ctx-mcp"
test -x "$HOME/.local/share/ctx/mcp/Ctx.Mcp"
```

## 4. Configure Your Agent

Most MCP clients use a JSON block like this.

Windows:

```json
{
  "mcpServers": {
    "ctx": {
      "command": "C:\\ctx\\bin\\ctx-mcp.cmd",
      "args": ["--repo", "C:\\sources\\ctx-open", "--mode", "read-only"]
    }
  }
}
```

Linux/macOS:

```json
{
  "mcpServers": {
    "ctx": {
      "command": "$HOME/.local/share/ctx/bin/ctx-mcp",
      "args": ["--repo", "$HOME/sources/ctx-open", "--mode", "read-only"]
    }
  }
}
```

Use `--mode write` only when you intentionally want the agent to create or update CTX artifacts.

## 5. Agent-Specific Examples

### Claude Code

Windows:

```powershell
claude mcp add ctx -- C:\ctx\bin\ctx-mcp.cmd --repo C:\sources\ctx-open --mode read-only
```

Linux/macOS:

```bash
claude mcp add ctx -- "$HOME/.local/share/ctx/bin/ctx-mcp" --repo "$HOME/sources/ctx-open" --mode read-only
```

### Claude Desktop

Use the generic `mcpServers` JSON block in the Claude Desktop configuration file.

### VS Code / Copilot

VS Code uses `servers` instead of `mcpServers`:

```json
{
  "servers": {
    "ctx": {
      "command": "C:\\ctx\\bin\\ctx-mcp.cmd",
      "args": ["--repo", "C:\\sources\\ctx-open", "--mode", "read-only"]
    }
  }
}
```

### Codex

Add a server entry to `config.toml`:

```toml
[mcp_servers.ctx]
command = "C:\\ctx\\bin\\ctx-mcp.cmd"
args = ["--repo", "C:\\sources\\ctx-open", "--mode", "read-only"]
```

## 6. Restart And Smoke Test

Restart the agent or IDE after changing MCP configuration.

Ask the agent:

```text
Use the ctx MCP server. Call ctx_status and ctx_audit for the configured repository. Report branch, head, dirty state, warning count, and error count.
```

Expected:

- the agent can see the `ctx` MCP server
- `ctx_status` returns branch, head, and dirty state
- `ctx_audit` returns without consistency errors

## Troubleshooting

- If no tools appear, restart the agent and re-check the config file.
- If the server says no `.ctx` repository was found, fix the `--repo` path.
- If write tools are rejected, the server is running in `read-only` mode.
- If you need another repository, add another MCP server entry with a different name.
- Do not expect `ctx-mcp` to print normal CLI help; MCP clients communicate with it over stdio.
