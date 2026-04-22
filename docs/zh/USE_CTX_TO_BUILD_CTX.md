# 用 CTX 构建 CTX
如果语言模型及其代理丢失了上下文，这就是你需要的工具。

## 目标

把 CTX 本身作为产品演进的官方认知仓库。

## 在这个过程中 CTX 应该保存什么

- 产品目标
- 结构化 backlog
- 价值与架构 hypotheses
- 来自真实测试的 evidence
- 技术与产品 decisions
- 每次迭代的 conclusions
- 每次 commit 的认知快照

## 当前仍缺少什么

- UI 中更好的 commit 比较
- 更丰富的 timeline 父子导航
- run 与 artifacts 的更强关联
- 更明确的 hypothesis scoring 模型
- 更紧凑的日常 CTX 操作流

## 推荐操作流

### 1. 打开循环

```powershell
ctx
ctx next
```

只有在当前状态不够明确时，才用 `ctx status`、`ctx audit`、`ctx graph summary`、`ctx log` 深入检查。

### 2. 创建迭代 goal

例如：

- harden viewer
- improve cognitive diff
- define hypothesis scoring
- prepare V1 pilot

### 3. 拆成 tasks 与 hypotheses

每次迭代最好包含：

- 1 个清晰 goal
- 2 到 5 个 tasks
- 1 到 3 个重要 hypotheses

### 4. 工作中持续记录 evidence

例如：

- test result
- discovered limitation
- viewer feedback
- benchmark
- usage friction

### 5. 用 decision 和 conclusion 闭环

不要留下未回答的问题：

- 我们学到了什么
- 我们改了什么
- 为什么这么改
- 还有什么未关闭

### 6. Cognitive commit

每个有价值的工作块都应结束于：

```powershell
ctx commit -m "<block result>"
```

### 7. 自主继续

如果操作员或模型只收到 `continua`，应该：

1. 重新读取 `ctx`
2. 如有需要，再深入使用 `ctx status`、`ctx audit`、`ctx graph summary`、`ctx log`
3. 选择当前主导的 active goal
4. 选择最阻塞或最有价值的 task
5. 产出真实 evidence
6. 记录 conclusion
7. 关闭一次 cognitive commit

如果 CTX 仓库已经明确了下一步，就不要等待手工指令。

## 建议的认知工作结构

### Goals

- usable local V1
- operational viewer
- cognitive capture method
- validation pilot

### Tasks

- improve UI
- package release
- install CLI
- test demos
- define scoring

### Hypotheses

- visual traceability reduces rework
- the viewer speeds up history comprehension
- simple scoring improves decision quality

## 推荐工作区

使用专门的 CTX 仓库来开发产品：

- `C:\sources\ctx-open`

## 预期结果

如果 CTX 能用来构建 CTX，它应该能证明：

- 迭代之间具备连续性
- 更少的上下文丢失
- 更明确的 decisions
- 更高质量的 backlog
- 产品为什么演进成现在的样子具有可追溯性
- 不依赖聊天也能继续自主工作
