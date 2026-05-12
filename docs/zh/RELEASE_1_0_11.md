# CTX 1.0.11 Release Notes

Release date:

- `2026-04-28`

Version:

- `1.0.11`

## Summary

CTX 1.0.11 is a public documentation and release-alignment patch after the 1.0.10 MCP release. It keeps the published `v1.0.10` tag immutable, moves the post-tag MCP quickstart and README improvements into a new release line, and aligns public demo links with the current release.

## Highlights

- Public README explains CTX as durable cognitive memory beyond provider-level model memory.
- Public README documents local stdio MCP exposure, read-only default mode, guarded write mode, and current boundaries.
- Public demo links now target the current release line.
- Public local MCP quickstart is included in the release branch.
- Changelog ordering is corrected around the 1.0.8 and 1.0.7 sections.

## Validation

- `ctx version` reports `1.0.11`.
- `dotnet build Ctx.sln --no-restore` passes.
- `dotnet test Ctx.Tests\Ctx.Tests.csproj --no-restore` passes.
- `Ctx.Mcp` builds as part of the solution.
- The public repository does not include local `.ctx` state.
