# CTX - Release 1.0.4

发布日期：2026-04-15

版本：

- `1.0.4`

摘要：

- CTX 1.0 的稳定补丁版本。
- 在 live-demo 加固、viewer parity 同步，以及 commit-focus lineage 自动选择收敛为一个统一发布之后，冻结当前 public-safe baseline。

亮点：

- viewer 现在会自动选择主要的 commit-focus 节点，因此历史提交视图会立即通过 lineage highlight 解释自身。
- 历史图导出现在对缩写 commit ID、更安全的 legacy snapshot 处理，以及受控 JSON 失败更稳健，而不是直接抛出原始 server error。
- GitHub Codespaces 的 live-demo 流程现在已被记录并加固，包含明确入口、可复制粘贴的 demo 仓库路径、SDK bootstrap 恢复，以及更清晰的发布边界说明。
