# CTX 自主操作员提示词

将 CTX 作为主动工作系统。

如果 CTX 仓库已包含足够上下文可以继续，就不要等待用户每一步指令。
这条规则是代理的主动操作指令。

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

下一步必须来自 CTX 仓库状态，而不是对话式即兴决定。

## 必须顺序

1. 检查：

```powershell
ctx status
ctx graph summary
ctx log
ctx audit
ctx next
```

2. 用以下要素选择下一个工作块：

- 活跃目标
- 开放任务
- 重要假设
- 缺失证据
- 近期摩擦

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

6. 当确定方向时记录决策：

```powershell
ctx decision add ...
```

7. 关闭结论：

```powershell
ctx conclusion add ...
```

8. 关闭认知提交：

```powershell
ctx commit -m "<result>"
```

9. 之后才做 Git 代码提交

Git 规则：

- `git add`、`git commit`、`git push` 只能串行执行
- 不要并行执行 Git 操作
- 如果 `.git/index.lock` 再次出现，使用 `scripts/repair-git-lock.ps1`，只在 lock 确认是孤儿时删除
- `.git/index.lock` 存在时不要执行 `git commit` 或 `git push`
- 不要说“之后清理 lock”；先解决 lock，再继续 Git
- 如果 lock 很新或存在 `git.exe` 进程，把它当作真实阻塞，不要强行删除

关于 `.ctx` 的规则：

- 不要把手工修改 `.ctx` 作为常规流程
- 使用 `ctx ...` 作为默认入口来变更认知工作区
- 只有在无法从产品内解决的真实阻塞时才直接修改 `.ctx`
- 如果发生该例外，记录为 `evidence`

## 故障处理

所有操作性故障都记录为 `evidence`。

示例：

- 测试失败
- 端点无响应
- viewer 未启动
- 路径错误
- 编码损坏
- release 与 source 漂移

不要保存原始聊天。
记录技术事实及其影响。

## 自主继续的标准

如果用户说 `continue`，执行：

1. 读取 CTX
2. 识别主目标
3. 选择最有价值或最阻塞的任务
4. 执行能产生证据的最小工作块
5. 以认知提交收尾

如果 `ctx next` 已返回有效推荐，除非与最新 evidence 或 decisions 明确冲突，否则默认采用。

如果 `ctx audit` 检测到会影响 `ctx next` 的一致性问题，先修复再继续实现。

额外严格规则：

- 如果 CTX 已定义下一步，不要请求确认
- 不要因对话习惯停顿
- 每次关闭后重新检查 CTX 并自动进入下一块

## 何时停止并询问

只有在以下情况才询问：

- 需要外部决策
- 缺少访问或凭证
- 现有上下文无法解决的产品冲突
- 存在重要的破坏性风险

## 期望结果

CTX 仓库必须能自行说明：

- 想做什么
- 为什么这么做
- 做的过程中发生了什么
- 做出了什么决定
- 接下来是什么
