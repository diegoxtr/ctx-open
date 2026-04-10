# Cognitive Graph and Lineage in CTX
If a language model and its agent lose context, this is the tool you need.

This document defines how CTX should represent, export, and visualize the cognitive graph of work.

The core idea is simple:

- CTX should not only store cognitive artifacts.
- CTX should show how those artifacts relate.
- CTX should make the evolution of reasoning visible over time.

## Goal

Add a cognitive lineage capability that allows CTX to:

- visualize goals and subgoals
- follow the chain between tasks, hypotheses, evidence, decisions, and conclusions
- detect reasoning gaps
- inspect evolution between commits
- build a view similar to a commit graph, but applied to knowledge

## Problem It Solves

CTX already persists and versions structured artifacts.

But it is still hard to visually answer:

- which goal originated this task
- which hypotheses support this decision
- which evidence supports this conclusion
- which conclusions are orphaned
- what part of the reasoning changed between two commits
- which knowledge line dominates a branch

The cognitive graph solves that.

## What the Cognitive Graph Is

It is a relational projection of the current cognitive state or a specific commit.

It should model:

- nodes
- relationships
- states
- metadata
- temporal evolution

## Primary Nodes

The base graph nodes should be:

- `Project`
- `Goal`
- `Task`
- `Hypothesis`
- `Evidence`
- `Decision`
- `Conclusion`
- `Run`
- `ContextPacket`
- `ContextCommit`

## Primary Relationships

Relationships already present in the model:

- `Project -> Goal`
- `Goal -> Task`
- `Task -> Hypothesis`
- `Hypothesis -> Evidence`
- `Hypothesis -> Decision`
- `Evidence -> Decision`
- `Decision -> Conclusion`
- `Evidence -> Conclusion`
- `Run -> ContextPacket`
- `ContextCommit -> WorkingContext snapshot`

Relationships that should be made explicit for the graph:

- `Goal -> Goal` for subgoals
- `Task -> Task` for dependencies
- `Conclusion -> Goal` for impact or closure
- `RunArtifact -> EntityReference` as a visualizable relationship
- `ContextCommit -> ContextCommit` as history and branching

## Conceptual Lineage Model

A typical knowledge line should look like:

```text
Goal
  -> Task
    -> Hypothesis
      -> Evidence
      -> Decision
        -> Conclusion
```

An operational line should look like:

```text
Task
  -> ContextPacket
    -> Run
      -> RunArtifact
        -> Decision / Evidence / Conclusion
```

A temporal line should look like:

```text
Commit A -> Commit B -> Commit C
```

## Graph Use Cases

### 1. See work structure

Example:

- what goals exist
- which tasks hang off each goal
- which tasks have no hypothesis attached

### 2. See reasoning justification

Example:

- a decision should show
  - related hypotheses
  - related evidence
  - derived conclusions

### 3. Detect gaps

Example:

- decisions without evidence
- hypotheses without tasks
- conclusions without decisions
- goals without tasks

### 4. See evolution

Example:

- what part of the graph changed between two commits
- which nodes were added, removed, or modified

### 5. Analyze a branch

Example:

- compare a reasoning line between `main` and an experimental branch

## Recommended Staged Design

## Stage 1: Graph Export

Goal:

- generate an exportable projection without relying on a UI yet

Suggested commands:

- `ctx graph export --format json`
- `ctx graph export --format mermaid`
- `ctx graph export --format dot`
- `ctx graph export --commit <commitId>`

Suggested JSON output:

```json
{
  "nodes": [],
  "edges": [],
  "metadata": {}
}
```

Suggested base structure:

- `nodes`
  - `id`
  - `type`
  - `label`
  - `state`
  - `metadata`
- `edges`
  - `from`
  - `to`
  - `relationship`
  - `metadata`

Value:

- enables visualization with external tools
- keeps frontend independence

## Stage 2: CLI Inspection

Goal:

- query the graph from the terminal

Suggested commands:

- `ctx graph show`
- `ctx graph focus --goal <id>`
- `ctx graph focus --task <id>`
- `ctx graph lineage --hypothesis <id>`
- `ctx graph lineage --decision <id>`
- `ctx graph diff <commitA> <commitB>`

Value:

- fast local analysis
- usable in automation

## Stage 3: Interactive Visualization

Goal:

- navigate the graph visually

Desired capabilities:

- zoom and pan
- filters by type
- filters by state
- color by branch or state
- commit view
- expand and collapse subtrees
- highlight changes between commits

Technical options:

- Mermaid for simple views
- Graphviz for export
- Web UI with D3, Cytoscape, or React Flow for interactive exploration

## Recommended Domain Changes

To make the graph strong, enrich key relationships.

### 1. Subgoals

Add to `Goal`:

- `ParentGoalId`

Current state:

- implemented in the domain and represented in the graph as `subgoal`

This enables:

- goals
- subgoals
- goal trees

### 2. Task dependencies

Add to `Task`:

- `DependsOnTaskIds`

Current state:

- implemented in the domain and represented in the graph as `depends-on`

This enables:

- visualizing blockers
- planning sequences
- representing real flows

### 3. Link conclusions to goals

Add to `Conclusion`:

- `GoalIds`
- or `TaskIds`

This enables:

- showing which goal is supported by a conclusion

### 4. Typed relationships in run artifacts

Today `RunArtifact` has references. Enrich with clearer semantics:

- `supports`
- `refutes`
- `summarizes`
- `proposes`

That significantly improves the graph.

## Recommended Export Model

Conceptual example:

```json
{
  "nodes": [
    {
      "id": "goal:1",
      "type": "Goal",
      "label": "Validate V1",
      "state": "Active",
      "metadata": {
        "priority": 1
      }
    }
  ],
  "edges": [
    {
      "from": "goal:1",
      "to": "task:1",
      "relationship": "contains"
    }
  ],
  "metadata": {
    "branch": "main",
    "headCommitId": "abc123",
    "generatedAtUtc": "2026-04-08T18:00:00Z"
  }
}
```

## Recommended Initial Visualizations

The first useful views:

### View 1: Work hierarchy

```text
Project
  -> Goals
    -> Tasks
```

### View 2: Reasoning chain

```text
Task
  -> Hypothesis
    -> Evidence
    -> Decision
      -> Conclusion
```

### View 3: Temporal lineage

```text
Commit
  -> Commit
  -> Commit
```

### View 4: Operational lineage

```text
ContextPacket
  -> Run
    -> Artifact
```

## Graph-Derived Metrics

Once implemented, the graph enables valuable metrics:

- goals without tasks
- tasks without hypotheses
- hypotheses without evidence
- decisions without evidence
- conclusions without decisions
- orphan nodes
- average reasoning depth
- graph density
- justification coverage

This can become a strong product value.

## Relationship to the Product

This is not a cosmetic extra.

It is part of CTX's core value because it:

- makes structured knowledge visible
- differentiates CTX from a simple prompt history
- enables reasoning audits
- connects commits to knowledge
- opens the door to a powerful future UI

## Recommended Roadmap

Suggested order:

1. document the graph model
2. add missing relationships to the domain
3. implement `ctx graph export --format json`
4. implement `ctx graph export --format mermaid`
5. implement `ctx graph lineage`
6. add an interactive visual view later

## Recommended Decision

Yes, CTX should formally include a cognitive graph and lineage.

Not as a replacement for the structured repository, but as a high-value projection on top of it.

