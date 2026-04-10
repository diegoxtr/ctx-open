# CTX Autonomous Operation Protocol
If a language model and its agent lose context, this is the tool you need.

## Objective

This document defines how a model must work on CTX without relying on constant manual direction from the user.

The idea is simple:

- CTX is not a post-hoc log
- CTX is the cognitive system that defines what comes next

The model must advance based on:

- goals
- tasks
- hypotheses
- evidence
- decisions
- conclusions
- cognitive commits

Operational reality:

- many models do not naturally use CTX correctly on first contact
- the operator may need to repeat the instruction to use CTX
- that repetition is expected when the model drifts back into chat-led planning

## Guiding principle

The next work step must not come from chat.
It must come from the current state of the CTX repository.
The planning frame must also come from CTX, not from conversational habit.

If the model already has enough context in CTX, it must:

- inspect
- choose
- execute
- record
- close

without waiting for user instructions on each micro-decision.

If the model starts planning from chat instead of CTX, the correct correction is:

1. stop the chat-led loop
2. re-run CTX inspection
3. restate that CTX is the system of record
4. continue from CTX state

## Primary rule

Before touching code or documentation, the model must answer internally:

1. what is the most important active goal
2. which open or operative tasks hang from that goal
3. which hypotheses justify the next step
4. what evidence is missing to validate or reject those hypotheses
5. what is the smallest block that can be closed today

## No-blocking rule

If CTX already contains enough information to determine the next step, the model must not ask the user for confirmation.

It must assume operational continuity and move forward.

It does not require confirmation to continue.

It must not wait for the user to type `continue` again.

Continuity is the default state unless there is a pause, redirect, or real block.

It should only stop if one of these appears:

- destructive or risky action
- strategic product decision with real product/legal/commercial impact
- missing credentials or external access
- ambiguity that cannot be resolved by reading CTX or the code

Explicit rules:

- `if the next step is already deduced from goals, tasks, hypotheses, evidence, decisions or conclusions, do not ask`
- `if there is no real block, continue without explicit confirmation`
- `if the block is only conversational habit, ignore it and continue`
- `if real external alignment is needed, then ask`

## Mandatory operating cycle

### 1. Initial inspection

Always start with:

```powershell
ctx status
ctx graph summary
ctx log
ctx audit
ctx next
```

If extra focus is needed:

```powershell
ctx graph lineage --goal <goalId>
ctx graph lineage --task <taskId>
ctx graph lineage --hypothesis <hypothesisId>
```

## 2. Selecting the next step

The model must choose the next block with this priority:

1. open tasks that unblock the primary goal
2. highest-score or highest-impact hypotheses
3. frictions repeatedly observed in evidence
4. gaps between current conclusion and the real product

If `ctx audit` detects important inconsistency, fix that debt first if it affects the reliability of `ctx next`.

If `ctx next` already returns a valid recommendation and there is no strong contradiction in evidence or decisions, that recommendation is the default next block.

Do not open new work if there is already an active task that is sufficiently clear.

## Focus rule

Tasks must be resolved one by one.

That means:

- choose an active task from CTX
- close it with evidence, conclusion, and a cognitive commit
- only then open or execute the next

Do not spread work across multiple open tasks unless CTX explicitly records a dependency or real block.

Additional strict rule:

- do not start implementation for a second task until the current task has evidence, conclusion, cognitive commit, and Git commit
- after each task is closed, re-inspect CTX and choose exactly one next task
- if there is no valid open task, record the gap as a task before continuing

## Sequence rule

The default sequence is linear and autonomous:

1. close the current task in CTX
2. cognitive-commit that closure
3. Git-commit the real change
4. re-inspect CTX
5. take the next open task or the strongest gap if no valid task exists

Do not skip this sequence.
Do not combine two tasks into one block for convenience.

After closing a task, the model must start the next one automatically from CTX without waiting for a new user instruction.

The model must not ask permission to move to the next step when CTX already marks it.

If there is a single open task, that is the next one.

If there are several open tasks, choose in this order:

1. the one that unblocks the primary goal
2. the one that fixes repeated friction
3. the one that validates the strongest open hypothesis

Do not open a new task out of habit if CTX already has a sufficient open task.

## 3. Open context if structure is missing

If the next step does not exist in CTX, the model must create it.

Minimum:

```powershell
ctx goal add --title "<goal>"
ctx task add --title "<task>" --goal <goalId>
ctx hypo add --statement "<hypothesis>" --task <taskId>
```

Do not start important work without:

- a task
- a hypothesis or decision that justifies it

## 4. Execute work

While working, the model must:

- read current code state
- make the minimum useful change
- validate
- record results

Operational rule about `.ctx`:

- do not edit files inside `.ctx` manually as a normal path
- the default path to mutate `.ctx` must always be `ctx ...`
- editing `.ctx` directly is only allowed as a last-resort recovery or when a real block cannot be resolved through the product
- if `.ctx` is edited directly, record the exception as `evidence` and explain why the normal flow was insufficient

## 5. Record evidence

Any observation that changes work direction must be recorded as `evidence`.

This includes:

- a test that fails
- a test that passes and validates a hypothesis
- a limitation of the current model
- a UX friction
- an encoding problem
- a misused command
- drift between release and installation
- documentation ambiguity

Format:

```powershell
ctx evidence add --title "<title>" --summary "<concrete finding>" --source "<source>" --kind Observation
```

If it validates a hypothesis:

```powershell
ctx evidence add --title "<title>" --summary "<validation>" --source "<source>" --kind Experiment --supports hypothesis:<hypothesisId>
```

## 6. Record decisions

A `decision` must be recorded when the model sets a rule or selects one option among several.

Examples:

- choose a scoring formula
- prioritize viewer over CLI
- adopt a branch structure
- choose an export format

Format:

```powershell
ctx decision add --title "<decision>" --rationale "<rationale>" --state Accepted --hypotheses <id> --evidence <id>
```

## 7. Record conclusions

Each block must close with a `conclusion` that says:

- what was achieved
- what was validated
- what remains open

Format:

```powershell
ctx conclusion add --summary "<conclusion>" --state Accepted --evidence <id> --goals <goalId> --tasks <taskId>
```

## 8. Cognitive commit

When closing a coherent block:

```powershell
ctx commit -m "<block result>"
```

Rule:

- do not go too long without a cognitive commit
- each substantial block of work must end with a cognitive commit

## 9. Code commit

After the cognitive commit:

- Git-commit the real change
- push if applicable
- run Git in series, not in parallel
- if `.git/index.lock` appears, first check for live `git.exe` processes and only clear orphaned locks
- use `scripts/repair-git-lock.ps1` as a preflight when the lock reappears

Strict Git lock rule:

- do not attempt `git commit` or `git push` while `.git/index.lock` exists
- do not say "I'll clear the lock later" and continue anyway
- resolve the lock with a safe preflight first
- only then continue the Git closeout
- if the lock is fresh or there are live `git.exe` processes, do not delete it by force
- treat `.git/index.lock` as a real operational block until proven orphaned

Recommended order:

1. validate work
2. evidence
3. decision or conclusion
4. cognitive commit
5. Git commit
6. push

Recommended Git closeout preflight:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\repair-git-lock.ps1
git add ...
git commit -m "..."
git push origin main
```

## How to choose whatâ€™s next without user help

If the user says `continue`, or even if the user adds no new instruction but CTX already allows continuation, use this algorithm:

1. inspect `ctx status`, `ctx log`, `ctx graph summary`, `ctx audit`
2. identify the most active or strategic goal
3. choose an open or implied task that:
   - increases product value
   - reduces friction
   - validates an important hypothesis
4. verify that the task is already represented in CTX
5. if not, create it
6. execute the smallest block that produces real evidence
7. close with evidence, conclusion, and cognitive commit

## When NOT to ask the user

No need to ask for:

- choosing the next obvious bug or improvement
- recording evidence
- closing cognitive commits
- improving necessary documentation
- continuing an already-open line in CTX
- validating a change with build, tests, or viewer
- executing the next step already implied by CTX even if the user did not repeat it
- fixing a friction already recorded as evidence

## When to ask the user

Ask only if one of these appears:

- real product conflict between two valid directions
- need for credentials or external access
- sensitive legal or commercial decision
- destructive change or risky migration
- ambiguity that cannot be resolved by reading CTX or the code

## Correct hypothesis usage

A hypothesis is not a task.
A hypothesis explains why something is worth doing or what is expected to be demonstrated.

Correct examples:

- `A ranked list of hypotheses will improve daily planning decisions`
- `Viewer branch semantics reduce confusion during self-hosting`

Incorrect examples:

- `Implement command X`
- `Update README`

Those are tasks, not hypotheses.

## Failure rule

Every failure must be recorded.

Examples:

- command invoked incorrectly
- endpoint returns 404
- process does not start
- patch fails because of encoding
- difference between source repo and installed release

Do not store raw chat.
Store the operational fact and its impact.

## Priority rule

If there are multiple options, prioritize:

1. what unblocks future work
2. what validates a strong hypothesis
3. what improves product usability
4. what reduces repeated friction
5. what improves CTXâ€™s ability to work on itself

## Final meta

The model must behave as an autonomous CTX operator.

That means:

- read cognitive state
- work from that state
- produce evidence
- leave traceability
- decide the next step with judgment

Do not wait for manual direction when the CTX repository already contains enough information to continue.

Do not wait for a resume keyword to proceed.

## Permanent operator instruction

The operator must treat this protocol as an active instruction over its own behavior.

It is not enough to document it:

- it must be internalized
- it must be re-read when the model notices it is asking for unnecessary confirmations
- it must correct that pattern immediately and continue from CTX

Control phrase:

`If CTX already knows what's next, I should too and move forward.`

