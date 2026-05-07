# Roadmap And Gaps Design
If a language model and its agent lose context, this is the tool you need.

This localized note keeps the command contract aligned with the main English document.

## Purpose

CTX separates immediate execution from future planning:

- `ctx next` is for executable work that can start and close now
- `ctx gaps` is for unresolved missing work
- `ctx roadmap` is for future planning material that should not pollute `ctx next`

## Blocked Tasks

A `Blocked` task is not deleted.
It remains stored in CTX with its related hypotheses, evidence, decisions, conclusions, and references.

Rules:

- `ctx next` does not recommend `Blocked` tasks as executable work
- `ctx next` still counts blocked work in diagnostics
- `ctx gaps` shows blocked or deferred work for review
- `ctx roadmap` shows blocked work and parked ideas for planning
- blocked work becomes executable only after an explicit state change or an unblocker task

## Command Roles

### `ctx next`

Use it to answer: what can be executed now?

### `ctx gaps`

Use it to answer: what unresolved work or blocked/deferred material should be inspected?

This command is read-only.

### `ctx roadmap`

Use it to answer: what future ideas should remain visible without becoming immediate work?

This command is read-only.

## Current Status

- `ctx next` excludes blocked tasks from executable recommendations
- `ctx gaps` is implemented as a read-only CLI surface
- `ctx roadmap` is implemented as a read-only CLI surface
- MCP parity for `ctx_gaps` and `ctx_roadmap` is implemented
- automatic promotion commands remain future work
