# Bootstrap 认知索引

CTX 不应该把 bootstrap 索引当成原始实体抽取。

bootstrap map 的目的应该是：

- 重建一条临时性的思考线
- 识别当前材料似乎在处理什么问题
- 推断候选假设
- 收集可作为支持性证据的片段
- 暴露仍然开放的问题
- 给出后续刻意思考可能需要的任务

bootstrap 的输出从设计上就是 provisional。

它不等同于：

- 最终 CTX 图
- durable 的任务计划
- 对仓库真实状态的完整导入

## 为什么需要它

当 CTX 从一个现有目录、项目、文章或文档集合启动时，可能完全没有既有认知状态。

bootstrap surface 应该帮助代理恢复：

- 这些材料似乎想解决什么
- 它们似乎提出了什么 claim 或 hypothesis
- 哪些证据支持这些 claim
- 还有哪些不确定性没有关闭

如果没有这一层，CTX 很容易退化成两种坏结果之一：

- 一个没有起始线索的空仓库
- 一个只保留实体、却丢掉“想法”的扁平 schema dump

## 命令形态

初始命令：

```powershell
ctx bootstrap map --from <path> [--mode auto|article|project] [--max-files <n>]
```

保守提升表面：

```powershell
ctx bootstrap apply --from <path> [--mode auto|article|project] [--max-files <n>] [--parent-goal <goalId>]
```

## 输出形态

命令应该返回一个 provisional map，包含：

- `projectSummary`
- `candidateThreads`
- `workingProblem`
- `candidateHypotheses`
- `supportingEvidence`
- `possibleTasks`
- `openQuestions`
- `guidance`

面对高矛盾材料时，这个 provisional map 还应该能够返回：

- `candidateHypothesisSets`
- `conflicts`
- `sharedEvidence`
- `openTensions`

## 设计规则

bootstrap 必须保留“思路”，而不只是“实体”。

坏的 bootstrap：

- `Goal exists`
- `Task exists`
- `Hypothesis exists`

好的 bootstrap：

- `这些材料看起来是在尝试解决 X`
- `它似乎依赖假设 Y`
- `这些摘录充当了 Y 的证据`
- `这些不确定性仍然开放`

## 持久化规则

第一版不应该自动写入最终 CTX 实体。

bootstrap 输出应当先保持可审查。

然后由后续 surface 决定是否：

- 打开一条工作线
- 提升某个候选假设
- 有意附加证据
- 或丢弃弱 bootstrap 推断

## Apply 规则

`ctx bootstrap apply` 的存在，是为了在“临时推断”和“durable CTX”之间搭桥，而不是假装 bootstrap 已经知道真相。

它应该：

- 只选择最强的候选 thread
- 打开一个 provisional goal
- 建立一个 review task
- 只提升有边界的一组 hypotheses 和 evidence
- 在 wording 和 trace tags 中明确标记该线是 bootstrap/provisional

面对高矛盾来源时，`apply` 不应默认合成出一个单一假设，如果来源本身就同时提出了多个仍然活着的解释。

在这种情况下，它应该能够：

- 提升多个并行 provisional hypotheses
- 尽可能分别按解释保留证据链接
- 显式保留未解决的冲突
- 把综合推迟到 review 决定 merge 或 supersede interpretations 之后

它不应该：

- 把整个 source tree 全量导入 CTX
- 自动接受 decisions
- 写出看起来像已经验证完成的 conclusions

`apply` 之后的正确动作不是立刻 closeout。

正确动作是：

- 检查被提升的那条线
- 验证或拒绝 hypotheses
- 有意增强 evidence
- 然后再决定是否写入 accepted decision 或 closure

## Bootstrap v2 方向

bootstrap 的下一步设计方向是 `hypothesis isolation`。

这意味着：

- 先保留 competing interpretations
- 之后只有在 explicit review 下才进行 synthesize

坏的矛盾处理方式：

- 检测到多个解释
- 把它们压成一个看似连贯的“最佳解释”

更好的矛盾处理方式：

- 保留 `Hypothesis A`
- 保留 `Hypothesis B`
- 保留 `Hypothesis C`
- 让每个假设保留自己的 evidence、score 和后续 partial conclusion

这对以下材料尤其重要：

- 科学或准科学写作
- 含糊的技术设计文档
- 存在多个合理解释的 postmortem
- 任何“正确输出应该是稳定张力而不是过早综合”的来源
