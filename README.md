# CTX
If a language model and its agent lose context, this is the tool you need.

CTX is a Cognitive Version Control System: a CLI for structured reasoning artifacts instead of chat transcripts.

Current version: `1.0.1`

## For Models And Agents

Models and agents do not automatically know how to work with CTX on first contact.
In practice, the operator often has to restate the rule more than once:

- use `ctx` first
- read the CTX repository before planning
- let planning and decisions come from CTX state, not from chat habit
- use chat only for user intent, clarification, reporting, or explicit exceptions

If an agent starts improvising from chat instead of CTX, repeat the instruction and redirect it back to `ctx status`, `ctx audit`, `ctx log`, `ctx graph summary`, and `ctx next`.

Exceptions should stay narrow:

- destructive or risky actions
- missing credentials or external access
- legal or commercial decisions
- ambiguity that cannot be resolved from CTX and the code

## Philosophy

CTX is designed around four principles:

- local-first usage
- structured reasoning instead of chat history
- an open `.ctx` repository format
- source-available software that can be modified and redistributed under the project license

Commercial local and on-premise use is allowed under the repository license.

Offering CTX as a competing hosted or managed service requires a separate commercial agreement.

## Solution

- `Ctx.Domain`: core entities, IDs, states, and traceability records
- `Ctx.Application`: contracts, repositories, provider abstractions, and application service API
- `Ctx.Core`: context building, diffs, commits, orchestration, hashing, and JSON serialization
- `Ctx.Persistence`: filesystem-backed `.ctx/` repositories
- `Ctx.Providers`: interchangeable OpenAI and Anthropic providers plus registry
- `Ctx.Infrastructure`: composition root
- `Ctx.Cli`: command-line interface for repository operations

## Commands

```powershell
ctx init --name CTX
ctx status
ctx goal add --title "Ship first CVCS core"
ctx task add --title "Implement commit engine" --goal <goalId>
ctx task add --title "Fix viewer selection bug" --parent <taskId>
ctx task update <taskId> --state Done
ctx hypo add --statement "Structured commits reduce repeated iterations" --task <taskId>
ctx hypo update <hypothesisId> --state Supported
ctx evidence add --title "Benchmark" --summary "Supports the current hypothesis" --supports hypothesis:<hypothesisId>
ctx decision add --title "Adopt structured commits" --hypotheses <hypothesisId> --evidence <evidenceId>
ctx conclusion add --summary "Proceed with structured commits" --decisions <decisionId> --evidence <evidenceId>
ctx run --provider openai --purpose "Evaluate next design decisions"
ctx run list
ctx packet list
ctx provider list
ctx metrics show
ctx next
ctx usage summary
ctx audit
ctx doctor
ctx export --output .\ctx-export.json
ctx import --input .\ctx-export.json
ctx commit -m "seed cognitive graph"
ctx usage coverage
```

Documentation:

- `docs/CLI_COMMANDS.md`
- `docs/COMMAND_ADOPTION_AND_COVERAGE.md`
- `docs/CTX_STRUCTURE.md`
- `docs/DOMAIN_MODEL.md`
- `docs/TECHNICAL_ARCHITECTURE.md`
- `docs/TECHNICAL_INDEX.md`
- `docs/PROJECT_PHILOSOPHY.md`
- `docs/COMMERCIAL_AND_GOVERNANCE_PHILOSOPHY.md`
- `docs/COGNITIVE_GRAPH_AND_LINEAGE.md`
- `docs/WORK_MODEL_AND_PRIORITIZATION.md`
- `docs/CTX_VIEWER_GUIDE.md`
- `docs/CTX_AUTONOMOUS_OPERATION_PROTOCOL.md`

Viewer:

- `dotnet run --project .\Ctx.Viewer`
- Open `http://localhost:5271`
- Load a `.ctx` repository path to inspect branches, timeline lanes, commits and graph traces over time
- If no repository path is stored or entered, the viewer first uses `CTX_VIEWER_DEFAULT_REPOSITORY_PATH` or `Viewer__DefaultRepositoryPath` when configured, and otherwise falls back to the project git root, which in this source repository resolves to `<repo-root>`
- This public source repository does not ship a live repo-root `.ctx` workspace, so the bundled demo or an explicit CTX repository path is the recommended starting point
- Default branch is `main` unless the browser already remembers a newer repository or branch selection
- `Auto-refresh` starts enabled by default unless the browser already remembers that you turned it off, and the viewer remembers that preference across reloads
- Use `Refresh` for manual reloads or keep `Auto-refresh` enabled for periodic sync
- History mode now uses a branch-first SourceTree-like explorer with a branch list, date ordering, grouped commit sections, and richer commit detail on click
- History mode now exposes `Author`, `Model` and `Commit` as separate columns so model provenance is visible directly in commit rows
- History rows now surface the primary `Goal` and a compact `Goal -> Task -> Hypothesis -> Decision/Conclusion` path so each commit reads like a cognitive line instead of a flat entity summary
- Commit detail now shows `Evidence` as a collapsible section so closures are explainable without cluttering the view
- Split and Graph modes now replace that full history explorer with a compact commit navigator so the graph and detail panels stay readable
- Viewer panels now adapt to the available viewport height and keep scroll inside each panel so History, Split and Graph stay balanced on screen
- Viewer panels can be resized via the vertical dividers and the width preference is persisted per mode
- Commit trace metadata can now expose optional `modelName` and `modelVersion`, and the viewer shows them when the runtime provides them
- Commit detail now includes a `Cognitive Path` section listing affected goals, tasks, hypotheses, decisions and conclusions
- The graph panel includes task-state filters so you can isolate active work from closed reasoning
- The graph panel also includes combinable focus presets for `All`, `Working`, `Thinking` and `Closed`
- The viewer remembers the last repository, branch, focus combination and task-state filter mix across reloads
- The left panel shows both active and closed tasks so current and completed work stay visible without opening the graph first
- Task items in the left panel can now focus the corresponding `Task` node and restore the working graph context
- Source repository root: `<repo-root>`
- Bundled demo repository: `<repo-root>\\examples\\viewer-demo`
- The bundled demo includes `main`, `feature/ux-timeline` and `research/validation`
- The bundled demo provides a ready-to-open `.ctx` repository for viewer validation
- Local publish/install is documented in `docs/LOCAL_CTX_INSTALLATION.md`
- The local launcher command is `ctx`
- Agent prompt example: `prompts/CTX_AGENT_PROMPT.md`
- Autonomous operator prompt: `prompts/CTX_AUTONOMOUS_OPERATOR_PROMPT.md`
- Authoring workflow example: `docs/USE_CTX_TO_BUILD_CTX.md`

## Planned Direction

### Cognitive Task Routing

Once work is classified inside CTX, each task could be assigned to the most appropriate executor instead of being handled by a single default model.

The long-term goal is not just multi-model support. The goal is cognitive orchestration:

- CTX classifies the work
- CTX selects the best executor for that task shape
- CTX preserves the reasoning trail
- CTX records why that route was chosen

Potential routing dimensions:

- task type: architecture, coding, debugging, research, refactor, documentation
- difficulty: simple, medium, complex
- urgency: immediate unblocker vs. deep work
- cost efficiency: cheapest model that can solve the task well
- reliability needs: when higher-confidence reasoning is worth the extra cost
- continuity: whether the current thread should stay with the same agent or be reassigned

Potential future executor profiles could include providers such as OpenAI, Anthropic, and DeepSeek, chosen per task instead of by fixed default.

Expected benefits:

- lower execution cost
- better task-to-model fit
- less waste from using expensive models on simple work
- stronger continuity because decisions remain in CTX rather than in chat
- measurable performance over time through evidence, outcomes, and cost tracking

This is a planned direction, not a statement of current capability.

## Notes

- The repository is pinned to `.NET SDK 8.0.419` via `global.json`.
- If provider API keys are not configured, provider execution falls back to deterministic offline structured output so the repository workflow remains testable.
- Licensing terms are documented in `LICENSE`.
- Copyright notice is documented in `COPYRIGHT.md`.
- Trademark usage is documented in `TRADEMARK.md`.
- Contributions are governed by `CONTRIBUTOR_ASSIGNMENT.md`.
- Merge operations detect divergent cognitive artifacts and return explicit conflict records for review.
- Product planning and V1 roadmap are documented in `docs/V1_PLAN.md`.
- Formal V1 functional specification is documented in `docs/V1_FUNCTIONAL_SPEC.md`.
- Pilot execution guidance is documented in `docs/PILOT_TESTING_GUIDE.md`.
- Installation and first-use guidance are documented in `docs/INSTALLATION_AND_USAGE_GUIDE.md`.
- Release baseline details are documented in `docs/RELEASE_1_0_0.md`.
- A repeatable smoke test is available at `scripts/run-smoke-test.ps1`.
- A repeatable branch/merge conflict demo is available at `scripts/run-merge-conflict-demo.ps1`.
- A local publish script is available at `scripts/publish-local.ps1`.
- A cross-platform portable distribution script is available at `scripts/build-distribution.ps1`.
- A safe orphaned Git lock repair script is available at `scripts/repair-git-lock.ps1`.
- `CTX_VIEWER_DEFAULT_REPOSITORY_PATH` or `Viewer__DefaultRepositoryPath` can override the viewer bootstrap repository when you do not want to default to the current project git root.
- Optional trace model metadata can be injected through `CTX_MODEL_NAME` and `CTX_MODEL_VERSION`.
- The default operating rule for agents is: inspect CTX first, plan from CTX, decide from CTX, and treat chat as an exception surface rather than the system of record.
- `ctx next` returns `Task` candidates when open work exists, and can return `Gap` candidates from strong recorded hypotheses when CTX has no open task to continue.
- Only hypotheses in `Proposed` or `UnderEvaluation` state are eligible as `Gap` candidates for `ctx next`.
- `ctx audit` reports cognitive consistency issues such as open hypotheses on closed tasks, draft conclusions on completed work, and missing thread closure.
- `docs/COMMAND_ADOPTION_AND_COVERAGE.md` summarizes which CLI surfaces are already used in the current workflow, which remain cold, and which unused commands should be validated first by product value.
- `ctx thread reconstruct --format markdown` emits a readable narrative thread artifact in addition to the structured JSON model.
- `docs/WORK_MODEL_AND_PRIORITIZATION.md` now defines the canonical distinction between `issue`, `gap`, `task`, `subtask`, `blocker`, `duplicate` and `follow-up`.
- Distribution assets now live under `distribution/`, including target manifests, platform installer scaffolding, and the shipped agent-link prompt fragment.

