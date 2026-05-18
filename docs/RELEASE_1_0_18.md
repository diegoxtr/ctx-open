# CTX 1.0.18 Release Notes

Release date:

- `2026-05-13`

Version:

- `1.0.18`

## Summary

CTX 1.0.18 improves Viewer live refresh detection by expanding the `/api/working-context/signal` fingerprint.

The Viewer already polled the working context signal, but the shipped fingerprint was narrower than the validated implementation. This release aligns the endpoint so Viewer refreshes can notice broader cognitive state changes, not just the active task slice.

## Changed

- `/api/working-context/signal` now fingerprints project metadata, repository dirty/head state, goals, epics, tasks, hypotheses, decisions, evidence, and conclusions.
- Viewer live sync can now react when non-task cognitive entities change.
- Viewer contract coverage now verifies the enriched signal surface.

## Validation Checklist

- `ctx version` reports `1.0.18`.
- `node --check Ctx.Viewer\wwwroot\app.mjs` passes.
- `dotnet build Ctx.sln --no-restore` passes.
- `dotnet test Ctx.Tests\Ctx.Tests.csproj --no-restore` passes.
- `git diff --check` passes.
- Viewer `/api/working-context/signal` returns HTTP 200 with a fingerprint for a valid `.ctx` repository.
- GitHub Pages artifact includes `index.html`, `docs.html`, `notes.html`, `mcp-local.html`, `talk-unju.html`, `styles.css`, and both Viewer screenshot assets.
- Landing screenshot paths resolve to `./assets/ctx-viewer-working-context.jpg` and `./assets/ctx-viewer-commit-thread.jpg`.
- Distribution zip and tar archives include the updated release docs and live-demo assets.
- Public repository root has no `.ctx` state staged.
- Machine-specific paths are absent from public docs and source.
