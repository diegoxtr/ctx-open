# Run CTX MCP Locally

This guide is the short public setup path for running the CTX MCP server on your own machine.

GitHub Pages is static. It cannot run CTX or inspect your local `.ctx` repository. The MCP server runs locally through your MCP-capable agent or IDE.

Current public release: CTX 1.0.12.

## 1. Install CTX

Install CTX from the latest release, then identify the install root for your platform.

| Platform | Default install root | MCP launcher | ACP launcher |
|---|---|---|---|
| Windows | `C:\ctx` | `C:\ctx\bin\ctx-mcp.cmd` | `C:\ctx\bin\ctx-agent-acp.cmd` |
| Linux | `$HOME/.local/share/ctx` | `$HOME/.local/share/ctx/bin/ctx-mcp` | `$HOME/.local/share/ctx/bin/ctx-agent-acp` |
| macOS | `$HOME/.local/share/ctx` | `$HOME/.local/share/ctx/bin/ctx-mcp` | `$HOME/.local/share/ctx/bin/ctx-agent-acp` |

## 2. Choose The Repository

The MCP server binds to one CTX repository at startup. Use a folder that contains `.ctx`.

Examples:

```powershell
C:\path\to\ctx-repo
```

```bash
$HOME/sources/ctx-repo
```

Prefer one MCP server entry per repository. Do not use one server to silently switch between unrelated repos.

## 3. Verify The Launcher

Windows:

```powershell
Test-Path C:\ctx\bin\ctx-mcp.cmd
Test-Path C:\ctx\bin\ctx-agent-acp.cmd
Test-Path C:\ctx\mcp\Ctx.Mcp.exe
```

Linux/macOS:

```bash
test -x "$HOME/.local/share/ctx/bin/ctx-mcp"
test -x "$HOME/.local/share/ctx/bin/ctx-agent-acp"
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
      "args": ["--repo", "C:\\path\\to\\ctx-repo", "--mode", "read-only"],
      "env": {},
      "tools": ["*"]
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
      "args": ["--repo", "$HOME/sources/ctx-repo", "--mode", "read-only"],
      "env": {},
      "tools": ["*"]
    }
  }
}
```

Use `--mode write` only when you intentionally want the agent to create or update CTX artifacts.

## 5. Agent-Specific Examples

### Claude Code

Windows:

```powershell
claude mcp add ctx -- C:\ctx\bin\ctx-mcp.cmd --repo C:\path\to\ctx-repo --mode read-only
```

Linux/macOS:

```bash
claude mcp add ctx -- "$HOME/.local/share/ctx/bin/ctx-mcp" --repo "$HOME/sources/ctx-repo" --mode read-only
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
      "args": ["--repo", "C:\\path\\to\\ctx-repo", "--mode", "read-only"]
    }
  }
}
```

### Codex

Add a server entry to `config.toml`:

```toml
[mcp_servers.ctx]
command = "C:\\ctx\\bin\\ctx-mcp.cmd"
args = ["--repo", "C:\\path\\to\\ctx-repo", "--mode", "read-only"]
```

## 6. ACP Adapter

MCP is the main tool-rich agent integration path. CTX 1.0.12 also ships `ctx-agent-acp` for clients that want an ACP-style session protocol.

Windows:

```powershell
C:\ctx\bin\ctx-agent-acp.cmd --repo C:\path\to\ctx-repo
```

Linux/macOS:

```bash
"$HOME/.local/share/ctx/bin/ctx-agent-acp" --repo "$HOME/sources/ctx-repo"
```

ACP is read-only in the current public release. Use `ACP_LOCAL_CONNECTION_GUIDE.md` for the JSON-RPC message flow.

## 7. Restart And Smoke Test

Restart the agent or IDE after changing MCP configuration.

Ask the agent:

```text
Use the ctx MCP server. Call ctx_plan with purpose "mcp-local-smoke-test". Report branch, dirty state, recommended task, context packet id, runbook suggestions, and guidance.
```

Expected:

- the agent can see the `ctx` MCP server
- `ctx_plan` returns branch, dirty state, next work, context, runbooks, and guidance
- lower-level `ctx_status` and `ctx_audit` can be used afterward for focused diagnostics

## Troubleshooting

- If no tools appear, restart the agent and re-check the config file.
- If the server says no `.ctx` repository was found, fix the `--repo` path.
- If write tools are rejected, the server is running in `read-only` mode.
- If you need another repository, add another MCP server entry with a different name.
- Do not expect `ctx-mcp` to print normal CLI help; MCP clients communicate with it over stdio.
