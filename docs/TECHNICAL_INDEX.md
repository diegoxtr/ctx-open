# CTX Technical Index
If a language model and its agent lose context, this is the tool you need.

This document centralizes technical and operational documentation available in the repo.

It is the entry point for:

- development
- architecture
- operations
- tests
- technical onboarding

## Quick reading map

Recommended order:

1. [README.md](C:/sources/ctx-open/README.md)
2. [V1_PLAN.md](C:/sources/ctx-open/docs/V1_PLAN.md)
3. [V1_FUNCTIONAL_SPEC.md](C:/sources/ctx-open/docs/V1_FUNCTIONAL_SPEC.md)
4. [TECHNICAL_ARCHITECTURE.md](C:/sources/ctx-open/docs/TECHNICAL_ARCHITECTURE.md)
5. [DOMAIN_MODEL.md](C:/sources/ctx-open/docs/DOMAIN_MODEL.md)
6. [CTX_STRUCTURE.md](C:/sources/ctx-open/docs/CTX_STRUCTURE.md)
7. [CLI_COMMANDS.md](C:/sources/ctx-open/docs/CLI_COMMANDS.md)
8. [BOOTSTRAP_COGNITIVE_INDEXING.md](C:/sources/ctx-open/docs/BOOTSTRAP_COGNITIVE_INDEXING.md)
9. [BOOTSTRAP_TEST_DEVELOPMENT.md](C:/sources/ctx-open/docs/BOOTSTRAP_TEST_DEVELOPMENT.md)
10. [HYPOTHESIS_BRANCH_SEMANTICS.md](C:/sources/ctx-open/docs/HYPOTHESIS_BRANCH_SEMANTICS.md)

## Documents by category

## Product and scope

- [V1_PLAN.md](C:/sources/ctx-open/docs/V1_PLAN.md)
  Summarizes objective, scope, phases, backlog, and V1 path.

- [V1_FUNCTIONAL_SPEC.md](C:/sources/ctx-open/docs/V1_FUNCTIONAL_SPEC.md)
  Defines modules, requirements, acceptance criteria, and V1 definition of done.

- [RELEASE_1_0_7.md](C:/sources/ctx-open/docs/RELEASE_1_0_7.md)
  Summarizes the current stable release baseline.

## Architecture and design

- [TECHNICAL_ARCHITECTURE.md](C:/sources/ctx-open/docs/TECHNICAL_ARCHITECTURE.md)
  Layers, responsibilities, dependencies, and end-to-end flows.

- [DOMAIN_MODEL.md](C:/sources/ctx-open/docs/DOMAIN_MODEL.md)
  Entities, strong IDs, states, relationships, and domain rules.

- [CTX_STRUCTURE.md](C:/sources/ctx-open/docs/CTX_STRUCTURE.md)
  `.ctx/` structure, base files, directories, and invariants.

- [CTX_SPECIFICATION_V1.md](C:/sources/ctx-open/docs/CTX_SPECIFICATION_V1.md)
  Minimal CTX v1 specification: how context is stored, versioned, and structured.

- [COGNITIVE_GRAPH_AND_LINEAGE.md](C:/sources/ctx-open/docs/COGNITIVE_GRAPH_AND_LINEAGE.md)
  Defines the relational projection of knowledge and its visualization roadmap.

- [COGNITIVE_THREAD_RECONSTRUCTION.md](C:/sources/ctx-open/docs/COGNITIVE_THREAD_RECONSTRUCTION.md)
  Canonical model for reconstructing the cognitive thread from structured artifacts, commits, and branches.

- [COGNITIVE_TRIGGERS.md](C:/sources/ctx-open/docs/COGNITIVE_TRIGGERS.md)
  Persistent origin model for cognitive lines, compact trigger summaries, and packet integration.

- [WORK_MODEL_AND_PRIORITIZATION.md](C:/sources/ctx-open/docs/WORK_MODEL_AND_PRIORITIZATION.md)
  Canonical taxonomy for issue/gap/task/subtask/duplicate/blocker and proximity-based prioritization.

- [OPERATIONAL_RUNBOOKS.md](C:/sources/ctx-open/docs/OPERATIONAL_RUNBOOKS.md)
  Compact design for recurring operational knowledge, packet injection, and overflow handling.

- [CTX_GOAL_FLOW_DIAGRAM.md](C:/sources/ctx-open/docs/CTX_GOAL_FLOW_DIAGRAM.md)
  Example flow and CTX commands to resolve a goal and build the `.ctx` map.

## Operations and usage

- [CLI_COMMANDS.md](C:/sources/ctx-open/docs/CLI_COMMANDS.md)
  Full reference of implemented CLI commands.

- [BOOTSTRAP_COGNITIVE_INDEXING.md](C:/sources/ctx-open/docs/BOOTSTRAP_COGNITIVE_INDEXING.md)
  Defines the idea-first bootstrap map/apply surfaces for reconstructing provisional cognitive threads from external material.

- [BOOTSTRAP_TEST_DEVELOPMENT.md](C:/sources/ctx-open/docs/BOOTSTRAP_TEST_DEVELOPMENT.md)
  Tracks how bootstrap indexing is being tested in practice, including regression cases, failure modes, and product conclusions.

- [HYPOTHESIS_BRANCH_SEMANTICS.md](C:/sources/ctx-open/docs/HYPOTHESIS_BRANCH_SEMANTICS.md)
  Proposed branch-like lifecycle, relations, and evidence behavior for competing hypotheses without coupling them yet to repository branches.

- [COMMAND_ADOPTION_AND_COVERAGE.md](C:/sources/ctx-open/docs/COMMAND_ADOPTION_AND_COVERAGE.md)
  Which commands are used, which are cold, and the recommended adoption order.

- [INSTALLATION_AND_USAGE_GUIDE.md](C:/sources/ctx-open/docs/INSTALLATION_AND_USAGE_GUIDE.md)
  Operational onboarding for install, run, and first use.

- [PILOT_TESTING_GUIDE.md](C:/sources/ctx-open/docs/PILOT_TESTING_GUIDE.md)
  Guide for running controlled pilots.

- [CTX_VIEWER_GUIDE.md](C:/sources/ctx-open/docs/CTX_VIEWER_GUIDE.md)
  How to interpret the viewer, its timeline, branches, and panels.

- [LOCAL_CTX_INSTALLATION.md](C:/sources/ctx-open/docs/LOCAL_CTX_INSTALLATION.md)
  Canonical local publish/install flow for `C:\ctx`, `ctx`, and `ctx-viewer`.

- [INSTALLER_AND_DISTRIBUTION.md](C:/sources/ctx-open/docs/INSTALLER_AND_DISTRIBUTION.md)
  Packaging model, portable archives, and distribution output policy.

## Operation prompts

- [CTX_HELPER_PROMPT.md](C:/sources/ctx-open/prompts/CTX_HELPER_PROMPT.md)
  Installed helper/bootstrap prompt that re-anchors agents and operators on the active repo, core docs, viewer, and private/public boundary before work starts.

- [CTX_BASE_PROMPT.md](C:/sources/ctx-open/prompts/CTX_BASE_PROMPT.md)
  Base template for operating CTX with new tools (objective, scope, adaptation).

- [CTX_AGENT_PROMPT.md](C:/sources/ctx-open/prompts/CTX_AGENT_PROMPT.md)
  Agent prompt with continuity, evidence, and cognitive closeout rules.

- [CTX_AUTONOMOUS_OPERATOR_PROMPT.md](C:/sources/ctx-open/prompts/CTX_AUTONOMOUS_OPERATOR_PROMPT.md)
  Autonomous operator prompt with strict inspection/execution/closeout sequence.

## Repository root

- [README.md](C:/sources/ctx-open/README.md)
  Project entry point.

- [CHANGELOG.md](C:/sources/ctx-open/CHANGELOG.md)
  Summarized product change history.

- [LICENSE](C:/sources/ctx-open/LICENSE)
  Source-available license.

- [COPYRIGHT.md](C:/sources/ctx-open/COPYRIGHT.md)
  Copyright notice.

- [TRADEMARK.md](C:/sources/ctx-open/TRADEMARK.md)
  Trademark usage rules.

- [CONTRIBUTOR_ASSIGNMENT.md](C:/sources/ctx-open/CONTRIBUTOR_ASSIGNMENT.md)
  Contribution assignment terms.

- [NOTICE](C:/sources/ctx-open/NOTICE)
  Supplemental repository notices.

## Validation scripts

- [run-smoke-test.ps1](C:/sources/ctx-open/scripts/run-smoke-test.ps1)
  Reproducible functional validation flow.

- [run-merge-conflict-demo.ps1](C:/sources/ctx-open/scripts/run-merge-conflict-demo.ps1)
  Reproducible branch/merge/conflict demo.

- [publish-local.ps1](C:/sources/ctx-open/scripts/publish-local.ps1)
  Publishes local install in `C:\ctx` while preserving versioned workspace.

- [build-distribution.ps1](C:/sources/ctx-open/scripts/build-distribution.ps1)
  Builds cross-platform portable CTX bundles from `distribution/targets.json`.

## Recommended reading by profile

### Developer

1. [README.md](C:/sources/ctx-open/README.md)
2. [TECHNICAL_ARCHITECTURE.md](C:/sources/ctx-open/docs/TECHNICAL_ARCHITECTURE.md)
3. [DOMAIN_MODEL.md](C:/sources/ctx-open/docs/DOMAIN_MODEL.md)
4. [CTX_STRUCTURE.md](C:/sources/ctx-open/docs/CTX_STRUCTURE.md)
5. [CLI_COMMANDS.md](C:/sources/ctx-open/docs/CLI_COMMANDS.md)

### Technical tester

1. [README.md](C:/sources/ctx-open/README.md)
2. [INSTALLATION_AND_USAGE_GUIDE.md](C:/sources/ctx-open/docs/INSTALLATION_AND_USAGE_GUIDE.md)
3. [CLI_COMMANDS.md](C:/sources/ctx-open/docs/CLI_COMMANDS.md)
4. [PILOT_TESTING_GUIDE.md](C:/sources/ctx-open/docs/PILOT_TESTING_GUIDE.md)

### Business/V1 scope

1. [V1_PLAN.md](C:/sources/ctx-open/docs/V1_PLAN.md)
2. [V1_FUNCTIONAL_SPEC.md](C:/sources/ctx-open/docs/V1_FUNCTIONAL_SPEC.md)
3. [RELEASE_1_0_0.md](C:/sources/ctx-open/docs/RELEASE_1_0_0.md)

## Documentation coverage status

Currently documented:

- product objective
- V1 scope
- domain model
- technical architecture
- persistence structure
- CLI commands
- installation
- viewer
- pilot
- stable release
- IP and contributions
- cognitive graph
- formal thread reconstruction
- cognitive triggers
- local install and distribution
- bootstrap regression development
- hypothesis branch-like semantics

## Useful future docs

Potential additions:

- technical decision ADRs
- manual cognitive conflict resolution guide
- provider integration guide
- operational troubleshooting guide
- post-V1 roadmap



