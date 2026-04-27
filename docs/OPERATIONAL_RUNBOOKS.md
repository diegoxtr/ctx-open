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

## Example: Public install bootstrap

Compact example:

```text
Operational Runbook
- Public install bootstrap
  When: a user clones the repository and runs install.ps1 or install.sh
  Preconditions: network access exists; the target platform has a published portable asset
  Do: run the single-entry install script from the repo root; let the bootstrap resolve the latest published release asset; use source mode only if explicitly requested
  Verify: ctx installs without requiring a local source build; install metadata reports the published version
  Escalate: stop if the platform has no published asset or the release metadata is missing the matching bundle
```

Design rule:

- clone-first installation must still resolve to published portable assets by default
- a local repository checkout is not, by itself, a request to compile CTX from source

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

- `CTX planning first`
- `State-driven CTX startup`
- `Local publish`
- `Git closeout`
- `Recover index.lock`
- `Viewer local validation`
- `PowerShell command chaining constraints`

## State-driven startup example

One runbook should teach startup in the same way as the bare `ctx` helper.

- trigger: `startup`, `helper`, `onboarding`
- when:
  use when an operator or agent needs to know the next CTX command without reading the full command surface
- do:
  - run `ctx`
  - read `Current State`
  - execute the printed `Next Command`
  - if the repo is new, initialize it first
  - if the repo already exists, stay in the loop:
    `ctx next -> work -> ctx closeout -> ctx commit -m "..."`
- verify:
  - helper, README, CLI docs, and operator protocol all describe the same state-driven loop
  - the agent can distinguish:
    - no CTX repo
    - open work
    - pending cognitive delta
    - durable boundary
    - no open work

This keeps onboarding compact and removes the need to memorize a large command list before the first useful action.

## Planning guardrail example

Another recurring operational guardrail is now explicit:

- CTX planning goes first, always
- open or update the active task in CTX before implementation, release, sync, or recovery work starts
- do not rely on chat-only intent when a CTX workspace is available

This is not process theater. It prevents work from starting outside durable cognitive state and avoids losing the active thread once execution begins.

## Repository guardrail example

One recurring repository guardrail is already worth making explicit:

- in this PowerShell host, do not use bash-style `&&`
- run `git add`, `git commit`, `git push`, and any `ctx` command as separate statements
- if the workflow touches public synchronization or Git closeout, keep the whole sequence strictly serial

This is not cosmetic shell preference. It is a real operational constraint that has already caused repeated closeout failures when ignored.

## Viewer validation guardrail example

`Viewer local validation` now includes a version-drift check:

- first verify `/api/overview` from `127.0.0.1:5271`
- if `productVersion` already matches the expected release, the remaining mismatch is a browser cache or stale-tab problem
- only recycle the local viewer process when the backend still reports the old version
- remember that the installed viewer runtime is `C:\ctx\viewer\Ctx.Viewer.exe`, not the repo build output
- if the topbar still shows an older release while `/api/overview` already reports the new version, hard-refresh or reopen the tab before assuming the backend is stale

For the installed Windows viewer, the canonical recovery flow is:

```powershell
Get-CimInstance Win32_Process |
  Where-Object { $_.Name -like 'Ctx.Viewer*' -or $_.CommandLine -like '*Ctx.Viewer*' } |
  ForEach-Object { Stop-Process -Id $_.ProcessId -Force }

powershell -ExecutionPolicy Bypass -File .\scripts\publish-local.ps1

Start-Process -FilePath C:\ctx\bin\ctx-viewer.cmd
```

For Codespaces or manual demo recovery, a canonical rebuild sequence is:

```bash
pkill -f "Ctx.Viewer" || true
rm -rf Ctx.Viewer/bin Ctx.Viewer/obj
bash scripts/ensure-dotnet-sdk.sh
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$DOTNET_ROOT:$DOTNET_ROOT/tools:/usr/share/dotnet:$PATH"
dotnet build Ctx.Viewer/Ctx.Viewer.csproj
bash scripts/start-codespaces-demo.sh
curl -I http://127.0.0.1:5271
```

If a single paste-friendly command is needed in a GitHub Codespaces terminal, use:

```bash
git pull --ff-only origin main; pkill -f "Ctx.Viewer" || true; rm -rf Ctx.Viewer/bin Ctx.Viewer/obj; bash scripts/ensure-dotnet-sdk.sh; export DOTNET_ROOT="$HOME/.dotnet"; export PATH="$DOTNET_ROOT:$DOTNET_ROOT/tools:/usr/share/dotnet:$PATH"; dotnet build Ctx.Viewer/Ctx.Viewer.csproj; bash scripts/start-codespaces-demo.sh; curl -I http://127.0.0.1:5271
```

## Implementation stance

This document defines the minimum high-value version:

- compact entity
- compact packet section
- deterministic ranking
- hard overflow limit
- canonical references instead of duplicated prose

Anything more complex should only be added if real usage proves the compact model insufficient.
