# Run CTX MCP Locally

This guide is the short public setup path for running the CTX MCP server on your own machine.

GitHub Pages is static. It cannot run CTX or inspect your local `.ctx` repository. The MCP server runs locally through your MCP-capable agent or IDE.

## 1. Install CTX

Install CTX from the latest release, then identify the install root for your platform.

| Platform | Default install root | MCP launcher |
|---|---|---|
| Windows | `C:\ctx` | `C:\ctx\bin\ctx-mcp.cmd` |
| Linux | `/home/you/.local/share/ctx` | `/home/you/.local/share/ctx/bin/ctx-mcp` |
| macOS | `/Users/you/.local/share/ctx` | `/Users/you/.local/share/ctx/bin/ctx-mcp` |

## 2. Choose The Repository

The MCP server binds to one CTX repository at startup. Use a folder that contains `.ctx`.

Examples:

```powershell
C:\path\to\ctx-repo
```

```bash
/home/you/sources/ctx-repo
```

Replace `you` with your real local user name. MCP JSON values should use concrete absolute paths; many clients do not expand `$HOME` inside JSON.

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

Keep `type` explicit for local stdio startup. Keep `tools: ["*"]` in clients that support tool allowlists so every CTX MCP tool, including `ctx_plan`, is exposed.

Windows:

```json
{
  "mcpServers": {
    "ctx": {
      "type": "stdio",
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
      "type": "stdio",
      "command": "/home/you/.local/share/ctx/bin/ctx-mcp",
      "args": ["--repo", "/home/you/sources/ctx-repo", "--mode", "read-only"],
      "env": {},
      "tools": ["*"]
    }
  }
}
```

For macOS, use `/Users/you/.local/share/ctx/bin/ctx-mcp` and `/Users/you/sources/ctx-repo`.

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
      "type": "stdio",
      "command": "C:\\ctx\\bin\\ctx-mcp.cmd",
      "args": ["--repo", "C:\\path\\to\\ctx-repo", "--mode", "read-only"],
      "env": {}
    }
  }
}
```

For a recording-ready walkthrough with narration, shot list, smoke test, and troubleshooting flow, see [VS_CODE_MCP_VIDEO_GUIDE.md](VS_CODE_MCP_VIDEO_GUIDE.md).

### Codex

Add a server entry to `config.toml`:

```toml
[mcp_servers.ctx]
command = "C:\\ctx\\bin\\ctx-mcp.cmd"
args = ["--repo", "C:\\path\\to\\ctx-repo", "--mode", "read-only"]
```

### Gemini CLI

Gemini CLI reads MCP servers from `settings.json`. Use `.gemini/settings.json` for a project-local setup, or your user Gemini settings file for a global setup:

```json
{
  "mcpServers": {
    "ctx": {
      "type": "stdio",
      "command": "C:\\ctx\\bin\\ctx-mcp.cmd",
      "args": ["--repo", "C:\\path\\to\\ctx-repo", "--mode", "read-only"],
      "env": {},
      "tools": ["*"]
    }
  }
}
```

Gemini also supports `gemini mcp add`; use the JSON form above when you want the repository path and mode to be explicit in versioned project setup.

### Devin

Devin can connect to MCP servers, but the runtime boundary matters. A cloud Devin session cannot start `C:\ctx\bin\ctx-mcp.cmd` on your private laptop. Use this STDIO shape only in an environment where Devin can execute the CTX launcher and access the target repository:

```json
{
  "transport": "STDIO",
  "command": "C:\\ctx\\bin\\ctx-mcp.cmd",
  "args": ["--repo", "C:\\path\\to\\ctx-repo", "--mode", "read-only"],
  "env_variables": {}
}
```

If Devin is running remotely, expose CTX through an approved reachable environment instead of pointing Devin at a local private path.

## 6. Restart And Smoke Test

Restart the agent or IDE after changing MCP configuration.

Ask the agent:

```text
Use the ctx MCP server. Call ctx_plan with purpose "quickstart-smoke-test". Report the repository branch, dirty state, recommended task, context packet id, and runbook suggestions.
```

For real work, ask the agent to apply returned playbooks:

```text
Use the ctx MCP server before acting.
Call ctx_plan with the active task or current purpose.
Read data.runbookSuggestions.
For every returned runbook, check Preconditions, follow applicable Do steps, validate with Verify, and stop at EscalationBoundary if a failure signal appears.
Do not infer playbooks from chat memory; use the runbookSuggestions returned by CTX.
```

Expected:

- the agent can see the `ctx` MCP server
- `ctx_plan` returns repository state, a next-step recommendation, focused context, runbook suggestions, and guidance
- the agent can call follow-up tools such as `ctx_status` or `ctx_audit` when narrower validation is needed

## Troubleshooting

- If no tools appear, restart the agent and re-check the config file.
- If the server says no `.ctx` repository was found, fix the `--repo` path.
- If write tools are rejected, the server is running in `read-only` mode.
- If you need another repository, add another MCP server entry with a different name.
- Do not expect `ctx-mcp` to print normal CLI help; MCP clients communicate with it over stdio.
