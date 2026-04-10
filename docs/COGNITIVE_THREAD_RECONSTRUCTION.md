# Cognitive Thread Reconstruction in CTX
If a language model and its agent lose context, this is the tool you need.

This document defines how CTX should formally reconstruct the context thread from structured artifacts.

The goal is not to recreate a conversation.

The goal is to reconstruct:

- why a work line was opened
- which hypotheses justified it
- what evidence appeared
- which decisions were made
- which conclusions closed the block
- how all of that evolved across commits and branches

## Goal

Give CTX a canonical way to answer:

`how did we arrive at the current cognitive state`

That means being able to reconstruct:

- a local thread for an entity
- a thread for a task or goal
- a branch thread
- a temporal thread between commits
- a consolidated repository thread

## Current Problem

Today CTX already has:

- structured entities
- relationships
- cognitive commits
- graph export
- focused lineage

But it still lacks a formal definition of full reconstruction.

That creates this gap:

- you can inspect the graph
- you can view commit history
- but there is no canonical algorithm for reconstructing a structured reasoning narrative

## Guiding Principle

The cognitive thread must not come from chat logs.

It must be reconstructed exclusively from:

- `Goal`
- `Task`
- `Hypothesis`
- `Evidence`
- `Decision`
- `Conclusion`
- `ContextCommit`
- `BranchReference`
- `Traceability`

## What a Cognitive Thread Is

A cognitive thread is a structured, justifiable sequence of knowledge evolution.

It has two dimensions:

### 1. Semantic dimension

Answers:

- which artifacts relate to each other
- what the justification chain is

Example:

```text
Goal
  -> Task
    -> Hypothesis
      -> Evidence
      -> Decision
        -> Conclusion
```

### 2. Temporal dimension

Answers:

- in what order artifacts appeared or changed
- which commit recorded them
- in which branch they evolved

Example:

```text
Commit A -> Commit B -> Commit C
```

## Reconstruction Types

CTX should support these formal reconstructions.

### Entity reconstruction

Input:

- `GoalId`
- `TaskId`
- `HypothesisId`
- `DecisionId`
- `ConclusionId`

Output:

- focus artifact
- antecedents
- dependent artifacts
- related decisions
- derived conclusions
- relevant commits

### Task reconstruction

Answers:

- what problem the task tried to solve
- which hypotheses opened it
- what evidence it produced
- which decisions it triggered
- whether it closed with a conclusion

### Goal reconstruction

Answers:

- which tasks implemented it
- which hypotheses dominated
- which decisions affected the goal
- whether there are enough conclusions to validate it

### Branch reconstruction

Answers:

- which knowledge line dominates the branch
- which artifacts diverge from `main`
- which decisions are branch-only
- which commits introduced those differences

### Temporal reconstruction

Answers:

- how a knowledge line evolved between commits
- which nodes appeared
- which nodes changed state
- which new evidence moved a hypothesis or decision

## Canonical Reconstruction Model

Reconstruction should return a formal artifact, for example:

```json
{
  "focus": {},
  "semanticThread": [],
  "timeline": [],
  "branchContext": {},
  "openQuestions": [],
  "gaps": []
}
```

### `focus` field

The main node or line of the reconstruction.

Examples:

- `task:123`
- `goal:abc`
- `hypothesis:xyz`

### `semanticThread` field

Ordered sequence of artifacts connected by semantics.

Example:

```text
Goal -> Task -> Hypothesis -> Evidence -> Decision -> Conclusion
```

Each step should include:

- `entityType`
- `entityId`
- `relationship`
- `state`
- `summary`

### `timeline` field

Sequence of relevant historical events.

Each event should include:

- commit id
- branch
- timestamp
- change type
- affected artifact
- change summary

### `branchContext` field

Branch metadata:

- current branch
- head commit
- relevant commits
- divergences if any

### `openQuestions` field

Questions the thread cannot close yet.

Examples:

- decision without sufficient evidence
- hypothesis without a conclusion
- task without a final decision

### `gaps` field

Detected structural gaps.

Examples:

- missing evidence
- orphan conclusion
- goal without tasks
- hypothesis without support

## Recommended Algorithm

### Step 1. Choose focus

Focus can come from:

- explicit command
- node selected in viewer
- current branch
- head commit

### Step 2. Expand semantic relations

Expand the relevant subgraph in both directions:

- backwards
- forwards

Expansion rules:

- `Task` looks for `Goal`, `Hypothesis`, `Evidence`, `Decision`, `Conclusion`
- `Hypothesis` looks for `Task`, `Evidence`, `Decision`, `Conclusion`
- `Decision` looks for `Hypothesis`, `Evidence`, `Conclusion`
- `Conclusion` looks for `Decision`, `Evidence`, `Task`, `Goal`

### Step 3. Order by semantic importance

The sequence should not be only topological.

Prioritize:

1. direct justifiers
2. accepted decisions
3. accepted conclusions
4. strongest evidence
5. supporting artifacts

### Step 4. Recover temporal history

For each included entity:

- find the commit where it first appeared
- find commits where it changed
- find the relevant branch

### Step 5. Detect gaps

Examples:

- `Hypothesis` without `Evidence`
- `Decision` without `Evidence`
- `Conclusion` without `Decision`
- `Task` without `Hypothesis`
- `Goal` without `Task`

### Step 6. Produce a structured narrative

The final output should not be only a list of nodes.

It should be able to describe:

- origin
- evolution
- support
- outcome
- open gaps

## Recommended Outputs

CTX should be able to output the thread in multiple formats.

### 1. Structured JSON

For automation and LLMs.

Suggested command:

```powershell
ctx thread reconstruct --task <id> --format json
```

### 2. Narrative Markdown

For humans.

Suggested command:

```powershell
ctx thread reconstruct --goal <id> --format markdown
```

Expected output:

- initial context
- key hypotheses
- evidence
- decisions
- conclusion
- gaps

### 3. Mermaid

For visualization.

Suggested command:

```powershell
ctx thread reconstruct --hypothesis <id> --format mermaid
```

### 4. Agent-ready packet

So another model can resume work.

Suggested command:

```powershell
ctx thread reconstruct --task <id> --format packet
```

## Difference Between Graph, Lineage, and Thread

### `graph`

Shows the relational universe.

Question it answers:

- `what nodes and relationships exist`

### `lineage`

Shows a focused subgraph.

Question it answers:

- `what surrounds this node`

### `thread`

Reconstructs a justified cognitive narrative.

Question it answers:

- `how did we arrive at this state and what does it mean`

That difference is key.

## Integration with Commits

Reconstruction should use `ContextCommit` as the temporal backbone.

Each step in the thread should be able to say:

- first commit where it appears
- last commit where it changes
- relevant intermediate commits
- branch where it lives

This enables:

- audits
- cognitive debugging
- branch comparisons
- reproducible reconstruction

## Integration with Branches

A branch is not just a list of commits.

In CTX, a branch should be able to answer:

- which line of thought was explored there
- which hypotheses remained exclusive
- which decisions diverge from `main`
- whether conclusions were integrated or not

That is why formal reconstruction should include `branchContext`.

## Use with Agents

This is one of CTX's strongest points.

An agent should not receive:

- full chat
- raw logs
- disorderly text history

It should receive a `thread reconstruction packet` with:

- focus
- initial context
- key hypotheses
- relevant evidence
- accepted decisions
- active conclusions
- open gaps
- current commit

That enables resuming work with far less ambiguity.

## Recommended MVP

You do not need to solve everything at once.

Suggested order:

1. define the `ContextThread` model
2. implement reconstruction by `Task`
3. add `json` output
4. add `markdown` output
5. expose it in the CLI
6. show it in the viewer

Recommended initial command:

```powershell
ctx thread reconstruct --task <id> --format markdown
```

## Recommended Decision

CTX should formally include a cognitive thread reconstruction capability.

Not as a replacement for `graph` or `lineage`, but as a higher layer that:

- synthesizes
- orders
- justifies
- makes work resumable

## Product Impact

If implemented well, CTX gains a strong property:

- it does not just store structured knowledge
- it can explain why that knowledge ended in its current state

That makes CTX much more useful for:

- audits
- continuity across sessions
- multi-agent work
- handoffs between people
- strong reasoning traceability

