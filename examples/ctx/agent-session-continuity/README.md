# Agent Session Continuity

This example demonstrates a CTX repository for multi-session agent continuity.

Scenario:

- An agent improves a notification digest feature across multiple sessions.
- The work is intentionally split into stages:
  - define digest shape
  - add quiet-hours filtering
  - add regression tests and closure evidence
- The cognitive repository preserves what was already decided so a later session can continue without depending on chat history.

Files:

- `src/digestBuilder.js`: final implementation
- `tests/digestBuilder.test.js`: executable validation
- `docs/session-1-notes.md`: first working session summary
- `docs/session-2-notes.md`: continuation session summary
- `.ctx/`: cognitive repository created with CTX

Run tests:

```powershell
node .\tests\digestBuilder.test.js
```
