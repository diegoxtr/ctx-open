# CTX Example Repositories

This folder contains example CTX repositories and supporting demo material.

## How To Read This Folder

The top-level demo folders are the examples you should open first:

- `critical-checkout-regression`
- `catalog-cache-branch-merge`
- `agent-session-continuity`

The top-level `version.json`, `config.json`, `HEAD`, `branches/`, and `metrics/` files are legacy repository artifacts for this folder itself. They are not the primary demo entry points.
Treat them as folder-level residue from an older workspace, not as the recommended way to understand the examples.

The three operational example repositories in this folder are currently aligned to the CTX `1.0.2` repository baseline.
`futbol-mundial` remains present as a sanitized public-style artifact set, but it is not part of the recommended internal showcase because it is not a fully live CTX workspace.

If you want to review the strongest commercial examples first, use this order:

1. `critical-checkout-regression`
2. `catalog-cache-branch-merge`
3. `agent-session-continuity`

## Demo Summary

### `critical-checkout-regression`

Best for:

- incident response
- root cause analysis
- auditability of technical decisions

What it shows:

- a critical bug investigation
- competing hypotheses
- evidence, decisions, conclusion, and executable validation

Key files:

- `critical-checkout-regression/README.md`
- `critical-checkout-regression/.ctx/`
- `critical-checkout-regression/tests/pricing.test.js`

### `catalog-cache-branch-merge`

Best for:

- architecture reviews
- branch comparison
- merge of competing reasoning lines

What it shows:

- separate cognitive branches
- rejection of one strategy
- acceptance of another
- final merge and closure on `main`

Key files:

- `catalog-cache-branch-merge/README.md`
- `catalog-cache-branch-merge/.ctx/`
- `catalog-cache-branch-merge/tests/catalogCache.test.js`

### `agent-session-continuity`

Best for:

- long-running agents
- cross-session continuity
- proving that CTX can replace chat memory as the system of record

What it shows:

- a first session that leaves explicit pending work
- a second session that resumes from CTX
- executable closure evidence

Key files:

- `agent-session-continuity/README.md`
- `agent-session-continuity/.ctx/`
- `agent-session-continuity/tests/digestBuilder.test.js`

## Useful Commands

Run these inside any example folder:

```powershell
ctx status
ctx log
ctx audit
ctx graph summary
```

If you are running directly from the source tree instead of the local install, the equivalent CLI surface is:

```powershell
dotnet C:\sources\ctx-public\Ctx.Cli\bin\Debug\net8.0\Ctx.Cli.dll status
dotnet C:\sources\ctx-public\Ctx.Cli\bin\Debug\net8.0\Ctx.Cli.dll log
dotnet C:\sources\ctx-public\Ctx.Cli\bin\Debug\net8.0\Ctx.Cli.dll audit
dotnet C:\sources\ctx-public\Ctx.Cli\bin\Debug\net8.0\Ctx.Cli.dll graph summary
```

Viewer:

```powershell
ctx-viewer
```

Then open `http://127.0.0.1:5271` and load one of the example directories above.

If you prefer the source-tree viewer instead of the local publish:

```powershell
dotnet run --project C:\sources\ctx-public\Ctx.Viewer --urls http://127.0.0.1:5271
```
