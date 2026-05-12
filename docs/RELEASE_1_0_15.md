# CTX 1.0.15 Release Notes

Release date:

- `2026-05-12`

Version:

- `1.0.15`

## Summary

CTX 1.0.15 completes the public sync of the post-1.0.13 release inventory.

The release focuses on two missing public surfaces from the internal release inventory: a read-only update check command and the newer Viewer cognitive commit comparison workflow.

Inventory trace: `docs/RELEASE_1_0_15_INVENTORY.md` records the public include/exclude pass from the internal release inventory.

## Added

- `ctx update` and alias `ctx -update` check the latest public `ctx-open` GitHub Release and compare it with the local product version.
- `ctx update` reports structured fields including `status`, `currentVersion`, `latestVersion`, `latestTag`, `latestReleaseUrl`, and `updateAvailable`.
- `ctx update` supports `--owner`, `--repo`, and `--repository` overrides, plus `CTX_RELEASE_OWNER` and `CTX_RELEASE_REPOSITORY` environment defaults.
- Viewer frontend now uses the native module entrypoint `/app.mjs`.
- Viewer API, storage, workspace-tab, history, and utility helpers are split into focused modules under `/js/`.
- Viewer contract tests cover Compare Graph behavior, node inspection, graph scrolling, zoom behavior, cognitive diff summaries, and module boundaries.

## Changed

- The CLI command coverage catalog now tracks `update`.
- English, Spanish, and Chinese command references document `ctx update`.
- Viewer Compare Graph remains the explicit surface for cognitive commit comparison, with zoom, selectable diff nodes, concise summaries, and a Back to Trace Graph path.
- Release documentation now includes a public sync inventory so the post-1.0.13 private-to-public pass is auditable without exposing private CTX state.

## Validation Checklist

- `ctx version` reports `1.0.15`.
- `ctx update` and `ctx -update` report the latest public GitHub Release without mutating installed files.
- `dotnet build Ctx.sln --no-restore` passes.
- `dotnet test Ctx.Tests\Ctx.Tests.csproj --no-restore` passes.
- Viewer serves `/app.mjs` and the `/js/viewer-*.mjs` modules.
- Viewer Compare Graph still opens from History comparison controls and node clicks populate the Details panel.
- Public repository root has no `.ctx` state staged.
- Private repository paths are absent from public docs and source.
