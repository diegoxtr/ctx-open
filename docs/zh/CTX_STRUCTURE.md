# `.ctx/` 的内部结构

如果语言模型及其代理丢失了上下文，这就是你需要的工具。

本文描述 CTX 的本地存储结构。

`.ctx/` 文件夹是磁盘上的持久化认知仓库。它包含配置、活动状态、可复现历史以及系统生成的操作性产物。

## 目标

`.ctx/` 结构的设计目标是：

- 持久化结构化推理，而不是原始对话
- 支持可复现的认知 commits
- 将 working state 与 immutable snapshots 分离
- 支持 branching、diff 与 merge
- 记录 runs、packets 与 metrics
- 支持 export、import、backup 与 audit

## 总览

目标结构：

```text
.ctx/
  version.json
  config.json
  project.json
  HEAD
  branches/
  commits/
  graph/
  working/
  staging/
  runs/
  packets/
  index/
  metrics/
  providers/
  logs/
```

并不是所有目录一开始都会有内容。部分目录只会在特定流程执行后才生成文件。

## 基础文件

### `version.json`

描述仓库格式版本。

职责：

- 仓库兼容性
- 持久化格式控制

示例：

```json
{
  "currentVersion": "1.0",
  "initializedAtUtc": "2026-04-07T20:12:05.4352656+00:00"
}
```

### `config.json`

仓库配置。

职责：

- default provider
- available providers
- packet token limit
- metrics tracking

示例：

```json
{
  "defaultProvider": "openai",
  "providers": [
    {
      "name": "openai",
      "defaultModel": "gpt-4.1",
      "endpoint": "https://api.openai.com/v1/responses",
      "enabled": true
    },
    {
      "name": "anthropic",
      "defaultModel": "claude-3-7-sonnet-latest",
      "endpoint": "https://api.anthropic.com/v1/messages",
      "enabled": true
    }
  ],
  "packetTokenLimit": 16000,
  "trackMetrics": true
}
```

### `project.json`

表示根 `Project` 实体。

职责：

- project identity
- default branch
- project state
- creation traceability

### `HEAD`

指向当前分支与最后一个已知 commit。

当前格式：

```text
main:676790c259864667940b503e6c7e5008
```

职责：

- 当前 checkout 的分支
- 指向当前 commit 的快速引用

规则：

- 如果还没有 commit，那么 commit 可以逻辑上为 `null`

## 目录

### `branches/`

每个 branch 一条引用。

职责：

- 记录 branch pointers
- 支持 `branch`、`checkout` 与 `merge`

CLI 关系：

- `ctx branch`
- `ctx checkout`
- `ctx merge`

### `commits/`

保存不可变的 `ContextCommit` snapshots。

职责：

- 可复现历史
- 作为 `log`、`diff`、`merge`、`export` 的基础

snapshot 规则：

- commits 不再只快照 `WorkingContext`
- commits 现在持久化一个 `RepositorySnapshot`
- `RepositorySnapshot` 包含：
  - `WorkingContext`，保存活跃认知状态
  - `Runbooks`，保存与仓库一起版本化的稳定操作记忆
  - `Triggers`，保存与仓库一起版本化的稳定 origin memory

为什么这很重要：

- `OperationalRunbook` 留在 mutable `working-context.json` 之外
- `CognitiveTrigger` 留在 mutable `working-context.json` 之外
- 但它们仍然进入 CTX 的 history、diff、merge、export 与 import 语义

不变量：

- commit 一旦持久化就不得再变

CLI 关系：

- `ctx commit`
- `ctx log`
- `ctx diff`

### `graph/`

当前认知图的物化视图。

当前文件：

- `graph/current-graph.json`

职责：

- 当前领域状态的可导航快照
- 支持检查与未来优化

说明：

- 它与 `working/` 共存，作为有用的操作状态
- 后续可演进成专门的 projection

### `working/`

保存活动中的 `WorkingContext`。

当前文件：

- `working/working-context.json`

职责：

- 可变的进行中状态
- 已添加但不一定已经 commit 的产物
- 作为 `status`、`context`、`run` 与 `commit` 的基础
- 有意排除稳定操作记忆，例如 `OperationalRunbook`

CLI 关系：

- `ctx status`
- `ctx goal add`
- `ctx task add`
- `ctx hypo add`
- `ctx decision add`
- `ctx evidence add`
- `ctx conclusion add`
- `ctx run`
- `ctx context`

### `staging/`

保存准备 materialize 成 commit 的快照。

当前文件：

- `staging/staged-context.json`

职责：

- 持久化 staging snapshot
- 在 working 与 commit 之间提供复现基础

说明：

- 目前它基本镜像 commit engine 最终持久化的 snapshot
- 将来可作为更细粒度 staging 的基础

### `runs/`

保存持久化 AI runs。

职责：

- 记录 `Run`
- 保存每次 run 的结构化输出
- 支持操作检查与分析

CLI 关系：

- `ctx run`
- `ctx run list`
- `ctx run show`

### `packets/`

保存由 `ContextBuilder` 生成的 `ContextPacket`。

职责：

- 持久化发送给 provider 的优化上下文
- 审计 run 中优先了哪些信息

CLI 关系：

- `ctx context`
- `ctx packet list`
- `ctx packet show`

### `index/`

为搜索/访问优化预留。

当前状态：

- 包含一个 `README.txt` 占位

职责：

- 未来索引
- 更快的本地查询
- 次级仓库 projection

### `metrics/`

保存操作与经济指标。

当前文件：

- `metrics/usage.json`

这个 self-hosted 仓库中的操作说明：

- `metrics/usage.json` 被视为 runtime telemetry，可从 Git 中排除，以避免只读命令也弄脏工作树

职责：

- 累计成本
- token 使用
- 避免的冗余
- 总执行时间

CLI 关系：

- `ctx metrics show`

### `providers/`

为 provider-specific metadata、缓存或未来 projections 预留。

### `logs/`

为诊断与操作日志预留。

## 目录之间的概念关系

操作摘要：

- `working/` = 当前 mutable state
- `staging/` = 已准备好或已对齐、等待 commit 的 snapshot
- `commits/` = 不可变历史
- `branches/` + `HEAD` = 导航与当前指针
- `runs/` + `packets/` = AI interaction
- `metrics/` = cost / usage observability
- `graph/` = 认知状态投影

## 典型写入流程

### 1. 初始化

`ctx init` 会创建：

- `version.json`
- `config.json`
- `project.json`
- `HEAD`
- 目录结构
- `working/working-context.json`
- `graph/current-graph.json`
- `staging/staged-context.json`
- `metrics/usage.json`

### 2. 日常工作

像 `goal add`、`task add`、`hypo add`、`decision add`、`evidence add`、`conclusion add` 与 `run` 这样的命令，主要更新：

- `working/`
- `graph/`
- `runs/`
- `packets/`
- `metrics/`

### 3. 认知 commit

`ctx commit`：

- 在 `commits/` 里生成一个不可变 snapshot
- 更新 `HEAD`
- 更新 `branches/` 中当前 active branch
- 同步 `staging/`

### 4. 导出与导入

- `ctx export` 序列化可移植的仓库状态
- `ctx import` 在另一个环境重建 `.ctx/`

## 推荐不变量

保持以下规则有助于维护完整性：

- `HEAD` 必须指向现有 branch
- 当前 branch 必须存在于 `branches/`
- `working-context.json` 必须始终可反序列化
- `commits/` 中的 commit 不得变更
- 实体间被引用的 ID 必须存在
- `metrics/usage.json` 必须保持累计且一致
- 如果仓库用 Git 版本化 `.ctx/`，必须显式决定 `metrics/usage.json` 属于认知历史还是仅本地遥测
- `config.json` 必须反映应用认可的有效 providers

## 哪些目录最初可以为空

这些目录在起步阶段为空是正常的：

- `runs/`
- `packets/`
- `index/`
- `providers/`
- `logs/`

这不是错误。它取决于执行过哪些流程以及仓库成熟度。

## 什么不应成为主源

按照设计，`.ctx/` 不应把这些作为主源：

- 原始聊天日志
- 完整的非结构化对话
- embeddings 作为领域真相

主源始终是结构化领域实体。

## 相关文档

- [CLI_COMMANDS.md](../CLI_COMMANDS.md)
- [INSTALLATION_AND_USAGE_GUIDE.md](../INSTALLATION_AND_USAGE_GUIDE.md)
- [V1_FUNCTIONAL_SPEC.md](../V1_FUNCTIONAL_SPEC.md)
