# 命令采用与覆盖率
如果语言模型及其代理丢失了上下文，这就是你需要的工具。

本文总结 CTX CLI 在 self-hosted 工作区中的真实使用情况，以及如何处理那些使用较少或仍然偏冷的命令。

目标不是为了覆盖率而追求覆盖率。

规则：

- 先采用能改善日常流程的命令
- 再验证能解锁未来工作的命令
- 最后才覆盖外围或罕见 surface

## 当前快照

来源：

- `ctx usage summary`
- `ctx usage coverage`

该仓库中的观察状态：

- `totalKnownCommands`: `49`
- `usedCommandCount`: `31`
- `unusedCommandCount`: `21`
- `coveragePercentage`: `63.27`

## 热命令

这些已经成为真实产品流的一部分：

- `evidence add`
- `commit`
- `status`
- `audit`
- `conclusion add`
- `next`
- `hypo add`
- `hypo update`
- `task update`
- `graph lineage`
- `decision add`
- `task add`

解读：

- CTX 已经被强烈用于可追溯性、认知闭环与一致性
- 当前主循环是：
  - observe
  - 记录 evidence
  - 更新 state
  - 用 conclusion closeout
  - cognitive commit

## 温命令

这些命令有使用，但还未进入主导循环：

- `log`
- `graph summary`
- `task list`
- `task show`
- `graph show`
- `goal list`
- `hypo rank`
- `hypo show`
- `usage summary`
- `usage coverage`
- `decision list`
- `diff`
- `metrics show`
- `thread reconstruct`
- `version`

解读：

- 这些 surface 已存在且有价值
- 但它们还没有定义日常主要流程
- 很多更依赖 inspection 或 debugging，而不是正常 closeout

## 冷命令或尚未使用的命令

目前还没有真实使用的 surface：

- `branch`
- `checkout`
- `conclusion show`
- `context`
- `doctor`
- `evidence list`
- `evidence show`
- `export`
- `goal add`
- `goal show`
- `graph export`
- `hypo list`
- `import`
- `init`
- `merge`
- `packet list`
- `packet show`
- `provider list`
- `run`
- `run list`
- `run show`

解读：

- 有些命令之所以冷，是因为当前流程还不需要它们
- 有些命令冷，是因为产品还没有在真实 self-hosting 中推动它们
- 有些命令本来就是 edge capabilities，不属于 happy path

## 并非所有冷命令都同样重要

应按价值分层。

### Tier 1：贴近日常流程的高价值命令

优先验证：

- `goal add`
- `goal show`
- `hypo list`
- `evidence list`
- `evidence show`
- `conclusion show`
- `doctor`
- `context`

原因：

- 非常接近日常主流程
- 能补齐 inspection 与操作缺口
- 不会引入太大的额外复杂度

### Tier 2：对 CTX 作为完整系统有结构价值的命令

稍后验证：

- `branch`
- `checkout`
- `merge`
- `graph export`
- `export`
- `import`

原因：

- 它们对完整 CTX 模型很重要
- 但尚未进入日常 self-hosting 路径
- 需要受控场景与有意测试

### Tier 3：未来或专用价值

延后处理：

- `run`
- `run list`
- `run show`
- `provider list`
- `packet list`
- `packet show`

原因：

- 依赖 providers、integrations 或更高级工作流
- 目前并不是 self-hosting 的主要摩擦点

### Tier 4：现在不应优先处理

- `init`

原因：

- 对已初始化仓库几乎没有价值
- 更适合 demo、onboarding 或新仓库场景

## 推荐优先级规则

当要决定先处理哪个冷命令时，按这个顺序：

1. 能减少当前日常流程摩擦的命令
2. 能补完整一个已经被大量使用的命令家族
3. 能解锁结构性系统能力的命令
4. 专用或集成型命令

不要按以下标准排序：

- 单纯看未使用命令数量
- 为了提高 coverage 而忽略操作价值

## 当前不完整的命令家族

### Goals

当前：

- `goal list` 已被使用
- `goal add` 与 `goal show` 尚未使用

含义：

- 这个家族存在，但还没有完全整合进真实流程

### Evidence

当前：

- `evidence add` 是 CTX 中最常用的命令
- `evidence list` 与 `evidence show` 尚未使用

含义：

- 这是一个明显缺口
- 写入已经被采用，但 evidence inspection 还没有进入循环

### Conclusions

当前：

- `conclusion add` 和 `conclusion update` 已被使用
- `conclusion show` 尚未使用

含义：

- closure 家族已经活跃
- 但读取 surface 尚未进入主循环

### Branching

当前：

- `branch`、`checkout`、`merge` 仍未使用

含义：

- 分支语义已存在于 viewer 和模型中
- 但真实流程还没有日常使用 cognitive branches

### Providers and runs

当前：

- `run*` 和 `provider list` 尚未使用

含义：

- CTX 已经强烈作为结构化认知系统运行
- 但还没有日常成为 model-run orchestrator

## 推荐计划

### Step 1

验证并记录：

- `evidence list`
- `evidence show`
- `goal add`
- `goal show`
