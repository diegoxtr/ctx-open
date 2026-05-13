# CTX 1.0.19 Release Notes

Release date:

- `2026-05-13`

Version:

- `1.0.19`

## Summary

CTX 1.0.19 fixes the Viewer planning rail that labels and counts parked epics.

The underlying CTX data was already correct: completed epics had `Completed` state. The Viewer rail labeled `Parked Epics` was listing every `Epic` node regardless of state. This hotfix makes that UI match the data.

## Fixed

- `Parked Epics` now only shows epics whose state is `Parked`.
- Completed and active epics no longer appear in the parked-epic rail or parked-epic footer count.
- Viewer contract coverage now locks this behavior.

## Validation Checklist

- `ctx version` reports `1.0.19`.
- `node --check Ctx.Viewer\wwwroot\app.mjs` passes.
- `dotnet build Ctx.sln --no-restore` passes.
- `dotnet test Ctx.Tests\Ctx.Tests.csproj --no-restore` passes.
- `git diff --check` passes.
- GitHub Pages artifact includes `index.html`, `docs.html`, `notes.html`, `mcp-local.html`, `talk-unju.html`, `styles.css`, and both Viewer screenshot assets.
- Distribution zip and tar archives include the updated release docs and live-demo assets.
- Public repository root has no `.ctx` state staged.
- Machine-specific paths are absent from public docs and source.
