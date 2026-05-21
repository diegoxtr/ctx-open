# CTX 1.0.22 Release Notes

Release date: 2026-05-21

## Summary

CTX 1.0.22 is a Viewer and packaging hotfix release.

It keeps Working/Thinking graph focus limited to executable working context and strengthens the distribution pipeline so release bundles and installed roots carry the documentation and prompt files operators actually need.

## Fixed

- Viewer Working/Thinking focus hides planning epics instead of rendering the parked/planning layer from the unfiltered graph.
- Viewer Working/Thinking focus no longer leaks archived promoted epics when every task in the scoped graph matches the selected task-state filters.
- The all-selected-tasks graph shortcut is only allowed when the planning layer is visible, such as All, Closed, or commit-focused views.

## Changed

- `scripts/build-distribution.ps1` now packages documentation from `distribution/packaged-docs.txt`.
- `scripts/build-distribution.ps1` now packages prompts from `distribution/packaged-prompts.txt`.
- `scripts/install-ctx.ps1` and `scripts/install-ctx.sh` now install and validate the same packaged documentation/prompt contract.
- Release docs now require the packaged documentation contract to be validated before publishing.

## Validation

- `node --check Ctx.Viewer\wwwroot\app.mjs`
- `dotnet test Ctx.Tests\Ctx.Tests.csproj --no-restore --filter ViewerAppContractTests`
- `ctx audit`
- Distribution validation must inspect at least one zip and one tar archive for the docs and prompts listed in `distribution/packaged-docs.txt` and `distribution/packaged-prompts.txt`.
