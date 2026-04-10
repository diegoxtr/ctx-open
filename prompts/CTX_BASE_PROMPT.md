# CTX Base Prompt (Template)

This prompt is a starting point for operating CTX with a new tool or integration.

Fill in the bracketed fields before use.

## Objective

[Describe the primary objective in one clear sentence.]

## Scope

- Includes: [what the agent should cover]
- Excludes: [what the agent must not touch]

## Operating Context

- CTX repository: [local path]
- Cognitive branch: [branch name]
- Environment/stack: [runtime, SDK, dependencies]
- Constraints: [access, credentials, permissions]

## CTX Working Rule

The agent must work from CTX as the primary source:

1. `ctx status`
2. `ctx graph summary`
3. `ctx log`
4. `ctx audit`
5. `ctx next`

If `ctx next` returns no candidates, record the gap as a task before proceeding.

Do not edit `.ctx` manually unless there is a real operational block.

## Cognitive Versioner ↔ Agent Link

The agent must bind its work to CTX as the system of record:

- Always derive the next step from `ctx next` or explicitly recorded gaps.
- Record every meaningful outcome as `evidence`, `decision`, or `conclusion`.
- Close a task with a cognitive commit before any Git commit.
- Do not invent work outside the CTX graph without adding the task/hypothesis first.

## Tool Adaptation

Describe the tool and how it integrates with CTX:

- Tool: [name]
- Expected input: [format]
- Expected output: [format]
- Evidence produced: [type]
- Limits or risks: [operational]

## Expected Flow

1. Identify the active goal and task from CTX.
2. Create a hypothesis if the justification is missing.
3. Execute the smallest block that produces real evidence.
4. Record evidence.
5. Record a decision if direction changes.
6. Close a conclusion.
7. `ctx commit -m "<result>"`
8. Git commit only after the cognitive commit.

## Deliverables

- [expected results]
- [artifacts / files]
- [required evidence]

## Exit Criteria

- The task is `Done`.
- Evidence and conclusion are linked.
- A cognitive commit exists.
- The Git commit reflects the real change.

## Notes

Ask only if an external decision or credential is missing.
If there is no real block, continue without asking for confirmation.

Distribution note:

- the reusable agent-link fragment is shipped in `distribution/agent-link/CTX_AGENT_LINK_PROMPT.txt`
