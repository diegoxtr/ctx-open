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
  Align the existing Playbook/runbook guidance before drifting into ad-hoc operation.
  If CTX already knows what's next, continue from CTX instead of waiting for chat.
  In ctx-open, keep examples, copy, and release surfaces intentionally public and sanitized.
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
    next command -> `ctx next`
  - Existing CTX repository with pending cognitive changes:
    next command -> `ctx closeout`
  - Existing CTX repository with a durable boundary ready:
    next command -> `ctx commit -m "<durable result>"`
  - Existing CTX repository with no open work:
    next command -> `ctx next`

Existing CTX repository:
  - run `ctx`
  - run `ctx status`
  - run `ctx audit`
  - run `ctx next`
  - continue from the recommended line

New cognitive project:
  - run `ctx init --name "<project>"`
  - if source material already exists, use `ctx bootstrap map` / `ctx bootstrap apply`
  - if the work is greenfield, create the first goal/task/hypothesis explicitly
  - then run `ctx next`

Minimum Operator Loop:
  - `ctx`
  - `ctx next`
  - do the work
  - `ctx closeout`
  - `ctx commit -m "<durable result>"`
