# CTX 领域模型

如果语言模型及其代理丢失了上下文，这就是你需要的工具。

本文描述 CTX 当前的领域模型。

它的目标是明确：

- 系统实体
- 强标识符
- 关系
- 生命周期状态
- 可追溯性规则
- 相关聚合与操作性产物

CTX 不把对话建模为主源，而是建模结构化认知产物。

## 模型原则

模型遵循这些规则：

- 重要信息必须结构化
- 每个相关产物必须有强身份
- 决策必须显式
- 证据必须可引用
- commits 必须可复现
- 推理演化必须可比较
- working state 和 history 必须分离

## 强标识符

每个主实体都使用基于 `record struct` 的 typed ID。

当前类型包括：

- `ProjectId`
- `GoalId`
- `TaskId`
- `HypothesisId`
- `DecisionId`
- `EvidenceId`
- `ConclusionId`
- `OperationalRunbookId`
- `RunId`
- `ContextCommitId`
- `ContextPacketId`
- `WorkingContextId`

它们的特点：

- 包装字符串值
- 以紧凑 GUID 生成
- 避免不同实体类型被误混

示例：

```csharp
public readonly record struct GoalId(string Value);
```

## 共享实体基类

可追溯的领域实体在概念上继承自：

```csharp
public abstract record CognitiveEntity<TId>(TId Id, Traceability Trace);
```

这强制保证：

- 强身份
- 必需的 traceability

## 可追溯性

对核心认知实体来说，traceability 是必需的。

结构包含：

- `CreatedBy`
- `CreatedAtUtc`
- `UpdatedBy`
- `UpdatedAtUtc`
- `Tags`
- `RelatedIds`

目的：

- 知道谁创建了某个产物
- 知道它何时被创建或更新
- 跟踪概念性 tags
- 连接次级相关 ID

## 核心实体

### Project

根认知项目。

字段：

- `Id`
- `Name`
- `Description`
- `DefaultBranch`
- `State`
- `Trace`

状态：

- `LifecycleState`

职责：

- 定义仓库身份
- 设置默认分支
- 作为概念根节点

### Goal

认知工作中的显式目标。

字段：

- `Id`
- `Title`
- `Description`
- `Priority`
- `State`
- `Trace`
- `TaskIds`

状态：

- `LifecycleState`

关系：

- 一个 `Goal` 可以分组多个 `Task`

职责：

- 表达高层意图
- 组织关联任务

### Task

具体的认知工作单元。

字段：

- `Id`
- `GoalId`
- `Title`
- `Description`
- `State`
- `Trace`
- `HypothesisIds`

状态：

- `TaskExecutionState`

取值：

- `Draft`
- `Ready`
- `InProgress`
- `Blocked`
- `Done`

关系：

- 可属于某个 `Goal`
- 可连接多个 `Hypothesis`

职责：

- 建模具体工作前线

### Hypothesis

一个待评估的假设或命题。

字段：

- `Id`
- `Statement`
- `Rationale`
- `Confidence`
- `State`
- `Trace`
- `TaskIds`
- `EvidenceIds`

状态：

- `HypothesisState`

取值：

- `Proposed`
- `UnderEvaluation`
- `Supported`
- `Refuted`
- `Archived`

关系：

- 连接一个或多个 `Task`
- 被 `Evidence` 支持

职责：

- 让可评估的想法显式化
- 避免隐式推理

#### Branch-like 扩展

在高矛盾工作中，CTX 需要 competing hypotheses 保持活着，而不是立即坍缩成一条综合叙事。

当前 `Hypothesis` 模型已经加入了第一层 branch-like 扩展，使假设能在同一个 lineage group 中表现得像 interpretation branches。

附加字段：

- `branchState`
- `branchRole`
- `lineageGroupId`
- `parentHypothesisIds`
- `mergedIntoHypothesisId`
- `supersedesHypothesisIds`

意图：

- `branchState`
  - `active`
  - `weakening`
  - `merged`
  - `deprecated`
  - `promoted`
- `branchRole`
  - `competing`
  - `integrative`
  - `dominant`
- `lineageGroupId`
  - 对同一未决问题的 rival interpretations 进行分组
- `parentHypothesisIds`
  - 表示它从一个或多个先前假设导出
- `mergedIntoHypothesisId`
  - 显式保留 merge 历史
- `supersedesHypothesisIds`
  - 记录某个更强假设对旧假设的替代

重要边界：

- 这些语义是在 hypothesis 模型内部提出的
- 它们不等同于 repository branches
- repository branching 仍应作为后续集成问题处理

### Decision

推理中的显式决策。

字段：

- `Id`
- `Title`
- `Rationale`
- `State`
- `Trace`
- `HypothesisIds`
- `EvidenceIds`

状态：

- `DecisionState`

取值：

- `Proposed`
- `Accepted`
- `Rejected`
- `Superseded`

关系：

- 引用 `Hypothesis`
- 引用 `Evidence`

职责：

- 记录显式选择
- 将决策与其依据连接

### OperationalRunbook

紧凑可复用的操作性记忆。

字段：

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

状态：

- `LifecycleState`

关系：

- 可作用于 `Goal`
- 可作用于 `Task`
- 可进入 `ContextPacket`
- 在仓库快照中版本化

职责：

- 保留 recurring procedures、guardrails 与 troubleshooting
- 不把它们混入可变 working execution state

### CognitiveTrigger

一条认知线的紧凑 typed origin。

字段：

- `Id`
- `Kind`
- `Summary`
- `Text`
- `Fingerprint`
- `GoalIds`
- `TaskIds`
- `OperationalRunbookIds`
- `State`
- `Trace`

关系：

- 可作用于 `Goal`
- 可作用于 `Task`
- 可引用 `OperationalRunbook`
- 可进入 `ContextPacket`
- 在仓库快照中版本化

职责：

- 保留真正打开或重定向认知线的来源
- 不把完整 prompt history 作为默认模型

### Evidence

可追溯的证据，用于支持或反驳其他产物。

字段：

- `Id`
- `Title`
- `Summary`
- `Source`
- `Kind`
- `Confidence`
- `State`
- `Trace`
- `Supports`

状态：

- `LifecycleState`

种类：

- `Observation`
- `Benchmark`
- `Document`
- `Experiment`
- `ProviderOutput`

关系：

- `Supports` 包含 `EntityReference`
- 可指向 `Hypothesis`、`Decision` 或 `Task`

职责：

- 记录可验证支持
- 避免没有显式依据的决策

### Conclusion

压缩后的结论。

字段：

- `Id`
- `Summary`
- `State`
- `Trace`
- `DecisionIds`
- `EvidenceIds`

状态：

- `ConclusionState`

取值：

- `Draft`
- `Accepted`
- `Superseded`

关系：

- 引用 `Decision`
- 引用 `Evidence`

职责：

- 压缩推理结果
- 显式关闭一条工作线

### Run

一次基于 `ContextPacket` 的 AI 执行。

字段：

- `Id`
- `Provider`
- `Model`
- `State`
- `StartedAtUtc`
- `CompletedAtUtc`
- `PacketId`
- `Usage`
- `PromptFingerprint`
- `Summary`
- `Artifacts`
- `Trace`

状态：

- `RunState`

取值：

- `Planned`
- `Running`
- `Completed`
- `Failed`

关系：

- 引用一个 `ContextPacket`
- 产出 `RunArtifact`

职责：

- 记录结构化 AI 交互
- 衡量成本、tokens、duration
- 为上下文演化保留有用输出

### ContextPacket

供 run 使用的优化上下文包。

字段：

- `Id`
- `ProjectId`
- `CreatedAtUtc`
- `Purpose`
- `Fingerprint`
- `EstimatedTokens`
- `GoalIds`
- `TaskIds`
- `HypothesisIds`
- `DecisionIds`
- `EvidenceIds`
- `ConclusionIds`
- `Sections`

职责：

- 选择相关信息
- 减少冗余
- 跟踪发送给 provider 的内容

说明：

- 当前实现中它不继承 `CognitiveEntity<TId>`
- 它是持久化结构化产物

### WorkingContext

当前可变的仓库状态。

字段：

- `Id`
- `RepositoryVersion`
- `CurrentBranch`
- `HeadCommitId`
- `Dirty`
- `Project`
- `Goals`
- `Tasks`
- `Hypotheses`
- `Decisions`
- `Evidence`
- `Conclusions`
- `Runs`
- `Trace`

职责：

- 集中承载进行中的状态
- 作为 `status`、`context`、`run` 与 `commit` 的基础
- 重建当前认知图
- 有意排除稳定的 `OperationalRunbook` 状态

相关方法：

- `ToGraph()` 构建一个 `ContextGraph`

### RepositorySnapshot

被 `ContextCommit` 使用的仓库级版本快照。

字段：

- `WorkingContext`
- `Runbooks`

职责：

- 将活跃认知状态与稳定操作记忆分离
- 但在 commits、diffs、merges、export、import 中一并版本化

### ContextCommit

仓库状态的不可变快照。

字段：

- `Id`
- `Branch`
- `Message`
- `ParentIds`
- `CreatedAtUtc`
- `SnapshotHash`
- `Diff`
- `Snapshot`
- `Trace`

职责：

- 保留可复现历史
- 捕获该快照的 diff
- 支持 `log`、`diff`、`branching`、`merge`

快照内容：

- `Snapshot.WorkingContext`
- `Snapshot.Runbooks`

规则：

- 一旦持久化，就不可变

## 支撑性产物

### ContextGraph

完整认知状态的投影。

包含：

- `Project`
- `Goals`
- `Tasks`
- `Hypotheses`
- `Decisions`
- `Evidence`
- `Conclusions`
- `Runs`

职责：

- 紧凑表示领域图

### RunArtifact

一次 run 产出的产物。

字段：

- `ArtifactType`
- `Title`
- `Content`
- `References`

职责：

- 提取 provider 输出中的结构化结果

### TokenUsage

执行使用量模型。

字段：

- `InputTokens`
- `OutputTokens`
- `AcuCost`
- `Duration`

派生：

- `TotalTokens`

### ContentSection

`ContextPacket` 中的结构化文本 section。

字段：

- `Title`
- `Content`
- `References`

### EntityReference

对其他领域实体的轻量引用。

字段：

- `EntityType`
- `EntityId`

用途：

- evidence supports
- run artifacts
- context sections

## 认知 diff 与 merge

### ContextDiffChange

表示状态之间检测到的一次变化。

字段：

- `ChangeType`
- `EntityType`
- `EntityId`
- `Summary`

### CognitiveConflict

显式认知冲突。

字段：

- `EntityType`
- `EntityId`
- `ConflictType`
- `CurrentSummary`
- `IncomingSummary`

职责：

- 解释语义分歧

### ContextDiff

按实体类型分组变化。

字段：

- `FromCommitId`
- `ToCommitId`
- `Decisions`
- `Hypotheses`
- `Evidence`
- `Tasks`
- `Conclusions`
- `Conflicts`
- `Summary`

职责：

- 检查推理演化

### MergeResult

一次 merge 操作的结果。

字段：

- `MergedContext`
- `Conflicts`
- `AutoMerged`
- `Summary`

职责：

- 表达认知分支之间的集成结果

## 配置与仓库

### ProviderConfiguration

字段：

- `Name`
- `DefaultModel`
- `Endpoint`
- `Enabled`

### RepositoryConfig

字段：

- `DefaultProvider`
- `Providers`
- `PacketTokenLimit`
- `TrackMetrics`

### RepositoryVersion

字段：

- `CurrentVersion`
- `InitializedAtUtc`

### HeadReference

字段：

- `Branch`
- `CommitId`

### BranchReference

字段：

- `Name`
- `CommitId`
- `UpdatedAtUtc`

## 可观测性与可移植性

### MetricsSnapshot

字段：

- `TotalRuns`
- `TotalTokens`
- `TotalAcuCost`
- `RepeatedIterations`
- `AvoidedRedundancyCount`
- `TotalExecutionTime`

### DoctorCheck

字段：

- `Name`
- `Status`
- `Detail`

### DoctorReport

字段：

- `ProductVersion`
- `WorkingDirectory`
- `RepositoryDetected`
- `Checks`

### RepositoryExport

字段：

- `ProductVersion`
- `RepositoryVersion`
- `Config`
- `Head`
- `WorkingContext`
- `Metrics`
- `Branches`
- `Commits`

## 关键关系

- `Project` 包含仓库上下文
- `Goal` 分组 `Task`
- `Task` 引用可选的 `Goal`
- `Task` 引用 `Hypothesis`
- `Hypothesis` 引用 `Task`
- `Hypothesis` 引用 `Evidence`
- `Decision` 引用 `Hypothesis`
- `Decision` 引用 `Evidence`
- `Conclusion` 引用 `Decision`
- `Conclusion` 引用 `Evidence`
- `Run` 引用 `ContextPacket`
- `WorkingContext` 聚合活跃实体
- `ContextCommit` 封装一个 `WorkingContext` 快照

## 生命周期状态

### `LifecycleState`

用于：

- `Project`
- `Goal`
- `Evidence`

取值：

- `Draft`
- `Active`
- `Validated`
- `Completed`
- `Superseded`
- `Archived`

### `TaskExecutionState`

用于：

- `Task`

取值：

- `Draft`
- `Ready`
- `InProgress`
- `Blocked`
- `Done`

### `HypothesisState`

用于：

- `Hypothesis`

取值：

- `Proposed`
- `UnderEvaluation`
- `Supported`
- `Refuted`
- `Archived`

### `DecisionState`

用于：

- `Decision`

取值：

- `Proposed`
- `Accepted`
- `Rejected`
- `Superseded`

### `ConclusionState`

用于：

- `Conclusion`

取值：

- `Draft`
- `Accepted`
- `Superseded`

### `RunState`

用于：

- `Run`

取值：

- `Planned`
- `Running`
- `Completed`
- `Failed`

## 操作性聚合

概念性聚合包括：

- `WorkingContext` 作为操作性聚合
- `ContextCommit` 作为不可变历史聚合
- `Run` 作为执行聚合
- `ContextPacket` 作为派生上下文产物

## 完整性规则

- 被引用的 ID 必须存在
- `Decision` 不得引用缺失的 hypotheses 或 evidence
- `Conclusion` 不得引用缺失的 decisions 或 evidence
- `Evidence.Supports` 必须使用有效引用
- `Run.PacketId` 必须指向已持久化 packet
- `WorkingContext.HeadCommitId` 必须与 `HEAD` 一致
- `ContextCommit.SnapshotHash` 必须与持久化快照匹配

## 为什么这个模型重要

这个模型使得以下能力成为可能：

- 对结构化推理进行版本化
- 比较认知演化
- 为决策给出依据
- 追踪证据
- 重建完整推理状态
- 在导出/导入时不丢失核心语义

## 相关参考

- [CTX_STRUCTURE.md](../CTX_STRUCTURE.md)
- [CLI_COMMANDS.md](../CLI_COMMANDS.md)
- [V1_FUNCTIONAL_SPEC.md](../V1_FUNCTIONAL_SPEC.md)
