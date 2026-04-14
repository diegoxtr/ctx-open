# CognitiveTrigger Design

`CognitiveTrigger` captures the compact origin that opened or redirected a cognitive line.

It is not prompt history.
It is a typed, versioned record of what materially started the line.

Examples:
- a user request
- an agent-originated continuation prompt
- a recurring issue trigger
- a runbook activation

## Minimal model

Fields:
- `Id`
- `Kind`
- `Summary`
- `Text`
- `Fingerprint`
- `GoalIds`
- `TaskIds`
- `OperationalRunbookIds`
- `State`
- `Trace`

Kinds:
- `UserPrompt`
- `AgentPrompt`
- `Continuation`
- `RunbookTrigger`
- `IssueTrigger`

## Design rules

- keep `Summary` short and always present
- keep `Text` optional and bounded
- do not create a trigger for trivial continuity messages
- create one when a message opens, redirects, or constrains a line of work

## Creation and inheritance policy

Create a new trigger when the line materially changes direction:
- a new `goal`
- a new tactical `sub-goal`
- a new top-level task that opens its own line
- a new issue framing or hard constraint
- a runbook activation that changes the direction of work

Inherit the nearest relevant trigger when the work is just continuing:
- `ok`
- `continua`
- local implementation steps
- validation or closeout inside the same line
- subtasks and dependency follow-ups

This keeps `Origin` meaningful instead of repeating low-signal continuity text.

## What repetition means in Origin

If the viewer shows the same origin text across multiple follow-up tasks, that is not necessarily a bug.

It usually means:
- the line kept the same cognitive origin
- no materially new direction was introduced
- the current task inherited the nearest relevant trigger instead of creating a new one

This is why the viewer distinguishes:
- `Direct`: the trigger belongs to the current focus
- `Inherited`: the current focus is continuing a nearby line and is reusing its origin

## Repository model

`CognitiveTrigger` is stored outside mutable `working-context.json` and versioned through `RepositorySnapshot`, alongside `OperationalRunbook`.

This keeps:
- active execution state in `WorkingContext`
- stable operational memory in `OperationalRunbook`
- origin memory in `CognitiveTrigger`

## Packet policy

Packets should include compact trigger summaries, not full transcripts.

Default packet shape:
- `Triggers`
- one or two short entries
- no full prompt dump unless explicitly needed later
