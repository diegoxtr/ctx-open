# OperationalRunbook Design
If a language model and its agent lose context, this is the tool you need.

## Goal

Define a compact first-class entity for recurring operational knowledge that can enter CTX packets without materially inflating context cost.

`OperationalRunbook` is meant to capture:

- recurring procedures
- recurrent troubleshooting
- operational policies
- guardrails that should be applied before execution drifts

It is not a replacement for tasks, docs, or scripts.
It is a compact operational layer that points agents toward the canonical path before they improvise.

## Why CTX needs this

Today CTX already stores:

- active cognitive work in `working`
- durable reasoning state through goals, tasks, hypotheses, evidence, decisions, and conclusions
- prompts, scripts, and docs outside the packet model

What is still missing is a structured surface for recurring operational knowledge such as:

- how to publish locally
- when Git closeout is allowed
- how to react to `.git/index.lock`
- how to validate the local viewer after publish

Those things should not live only as long prose in docs, and they should not be re-discovered from chat.

## Versioning model

`OperationalRunbook` is not part of mutable `WorkingContext`.

Instead, CTX versions it through `RepositorySnapshot`:

- `working-context.json` stays focused on active cognitive execution state
- `.ctx/runbooks/` stores stable operational memory on disk
- `ContextCommit.Snapshot` now captures both:
  - `WorkingContext`
  - `Runbooks`

This lets CTX version recurring operational memory without polluting the in-progress workspace model.

## Design rule

`OperationalRunbook` must stay:

- compact
- descriptive
- cheap to select
- cheap to inject into packets

If a runbook becomes long, the runbook should summarize and point to canonical references instead of duplicating them.

## Minimal entity

Proposed fields:

- `Id`
- `Title`
- `Kind`
- `Triggers`
- `WhenToUse`
- `Preconditions`
- `Do`
- `Verify`
- `FailureSignals`
- `EscalationBoundary`
- `References`
- `GoalIds`
- `TaskIds`
- `State`
- `Trace`

### `Title`

Short operator-facing name.

Examples:

- `Local publish`
- `Git closeout`
- `Recover index.lock`

### `Kind`

Allowed minimal values:

- `Procedure`
- `Troubleshooting`
- `Policy`
- `Guardrail`

### `Triggers`

Compact activation strings.

Examples:

- `publish-local`
- `git-commit`
- `git-push`
- `index.lock`
- `viewer`

No complex matching DSL is needed in the first version.

### `WhenToUse`

One short sentence describing activation.

Example:

- `Use when publishing the local CLI or viewer build.`

### `Do`

Short ordered list of actions.

Hard guideline:

- prefer `3-5` items
- each item should stay short
- use canonical commands or paths, not long prose

### `Preconditions`

Short ordered list of conditions that should already be true before the runbook is followed.

Hard guideline:

- prefer `2-4` items
- keep them binary and checkable
- use them to stop drift before execution starts

### `Verify`

Short ordered list of checks that confirm the runbook was applied correctly.

### `FailureSignals`

Short list of concrete symptoms that activate troubleshooting or guardrail behavior.

Examples:

- `.git/index.lock`
- `Failed to copy Ctx.Viewer.exe`
- `127.0.0.1:5271 does not respond`

### `EscalationBoundary`

Short list describing when the runbook should stop and hand control back to the operator instead of continuing to force recovery.

Examples:

- `Do not delete the lock if git.exe is still running`
- `Do not keep retrying publish while the installed binary is still in use`

### `References`

Canonical supporting paths or commands.

Examples:

- `docs/LOCAL_CTX_INSTALLATION.md`
- `scripts/publish-local.ps1`
- `ctx audit`
- `ctx closeout`

### `GoalIds` and `TaskIds`

Minimal explicit scoping:

- empty + empty = global runbook
- `GoalIds` = strategic/tactical scope
- `TaskIds` = exact execution scope

### `State`

Minimal lifecycle:

- `Active`
- `Archived`

## What an OperationalRunbook is not

It is not:

- a replacement for `Task`
- a replacement for `Evidence`
- a long-form procedural document
- a historical record of what happened in one execution

Rules:

- use `Task` for executable work
- use `Evidence` for observed facts
- use docs/scripts for the canonical detailed procedure
- use `OperationalRunbook` for compact reusable operational guidance

## Packet injection policy

The packet should not include every matching runbook.

Default hard limit:

- include at most `2` runbooks in the main packet

Reason:

- lower token cost
- lower instruction interference
- better operator focus

## Selection order

When more than one runbook matches, rank them in this order:

1. exact `TaskId` match
2. `GoalId` match
3. exact trigger match against packet purpose
4. `Guardrail` before `Procedure` when operational risk exists
5. `Troubleshooting` only when a relevant failure signal exists
6. stable manual priority or deterministic title ordering as final tie-break

## Overflow handling

If more runbooks match than the packet limit allows:

- inject the top `2`
- keep the rest out of the main packet body
- expose the remainder as `available runbooks`

Compact packet pattern:

```text
Operational Runbooks
- Local publish
  When: publishing the local CLI or viewer build
  Preconditions: release build exists; installed binaries are not locked
  Do: run scripts/publish-local.ps1; verify C:\ctx outputs; validate installed viewer
  Verify: ctx audit clean; local viewer responds
- Git closeout
  When: before git commit or git push
  Preconditions: ctx audit clean; closeout reviewed
  Do: run ctx closeout; ensure no .git/index.lock; commit CTX before Git
  Verify: git status clean; CTX clean
  Escalate: switch to lock recovery if index.lock appears

Additional runbooks available: Recover index.lock
```

This preserves discoverability without paying the full context cost.

## Failure-driven activation

Some runbooks should never enter the packet by default.

Example:

- `Recover index.lock` should enter only when a lock exists or a relevant Git failure was observed

That keeps troubleshooting dormant until it is actually needed.

## Persistence direction

To keep operational knowledge distinct from mutable cognitive work, the preferred storage direction is:

- `.ctx/runbooks/`

This keeps runbooks separate from `working-context.json` while still making them available to packet construction.

## First runbooks CTX should likely ship

- `Local publish`
- `Git closeout`
- `Recover index.lock`
- `Viewer local validation`

## Implementation stance

This document defines the minimum high-value version:

- compact entity
- compact packet section
- deterministic ranking
- hard overflow limit
- canonical references instead of duplicated prose

Anything more complex should only be added if real usage proves the compact model insufficient.
