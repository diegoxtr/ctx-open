# CTX - V1 Functional Specification
If a language model and its agent lose context, this is the tool you need.

## 1. Document objective

This document defines the target functional specification for CTX V1.

Its purpose is to:

- define what the product must do
- delimit what is in and out of V1
- establish acceptance criteria
- serve as a technical execution and validation reference

## 2. Product summary

CTX is a cognitive version control system that records, versions, compares, and reuses structured reasoning.

The product operates on explicit cognitive artifacts:

- context
- goals
- tasks
- hypotheses
- decisions
- evidence
- conclusions
- runs
- packets
- commits

## 3. V1 functional objective

V1 must allow a technical user to:

- initialize a cognitive repository
- model a problem using structured artifacts
- build relevant context
- execute AI iterations
- save reproducible cognitive snapshots
- compare changes
- work with branches
- detect cognitive conflicts
- inspect repository state operationally

## 4. V1 functional scope

## 4.1 Included in V1

- local `.ctx/` repository
- filesystem persistence
- CLI as primary interface
- interchangeable providers
- structured context
- cognitive commits
- cognitive diff
- branching and merge
- basic metrics
- automated core tests
- base operational documentation

## 4.2 Excluded from V1

- web UI
- remote sync
- real multi-user collaborative work
- advanced authentication and authorization
- visual dashboards
- deep IDE integrations
- cloud service
- workspace-level access control
- advanced semantic search

## 5. Functional modules

## Module A - Cognitive repository

### Objective

Initialize and maintain a local CTX repository with reproducible structure.

### Functional requirements

- create `.ctx/` with a known structure
- persist `version.json`, `config.json`, `project.json`, `HEAD`
- create functional repository directories
- load and save working state
- allow future repository format evolution

### Acceptance criteria

- `ctx init` creates the full structure
- `ctx status` reads the repository without errors
- state persists between runs
- repository format is identified by version

## Module B - Cognitive domain model

### Objective

Represent structured reasoning via traceable entities.

### Functional requirements

- support `Project`
- support `Goal`
- support `Task`
- support `Hypothesis`
- support `Decision`
- support `Evidence`
- support `Conclusion`
- support `Run`
- support `ContextCommit`
- support `ContextPacket`
- support `WorkingContext`

### Acceptance criteria

- all entities have strong identifiers
- all entities have relevant state or traceability
- relationships can be persisted and restored
- the model can reconstruct full cognitive state

## Module C - Cognitive artifact management

### Objective

Create, list, and query cognitive artifacts from the CLI.

### Functional requirements

- create goals
- create tasks
- create hypotheses
- create evidence
- create decisions
- create conclusions
- list artifacts by type
- show single artifacts by ID
- validate cross-references

### Acceptance criteria

- each artifact can be created via command
- each artifact can be inspected via `list/show`
- missing-reference errors are clear
- relationships between hypothesis, evidence, decision, and conclusion are traceable

## Module D - ContextBuilder

### Objective

Build optimized context packets for AI iterations.

### Functional requirements

- select relevant artifacts by goal or task
- avoid empty or irrelevant information
- generate context fingerprint
- estimate tokens
- persist packets

### Acceptance criteria

- `ctx context` returns a usable packet
- packet sections are coherent
- fingerprint changes when relevant content changes
- packets can be retrieved from the repository

## Module E - Runs and providers

### Objective

Execute AI runs with interchangeable providers and record results.

### Functional requirements

- define `IAIProvider` interface
- support at least OpenAI
- support at least Anthropic
- execute `Run` from a `ContextPacket`
- record run artifacts
- record usage, cost, and duration
- allow offline fallback for controlled testing

### Acceptance criteria

- `ctx run` executes without breaking the flow
- the run is persisted
- `run list/show` works
- `metrics show` reflects run impact
- without keys, the system remains testable

## Module F - Cognitive commits

### Objective

Persist reproducible cognitive snapshots of repository state.

### Functional requirements

- build commit from `WorkingContext`
- calculate snapshot hash
- record parent commit
- clear dirty state on commit
- persist commit in `.ctx/commits`

### Acceptance criteria

- `ctx commit` generates a stable commit
- snapshot is persisted
- `HEAD` updates
- active branch points to the created commit

## Module G - Cognitive diff

### Objective

Compare cognitive states and highlight relevant changes.

### Functional requirements

- detect changes in tasks
- detect changes in hypotheses
- detect changes in decisions
- detect changes in evidence
- detect changes in conclusions
- compare working state or commits

### Acceptance criteria

- `ctx diff` returns understandable changes
- changes distinguish added, modified, and removed
- diff summary is useful to users

## Module H - Branching and merge

### Objective

Explore alternative lines of reasoning.

### Functional requirements

- create branches
- switch branches
- merge branches
- detect divergent cognitive conflicts
- return an interpretable merge result

### Acceptance criteria

- `ctx branch` creates a usable branch
- `ctx checkout` switches active context
- `ctx merge` produces a clear result
- conflicts are listed explicitly

## Module I - Operational observability

### Objective

Provide visibility into system usage for testing and analysis.

### Functional requirements

- list providers
- list runs
- list packets
- show metrics
- show repository state
- show commit history

### Acceptance criteria

- users can audit work from the CLI
- cost and iteration counts can be reviewed
- operational state is understandable without manual file inspection

## Module J - Usage documentation

### Objective

Enable onboarding and product testing without direct developer dependency.

### Functional requirements

- document installation
- document first use
- document V1 plan
- document pilot
- document internal release

### Acceptance criteria

- a technical tester can start from written documentation
- a pilot guide exists
- an installation guide exists
- a V1 scope reference exists

## 6. Non-functional requirements

V1 must satisfy:

- .NET 8 compatibility
- reproducible local persistence
- understandable errors
- modular architecture
- low coupling between layers
- ability to add providers without major redesign
- automated core tests
- structured CLI output

## 7. Quality requirements

V1 is acceptable if:

- build passes
- core tests pass
- no manual JSON edits are required for the main flow
- CLI can cover a simple real case end-to-end
- the product supports at least one controlled technical pilot

## 8. Definition of done by module

A module is considered done if:

- behavior is implemented
- minimal validation exists
- automated coverage exists where relevant
- persistence is consistent
- it can be used from the CLI where applicable
- it has basic documentation if it impacts the user

## 9. V1 priority matrix

### Critical

- cognitive repository
- domain model
- artifact management
- context builder
- runs
- commits
- diff
- branching and merge
- minimal observability

### Important

- installation documentation
- pilot documentation
- internal release
- CLI UX improvements

### Post-V1

- visual UI
- remote sync
- multi-user
- advanced integrations
- advanced analytics

## 10. Formal V1 acceptance criteria

V1 is acceptable when:

- the main flow works end-to-end
- a stable internal release exists
- documentation is sufficient for a pilot
- the product supports at least a limited set of real cases
- initial feedback reveals no structural blockers

## 11. Expected outcome

If this specification is met, CTX reaches a functional V1 geared toward a technical pilot, with sufficient base to decide commercial and technical continuation.

