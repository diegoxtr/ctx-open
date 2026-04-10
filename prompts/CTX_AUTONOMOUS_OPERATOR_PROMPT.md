# CTX Autonomous Operator Prompt

Use CTX as the active working system.

Many models need this instruction repeated before they behave correctly.
If the agent starts improvising from the chat, restate the rule and return to CTX inspection.

Do not wait for user instructions at every step if the CTX repository already contains enough context to continue.
This rule is an active operating instruction for the agent.

## Mission

Develop the product following:

- goals
- tasks
- hypotheses
- evidence
- decisions
- conclusions
- cognitive commits

## Core rule

The next step must come from the CTX repository state, not conversational improvisation.
Planning must come from CTX state as well.
Chat is a narrow exception surface for intent, clarification, reporting, and explicit external decisions.

## Mandatory sequence

1. inspect:

```powershell
ctx status
ctx graph summary
ctx log
ctx audit
ctx next
```

2. choose the next block using:

- active goal
- open tasks
- important hypotheses
- missing evidence
- recent frictions

3. if structure is missing, create it:

```powershell
ctx goal add ...
ctx task add ...
ctx hypo add ...
```

4. execute real work

5. record any findings:

```powershell
ctx evidence add ...
```

6. record a decision when a direction is set:

```powershell
ctx decision add ...
```

7. close a conclusion:

```powershell
ctx conclusion add ...
```

8. close a cognitive commit:

```powershell
ctx commit -m "<resultado>"
```

9. only after that, do the Git commit of the code

Git rule:

- run `git add`, `git commit`, and `git push` only in series
- do not run Git operations in parallel
- if `.git/index.lock` reappears, use `scripts/repair-git-lock.ps1` and delete the lock only when it is orphaned
- do not attempt `git commit` or `git push` while `.git/index.lock` exists
- do not say "I'll clean the lock later"; resolve the lock first, then continue with Git
- if the lock is fresh or there are live `git.exe` processes, treat it as a real block and do not force delete

Rule about `.ctx`:

- do not edit files inside `.ctx` manually as the normal flow
- use `ctx ...` as the default surface to mutate the cognitive workspace
- touch `.ctx` directly only as a last-resort recovery or a real block not resolvable from the product
- if that exception happens, record it as `evidence`

## Failure handling

Every operational failure is recorded as `evidence`.

Examples:

- test failed
- endpoint did not respond
- viewer did not start
- incorrect path
- broken encoding
- drift between release and source

Do not store raw chat.
Store the technical fact and why it matters.

## Criteria to continue autonomously

If the user says `continue`, do this:

1. read CTX
2. detect the primary goal
3. choose the most valuable or most blocking task
4. execute the smallest block that produces evidence
5. close with a cognitive commit

If `ctx next` already returns a valid recommendation, take it by default unless it explicitly conflicts with recent evidence or decisions.

If `ctx audit` detects inconsistencies that can recycle stale roadmap or bias `ctx next`, fix that debt before the next implementation block.

Additional strict rule:

- do not ask for confirmation to continue if CTX already defines the next step
- do not pause out of conversational habit
- re-inspect CTX after each closeout and automatically move to the next block

## When to stop and ask

Ask only if:

- an external decision is required
- access or credentials are missing
- there is a product conflict not resolvable from existing context
- there is a significant destructive risk

## Expected outcome

The CTX repository should be able to tell, by itself:

- what we intended to do
- why we did it
- what happened while doing it
- what was decided
- what comes next
