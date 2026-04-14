# CTX Installation and Usage Guide
If a language model and its agent lose context, this is the tool you need.

## 1. Objective

This guide explains how to install, build, and use CTX for the first time in a local environment.

Intended for:

- technical testers
- developers
- architects
- early V1 users

## 2. Requirements

Before starting, verify:

- Windows with a terminal available
- .NET SDK 8 installed
- access to the repository source code
- read/write permissions in the working folder

Optional:

- `OPENAI_API_KEY`
- `ANTHROPIC_API_KEY`

If keys are not configured, CTX can run in offline fallback mode for functional testing.

## 3. Verify .NET installation

Run:

```powershell
dotnet --version
```

Expected:

- version `8.x`

You can also validate:

```powershell
dotnet --list-sdks
```

## 4. Get the code

If you already have the repository, move to the project root.

Example:

```powershell
cd C:\sources\ctx
```

## 5. Restore, build, test

From the repo root run:

```powershell
dotnet restore Ctx.sln
dotnet build Ctx.sln
dotnet test .\Ctx.Tests\Ctx.Tests.csproj
```

Expected:

- restore succeeds
- build succeeds
- tests succeed

## 6. Run the CLI

Run the CLI directly:

```powershell
dotnet run --project .\Ctx.Cli -- status
```

If there is no cognitive repository in the current folder, run `init` first.

## 7. Create a cognitive repository

Move to a workspace folder and run:

```powershell
dotnet run --project C:\sources\ctx\Ctx.Cli -- init --name "CTX-DEMO" --description "First cognitive repo"
```

Expected:

- `.ctx/` is created
- config files are generated
- current branch is `main`

Verify:

```powershell
dotnet run --project C:\sources\ctx\Ctx.Cli -- status
```

## 8. First recommended flow

### Step 1 - Create a goal

```powershell
dotnet run --project C:\sources\ctx\Ctx.Cli -- goal add --title "Define testing strategy" --description "Prepare a technical pilot"
```

### Step 2 - Create a task

```powershell
dotnet run --project C:\sources\ctx\Ctx.Cli -- task add --title "Evaluate CLI flow" --description "Validate core commands"
```

### Step 3 - Create a hypothesis

```powershell
dotnet run --project C:\sources\ctx\Ctx.Cli -- hypo add --statement "Structured flow improves traceability" --rationale "State is persisted in artifacts"
```

### Step 4 - Record evidence

```powershell
dotnet run --project C:\sources\ctx\Ctx.Cli -- evidence add --title "Initial test" --summary "Structure helps resume context" --source "manual evaluation" --kind Observation --supports hypothesis:<hypothesisId>
```

### Step 5 - Record a decision

```powershell
dotnet run --project C:\sources\ctx\Ctx.Cli -- decision add --title "Use CTX in pilot" --rationale "Traceability is sufficient for pilot" --state Accepted --hypotheses <hypothesisId> --evidence <evidenceId>
```

### Step 6 - Record a conclusion

```powershell
dotnet run --project C:\sources\ctx\Ctx.Cli -- conclusion add --summary "Approve internal pilot usage" --state Accepted --decisions <decisionId> --evidence <evidenceId>
```

### Step 7 - Execute a run

```powershell
dotnet run --project C:\sources\ctx\Ctx.Cli -- run --provider openai --purpose "Review pilot risks"
```

### Step 8 - Create a cognitive commit

```powershell
dotnet run --project C:\sources\ctx\Ctx.Cli -- commit -m "first end-to-end flow"
```

## 9. Useful commands

### State and navigation

```powershell
dotnet run --project .\Ctx.Cli -- status
dotnet run --project .\Ctx.Cli -- log
dotnet run --project .\Ctx.Cli -- diff
```

### Cognitive artifacts

```powershell
dotnet run --project .\Ctx.Cli -- goal list
dotnet run --project .\Ctx.Cli -- task list
dotnet run --project .\Ctx.Cli -- hypo list
dotnet run --project .\Ctx.Cli -- decision list
dotnet run --project .\Ctx.Cli -- evidence list
dotnet run --project .\Ctx.Cli -- conclusion list
```

### Operational inspection

```powershell
dotnet run --project .\Ctx.Cli -- provider list
dotnet run --project .\Ctx.Cli -- run list
dotnet run --project .\Ctx.Cli -- packet list
dotnet run --project .\Ctx.Cli -- metrics show
```

### Branching and merge

```powershell
dotnet run --project .\Ctx.Cli -- branch feature-x
dotnet run --project .\Ctx.Cli -- checkout feature-x
dotnet run --project .\Ctx.Cli -- merge main
```

## 10. Generated local structure

When initializing a repo, CTX creates:

- `.ctx/version.json`
- `.ctx/config.json`
- `.ctx/project.json`
- `.ctx/HEAD`
- `.ctx/branches/`
- `.ctx/commits/`
- `.ctx/graph/`
- `.ctx/working/`
- `.ctx/staging/`
- `.ctx/runs/`
- `.ctx/packets/`
- `.ctx/index/`
- `.ctx/metrics/`
- `.ctx/providers/`
- `.ctx/logs/`

## 11. Usage recommendations

- keep each test case in a separate folder
- use clear cognitive commit messages
- record evidence before accepting important decisions
- use `packet list` and `run list` to review iterations
- review `metrics show` at the end of each scenario
- if cognitive conflicts appear, do not ignore them; review merge output before continuing

## 12. Common issues

### Build fails

Check:

- `dotnet --version` is `8.x`
- `global.json` exists
- restore completed

### No provider keys

Not a blocker:

- CTX can run with offline fallback

### IDs are confusing

Recommendation:

- use `list` to locate entities
- use `show` to inspect details
- copy the `id.value` from the needed artifact

### Merge conflicts

Interpretation:

- the system detected divergence in the same cognitive artifact
- review artifacts before accepting the merge

## 13. Correct usage criteria

A user operated CTX correctly if they:

- initialized a repository
- created cognitive artifacts
- executed at least one `run`
- created at least one cognitive commit
- inspected results from the CLI

## 14. Next recommended reading

Continue with:

- `docs/V1_PLAN.md`
- `docs/PILOT_TESTING_GUIDE.md`
- `README.md`

