# Work Model and Prioritization in CTX
If a language model and its agent lose context, this is the tool you need.

## Goal

This document defines how CTX should reason about new work, derived work, and operational priorities without duplication or unnecessary context switching.

The core idea:

- not everything that appears deserves a new `task`
- not every global priority should beat a local blocker
- not every finding is a hypothesis
- not every related item should open a new thread

CTX needs to distinguish between:

- new roadmap work
- subwork
- duplicated work
- blockers
- close follow-ups

## Canonical taxonomy

Before deciding whether something deserves a `task`, CTX should distinguish these levels:

- `goal`
- `sub-goal`
- `issue`
- `gap`
- `task`
- `subtask`
- `blocker`
- `duplicate`
- `follow-up`
- `evidence`

Base rule:

- `goal` defines a durable strategic lane
- `sub-goal` defines a tactical line under a broader goal
- `operational runbook` defines compact reusable operational knowledge
- `issue` describes something that bothers, fails, or creates friction
- `gap` names the concrete difference between current state and desired state
- `task` defines the executable work to close that gap

Short formula:

```text
goal -> sub-goal -> task
operational runbook -> execution guidance
issue -> gap -> task
```

### `OperationalRunbook`

An `OperationalRunbook` stores compact recurring operational knowledge such as:

- local publish flows
- Git closeout rules
- recurring troubleshooting
- reusable guardrails

Use it when the knowledge:

- applies more than once
- should guide execution before the agent improvises
- is too important to remain buried only in docs or past evidence
- should stay small enough to enter a packet without inflating context cost

Do not use it for one-off execution history.
That still belongs in `evidence`, `decision`, `conclusion`, and normal task closure.

### `CognitiveTrigger`

A `CognitiveTrigger` stores the compact origin that opened or redirected a cognitive line.

Use it when:
- a user message opens a new line of work
- an agent continuation prompt materially redirects the line
- a recurring issue or runbook activation becomes part of the line origin
- the origin should remain auditable without relying on external chat history

Do not use it for every small reply.
Messages like `ok`, `continue`, or other trivial continuity nudges should not become standalone triggers.

### `Goal`

A `goal` is a strategic lane that should stay meaningful across many tasks and commits.

Use it when the work:

- opens or expands a durable product, platform, or operational direction
- should remain understandable even when no single execution thread is active
- is broader than one tactical branch of work

Do not create a new `goal` when the work is only a thematic branch of an already active strategic lane.

### `Sub-goal`

A `sub-goal` is a tactical line under an existing goal.

Use it when the work:

- belongs to an active strategic goal
- needs its own cognitive lane because the parent goal mixes multiple themes
- groups several related tasks that should not hang directly from the umbrella goal

This is the right layer for things like a UI branch under a viewer goal or a packaging branch under a distribution goal.

### Canonical structure rule

When new work appears, classify it in this order:

1. create a `goal` if the work opens or changes a durable strategic lane
2. create a `sub-goal` if the work belongs to an existing goal but needs its own thematic branch
3. create a `task` if the work is a concrete executable unit inside an existing lane
4. create a `subtask` only if the work exists mainly to help close a current parent task

Operationally:

- keep strategic goals open
- open new UI or product lines under them as `sub-goals`
- attach new tasks to the nearest tactical line, not to the umbrella goal by default
- use `ctx line open` when the operator intent is "open a tactical line here and start working inside it"

### `Issue`

An `issue` is any problem, friction, or negative observation.

Examples:

- the viewer loses visual focus on refresh
- git emitted an `index.lock`
- a hypothesis was linked to the wrong task

An `issue` does not automatically imply a new task.

### `Gap`

A `gap` is a specific, nameable, actionable difference between:

- what the system does today
- and what it should do for the workflow to be coherent

Example:

- current: the viewer has focus presets
- should: the viewer should preserve that focus across reloads
- gap: `graph focus does not persist across reloads`

A `gap` is precise enough to justify work.
But it is still a gap, not the implementation itself.

### `Task`

A `task` is the executable work that closes a gap or produces a real roadmap unit.

Example:

- gap: `the graph focus does not persist across reloads`
- task: `Persist graph focus selection in viewer`

### Translation rule

When something new appears, CTX should translate it in this order:

1. describe the `issue`
2. state the specific `gap`
3. decide whether it needs a `task`, `subtask`, `blocker`, or just `evidence`

Do not jump straight from an issue to a task if the gap is not well named.

## Current problem

CTX already has:

- `goal`
- `task`
- `hypothesis`
- `evidence`
- `decision`
- `conclusion`
- hypothesis scoring
- `ctx next` scoring

But it still lacks a layer to classify the nature of new work when it appears.

Without that layer, these failures happen:

- opening redundant tasks
- creating hypotheses for what is actually a local bug
- choosing a high-score task while a nearby blocker makes it irrational
- losing cognitive continuity by switching context too early

## Primary rule

Before creating new work, CTX should decide what type of work it is.

Proposed classification:

- `NewTask`
- `Subtask`
- `Duplicate`
- `Blocker`
- `RelatedFollowup`
- `EvidenceOnly`

## Work types

### `NewTask`

Use when a real roadmap unit appears.

Criteria:

- has standalone product value
- does not depend semantically on a single active task
- deserves its own evidence and conclusion
- is not a small local extension of existing work

### `Subtask`

Use when the work exists to close a parent task.

Criteria:

- originates inside an active task
- has its own verifiable outcome
- blocks or unblocks the parent task
- makes little sense as a separate roadmap item

Do not open a `subtask` if:

- it is a trivial implementation step
- it is just an operational note
- it is only evidence

### `Duplicate`

Use when the new work is already represented by an existing entity.

Criteria:

- same substantive intent
- same underlying problem
- same expected outcome
- only wording or observation time changed

In that case do not create another task.

Instead:

- link to the existing entity
- record additional evidence
- optionally mark it as `duplicate-of`

### `Blocker`

Use when something prevents closing a current or nearby task.

Criteria:

- appears during execution of an active task
- prevents validation or completion
- resolving it now is more valuable than jumping to a global priority

`Blocker` does not always require a new task.
Sometimes it only requires:

- evidence
- a decision
- or a small subtask

### `RelatedFollowup`

Use when the work is not duplicate, but not a fully independent new task.

Criteria:

- extends an existing line
- refines a hypothesis
- adds a natural improvement to an already worked feature
- depends strongly on the same context

### `EvidenceOnly`

Use when there is no new work, only new knowledge.

Examples:

- an observed failure
- an environment limitation
- a measurement
- a hypothesis validation

In that case record it as `evidence`, not as a `task`.

## Deduplication rule

Before creating a new task, CTX should run a conceptual check like this:

1. does an equivalent `task` already exist
2. does a `hypothesis` already cover this intent
3. did the problem appear within an active task
4. does the new item block a current task
5. is the work only a local extension of the current thread
6. was the problem already solved and only needs cognitive closure

Expected outcome:

- if an equivalent task exists: link it
- if it originates inside an active task: `subtask` or `blocker`
- if it is a natural continuation: `related-followup`
- if it only adds knowledge: `evidence`
- if it opens a real roadmap unit: `new task`

## Proximity rule

Priority should not come only from global score.

CTX should also consider proximity to the active node.

Example:

- you are closing a task
- a small bug appears that blocks validation
- another task has higher global score

In many cases, switching context is worse than resolving the local blocker first.

So it is useful to distinguish:

- `globalPriority`
- `executionPriority`

## Proposed scoring

Today `ctx next` already uses a reasonable global score.

The next evolution should add an execution layer:

```text
executionScore =
  globalPriorityScore
+ proximityScore
+ unblockScore
- contextSwitchCost
- duplicationRisk
```

### `globalPriorityScore`

Comes from:

- goal priority
- task state
- related hypothesis score

### `proximityScore`

Measures how close the new work is to the active node.

Examples that increase proximity:

- same task
- same goal
- same hypothesis
- same commit thread

### `unblockScore`

Increases when resolving this work unlocks validation or closure.

### `contextSwitchCost`

Increases when moving to that work breaks cognitive continuity.

Examples:

- changing goal
- switching product area
- abandoning a task close to closure

### `duplicationRisk`

Increases when the work resembles an existing task, hypothesis, or decision.

## Missing relationships in the model

To do this well, CTX should add explicit relationships between entities:

- `parent-task`
- `subtask-of`
- `blocks`
- `blocked-by`
- `duplicate-of`
- `follow-up-to`
- `refines`
- `discovered-during`

With these relations, the system can explain why new work does not deserve a new root task.

## Recommended operational rule

When new work appears:

1. check if an equivalent item already exists
2. if it exists, link instead of duplicating
3. if it originates inside an active task, evaluate `subtask` or `blocker`
4. if it only adds knowledge, record `evidence`
5. only create a new `task` if it truly opens another roadmap unit

## Useful future commands

To support this model, CTX could add commands such as:

- `ctx work classify`
- `ctx work attach --duplicate-of <id>`
- `ctx work attach --blocked-by <id>`
- `ctx task add --parent <taskId>`
- `ctx next --context <nodeId>`

Example:

```powershell
ctx next --context task:<taskId>
```

That would allow next-step selection based on continuity, not only global score.

## Conclusion

The right evolution for CTX is not just better scoring.

It is moving from:

- "what has the highest score"

to:

- "what should we do now, in this thread, without duplicating work or breaking continuity"

That requires:

- formal work classification
- deduplication before creating tasks
- real subtasks
- explicit blockers
- priority by proximity and ability to unblock

