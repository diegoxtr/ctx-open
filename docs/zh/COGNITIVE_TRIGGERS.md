# CognitiveTrigger 设计
如果语言模型和它的代理会丢失上下文，这就是你需要的工具。

`CognitiveTrigger` 用来保存开启或重定向一条认知线路的紧凑来源。

它不是 prompt 历史。
它是一个有类型、可版本化的记录，用来表示真正启动这条线路的原因。

示例：
- 用户请求
- 代理发起的延续性 prompt
- 反复出现的问题触发
- runbook 激活

## 最小模型

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

Kinds：
- `UserPrompt`
- `AgentPrompt`
- `Continuation`
- `RunbookTrigger`
- `IssueTrigger`

## 设计规则

- `Summary` 必须始终简短且存在
- `Text` 保持可选且有长度边界
- 不要为低价值的连续性消息创建 trigger
- 只有当消息真正开启、重定向或约束一条工作线时才创建 trigger

## 创建与继承策略

当线路发生实质性方向变化时，创建新的 trigger：
- 新的 `goal`
- 新的战术性 `sub-goal`
- 打开自身工作线的新 top-level task
- 新的问题 framing 或强约束
- 改变工作方向的 runbook 激活

当工作只是沿着同一条线继续推进时，继承最近的相关 trigger：
- `ok`
- `continua`
- 本地实现步骤
- 同一条线内的验证或关闭
- 子任务和依赖后续任务

这样可以让 `Origin` 保持有意义，而不是重复低信号的连续性文本。

## Origin 中重复文本意味着什么

如果 viewer 在多个后续 task 中显示相同的 origin 文本，这不一定是 bug。

这通常意味着：
- 这条线保持了相同的认知来源
- 没有引入实质性的新方向
- 当前 task 继承了最近的相关 trigger，而不是创建新的 trigger

因此 viewer 会区分：
- `Direct`：trigger 直接属于当前焦点
- `Inherited`：当前焦点正在继续附近的一条认知线路，并复用它的 origin

## 仓库模型

`CognitiveTrigger` 不存放在可变的 `working-context.json` 中，而是与 `OperationalRunbook` 一起通过 `RepositorySnapshot` 进行版本化。

这保证了：
- 活跃执行状态保留在 `WorkingContext`
- 稳定的操作性记忆保留在 `OperationalRunbook`
- 来源记忆保留在 `CognitiveTrigger`

## Packet 策略

packet 应该包含紧凑的 trigger 摘要，而不是完整 transcript。

默认 packet 形态：
- `Triggers`
- 一到两个简短条目
- 除非之后明确需要，否则不要完整导出 prompt
