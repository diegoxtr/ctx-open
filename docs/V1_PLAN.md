# CTX - Objective, Scope, and Plan to V1
If a language model and its agent lose context, this is the tool you need.

## 1. Purpose

This document defines:

- the product objective for CTX
- the functional scope of the first usable version
- the intellectual property position of the project
- the development stages required to reach V1
- the initial plan to start testing with real users

Its goal is to align product, architecture, implementation, and validation.

## 2. Product vision

CTX is a cognitive version control system for structured reasoning work. Instead of versioning conversations, it versions explicit cognitive artifacts:

- context
- goals
- tasks
- hypotheses
- decisions
- evidence
- conclusions
- AI runs
- reasoning evolution

The vision is for CTX to enable AI work that is reproducible, auditable, and less costly than opaque conversational histories.

## 3. Business and product objective

### Primary objective

Build a commercializable CTX base that can:

- initialize a local cognitive repository
- record structured artifacts
- generate optimized context packets
- execute runs with AI providers
- persist reproducible cognitive commits
- compare changes between states
- work with branches and merges
- detect cognitive conflicts
- provide operational observability for real testing

### Expected V1 outcome

A version usable by a small group of technical testers that validates:

- real usefulness of the work model
- clarity of cognitive artifacts
- decision traceability
- reduction of redundant iterations
- viability with real providers
- quality of the CLI flow

## 4. Problem it solves

CTX addresses:

- context loss between AI work sessions
- difficulty understanding why a decision was made
- duplication of reasoning already done
- lack of traceability between hypotheses, evidence, and conclusions
- inability to reproduce a working cognitive state
- excessive cost of resending irrelevant context
- missing branching and merge mechanisms for structured reasoning

## 5. Product scope

## 5.1 Functional scope included for V1

V1 must include at minimum:

- local `.ctx/` repository
- functional CLI
- structured domain model
- reproducible cognitive commits
- diff between cognitive states
- branching, checkout, and merge
- cognitive conflict detection
- `ContextPacket` construction
- `Run` execution with interchangeable providers
- basic cost and usage metrics
- inspection of goals, tasks, hypotheses, decisions, evidence, conclusions
- inspection of runs, packets, providers, and metrics
- automated tests for the central flow

## 5.2 Out of scope for V1

Not part of V1:

- web or desktop UI
- network multi-user
- remote sync
- advanced role-based permissions and security
- enterprise authentication
- provider/plugin marketplace
- advanced semantic engine with complex indexing
- prompt optimization via ML
- full visual dashboards
- deep IDE integrations
- shared server

## 6. Intellectual property

## 6.1 Ownership

The intellectual property of CTX software, architecture, source code, documentation, improvements, derivatives, and associated materials belongs to Diego Mariano Verrastro, under the terms defined in:

- `LICENSE`
- `CONTRIBUTOR_ASSIGNMENT.md`
- `NOTICE`

## 6.2 Third-party collaboration

External collaboration is allowed under these conditions:

- contributions must follow the project terms
- ownership of the project does not transfer
- contributions are subject to assignment or exclusive license in favor of the holder
- economic and commercial exploitation remains under holder control

## 6.3 Operational note

For robust commercialization, any significant external contributor should accept a written agreement consistent with `CONTRIBUTOR_ASSIGNMENT.md`.

## 7. Target V1 users

Initial users to validate:

- software architects
- platform engineers
- teams working iteratively with LLMs
- technical researchers
- people needing decision traceability
- developers who want to reduce conversational cost and noise

## 8. V1 value proposition

V1 must demonstrate:

- structured reasoning is more reusable than raw conversation
- cognitive history can be versioned with clear rules
- iteration cost can be measured
- reasoning conflicts can be detected as explicit entities
- a CLI is sufficient for early product testing

## 9. Current project state

Today the repository already includes:

- .NET 8 solution
- layered architecture
- typed domain
- filesystem persistence in `.ctx/`
- CLI with core operations
- OpenAI and Anthropic providers with offline fallback
- merge with cognitive conflicts
- automated core tests

This means the project is beyond ideation; it is consolidating toward a testable V1.

## 10. Stages to reach V1

## Stage 1 - Stable foundation

Goal:
- consolidate the technical core to avoid structural rework

Deliverables:
- stable domain
- consistent persistence
- core flow test coverage
- essential CLI commands
- consistent basic error handling

Status:
- mostly advanced

## Stage 2 - Functional completeness of the cognitive repository

Goal:
- ensure all key cognitive artifacts can be created, inspected, versioned, and compared

Tasks:
- complete missing read and navigation commands
- improve CLI messages and errors
- strengthen diffs by artifact type
- improve merge with guided conflict resolution
- ensure full traceability between entities

Exit criteria:
- a technical user can operate CTX end-to-end without manual file edits

## Stage 3 - Operational quality for pilot

Goal:
- prepare the tool for real low-scale use

Tasks:
- harden input validation
- improve serialization and file compatibility
- add diagnostic exports
- add clear operational logs
- define `.ctx/` repository versioning strategy
- ensure reproducibility of commits and packets

Exit criteria:
- a pilot can run without constant developer assistance

## Stage 4 - Real provider integration

Goal:
- move from simulation/controlled to real usage with accounts and production models

Tasks:
- harden OpenAI and Anthropic adapters
- support file and environment configuration
- define error handling and retries
- record costs more precisely
- allow model selection policies

Exit criteria:
- testers can run real cases and measure cost/benefit

## Stage 5 - V1 pilot

Goal:
- release a controlled trial version

Deliverables:
- clear installation
- initial usage guide
- test scenarios
- stable commands
- version changelog
- documented feedback loop

Exit criteria:
- at least 3 to 5 technical users can complete real flows and return actionable feedback

## 11. Development backlog toward V1

### High priority

- `log` command with better navigation and filters
- assisted cognitive conflict resolution
- provider configuration by file and secrets
- repository version and migration handling
- more readable diff output between commits
- end-to-end CLI integration tests
- more complete command help
- cross-reference validation
- cognitive repo export/import
- workflow documentation

### Medium priority

- search indices in `.ctx/index`
- better `ContextBuilder` heuristics
- tag support and temporal filters
- packet snapshots tied to commits
- automatic cognitive history summary
- repository statistics commands

### Low priority after V1

- graphical interface
- remote sync
- concurrent multi-user collaboration
- editor/IDE integrations
- server mode
- visual dashboards

## 12. Definition of V1

V1 exists when:

- the repository can be initialized and used without manual JSON edits
- all main cognitive artifacts can be created, listed, and queried
- context can be built and runs executed
- commits are reproducible
- diff between states is useful
- merge identifies cognitive conflicts
- basic usage metrics exist
- automated tests cover the core
- documentation is sufficient for technical onboarding
- at least one controlled pilot can run

## 13. Initial test plan

## 13.1 Pilot objective

Validate whether CTX improves quality and efficiency of AI work in real analysis and development scenarios.

## 13.2 Tester profile

- technical
- familiar with CLI
- capable of formulating goals, tasks, hypotheses, and decisions
- willing to record structured feedback

## 13.3 Suggested test scenarios

- architecture design for a new module
- evaluation of technical alternatives
- investigation of a complex bug
- decision analysis with evidence
- product iteration with multiple hypotheses

## 13.4 Indicators to observe

- time to reach a useful conclusion
- number of repeated iterations
- amount of resent context
- clarity of recorded decisions
- ease of recovering prior state
- understanding of diffs and merges
- token or ACU cost

## 13.5 Expected pilot outcome

The pilot should answer:

- whether CTX's mental model is understandable
- whether operational cost is acceptable
- whether the CLI is sufficient
- whether artifact structure adds real value
- whether to move to a restricted public V1 or a private beta

## 14. Primary risks

- excessive domain model complexity
- CLI friction
- difficulty for users to think in structured artifacts
- real provider cost higher than expected
- unintuitive cognitive merges
- insufficient validation of IP ownership in contributions
- lack of real test cases

## 15. Recommended operational decision

Recommended next steps:

1. consolidate the high-priority backlog
2. prepare 3 real test scenarios
3. run a closed technical pilot
4. measure cost, traceability, and usefulness
5. use that feedback to freeze V1 scope

## 16. Recommended immediate next step

The concrete next step is:

- close the real CLI experience
- improve inspection and conflict resolution
- prepare a short test guide
- run an internal pilot of the V1 candidate

That step moves CTX from a promising technical base to a truly evaluable solution.

