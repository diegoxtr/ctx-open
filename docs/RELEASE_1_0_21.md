# CTX 1.0.21 Release Notes

Release date:

- `TBD`

Version:

- `1.0.21`

## Summary

CTX 1.0.21 prepares the next public Viewer and MCP maintenance release.

The Viewer gets the private chrome/layout improvements promoted into the public build: compact top chrome, collapsible project tabs, a minimizable graph intro, stable graph sizing, and Compare Graph controls for auto-fit and expanded canvas mode. CTX also adds first-class `runbook update` support across CLI and MCP so operators and write-mode agents can maintain operational playbooks without editing `.ctx` files by hand.

## Added

- `ctx runbook update <runbookId>` for updating existing operational runbooks.
- `ctx_runbook_update` MCP write tool for agent-facing runbook maintenance.
- Repeated list option handling for runbook add/update flows.
- `runbook update --append` for appending list entries without replacing current playbook values.
- Viewer chrome layout controller for measuring fixed topbar, project tabs, view toolbar, and footer heights.
- Compare Graph auto-fit and expanded-canvas controls inside the graph surface.
- Contract coverage for Viewer chrome controls, graph sizing, and runbook update behavior.

## Changed

- Viewer topbar can now collapse and restore from a compact control.
- Viewer project tabs are compact by default, collapsible, and vertically adjustable.
- Viewer graph intro can be minimized to keep the graph closer to the controls.
- Viewer graph canvas can resize freely on desktop without a viewport-height ceiling.
- Viewer parked epic rail and graph summary stay aligned to the resizable graph window.
- Viewer center panel owns its scroll surface so fixed chrome and side rail controls remain reachable.
- MCP parity documentation now marks `runbook add/update` as implemented and keeps attach/detach plus prompt-list follow-ups explicit.

## Validation Checklist

- `ctx version` reports `1.0.21`.
- `node --check Ctx.Viewer\wwwroot\app.mjs` passes.
- `dotnet build Ctx.sln --no-restore` passes.
- `dotnet test Ctx.Tests\Ctx.Tests.csproj --no-restore` passes.
- Viewer contract coverage verifies chrome controls, graph sizing, Compare Graph controls, and runbook update behavior.
- Public/private consistency guard passes with no unclassified drift or public leak findings.
- Public repository root has no `.ctx` state staged.
- Machine-specific paths and private source references are absent from release candidate files.
