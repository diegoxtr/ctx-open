# CTX Domain Model
If a language model and its agent lose context, this is the tool you need.

This document describes the current CTX domain model.

Its goal is to make explicit:

- system entities
- strong identifiers
- relationships
- lifecycle states
- traceability rules
- relevant aggregates and operational artifacts

CTX does not model conversations as the primary source. It models structured cognitive artifacts.

## Model principles

The model follows these rules:

- all important information must be structured
- each relevant artifact must have strong identity
- decisions must be explicit
- evidence must be referenceable
- commits must be reproducible
- reasoning evolution must be comparable
- working state and history must be separated

## Strong identifiers

Each primary entity uses a typed ID implemented as a `record struct`.

Current types:

- `ProjectId`
- `GoalId`
- `TaskId`
- `HypothesisId`
- `DecisionId`
- `EvidenceId`
- `ConclusionId`
- `OperationalRunbookId`
- `RunId`
- `ContextCommitId`
- `ContextPacketId`
- `WorkingContextId`

Traits:

- wrap a string value
- generated as compact GUIDs
- avoid mixing entity types by mistake

Example:

```csharp
public readonly record struct GoalId(string Value);
```

## Shared entity base

Traceable domain entities conceptually inherit from:

```csharp
public abstract record CognitiveEntity<TId>(TId Id, Traceability Trace);
```

This enforces:

- strong identity
- required traceability

## Traceability

Traceability is mandatory for core cognitive entities.

Structure:

- `CreatedBy`
- `CreatedAtUtc`
- `UpdatedBy`
- `UpdatedAtUtc`
- `Tags`
- `RelatedIds`

Purpose:

- know who created an artifact
- know when it was created or updated
- track conceptual tags
- link secondary related IDs

## Core entities

### Project

Root cognitive project.

Fields:
- `Id`
- `Name`
- `Description`
- `DefaultBranch`
- `State`
- `Trace`

State:
- `LifecycleState`

Responsibility:
- define repo identity
- set default branch
- act as conceptual root

### Goal

Explicit goal of cognitive work.

Fields:
- `Id`
- `Title`
- `Description`
- `Priority`
- `State`
- `Trace`
- `TaskIds`

State:
- `LifecycleState`

Relations:
- a `Goal` groups many `Task`

Responsibility:
- express high-level intent
- organize linked tasks

### Task

Concrete unit of cognitive work.

Fields:
- `Id`
- `GoalId`
- `Title`
- `Description`
- `State`
- `Trace`
- `HypothesisIds`

State:
- `TaskExecutionState`

Values:
- `Draft`
- `Ready`
- `InProgress`
- `Blocked`
- `Done`

Relations:
- can belong to a `Goal`
- can link many `Hypothesis`

Responsibility:
- model a specific work front

### Hypothesis

An assumption or proposition to evaluate.

Fields:
- `Id`
- `Statement`
- `Rationale`
- `Confidence`
- `State`
- `Trace`
- `TaskIds`
- `EvidenceIds`

State:
- `HypothesisState`

Values:
- `Proposed`
- `UnderEvaluation`
- `Supported`
- `Refuted`
- `Archived`

Relations:
- links to one or more `Task`
- supported by `Evidence`

Responsibility:
- make evaluable ideas explicit
- avoid implicit reasoning

### Decision

Explicit decision in reasoning.

Fields:
- `Id`
- `Title`
- `Rationale`
- `State`
- `Trace`
- `HypothesisIds`
- `EvidenceIds`

State:
- `DecisionState`

Values:
- `Proposed`
- `Accepted`
- `Rejected`
- `Superseded`

Relations:
- references `Hypothesis`
- references `Evidence`

Responsibility:
- record explicit choices
- link a decision to its basis

### OperationalRunbook

Compact reusable operational memory.

Fields:
- `Id`
- `Title`
- `Kind`
- `Triggers`
- `WhenToUse`
- `Do`
- `Verify`
- `References`
- `GoalIds`
- `TaskIds`
- `State`
- `Trace`

State:
- `LifecycleState`

Relations:
- can scope to `Goal`
- can scope to `Task`
- can enter `ContextPacket`
- is versioned in repository snapshots

Responsibility:
- preserve recurring procedures, guardrails, and troubleshooting without mixing them into mutable working execution state

### CognitiveTrigger

Compact typed origin for a cognitive line.

Fields:
- `Id`
- `Kind`
- `Summary`
- `Text`
- `Fingerprint`
- `GoalIds`
- `TaskIds`
- `OperationalRunbookIds`
- `State`
- `Trace`

Relations:
- can scope to `Goal`
- can scope to `Task`
- can reference `OperationalRunbook`
- can enter `ContextPacket`
- is versioned in repository snapshots

Responsibility:
- preserve what materially opened or redirected a cognitive line without storing full prompt history as the default model

### Evidence

Traceable evidence supporting or contradicting artifacts.

Fields:
- `Id`
- `Title`
- `Summary`
- `Source`
- `Kind`
- `Confidence`
- `State`
- `Trace`
- `Supports`

State:
- `LifecycleState`

Kinds:
- `Observation`
- `Benchmark`
- `Document`
- `Experiment`
- `ProviderOutput`

Relations:
- `Supports` contains `EntityReference`
- can point to `Hypothesis`, `Decision`, or `Task`

Responsibility:
- record verifiable support
- prevent decisions without explicit grounding

### Conclusion

Consolidated conclusion.

Fields:
- `Id`
- `Summary`
- `State`
- `Trace`
- `DecisionIds`
- `EvidenceIds`

State:
- `ConclusionState`

Values:
- `Draft`
- `Accepted`
- `Superseded`

Relations:
- references `Decision`
- references `Evidence`

Responsibility:
- compress a reasoned outcome
- close a work line explicitly

### Run

An AI execution over a `ContextPacket`.

Fields:
- `Id`
- `Provider`
- `Model`
- `State`
- `StartedAtUtc`
- `CompletedAtUtc`
- `PacketId`
- `Usage`
- `PromptFingerprint`
- `Summary`
- `Artifacts`
- `Trace`

State:
- `RunState`

Values:
- `Planned`
- `Running`
- `Completed`
- `Failed`

Relations:
- references a `ContextPacket`
- produces `RunArtifact`

Responsibility:
- record structured AI interaction
- measure cost, tokens, duration
- capture useful output for context evolution

### ContextPacket

Optimized context packet for a run.

Fields:
- `Id`
- `ProjectId`
- `CreatedAtUtc`
- `Purpose`
- `Fingerprint`
- `EstimatedTokens`
- `GoalIds`
- `TaskIds`
- `HypothesisIds`
- `DecisionIds`
- `EvidenceIds`
- `ConclusionIds`
- `Sections`

Responsibility:
- select relevant information
- reduce redundancy
- trace what was sent to a provider

Notes:
- does not inherit `CognitiveEntity<TId>` in current implementation
- persisted structured artifact

### WorkingContext

Current mutable repository state.

Fields:
- `Id`
- `RepositoryVersion`
- `CurrentBranch`
- `HeadCommitId`
- `Dirty`
- `Project`
- `Goals`
- `Tasks`
- `Hypotheses`
- `Decisions`
- `Evidence`
- `Conclusions`
- `Runs`
- `Trace`

Responsibility:
- centralize in-progress state
- base for `status`, `context`, `run`, and `commit`
- rebuild the current cognitive graph
- intentionally excludes stable `OperationalRunbook` state

Relevant method:

- `ToGraph()` builds a `ContextGraph`

### RepositorySnapshot

Versioned repository-level snapshot used by `ContextCommit`.

Fields:
- `WorkingContext`
- `Runbooks`

Responsibility:
- keep active cognitive state and stable operational memory separate
- version both together in commits, diffs, merges, export, and import

### ContextCommit

Immutable snapshot of repository state.

Fields:
- `Id`
- `Branch`
- `Message`
- `ParentIds`
- `CreatedAtUtc`
- `SnapshotHash`
- `Diff`
- `Snapshot`
- `Trace`

Responsibility:
- persist reproducible history
- capture diff for the snapshot
- enable `log`, `diff`, `branching`, `merge`

Snapshot contents:
- `Snapshot.WorkingContext`
- `Snapshot.Runbooks`

Rule:
- once persisted, it is immutable

## Support artifacts

### ContextGraph

Projection of the full cognitive state.

Contains:
- `Project`
- `Goals`
- `Tasks`
- `Hypotheses`
- `Decisions`
- `Evidence`
- `Conclusions`
- `Runs`

Responsibility:
- represent the domain graph compactly

### RunArtifact

Artifact produced by a run.

Fields:
- `ArtifactType`
- `Title`
- `Content`
- `References`

Responsibility:
- extract structured outputs from providers

### TokenUsage

Execution usage model.

Fields:
- `InputTokens`
- `OutputTokens`
- `AcuCost`
- `Duration`

Derived:
- `TotalTokens`

### ContentSection

Structured textual section inside a `ContextPacket`.

Fields:
- `Title`
- `Content`
- `References`

### EntityReference

Lightweight reference to another domain entity.

Fields:
- `EntityType`
- `EntityId`

Use:
- evidence supports
- run artifacts
- context sections

## Cognitive diff and merge

### ContextDiffChange

Represents a detected change between states.

Fields:
- `ChangeType`
- `EntityType`
- `EntityId`
- `Summary`

### CognitiveConflict

Explicit cognitive conflict.

Fields:
- `EntityType`
- `EntityId`
- `ConflictType`
- `CurrentSummary`
- `IncomingSummary`

Responsibility:
- explain semantic divergence

### ContextDiff

Groups changes by entity type.

Fields:
- `FromCommitId`
- `ToCommitId`
- `Decisions`
- `Hypotheses`
- `Evidence`
- `Tasks`
- `Conclusions`
- `Conflicts`
- `Summary`

Responsibility:
- inspect reasoning evolution

### MergeResult

Result of a merge operation.

Fields:
- `MergedContext`
- `Conflicts`
- `AutoMerged`
- `Summary`

Responsibility:
- express integration outcome between cognitive branches

## Configuration and repository

### ProviderConfiguration

Fields:
- `Name`
- `DefaultModel`
- `Endpoint`
- `Enabled`

### RepositoryConfig

Fields:
- `DefaultProvider`
- `Providers`
- `PacketTokenLimit`
- `TrackMetrics`

### RepositoryVersion

Fields:
- `CurrentVersion`
- `InitializedAtUtc`

### HeadReference

Fields:
- `Branch`
- `CommitId`

### BranchReference

Fields:
- `Name`
- `CommitId`
- `UpdatedAtUtc`

## Observability and portability

### MetricsSnapshot

Fields:
- `TotalRuns`
- `TotalTokens`
- `TotalAcuCost`
- `RepeatedIterations`
- `AvoidedRedundancyCount`
- `TotalExecutionTime`

### DoctorCheck

Fields:
- `Name`
- `Status`
- `Detail`

### DoctorReport

Fields:
- `ProductVersion`
- `WorkingDirectory`
- `RepositoryDetected`
- `Checks`

### RepositoryExport

Fields:
- `ProductVersion`
- `RepositoryVersion`
- `Config`
- `Head`
- `WorkingContext`
- `Metrics`
- `Branches`
- `Commits`

## Key relationships

- `Project` contains the repository context
- `Goal` groups `Task`
- `Task` references optional `Goal`
- `Task` references `Hypothesis`
- `Hypothesis` references `Task`
- `Hypothesis` references `Evidence`
- `Decision` references `Hypothesis`
- `Decision` references `Evidence`
- `Conclusion` references `Decision`
- `Conclusion` references `Evidence`
- `Run` references `ContextPacket`
- `WorkingContext` aggregates active entities
- `ContextCommit` encapsulates a `WorkingContext` snapshot

## Lifecycle states

### `LifecycleState`

Used by:
- `Project`
- `Goal`
- `Evidence`

Values:
- `Draft`
- `Active`
- `Validated`
- `Completed`
- `Superseded`
- `Archived`

### `TaskExecutionState`

Used by:
- `Task`

Values:
- `Draft`
- `Ready`
- `InProgress`
- `Blocked`
- `Done`

### `HypothesisState`

Used by:
- `Hypothesis`

Values:
- `Proposed`
- `UnderEvaluation`
- `Supported`
- `Refuted`
- `Archived`

### `DecisionState`

Used by:
- `Decision`

Values:
- `Proposed`
- `Accepted`
- `Rejected`
- `Superseded`

### `ConclusionState`

Used by:
- `Conclusion`

Values:
- `Draft`
- `Accepted`
- `Superseded`

### `RunState`

Used by:
- `Run`

Values:
- `Planned`
- `Running`
- `Completed`
- `Failed`

## Operational aggregates

Conceptual aggregates:

- `WorkingContext` as the operational aggregate
- `ContextCommit` as the immutable historical aggregate
- `Run` as the execution aggregate
- `ContextPacket` as the derived context artifact

## Integrity rules

- referenced IDs must exist
- `Decision` must not reference missing hypotheses or evidence
- `Conclusion` must not reference missing decisions or evidence
- `Evidence.Supports` must use valid references
- `Run.PacketId` must point to a persisted packet
- `WorkingContext.HeadCommitId` must be consistent with `HEAD`
- `ContextCommit.SnapshotHash` must match the persisted snapshot

## Why this model matters

This model enables:

- versioning structured reasoning
- comparing cognitive evolution
- justifying decisions
- tracing evidence
- rebuilding the full reasoning state
- exporting/importing without losing core semantics

## Related references

- [CTX_STRUCTURE.md](C:/sources/ctx-public/docs/CTX_STRUCTURE.md)
- [CLI_COMMANDS.md](C:/sources/ctx-public/docs/CLI_COMMANDS.md)
- [V1_FUNCTIONAL_SPEC.md](C:/sources/ctx-public/docs/V1_FUNCTIONAL_SPEC.md)

