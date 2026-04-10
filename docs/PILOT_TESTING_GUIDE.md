# CTX Pilot Testing Guide
If a language model and its agent lose context, this is the tool you need.

## 1. Objective

This guide defines how to run an initial CTX pilot to validate whether the solution is mature enough to evolve into a V1 candidate for technical users.

The goal is not only to verify that the software works, but to confirm:

- the work model is understandable
- the operational flow adds real value
- the cognitive structure improves AI usage
- the tool reduces rework
- iteration cost is reasonable

## 2. Pilot scope

The pilot should focus on:

- local CLI usage
- real or near-real technical cases
- structured recording of goals, tasks, hypotheses, evidence, decisions, conclusions
- cognitive commits
- context generation
- runs
- diff and merge review
- metrics, packets, and runs inspection

It should not yet include:

- remote integrations
- real multi-user usage
- graphical interfaces
- enterprise scenarios
- complex external automation

## 3. Recommended tester profile

Ideal tester:

- technical
- comfortable with CLI
- able to structure reasoning into steps
- able to explain why a decision was good or bad
- willing to record clear feedback

Ideally:

- architect
- senior developer
- platform engineer
- technical researcher
- frequent LLM user

## 4. Environment preparation

### 4.1 Requirements

- .NET SDK 8 installed
- CTX repo builds
- console access
- optional provider keys for real runs:
  - `OPENAI_API_KEY`
  - `ANTHROPIC_API_KEY`

If no keys, the system is still testable via offline fallback.

### 4.2 Initial verification

```powershell
dotnet build Ctx.sln
dotnet test .\Ctx.Tests\Ctx.Tests.csproj
```

Expected:

- build succeeds
- tests succeed
- no critical environment errors

### 4.3 Prepare a pilot repository

```powershell
dotnet run --project .\Ctx.Cli -- init --name CTX-PILOT --description "Initial validation pilot"
dotnet run --project .\Ctx.Cli -- status
```

Expected:

- `.ctx/` created
- `status` shows `main`
- no initialization errors

## 5. Pilot goals

During the pilot, answer:

1. Does the user understand how to model the problem?
2. Is structured cognitive flow more helpful than free chat?
3. Is it easier to resume work?
4. Are commits and diffs useful?
5. Are merges and conflicts interpretable?
6. Is context/run cost acceptable?
7. Would the user use CTX for a real case?

## 6. Recommended scenarios

### Scenario 1 - Architecture analysis

Goal:
- evaluate an architecture decision with hypotheses and evidence

### Scenario 2 - Technical investigation

Goal:
- structure a root cause investigation

### Scenario 3 - AI-guided iteration

Goal:
- measure whether CTX reduces rework across multiple AI runs

## 7. Recommended test flow

### Step 1 - Create a goal

```powershell
dotnet run --project .\Ctx.Cli -- goal add --title "Evaluate module X architecture" --description "Define primary alternative"
```

### Step 2 - Create tasks

```powershell
dotnet run --project .\Ctx.Cli -- task add --title "Analyze option A" --description "Pros and risks"
dotnet run --project .\Ctx.Cli -- task add --title "Analyze option B" --description "Pros and risks"
```

### Step 3 - Record hypotheses

```powershell
dotnet run --project .\Ctx.Cli -- hypo add --statement "Option A reduces operational complexity" --rationale "Fewer components"
dotnet run --project .\Ctx.Cli -- hypo add --statement "Option B scales better long term" --rationale "More flexibility"
```

### Step 4 - Record evidence

```powershell
dotnet run --project .\Ctx.Cli -- evidence add --title "Initial benchmark" --summary "A shows lower latency" --source "local test" --kind Benchmark --supports hypothesis:<hypothesisId>
```

### Step 5 - Make decisions

```powershell
dotnet run --project .\Ctx.Cli -- decision add --title "Adopt option A for pilot" --rationale "Lower complexity and favorable evidence" --state Accepted --hypotheses <hypothesisId> --evidence <evidenceId>
```

### Step 6 - Record a conclusion

```powershell
dotnet run --project .\Ctx.Cli -- conclusion add --summary "Proceed with A for faster validation" --state Accepted --decisions <decisionId> --evidence <evidenceId>
```

### Step 7 - Execute a run

```powershell
dotnet run --project .\Ctx.Cli -- run --provider openai --purpose "Review decision and propose risks"
```

### Step 8 - Create a cognitive commit

```powershell
dotnet run --project .\Ctx.Cli -- commit -m "pilot architecture scenario"
```

### Step 9 - Inspect results

```powershell
dotnet run --project .\Ctx.Cli -- log
dotnet run --project .\Ctx.Cli -- metrics show
dotnet run --project .\Ctx.Cli -- run list
dotnet run --project .\Ctx.Cli -- packet list
```

## 8. Tester checklist

Mark during the pilot:

- I initialized the repo without help
- I understood how to create goals, tasks, hypotheses
- I understood how to link evidence to hypotheses or decisions
- I was clear on recording decisions and conclusions
- `status`, `log`, `diff`, `metrics` output was understandable
- generated packets were useful
- run output was reusable
- cognitive commits represented the achieved state
- the flow was better than ad-hoc prompts
- I would use CTX for a real case

## 9. Evaluation criteria

### 9.1 Functional

Success if:

- flow can complete without manual JSON edits
- no blocking errors appear
- commands return consistent results
- artifacts are traceable

### 9.2 Usability

Acceptable if:

- the model is understandable with limited explanation
- the CLI does not create major confusion
- command names feel reasonable
- structure forces better thinking without too much friction

### 9.3 Value

Promising if:

- user feels less context loss
- decisions are easier to justify
- work can resume after a pause
- cognitive commits are useful
- repetition with AI is reduced

## 10. Indicators to measure

Record per scenario:

- total execution time
- command count
- goals/tasks/hypotheses/evidence/decisions/conclusions counts
- run count
- tokens used
- cost estimate
- repeated iterations
- cognitive conflicts detected
- subjective usefulness

## 11. Tester feedback template

General:

- tester name
- date
- scenario
- duration

Evaluation:

- problem worked
- objective achieved
- most useful commands
- confusing commands
- most valuable part of the flow
- most frictional part
- missing information
- missing commands/help
- would use CTX for this case: yes/no

Suggested 1â€“5 scores:

- model clarity
- ease of use
- value of structured context
- cognitive commit usefulness
- diff usefulness
- merge usefulness
- metrics usefulness
- likelihood of reuse

## 12. Post-pilot decisions

### Move to V1 candidate

If:

- no severe blockers
- testers understand the flow
- perceived value is high
- costs are reasonable

### Keep internal iteration

If:

- concept is liked
- CLI/UX still creates too much friction

### Reconsider before V1

If:

- perceived value is low
- model is unclear
- operational cost is too high
- artifacts do not improve decisions

## 13. Practical recommendation

The initial pilot should use:

- 3 scenarios
- 3â€“5 technical testers
- short duration
- mandatory written feedback
- a final review session

## 14. Expected outcome

If executed properly, you will have:

- concrete evidence of value or friction
- a real list of improvements before V1
- an objective basis to decide if CTX can move to a controlled product test

