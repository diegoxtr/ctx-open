# CTX Specification v1

本文档定义 CTX v1 的最小规格。

它回答三个问题：

- 上下文如何保存
- 如何进行版本化
- 如何组织结构

## 1. CTX 如何保存上下文

CTX 将上下文保存为持久化的结构化工件，而不是聊天记录。

规范性的存储根目录是 `.ctx/`。

在这个根目录中，CTX 持久化保存：

- goals
- tasks
- hypotheses
- evidence
- decisions
- conclusions
- commits
- branches
- runbooks
- triggers
- packets

系统的真实记录源是 `.ctx/` 中的仓库状态，而不是导向该状态的对话历史。

## 2. CTX 如何对上下文进行版本化

CTX 通过自己的 commit 模型对认知状态进行版本化。

一个 CTX commit 会：

- 对结构化认知状态做快照
- 记录相对前一个 commit 的认知 diff
- 保留父关系
- 支持 branch、checkout、merge、diff 与历史检查

这意味着上下文不仅被保存。
它也作为推理随时间演化的可检查历史被版本化。

## 3. CTX 如何组织上下文结构

CTX 将上下文组织为一个显式的认知图。

核心层级是：

- `goal`
- `sub-goal`
- `task`
- `hypothesis`
- `decision`
- `conclusion`

支持性工件包括：

- `evidence`
- `runbook`
- `trigger`
- `packet`

这种结构使上下文具备：

- 显式性
- 可查询性
- 可审计性
- 可重建性
- 可在多次会话与多代理之间复用

## 4. CTX v1 规则

CTX v1 不把提示词历史当作主记忆层。

主记忆层是：

- 结构化工件
- 持久化于 `.ctx/`
- 通过 CTX commits 进行版本化
- 通过显式认知图连接起来

这就是 CTX v1 的核心规格。

## 5. 关键短句

这句话必须被明确写出：

`CTX is the standard for persistent context in AI agents`
