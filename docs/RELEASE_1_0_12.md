# CTX 1.0.12 Release Notes

Release date:

- `2026-04-30`

Version:

- `1.0.12`

## Summary

CTX 1.0.12 makes local agent connection easier and clearer.

The main path is simple: install CTX, point `ctx-mcp` at a repository, start in read-only mode, call `ctx_plan`, and only then opt into write mode when the operator wants an agent to create CTX artifacts.

## Highlights

- Claude Code, Claude Desktop, VS Code/Copilot, and Codex setup snippets remain copy-ready from the static page.
- `ctx plan` and MCP `ctx_plan` are the recommended first smoke test for agent connection because they return branch state, dirty state, next work, focused context, and runbook suggestions in one response.
- `ctx prompt list` / `ctx prompts` provides a stable chronological prompt/trigger timeline for inspecting how cognitive work lines were opened.
- `ctx operational review` and enhanced `ctx preflight` help promote repeated operational issues into runbook updates.

## Added

- `ctx prompt list` and alias `ctx prompts` for chronological prompt-like trigger extraction.
- `ctx plan` for compact agent startup planning.
- `ctx operational review` for repeated issue detection and runbook improvement guidance.
- Claude-specific MCP setup blocks on the static MCP page:
  - Claude Code command
  - Claude Desktop JSON
  - project-scoped `.mcp.json`
  - Claude smoke-test prompt using `ctx_plan`

## Changed

- Static landing copy now highlights local MCP connection as a first-class path for agents, not only the Codespaces demo.
- MCP setup guidance now emphasizes read-only first connection, `ctx_plan` validation, and explicit write-mode opt-in.
- `ctx preflight` now includes repeated operational issue guidance when recurrence reaches the configured threshold.
- Example docs now avoid machine-specific checkout paths where relative or placeholder paths are sufficient.

## Fixed

- Stale hardcoded local checkout references in active example documentation were converted to relative paths or neutral placeholders.
- Static MCP release links were advanced from the 1.0.11 line to the 1.0.12 release line.

## Validation Checklist

- `ctx version` reports `1.0.12`.
- `dotnet build Ctx.sln --no-restore` passes.
- `dotnet test Ctx.Tests\Ctx.Tests.csproj --no-restore` passes.
- `dotnet test Ctx.Tests\Ctx.Tests.csproj --no-restore --filter "ListPromptTimelineAsync|CommandCoverage"` passes.
- `ctx prompt list --kind UserPrompt` and `ctx prompt list --kind AgentPrompt` succeed against a configured CTX repository.
- JSON snippets in MCP, ACP, and CLI docs parse.
- Static MCP page snippets use valid JSON/TOML shapes and smoke tests mention `ctx_plan`.
- `git ls-files .ctx` is empty for the public repository root.
- `git status --short` does not include `.ctx` state.
