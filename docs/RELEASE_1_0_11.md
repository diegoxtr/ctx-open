# CTX 1.0.11 Release Notes

Release date:

- `2026-04-28`

Version:

- `1.0.11`

## Summary

CTX 1.0.11 is a public documentation and release-alignment patch after the 1.0.10 MCP release. It keeps the published 1.0.10 tag immutable, moves the post-tag MCP quickstart and README improvements into a new release line, and aligns public demo links with the current release.

## Highlights

- Public README now explains CTX as durable cognitive memory beyond provider-level model memory.
- Public README now documents the local stdio MCP exposure, read-only default, guarded write mode, and current boundaries.
- Public GitHub Pages demo links now target the current release line.
- Public local MCP quickstart is included in the release branch.
- Changelog release ordering is corrected for the 1.0.8 and 1.0.7 sections.

## Added

- `docs/MCP_LOCAL_QUICKSTART.md` as a short public setup path for local MCP-capable agents.
- Live-demo MCP setup page for copy-ready local configuration snippets.
- README sections for `Beyond Model Memory` and `MCP Exposure`.

## Changed

- Live-demo landing copy now advertises CTX 1.0.11.
- Release download links now point at `v1.0.11`.
- README now front-loads the durable-memory and MCP operating model before reference details.

## Fixed

- Changelog section ordering around `1.0.8` and `1.0.7`.
- Stale public release links left behind after the 1.0.10 publication.

## Validation Checklist

- `ctx version` reports `1.0.11`.
- `dotnet build Ctx.sln --no-restore` passes.
- `dotnet test Ctx.Tests\Ctx.Tests.csproj --no-restore` passes.
- `Ctx.Mcp` builds as part of the solution.
- `node --check Ctx.Viewer\wwwroot\app.js` passes when Node is available.
- PowerShell and Bash install scripts parse.
- `git ls-files .ctx` is empty for the public repository root.
- `git status --short` does not include local `.ctx` state.
