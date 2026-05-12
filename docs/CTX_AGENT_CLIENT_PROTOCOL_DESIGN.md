# CTX Agent Client Protocol Design

Status: public design plus Phase 1 internal and ACP adapter prototype

This document designs an agent-facing layer for CTX that can later be exposed through Agent Client Protocol style clients without binding CTX cognitive semantics to one transport.

## Problem

CTX already exposes a strong local operation surface through CLI and MCP. MCP is the correct protocol for tools, resources, and prompts, but it is not a full session model for interactive agent operation.

Editors, IDEs, and coding-agent clients increasingly need a session-oriented endpoint that can:

- initialize an agent capability surface
- create and resume work sessions
- accept user prompts as turns
- stream progress and structured updates
- ask for permission before risky actions
- preserve the work as cognitive state instead of chat residue

The design goal is not to replace MCP. The goal is to add an internal CTX Agent layer and expose it through adapters, with ACP as the first likely session adapter.

## What Exists Today

The current codebase already has most of the primitives needed for an agent layer:

- `Ctx.Application.ICtxApplicationService` is the stable application facade.
- `Ctx.Core.CtxApplicationService` implements the cognitive operations over repositories.
- `Ctx.Infrastructure.Bootstrapper` wires persistence, core services, provider execution, metrics, runbooks, triggers, and the application facade.
- `Ctx.Mcp` is a thin local stdio adapter over the application facade.
- `Ctx.Mcp.Mcp.RepositoryGuard` enforces configured repository boundaries and read-only/write modes for MCP.
- `ctx plan` and MCP `ctx_plan` already produce a compact planning packet with next work, focused context, runbook suggestions, and guidance.
- Cognitive triggers already provide a durable way to preserve important user/operator prompts.
- `ctx check`, `ctx closeout`, `ctx preflight`, `ctx operational review`, and `ctx commit` provide the closure and governance operations needed for disciplined agent work.
- `Ctx.Agent` now provides the first internal read-only session layer over `ICtxApplicationService`.
- `Ctx.Agent.Acp` now provides a first newline-delimited stdio JSON-RPC adapter over `Ctx.Agent`.

The current gap is explicit agent session state. CTX has repositories, tasks, runbooks, triggers, plans, and provider runs, but it does not yet have a first-class "agent session" abstraction for client-driven turns.

The first prototype keeps session state in memory. Durable session persistence belongs to a later phase after the internal contracts stabilize.

## Design Principle

Build `Ctx.Agent` first, then expose it through protocol adapters.

The internal layer should speak CTX concepts:

- repositories
- sessions
- prompts
- turns
- plans
- permissions
- cognitive triggers
- work-block closeout
- handoff packets

Protocol adapters should translate external messages into that model. ACP, MCP, CLI, viewer, or future HTTP adapters should not own the cognitive semantics.

## Proposed Projects

### `Ctx.Agent`

Internal agent orchestration library.

Responsibilities:

- create and resume agent sessions
- map a prompt turn to CTX planning/context/runbook surfaces
- optionally persist significant prompts as cognitive triggers
- decide whether a turn is read-only, cognitive-write, or execution-oriented
- emit structured updates for adapters
- enforce permission boundaries before write, filesystem, terminal, Git, release, or public-sync operations
- prepare closeout and handoff state

Dependencies:

- `Ctx.Application`
- `Ctx.Domain`

It should not depend on ACP, MCP, JSON-RPC, stdio, web transport, or editor-specific concepts.

### `Ctx.Agent.Acp`

ACP-style stdio adapter over `Ctx.Agent`.

Responsibilities:

- JSON-RPC transport
- protocol initialize/capability exchange
- session lifecycle message translation
- prompt/update streaming translation
- permission request/response translation
- cancellation handling

Current Phase 1 behavior:

- supports `initialize`
- supports `session/new`
- supports `session/prompt`
- accepts `session/cancel` as a no-op notification
- emits `session/update` plan notifications before `session/prompt` responses
- advertises no filesystem, terminal, HTTP MCP, SSE MCP, session load, or cognitive-write capability

Dependencies:

- `Ctx.Agent`
- `Ctx.Infrastructure`

### Optional Future Adapters

- `Ctx.Agent.Http` for local HTTP/WebSocket clients.
- `Ctx.Viewer` integration for visual session inspection.
- MCP resources that expose agent-session snapshots without turning MCP into the session transport.

## Agent Layer Contracts

The first internal contract can stay small:

```csharp
public interface ICtxAgentService
{
    Task<AgentSessionSummary> StartSessionAsync(StartAgentSessionRequest request, CancellationToken cancellationToken);
    Task<AgentTurnPlan> PlanTurnAsync(PlanAgentTurnRequest request, CancellationToken cancellationToken);
    Task<AgentTurnResult> ReceivePromptAsync(ReceiveAgentPromptRequest request, CancellationToken cancellationToken);
    Task<AgentCloseoutSummary> CloseWorkBlockAsync(CloseAgentWorkBlockRequest request, CancellationToken cancellationToken);
    Task<AgentHandoffPacket> PrepareHandoffAsync(PrepareAgentHandoffRequest request, CancellationToken cancellationToken);
}
```

Write-capable operations should be explicit and permission-gated. The first prototype should not include unrestricted file editing, terminal execution, Git operations, release operations, or public-sync operations.

## Session State

A CTX agent session should be durable enough to resume work without storing raw chat as the system of record.

Suggested fields:

- `sessionId`
- `repositoryPath`
- `mode`: `read-only`, `cognitive-write`, or `execution`
- `createdBy`
- `createdAt`
- `updatedAt`
- `activeGoalId`
- `activeTaskId`
- `lastPlanPacketId`
- `linkedTriggerIds`
- `permissionProfile`
- `state`: `active`, `waiting-for-permission`, `closing`, `closed`
- `summary`

Raw transcript storage should remain optional and secondary. The canonical memory should be structured CTX artifacts.

## ACP Mapping

The adapter should map ACP-style messages into CTX Agent operations:

| ACP concept | CTX mapping |
|---|---|
| `initialize` | return server info, protocol support, permission model, and capabilities |
| `session/new` | create an agent session and return initial `ctx_plan`/context guidance |
| `session/prompt` | process a prompt turn through `ReceivePromptAsync` |
| `session/update` | stream plan, status, evidence, decision, closeout, or permission updates |
| cancellation | cancel the active turn without deleting CTX artifacts already written |
| permission request | ask before cognitive writes, filesystem writes, terminal commands, Git, release, or public sync |
| filesystem access | deferred until execution phase |
| terminal access | deferred until execution phase |

This keeps ACP as a client protocol while CTX remains the cognitive state authority.

## Permissions

The permission model should be stricter than simple read/write:

- `read`: status, plan, context, graph, artifact list/show, runbook lookup
- `cognitive-write`: trigger, evidence, decision, conclusion, task/goal updates, CTX commit
- `filesystem-write`: code/docs edits
- `terminal`: shell commands
- `git`: branch, commit, tag, push
- `release`: version bump, packaging, release notes, publish
- `public-sync`: any operation touching `ctx-public` or `ctx-open`

Default prototype mode should be `read-only`. The adapter must never infer publication permission from local work.

## Phases

### Phase 0: Design

- Document architecture and boundary decisions.
- Inventory existing service surfaces.
- Define acceptance criteria for the first prototype.

### Phase 1: Read-Only Agent Session

- Add `Ctx.Agent` with session creation and prompt planning.
- Add `Ctx.Agent.Acp` with initialize, session/new, and session/prompt.
- Return `ctx_plan`, focused context, and runbook suggestions.
- No cognitive writes by default.
- No filesystem, terminal, Git, release, or public-sync permissions.

Current implementation status:

- `Ctx.Agent` exists as an internal library.
- `ICtxAgentService` exposes start session, plan turn, receive prompt, closeout, and handoff methods.
- `CtxAgentService` uses `ICtxApplicationService.PlanAsync`, `ContextAsync`, `CheckAsync`, and `CloseoutAsync`.
- `Bootstrapper.Create()` wires `ICtxAgentService` into `CtxRuntime`.
- read-only prompt turns return planning packets without mutating CTX state.
- read-only sessions reject trigger persistence.
- local publish, install, and portable distribution expose the ACP adapter through a dedicated `ctx-agent-acp` launcher and `acp/` install directory.

Still pending for full Phase 1:

- richer protocol-facing permission payloads

### Phase 2: Controlled Cognitive Writes

- Allow prompt turns to create cognitive triggers, evidence, decisions, conclusions, and closeout summaries only after permission approval.
- Add session-to-artifact linkage.
- Add tests for permission rejection and mode enforcement.

### Phase 3: Execution Integration

- Add controlled file and terminal execution.
- Integrate Git and release playbooks.
- Emit richer streaming updates.
- Surface sessions in the viewer.

## Prototype Acceptance Criteria

The first implementation should pass these checks:

- `dotnet build Ctx.sln --no-restore`
- read-only ACP smoke session returns an initial CTX planning packet
- read-only mode rejects all cognitive writes
- session prompt can produce a focused plan without mutating `.ctx`
- write mode can persist a cognitive trigger only when explicitly requested and permitted
- repository guard prevents dynamic repo access outside configured roots

## Risks

- ACP terminology is still easy to confuse with MCP or other agent protocols; docs must define the boundary clearly.
- Directly binding CTX to ACP would make future clients harder to support.
- Storing raw chat as memory would weaken the product thesis; structured artifacts must remain canonical.
- Permission prompts must be conservative, especially around Git, release, and public sync.
- The agent layer should not duplicate the provider-run orchestration already in `RunOrchestrator`.

## Recommendation

Implement `Ctx.Agent` as the internal model and keep `Ctx.Agent.Acp` as a thin adapter.

Start with Phase 1 read-only sessions. That gives agents a useful session surface immediately, proves the protocol shape, and avoids introducing risky execution permissions before the cognitive flow is stable.
