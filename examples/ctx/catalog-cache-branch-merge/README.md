# Catalog Cache Branch Merge

This example demonstrates CTX branching and cognitive merge around an architecture choice.

Scenario:

- A product catalog needs cache invalidation that remains consistent after price and stock updates.
- Two candidate strategies were evaluated in separate CTX branches:
  - `experiment/ttl-only`
  - `experiment/tag-aware`
- The final state on `main` preserves both reasoning lines and records the merged decision.

Files:

- `src/catalogCache.js`: final chosen implementation
- `tests/catalogCache.test.js`: executable validation for tag-aware invalidation
- `docs/ttl-strategy.md`: summary of the simpler candidate
- `docs/tag-strategy.md`: summary of the selected candidate
- `.ctx/`: cognitive repository with branches, commits, merge, and closure

Run tests:

```powershell
node .\tests\catalogCache.test.js
```
