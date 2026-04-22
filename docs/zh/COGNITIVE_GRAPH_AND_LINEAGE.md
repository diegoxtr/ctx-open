# CTX 中的认知图与谱系
如果语言模型及其代理丢失了上下文，这就是你需要的工具。

本文定义 CTX 应如何表示、导出并可视化工作的认知图。

核心思想很简单：

- CTX 不应只保存认知工件。
- CTX 应能显示这些工件之间的关系。
- CTX 应让推理如何随时间演化变得可见。

## 目标

为 CTX 增加认知谱系能力，以便：

- 可视化目标与子目标
- 跟踪任务、假设、证据、决策与结论之间的链路
- 发现推理缺口
- 检查提交之间的演化
- 构建类似提交图、但面向知识的视图

## 它解决什么问题

CTX 已经可以持久化并版本化结构化工件。

但它仍然难以直接回答：

- 这个任务源自哪个目标
- 哪些假设支持了这个决策
- 哪些证据支持了这个结论
- 哪些结论是孤立的
- 两次提交之间推理的哪一部分发生了变化
- 哪条知识线在某个分支中占主导

认知图就是用来解决这些问题的。

## 什么是认知图

它是当前认知状态或某个特定提交的关系投影。

它应建模：

- 节点
- 关系
- 状态
- 元数据
- 时间演化

## 核心节点

基础图节点应包括：

- `Project`
- `Goal`
- `Task`
- `Hypothesis`
- `Evidence`
- `Decision`
- `Conclusion`
- `Run`
- `ContextPacket`
- `ContextCommit`

## 核心关系

模型中已存在的关系：

- `Project -> Goal`
- `Goal -> Task`
- `Task -> Hypothesis`
- `Hypothesis -> Evidence`
- `Hypothesis -> Decision`
- `Evidence -> Decision`
- `Decision -> Conclusion`
- `Evidence -> Conclusion`
- `Run -> ContextPacket`
- `ContextCommit -> WorkingContext snapshot`

为了图而需要显式化的关系：

- `Goal -> Goal`，用于子目标
- `Task -> Task`，用于依赖
- `Conclusion -> Goal`，用于影响或闭环
- `RunArtifact -> EntityReference`，用于可视化的关系
- `ContextCommit -> ContextCommit`，用于历史和分支

## 概念谱系模型

典型知识线应如下所示：

```text
Goal
  -> Task
    -> Hypothesis
      -> Evidence
      -> Decision
        -> Conclusion
```

典型操作线应如下所示：

```text
Task
  -> ContextPacket
    -> Run
      -> RunArtifact
        -> Decision / Evidence / Conclusion
```

时间线应如下所示：

```text
Commit A -> Commit B -> Commit C
```

## 图的使用场景

### 1. 查看工作结构

示例：

- 当前有哪些目标
- 每个目标下面挂了哪些任务
- 哪些任务还没有附带假设

### 2. 查看推理依据

示例：

- 一个决策应显示：
  - 相关假设
  - 相关证据
  - 派生出的结论

### 3. 发现缺口

示例：

- 没有证据的决策
- 没有任务的假设
- 没有决策的结论
- 没有任务的目标

### 4. 查看演化

示例：

- 两次提交之间图的哪一部分发生了变化
- 哪些节点被新增、删除或修改

### 5. 分析分支

示例：

- 比较 `main` 与实验分支中的某条推理线

## 推荐的分阶段设计

## 阶段 1：图导出

目标：

- 在不依赖 UI 的前提下生成可导出的投影

建议命令：

- `ctx graph export --format json`
- `ctx graph export --format mermaid`
- `ctx graph export --format dot`
- `ctx graph export --commit <commitId>`

建议的 JSON 输出：

```json
{
  "nodes": [],
  "edges": [],
  "metadata": {}
}
```

建议的基础结构：

- `nodes`
  - `id`
  - `type`
  - `label`
  - `state`
  - `metadata`
- `edges`
  - `from`
  - `to`
  - `relationship`
  - `metadata`

价值：

- 可用外部工具进行可视化
- 保持前端独立

## 阶段 2：CLI 检查

目标：

- 从终端查询图

建议命令：

- `ctx graph show`
- `ctx graph focus --goal <id>`
- `ctx graph focus --task <id>`
- `ctx graph lineage --hypothesis <id>`
- `ctx graph lineage --decision <id>`
- `ctx graph diff <commitA> <commitB>`

价值：

- 快速本地分析
- 可用于自动化

## 阶段 3：交互式可视化

目标：

- 以图形方式浏览图

期望能力：

- 缩放与平移
- 按类型过滤
- 按状态过滤
- 按分支或状态着色
- 按提交查看
- 展开与折叠子树
- 高亮提交之间的变化

技术选项：

- Mermaid 适合简单视图
- Graphviz 适合导出
