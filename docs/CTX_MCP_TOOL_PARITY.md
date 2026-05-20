# CTX MCP Tool Parity

This document tracks the practical parity between the CTX CLI and the local stdio MCP server.

The goal is not to expose every CLI command blindly. The goal is to let agents operate CTX through MCP for normal cognitive work without falling back to shell commands.

## Current Must-Have Surface

### Repository and Health

| CLI surface | MCP tool | Status |
|---|---|---|
| `ctx version` | `ctx_version` | implemented |
| `ctx doctor` | `ctx_doctor` | implemented |
| `ctx status` | `ctx_status` | implemented |
| `ctx audit` | `ctx_audit` | implemented |
| `ctx next` | `ctx_next` | implemented |
| `ctx gaps` | `ctx_gaps` | implemented |
| `ctx roadmap` | `ctx_roadmap` | implemented |
| `ctx context` | `ctx_context` | implemented |
| `ctx plan` | `ctx_plan` | implemented |
| `ctx check` | `ctx_check` | implemented |
| `ctx closeout` | `ctx_closeout` | implemented |
| `ctx preflight` | `ctx_preflight` | implemented |
| `ctx operational review` | `ctx_operational_review` | implemented |

`ctx_plan` is the recommended first MCP call for a new agent turn. It returns repository state, the `ctx_next` recommendation, a focused context packet, runbook suggestions, and short guidance in one response.

### History and Graph

| CLI surface | MCP tool | Status |
|---|---|---|
| `ctx log` | `ctx_log` | implemented |
| `ctx diff` | `ctx_diff` | implemented |
| `ctx graph summary` | `ctx_graph_summary` | implemented |
| `ctx graph show` | `ctx_graph_show` | implemented |
| `ctx graph export` | `ctx_graph_export` | implemented |
| `ctx graph lineage` | `ctx_graph_lineage` | implemented |
| `ctx thread reconstruct` | `ctx_thread_reconstruct` | implemented |

### Artifact Inspection

| CLI surface | MCP tool | Status |
|---|---|---|
| `ctx goal list/show` | `ctx_goal_list`, `ctx_goal_show` | implemented |
| `ctx epic list/show` | `ctx_epic_list`, `ctx_epic_show` | implemented |
| `ctx task list/show` | `ctx_task_list`, `ctx_task_show` | implemented |
| `ctx hypo list/show/rank` | `ctx_hypothesis_list`, `ctx_hypothesis_show`, `ctx_hypothesis_rank` | implemented |
| `ctx evidence list/show` | `ctx_evidence_list`, `ctx_evidence_show` | implemented |
| `ctx decision list/show` | `ctx_decision_list`, `ctx_decision_show` | implemented |
| `ctx conclusion list/show` | `ctx_conclusion_list`, `ctx_conclusion_show` | implemented |
| generic artifact list/show | `ctx_artifact_list`, `ctx_artifact_show` | implemented |

### Cognitive Writes

These require `ctx-mcp --mode write`.

| CLI surface | MCP tool | Status |
|---|---|---|
| `ctx init` | `ctx_init` | implemented |
| `ctx goal add/update` | `ctx_goal_add`, `ctx_goal_update` | implemented |
| `ctx epic add/update/promote` | `ctx_epic_add`, `ctx_epic_update`, `ctx_epic_promote` | implemented |
| `ctx line open` | `ctx_line_open` | implemented |
| `ctx task add/update` | `ctx_task_add`, `ctx_task_update` | implemented |
| `ctx hypo add/update` | `ctx_hypothesis_add`, `ctx_hypothesis_update` | implemented |
| `ctx hypo relate/merge/supersede` | `ctx_hypothesis_relate`, `ctx_hypothesis_merge`, `ctx_hypothesis_supersede` | implemented |
| `ctx evidence add/share` | `ctx_evidence_add`, `ctx_evidence_share` | implemented |
| `ctx decision add/update` | `ctx_decision_add`, `ctx_decision_update` | implemented |
| `ctx conclusion add/update` | `ctx_conclusion_add`, `ctx_conclusion_update` | implemented |
| `ctx runbook add/update` | `ctx_runbook_add`, `ctx_runbook_update` | implemented |
| `ctx trigger add` | `ctx_trigger_add` | implemented |
| `ctx commit` | `ctx_commit` | implemented |

### Runbooks, Triggers, and Bootstrap

| CLI surface | MCP tool | Status |
|---|---|---|
| `ctx runbook list/show` | `ctx_runbook_list`, `ctx_runbook_show` | implemented |
| `ctx trigger list/show` | `ctx_trigger_list`, `ctx_trigger_show` | implemented |
| `ctx bootstrap map` | `ctx_bootstrap_map` | implemented |
| ACP-style session smoke | `ctx_acp_session_smoke` | implemented |
| `ctx bootstrap apply` | `ctx_bootstrap_apply` | implemented, write mode required |

## Intentionally Deferred

These are useful, but not required for the current agent-facing MCP baseline:

| CLI surface | Reason deferred |
|---|---|
| `ctx branch`, `ctx checkout`, `ctx merge` | repository-level state changes need stronger MCP guardrails |
| `ctx import`, `ctx export` | portable snapshot mutation/egress should be designed explicitly |
| `ctx update` | installed-binary self-update needs an explicit safe protocol before exposing through a running MCP server |
| `ctx runbook attach`, `ctx runbook detach` | useful follow-up; lower risk than repository mutation, but not in the current MCP write surface yet |
| `ctx prompt list` | useful read-only follow-up for trigger/prompt timeline inspection |
| `ctx run`, `ctx run list/show` | provider execution has credential and cost implications |
| `ctx provider list` | lower priority until provider execution is exposed |
| `ctx packet list/show` | useful for diagnostics, but not required for daily cognitive operation |
| `ctx metrics show`, `ctx usage summary`, `ctx usage coverage` | telemetry surfaces can follow after operational parity stabilizes |

## Compatibility Notes

- MCP remains local stdio only.
- Read tools work in `read-only` and `write` mode.
- Mutating tools require `--mode write`.
- `ctx_init` is special: it is allowed to target a configured folder that does not yet contain `.ctx`, but it still respects mode and allow-root boundaries.
- Hypothesis relation tokens accept CLI-friendly forms such as `derived-from`, `merged-into`, `competes-with`, and `borrows-evidence-from`.

## Validation

Current automated coverage:

```powershell
dotnet test Ctx.Tests\Ctx.Tests.csproj --no-restore --filter McpToolTests
```

The MCP tests cover:

- read-only write rejection
- write-mode task and commit flow
- repository initialization through MCP
- version, doctor, log, diff, artifact list/show, graph export, and lineage wrappers
- runbook and trigger add/list/show wrappers
- hypothesis relation wrapper
- bootstrap map/apply wrappers on a disposable repository
