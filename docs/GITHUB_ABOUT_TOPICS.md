# GitHub Repository About Topics

Source repository: `diegoxtr/ctx-open`

Last verified: 2026-05-06

This file documents the public GitHub repository About metadata and the target topic set for the next public repository update.

## About Metadata

| Field | Value |
| --- | --- |
| Repository | `diegoxtr/ctx-open` |
| URL | `https://github.com/diegoxtr/ctx-open` |
| Homepage | `https://diegoxtr.github.io/ctx-open/` |
| Description | CTX is the standard Cognitive Version Control System for AI. Structured working memory for AI agents. CTX preserves goals, tasks, evidence, decisions, conclusions, runbooks, and origins so agents can continue work instead of restarting from scratch. |

## Target Topics

| Topic | Purpose |
| --- | --- |
| `acp` | Highlights Agent Client Protocol support and agent integration. |
| `agent-memory` | Signals persistent working memory for agents. |
| `ai` | Keeps broad AI discoverability. |
| `ai-agents` | Makes the project discoverable for agentic development use cases. |
| `ai-context-memory` | Positions CTX as context memory infrastructure for AI systems. |
| `ai-memory` | Positions CTX as durable memory for AI agents. |
| `claude-context-memory` | Connects CTX to Claude workflows focused on context continuity. |
| `codex-context-memory` | Connects CTX to Codex workflows focused on persistent context. |
| `cognitive-context-control` | Names CTX as control infrastructure for cognitive context. |
| `cognitive-ia` | Spanish discoverability variant for cognitive AI. |
| `cognitive-memory-control` | Names CTX as control infrastructure for cognitive memory. |
| `cognitive-version-control` | Names the core CTX category. |
| `context-engineering` | Connects CTX to context design and continuity workflows. |
| `deepseek-context-memory` | Connects CTX to DeepSeek workflows focused on context continuity. |
| `gemini-context-memory` | Connects CTX to Gemini workflows focused on context continuity. |
| `knowledge-graph` | Connects CTX to graph-based cognitive state and lineage. |
| `reasoning` | Signals structured reasoning support. |
| `mcp-context-server` | Highlights MCP server support for context exchange. |
| `mcp-memory-server` | Highlights MCP server support for memory access. |
| `mpc-control-context` | Replaces `mpc-control` with a more context-specific topic. |

## Remove Or Replace

| Current topic | Action | Replacement |
| --- | --- | --- |
| `developer-tools` | Remove | Covered by CTX-specific topics. |
| `traceability` | Remove | Covered by CTX description and cognitive control topics. |
| `codex` | Replace | `codex-context-memory` |
| `mpc-control` | Replace | `mpc-control-context` |
| `llm` | Remove | Too generic for the public About topic budget. |
| `claude-ai` | Replace | `claude-context-memory` |
| `claude-code` | Remove | Covered by Claude context-memory topic. |
| `codex-tools` | Remove | Covered by Codex context-memory topic. |
| `mcp-server` | Remove | Covered by `mcp-context-server` and `mcp-memory-server`. |

## Maintenance Checklist

- Run `gh repo view diegoxtr/ctx-open --json description,homepageUrl,repositoryTopics,url` before updating this file.
- Confirm the About description matches the public repository exactly.
- Confirm the homepage URL points to the current public landing page.
- Confirm the public repository uses exactly the 20 topics in `Target Topics`.
- Confirm every removed topic is absent from GitHub after the update.
- GitHub allows no more than 20 repository topics; keep this list capped.
- Review `mpc-control-context`; if `mpc` is a typo, rename it to `mcp-control-context` before applying the public update.
- Do not add private paths, private repository names, tokens, local usernames, or unpublished release details.
