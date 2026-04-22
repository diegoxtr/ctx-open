# CTX 中的工作模型与优先级

如果语言模型及其代理丢失了上下文，这就是你需要的工具。

## 目标

本文定义 CTX 应如何在不重复、也不引发不必要上下文切换的前提下，对新工作、派生工作与操作优先级进行推理。

核心思想是：

- 不是所有新出现的内容都值得变成一个新的 `task`
- 不是所有全局优先级都应该压过局部 blocker
- 不是所有发现都应该变成 hypothesis
- 不是所有相关项都应该开启一条新线程

CTX 需要区分：

- 新 roadmap work
- subwork
- duplicated work
- blockers
- close follow-ups

## 规范分类法

在决定某件事是否值得成为一个 `task` 之前，CTX 应先区分这些层级：

- `goal`
- `sub-goal`
- `issue`
- `gap`
- `task`
- `subtask`
- `blocker`
- `duplicate`
- `follow-up`
- `evidence`

基础规则：

- `goal` 定义 durable 的战略通道
- `sub-goal` 定义大目标之下的战术线
- `operational runbook` 定义紧凑可复用的操作知识
- `issue` 描述烦扰、失败或摩擦
- `gap` 命名当前状态与目标状态之间的具体差距
- `task` 定义用于关闭该差距的可执行工作

简式公式：

```text
goal -> sub-goal -> task
operational runbook -> execution guidance
issue -> gap -> task
```

### `OperationalRunbook`

`OperationalRunbook` 用于保存紧凑、可复用的操作性知识，例如：

- 本地 publish 流程
- Git closeout 规则
- recurring troubleshooting
- reusable guardrails

适用场景：

- 该知识会反复使用
- 它应在代理 improvises 之前指导执行
- 它太重要，不能只埋在文档或过去 evidence 里
- 它足够小，可以进入 packet 而不会显著膨胀上下文成本

不要把它用于一次性执行历史。
一次性的执行历史仍属于 `evidence`、`decision`、`conclusion` 与正常任务 closure。

### `CognitiveTrigger`

`CognitiveTrigger` 保存一条认知线被打开或重定向时的紧凑 origin。

适用场景：

- 用户消息打开一条新工作线
- agent continuation prompt 实质性重定向该线
- recurring issue 或 runbook activation 成为该线 origin 的一部分
- origin 需要可审计，而不能依赖外部聊天历史

不要为每个小回复都建 trigger。
像 `ok`、`continue` 这类轻量 continuity nudges 不应成为独立 trigger。

### `Goal`

`goal` 是一个战略性通道，应该在多个 tasks 与 commits 之间依然有意义。

适用场景：

- 工作开启或扩展了 durable 的产品、平台或操作方向
- 即便当前没有具体执行线程，它仍应该保持可理解
- 它比单个战术工作分支更宽

不要在工作只是某个现有战略通道的主题性分支时创建新的 `goal`。

战略 goals 可以在其下暂时没有日常执行线程时继续保持 `Active`。
这本身不构成 closure bug。它只表示该 goal 仍代表一条 durable 的产品或操作方向。

closure 规则：

- 如果一个 `goal` 仍然命名着活着的战略通道，就保持 active
- 当一个 `goal` 只描述了一个已经不再是当前主线的 bounded objective 时，关闭或 supersede 它
- 不要为了让 graph 看起来安静而去关闭一个战略 goal

### `Sub-goal`

`sub-goal` 是现有 goal 之下的一条战术工作线。

适用场景：

- 它属于某个 active 的战略目标
- 它需要自己的认知线，因为 parent goal 混合了多个主题
- 它分组了若干相关 task，而这些 task 不应该直接挂在 umbrella goal 之下

例如，viewer goal 之下的 UI 分支，或 distribution goal 之下的 packaging 分支。

closure 规则：

- 当它仍在分组当前战术工作时，保持 `sub-goal` active
- 当该执行分支完成且不再需要独立工作线时，关闭 `sub-goal`
- 优先开启新的 `sub-goal`，而不是默认把不相关任务直接挂到 umbrella goal 下

### 规范结构规则

当新工作出现时，按这个顺序分类：

1. 如果工作打开或改变了一条 durable 的战略通道，创建 `goal`
2. 如果工作属于现有 goal，但需要自己的主题分支，创建 `sub-goal`
3. 如果工作是某条现有线中的具体可执行单元，创建 `task`
4. 仅当工作主要是为了帮助关闭当前 parent task 时，才创建 `subtask`

操作上：

- 保持战略 goals 打开
- 在其下以 `sub-goals` 开启新的 UI 或产品线
- 新任务优先附着到最近的战术线，而不是默认挂在 umbrella goal 下
- 当操作意图是“在这里打开一条战术线并开始工作”时，使用 `ctx line open`

viewer 规则：

- 日常 `Working` focus 应优先展示 `task -> sub-goal -> parent goal`
- 最近的战术线应承担最多的视觉权重
- umbrella strategic goals 可以继续在仓库中 active，但不应主导 working 视图
- 当没有 task 直接挂在其下时，战略 goal 在 `Working` 中主要应作为 active tactical line 的轻量背景，而不是主角

### `Issue`

`issue` 是任何问题、摩擦或负面观察。

示例：

- viewer 在 refresh 时丢失 visual focus
- git 抛出了 `index.lock`
- 某个 hypothesis 被链接到了错误的 task

`issue` 不会自动意味着需要一个新 task。

### `Gap`

`gap` 是当前状态与目标状态之间一个具体、可命名、可行动的差距。

示例：

- current：viewer 有 focus presets
- should：viewer 在 reload 后应保留这些 focus
- gap：`graph focus does not persist across reloads`

`gap` 足够精确，足以支撑一项工作。
但它仍然是差距，不是实现本身。

### `Task`

`task` 是关闭某个 gap 的可执行工作，或一个真正的 roadmap unit。

示例：

- gap：`the graph focus does not persist across reloads`
- task：`Persist graph focus selection in viewer`

### 翻译规则

当新内容出现时，CTX 应按这个顺序翻译：

1. 描述 `issue`
2. 陈述具体 `gap`
3. 决定它是否需要 `task`、`subtask`、`blocker`，还是仅仅需要 `evidence`

在 gap 还没有被命名好之前，不要直接从 issue 跳到 task。

## 当前问题

CTX 已经有：

- `goal`
- `task`
- `hypothesis`
- `evidence`
- `decision`
- `conclusion`
- hypothesis scoring
- `ctx next` scoring

但它仍缺少一层，用来在新工作出现时分类“这到底是什么工作”。

没有这层时，会出现这些失败：

- 打开重复任务
- 为其实只是局部 bug 的问题创建 hypotheses
- 明明附近 blocker 更重要，却选择了全局高分任务
- 因上下文切换过早而丢失认知连续性

## 主规则

在创建新工作之前，CTX 应先判断它是哪一类工作。

建议分类：

- `NewTask`
- `Subtask`
- `Duplicate`
- `Blocker`
- `RelatedFollowup`
- `EvidenceOnly`

## 工作类型

### `NewTask`

用于真正的新 roadmap unit。

标准：

- 具有独立产品价值
- 不在语义上依赖某个单一 active task
- 值得有自己的 evidence 和 conclusion
- 不是现有工作的小范围延伸

### `Subtask`

用于为了关闭 parent task 而存在的工作。

标准：

- 起源于一个 active task
- 有独立可验证的结果
- 阻塞或解锁 parent task
- 作为独立 roadmap item 的意义不大

不要在这些情况下打开 `subtask`：

- 它只是一个琐碎实现步骤
- 它只是操作性备注
- 它只是 evidence

### `Duplicate`

用于当新工作已经被现有实体表示时。

标准：

- 实质意图相同
- 底层问题相同
- 预期结果相同
- 只有 wording 或观察时间不同

此时不要再创建另一项 task。

应该：

- 链接现有实体
- 记录额外 evidence
- 视情况标记为 `duplicate-of`

### `Blocker`

用于某件事阻止了当前或附近 task 的关闭。

标准：

- 出现在 active task 的执行期间
- 阻止验证或完成
- 现在解决它，比跳到全局优先级更有价值

`Blocker` 并不总是要求新 task。
有时它只需要：

- evidence
- 一个 decision
- 或一个小 subtask

### `RelatedFollowup`

用于它不是 duplicate，但也不是完全独立的新 task 的情形。

标准：

- 延展现有工作线
- 细化 hypothesis
- 给已经工作过的 feature 增加自然改进
- 强依赖同一上下文

### `EvidenceOnly`

用于没有新工作、只有新知识的情形。

示例：

- 一个观察到的失败
- 一个环境限制
- 一次测量
- 一次 hypothesis validation

此时应将其记录为 `evidence`，而不是 `task`。

## 去重规则

在创建新 task 前，CTX 应执行这样一个概念检查：

1. 是否已经存在等价 `task`
2. 是否已有 `hypothesis` 覆盖了这个意图
3. 问题是否发生在一个 active task 内部
4. 新项是否阻塞当前 task
5. 这项工作是否只是当前线程的局部延伸
6. 问题是否已解决，仅缺少认知 closeout

预期结果：

- 如果已有等价 task：链接它
- 如果它起源于 active task 内部：标记为 `subtask` 或 `blocker`
- 如果它是自然延续：标记为 `related-followup`
- 如果它只是增加知识：记录为 `evidence`
- 如果它开启了真正的 roadmap unit：标记为 `new task`

## 近程规则

优先级不应只来自全局分数。

CTX 还应考虑它与 active node 的距离。

示例：

- 你正在关闭一个 task
- 这时出现一个阻塞验证的小 bug
- 另一个 task 具有更高的全局分数

很多情况下，切换上下文反而更差，不如先解决局部 blocker。

因此区分这两种优先级很有意义：

- `globalPriority`
- `executionPriority`

## 建议评分

今天的 `ctx next` 已经有相对合理的全局分数。

下一步演进应增加一层执行分数：

```text
executionScore =
  globalPriorityScore
+ proximityScore
+ unblockScore
- contextSwitchCost
- duplicationRisk
```

### `globalPriorityScore`

来源于：

- goal priority
- task state
- related hypothesis score

### `proximityScore`

衡量新工作与 active node 的距离。

提高 proximity 的例子：

- 同一个 task
- 同一个 goal
- 同一个 hypothesis
- 同一个 commit thread

### `unblockScore`

如果解决这项工作能解锁验证或 closure，就应提高。

### `contextSwitchCost`

当切换到该工作会打断认知连续性时提高。

示例：

- 更换 goal
- 切换产品区域
- 放弃一个接近关闭的 task

### `duplicationRisk`

当工作与现有 task、hypothesis 或 decision 相似时提高。

## 模型中缺失的关系

为了更好实现这一模型，CTX 应增加显式关系：

- `parent-task`
- `subtask-of`
- `blocks`
- `blocked-by`
- `duplicate-of`
- `follow-up-to`
- `refines`
- `discovered-during`

有了这些关系，系统就能解释为什么某件新工作不值得拥有一个新的根 task。

## 推荐操作规则

当新工作出现时：

1. 检查是否已存在等价项
2. 如果存在，链接而不是重复创建
3. 如果它起源于 active task 内部，评估 `subtask` 或 `blocker`
4. 如果它只是增加知识，记录 `evidence`
5. 只有当它真的开启了另一个 roadmap unit 时才创建新 `task`

## 有用的未来命令

为支持这个模型，CTX 可以增加类似命令：

- `ctx work classify`
- `ctx work attach --duplicate-of <id>`
- `ctx work attach --blocked-by <id>`
- `ctx task add --parent <taskId>`
- `ctx next --context <nodeId>`

示例：

```powershell
ctx next --context task:<taskId>
```

这将使下一步选择不仅依赖全局分数，还依赖连续性。

## 结论

CTX 的正确演进不只是“更好的打分”。

而是从：

- “什么分最高”

转向：

- “在当前线程里，我们现在应该做什么，才能不重复工作也不打断连续性”

这需要：

- 正式的工作分类
- 创建 task 之前先去重
- 真正的 subtasks
- 显式 blockers
- 基于 proximity 与 unblock ability 的优先级
