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
- `Ctx.Tests`

Main rule:

- domain defines language and types
- application defines contracts and use cases
- core implements critical logic
- persistence implements local storage
- providers implement LLM integrations
- infrastructure wires dependencies
- CLI exposes the interface

## Layer map

```text
CLI
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

## 1. Domain layer

Files:
- [Model.cs](C:/sources/ctx/Ctx.Domain/Model.cs)
- [Identifiers.cs](C:/sources/ctx/Ctx.Domain/Identifiers.cs)
- [Enums.cs](C:/sources/ctx/Ctx.Domain/Enums.cs)

Responsibility:
- define domain model
- strong IDs
- lifecycle states
- diff/merge/metrics/export artifacts

Principle:
- this layer knows nothing about CLI, files, HTTP, or concrete providers

## 2. Application layer

Files:
- [ICtxApplicationService.cs](C:/sources/ctx/Ctx.Application/ICtxApplicationService.cs)

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
- [CtxApplicationService.cs](C:/sources/ctx/Ctx.Core/CtxApplicationService.cs)
- [ContextBuilder.cs](C:/sources/ctx/Ctx.Core/ContextBuilder.cs)
- [RunOrchestrator.cs](C:/sources/ctx/Ctx.Core/RunOrchestrator.cs)
- [CommitEngine.cs](C:/sources/ctx/Ctx.Core/CommitEngine.cs)
- [DiffEngine.cs](C:/sources/ctx/Ctx.Core/DiffEngine.cs)
- [MergeEngine.cs](C:/sources/ctx/Ctx.Core/MergeEngine.cs)

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

## 8. Tests

Tests cover:

- core engines
- critical use cases
- portability, doctor, export/import, CLI summaries

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
- provider abstraction for portability
- specialized engines in Core to keep logic focused

## Current limits

- manual wiring in Infrastructure
- handcrafted CLI parsing
- local persistence only
- no guided conflict resolution
- no background processing
- limited concurrency control

## Related references

- [DOMAIN_MODEL.md](C:/sources/ctx-public/docs/DOMAIN_MODEL.md)
- [CTX_STRUCTURE.md](C:/sources/ctx-public/docs/CTX_STRUCTURE.md)
- [CLI_COMMANDS.md](C:/sources/ctx-public/docs/CLI_COMMANDS.md)
- [V1_FUNCTIONAL_SPEC.md](C:/sources/ctx-public/docs/V1_FUNCTIONAL_SPEC.md)

