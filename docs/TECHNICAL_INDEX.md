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

1. [README.md](../README.md)
2. [V1_PLAN.md](V1_PLAN.md)
3. [V1_FUNCTIONAL_SPEC.md](V1_FUNCTIONAL_SPEC.md)
4. [TECHNICAL_ARCHITECTURE.md](TECHNICAL_ARCHITECTURE.md)
5. [DOMAIN_MODEL.md](DOMAIN_MODEL.md)
6. [CTX_STRUCTURE.md](CTX_STRUCTURE.md)
7. [CLI_COMMANDS.md](CLI_COMMANDS.md)
8. [BOOTSTRAP_COGNITIVE_INDEXING.md](BOOTSTRAP_COGNITIVE_INDEXING.md)
9. [BOOTSTRAP_TEST_DEVELOPMENT.md](BOOTSTRAP_TEST_DEVELOPMENT.md)
10. [HYPOTHESIS_BRANCH_SEMANTICS.md](HYPOTHESIS_BRANCH_SEMANTICS.md)
11. [ROADMAP_AND_GAPS_DESIGN.md](ROADMAP_AND_GAPS_DESIGN.md)
12. [CTX_MCP_AGENT_SETUP.md](CTX_MCP_AGENT_SETUP.md)
13. [CTX_MCP_TOOL_PARITY.md](CTX_MCP_TOOL_PARITY.md)
14. [INSTALLER_AND_DISTRIBUTION.md](INSTALLER_AND_DISTRIBUTION.md)

## Documents by category

## Product and scope

- [PROJECT_PHILOSOPHY.md](PROJECT_PHILOSOPHY.md)
  Product philosophy, local-first principles, openness, and governance boundaries.

- [COMMERCIAL_AND_GOVERNANCE_PHILOSOPHY.md](COMMERCIAL_AND_GOVERNANCE_PHILOSOPHY.md)
  Commercial and governance posture for CTX usage, derivatives, and service boundaries.

- [V1_PLAN.md](V1_PLAN.md)
  Summarizes objective, scope, phases, backlog, and V1 path.

- [V1_FUNCTIONAL_SPEC.md](V1_FUNCTIONAL_SPEC.md)
  Defines modules, requirements, acceptance criteria, and V1 definition of done.

- [RELEASE_1_0_15.md](RELEASE_1_0_15.md)
  Summarizes the current release baseline.

- [RELEASE_1_0_15_INVENTORY.md](RELEASE_1_0_15_INVENTORY.md)
  Records the public include/exclude pass from the post-1.0.13 release inventory.

- [RELEASE_1_0_14.md](RELEASE_1_0_14.md)
  Previous stable release notes.

- [RELEASE_1_0_13.md](RELEASE_1_0_13.md)
  Previous stable release notes.

- [RELEASE_1_0_12.md](RELEASE_1_0_12.md)
  Previous stable release notes.

- [RELEASE_1_0_3.md](RELEASE_1_0_3.md)
  Previous stable release notes.

- [RELEASE_1_0_2.md](RELEASE_1_0_2.md)
  Previous stable release notes.

- [RELEASE_1_0_1.md](RELEASE_1_0_1.md)
  Previous stable release notes.

- [RELEASE_1_0_0.md](RELEASE_1_0_0.md)
  Initial stable release notes.

## Architecture and design

- [TECHNICAL_ARCHITECTURE.md](TECHNICAL_ARCHITECTURE.md)
  Layers, responsibilities, dependencies, and end-to-end flows.

- [DOMAIN_MODEL.md](DOMAIN_MODEL.md)
  Entities, strong IDs, states, relationships, and domain rules.

- [CTX_STRUCTURE.md](CTX_STRUCTURE.md)
  `.ctx/` structure, base files, directories, and invariants.

- [CTX_SPECIFICATION_V1.md](CTX_SPECIFICATION_V1.md)
  Minimal CTX v1 specification: how context is stored, versioned, and structured.

- [COGNITIVE_GRAPH_AND_LINEAGE.md](COGNITIVE_GRAPH_AND_LINEAGE.md)
  Defines the relational projection of knowledge and its visualization roadmap.

- [COGNITIVE_THREAD_RECONSTRUCTION.md](COGNITIVE_THREAD_RECONSTRUCTION.md)
  Canonical model for reconstructing the cognitive thread from structured artifacts, commits, and branches.

- [COGNITIVE_TRIGGERS.md](COGNITIVE_TRIGGERS.md)
  Persistent origin model for cognitive lines, compact trigger summaries, and packet integration.

- [HYPOTHESIS_SCORING.md](HYPOTHESIS_SCORING.md)
  Scoring model for hypothesis confidence, evidence, validation cost, and prioritization.

- [WORK_MODEL_AND_PRIORITIZATION.md](WORK_MODEL_AND_PRIORITIZATION.md)
  Canonical taxonomy for issue/gap/task/subtask/duplicate/blocker and proximity-based prioritization.

- [ROADMAP_AND_GAPS_DESIGN.md](ROADMAP_AND_GAPS_DESIGN.md)
  Current split between executable `ctx next`, read-only unresolved gaps, read-only roadmap suggestions, and parked epics.

- [OPERATIONAL_RUNBOOKS.md](OPERATIONAL_RUNBOOKS.md)
  Compact design for recurring operational knowledge, packet injection, and overflow handling.

- [CTX_GOAL_FLOW_DIAGRAM.md](CTX_GOAL_FLOW_DIAGRAM.md)
  Example flow and CTX commands to resolve a goal and build the `.ctx` map.

## Operations and usage

- [CLI_COMMANDS.md](CLI_COMMANDS.md)
  Full reference of implemented CLI commands.

- [BOOTSTRAP_COGNITIVE_INDEXING.md](BOOTSTRAP_COGNITIVE_INDEXING.md)
  Defines the idea-first bootstrap map/apply surfaces for reconstructing provisional cognitive threads from external material.

- [BOOTSTRAP_TEST_DEVELOPMENT.md](BOOTSTRAP_TEST_DEVELOPMENT.md)
  Tracks how bootstrap indexing is being tested in practice, including regression cases, failure modes, and product conclusions.

- [HYPOTHESIS_BRANCH_SEMANTICS.md](HYPOTHESIS_BRANCH_SEMANTICS.md)
  Proposed branch-like lifecycle, relations, and evidence behavior for competing hypotheses without coupling them yet to repository branches.

- [COMMAND_ADOPTION_AND_COVERAGE.md](COMMAND_ADOPTION_AND_COVERAGE.md)
  Which commands are used, which are cold, and the recommended adoption order.

- [INSTALLATION_AND_USAGE_GUIDE.md](INSTALLATION_AND_USAGE_GUIDE.md)
  Operational onboarding for install, run, and first use.

- [PILOT_TESTING_GUIDE.md](PILOT_TESTING_GUIDE.md)
  Guide for running controlled pilots.

- [CTX_VIEWER_GUIDE.md](CTX_VIEWER_GUIDE.md)
  How to interpret the viewer, its timeline, branches, and panels.

- [LOCAL_CTX_INSTALLATION.md](LOCAL_CTX_INSTALLATION.md)
  Canonical local publish/install flow for `C:\ctx`, `ctx`, `ctx-mcp`, `ctx-agent-acp`, and `ctx-viewer`.

- [CTX_MCP_AGENT_SETUP.md](CTX_MCP_AGENT_SETUP.md)
  Step-by-step setup for connecting MCP-capable agents to the local CTX MCP server.

- [VS_CODE_MCP_VIDEO_GUIDE.md](VS_CODE_MCP_VIDEO_GUIDE.md)
  Recording-ready script, storyboard, JSON snippets, and smoke test for connecting VS Code / Copilot Chat to the local CTX MCP server.

- [CTX_MCP_TOOL_PARITY.md](CTX_MCP_TOOL_PARITY.md)
  CLI-to-MCP parity matrix for shipped tools, deferred surfaces, and validation coverage.

- [CTX_AGENT_CLIENT_PROTOCOL_DESIGN.md](CTX_AGENT_CLIENT_PROTOCOL_DESIGN.md)
  Design draft for a CTX agent-session layer and ACP-style adapter.

- [ACP_LOCAL_CONNECTION_GUIDE.md](ACP_LOCAL_CONNECTION_GUIDE.md)
  Local `ctx-agent-acp` connection guide with command, JSON-RPC messages, and read-only test flow.

- [MCP_SERVER_PROPOSAL.md](MCP_SERVER_PROPOSAL.md)
  Architecture and phased roadmap for the CTX MCP server.

- [INSTALLER_AND_DISTRIBUTION.md](INSTALLER_AND_DISTRIBUTION.md)
  Packaging model, portable archives, and distribution output policy.

- [USE_CTX_TO_BUILD_CTX.md](USE_CTX_TO_BUILD_CTX.md)
  Self-hosting workflow for using CTX to build and evolve CTX itself.

- [CTX_AUTONOMOUS_OPERATION_PROTOCOL.md](CTX_AUTONOMOUS_OPERATION_PROTOCOL.md)
  Autonomous operation protocol for CTX-first planning, execution, validation, and closeout.

## Demo and talk material

- [LIVE_DEMO.md](LIVE_DEMO.md)
  Public live demo surfaces, Codespaces expectations, static pages, local MCP setup, and validation paths.

- [live-demo/index.html](live-demo/index.html)
  Static public landing page for CTX 1.0.15.

- [live-demo/mcp-local.html](live-demo/mcp-local.html)
  Static local MCP setup page with copy-ready client snippets.

- [live-demo/talk-unju.html](live-demo/talk-unju.html)
  Public UNJu talk deck for CTX 1.0.15.

- [live-demo/notes.html](live-demo/notes.html)
  Demo notes and validation flow for the public static site.

## Operation prompts

- [CTX_HELPER_PROMPT.md](../prompts/CTX_HELPER_PROMPT.md)
  Installed helper/bootstrap prompt that re-anchors agents and operators on the active repo, core docs, viewer, and publication boundary before work starts.

- [CTX_BASE_PROMPT.md](../prompts/CTX_BASE_PROMPT.md)
  Base template for operating CTX with new tools (objective, scope, adaptation).

- [CTX_AGENT_PROMPT.md](../prompts/CTX_AGENT_PROMPT.md)
  Agent prompt with continuity, evidence, and cognitive closeout rules.

- [CTX_AUTONOMOUS_OPERATOR_PROMPT.md](../prompts/CTX_AUTONOMOUS_OPERATOR_PROMPT.md)
  Autonomous operator prompt with strict inspection/execution/closeout sequence.

## Repository root

- [README.md](../README.md)
  Project entry point.

- [CHANGELOG.md](../CHANGELOG.md)
  Summarized product change history.

- [LICENSE](../LICENSE)
  Source-available license.

- [COPYRIGHT.md](../COPYRIGHT.md)
  Copyright notice.

- [TRADEMARK.md](../TRADEMARK.md)
  Trademark usage rules.

- [CONTRIBUTOR_ASSIGNMENT.md](../CONTRIBUTOR_ASSIGNMENT.md)
  Contribution assignment terms.

- [NOTICE](../NOTICE)
  Supplemental repository notices.

## Validation scripts

- [run-smoke-test.ps1](../scripts/run-smoke-test.ps1)
  Reproducible functional validation flow.

- [run-merge-conflict-demo.ps1](../scripts/run-merge-conflict-demo.ps1)
  Reproducible branch/merge/conflict demo.

- [publish-local.ps1](../scripts/publish-local.ps1)
  Publishes local install in `C:\ctx` while preserving versioned workspace.

- [build-distribution.ps1](../scripts/build-distribution.ps1)
  Builds cross-platform portable CTX bundles from `distribution/targets.json`.

- [update-release-version.ps1](../scripts/update-release-version.ps1)
  Updates public release version surfaces before rebuilding artifacts.

## Recommended reading by profile

### Developer

1. [README.md](../README.md)
2. [TECHNICAL_ARCHITECTURE.md](TECHNICAL_ARCHITECTURE.md)
3. [DOMAIN_MODEL.md](DOMAIN_MODEL.md)
4. [CTX_STRUCTURE.md](CTX_STRUCTURE.md)
5. [CLI_COMMANDS.md](CLI_COMMANDS.md)

### Technical tester

1. [README.md](../README.md)
2. [INSTALLATION_AND_USAGE_GUIDE.md](INSTALLATION_AND_USAGE_GUIDE.md)
3. [CLI_COMMANDS.md](CLI_COMMANDS.md)
4. [PILOT_TESTING_GUIDE.md](PILOT_TESTING_GUIDE.md)

### Business/V1 scope

1. [V1_PLAN.md](V1_PLAN.md)
2. [V1_FUNCTIONAL_SPEC.md](V1_FUNCTIONAL_SPEC.md)
3. [RELEASE_1_0_0.md](RELEASE_1_0_0.md)

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
- MCP server setup and operating flow
- Agent client protocol design
- bootstrap regression development
- hypothesis branch-like semantics
- release handoff notes
- demo and talk material

## Useful future docs

Potential additions:

- technical decision ADRs
- manual cognitive conflict resolution guide
- provider integration guide
- operational troubleshooting guide
- post-V1 roadmap
