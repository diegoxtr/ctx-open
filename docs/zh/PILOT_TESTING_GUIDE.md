# CTX 试点测试指南

如果语言模型及其代理丢失了上下文，这就是你需要的工具。

## 1. 目标

本文定义如何运行一个初始 CTX pilot，以验证该方案是否已经足够成熟，可以演进为面向技术用户的 V1 候选。

目标不仅是验证软件能运行，还要确认：

- 工作模型是否可理解
- 操作流程是否带来真实价值
- 认知结构是否改善了 AI 使用
- 工具是否减少返工
- 迭代成本是否合理

## 2. Pilot 范围

pilot 应聚焦于：

- 本地 CLI 使用
- 真实或接近真实的技术案例
- 对 goals、tasks、hypotheses、evidence、decisions、conclusions 的结构化记录
- cognitive commits
- context generation
- runs
- diff 和 merge review
- metrics、packets 与 runs 的检查

它暂时不应包括：

- 远程集成
- 真实多用户使用
- 图形界面
- 企业场景
- 复杂的外部自动化

## 3. 推荐测试者画像

理想测试者应：

- 具备技术背景
- 习惯使用 CLI
- 能将推理组织成步骤
- 能解释某个决策为什么好或不好
- 愿意记录清晰反馈

更理想的是：

- 架构师
- 高级开发者
- 平台工程师
- 技术研究者
- 高频 LLM 用户

## 4. 环境准备

### 4.1 要求

- 安装 .NET SDK 8
- CTX 仓库可成功构建
- 具备控制台访问
- 可选的真实 runs provider keys：
  - `OPENAI_API_KEY`
  - `ANTHROPIC_API_KEY`

即使没有 keys，系统仍可通过 offline fallback 进行测试。

### 4.2 初始验证

```powershell
dotnet build Ctx.sln
dotnet test .\Ctx.Tests\Ctx.Tests.csproj
```

预期：

- build 成功
- tests 成功
- 没有关键环境错误

### 4.3 准备一个 pilot 仓库

```powershell
dotnet run --project .\Ctx.Cli -- init --name CTX-PILOT --description "Initial validation pilot"
dotnet run --project .\Ctx.Cli -- status
```

预期：

- 创建 `.ctx/`
- `status` 显示 `main`
- 无初始化错误

## 5. Pilot 目标问题

pilot 期间应回答：

1. 用户是否理解如何建模问题？
2. 结构化认知流是否比自由聊天更有帮助？
3. 是否更容易恢复工作？
4. commits 和 diffs 是否有用？
5. merges 和 conflicts 是否可解释？
6. context/run 成本是否可接受？
7. 用户是否愿意在真实案例中使用 CTX？

## 6. 推荐场景

### 场景 1 - 架构分析

目标：

- 用 hypotheses 与 evidence 评估一个架构决策

### 场景 2 - 技术调查

目标：

- 结构化一个 root cause investigation

### 场景 3 - AI 引导迭代

目标：

- 衡量 CTX 是否减少多次 AI runs 之间的返工

## 7. 推荐测试流程

### 步骤 1 - 创建 goal

```powershell
dotnet run --project .\Ctx.Cli -- goal add --title "Evaluate module X architecture" --description "Define primary alternative"
```

### 步骤 2 - 创建 tasks

```powershell
dotnet run --project .\Ctx.Cli -- task add --title "Analyze option A" --description "Pros and risks"
dotnet run --project .\Ctx.Cli -- task add --title "Analyze option B" --description "Pros and risks"
```

### 步骤 3 - 记录 hypotheses

```powershell
dotnet run --project .\Ctx.Cli -- hypo add --statement "Option A reduces operational complexity" --rationale "Fewer components"
dotnet run --project .\Ctx.Cli -- hypo add --statement "Option B scales better long term" --rationale "More flexibility"
```

### 步骤 4 - 记录 evidence

```powershell
dotnet run --project .\Ctx.Cli -- evidence add --title "Initial benchmark" --summary "A shows lower latency" --source "local test" --kind Benchmark --supports hypothesis:<hypothesisId>
```

### 步骤 5 - 做出 decisions

```powershell
dotnet run --project .\Ctx.Cli -- decision add --title "Adopt option A for pilot" --rationale "Lower complexity and favorable evidence" --state Accepted --hypotheses <hypothesisId> --evidence <evidenceId>
```

### 步骤 6 - 记录 conclusion

```powershell
dotnet run --project .\Ctx.Cli -- conclusion add --summary "Proceed with A for faster validation" --state Accepted --decisions <decisionId> --evidence <evidenceId>
```

### 步骤 7 - 执行一次 run

```powershell
dotnet run --project .\Ctx.Cli -- run --provider openai --purpose "Review decision and propose risks"
```

### 步骤 8 - 创建 cognitive commit

```powershell
dotnet run --project .\Ctx.Cli -- commit -m "pilot architecture scenario"
```

### 步骤 9 - 检查结果

```powershell
dotnet run --project .\Ctx.Cli -- log
dotnet run --project .\Ctx.Cli -- metrics show
dotnet run --project .\Ctx.Cli -- run list
dotnet run --project .\Ctx.Cli -- packet list
```

## 8. 测试者清单

在 pilot 过程中标记：

- 我无需帮助就完成了 repo 初始化
- 我理解如何创建 goals、tasks、hypotheses
- 我理解如何把 evidence 链接到 hypotheses 或 decisions
- 我清楚如何记录 decisions 和 conclusions
- `status`、`log`、`diff`、`metrics` 输出是可理解的
- 生成的 packets 是有用的
- run output 可以复用
- cognitive commits 能准确表示达成状态
- 这套流程优于 ad-hoc prompts
- 我会把 CTX 用在真实案例中

## 9. 评估标准

### 9.1 功能

成功的条件：

- 流程可以完成，且不需要手工修改 JSON
- 没有阻塞性错误
- 命令返回一致结果
- artifacts 可追踪

### 9.2 可用性

可接受的条件：

- 在有限解释下模型可理解
- CLI 不造成严重困惑
- 命令命名感觉合理
- 结构化思考虽带来约束，但不过度摩擦

### 9.3 价值

有前景的条件：

- 用户感觉上下文丢失减少
- 决策更容易解释
- 工作可以在暂停后恢复
- cognitive commits 有用
- 与 AI 的重复迭代减少

## 10. 要记录的指标

每个场景记录：

- 总执行时间
- 命令数量
- goals/tasks/hypotheses/evidence/decisions/conclusions 数量
- run 数量
- tokens 使用量
- 成本估算
- 重复迭代次数
- 检测到的认知冲突
- 主观有用性

## 11. 测试者反馈模板

通用信息：

- tester name
- date
- scenario
- duration

评价内容：

- 处理的问题
- 达成的目标
- 最有用的命令
- 最令人困惑的命令
- 流程中最有价值的部分
- 流程中摩擦最大的部分
- 缺失的信息
- 缺失的命令或帮助
- 是否会在此类案例中使用 CTX：yes/no

建议 1–5 分项：

- model clarity
- ease of use
- value of structured context
- cognitive commit usefulness
- diff usefulness
- merge usefulness
- metrics usefulness
- likelihood of reuse

## 12. Pilot 后的决策

### 进入 V1 candidate

如果：

- 没有严重 blocker
- testers 理解流程
- 感知价值高
- 成本合理

### 继续内部迭代

如果：

- 概念被认可
- CLI / UX 仍带来过多摩擦

### 在 V1 前重新评估

如果：

- 感知价值低
- 模型不清晰
- 操作成本过高
- artifacts 没有改善决策

## 13. 实际建议

初始 pilot 应使用：

- 3 个 scenarios
- 3–5 位技术 testers
- 短周期
- 强制书面反馈
- 最终复盘会议

## 14. 预期结果

如果执行得当，你应得到：

- 关于价值与摩擦的具体证据
- V1 前真实可执行的改进清单
- 一套客观依据，用来判断 CTX 是否能进入受控产品测试
