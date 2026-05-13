# CTX 1.0.17 Release Notes

Release date:

- `2026-05-13`

Version:

- `1.0.17`

## Summary

CTX 1.0.17 aligns the public Viewer Compare backend with the validated parent/child comparison behavior.

The 1.0.16 hotfix restored the public `/api/diff` endpoint. This release keeps `v1.0.16` immutable and improves that endpoint so direct parent comparisons return the preserved cognitive commit diff instead of rebuilding the preview from snapshots.

## Fixed

- Viewer Compare Graph now returns the preserved `targetCommit.Diff` when `from` is a direct parent of `to`.
- `/api/diff` still falls back to the cognitive diff service for non-parent comparisons.
- Viewer contract tests now cover the mapped endpoint, repository guard, parent-direct preserved diff path, and fallback diff path.

## Validation Checklist

- `ctx version` reports `1.0.17`.
- `node --check Ctx.Viewer\wwwroot\app.mjs` passes.
- `dotnet build Ctx.sln --no-restore` passes.
- `dotnet test Ctx.Tests\Ctx.Tests.csproj --no-restore` passes.
- `git diff --check` passes.
- Viewer `/api/diff` returns HTTP 200 with `summary` and `diff` JSON for a parent/child cognitive commit pair.
- GitHub Pages artifact includes `index.html`, `docs.html`, `notes.html`, `mcp-local.html`, `talk-unju.html`, `styles.css`, and both Viewer screenshot assets.
- Landing screenshot paths resolve to `./assets/ctx-viewer-working-context.jpg` and `./assets/ctx-viewer-commit-thread.jpg`.
- Distribution zip and tar archives include the updated release docs and live-demo assets.
- Public repository root has no `.ctx` state staged.
- Machine-specific paths are absent from public docs and source.
