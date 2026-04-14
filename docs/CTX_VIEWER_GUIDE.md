# CTX Viewer Guide
If a language model and its agent lose context, this is the tool you need.

## What it is

`CTX Viewer` is a visual interface for inspecting a `.ctx` repository.

It is not an editor. Its main job is to help you see:

- cognitive history over time
- the branch you are looking at
- the selected commit or snapshot
- the graph of goals, tasks, hypotheses, evidence, decisions, and conclusions
- the current hypothesis ranking

Think of it as a mix between:

- a version-control style history
- a cognitive graph inspector

## What each area of the screen means

The screen is divided into three main zones.

### 1. Top bar

Elements:

- `Repository`
- `Branch`
- `Load` button
- `Refresh` button
- `Auto-refresh` toggle

What each does:

- `Repository`: local path where a `.ctx/` folder exists
- `Branch`: the cognitive branch you want to inspect
- `Load`: loads the repository and refreshes the entire view
- `Refresh`: re-reads the current repository without reloading the page
- `Auto-refresh`: automatically reloads the view every few seconds

Current default behavior:

- if no repository path is stored or entered, the viewer first checks `CTX_VIEWER_DEFAULT_REPOSITORY_PATH` or `Viewer__DefaultRepositoryPath`
- if no configured default exists, the viewer resolves the default root from the nearest project `.git` directory
- in this self-hosting repository that fallback root resolves to `C:\sources\ctx-public`
- default cognitive branch is `main`
- `Auto-refresh` starts enabled by default unless the browser already remembers it being turned off
- it remembers the last `Repository` and `Branch` used in the browser
- it also remembers the `Auto-refresh` preference
- side panels can be resized with vertical dividers, and widths are saved per mode (`History`, `Split`, `Graph`)
- side panels can also collapse into compact rails from the vertical dividers, and that collapsed state is saved per mode

Example repositories:

- `C:\sources\ctx-public\examples\viewer-demo`
- `C:\ctx\workspace\ctx-self-host`

Example override:

```powershell
$env:CTX_VIEWER_DEFAULT_REPOSITORY_PATH = "C:\ctx\workspace\ctx-self-host"
dotnet run --project .\Ctx.Viewer --urls http://127.0.0.1:5271
```

## 2. Left panel

This panel summarizes overall state and then shows the timeline.

### Summary cards

The top cards mean:

- `Branch`: currently loaded branch
- `Head`: commit pointed to by that branch
- `Branches`: total branches in the repository
- `Timeline`: total visible commits in history
- `Open Tasks`: tasks still open
- `Closed Tasks`: tasks already closed
- `Nodes`: node count in the selected snapshot graph
- `Edges`: relationship count in that graph

### Top Hypotheses

This section shows the highest `score` hypotheses.

Each item shows:

- `score`: computed priority
- short hypothesis ID
- hypothesis text
- `p`: probability
- `i`: impact

Below the title you also see:

- `Last loaded`: when the view last loaded
- `Auto-refresh off/on`: current freshness state

Interpretation:

- if a hypothesis is on top, it is one of the most important to inspect or validate
- if `impact`, `evidenceStrength`, or `costToValidate` are `0`, the hypothesis is likely older and has not been enriched with the newer model
- main viewer surfaces now share available viewport height and keep scroll inside each panel so `History`, `Split`, and `Graph` remain balanced across modes
- when you enter `Working context`, the graph now prefers the nearest tactical line for each active task; if a task belongs to a sub-goal, that sub-goal stays visible and umbrella goals are not expanded unless the task points to them directly

### Tasks

This section explicitly shows what is being worked on and what is closed.

It is split into:

- `Active`: tasks still open
- `Closed`: tasks already `Done`

Each item shows:

- `state`
- short ID
- task title
- parent goal
- number of linked hypotheses
- dependencies if any

Each item also acts as a focus action:

- clicking a task jumps the viewer to that `Task` node
- if you were viewing an old commit, it returns to `working`
- if the graph was hidden in `History`, it switches to a graph-visible view
- if the task state was filtered out, it re-enables it so the task does not vanish from the graph

Interpretation:

- to see what is being worked on now, look at `Active`
- to see what lines are already closed, look at `Closed`
- this avoids deducing work only from commits or the graph

## 3. Right panel

The `Details` panel now uses tabs instead of stacking every surface at once:

- `Details`: selected commit metadata plus raw node detail
- `Origin`: compact `CognitiveTrigger` provenance for the current task, goal, or commit focus
- `Playbook`: compact `OperationalRunbook` guidance for the current task, goal, or commit focus
- `Hypotheses`: the current ranking plus freshness state

The active tab is remembered locally, so the viewer returns to the last inspection surface you were using.

The `Playbook` section is intentionally compact:

- it shows up to two top-matching runbooks
- it keeps `When`, `Do`, `Verify`, and `References` short
- additional matches stay collapsed under `Additional runbooks available`
- it does not turn runbooks into graph nodes; the guidance stays adjacent to the graph instead of competing with it

The `Origin` tab uses two provenance states:

- `Direct`: the current task, goal, or focus has its own trigger
- `Inherited`: the current focus is continuing the nearest matching cognitive line and reuses that origin

Because of that, repeated origin text is sometimes the correct behavior. If a task is a continuation and did not open a materially new direction, the viewer should keep showing the same inherited origin instead of inventing a new one.

Strategic-vs-tactical visibility rule:

- `Working` should visually emphasize the current `Task` and its nearest `Sub-goal`
- a parent strategic `Goal` may still be shown as context
- umbrella goals that remain active as long-lived product lanes should not dominate the working graph when no direct task in focus hangs from them
- this means an active strategic goal can still be correct in the repository while being visually de-emphasized in the viewer

### History

Below is the main repository history.

In `History`, the view is now `branch-first`.

That means you do not see a single flat timeline. Instead you see:

- a side list of `Branches`
- an `Order` selector for `Newest first` or `Oldest first`
- commit groups separated by branch
- a dense table per branch
- commit detail on click

Reading order:

1. `working` appears first
2. choose which branches to view on the side
3. each visible branch gets its own block
4. each row has fixed columns:
   - `Graph`: lanes and heads
   - `Description`: branch, primary goal, message, and a summarized cognitive path
   - `Changes`: number of cognitive entities touched and short breakdown
   - `Date`: exact timestamp
   - `Author`: who produced the cognitive commit
   - `Model`: `modelName` and `modelVersion` if present, or `not recorded`
   - `Commit`: short ID

Interpretation:

- `Changes` does not represent modified files like Git
- it represents CTX entities touched by the commit:
  - `task`
  - `hypo`
  - `ev`
  - `dec`
  - `con`

This makes history more scannable and tells you not only "when a commit happened" but also "how much the commit moved cognitively".

Each row also tries to show the most important cognitive line of the commit:

- `Goal`
- `Task`
- `Hypothesis`
- `Decision` or `Conclusion`

The idea is that you can quickly read:

- why that change existed
- which task it affected
- what the justification was
- whether the commit changes direction or closes something

### Evidence in detail

Evidence is shown collapsed in the commit detail panel.

This answers "why it was closed" without saturating the main view:

- by default you see the commit summary
- expanding `Evidence` shows findings with title and summary

The branch sidebar lets you:

- show only certain branches
- compare a primary branch against a secondary branch
- hide noisy branches
- keep history separated by real cognitive threads

The `Order` selector lets you:

- see the newest first, which is the normal daily workflow
- or invert the order to reconstruct a line from the beginning
- keep that preference stored in the browser alongside repo and branch

When you click a row:

- the commit becomes selected
- the right panel shows its full description
- message, branch, date, author, model, snapshot, parents, and cognitive diff load
- a `Cognitive Path` section appears with:
  - affected goals
  - affected tasks
  - affected hypotheses
  - affected decisions
  - affected conclusions

### Split and Graph

`Split` and `Graph` do not use the full branch-first history.

In those modes, the left panel becomes a compact commit navigator:

- keeps `Order`
- keeps branch filters
- lets you select commits
- preserves lane visuals
- reduces density to avoid stealing space from the graph

Interpretation:

- `History`: read and navigate history
- `Split`: alternate between commit and graph without losing too much width
- `Graph`: prioritize the graph and keep history as a compact selector
- the central graph shows the snapshot for the selected commit

### What `working` means

`working` is not a commit.

It represents the current `.ctx` state that has not yet been closed into a cognitive commit.

It lets you see:

- the current graph
- what you are building right now

## 3. Center panel

It is called `Trace Graph`.

It shows the cognitive graph for the selected commit or `working` state.

Above the graph there is a `Task states` filter.

Above those checkboxes there are focus presets:

- `All`
- `Working`
- `Thinking`
- `Closed`

Meaning:

- `All`: show all threads without semantic focus limits
- `Working`: show `Ready`, `InProgress`, and `Blocked`
- `Thinking`: show `Draft` and `Ready`
- `Closed`: show only `Done`

These presets are not exclusive:

- you can enable `Working` + `Thinking` together
- you can add `Closed` if you want to mix active and resolved context
- `All` returns to no focus restriction

The viewer remembers three things in the browser:

- last `Repository`
- last `Branch`
- the focus combination and state selection

So if you refresh or reload, it comes back to the same focus instead of resetting to `All`.

That filter lets you show or hide subgraphs associated with tasks in these states:

- `Draft`
- `Ready`
- `InProgress`
- `Blocked`
- `Done`

How to read it:

- with only `InProgress` and `Ready`, the graph focuses on active work
- `Working` does that in one click
- `Thinking` shows exploration and preparation
- `Working` + `Thinking` blends execution and exploration without losing manual state filtering
- disabling `Done` removes most closed reasoning
- enabling `Done` brings closed context back for review

Important:

- the filter does not delete repository information
- it only changes what part of the graph renders
- filtered tasks bring their linked hypotheses, evidence, decisions, and conclusions
- shared nodes tied to hidden tasks stop re-expanding other threads
- tasks not selected do not re-enter through shared goals, evidence, or decisions

Each column groups node types:

- `Project`
- `Goal`
- `Task`
- `Hypothesis`
- `Evidence`
- `Decision`
- `Conclusion`
- `Run`
- `ContextPacket`

Each box is a node. Each line is a relation.

Example relations:

- `Goal -> Task`
- `Task -> Hypothesis`
- `Hypothesis -> Evidence`
- `Hypothesis -> Decision`
- `Decision -> Conclusion`

If a node is a `Hypothesis`, it can show a `score` badge.
`Task`, `Decision`, and `Conclusion` nodes can show compact state badges instead, so the graph does not imply that only hypotheses carry meaningful inline metadata.

That means:

- the hypothesis already has a computed weight
- and can be compared with other hypotheses

## 4. Right panel

It has two parts.

### Commit

When you click a commit, you see:

- message
- full ID
- commit branch
- author
- model
- date
- snapshot hash
- total cognitive changes
- parents
- diff summary
- change lists by type

Parents matter because they show where the commit came from.

Clicking a parent navigates backward in history.

### Model metadata

CTX supports optional metadata:

- `modelName`
- `modelVersion`

This metadata lives in `Traceability`.

Important:

- not all older commits have it
- if the runtime does not provide it, the viewer shows `not recorded`
- it can be populated today via environment variables:
  - `CTX_MODEL_NAME`
  - `CTX_MODEL_VERSION`

### Node

When you click a graph node, you see:

- the selected node
- `incoming`: relations that come into it
- `outgoing`: relations that leave it
- `connectedNodes`: directly connected nodes

This answers questions like:

- which task a hypothesis came from
- what evidence supports a decision
- what conclusion a decision produced

## How to read branches

This is the most important part.

A CTX `branch` works like a version-control branch, but applied to context.

It does not represent only different code.
It represents a different line of reasoning evolution.

## Semantic consistency between branches, hypotheses, and work

A branch defines a separate line of thought, not a type of entity.

Clear rules:

- branch = reasoning or execution trajectory (experiment, research, variant)
- task = concrete work inside that trajectory
- hypothesis = justification or expectation attached to a task
- evidence/decision/conclusion = validation and closure within that same line

Implications:

- do not open branches just for "another hypothesis"; hypotheses live inside a branch
- use branches when reasoning should not mix with `main`
- avoid mixing tasks with contradictory directions in the same branch

Recommended consistency checks:

1. `ctx audit` for structural consistency
2. `ctx graph lineage --goal <goalId>` and `ctx graph lineage --task <taskId>` to validate narrative
3. if a branch is only for a short experiment, close it with a clear conclusion and cognitive commit

Suggested naming conventions:

- `main`: primary line
- `feature/*`: product improvements
- `research/*`: validation or research
- `experiment/*`: short-lived tests

### What a branch means in CTX

A branch lets you work on a variant without immediately mixing it with another.

Examples:

- `main`: main line
- `feature/ux-timeline`: UX improvement exploration
- `research/validation`: validation or research line

In other words:

- `main` = primary state
- another branch = experiment, alternative, or separate line of work

## How a branch appears in the viewer

In the timeline:

- each commit has a badge with its origin branch
- some branches can point to the same commit
- if a branch is at `HEAD`, it appears as an additional badge

This does NOT necessarily mean multiple lines drawn like a full Git graph.

In this viewer version:

- the timeline is organized by branch
- each branch gets a visual lane or color
- the commit indicates the branch it was created on
- the viewer does not yet compute a full merge graph like Sourcetree

So when you see a branch badge:

- it tells you which line of work produced that commit

When you see multiple badges:

- it tells you multiple branches currently reference the same commit

## Simple branch example

Suppose:

1. you are on `main`
2. you make a cognitive commit
3. you create `feature/ux-timeline`
4. you switch to that branch
5. you make two commits
6. you go back to `main`

Meaning:

- `main` retains its main line
- `feature/ux-timeline` advances separately
- the viewer lets you load either branch from the `Branch` selector

When you change the `Branch` selector:

- the base branch of the timeline changes
- the displayed `Head` changes
- the default snapshot you inspect changes

## Difference between branch and commit

A common mistake is to confuse them.

- `commit`: a point-in-time reproducible snapshot of context
- `branch`: a pointer to a line of commits

In short:

- the commit is the snapshot
- the branch is the living line

## Difference between branch and working

- `branch`: recorded history
- `working`: current state not yet closed in a commit

You can be on `main`, but `working` can still include uncommitted cognitive changes.

## Recommended viewer flow

1. open the viewer
2. load the repository path
3. review the summary cards
4. check hypothesis ranking
5. select a commit of interest
6. read the commit diff
7. inspect the graph for that snapshot
8. click relevant nodes to inspect connections
9. switch branch if you want to compare a different line

## How to interpret the `viewer-demo`

In the bundled demo:

- `main` represents the main line
- `feature/ux-timeline` represents a UX improvement branch
- `research/validation` represents a validation or research line

The demo shows that CTX stores not only artifacts, but also alternative reasoning paths.

## Current viewer limitations

Today the viewer:

- loads local `.ctx` repositories
- shows timeline
- shows graph per snapshot
- shows hypothesis ranking
- supports manual refresh and optional auto-refresh
- allows commit and node navigation

Today the viewer does not yet:

- allow direct editing from the UI
- provide visual merges
- provide visual conflict resolution
- provide a full Git-graph style layout
- provide side-by-side commit comparison

## Short version

If you want a quick map:

- left: history, priorities, and open/closed tasks
- center: cognitive map
- right: exact commit and node detail

And on branches:

- a branch is a separate line of context evolution
- it is not another repository
- it is not another project
- it is another possible trajectory within the same repository

