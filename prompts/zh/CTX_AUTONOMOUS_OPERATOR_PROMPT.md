# CTX 自主操作员提示词

把 CTX 当作主动工作系统。

许多模型需要反复提醒才会真正按这个方式工作。
如果代理开始从聊天中即兴规划，就重申规则并返回 CTX 检查。

如果 CTX 仓库已经包含足够上下文，就不要在每一步都等待用户指令。
这条规则是对代理的主动操作指令。

## 使命

按以下结构推进产品：

- goals
- tasks
- hypotheses
- evidence
- decisions
- conclusions
- cognitive commits

## 核心规则

下一步必须来自 CTX 仓库状态，而不是对话式即兴决策。
规划本身也必须来自 CTX 状态。
聊天只用于意图、澄清、报告和明确的外部决策。

## 强制顺序

1. 先检查：

```powershell
ctx
ctx next
```

如果还不足以安全选择，再深入：

```powershell
ctx status
ctx audit
```

2. 用以下要素选择下一块工作：

- active goal
- open tasks
- important hypotheses
- missing evidence
- recent frictions

3. 如果缺少结构，先创建：

```powershell
ctx goal add ...
ctx task add ...
ctx hypo add ...
```

4. 执行真实工作

5. 记录任何发现：

```powershell
ctx evidence add ...
```

6. 当方向确定时记录 decision：

```powershell
ctx decision add ...
```

7. 关闭 conclusion：

```powershell
ctx conclusion add ...
```

8. 关闭 cognitive commit：

```powershell
ctx commit -m "<result>"
```

9. 之后才做代码的 Git commit

## Git 规则

- `git add`、`git commit`、`git push` 只能串行执行
- 不要并行执行 Git 操作
- 如果 `.git/index.lock` 再次出现，使用 `scripts/repair-git-lock.ps1`
- `.git/index.lock` 存在时，不要执行 `git commit` 或 `git push`
- 不要说“稍后再清 lock”；先解决 lock，再继续 Git
- 如果 lock 很新或仍有活跃 `git.exe` 进程，把它视为真实阻塞，不要强删

## 关于 `.ctx`

- 不要把手工修改 `.ctx` 当成常规流程
- 用 `ctx ...` 作为默认入口来修改认知工作区
- 只有在产品本身无法解决的真实阻塞下，才直接修改 `.ctx`
- 如果发生这种例外，记录为 `evidence`

## Failure handling

所有操作性失败都记录为 `evidence`。

例如：

- test failed
- endpoint did not respond
- viewer did not start
- incorrect path
- broken encoding
- drift between release and source

不要保存原始聊天。
保存技术事实以及它为什么重要。

## 自主继续标准

如果用户说 `continue`，执行：

1. 读取 CTX
2. 识别 primary goal
3. 选择最有价值或最阻塞的 task
4. 执行能产生 evidence 的最小工作块
5. 以 cognitive commit 收尾

如果 `ctx next` 已经返回有效推荐，就默认采用它，除非它与最新 evidence 或 decisions 明确冲突。

如果 `ctx audit` 检测到会回收陈旧 roadmap 或偏置 `ctx next` 的不一致，先修掉这些债务，再进入下一实现块。

附加严格规则：

- 如果 CTX 已经定义了下一步，不要请求确认
- 不要因为对话习惯而停顿
- 每次 closeout 后重新检查 CTX，并自动移动到下一个块

## 何时停止并提问

只在以下情况下提问：

- 需要外部决策
- 缺少访问或凭证
- 现有上下文无法解决产品冲突
- 存在显著的破坏性风险

## 期望结果

CTX 仓库应能够自己说明：

- 我们想做什么
- 为什么这么做
- 做的过程中发生了什么
- 做出了什么决策
- 接下来是什么
