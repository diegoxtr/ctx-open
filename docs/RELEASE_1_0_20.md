# CTX 1.0.20 Release Notes

Release date:

- `2026-05-18`

Version:

- `1.0.20`

## Summary

CTX 1.0.20 improves the Viewer comparison workflow and mobile Viewer layout.

Commit details now launch parent comparisons directly, Compare Graph can include snapshot context around changed entities, and narrow screens present the Viewer in a graph-first order.

## Added

- Commit detail actions for `Use as base`, `Compare with selected base`, and `Compare with parent`.
- `/api/diff` response overlay with unchanged target snapshot context for Viewer comparisons.
- Compare Graph `Snapshot Context` nodes so changed entities can be inspected with surrounding cognitive state.
- Viewer contract coverage for commit-detail parent comparison and snapshot-context overlay rendering.

## Changed

- Mobile Viewer layouts now use page-level scrolling on narrow screens.
- Mobile panel order is now Trace Graph, Details, then History.
- Repository controls, tab bars, graph controls, and detail panels adapt to phone-width screens.
- Compare Graph summary text now distinguishes changed entities from snapshot context.

## Validation Checklist

- `ctx version` reports `1.0.20`.
- `node --check Ctx.Viewer\wwwroot\app.mjs` passes.
- `dotnet build Ctx.sln --no-restore` passes.
- `dotnet test Ctx.Tests\Ctx.Tests.csproj --no-restore` passes.
- Viewer `/api/diff` returns an `overlay.contextCount` for a real parent/child comparison.
- GitHub Pages artifact includes `index.html`, `docs.html`, `notes.html`, `mcp-local.html`, `talk-unju.html`, `styles.css`, and both Viewer screenshot assets.
- Distribution zip and tar archives include current release docs and live-demo assets.
- Public repository root has no `.ctx` state staged.
- Machine-specific paths and non-public source references are absent from public docs and source.
