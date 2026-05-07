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
- `CTX write lock retry`

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
- `working-context`
- `ctx-write`

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

All active runbooks attached to the selected task are promoted into the main packet. The normal two-runbook packet limit applies only to fallback matches from goals, triggers, and global guardrails.

## Concept map: how a task gets a runbook

```text
Operator intent
  |
  |  creates or updates
  v
Task ---------------------------------------------------+
  |                                                     |
  | has GoalId                                          |
  |                                                     |
  v                                                     |
Goal                                                    |
  |                                                     |
  | selected by ctx plan/context/check                  |
  v                                                     |
Context packet selection                               |
  |                                                     |
  | compares active task/goal/purpose with runbooks     |
  v                                                     |
OperationalRunbook                                     |
  |                                                     |
  | matches by TaskIds, GoalIds, or Triggers            |
  v                                                     |
runbookSuggestions                                     |
  |
  | operator/agent reviews before execution
  v
Command or procedure is run deliberately
```

There are three ways a runbook becomes relevant:

- task attachment: `ctx runbook attach <runbookId> --task <taskId>`
- goal attachment: `ctx runbook attach <runbookId> --goal <goalId>`
- trigger match: the active purpose or operation contains a runbook trigger, for example `publish-local`

Task attachment is the strongest normal signal. It means the runbook is selected even when the prompt says only "continue this task" and does not mention the trigger text. Use this for task-specific playbooks instead of relying on title or purpose text.

Goal attachment is broader. It should be used for operational rules that apply to a whole work line.

Trigger matching is situational. It is useful for preflight and ad hoc operations such as:

```powershell
ctx preflight --operation publish-local
```

That command selects runbooks whose triggers include `publish-local`.

The runbook does not execute automatically. CTX surfaces it as `runbookSuggestions`, and the operator or agent deliberately runs the commands listed in `Do` after checking `Preconditions`.

## Agent handling contract

Agents should treat `runbookSuggestions` as the canonical playbook list for the current CTX packet.

Use this rule in prompts, MCP instructions, and ACP adapters:

```text
Read runbookSuggestions before acting.
For every returned runbook:
- check Preconditions
- follow applicable Do steps
- validate with Verify
- stop at EscalationBoundary if a failure signal appears
Do not infer playbooks from chat memory or titles.
```

If `runbookSuggestions` is empty, the agent continues from the recommended task, context packet, and guidance. If it is not empty, the agent should mention which playbooks apply before executing work.

For `ctx plan` specifically, `data.runbookSuggestions` is the authoritative effective playbook list for the turn.
It is selected from the focused context packet and mirrored into `data.next.runbookSuggestions` for compatibility, so agents do not need to reconcile two different runbook lists.

## Attach and verify a task runbook

Use this flow when a playbook must always follow a specific task:

```powershell
ctx runbook list
ctx runbook attach <runbookId> --task <taskId>
ctx plan --task <taskId> --purpose "continue this task"
ctx check --task <taskId>
```

The verification is intentionally plain: the purpose text does not include the runbook trigger. If the runbook still appears in `runbookSuggestions`, the task relationship is driving selection correctly.

Expected result:

- direct `TaskId` matches appear before goal, trigger, and global fallback matches
- all active runbooks attached directly to the task appear in the main suggestion list
- the same runbook can stay attached to its original task and also be attached to a newer task that reuses the procedure
- detaching removes only the selected relationship; it does not delete the runbook

```powershell
ctx runbook detach <runbookId> --task <taskId>
```

## Runtime lock guidance

CTX has two different lock classes and they should not be handled the same way:

- Git locks: `.git/index.lock` belongs to Git closeout and must use the recover-index-lock flow.
- CTX working locks: `.ctx/working/working-context.json` or `.ctx/write.lock` belongs to CTX repository persistence and should be retried serially first.

When a CTX command reports a transient `working-context.json` access failure:

1. do not edit `.ctx` by hand
2. stop overlapping CTX/Git/build/test commands that touch the same repo
3. retry the CTX command as a single serial operation
4. run `ctx audit` after the retry if the command changed state
5. only open a recovery block if the same lock persists after serial retry

The private runbook `CTX write lock retry` captures this as compact operational guidance. The persistence layer also retries transient `IOException` and `UnauthorizedAccessException` cases around `working-context.json`, so brief viewer/agent/process overlap should not require manual recovery.

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

If local publish fails with `Remove-Item` on `C:\ctx\viewer` and no `Ctx.Viewer.exe` process is visible, check for a viewer-spawned Browse directory dialog still alive as a PowerShell STA process:

```powershell
Get-CimInstance Win32_Process |
  Where-Object { $_.Name -eq 'powershell.exe' -and $_.CommandLine -like '*FolderBrowserDialog*' } |
  Select-Object ProcessId,Name,CommandLine
```

Close the visible dialog or stop only the matching process, then retry:

```powershell
Stop-Process -Id <processId> -Force
powershell -ExecutionPolicy Bypass -File .\scripts\publish-local.ps1
Start-Process http://127.0.0.1:5271
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

## Release announcement and flyer example

`Release bilingual announcement and flyer` is used after release notes are consolidated and before the release is shared.

## Release packaged console reference validation

Use this guardrail before tagging or publishing a release artifact.

The release is not ready if a console-facing command, helper, installer note, README section, or release note tells the operator to read a file that is not present in the installed package.

Required installed references:

- `prompts/CTX_HELPER_PROMPT.md`
- `prompts/CTX_AGENT_PROMPT.md`
- `docs/CLI_COMMANDS.md`
- `docs/CTX_VIEWER_GUIDE.md`
- `docs/CTX_AUTONOMOUS_OPERATION_PROTOCOL.md`

Do:

- build the portable distribution from the release branch
- inspect the bundle layout before publishing
- install from the bundle into a disposable install root
- run `ctx helper` from the installed launcher
- verify every path printed by the helper exists under the install root
- verify installer docs and release notes do not mention package-local files that are absent from the bundle

Verify:

- `docs/CLI_COMMANDS.md` exists in the portable bundle and installed root
- `ctx helper` points to files that exist after install
- Windows, Linux, and macOS install scripts copy the same console-referenced docs

Escalate:

- stop release publication if a console-referenced file is missing from the bundle or installed root
- either ship the file or remove the installed-path reference before tagging

The operator-facing output should be pasted directly in chat and be ready to share:

```text
CTX <version> is out.

English:
<Two or three short sentences describing what improved, who benefits, and why this release matters. Keep shipped scope only.>

Espanol:
<La misma descripcion en espanol, breve y equivalente a la version en ingles.>

Flyer:
CTX <version>
<One-line release thesis>

Top highlights
- <Highlight 1: the most important shipped improvement>
- <Highlight 2: the second most important shipped improvement>
- <Highlight 3: optional; include only if it is genuinely important>

Validated
- <Build/test/install/viewer validation>

Use it when
- <Main practical use case>

Link
<release or demo link>
```

Design rules:

- English goes first, Spanish second.
- Keep the two descriptions semantically equivalent.
- The flyer should feel like the UNJU talk material: direct title, clear thesis, restrained CTX/UNJU style, concrete improvements, and a practical call to action.
- The flyer must highlight only the two or three most important shipped points from the release. Prefer two strong points over three weak ones.
- Each highlight should be one line, user-facing, and specific enough to stand alone in a visual card.
- Do not include roadmap or future work as shipped functionality.

Flyer visual template:

```text
CTX / RELEASE <version>
by <author or team>

CTX <version>
<Short thesis: what this release makes possible>

<One-sentence context for why it matters.>

Highlights
1. <Most important shipped improvement>
2. <Second most important shipped improvement>
3. <Optional third shipped improvement>

Validated
<One compact validation line: tests/build/assets/docs>

Repository
<release link or repository link>
```

## Implementation stance

## Documentation consistency pass example

`Documentation consistency pass` is used when the operator says "document everything", "documenta todo", or asks for a full MD/TXT consistency pass.

Compact operational contract:

```text
Operational Runbook
- Documentation consistency pass
  When: documentation must be aligned across README, docs, prompts, examples, release notes, and localized references
  Preconditions: CTX planning is anchored; the target repository is explicit; public/private boundary is clear
  Do: inventory all .md/.txt files; scan for stale command names and path leaks; update canonical docs first; align es/zh/localized references; update release notes and CHANGELOG when release scope changes
  Verify: command docs list the shipped CLI surface; localized docs do not contradict canonical docs; relative paths or placeholders replace private hard paths; ctx audit is clean
  Escalate: stop before touching the public repo if the operator requested private-only work; stop before publishing docs that contain private paths or sensitive notes
```

Rules:

- start from the canonical English docs, then align Spanish and Chinese surfaces
- treat `README.md`, `docs/CLI_COMMANDS.md`, `docs/TECHNICAL_INDEX.md`, `docs/OPERATIONAL_RUNBOOKS.md`, `CHANGELOG.md`, and the current `docs/RELEASE_*.md` as mandatory review files
- include examples and prompts in the scan, but avoid rewriting examples unless they contradict the canonical contract
- do not claim MCP parity for CLI-only commands until MCP tools exist
- keep future work visible in `ctx roadmap` or release roadmap notes, not in executable `ctx next`

## Implementation stance

This document defines the minimum high-value version:

- compact entity
- compact packet section
- deterministic ranking
- hard overflow limit
- canonical references instead of duplicated prose

Anything more complex should only be added if real usage proves the compact model insufficient.
