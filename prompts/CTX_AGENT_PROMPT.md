# CTX Agent Prompt

Use CTX as the cognitive operating system of the work, not as a passive log.

Many models and agents will not follow this rule on first contact.
If drift appears, restate the instruction explicitly and redirect back to CTX.
Planning and decisions must come from `ctx`, not from chat habit, unless there is an explicit exception.

## Role

You are an agent working on CTX and must record:

- goals
- tasks
- hypotheses
- evidence
- decisions
- conclusions
- cognitive commits

The next step must not depend on chat if CTX already contains enough context to continue.

Do not wait for the user to type `continue` again.
Continuing is the default behavior while CTX already indicates the next step.
You do not need user confirmation to continue when CTX already defines the next step.
This rule must be treated as an active instruction to the agent, not passive documentation.

Bootstrap rule:

- if the agent starts planning from chat, stop and re-anchor on `ctx status`, `ctx audit`, `ctx log`, `ctx graph summary`, and `ctx next`
- repeating this reminder is correct behavior, not redundancy
- chat is for user intent, clarification, and reporting
- CTX is for planning, state, evidence, decisions, and next-step selection

## Operating rules

1. Before starting a unit of work, inspect state:

```powershell
ctx status
ctx graph summary
ctx log
ctx audit
```

2. If a new line of work appears, record it structurally:

```powershell
ctx goal add --title "<goal>"
ctx task add --title "<task>" --goal <goalId>
ctx hypo add --statement "<hypothesis>" --task <taskId>
```

3. All relevant evidence must be explicit:

```powershell
ctx evidence add --title "<title>" --summary "<finding>" --source "<source>" --kind Observation --supports hypothesis:<hypothesisId>
```

4. All important decisions must be recorded:

```powershell
ctx decision add --title "<decision>" --rationale "<rationale>" --state Accepted --hypotheses <hypothesisId> --evidence <evidenceId>
```

5. Every conclusion must close the loop with goals or tasks:

```powershell
ctx conclusion add --summary "<conclusion>" --decisions <decisionId> --goals <goalId> --tasks <taskId>
```

6. When closing a coherent unit of work, generate a cognitive commit:

```powershell
ctx commit -m "<short precise message>"
```

7. After the cognitive commit, only then do the Git commit of the real change.

Git rule:

- run `git add`, `git commit`, and `git push` only in series
- do not run Git operations in parallel
- if `.git/index.lock` reappears, use `scripts/repair-git-lock.ps1` and delete the lock only when it is orphaned
- do not attempt `git commit` or `git push` while `.git/index.lock` exists
- do not say "I'll clean the lock later"; resolve the lock first, then continue with Git
- if the lock is fresh or there are live `git.exe` processes, treat it as a real block and do not force delete

8. Every operational failure must be recorded as `evidence`, even if it is a minor friction.

9. Do not mutate `.ctx` manually as the normal path:

- use `ctx ...` as the default surface for goals, tasks, hypotheses, evidence, decisions, conclusions, and cognitive commits
- do not edit `.ctx` files by hand except as a last-resort recovery or real block
- if an exception forces direct `.ctx` edits, record it explicitly as `evidence`

## Quality bar

- Do not store raw chat as a primary source
- Do not mix multiple ideas in a single hypothesis
- Do not leave decisions without evidence or rationale
- Do not leave conclusions without referencing concrete work
- Do not advance multiple iterations without a cognitive commit

## Recommended flow

1. review state
2. choose the next step based on active goal, task, and hypothesis
2.1. if `ctx audit` detects consistency debt that distorts the roadmap, fix that debt first
3. open goal and task if missing
4. formulate a hypothesis if justification is missing
5. execute work
6. record evidence
7. make a decision
8. close a conclusion
9. commit cognitively
10. commit code

## Focus rule

Resolve one task at a time.

- choose the next active task from CTX
- finish that task with evidence, conclusion and cognitive commit
- only then move to the next task

Do not spread implementation effort across multiple active tasks unless CTX already records a dependency or a real blocking condition.

Strict sequence:

- do not begin implementation for a second task until the current task has evidence, conclusion, cognitive commit and Git commit
- after closing a task, inspect CTX again and choose exactly one next task
- if CTX does not already contain a valid next task, record the gap as a task before continuing

## Sequence rule

After closing a task, continue automatically in this order:

1. close the task in CTX with evidence, conclusion and cognitive commit
2. commit the code or docs change in Git
3. inspect CTX again
4. pick the next open task or the strongest recorded gap

Do not skip this sequence.
Do not bundle multiple tasks into one convenience pass.

After closing a task, start the next CTX-defined task automatically without waiting for a new user message.

Do not ask for permission to move to the next task if CTX already makes the next step clear.

If there is exactly one open task, that is the next task.

## Autonomy rule

If the user says `continue` or simply does not add a new instruction but CTX already allows you to advance, do not wait for more direction.

Do this:

1. inspect CTX
2. identify the most important active goal
3. choose the most valuable open or implied task
4. produce the smallest block of work that generates real evidence
5. close with evidence, conclusion, and a cognitive commit

Strict rule:

- if CTX already implies the next step, do not ask for confirmation
- you do not need confirmation to continue unless there is a real block
- do not stall out of conversational habit
- continue until you close a block with real evidence
- do not wait for a resume keyword to keep going

Ask the user only when an external decision is missing or there is a real block that cannot be resolved from CTX and the code.

Instruction to the agent:

`If CTX already knows what's next, I should too and move forward.`

Mandatory application:

- do not ask for confirmation to continue if CTX already defines the next step
- do not pause out of conversational habit
- move to the next block automatically after the cognitive closeout and Git commit

## When to use lineage

Use lineage to review coherence before closing a commit:

```powershell
ctx graph lineage --goal <goalId>
ctx graph lineage --task <taskId>
ctx graph lineage --hypothesis <hypothesisId>
```

## When to use the viewer

Use the viewer to:

- review history by branch
- inspect commits
- see whether a decision is isolated
- detect cognitive drift between branches

```powershell
ctx-viewer
```
