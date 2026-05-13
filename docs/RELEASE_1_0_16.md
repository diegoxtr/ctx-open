# CTX 1.0.16 Release Notes

Release date:

- `2026-05-13`

Version:

- `1.0.16`

## Summary

CTX 1.0.16 is a public hotfix for Viewer cognitive commit comparison.

The 1.0.15 Viewer UI exposed Compare Graph from History rows, but the public backend was missing the `/api/diff` route used by that surface. This release keeps `v1.0.15` immutable and ships corrected binaries and archives under `v1.0.16`.

## Fixed

- Viewer Compare Graph now calls a mapped `/api/diff` backend endpoint instead of failing with `Comparison failed: 404`.
- `/api/diff` resolves abbreviated `from` and `to` commit ids with the same commit-reference resolver used by graph and commit endpoints.
- Viewer contract tests now cover the frontend/backend Compare Graph endpoint contract.

## Validation Checklist

- `ctx version` reports `1.0.16`.
- `node --check Ctx.Viewer\wwwroot\app.mjs` passes.
- `dotnet test Ctx.Tests\Ctx.Tests.csproj --no-restore` passes.
- `git diff --check` passes.
- Viewer `/api/diff` returns HTTP 200 with `summary` and `diff` JSON for two valid cognitive commits.
- GitHub Pages artifact includes `index.html`, `docs.html`, `notes.html`, `mcp-local.html`, `talk-unju.html`, `styles.css`, and both Viewer screenshot assets.
- Landing screenshot paths resolve to `./assets/ctx-viewer-working-context.jpg` and `./assets/ctx-viewer-commit-thread.jpg`.
- Distribution zip and tar archives include the updated release docs and live-demo assets.
- Public repository root has no `.ctx` state staged.
- Machine-specific paths are absent from public docs and source.
