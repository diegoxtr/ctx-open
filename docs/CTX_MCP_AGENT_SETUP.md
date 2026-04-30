# CTX MCP Agent Setup

This guide shows how to expose CTX to any MCP-capable agent or IDE client.

The current CTX MCP server is local and stdio-based.

It runs in `read-only` mode by default, which lets an agent inspect CTX without asking the user to paste `ctx status`, `ctx next`, `ctx graph summary`, or thread output into chat.

It can also run in `write` mode when the operator intentionally wants an agent to create cognitive artifacts through MCP.

For a shorter public setup path, use [MCP_LOCAL_QUICKSTART.md](MCP_LOCAL_QUICKSTART.md). The static landing page also includes copy-ready configuration buttons at `docs/live-demo/mcp-local.html`.

## Platform Paths

| Platform | Default install root | MCP launcher |
|---|---|---|
| Windows | `C:\ctx` | `C:\ctx\bin\ctx-mcp.cmd` |
| Linux | `$HOME/.local/share/ctx` | `$HOME/.local/share/ctx/bin/ctx-mcp` |
| macOS | `$HOME/.local/share/ctx` | `$HOME/.local/share/ctx/bin/ctx-mcp` |

Replace the repository path in every example with the local folder that contains `.ctx`.

## What You Get

The installed MCP server exposes these tools:

```text
ctx_version
ctx_doctor
ctx_status
ctx_audit
ctx_next
ctx_context
ctx_plan
ctx_graph_summary
ctx_graph_show
ctx_graph_export
ctx_graph_lineage
ctx_thread_reconstruct
ctx_preflight
ctx_operational_review
ctx_check
ctx_closeout
ctx_log
ctx_diff
ctx_artifact_list
ctx_artifact_show
ctx_goal_list
ctx_goal_show
ctx_task_list
ctx_task_show
ctx_hypothesis_list
ctx_hypothesis_show
ctx_hypothesis_rank
ctx_evidence_list
ctx_evidence_show
ctx_decision_list
ctx_decision_show
ctx_conclusion_list
ctx_conclusion_show
ctx_runbook_list
ctx_runbook_show
ctx_trigger_list
ctx_trigger_show
ctx_bootstrap_map
ctx_init
ctx_goal_add
ctx_goal_update
ctx_line_open
ctx_task_add
ctx_task_update
ctx_hypothesis_add
ctx_hypothesis_update
ctx_hypothesis_relate
ctx_hypothesis_merge
ctx_hypothesis_supersede
ctx_evidence_add
ctx_evidence_share
ctx_decision_add
ctx_decision_update
ctx_conclusion_add
ctx_conclusion_update
ctx_runbook_add
ctx_trigger_add
ctx_bootstrap_apply
ctx_commit
```

Write tools are rejected unless the server was started with `--mode write`.

## Prerequisites

- Windows PowerShell
- .NET 8 SDK available in the development environment
- A local CTX repository with a `.ctx` folder
- CTX published locally to `C:\ctx`
- An agent or IDE client that supports MCP servers through an MCP JSON configuration

## Step 1: Publish CTX Locally

From the CTX repository:

```powershell
cd C:\path\to\ctx-repo
powershell -ExecutionPolicy Bypass -File .\scripts\publish-local.ps1
```

Expected files:

```text
C:\ctx\bin\ctx.cmd
C:\ctx\bin\ctx-viewer.cmd
C:\ctx\bin\ctx-mcp.cmd
C:\ctx\mcp\Ctx.Mcp.exe
```

## Step 2: Verify The Target Repository

Choose the repository the agent should inspect.

Example:

```powershell
cd C:\path\to\ctx-repo
C:\ctx\bin\ctx.cmd status
C:\ctx\bin\ctx.cmd audit
```

Expected:

- `ctx status` finds the repository
- `ctx audit` returns 0 errors before you rely on agent context

## Step 3: Smoke-Test The MCP Server

The MCP server speaks over stdio, so it is normally launched by the agent client. You can still verify that the command exists:

```powershell
Test-Path C:\ctx\bin\ctx-mcp.cmd
Test-Path C:\ctx\mcp\Ctx.Mcp.exe
```

Both should return:

```text
True
True
```

Do not expect `ctx-mcp.cmd` to print a normal CLI help screen. MCP clients communicate with it using the MCP protocol.

## Step 4: Add The MCP Server To The Agent

Most MCP clients accept a local stdio server block shaped like this.

For clients that expose a tool allowlist, keep `tools: ["*"]` so the full CTX MCP surface is available.

```json
{
  "mcpServers": {
    "ctx": {
      "type": "stdio",
      "command": "C:\\ctx\\bin\\ctx-mcp.cmd",
      "args": [
        "--repo",
        "C:\\path\\to\\ctx-repo",
        "--mode",
        "read-only"
      ],
      "env": {},
      "tools": ["*"]
    }
  }
}
```

Replace `C:\\path\\to\\ctx-repo` with the repository the agent should inspect.

Keep `--mode read-only` unless you intentionally want the agent to write cognitive artifacts.

Some clients use a slightly different connection type:

- VS Code uses top-level `servers` and `type: "stdio"`.
- GitHub Copilot CLI/SDK uses `mcpServers`, commonly with `type: "local"` and `tools: ["*"]`.
- Claude Code accepts explicit `type: "stdio"` in JSON-based setup.

For write-enabled operation:

```json
{
  "mcpServers": {
    "ctx": {
      "type": "stdio",
      "command": "C:\\ctx\\bin\\ctx-mcp.cmd",
      "args": [
        "--repo",
        "C:\\path\\to\\ctx-repo",
        "--mode",
        "write"
      ],
      "env": {},
      "tools": ["*"]
    }
  }
}
```

Use write mode only for trusted local repositories. The write tools mutate `.ctx`.

## Client Configuration Examples

The CTX MCP server is the same process in every client:

```text
C:\ctx\bin\ctx-mcp.cmd --repo C:\path\to\ctx-repo --mode read-only
```

What changes is only the MCP client configuration format.

### Claude Code / Anthropic

Claude Code can add a local stdio server from the CLI:

```powershell
claude mcp add ctx -- C:\ctx\bin\ctx-mcp.cmd --repo C:\path\to\ctx-repo --mode read-only
```

For write-enabled local operation:

```powershell
claude mcp add ctx-write -- C:\ctx\bin\ctx-mcp.cmd --repo C:\path\to\ctx-repo --mode write
```

Project-scoped Claude Code configuration can also live in `.mcp.json`:

```json
{
  "mcpServers": {
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

Use the write-mode variant only in trusted local repositories:

```json
{
  "mcpServers": {
    "ctx-write": {
      "type": "stdio",
      "command": "C:\\ctx\\bin\\ctx-mcp.cmd",
      "args": [
        "--repo",
        "C:\\path\\to\\ctx-repo",
        "--mode",
        "write"
      ],
      "env": {}
    }
  }
}
```

### Claude Desktop

Claude Desktop uses the same `mcpServers` shape in its desktop configuration file:

```json
{
  "mcpServers": {
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

Restart Claude Desktop after editing the config.

### VS Code / GitHub Copilot Chat

VS Code stores MCP configuration in `mcp.json`. This can be workspace-local at:

```text
.vscode\mcp.json
```

or in the user MCP configuration.

Use `servers`, not `mcpServers`:

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

After changing this file, run `MCP: List Servers` or `MCP: Reset Cached Tools` from the Command Palette if VS Code does not show the updated tools.

### GitHub Copilot CLI

Copilot CLI can add MCP servers interactively with `/mcp add`, or by editing:

```text
~/.copilot/mcp-config.json
```

Example:

```json
{
  "mcpServers": {
    "ctx": {
      "type": "local",
      "command": "C:\\ctx\\bin\\ctx-mcp.cmd",
      "args": [
        "--repo",
        "C:\\path\\to\\ctx-repo",
        "--mode",
        "read-only"
      ],
      "env": {},
      "tools": ["*"]
    }
  }
}
```

Use `tools: ["*"]` to expose every CTX MCP tool that the server reports.

### GitHub Copilot SDK

When creating a Copilot SDK session, configure CTX as a local MCP server:

```ts
import { CopilotClient } from "@github/copilot-sdk";

const client = new CopilotClient();

const session = await client.createSession({
  mcpServers: {
    ctx: {
      type: "local",
      command: "C:\\ctx\\bin\\ctx-mcp.cmd",
      args: [
        "--repo",
        "C:\\path\\to\\ctx-repo",
        "--mode",
        "read-only"
      ],
      env: {},
      tools: ["*"],
      timeout: 30000
    }
  }
});
```

### DeepSeek

DeepSeek is a model/provider, not a single canonical local MCP client. The practical setup is:

1. choose an MCP-capable client or agent runtime that can use a DeepSeek model;
2. configure that client with the CTX MCP server using one of the formats above;
3. configure the model/provider separately with the DeepSeek API key or OpenAI-compatible endpoint required by that client.

For example, if a DeepSeek-backed custom agent uses a generic `mcpServers` JSON shape:

```json
{
  "model": {
    "provider": "deepseek",
    "model": "deepseek-chat"
  },
  "mcpServers": {
    "ctx": {
      "type": "stdio",
      "command": "C:\\ctx\\bin\\ctx-mcp.cmd",
      "args": [
        "--repo",
        "C:\\path\\to\\ctx-repo",
        "--mode",
        "read-only"
      ],
      "env": {},
      "tools": ["*"]
    }
  }
}
```

The exact `model` block is client-specific. The CTX MCP block stays the same.

### Multiple Repositories

Prefer one MCP server entry per CTX repository:

```json
{
  "mcpServers": {
    "ctx-main": {
      "type": "stdio",
      "command": "C:\\ctx\\bin\\ctx-mcp.cmd",
      "args": ["--repo", "C:\\path\\to\\ctx-repo", "--mode", "read-only"],
      "env": {},
      "tools": ["*"]
    },
    "ctx-almacen-demo": {
      "type": "stdio",
      "command": "C:\\ctx\\bin\\ctx-mcp.cmd",
      "args": [
        "--repo",
        "C:\\path\\to\\ctx-repo\\examples\\ctx\\almacen-barrial-reglas",
        "--mode",
        "read-only"
      ],
      "env": {},
      "tools": ["*"]
    }
  }
}
```

This keeps repository boundaries explicit and avoids surprising dynamic repo switching.

Reference points for client-specific formats:

- Anthropic Claude Code MCP setup: `https://docs.anthropic.com/en/docs/claude-code/mcp`
- Anthropic Claude Code SDK `.mcp.json` shape: `https://docs.anthropic.com/en/docs/claude-code/sdk/sdk-mcp`
- VS Code MCP configuration reference: `https://code.visualstudio.com/docs/copilot/reference/mcp-configuration`
- GitHub Copilot CLI MCP setup: `https://docs.github.com/copilot/how-tos/copilot-cli/customize-copilot/add-mcp-servers`
- GitHub Copilot SDK MCP setup: `https://docs.github.com/copilot/how-tos/copilot-sdk/use-copilot-sdk/mcp-servers`

### Codex Local Configuration

For a local Codex agent on this Windows workstation, edit:

```text
C:\Users\you\.codex\config.toml
```

Add:

```toml
[mcp_servers.ctx]
command = "C:\\ctx\\bin\\ctx-mcp.cmd"
args = ["--repo", "C:\\path\\to\\ctx-repo", "--mode", "read-only"]
```

This binds the MCP server name `ctx` to the selected local repository.

To let Codex record CTX artifacts directly through MCP:

```toml
[mcp_servers.ctx]
command = "C:\\ctx\\bin\\ctx-mcp.cmd"
args = ["--repo", "C:\\path\\to\\ctx-repo", "--mode", "write"]
```

Use a different server name when testing another repository:

```toml
[mcp_servers.ctx_almacen_demo]
command = "C:\\ctx\\bin\\ctx-mcp.cmd"
args = ["--repo", "C:\\path\\to\\ctx-repo\\examples\\ctx\\almacen-barrial-reglas", "--mode", "read-only"]
```

After changing `config.toml`, restart the Codex session. MCP servers are loaded at process startup, so an already-running agent session should not be treated as proof that the configuration failed.

## Step 5: Restart The Agent

After editing the agent MCP configuration:

1. Save the config file.
2. Fully restart the agent or IDE.
3. Open the agent's MCP/tool panel if it has one.
4. Confirm the `ctx` server is connected.
5. Confirm the tools listed in this guide appear.

## Step 6: Ask The Agent To Use CTX

Use a prompt like:

```text
Use the CTX MCP server named ctx.

Call ctx_plan with purpose "agent-startup".
Use the returned recommended task, context packet, runbook suggestions, and guidance as the planning anchor.
Do not reconstruct the plan from chat when ctx_plan already provides it.
```

Use lower-level tools only when you need a narrower follow-up after `ctx_plan`.

For graph inspection:

```text
Use CTX through MCP.
Call ctx_graph_summary, then explain the active cognitive graph.
If a specific task is active, reconstruct its thread with ctx_thread_reconstruct.
```

For closeout readiness:

```text
Use CTX through MCP.
Call ctx_preflight with operation git-closeout.
Explain which runbooks or guardrails apply before the next Git commit.
```

## Tool Notes

### `ctx_status`

Reads branch, head, dirty state, counts, and pending cognitive changes.

### `ctx_audit`

Runs the CTX consistency audit.

Use before trusting a repository state.

### `ctx_next`

Returns CTX's next-step recommendation, diagnostics, and runbook suggestions.

### `ctx_context`

Builds a compact context packet.

Useful arguments:

```json
{
  "purpose": "agent-startup",
  "goalId": null,
  "taskId": null
}
```

### `ctx_plan`

Builds a compact planning packet for the next work turn.

This is the preferred first tool for agents because it combines the common startup calls into one packet:

- repository branch, head, and dirty state
- `ctx_next` recommendation and diagnostics
- focused `ctx_context` packet
- applicable runbook suggestions
- short planning guidance

Useful arguments:

```json
{
  "purpose": "agent-startup",
  "goalId": null,
  "taskId": null
}
```

Use `goalId` or `taskId` when the operator has already identified the work line. Otherwise, let CTX rank the next task.

### `ctx_graph_summary`

Summarizes nodes, edges, and graph shape.

### `ctx_graph_show`

Shows a specific node.

Example node ids:

```text
Task:<taskId>
Hypothesis:<hypothesisId>
Decision:<decisionId>
Conclusion:<conclusionId>
```

### `ctx_graph_export` and `ctx_graph_lineage`

Export the graph or inspect a focused semantic lineage.

Useful lineage arguments:

```json
{
  "focusType": "task",
  "focusId": "<taskId>",
  "format": "json"
}
```

### `ctx_log` and `ctx_diff`

Read durable cognitive history and compare working or committed cognitive states.

### `ctx_*_list`, `ctx_*_show`, and `ctx_artifact_*`

Use typed tools for common entities:

```text
ctx_goal_list / ctx_goal_show
ctx_task_list / ctx_task_show
ctx_hypothesis_list / ctx_hypothesis_show / ctx_hypothesis_rank
ctx_evidence_list / ctx_evidence_show
ctx_decision_list / ctx_decision_show
ctx_conclusion_list / ctx_conclusion_show
```

Use `ctx_artifact_list` and `ctx_artifact_show` when a client wants one generic list/show surface.

### `ctx_runbook_list`, `ctx_runbook_show`, `ctx_trigger_list`, and `ctx_trigger_show`

Inspect operational runbooks and cognitive triggers that can explain why a work line exists or which procedure applies.

### `ctx_bootstrap_map`

Builds a provisional cognitive map from existing source material. This is read-only and does not mutate `.ctx`.

### `ctx_thread_reconstruct`

Reconstructs a cognitive thread for a task.

Arguments:

```json
{
  "taskId": "<taskId>",
  "format": "json"
}
```

Use `format: "markdown"` when the agent needs a readable narrative.

### `ctx_preflight`

Returns runbook-aware guidance for an operation.

Example:

```json
{
  "operation": "git-closeout",
  "taskId": "<taskId>"
}
```

### `ctx_operational_review`

Reviews repeated operational issue triggers and recommends runbook updates when a recurrence threshold is reached.

Example:

```json
{
  "operation": "publish-local",
  "threshold": 2
}
```

Use this when a procedure fails in the same way more than once and the fix should become durable playbook/runbook guidance.

### `ctx_check`

Checks whether a task thread has enough hypotheses, evidence, decisions, and conclusions for a coherent cognitive commit.

Arguments:

```json
{
  "taskId": "<taskId>"
}
```

### `ctx_closeout`

Reviews pending cognitive changes against HEAD. This is read-only and should be called before `ctx_commit`.

### Write Tools

These tools require `--mode write`:

```text
ctx_init
ctx_goal_add
ctx_goal_update
ctx_line_open
ctx_task_add
ctx_task_update
ctx_hypothesis_add
ctx_hypothesis_update
ctx_hypothesis_relate
ctx_hypothesis_merge
ctx_hypothesis_supersede
ctx_evidence_add
ctx_evidence_share
ctx_decision_add
ctx_decision_update
ctx_conclusion_add
ctx_conclusion_update
ctx_runbook_add
ctx_trigger_add
ctx_bootstrap_apply
ctx_commit
```

Recommended write flow:

```text
ctx_status
ctx_next
ctx_task_add or ctx_line_open
ctx_hypothesis_add
ctx_evidence_add
ctx_decision_add
ctx_conclusion_add
ctx_task_update
ctx_check
ctx_closeout
ctx_commit
```

The server still uses the existing CTX application-service layer and repository write lock.

`ctx_bootstrap_apply` also requires write mode because it promotes a provisional line into CTX.

Full CLI/MCP parity tracking is documented in [CTX_MCP_TOOL_PARITY.md](CTX_MCP_TOOL_PARITY.md).

## Agent Operating Flow

Use the MCP server as the agent-facing CTX interface. Use the CLI as a human/operator fallback, or for commands that have not been exposed through MCP yet.

The normal agent flow is:

1. Start the MCP server through the client configuration.
2. Re-anchor on the active cognitive context with `ctx_plan`.
3. Use `ctx_status`, `ctx_context`, `ctx_next`, or `ctx_graph_summary` only when you need a narrower follow-up.
4. Open the work block with `ctx_task_add` or `ctx_line_open` when no suitable task already exists.
5. Record the reasoning structure while the work happens:
   - `ctx_hypothesis_add`
   - `ctx_evidence_add`
   - `ctx_decision_add`
   - `ctx_conclusion_add`
6. Mark the task done with `ctx_task_update` only after the implementation or documentation change is complete.
7. Validate the cognitive thread with `ctx_check`, `ctx_closeout`, and `ctx_audit`.
8. Create the durable cognitive commit with `ctx_commit`.
9. Create the Git commit with the changed files, including the generated `.ctx` commit artifact.

The expected closeout has two commits:

- CTX commit: captures why the work happened, what evidence was used, what was decided, and what conclusion closed the thread.
- Git commit: captures the file changes that implement the work.

Do not treat the Git commit as the cognitive source of truth. The Git commit is the implementation snapshot; the CTX commit is the reasoning snapshot.

Recommended prompt for agents:

```text
Use CTX through MCP before answering or editing.
Call ctx_plan first with purpose "agent-startup".
Use the returned task recommendation, context packet, runbooks, and guidance as the planning anchor.
If there is no active task for this work, open one with ctx_task_add or ctx_line_open.
Record hypotheses, evidence, decisions, and conclusions as the work evolves.
Before the final Git commit, call ctx_check, ctx_closeout, ctx_audit, and ctx_commit.
```

## Repository Boundary Rules

The server is started with one repository root:

```text
--repo C:\path\to\ctx-repo
```

Dynamic `repo` arguments are intentionally restricted by default. If you need another repository, configure another MCP server entry with a different name.

Only use `--allow-root <path>` when you intentionally want one server to accept multiple repositories under a known parent directory. Without `--allow-root`, a tool call cannot switch from the configured `--repo` to another `.ctx` repository.

Example:

```json
{
  "mcpServers": {
    "ctx-main": {
      "type": "stdio",
      "command": "C:\\ctx\\bin\\ctx-mcp.cmd",
      "args": ["--repo", "C:\\path\\to\\ctx-repo", "--mode", "read-only"],
      "env": {},
      "tools": ["*"]
    },
    "ctx-demo": {
      "type": "stdio",
      "command": "C:\\ctx\\bin\\ctx-mcp.cmd",
      "args": ["--repo", "C:\\path\\to\\ctx-repo\\examples\\ctx\\almacen-barrial-reglas", "--mode", "read-only"],
      "env": {},
      "tools": ["*"]
    }
  }
}
```

## Troubleshooting

### The Agent Does Not Show CTX Tools

Check:

```powershell
Test-Path C:\ctx\bin\ctx-mcp.cmd
Test-Path C:\ctx\mcp\Ctx.Mcp.exe
```

Then restart the agent completely.

### The Server Says No `.ctx` Repository Was Found

The `--repo` path must point to a folder that contains `.ctx`.

Check:

```powershell
Test-Path C:\path\to\ctx-repo\.ctx
```

### The Agent Uses Chat Instead Of CTX

Use a stricter prompt:

```text
Do not answer from chat memory.
Call ctx_plan through MCP before answering.
If MCP is unavailable, say that instead of guessing.
```

### The Repository Looks Dirty

Ask the agent to call:

```text
ctx_status
ctx_audit
ctx_preflight
```

If `ctx_status` reports `dirty: true`, close the cognitive block with the normal CTX/CLI workflow before committing Git changes.

## Current Limitations

- No HTTP transport.
- No remote auth model.
- Write mode is local-only and should be enabled only for trusted repositories.
- Branch, checkout, merge, import, export, provider run, packet, metrics, and usage telemetry surfaces are intentionally deferred from the MCP baseline.

This is intentional for the MVP. The first purpose is reliable context access and controlled local cognitive writes for agents.

## Reproducible Test Plan

Use this sequence after configuring an agent.

### Test 1: Local files exist

```powershell
Test-Path C:\ctx\bin\ctx-mcp.cmd
Test-Path C:\ctx\mcp\Ctx.Mcp.exe
Test-Path C:\path\to\ctx-repo\.ctx
```

Expected:

```text
True
True
True
```

### Test 2: Agent session exposes MCP

Start a new agent session and ask:

```text
List the MCP resources and tools available in this session. Confirm whether the ctx MCP server is loaded.
```

Expected:

- the session reports the `ctx` MCP server or its tools
- if no MCP tools are visible, restart the agent and re-check the config file

### Test 3: Read CTX plan through MCP

Ask:

```text
Use the ctx MCP server. Call ctx_plan with purpose "smoke-test".
Tell me the repository branch, dirty state, recommended task, context packet id, runbook suggestions, and guidance.
```

Expected:

- branch is reported
- dirty state is reported
- open work is reported from CTX, not from chat memory
- `ctx_plan` returns a compact planning packet that can replace separate startup calls to `ctx_status`, `ctx_next`, `ctx_context`, and runbook inspection

### Test 4: Validate lower-level diagnostics

Ask:

```text
Use the ctx MCP server. Call ctx_status and ctx_audit for the configured repository.
Report branch, head, dirty state, warning count, and error count.
```

Expected:

- head commit is reported
- audit returns without protocol errors
- lower-level status and audit agree with the `ctx_plan` startup packet

### Test 5: Validate write mode on a disposable repository

Use a temporary repository first, not an important production workspace:

```powershell
mkdir C:\path\to\ctx-repo\tmp\mcp-write-smoke
cd C:\path\to\ctx-repo\tmp\mcp-write-smoke
C:\ctx\bin\ctx.cmd init --name "MCP Write Smoke"
```

Configure a separate MCP server entry:

```toml
[mcp_servers.ctx_write_smoke]
command = "C:\\ctx\\bin\\ctx-mcp.cmd"
args = ["--repo", "C:\\path\\to\\ctx-repo\\tmp\\mcp-write-smoke", "--mode", "write"]
```

Restart the agent and ask:

```text
Use the ctx_write_smoke MCP server.
Call ctx_task_add with title "Smoke task".
Call ctx_hypothesis_add linked to that task.
Call ctx_evidence_add supporting that hypothesis.
Call ctx_decision_add and ctx_conclusion_add.
Call ctx_task_update to mark the task Done.
Call ctx_check, ctx_closeout, and then ctx_commit with message "mcp write smoke".
```

Expected:

- each write call succeeds
- `ctx_closeout` shows the pending artifacts before commit
- `ctx_commit` makes `ctx_status.dirty` false

### Test 6: Repository boundary

Ask the same agent to inspect another repository without adding another MCP server entry.

Expected:

- the agent should not silently switch repositories through the `ctx` server
- to inspect a second repository, configure a second MCP server entry with a different name
- dynamic switching requires an explicit `--allow-root` boundary

### Test 7: Viewer MCP Status Indicator

The local viewer exposes a server-side MCP health probe:

```powershell
Invoke-RestMethod "http://127.0.0.1:5271/api/mcp-status?path=C%3A%5Cpath%5Cto%5Cctx-repo&ensure=true"
```

Expected when the local MCP server is installed:

```text
healthy: true
launcherExists: true
executableExists: true
startedByViewer: true
ownedRepositoryPath: C:\path\to\ctx-repo
```

Expected viewer behavior:

- the top bar shows `MCP Server` inside a compact status capsule
- the status dot is green when `healthy` is true
- the viewer may start a read-only MCP process for the active repository when `ensure=true`
- the status dot is red when the launcher or executable is missing, or when MCP startup fails

Failure drill:

1. Stop the installed viewer and any viewer-owned `Ctx.Mcp.exe` process.
2. Refresh the viewer.
3. Confirm `/api/mcp-status?path=<repo>&ensure=true` returns `healthy: true` and `startedByViewer: true`.
4. Temporarily move or remove the installed MCP executable only in a disposable install test.
5. Confirm `/api/mcp-status` reports `healthy: false` and the top bar `MCP Server` dot turns red.

## Next Planned MCP Phases

1. Add optional HTTP transport with auth and repository allowlists.
2. Add richer multi-repository allowlist support.
3. Add guarded branch, checkout, merge, import, and export tools.
4. Add higher-level agent workflows that bundle check, closeout, and commit guidance without hiding the underlying CTX artifacts.
