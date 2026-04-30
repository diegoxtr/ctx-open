# CTX ACP Local Connection Guide
If a language model and its agent lose context, this is the tool you need.

## Purpose

Use this guide to connect a local ACP-style client to CTX through the installed `ctx-agent-acp` adapter.

Current status:

- transport: local stdio
- protocol shape: newline-delimited JSON-RPC
- mode: read-only
- writes: disabled
- filesystem, terminal, Git, release, and public sync: disabled

Use MCP for tool-rich agent integrations today. Use ACP when the client wants a session-oriented flow with `initialize`, `session/new`, and `session/prompt`.

## From This Chat Through MCP

This chat cannot load ACP directly unless the host client adds native ACP support. The practical local path is:

1. Configure Codex with CTX MCP in write mode.
2. Restart the Codex session so the MCP tools are loaded.
3. Call `ctx_acp_session_smoke` from MCP to exercise the ACP-style session flow.

Local Codex config:

```toml
[mcp_servers.ctx]
command = "C:\\ctx\\bin\\ctx-mcp.cmd"
args = ["--repo", "C:\\path\\to\\ctx-repo", "--mode", "write"]
```

Line by line:

- `[mcp_servers.ctx]`: names the local MCP server `ctx`.
- `command`: points Codex to the installed CTX MCP launcher.
- `--repo`: fixes the repository boundary.
- `C:\path\to\ctx-repo`: the CTX repository boundary used by this local adapter.
- `--mode`: selects the MCP permission mode.
- `write`: enables cognitive write tools such as task/evidence/decision/conclusion/commit. Use only for trusted local repos.

The bridge tool is:

```text
ctx_acp_session_smoke
```

It is still read-only at the ACP layer. It returns ACP-shaped payloads for `initialize`, `session/new`, `session/update`, and `session/prompt`, with `mutated: false`.

## Installed Command

Windows:

```powershell
C:\ctx\bin\ctx-agent-acp.cmd --repo C:\path\to\ctx-repo
```

Direct executable:

```powershell
C:\ctx\acp\Ctx.Agent.Acp.exe --repo C:\path\to\ctx-repo
```

Linux/macOS:

```bash
$HOME/.local/share/ctx/bin/ctx-agent-acp --repo "$HOME/sources/ctx-repo"
```

## Command Line, Line By Line

```powershell
C:\ctx\bin\ctx-agent-acp.cmd
```

Starts the installed ACP launcher. The launcher is a thin wrapper around `C:\ctx\acp\Ctx.Agent.Acp.exe`.

```powershell
--repo
```

Tells the adapter which CTX repository is allowed for this process. This is a safety boundary.

```powershell
C:\path\to\ctx-repo
```

The repository root that contains `.ctx`. Use the repository you intentionally want this adapter to read.

## Message 1: initialize

Client sends:

```json
{"jsonrpc":"2.0","id":0,"method":"initialize","params":{"protocolVersion":1,"clientCapabilities":{}}}
```

Line by line:

- `jsonrpc`: JSON-RPC protocol marker.
- `id`: request id; the response repeats it.
- `method`: `initialize` starts the capability exchange.
- `protocolVersion`: ACP protocol version requested by the client.
- `clientCapabilities`: what the client can do. Empty is valid for the current read-only smoke test.

The important response field is `_meta.ctx`: it declares the adapter is read-only and cannot edit files, run terminal commands, mutate Git, publish releases, or sync public repositories.

## Message 2: session/new

Client sends:

```json
{"jsonrpc":"2.0","id":1,"method":"session/new","params":{"cwd":"C:\\path\\to\\ctx-repo","mcpServers":[]}}
```

Line by line:

- `method`: `session/new` creates an ACP session.
- `cwd`: the working directory requested by the client. It must match the configured `--repo` unless the adapter is explicitly changed later.
- `mcpServers`: ACP allows clients to describe MCP servers available to the session. Empty is valid here.

Save `sessionId` from the response. The next prompt must send that id back.

## Message 3: session/prompt

Client sends:

```json
{"jsonrpc":"2.0","id":2,"method":"session/prompt","params":{"sessionId":"<session-id>","prompt":[{"type":"text","text":"What is the next safe planning step from CTX?"}],"_meta":{"ctx":{"repositoryPath":"C:\\path\\to\\ctx-repo"}}}}
```

Line by line:

- `method`: `session/prompt` sends one user turn to the ACP session.
- `sessionId`: the id returned by `session/new`.
- `prompt`: an array of content blocks. Phase 1 supports `text` and `resource_link`.
- `type`: `text` marks this block as plain text.
- `text`: the user prompt.
- `_meta.ctx.repositoryPath`: explicit repository path used by the CTX agent layer. Phase 1 requires it for prompt turns.

Expected first response:

```json
{"jsonrpc":"2.0","method":"session/update","params":{"sessionId":"<session-id>","update":{"sessionUpdate":"plan","entries":[]}}}
```

This is the streamed planning update. Entries describe the next CTX step, runbooks, and guidance.

Expected second response:

```json
{"jsonrpc":"2.0","id":2,"result":{"stopReason":"end_turn","_meta":{"ctx":{"mutated":false,"lastPlanPacketId":"<ctx-plan-packet-id>"}}}}
```

`mutated: false` is the key read-only guarantee.

## Minimal Client Shape

An ACP client must:

1. Start the process with `ctx-agent-acp --repo <repo>`.
2. Write one compact JSON-RPC object per line to stdin.
3. Read one compact JSON-RPC object per line from stdout.
4. Keep the process open while the session is active.
5. Reuse the `sessionId` returned by `session/new`.
6. Include `_meta.ctx.repositoryPath` in `session/prompt` during Phase 1.

## Common Mistakes

- Sending `prompt` as a string instead of an array.
- Forgetting `_meta.ctx.repositoryPath` on `session/prompt`.
- Using a `sessionId` that was not returned by the active process.
- Piping from PowerShell in a way that adds a UTF-8 BOM. If that happens, the adapter returns a parse error for the first line.
- Expecting writes. ACP Phase 1 is read-only.

## Automated Test

```powershell
dotnet test .\Ctx.Tests\Ctx.Tests.csproj --no-restore --filter LocalConnectionFlow_InitializeSessionPromptReturnsReadOnlyPlan
```

The test validates:

- `initialize`
- `session/new`
- `session/prompt`
- `session/update`
- `end_turn`
- `mutated: false`

The MCP bridge test is:

```powershell
dotnet test .\Ctx.Tests\Ctx.Tests.csproj --no-restore --filter AcpSessionSmoke_ReturnsReadOnlySessionShapeWithoutMutating
```
