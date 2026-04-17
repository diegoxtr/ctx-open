# CTX - Release 1.0.6

Release date: 2026-04-17

Version:

- `1.0.6`

Summary:

- Stable patch release for CTX 1.0.
- Freezes the first public-safe line where bootstrap indexing, branch-like hypothesis semantics, and interpretation-aware viewer surfaces ship together.

Highlights:

- CTX now exposes `ctx bootstrap map` and `ctx bootstrap apply` publicly so agents can build provisional cognitive threads from articles and projects before promoting them into durable CTX work.
- The public domain model and CLI now support branch-like hypothesis semantics, including branch state, branch role, lineage grouping, inter-hypothesis relations, merge/supersede flows, and evidence sharing.
- The public viewer now includes an `Interpretations` detail tab and an optional `Show interpretation relations` overlay so competing hypotheses can stay visible without degrading the default trace graph.
- The public repo now carries sanitized agriculture bootstrap example packs for `v1`, `v2`, `v3`, and `v4`, including plans and real-testing notes, while keeping private `.ctx` workspaces out of the published surface.
- Public helper and technical docs are aligned with the private/public repo boundary so release operators and agents stay anchored on the intended public-safe workflow.
