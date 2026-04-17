# Bootstrap Agriculture Demo V4

This example is the branch-semantics validation repo for the same contradiction-heavy agricultural text used in `v1`, `v2`, and `v3`.

It exists after:

- `bootstrap-agriculture-demo` proved that CTX review can manually recover multiple hypotheses
- `bootstrap-agriculture-demo-v2` proved that the old bootstrap collapsed contradiction too early
- `bootstrap-agriculture-demo-v3` proved that coexistence-first bootstrap can preserve multiple hypotheses

The purpose of `v4` is different:

- validate the new branch-like hypothesis model and CLI operations
- test the same contradiction-heavy article again
- confirm that competing interpretations can now carry lifecycle, relations, and evidence sharing semantics

Expected success condition:

- competing interpretations remain explicit
- branch-like metadata is visible in CTX state
- merge, relation, and evidence-sharing operations behave coherently on the same regression case

Artifacts for this version:

- [V4_PLAN.md](C:/sources/ctx-open/examples/bootstrap-agriculture-demo-v4/V4_PLAN.md)
- [REAL_TESTING.md](C:/sources/ctx-open/examples/bootstrap-agriculture-demo-v4/REAL_TESTING.md)

In `ctx-open`, this folder is published as a regression-and-testing pack.
If you want to replay the case, initialize it locally as its own CTX repository so the branch-semantics layer can be measured independently from `v1`, `v2`, and `v3`.

