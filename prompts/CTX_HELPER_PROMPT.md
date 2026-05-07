Project Context:
  We are continuing from previous context. Re-anchor on CTX before planning from chat.
  CTX local install root: {{ctxRoot}} (if you exported the binaries there)
  Local viewer URL: {{viewerUrl}}
  Active project root: {{projectRoot}}

Read These First:
  {{viewerGuide}}
  {{agentPrompt}}
  {{autonomousProtocol}}

Operating Reminder:
  Use those files as the operating baseline for this project.
  Align existing Playbook/runbook guidance before drifting into ad-hoc operation.
  If CTX already knows what's next, continue from CTX instead of waiting for chat.
  Respect the active repository boundary instead of assuming private and public repos are interchangeable.
  Validate locally first; propagate public release artifacts only when the active CTX task explicitly calls for it.
  Treat `Working context` as cognition in motion and `ctx commit` as a durable cognitive snapshot, not as a log of every thought.
  If a cognitive delta exists, it must be visible either in `Working context` or in `Commit history`.

Start Here:
  Decide which case you are in:
  - Existing CTX repository: `.ctx/` already exists in the project root
  - New cognitive project: `.ctx/` does not exist yet

State Model:
  - No CTX repository:
    next command -> `ctx init --name "<project>"`
  - Existing CTX repository with open work:
    next command -> `ctx plan --purpose "<current intent>"`
  - Existing CTX repository with pending cognitive changes:
    next command -> `ctx closeout`
  - Existing CTX repository with a durable boundary ready:
    next command -> `ctx commit -m "<durable result>"`
  - Existing CTX repository with no open work:
    next command -> `ctx next`, then `ctx gaps` or `ctx roadmap` if no executable work exists

Existing CTX repository:
  - run `ctx`
  - run `ctx doctor`
  - run `ctx status`
  - run `ctx audit`
  - run `ctx plan --purpose "<current intent>"`
  - read `data.runbookSuggestions` before acting
  - continue from the recommended executable line, or inspect `ctx gaps` / `ctx roadmap` when only blocked or future work remains

New cognitive project:
  - run `ctx init --name "<project>"`
  - if source material already exists, use `ctx bootstrap map` / `ctx bootstrap apply`
  - if the work is greenfield, create the first goal/task/hypothesis explicitly
  - then run `ctx plan --purpose "<current intent>"`

Minimum Operator Loop:
  - `ctx`
  - `ctx plan --purpose "Plan the next work turn"`
  - apply returned `runbookSuggestions`
  - do the work
  - `ctx closeout`
  - `ctx commit -m "<durable result>"`

Daily Self-Inspection:
  - for larger work, release passes, repo syncs, or handoffs, also run `ctx doctor`
  - when a task is known, run `ctx plan --task <taskId> --purpose "<current work>"`
  - inspect the compact packet with `ctx context --task <taskId> --purpose "<current work>"`
  - use `ctx evidence list` as inventory, then `ctx evidence show <evidenceId>` for the exact supporting record
  - use `ctx conclusion show <conclusionId>` and `ctx goal show <goalId>` for exact closure and goal records returned by CTX

Runbook Rule:
  - for `ctx plan`, `data.runbookSuggestions` is the effective playbook list for the turn
  - check Preconditions, follow applicable Do steps, validate Verify, and stop at EscalationBoundary
  - do not infer playbooks from chat memory, task titles, or stale habits

Planning Boundaries:
  - `ctx next` recommends executable work only
  - `Blocked` tasks remain preserved but are reviewed through `ctx gaps` and `ctx roadmap`
  - future ideas stay visible without competing as immediate next work

Command Surface Guardrail:
  - if a CLI command is added, renamed, removed, or its contract changes, verify docs and MCP parity
  - MCP-first agents should start with `ctx_plan`
  - MCP-first agents can inspect planning debt with `ctx_gaps` and future planning lanes with `ctx_roadmap`
  - when a command changes, update CLI help, docs, prompts, MCP wrappers, and MCP parity tests together
