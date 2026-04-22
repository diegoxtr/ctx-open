# CTX - Release 1.0.6

发布日期：2026-04-17

版本：

- `1.0.6`

摘要：

- CTX 1.0 的稳定补丁版本。
- 冻结第一条 public-safe 基线：bootstrap indexing、branch-like hypothesis semantics 与 interpretation-aware viewer surfaces 一起发布。

亮点：

- CTX 现在公开提供 `ctx bootstrap map` 和 `ctx bootstrap apply`，使代理能够先从文章和项目中建立 provisional cognitive threads，再将其提升为 durable CTX work。
- 公共领域模型和 CLI 现在支持 branch-like hypothesis semantics，包括 branch state、branch role、lineage grouping、inter-hypothesis relations、merge/supersede flows，以及 evidence sharing。
- 公共 viewer 现在包含 `Interpretations` 详情标签，以及可选的 `Show interpretation relations` overlay，使 competing hypotheses 在不破坏默认 trace graph 的情况下保持可见。
- 公共仓库现在携带经过清洗的 agriculture bootstrap example packs（`v1`、`v2`、`v3`、`v4`），包括 plans 和 real-testing notes，同时将 private `.ctx` workspaces 保持在已发布 surface 之外。
- 公共 helper 与技术文档现已与 private/public repo boundary 对齐，使 release operators 与 agents 始终锚定在预期的 public-safe workflow 上。
