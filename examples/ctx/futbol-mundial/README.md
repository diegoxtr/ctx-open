# futbol-mundial

Sanitized public CTX example based on a small project that tracks planning and closure for a football world cup fixture site.

What this example includes:

- a stable `.ctx` history with branches, commits, graph state, and project metadata
- completed tasks, hypotheses, evidence, decisions, and conclusions
- enough lineage to inspect the repository in CTX Viewer or with the CLI

What was removed from the original working repository:

- `working/` and `staging/` transient state
- `runs/`, `metrics/`, `providers/`, `logs/`, and `packets/`
- `write.lock`
- operator-specific trace values and chat-derived source labels

Suggested usage:

```powershell
dotnet run --project .\Ctx.Viewer
```

Then load:

```text
examples\ctx\futbol-mundial
```
