# CTX 启动式认知索引

CTX 不应把 bootstrap 当成扁平的实体抽取。

它的目标是从现有材料中重建一条临时性的认知线程。

它应该帮助回答：

- 这些材料看起来在解决什么问题
- 它们依赖哪些候选假设
- 哪些片段构成支持性证据
- 哪些不确定性仍然开放

## 初始表面

检查命令：

```powershell
ctx bootstrap map --from <path> [--mode auto|article|project] [--max-files <n>]
```

保守提升命令：

```powershell
ctx bootstrap apply --from <path> [--mode auto|article|project] [--max-files <n>] [--parent-goal <goalId>]
```

## 设计规则

Bootstrap 必须保留“思想”，而不只是“实体”。

坏的 bootstrap：

- `Goal exists`
- `Task exists`
- `Hypothesis exists`

好的 bootstrap：

- `材料似乎在尝试解决 X`
- `它似乎依赖假设 Y`
- `这些摘录可作为 Y 的证据`
- `这些问题仍然开放`

## 持久化规则

`ctx bootstrap map` 不应直接写入最终 CTX。

它应先返回一个可审查的临时地图。

`ctx bootstrap apply` 的作用，是在不假装 bootstrap 已经理解全部真实意图的前提下，打开一条临时工作线。

它应该：

- 只选择最强的候选线程
- 打开一个临时 goal
- 建立一个 review task
- 只提升有限数量的 hypotheses 和 evidence
- 明确把这些结果标记为 bootstrap/provisional

它不应该：

- 把整个源码树都导入 CTX
- 自动接受 decisions
- 像已经验证完成一样关闭该线程

`apply` 之后的正确动作，是继续检查、验证、拒绝或加强这条线程，然后再决定是否写入 accepted decision 或 closing conclusion。

