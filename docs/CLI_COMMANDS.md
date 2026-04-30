# CTX CLI Commands
If a language model and its agent lose context, this is the tool you need.

This document describes the current CTX CLI surface in this repository.

CTX returns structured JSON output with this base format:

```json
{
  "success": true,
  "message": "short summary",
  "data": {}
}
```

## Conventions

- Run commands from the cognitive repository folder.
- In local development, the usual format is:

```powershell
dotnet run --project .\Ctx.Cli -- <command>
```

- In a published local installation:

```powershell
ctx <command>
```

- IDs like `<goalId>`, `<taskId>`, `<hypothesisId>`, or `<commitId>` come from previous commands.
- Multiple lists use comma-separated values.
- Most mutating changes affect `.ctx/working`, `.ctx/staging`, `.ctx/graph`, and eventually a later cognitive commit.

## General Commands

## First-Use Startup Flows

Do not start from a full command dump.
Start by deciding which repository state you are in.

## Operator State Machine

CTX should be operated as a small state machine, not as an unordered command list.

| Current state | Meaning | Next command |
|---|---|---|
| No CTX repository | `.ctx/` does not exist yet | `ctx init --name "<project>"` |
| Existing repo with open work | There are active task lines to continue | `ctx next` |
| Existing repo with pending cognitive changes | `Working context` is dirty and not yet snapshotteado | `ctx closeout` |
| Existing repo at a durable boundary | Closeout is clear and the block should become durable history | `ctx commit -m "<durable result>"` |
| Existing repo with no open work | No tasks are active; inspect remaining gaps or confirm closure | `ctx next` |

### Existing CTX repository

Use this when `.ctx/` already exists in the project root.

```powershell
ctx
ctx status
ctx audit
ctx next
```

Then continue from the recommended work line.

### New cognitive project

Use this when `.ctx/` does not exist yet.

```powershell
ctx init --name "<project>"
```

Then choose one of these:

- existing material already exists:

```powershell
ctx bootstrap map --from <path>
ctx bootstrap apply --from <path>
ctx next
```

- greenfield work:

```powershell
ctx goal add --title "<goal>"
ctx task add --title "<task>" --goal <goalId>
ctx hypo add --statement "<hypothesis>" --task <taskId>
ctx next
```

## Minimum Operator Loop

For most agents, the correct operating loop is:

```powershell
ctx
ctx next
```

Work inside `Working context`, then:

```powershell
ctx closeout
ctx commit -m "<durable result>"
```

`ctx commit` is not a raw thought log.
It records a durable cognitive state transition.

### `ctx`

Without arguments, shows helper-first operator guidance and points to the canonical command reference.

```powershell
dotnet run --project .\Ctx.Cli --
```

### `ctx version`

Shows product version and repository format version.

```powershell
dotnet run --project .\Ctx.Cli -- version
```

### `ctx init`

Initializes a cognitive repository in the current folder.

Options:
- `--name <project>`
- `--description <text>`
- `--branch <name>`

```powershell
dotnet run --project .\Ctx.Cli -- init --name "CTX Demo" --description "Sample repo" --branch main
```

### `ctx goal update`

Updates goal metadata or lifecycle state.

Options:
- `--title <text>`
- `--description <text>`
- `--priority <n>`
- `--state <Draft|Active|Validated|Completed|Superseded|Archived>`

```powershell
dotnet run --project .\Ctx.Cli -- goal update <goalId> --state Completed
```

### `ctx status`

Shows current repository status.

Includes:
- current branch
- `HEAD`
- `dirty` state
- counts for goals, tasks, hypotheses, decisions, evidence, conclusions, and runs
- when `dirty`, a bounded pending preview with:
  - compact diff summary
  - pending artifact count
  - up to five pending items
  - a suggested next action toward `ctx closeout`

```powershell
dotnet run --project .\Ctx.Cli -- status
```

### `ctx doctor`

Runs a technical diagnostic of the environment and the current repository.

Validates:
- product version
- `.ctx/` presence
- `HEAD`
- working context
- metrics
- configured providers
- environment credentials

```powershell
dotnet run --project .\Ctx.Cli -- doctor
```

### `ctx audit`

Runs a cognitive consistency audit on the current workspace.

Today it detects:
- tasks without hypotheses
- `Done` tasks without accepted conclusions
- hypotheses without evidence
- open hypotheses linked only to closed tasks
- `Accepted` decisions without `rationale` or `evidence`
- `Draft` conclusions linked to `Done` tasks

Returns:
- `consistencyScore`
- list of `issues`
- `severity`, `issueType`, `entityType`, `entityId`
- `suggestedAction`
- summary counts

Recommended usage:
- run before `ctx next`
- use to clear cognitive debt and avoid stale roadmap

```powershell
dotnet run --project .\Ctx.Cli -- audit
```

### `ctx next`

Prioritizes the next work block based on the current repository state.

Returns:
- recommended candidate
- composite score
- score factors
- ranked candidates
- diagnostics with:
  - task-state counts
  - eligible gap counts
  - gap exclusion counts
  - short recovery guidance when no recommendation is available

Current types:
- `Task`
- `Gap`

Important rules:
- prioritizes open tasks when they exist
- can promote gaps from strong hypotheses when no tasks are open
- only hypotheses in `Proposed` or `UnderEvaluation` are eligible as `Gap`
- hypotheses already closed by accepted conclusions should not resurface
- malformed metrics telemetry should not break `ctx next` or other normal CLI commands

Current task factors:
- `stateScore`
- `goalPriorityScore`
- `hypothesisScore`
- `dependencyReadinessScore`

```powershell
dotnet run --project .\Ctx.Cli -- next
```

### `ctx plan`

Builds a compact planning packet for the next work turn.

MCP equivalent: `ctx_plan`.

This is the preferred agent startup surface because it combines the common planning reads into one response instead of requiring separate calls to `ctx status`, `ctx next`, `ctx context`, and runbook inspection.

Returns:
- repository branch, head, and dirty state
- `ctx next` recommendation and diagnostics
- a focused context packet
- applicable runbook suggestions
- short planning guidance

Options:
- `--purpose <text>`
- `--goal <goalId>`
- `--task <taskId>`

```powershell
dotnet run --project .\Ctx.Cli -- plan --purpose "Plan MCP planning surface"
```

Typical output shape:

```json
{
  "branch": "main",
  "headCommitId": "<ctx-head>",
  "dirty": false,
  "purpose": "Plan MCP planning surface",
  "next": {
    "recommended": {
      "candidateType": "Task",
      "entityId": "<taskId>",
      "title": "<task title>",
      "state": "Ready",
      "score": 0.62
    },
    "diagnostics": {},
    "runbookSuggestions": []
  },
  "context": {
    "id": { "value": "<contextPacketId>" },
    "estimatedTokens": 1900,
    "sections": []
  },
  "runbookSuggestions": [],
  "additionalRunbooksAvailable": [],
  "guidance": []
}
```

### `ctx check`

Checks whether a task thread is ready for a cognitive commit.

Options:
- `--task <taskId>` to force a specific task

If `--task` is omitted, CTX resolves the focus task by:
- the single `InProgress` task, when that is unambiguous
- otherwise the top-ranked `Task` candidate from `ctx next`
- otherwise the only remaining open task

Returns:
- focused task id and title
- selection reason
- counts for linked hypotheses, evidence, decisions, and conclusions
- accepted decision and conclusion counts
- `readyForCommit`
- explicit missing closure items
- compact `runbookSuggestions` when matching `OperationalRunbook` entries apply to the focused task thread
- `additionalRunbooksAvailable` when more runbooks match than the suggestion limit allows
- short guidance for the next closeout step

```powershell
dotnet run --project .\Ctx.Cli -- check
dotnet run --project .\Ctx.Cli -- check --task <taskId>
```

### `ctx closeout`

Explains what still separates the current `working` state from `HEAD`.

Returns:
- `dirty`
- whether pending cognitive changes exist
- a compact diff summary
- pending artifacts with `changeType`, `entityType`, `entityId`, and `summary`
- short operator guidance for cognitive closeout
- optional `microCloseout` guidance when the pending delta is small enough to treat as a trailing closure instead of a full new block

Typical usage:
- run after recording evidence or conclusions
- run before `ctx commit`
- run before the Git commit when you are unsure whether CTX is fully closed

Semantic note:
- `ctx closeout` helps decide whether the current line crossed a durable snapshot boundary
- it does not mean every closed task must become its own commit
- use it to decide whether the current cognitive state is stable enough to preserve

```powershell
dotnet run --project .\Ctx.Cli -- closeout
```

### `ctx preflight`

Runs a compact operational preflight for a critical operation before execution or Git closeout.

Options:
- `--operation <git-closeout|publish-local|viewer-validation|public-release|recover-index-lock>`
- `--goal <goalId>` optional
- `--task <taskId>` optional

Returns:
- normalized operation id and label
- resolved scope (`workspace`, `goal:<id>`, or `task:<id>`)
- compact `runbookSuggestions`
- `additionalRunbooksAvailable` when more matches exist than the suggestion limit allows
- short guidance for the specific critical operation

Typical usage:
- before Git commit or push: `ctx preflight --operation git-closeout`
- before local publish: `ctx preflight --operation publish-local`
- after publishing a GitHub Release, before sharing or closing the release: `ctx preflight --operation github-release`
- when Git is blocked by `index.lock`: `ctx preflight --operation recover-index-lock`

```powershell
dotnet run --project .\Ctx.Cli -- preflight --operation git-closeout
dotnet run --project .\Ctx.Cli -- preflight --operation publish-local --task <taskId>
dotnet run --project .\Ctx.Cli -- preflight --operation github-release
```

### `ctx operational review`

Reviews repeated operational `IssueTrigger` entries and promotes recurring procedure failures into runbook-update guidance.

Options:
- `--operation <operation>` optional, for example `publish-local` or `git-closeout`
- `--threshold <n>` optional, defaults to `2`

Returns:
- repeated issue fingerprints that reached the threshold
- occurrence count and trigger ids
- related runbooks when any already match the operation
- suggested runbook updates for preconditions, failure signals, verification, and escalation boundaries

Typical usage:
- after the same blocker appears twice in one procedure
- before retrying a procedure that preflight reports as having repeated issues
- when deciding whether an operational lesson belongs in a playbook/runbook

```powershell
dotnet run --project .\Ctx.Cli -- operational review --operation publish-local --threshold 2
```

### `ctx runbook add`

Adds a compact `OperationalRunbook` for recurring procedures, troubleshooting, policies, or guardrails.

Options:
- `--title <text>`
- `--kind Procedure|Troubleshooting|Policy|Guardrail`
- `--when <text>`
- `--trigger <text>` repeatable
- `--precondition <text>` repeatable
- `--do <text>` repeatable
- `--verify <text>` repeatable
- `--signal <text>` repeatable
- `--escalate <text>` repeatable
- `--reference <text>` repeatable
- `--goal <goalId>` repeatable
- `--task <taskId>` repeatable

Design rules:
- keep the runbook compact
- summarize the operational path instead of duplicating long docs
- prefer canonical scripts, commands, and references
- keep `Preconditions`, `FailureSignals`, and `EscalationBoundary` short and checkable

```powershell
dotnet run --project .\Ctx.Cli -- runbook add --title "Local publish" --kind Procedure --trigger publish-local --when "Use when refreshing the installed local viewer" --precondition "No installed CTX binary is locked" --do "Run scripts/publish-local.ps1" --verify "Viewer responds locally" --signal "Failed to copy Ctx.Viewer.exe" --escalate "Stop retrying publish if binaries remain locked" --reference "scripts/publish-local.ps1"
```

### `ctx runbook list`

Lists stored `OperationalRunbook` entries for the current repository.

```powershell
dotnet run --project .\Ctx.Cli -- runbook list
```

### `ctx runbook show <runbookId>`

Shows a specific `OperationalRunbook`.

```powershell
dotnet run --project .\Ctx.Cli -- runbook show <runbookId>
```

### `ctx trigger add`

Adds a compact `CognitiveTrigger` for the origin of a cognitive line.

Options:
- `--summary <text>`
- `--kind UserPrompt|AgentPrompt|Continuation|RunbookTrigger|IssueTrigger`
- `--text <text>`
- `--goal <goalId>` repeatable
- `--task <taskId>` repeatable
- `--runbook <runbookId>` repeatable

```powershell
dotnet run --project .\Ctx.Cli -- trigger add --kind UserPrompt --summary "Fix viewer collapse interaction" --text "The collapse button feels broken and should move to a cleaner rail pattern." --task <taskId>
```

### `ctx prompt list`

Extracts prompt-like `CognitiveTrigger` entries in stable chronological order.

Default kinds:
- `UserPrompt`
- `AgentPrompt`
- `Continuation`

Options:
- `--kind <kind>` optional. Use `AgentPrompt`, `UserPrompt`, `Continuation`, `RunbookTrigger`, `IssueTrigger`, `prompt`, or `all`.

Output is ordered by `createdAtUtc` ascending and then by trigger id, so repeated exports are consistent.

```powershell
dotnet run --project .\Ctx.Cli -- prompt list
dotnet run --project .\Ctx.Cli -- prompt list --kind AgentPrompt
```

Alias:

```powershell
dotnet run --project .\Ctx.Cli -- prompts --kind all
```

### `ctx trigger list`

Lists stored `CognitiveTrigger` entries for the current repository.

```powershell
dotnet run --project .\Ctx.Cli -- trigger list
```

### `ctx trigger show <triggerId>`

Shows a specific `CognitiveTrigger`.

```powershell
dotnet run --project .\Ctx.Cli -- trigger show <triggerId>
```

## Cognitive Graph

### `ctx graph summary`

Returns a quick view of the current cognitive graph.

Includes:
- branch and `HEAD`
- total node/edge count
- counts by entity type
- available lineage focuses

```powershell
dotnet run --project .\Ctx.Cli -- graph summary
```

### `ctx graph show <nodeId>`

Returns a specific node and its immediate connections.

Accepts:
- raw ID, for example `<hypothesisId>`
- full ID, for example `Hypothesis:<hypothesisId>`

Includes:
- `node`
- `incoming`
- `outgoing`
- `connectedNodes`

```powershell
dotnet run --project .\Ctx.Cli -- graph show <hypothesisId>
dotnet run --project .\Ctx.Cli -- graph show Hypothesis:<hypothesisId>
```

### `ctx graph export`

Exports the cognitive graph projection.

Options:
- `--format json|mermaid`
- `--commit <commitId>`

Current formats:
- `json`
- `mermaid`

```powershell
dotnet run --project .\Ctx.Cli -- graph export --format json
dotnet run --project .\Ctx.Cli -- graph export --format mermaid
dotnet run --project .\Ctx.Cli -- graph export --format json --commit <commitId>
```

### `ctx graph lineage`

Returns a focused subgraph for semantic lineage navigation.

Supported focuses:
- `--goal <id>`
- `--task <id>`
- `--hypothesis <id>`
- `--decision <id>`
- `--conclusion <id>`

Common options:
- `--format json|mermaid`
- `--output <path>`

Typical cases:

```powershell
dotnet run --project .\Ctx.Cli -- graph lineage --goal <goalId>
dotnet run --project .\Ctx.Cli -- graph lineage --task <taskId>
dotnet run --project .\Ctx.Cli -- graph lineage --hypothesis <hypothesisId> --format mermaid
dotnet run --project .\Ctx.Cli -- graph lineage --decision <decisionId> --output .\tmp\decision-lineage.json
```

## Thread Reconstruction

### `ctx thread reconstruct --task <id>`

Reconstructs a formal cognitive thread for a task.

Current output:
- `focus`
- `semanticThread`
- `timeline`
- `branchContext`
- `openQuestions`
- `gaps`

Options:
- `--task <id>` required
- `--format json|markdown`

Current status:
- only `task` focus is supported today
- `json` is the structured output
- `markdown` is the human narrative output

```powershell
dotnet run --project .\Ctx.Cli -- thread reconstruct --task <taskId>
dotnet run --project .\Ctx.Cli -- thread reconstruct --task <taskId> --format markdown
```

## Goals

### `ctx goal add`

Creates a goal.

Options:
- `--title <text>` required
- `--description <text>`
- `--priority <n>`
- `--parent <goalId>`

```powershell
dotnet run --project .\Ctx.Cli -- goal add --title "Ship first CVCS core" --priority 1
dotnet run --project .\Ctx.Cli -- goal add --title "Improve viewer" --parent <goalId>
```

### `ctx line open`

Opens a tactical work line under an existing goal by creating a child goal and, optionally, the first task under that child goal.

Options:
- `--goal <goalId>` required parent goal
- `--title <text>` required tactical line title
- `--description <text>`
- `--priority <n>` defaults to the parent goal priority
- `--task-title <text>` optional first task title
- `--task-description <text>`

```powershell
dotnet run --project .\Ctx.Cli -- line open --goal <goalId> --title "Viewer working-focus UX"
dotnet run --project .\Ctx.Cli -- line open --goal <goalId> --title "Viewer working-focus UX" --task-title "Reduce umbrella-goal noise in Working view"
```

Notes:
- use this when the strategic goal should stay active but the new work needs its own tactical cognitive line
- this is a convenience flow over `goal add --parent` plus `task add --goal <subGoalId>`

### `ctx goal list`

Lists goals.

```powershell
dotnet run --project .\Ctx.Cli -- goal list
```

### `ctx goal show <goalId>`

Shows a specific goal.

```powershell
dotnet run --project .\Ctx.Cli -- goal show <goalId>
```

## Tasks

### `ctx task add`

Creates a task.

Options:
- `--title <text>` required
- `--description <text>`
- `--goal <goalId>`
- `--depends-on <taskId,taskId>`
- `--parent <taskId>`

```powershell
dotnet run --project .\Ctx.Cli -- task add --title "Implement commit engine" --goal <goalId>
dotnet run --project .\Ctx.Cli -- task add --title "Render graph panel" --goal <goalId> --depends-on <taskId>
dotnet run --project .\Ctx.Cli -- task add --title "Fix graph selection edge case" --parent <taskId>
```

Notes:
- `--parent` records the new task as a subtask of the given task
- if you omit `--goal`, the subtask inherits the parent's goal
- you cannot combine `--parent` with a different `--goal`

### `ctx task update <taskId>`

Updates a task.

Options:
- `--title <text>`
- `--description <text>`
- `--state Draft|Ready|InProgress|Blocked|Done`

```powershell
dotnet run --project .\Ctx.Cli -- task update <taskId> --state Done
```

### `ctx task list`

Lists tasks.

```powershell
dotnet run --project .\Ctx.Cli -- task list
```

### `ctx task show <taskId>`

Shows a specific task.

```powershell
dotnet run --project .\Ctx.Cli -- task show <taskId>
```

## Hypotheses

### `ctx hypo add`

Creates a hypothesis.

Options:
- `--statement <text>` required
- `--rationale <text>`
- `--task <taskId>`
- `--probability <0..1>` or `--confidence <0..1>`
- `--impact <0..1>`
- `--evidence-strength <0..1>`
- `--cost-to-validate <0..1>`

Notes:
- `confidence` and `probability` converge to the same value
- CTX computes `score` automatically

```powershell
dotnet run --project .\Ctx.Cli -- hypo add --statement "Structured commits reduce repeated iterations" --task <taskId>
dotnet run --project .\Ctx.Cli -- hypo add --statement "A compact timeline will improve readability" --task <taskId> --probability 0.8 --impact 0.9 --evidence-strength 0.5 --cost-to-validate 0.2
```

### `ctx hypo update <hypothesisId>`

Updates a hypothesis.

Options:
- `--statement <text>`
- `--rationale <text>`
- `--probability <0..1>` or `--confidence <0..1>`
- `--impact <0..1>`
- `--evidence-strength <0..1>`
- `--cost-to-validate <0..1>`
- `--state Proposed|UnderEvaluation|Supported|Refuted|Archived`

Typical usage:
- promote hypotheses after validating evidence
- archive stale hypotheses

```powershell
dotnet run --project .\Ctx.Cli -- hypo update <hypothesisId> --state Supported
```

### `ctx hypo rank`

Returns hypotheses ordered by score.

Useful for:
- validation prioritization
- gap inspection
- UI and planning

```powershell
dotnet run --project .\Ctx.Cli -- hypo rank
```

### `ctx hypo list`

Lists hypotheses.

```powershell
dotnet run --project .\Ctx.Cli -- hypo list
```

### `ctx hypo show <hypothesisId>`

Shows a specific hypothesis.

```powershell
dotnet run --project .\Ctx.Cli -- hypo show <hypothesisId>
```

### `ctx hypo relate <hypothesisId>`

Adds an explicit relation between two hypotheses.

Options:
- `--relation <type>` required
- `--to <hypothesisId>` required
- `--note <text>` optional

Useful relation types:
- `competes-with`
- `merged-into`
- `supersedes`
- `derived-from`
- `borrows-evidence-from`

```powershell
dotnet run --project .\Ctx.Cli -- hypo relate <hypothesisId> --relation competes-with --to <otherHypothesisId> --note "same evidence, different interpretation"
```

### `ctx hypo merge <sourceHypothesisId>`

Merges one hypothesis into another while preserving lineage.

Options:
- `--into <targetHypothesisId>` required

```powershell
dotnet run --project .\Ctx.Cli -- hypo merge <sourceHypothesisId> --into <targetHypothesisId>
```

### `ctx hypo supersede <oldHypothesisId>`

Marks one hypothesis as superseded by another.

Options:
- `--by <newHypothesisId>` required

```powershell
dotnet run --project .\Ctx.Cli -- hypo supersede <oldHypothesisId> --by <newHypothesisId>
```

### Branch-like hypothesis commands

These commands support the first branch-like hypothesis surface for competing interpretations.

Implemented commands:

- `ctx hypo update <id> --branch-state active|weakening|merged|deprecated|promoted`
- `ctx hypo relate <a> --relation competes-with|merged-into|supersedes|derived-from|borrows-evidence-from --to <b>`
- `ctx hypo merge <from> --into <to>`
- `ctx hypo supersede <old> --by <new>`
- `ctx evidence share <evidenceId> --to hypothesis:<id>`

Design intent:

- preserve several live interpretations without forcing immediate synthesis
- expose explicit relations between competing hypotheses
- keep evidence ownership or borrowing visible
- avoid coupling these operations to repository branches in the first version

Current surface:

- the CLI surface is available now
- the viewer exposes badges plus an `Interpretations` detail tab
- graph-level interpretation relations stay behind an explicit toggle so the main trace graph remains readable

## Evidence

### `ctx evidence add`

Adds evidence.

Options:
- `--title <text>` required
- `--summary <text>`
- `--source <text>`
- `--kind Observation|Experiment|Document|Validation`
- `--confidence <0..1>`
- `--supports <entityType:id,...>`

`--supports` accepts references such as:
- `hypothesis:<hypothesisId>`
- other supported entities

```powershell
dotnet run --project .\Ctx.Cli -- evidence add --title "Benchmark" --summary "Supports the current hypothesis" --source "pilot" --kind Experiment --supports hypothesis:<hypothesisId>
```

### `ctx evidence share <evidenceId>`

Shares existing evidence to another supported entity without duplicating the evidence record.

Options:
- `--to <entityType:id>` required

Typical usage:
- share evidence from one hypothesis branch to another interpretation
- attach already-recorded evidence to a later decision or conclusion when the model supports that reference

```powershell
dotnet run --project .\Ctx.Cli -- evidence share <evidenceId> --to hypothesis:<hypothesisId>
```

### `ctx evidence list`

Lists evidence.

```powershell
dotnet run --project .\Ctx.Cli -- evidence list
```

### `ctx evidence show <evidenceId>`

Shows specific evidence.

```powershell
dotnet run --project .\Ctx.Cli -- evidence show <evidenceId>
```

## Decisions

### `ctx decision add`

Adds a decision.

Options:
- `--title <text>` required
- `--rationale <text>`
- `--state Proposed|Accepted|Rejected`
- `--hypothesis <id,id>` or `--hypotheses <id,id>`
- `--evidence <id,id>`

```powershell
dotnet run --project .\Ctx.Cli -- decision add --title "Adopt structured commits" --rationale "Reduces drift" --state Accepted --hypotheses <hypothesisId> --evidence <evidenceId>
```

### `ctx decision update <decisionId>`

Updates an existing decision.

Options:
- `--title <text>`
- `--rationale <text>`
- `--state Proposed|Accepted|Superseded`
- `--hypothesis <id,id>` or `--hypotheses <id,id>`
- `--evidence <id,id>`

```powershell
dotnet run --project .\Ctx.Cli -- decision update <decisionId> --state Accepted --evidence <evidenceId>
```

### `ctx decision list`

Lists decisions.

```powershell
dotnet run --project .\Ctx.Cli -- decision list
```

### `ctx decision show <decisionId>`

Shows a specific decision.

```powershell
dotnet run --project .\Ctx.Cli -- decision show <decisionId>
```

## Conclusions

### `ctx conclusion add`

Adds a conclusion.

Options:
- `--summary <text>` required
- `--state Draft|Accepted|Rejected`
- `--decision <id,id>` or `--decisions <id,id>`
- `--evidence <id,id>`
- `--goal <id,id>` or `--goals <id,id>`
- `--task <id,id>` or `--tasks <id,id>`

```powershell
dotnet run --project .\Ctx.Cli -- conclusion add --summary "Proceed with structured commits" --state Accepted --decisions <decisionId> --evidence <evidenceId> --tasks <taskId>
```

### `ctx conclusion update <conclusionId>`

Updates an existing conclusion.

Options:
- `--summary <text>`
- `--state Draft|Accepted|Rejected`

```powershell
dotnet run --project .\Ctx.Cli -- conclusion update <conclusionId> --state Accepted
```

### `ctx conclusion list`

Lists conclusions.

```powershell
dotnet run --project .\Ctx.Cli -- conclusion list
```

### `ctx conclusion show <conclusionId>`

Shows a specific conclusion.

```powershell
dotnet run --project .\Ctx.Cli -- conclusion show <conclusionId>
```

## Context and Execution

### `ctx context`

Builds a summarized operational context for a run.

Options:
- `--purpose <text>`
- `--goal <goalId>`
- `--task <taskId>`

When matching `OperationalRunbook` entries exist, CTX injects up to `2` compact runbook summaries into the packet and exposes any overflow as `Additional runbooks available`.

```powershell
dotnet run --project .\Ctx.Cli -- context --purpose "Prepare architecture review" --goal <goalId>
```

### `ctx run`

Executes an AI run using the current context.

Options:
- `--provider openai|anthropic`
- `--model <model>`
- `--purpose <text>`
- `--goal <goalId>`
- `--task <taskId>`

Notes:
- if no credentials, CTX uses deterministic offline fallback

```powershell
dotnet run --project .\Ctx.Cli -- run --provider openai --model gpt-4.1 --purpose "Evaluate next step" --task <taskId>
```

### `ctx run list`

Lists recorded runs.

```powershell
dotnet run --project .\Ctx.Cli -- run list
```

### `ctx run show <runId>`

Shows a specific run.

```powershell
dotnet run --project .\Ctx.Cli -- run show <runId>
```

## Cognitive Versioning

### `ctx commit`

Creates a cognitive commit of the current state.

Semantic rule:

- `Working context` holds active reasoning
- `ctx commit` records a durable cognitive state transition
- task closure can suggest a commit boundary, but does not define it by itself

Use `ctx commit` when the current line crossed a meaningful durability boundary, for example:
- a decision was accepted
- a conclusion became stable
- a branch of reasoning was consolidated enough to preserve
- a work block reached a state worth reconstructing later without replaying the whole thought process

Do not treat `ctx commit` as:
- a dump of every thought
- a mandatory action after every tiny task closure
- a replacement for `Working context`

Options:
- `-m <message>`
- `--message <message>`

```powershell
dotnet run --project .\Ctx.Cli -- commit -m "seed cognitive graph"
```

### `ctx log`

Shows the history of the current branch.

Includes:
- branch
- commit count
- summary
- commit list

```powershell
dotnet run --project .\Ctx.Cli -- log
```

### `ctx diff [fromCommitId] [toCommitId]`

Calculates cognitive diffs.

Cases:
- no params: working context vs base
- one param: from commit to working context
- two params: between two commits

Summarizes changes in:
- decisions
- hypotheses
- evidence
- tasks
- conclusions
- conflicts

```powershell
dotnet run --project .\Ctx.Cli -- diff
dotnet run --project .\Ctx.Cli -- diff <fromCommitId>
dotnet run --project .\Ctx.Cli -- diff <fromCommitId> <toCommitId>
```

### `ctx branch <name>`

Creates a new branch from the current `HEAD`.

```powershell
dotnet run --project .\Ctx.Cli -- branch feature-context-ranking
```

### `ctx checkout <name>`

Switches to the given branch and updates the working context.

```powershell
dotnet run --project .\Ctx.Cli -- checkout main
```

### `ctx merge <sourceBranch>`

Merges another branch into the current branch.

Notes:
- if there is cognitive divergence, explicit conflicts are returned

```powershell
dotnet run --project .\Ctx.Cli -- merge feature-context-ranking
```

## Operational Inspection

### `ctx packet list`

Lists generated packets.

```powershell
dotnet run --project .\Ctx.Cli -- packet list
```

### `ctx packet show <packetId>`

Shows a specific packet.

```powershell
dotnet run --project .\Ctx.Cli -- packet show <packetId>
```

### `ctx provider list`

Lists configured providers.

```powershell
dotnet run --project .\Ctx.Cli -- provider list
```

### `ctx metrics show`

Shows accumulated metrics.

Includes:
- total runs
- tokens
- total ACU cost
- total time
- avoided redundancy
- invocations by command

```powershell
dotnet run --project .\Ctx.Cli -- metrics show
```

### `ctx usage summary`

Summarizes real CLI usage by command.

Includes:
- total invocations
- commands used
- frequency per command
- successes and failures
- commands not yet used

```powershell
dotnet run --project .\Ctx.Cli -- usage summary
```

### `ctx usage coverage`

Compares the known command catalog against real telemetry.

Includes:
- known commands
- used commands
- unused commands
- coverage percentage
- `usedCommands` and `unusedCommands` lists

```powershell
dotnet run --project .\Ctx.Cli -- usage coverage
```

## Portability

### `ctx bootstrap map`

Builds a provisional cognitive map from a file or directory without persisting entities into `.ctx`.

This is a bootstrap surface, not a final importer:

- it infers candidate threads
- it infers candidate hypotheses
- it extracts supporting evidence-like excerpts
- it highlights open questions and possible next tasks
- it keeps everything provisional so CTX can stay idea-first instead of collapsing into raw entity extraction

Options:
- `--from <path>`
- `--mode auto|article|project`
- `--max-files <n>`

```powershell
dotnet run --project .\Ctx.Cli -- bootstrap map --from .\README.md
dotnet run --project .\Ctx.Cli -- bootstrap map --from .\docs --mode project --max-files 12
```

### `ctx bootstrap apply`

Promotes only the strongest provisional bootstrap thread into durable CTX as a reviewable work line.

This command is intentionally conservative:

- it opens one provisional goal
- it seeds one review task
- it promotes up to three candidate hypotheses
- it attaches bootstrap evidence excerpts
- it keeps all promoted artifacts explicitly provisional

It does not:

- import every candidate thread
- accept decisions automatically
- close the work line

Options:
- `--from <path>`
- `--mode auto|article|project`
- `--max-files <n>`
- `--parent-goal <goalId>`

```powershell
dotnet run --project .\Ctx.Cli -- bootstrap apply --from .\README.md
dotnet run --project .\Ctx.Cli -- bootstrap apply --from .\docs --mode project --max-files 12 --parent-goal e8ab03570a874529b292f6265a3a67ee
```

### `ctx export`

Exports the current repository to a portable JSON snapshot.

Options:
- `--output <path>`

```powershell
dotnet run --project .\Ctx.Cli -- export --output .\tmp\ctx-export.json
```

### `ctx import`

Imports an exported snapshot.

Options:
- `--input <path>`

```powershell
dotnet run --project .\Ctx.Cli -- import --input .\tmp\ctx-export.json
```

## Recommended flow

Short cognitive workflow:

```powershell
dotnet run --project .\Ctx.Cli -- status
dotnet run --project .\Ctx.Cli -- audit
dotnet run --project .\Ctx.Cli -- next
dotnet run --project .\Ctx.Cli -- closeout
dotnet run --project .\Ctx.Cli -- goal add --title "Validate flow"
dotnet run --project .\Ctx.Cli -- task add --title "Execute end-to-end case" --goal <goalId>
dotnet run --project .\Ctx.Cli -- hypo add --statement "CTX reduces rework" --task <taskId>
dotnet run --project .\Ctx.Cli -- evidence add --title "Pilot result" --summary "Supports the current hypothesis" --supports hypothesis:<hypothesisId>
dotnet run --project .\Ctx.Cli -- conclusion add --summary "Proceed" --state Accepted --tasks <taskId>
dotnet run --project .\Ctx.Cli -- commit -m "close working block"
```

Inspection and traceability flow:

```powershell
dotnet run --project .\Ctx.Cli -- graph summary
dotnet run --project .\Ctx.Cli -- graph lineage --task <taskId> --format mermaid
dotnet run --project .\Ctx.Cli -- thread reconstruct --task <taskId> --format markdown
dotnet run --project .\Ctx.Cli -- log
dotnet run --project .\Ctx.Cli -- diff
```


