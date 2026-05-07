# CTX Technical Architecture
If a language model and its agent lose context, this is the tool you need.

This document describes the current CTX technical architecture.

Goals:

- how the solution is organized
- responsibilities of each layer
- how components connect
- how a CLI operation flows to persistence and providers

CTX is built on `.NET 8` and follows a modular structure inspired by Clean Architecture and DDD.

## Overview

Solution projects:

- `Ctx.Domain`
- `Ctx.Application`
- `Ctx.Core`
- `Ctx.Persistence`
- `Ctx.Providers`
- `Ctx.Infrastructure`
- `Ctx.Cli`
- `Ctx.Viewer`
- `Ctx.Mcp`
- `Ctx.Tests`

Main rule:

- domain defines language and types
- application defines contracts and use cases
- core implements critical logic
- persistence implements local storage
- providers implement LLM integrations
- infrastructure wires dependencies
- CLI exposes the interface
- viewer exposes local timeline/graph inspection
- MCP exposes CTX to MCP-capable agents over local stdio

## Layer map

```text
CLI / Viewer / MCP
  -> Application
    -> Core
      -> Domain
    -> Persistence
    -> Providers
  -> Infrastructure
```

Conceptual view:

- `Ctx.Domain` depends on nothing
- `Ctx.Application` depends on Domain and exposes abstractions
- `Ctx.Core` implements Application interfaces using Domain
- `Ctx.Persistence` implements filesystem repositories
- `Ctx.Providers` implements interchangeable providers
- `Ctx.Infrastructure` composes concrete implementations
- `Ctx.Cli` consumes `ICtxApplicationService`
- `Ctx.Viewer` consumes the same persisted `.ctx` model for local inspection APIs and static UI
- `Ctx.Mcp` consumes `ICtxApplicationService` through MCP tools

## 1. Domain layer

Files:
- [Model.cs](../Ctx.Domain/Model.cs)
- [Identifiers.cs](../Ctx.Domain/Identifiers.cs)
- [Enums.cs](../Ctx.Domain/Enums.cs)

Responsibility:
- define domain model
- strong IDs
- lifecycle states
- diff/merge/metrics/export artifacts

Principle:
- this layer knows nothing about CLI, files, HTTP, or concrete providers

## 2. Application layer

Files:
- [ICtxApplicationService.cs](../Ctx.Application/ICtxApplicationService.cs)

Responsibility:
- define use-case contracts
- requests/responses
- repository interfaces
- provider abstractions

Key interfaces:
- `ICtxApplicationService`
- `IWorkingContextRepository`
- `ICommitRepository`
- `IBranchRepository`
- `IRunRepository`
- `IPacketRepository`
- `IMetricsRepository`
- `IAIProvider`
- `IAIProviderRegistry`
- `IContextBuilder`
- `IRunOrchestrator`
- `ICommitEngine`
- `IDiffEngine`
- `IMergeEngine`

## 3. Core layer

Files:
- [CtxApplicationService.cs](../Ctx.Core/CtxApplicationService.cs)
- [ContextBuilder.cs](../Ctx.Core/ContextBuilder.cs)
- [RunOrchestrator.cs](../Ctx.Core/RunOrchestrator.cs)
- [CommitEngine.cs](../Ctx.Core/CommitEngine.cs)
- [DiffEngine.cs](../Ctx.Core/DiffEngine.cs)
- [MergeEngine.cs](../Ctx.Core/MergeEngine.cs)

Responsibility:
- implement core product logic
- coordinate repositories and engines
- convert commands into persisted domain operations

## 4. Persistence layer

Responsibility:
- persist local cognitive repository
- manage `.ctx/` structure
- read/write JSON
- encapsulate filesystem paths and serialization

Main implementations:
- `FileSystemWorkingContextRepository`
- `FileSystemCommitRepository`
- `FileSystemBranchRepository`
- `FileSystemRunRepository`
- `FileSystemPacketRepository`
- `FileSystemMetricsRepository`

## 5. Providers layer

Responsibility:
- abstract LLM execution
- keep providers interchangeable
- encapsulate HTTP/auth/response parsing

Components:
- `AIProviderRegistry`
- `HttpAiProviderBase`
- `OpenAiProvider`
- `AnthropicProvider`

Note:
- missing credentials trigger deterministic offline fallback

## 6. Infrastructure layer

Responsibility:
- composition root
- instantiate concrete implementations
- wire dependencies
- set JSON options for CLI output

## 7. CLI layer

Responsibility:
- parse arguments
- map to application requests
- serialize `CommandResult`
- output structured JSON

Note:
- CLI contains minimal business logic

## 8. Viewer layer

Responsibility:
- serve the local timeline and cognitive graph UI
- expose local HTTP APIs for repository overview, graph snapshots, history paging, runbooks, origins, and MCP health
- keep graph/history rendering separate from CTX domain mutation logic

Note:
- viewer is an inspector, not an editor
- local installs run from `C:\ctx\viewer` and display `local-version` in the header

## 9. MCP layer

Responsibility:
- expose CTX tools to MCP-capable agents over stdio
- reuse `ICtxApplicationService` instead of duplicating CLI/domain logic
- run `read-only` by default and require `--mode write` for mutation tools
- enforce repository boundary checks for configured roots

Main tools:
- status, audit, next, context, graph summary/show, thread reconstruct, preflight, check, closeout
- version, doctor, log, diff, graph export/lineage, typed artifact list/show, hypothesis rank, runbook list/show, and trigger list/show
- read-only bootstrap map for provisional cognitive indexing demos
- write-mode init, goal, line, task, hypothesis, evidence, decision, conclusion, runbook, trigger, bootstrap apply, and commit operations

## 10. Tests

Tests cover:

- core engines
- critical use cases
- portability, doctor, export/import, CLI summaries
- MCP read-only/write-mode guardrails
- viewer history and graph behavior where covered by unit tests

## End-to-end flows (summary)

- `ctx init`: create base repository structure
- `ctx goal add`: update working context and graph
- `ctx context`: build a `ContextPacket`
- `ctx run`: execute provider, persist run + metrics
- `ctx commit`: generate immutable snapshot and diff
- `ctx diff`: compare commits or working state
- `ctx merge`: integrate branches with cognitive conflicts

## Key architecture decisions

- local filesystem persistence for simplicity and portability
- JSON as primary format
- CLI-first interface for automation
- MCP as an adapter over the same application service, not a parallel product core
- provider abstraction for portability
- specialized engines in Core to keep logic focused

## Current limits

- manual wiring in Infrastructure
- handcrafted CLI parsing
- local persistence only
- no guided conflict resolution
- no background processing
- limited concurrency control
- MCP has no HTTP transport or remote auth model yet
- MCP branch/checkout/merge, import/export, provider run, packet, metrics, and usage telemetry tools are intentionally deferred

## Related references

- [DOMAIN_MODEL.md](DOMAIN_MODEL.md)
- [CTX_STRUCTURE.md](CTX_STRUCTURE.md)
- [CLI_COMMANDS.md](CLI_COMMANDS.md)
- [V1_FUNCTIONAL_SPEC.md](V1_FUNCTIONAL_SPEC.md)
