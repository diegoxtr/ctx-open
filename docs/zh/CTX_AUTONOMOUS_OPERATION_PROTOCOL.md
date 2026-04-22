# CTX 自主操作协议
如果语言模型及其代理丢失了上下文，这就是你需要的工具。

## 目标

本文档定义模型如何在 CTX 上工作，而不是持续依赖用户逐步指挥。

核心思想：

- CTX 不是事后日志
- CTX 是定义下一步的认知系统

模型应基于以下结构推进：

- goals
- tasks
- hypotheses
- evidence
- decisions
- conclusions
- cognitive commits

语义规则：

- `Working context` 是活动推理所在的位置
- `ctx commit` 用来保存 durable cognitive state
- 不要为每一个想法或每一个小 task closure 强行做认知提交

## 入口分叉：已有仓库 vs 新项目

模型必须首先区分以下两种情况。

## 状态驱动规则

不要从命令大全来操作 CTX。
应该从当前状态以及该状态暗示的下一条命令来操作。

- 没有 CTX 仓库：
  - 下一条命令 -> `ctx init --name "<project>"`
- 已有 CTX 仓库且有开放工作：
  - 下一条命令 -> `ctx next`
- 已有 CTX 仓库且有待收尾认知变更：
  - 下一条命令 -> `ctx closeout`
- 已到 durable boundary：
  - 下一条命令 -> `ctx commit -m "<durable result>"`
- 已有 CTX 仓库但没有开放工作：
  - 下一条命令 -> `ctx next`

### 已存在的 CTX 仓库

如果项目根目录已经有 `.ctx/`，第一步不是 `ctx init`。

应使用：

```powershell
ctx
```

然后跟随当前 CTX 状态暗示的下一条命令。`ctx status`、`ctx audit`、`ctx graph summary`、`ctx log` 只在确实需要更深检查时再使用。

### 新认知项目

如果 `.ctx/` 还不存在，先初始化：

```powershell
ctx init --name "<project>"
```

初始化后：

- 如果项目或资料已经存在，优先 bootstrap
- 如果是纯 greenfield，就显式创建第一个 goal、task 和 hypothesis

之后再进入常规的 `ctx next` 循环。

## 指导原则

下一步不能来自聊天本身。
它必须来自 CTX 仓库的当前状态。

如果 CTX 已经有足够上下文，模型应当：

- inspect
- choose
- execute
- record
- close

而不是等待用户在每个小步骤上重复确认。

## 主要规则

在碰代码或文档之前，模型应在内部回答：

1. 当前最重要的活跃 goal 是什么
2. 它下面挂着哪些开放 task
3. 下一步由哪些 hypothesis 支撑
4. 还缺哪些 evidence 才能验证或否定这些 hypothesis
5. 今天能够闭合的最小工作块是什么

## 无阻塞规则

如果 CTX 已经包含了足够信息来确定下一步，就不要向用户确认。

默认继续，除非出现真实阻塞：

- 破坏性或高风险操作
- 具有产品 / 法律 / 商业影响的战略决策
- 缺失凭证或外部访问
- 无法通过读取 CTX 和代码消解的歧义

## 强制操作循环

### 1. 初始检查

永远从这里开始：

```powershell
ctx
ctx next
```

如果 helper 输出还不足以安全选择，再深入：

```powershell
ctx status
ctx audit
```

如果需要额外聚焦：

```powershell
ctx graph lineage --goal <goalId>
ctx graph lineage --task <taskId>
ctx graph lineage --hypothesis <hypothesisId>
```

## 2. 选择下一步

优先级：

1. 能解锁主 goal 的开放 task
2. 分数最高或影响最大的 hypothesis
3. 在 evidence 中反复出现的 friction
4. 当前 conclusion 与真实产品之间的 gap

如果 `ctx audit` 发现会影响 `ctx next` 可靠性的债务，应先修复。

## Focus 规则

一次只解决一个 task。

这意味着：

- 从 CTX 里选择一个 active task
- 用 evidence、conclusion 和 cognitive commit 关闭它
- 然后再进入下一个

补充规则：

- task closure 不自动等于 `ctx commit`
- 一个 task 可以在更大的仍然开放的认知块内部关闭
- commit 应发生在 resulting state 已经足够 durable 时，而不只是行政状态变化时
- 如果认知 delta 还存在，它必须继续可见于 `Working context` 或 `Commit history`

## 顺序规则

默认顺序：

1. 在 CTX 中关闭当前 task
2. 做 cognitive commit
3. 做 Git commit
4. 重新检查 CTX
5. 继续下一个 task 或最强 gap

不要跳过这个顺序。
不要为了方便把多个 task 混成一个块。

## 3. 结构缺失时就补齐

```powershell
ctx goal add --title "<goal>"
ctx task add --title "<task>" --goal <goalId>
ctx hypo add --statement "<hypothesis>" --task <taskId>
```

重要工作必须至少有：

- task
- hypothesis 或 decision 作为正当性

## 4. 执行工作

执行时要：

- 阅读当前代码状态
- 做最小但有价值的改动
- 验证
- 记录结果

关于 `.ctx` 的规则：

- 不要把手工修改 `.ctx` 当作常规路径
- 默认路径永远是 `ctx ...`
- 只有在恢复或真实阻塞时，才直接触碰 `.ctx`
- 如果发生这种例外，必须把它记为 `evidence`

## 5. 记录 evidence

任何能改变工作方向的观察都应该记为 `evidence`。

包括：

- 测试失败
- 测试通过并验证 hypothesis
- 当前模型限制
- UX friction
- 编码问题
- 命令误用
- release 与安装之间的 drift
- 文档歧义

```powershell
ctx evidence add --title "<title>" --summary "<concrete finding>" --source "<source>" --kind Observation
```

如果它验证了 hypothesis：

```powershell
ctx evidence add --title "<title>" --summary "<validation>" --source "<source>" --kind Experiment --supports hypothesis:<hypothesisId>
```

## 6. 记录 decision

当模型确定规则或在多个选项之间做出选择时，必须记录 decision。

```powershell
ctx decision add --title "<decision>" --rationale "<rationale>" --state Accepted --hypotheses <id> --evidence <id>
```

## 7. 记录 conclusion

每个工作块都应该用 conclusion 收尾，说明：

- 完成了什么
- 验证了什么
- 还剩什么未关闭

```powershell
ctx conclusion add --summary "<conclusion>" --state Accepted --evidence <id> --goals <goalId> --tasks <taskId>
```

## 8. Cognitive commit

当一个连贯工作块关闭时：

```powershell
ctx commit -m "<block result>"
```

边界规则：

- 提交 durable cognitive state transition
- 不要提交每个微小想法
- 不要把每个小 task closure 都当成自动 commit boundary
- 在 reasoning 稳定之前，让 `Working context` 承载活动推理

## 9. Code commit

认知提交之后，才进行 Git 提交。

推荐顺序：

1. validate work
2. evidence
3. decision 或 conclusion
4. cognitive commit
5. Git commit
6. push

## 如何在没有用户帮助时选择下一步

如果用户说 `continue`，或者没有给新指令但 CTX 已经允许继续，使用以下算法：

1. inspect `ctx`
2. 如有需要，用 `ctx status`、`ctx audit`、`ctx log`、`ctx graph summary` 加深检查
3. 找到最活跃或最关键的 goal
4. 选择一个开放或隐含的 task，它应该：
   - 增加产品价值
   - 降低 friction
   - 验证重要 hypothesis
5. 确认该 task 已经在 CTX 中表示
6. 如果没有，就创建它
7. 执行能产生真实 evidence 的最小工作块
8. 用 evidence、conclusion 和 cognitive commit 收尾

## 什么时候不要问用户

不需要询问：

- 选择下一个明显 bug 或改进
- 记录 evidence
- 关闭 cognitive commit
- 改进必要文档
- 继续已在 CTX 中打开的工作线
- 执行 build、tests、viewer 验证
- 纠正已记录为 evidence 的 friction

## 什么时候应该问用户

只在这些情况下提问：

- 两个有效方向发生真实产品冲突
- 需要凭证或外部访问
- 敏感的法律或商业决策
- 破坏性变更或高风险迁移
- 无法通过 CTX 与代码本身解决的歧义

## 最终元规则

模型必须像一个 CTX autonomous operator 一样行动。

这意味着：

- 读取认知状态
- 从这个状态工作
- 产生 evidence
- 保持 traceability
- 用 judgment 决定下一步

控制句：

`If CTX already knows what's next, I should too and move forward.`
