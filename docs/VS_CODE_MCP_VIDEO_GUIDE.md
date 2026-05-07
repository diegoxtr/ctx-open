# VS Code MCP Video Guide

This is a recording-ready guide for showing how to connect VS Code / GitHub Copilot Chat to a local CTX MCP server.

Use it as:

- a video script
- a screen-recording checklist
- a short operator handout after the video

Canonical references:

- [MCP_LOCAL_QUICKSTART.md](MCP_LOCAL_QUICKSTART.md)
- [CTX_MCP_AGENT_SETUP.md](CTX_MCP_AGENT_SETUP.md)
- [LOCAL_CTX_INSTALLATION.md](LOCAL_CTX_INSTALLATION.md)
- [VS Code MCP configuration reference](https://code.visualstudio.com/docs/copilot/reference/mcp-configuration)
- [Add and manage MCP servers in VS Code](https://code.visualstudio.com/docs/copilot/customization/mcp-servers)

## Video Goal

By the end of the video, the viewer should understand:

1. CTX runs a local MCP server through `ctx-mcp`.
2. VS Code reads MCP server definitions from `mcp.json`.
3. VS Code uses a top-level `servers` object, not `mcpServers`.
4. The safest first setup is `--mode read-only`.
5. The connection is validated by asking Copilot Chat to call `ctx_plan` or inspect CTX context.

## Target Runtime

Recommended length: 4 to 6 minutes.

Audience:

- developers who already use VS Code
- operators who installed CTX locally
- agents/users who need a copy-paste MCP setup path

Recording setup:

- VS Code open on a CTX repository
- terminal visible inside VS Code
- Copilot Chat available with Agent mode
- local CTX install available

## Scene Map

| Time | Screen | Narration goal |
| --- | --- | --- |
| 0:00 | Title slide or repo README | "We will connect VS Code to a local CTX MCP server." |
| 0:20 | Terminal | Verify `ctx` and `ctx-mcp` exist. |
| 0:55 | Repository root | Explain that VS Code can use workspace `.vscode/mcp.json`. |
| 1:25 | `.vscode/mcp.json` | Paste the JSON configuration. |
| 2:20 | Command Palette | Run `MCP: List Servers` and start/restart the CTX server. |
| 3:00 | Copilot Chat | Ask a smoke-test prompt that requires CTX tools. |
| 4:00 | Output/logs | Show how to troubleshoot if tools do not appear. |
| 5:00 | Recap | Repeat the three important rules: `servers`, `stdio`, `read-only` first. |

## Scene 1 - Open With The Mental Model

Narration:

```text
CTX stores structured working context in the repository.
The MCP server is the bridge between that CTX workspace and an AI client.
In this video, VS Code is the MCP client and `ctx-mcp` is the local server.
```

On screen:

```text
VS Code / Copilot Chat
        |
        | MCP stdio
        v
ctx-mcp --repo <repo> --mode read-only
        |
        v
.ctx working context
```

## Scene 2 - Verify CTX Is Installed

Windows:

```powershell
ctx version
Test-Path C:\ctx\bin\ctx-mcp.cmd
```

Linux:

```bash
ctx version
test -x "$HOME/.local/share/ctx/bin/ctx-mcp"
```

macOS:

```bash
ctx version
test -x "$HOME/.local/share/ctx/bin/ctx-mcp"
```

Narration:

```text
Before touching VS Code, confirm the launcher exists.
Do not run `ctx-mcp` expecting a normal help screen.
It is a protocol server; VS Code starts it and talks to it over stdio.
```

## Scene 3 - Create `.vscode/mcp.json`

Create this file in the workspace:

```text
.vscode/mcp.json
```

Windows example:

```json
{
  "servers": {
    "ctx": {
      "type": "stdio",
      "command": "C:\\ctx\\bin\\ctx-mcp.cmd",
      "args": [
        "--repo",
        "C:\\path\\to\\ctx-repo",
        "--mode",
        "read-only"
      ],
      "env": {}
    }
  }
}
```

Linux example:

```json
{
  "servers": {
    "ctx": {
      "type": "stdio",
      "command": "/home/you/.local/share/ctx/bin/ctx-mcp",
      "args": [
        "--repo",
        "/home/you/sources/ctx-repo",
        "--mode",
        "read-only"
      ],
      "env": {}
    }
  }
}
```

macOS example:

```json
{
  "servers": {
    "ctx": {
      "type": "stdio",
      "command": "/Users/you/.local/share/ctx/bin/ctx-mcp",
      "args": [
        "--repo",
        "/Users/you/sources/ctx-repo",
        "--mode",
        "read-only"
      ],
      "env": {}
    }
  }
}
```

Narration:

```text
The important VS Code detail is the top-level `servers` object.
Some other clients use `mcpServers`; VS Code uses `servers`.
This server is local, so the type is `stdio`.
Start with read-only mode so the agent can inspect CTX before it can write anything.
```

## Scene 4 - Start The Server In VS Code

Use one of these flows:

1. Open `.vscode/mcp.json` and use the inline MCP controls that VS Code shows in the editor.
2. Open the Command Palette and run `MCP: List Servers`.
3. Select `ctx`.
4. Start or restart the server.
5. If VS Code asks for trust, review the command and approve it only if the path and repo are correct.

Narration:

```text
VS Code treats local MCP servers as code that can run on your machine.
Review the command before trusting it.
For CTX, check that the command points to your local `ctx-mcp` launcher and that `--repo` points to the intended repository.
```

## Scene 5 - Smoke Test In Copilot Chat

Prompt:

```text
Use the CTX MCP server to inspect this repository. Start with ctx_plan or ctx_status and tell me the active task, the current branch, and the next recommended step.
```

Expected result:

- Copilot Chat shows MCP tool usage.
- CTX returns repository status, plan, or context.
- The answer mentions the active CTX task or states that there is no open work.

If the chat does not use CTX tools, run:

```text
MCP: List Servers
```

Then choose the `ctx` server and show output/logs.

## Scene 6 - Switch To Write Mode Only When Needed

Read-only mode is the default for inspection:

```json
"args": ["--repo", "C:\\path\\to\\ctx-repo", "--mode", "read-only"]
```

Write mode allows the agent to create or update CTX artifacts:

```json
"args": ["--repo", "C:\\path\\to\\ctx-repo", "--mode", "write"]
```

Narration:

```text
Read-only is enough for planning, status, context, and audit.
Use write mode only when you intentionally want the agent to create tasks, evidence, decisions, conclusions, runbooks, or CTX commits.
```

## Troubleshooting Shot List

Show these only if the demo fails.

### The server does not appear

- Check that `.vscode/mcp.json` is valid JSON.
- Confirm VS Code uses `servers`, not `mcpServers`.
- Run `MCP: List Servers`.
- Restart the server from the MCP list.

### Tools do not appear in chat

- Run `MCP: Reset Cached Tools`.
- Restart VS Code.
- Confirm the server started without errors in the MCP output log.

### The process exits immediately

- Verify the `command` path exists.
- Verify the `--repo` path exists.
- Confirm the repository contains `.ctx`.
- Keep the server in `read-only` until the connection is stable.

### Windows path escaping breaks JSON

Use doubled backslashes:

```json
"command": "C:\\ctx\\bin\\ctx-mcp.cmd"
```

Do not use this in JSON:

```json
"command": "C:\ctx\bin\ctx-mcp.cmd"
```

## Final Recap

Narration:

```text
The whole setup is one file.
For VS Code, create `.vscode/mcp.json`.
Use `servers`, `type: stdio`, the local `ctx-mcp` command, and a repo path.
Start in read-only mode, validate with a smoke test, and switch to write mode only when you want the agent to update CTX.
```

Final on-screen card:

```text
VS Code + CTX MCP

1. Install CTX
2. Create .vscode/mcp.json
3. Use servers + stdio
4. Start ctx in read-only
5. Smoke test with ctx_plan
```

