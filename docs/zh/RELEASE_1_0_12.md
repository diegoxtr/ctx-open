# CTX 1.0.12 Release Notes

Release date:

- `2026-04-30`

Version:

- `1.0.12`

## Summary

CTX 1.0.12 makes local agent connection clearer and easier to validate with MCP.

The recommended flow is: install CTX, point `ctx-mcp` at a repository, start in read-only mode, call `ctx_plan`, and only then enable write mode when the operator wants the agent to create CTX artifacts.

## Highlights

- The static page includes copy-ready snippets for Claude Code, Claude Desktop, VS Code/Copilot, and Codex.
- `ctx plan` and MCP `ctx_plan` are the recommended first smoke test because they return branch state, dirty state, next work, focused context, and runbook suggestions in one response.
- `ctx prompt list` / `ctx prompts` adds a stable chronological prompt and trigger timeline.
- `ctx operational review` and enhanced `ctx preflight` help promote repeated operational issues into runbook updates.

## Added

- `ctx prompt list` and alias `ctx prompts`.
- `ctx plan` for compact agent startup planning.
- `ctx operational review` for repeated issue detection and runbook improvement guidance.
- Claude-specific blocks on the static MCP page:
  - Claude Code command
  - Claude Desktop JSON
  - project `.mcp.json`
  - smoke-test prompt using `ctx_plan`

## Changed

- Public landing copy now presents local MCP connection as a first-class path for agents.
- MCP setup guidance emphasizes read-only first connection, `ctx_plan` validation, and explicit write-mode opt-in.
- `ctx preflight` now includes repeated operational issue guidance when recurrence reaches the configured threshold.
- Example docs avoid machine-specific local paths where relative links or neutral placeholders are enough.

## Validation

- `ctx version` reports `1.0.12`.
- `dotnet build Ctx.sln --no-restore` passes.
- `dotnet test Ctx.Tests\Ctx.Tests.csproj --no-restore` passes.
- `dotnet test Ctx.Tests\Ctx.Tests.csproj --no-restore --filter "ListPromptTimelineAsync|CommandCoverage"` passes.
- MCP, ACP, and CLI JSON snippets parse.
- `git ls-files .ctx` returns no files for the public repository root.
