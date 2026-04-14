# OperationalRunbook 设计
如果语言模型和它的代理会丢失上下文，这就是你需要的工具。

## 目标

定义一个紧凑的一等实体，用来保存可重复使用的操作知识，并且能够进入 CTX packet，而不会显著增加上下文成本。

`OperationalRunbook` 用来捕获：

- 可重复的操作流程
- 反复出现的故障排查
- 操作策略
- 在执行漂移之前应生效的 guardrail

它不是 task、文档或脚本的替代品。
它是一层紧凑的操作性引导，用来在代理开始即兴发挥之前把它拉回规范路径。

## 为什么 CTX 需要它

目前 CTX 已经保存了：

- `working` 中的活跃认知工作
- 通过 goals、tasks、hypotheses、evidence、decisions 和 conclusions 存储的持久推理状态
- packet 模型之外的 prompts、scripts 和 docs

仍然缺失的是一层结构化的、可复用的操作知识，例如：

- 如何本地发布
- 什么情况下允许 Git closeout
- 如何处理 `.git/index.lock`
- 发布后如何验证本地 viewer

这些内容不应该只以长篇 prose 的方式存在于文档里，也不应该每次都靠 chat 重新发现。

## 版本化模型

`OperationalRunbook` 不是可变 `WorkingContext` 的一部分。

相反，CTX 通过 `RepositorySnapshot` 对它进行版本化：

- `working-context.json` 继续聚焦于活跃的认知执行状态
- `.ctx/runbooks/` 在磁盘上保存稳定的操作性记忆
- `ContextCommit.Snapshot` 现在同时捕获：
  - `WorkingContext`
  - `Runbooks`

这样 CTX 就能对重复出现的操作记忆进行版本化，而不会污染 in-progress 工作区模型。

## 设计规则

`OperationalRunbook` 必须保持：

- 紧凑
- 描述性强
- 选择成本低
- 注入 packet 的成本低

如果某个 runbook 变得很长，它应该进行总结并指向规范引用，而不是复制整份详细文档。

## 最小实体

建议字段：

- `Id`
- `Title`
- `Kind`
- `Triggers`
- `WhenToUse`
- `Do`
- `Verify`
- `References`
- `GoalIds`
- `TaskIds`
- `State`
- `Trace`

### `Title`

面向操作员的短名称。

示例：

- `Local publish`
- `Git closeout`
- `Recover index.lock`

### `Kind`

允许的最小值：

- `Procedure`
- `Troubleshooting`
- `Policy`
- `Guardrail`

### `Triggers`

紧凑的激活字符串。

示例：

- `publish-local`
- `git-commit`
- `git-push`
- `index.lock`
- `viewer`

第一版不需要复杂的匹配 DSL。

### `WhenToUse`

一句简短的话说明激活条件。

示例：

- `Use when publishing the local CLI or viewer build.`

### `Do`

简短的有序动作列表。

硬性建议：

- 最好控制在 `3-5` 条
- 每条都尽量简短
- 使用规范命令或路径，而不是长 prose

### `Verify`

简短的检查列表，用来确认 runbook 是否被正确执行。

### `References`

规范的支持路径或命令。

示例：

- `docs/LOCAL_CTX_INSTALLATION.md`
- `scripts/publish-local.ps1`
- `ctx audit`
- `ctx closeout`

### `GoalIds` 与 `TaskIds`

最小显式作用域：

- 两者都为空 = 全局 runbook
- `GoalIds` = 战略或战术范围
- `TaskIds` = 精确执行范围

### `State`

最小生命周期：

- `Active`
- `Archived`

## OperationalRunbook 不是什么

它不是：

- `Task` 的替代品
- `Evidence` 的替代品
- 长篇的流程文档
- 单次执行发生了什么的历史记录

规则：

- 可执行工作使用 `Task`
- 观察到的事实使用 `Evidence`
- 详细规范流程使用 docs/scripts
- 紧凑且可复用的操作指导使用 `OperationalRunbook`

## Packet 注入策略

packet 不应该包含所有匹配的 runbook。

默认硬限制：

- 主 packet 中最多包含 `2` 个 runbook

原因：

- 降低 token 成本
- 降低指令相互干扰
- 提高操作员焦点

## 选择顺序

如果多个 runbook 同时匹配，按以下顺序排序：

1. 精确的 `TaskId` 匹配
2. `GoalId` 匹配
3. 对 packet purpose 的精确 trigger 匹配
4. 存在操作风险时，`Guardrail` 优先于 `Procedure`
5. `Troubleshooting` 只有在存在相关失败信号时才进入
6. 最后用稳定的手动优先级或确定性的标题排序打破平局

## 溢出处理

如果匹配的 runbook 数量超过 packet 限制：

- 注入前 `2` 个
- 剩余 runbook 不进入主 packet 正文
- 以 `available runbooks` 的形式暴露剩余项

紧凑 packet 形态：

```text
Operational Runbooks
- Local publish
  When: publishing the local CLI or viewer build
  Do: run scripts/publish-local.ps1; verify C:\ctx outputs; validate installed viewer
  Verify: ctx audit clean; local viewer responds
- Git closeout
  When: before git commit or git push
  Do: run ctx closeout; ensure no .git/index.lock; commit CTX before Git
  Verify: git status clean; CTX clean

Additional runbooks available: Recover index.lock
```

这样可以保留可发现性，而不需要支付完整的上下文成本。

## 失败驱动的激活

有些 runbook 不应该默认进入 packet。

例如：

- `Recover index.lock` 只有在 lock 存在，或者观察到相关 Git 失败时才应进入

这能让 troubleshooting 在真正需要之前保持休眠。

## 持久化方向

为了让操作知识与可变认知工作分离，推荐的存储方向是：

- `.ctx/runbooks/`

这样 runbook 可以独立于 `working-context.json`，同时仍然可用于 packet 构建。

## CTX 应优先内建的首批 runbook

- `Local publish`
- `Git closeout`
- `Recover index.lock`
- `Viewer local validation`

## 实现立场

本文档定义的是最小但高价值的版本：

- 紧凑实体
- 紧凑 packet section
- 确定性排序
- 硬性溢出限制
- 通过规范引用避免重复长 prose

只有当真实使用证明这个紧凑模型不够时，才应继续增加复杂度。
