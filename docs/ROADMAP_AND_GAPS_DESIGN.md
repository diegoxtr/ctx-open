# Roadmap And Gaps Design
If a language model and its agent lose context, this is the tool you need.

## Purpose

This document defines the intended split between immediate execution and future planning in CTX.

The core rule:

- `ctx next` is for executable work that can be started and closed now
- `ctx gaps` is for unresolved missing work inferred from CTX knowledge
- `ctx roadmap` is for suggested future planning material that should not pollute `ctx next`

This keeps the next-step loop small while preserving long-horizon ideas.

## Problem

CTX already records goals, tasks, hypotheses, evidence, decisions, conclusions, runbooks, triggers, and cognitive commits.

That is enough to avoid losing work, but it creates a planning tension:

- immediate tasks should be easy to select
- future ideas should not be forgotten
- blocked work should not look executable
- large ideas should not be stored as normal tasks only to keep them visible
- agents need a way to see suggested future work without switching away from the current execution thread

If everything is a `Task`, `ctx next` becomes noisy.
If future ideas are not represented, they disappear into chat.

## Command Roles

### `ctx next`

Use for the next executable work block.

It should return:

- `Task` candidates that are `Ready` or `InProgress`
- `Gap` candidates only when no executable tasks remain
- no long-horizon epics or future ideas as executable candidates

Blocked tasks may appear in diagnostics, but should not be the primary recommendation unless the recommended action is specifically to unblock them.

Current behavior:

- `Blocked` tasks are counted in diagnostics
- `Blocked` tasks are not returned as executable `ctx next` recommendations
- if only blocked work remains, `ctx next` returns no recommendation and points the operator to `ctx gaps` or `ctx roadmap`
- blocked or parked work must be promoted explicitly before it becomes executable again

### `ctx gaps`

Use for specific missing work.

It should answer:

- what is unresolved
- why it is unresolved
- which CTX artifacts support that reading
- whether the gap is actionable now
- what task could be opened to close it

Candidate sources:

- strong `Proposed` or `UnderEvaluation` hypotheses whose related tasks are closed
- accepted conclusions that explicitly leave future work open
- blocked tasks with no active unblocker task
- command adoption gaps from `ctx usage coverage`
- structural gaps from thread reconstruction
- documentation TODOs or release notes that are already represented in CTX evidence

`ctx gaps` should not create tasks by default.
It should recommend task openings conservatively.

### `ctx roadmap`

Use for future planning.

It should answer:

- what future lines are worth remembering
- which ideas are candidates, blocked, deferred, or ready to activate
- which work belongs together
- what should be promoted into tasks next
- what is intentionally parked

Candidate sources:

- parked future tasks
- future epics
- high-score hypotheses not yet converted into work
- unresolved gaps from `ctx gaps`
- recurring operational issues
- cold but high-value commands from command coverage
- release notes marked as future work
- active goals whose subtree has no live work

`ctx roadmap` should be suggestive, not imperative.
It can say "consider opening this task" but must not compete with `ctx next`.

## Epic Semantics

An epic is a durable future planning container.

Use it when an idea:

- is too large for one executable task
- should not enter `ctx next`
- should remain visible for future roadmap generation
- needs multiple future tasks or decisions
- is not ready for implementation

An epic should carry:

- title
- summary
- parent goal
- state
- rationale
- evidence links
- optional candidate tasks
- activation criteria

Suggested states:

- `Idea`: durable thought, no commitment
- `Candidate`: plausible future line, needs ranking
- `Parked`: intentionally not active
- `Ready`: can be decomposed into tasks
- `Active`: has executable tasks
- `Closed`: no longer needed or already delivered

Rule:

- epics never appear as executable `ctx next` candidates
- epics can appear in `ctx roadmap`
- epics can feed `ctx gaps` only when they expose a concrete missing work item

## Output Shape

### `ctx gaps`

Suggested JSON shape:

```json
{
  "summary": {
    "candidateCount": 3,
    "actionableCount": 1,
    "blockedCount": 1,
    "deferredCount": 1
  },
  "candidates": [
    {
      "candidateType": "Gap",
      "title": "Blocked future task has no unblocker",
      "sourceType": "Task",
      "sourceId": "<taskId>",
      "state": "Actionable",
      "score": 0.72,
      "rationale": "The task is important, blocked, and no active task exists to remove the block.",
      "recommendedAction": "Open an unblocker task",
      "suggestedTaskTitle": "Design unblocker for <topic>",
      "references": [
        { "entityType": "Task", "entityId": "<taskId>" },
        { "entityType": "Hypothesis", "entityId": "<hypothesisId>" }
      ]
    }
  ]
}
```

### `ctx roadmap`

Suggested JSON shape:

```json
{
  "summary": {
    "epicCount": 2,
    "gapCount": 3,
    "readyToPromoteCount": 1,
    "parkedCount": 4
  },
  "lanes": [
    {
      "lane": "Ready to promote",
      "items": [
        {
          "candidateType": "RoadmapItem",
          "sourceType": "Gap",
          "sourceId": "<gapSourceId>",
          "title": "Add ctx gaps command",
          "recommendedAction": "Promote to task"
        }
      ]
    },
    {
      "lane": "Parked ideas",
      "items": [
        {
          "candidateType": "Epic",
          "sourceType": "Epic",
          "sourceId": "<epicId>",
          "title": "Epic planning layer",
          "recommendedAction": "Keep parked until roadmap/gaps semantics are implemented"
        }
      ]
    }
  ]
}
```

## Scoring Model

Use conservative scoring.

Suggested factors:

- `evidenceStrength`
- `hypothesisScore`
- `goalPriorityScore`
- `recurrenceScore`
- `blockerScore`
- `freshnessScore`
- `activationReadiness`
- `contextNoisePenalty`
- `duplicationRiskPenalty`

Formula sketch:

```text
roadmapScore =
  hypothesisScore * 0.25
+ evidenceStrength * 0.20
+ goalPriorityScore * 0.15
+ recurrenceScore * 0.15
+ blockerScore * 0.10
+ activationReadiness * 0.10
- contextNoisePenalty * 0.10
- duplicationRiskPenalty * 0.10
```

For `ctx gaps`, prefer specificity over ambition.
For `ctx roadmap`, prefer durable value over immediacy.

## Promotion Rules

A roadmap item can become a task only when:

- the gap is specific
- the outcome is testable
- the parent goal or sub-goal is known
- the work is small enough to close
- duplicate search found no equivalent active task
- the recommended task title describes work, not only an idea

Promotion should be explicit:

```powershell
ctx gaps
ctx task add --title "<suggested task>" --goal <goalId>
```

Future command:

```powershell
ctx roadmap promote <itemId> --goal <goalId>
```

Promotion should create normal CTX structure:

- task
- optional hypothesis
- evidence reference to the roadmap or gap source

## Relationship With Existing Commands

`ctx next`:

- stays focused on execution
- should not become a full planning board

`ctx plan`:

- can include a small roadmap/gaps summary
- should still focus on the current task and runbooks

`ctx context`:

- should include roadmap items only when explicitly requested

`ctx usage coverage`:

- can feed `ctx gaps` with command adoption gaps

`ctx thread reconstruct`:

- can feed `ctx gaps` with structural closure gaps

## Implementation Path

Recommended sequence:

1. document the semantics and output contracts
2. adjust `ctx next` so blocked future work does not become the primary executable recommendation
3. implement read-only `ctx gaps`
4. implement read-only `ctx roadmap`
5. add MCP parity as `ctx_gaps` and `ctx_roadmap`
6. add optional `--json` and `--limit`
7. add explicit promotion commands only after the read-only surfaces are trusted

Current implementation status:

- steps 1 through 5 are implemented
- `ctx gaps` and `ctx roadmap` are read-only CLI surfaces
- MCP parity is implemented as `ctx_gaps` and `ctx_roadmap`
- promotion commands remain future work

## Non-Goals

- do not auto-create tasks from roadmap suggestions
- do not let epics compete with executable tasks
- do not turn `ctx next` into a planning dashboard
- do not infer future work from raw chat unless it has been recorded as CTX evidence, trigger, hypothesis, decision, conclusion, or task
- do not add ACP-specific behavior as part of this design

## Design Decision

Planning should become a two-layer model:

```text
execution layer: ctx next / ctx plan
future-planning layer: ctx gaps / ctx roadmap / future epics
```

This keeps agents simple:

- ask `ctx next` to work
- ask `ctx gaps` to discover missing actionable work
- ask `ctx roadmap` to review future planning
