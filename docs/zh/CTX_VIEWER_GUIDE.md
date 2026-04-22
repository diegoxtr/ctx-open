# CTX Viewer 指南
如果语言模型及其代理丢失了上下文，这就是你需要的工具。

## 它是什么

`CTX Viewer` 是一个用于检查 `.ctx` 仓库的可视化界面。

它不是编辑器。它的主要作用是帮助你看到：

- 随时间变化的认知历史
- 当前正在查看的 branch
- 选中的 commit 或 snapshot
- 由 goals、tasks、hypotheses、evidence、decisions、conclusions 构成的图谱
- 当前 hypothesis ranking

可以把它理解为：

- 一个版本控制式的历史界面
- 加上一个认知图谱检查器

## 屏幕区域说明

界面分为三个主要区域。

### 1. 顶部栏

元素：

- `Repository`
- `Branch`
- `Load`
- `Refresh`
- `Auto-refresh`

含义：

- `Repository`：包含 `.ctx/` 的本地路径
- `Branch`：想查看的认知 branch
- `Load`：加载仓库并刷新整个视图
- `Refresh`：在不刷新页面的情况下重新读取当前仓库
- `Auto-refresh`：每隔几秒自动刷新

当前默认行为：

- 如果没有保存或输入仓库路径，viewer 会先检查 `CTX_VIEWER_DEFAULT_REPOSITORY_PATH` 或 `Viewer__DefaultRepositoryPath`
- 如果没有配置默认路径，就从最近的项目 `.git` 根目录推断默认根路径
- 在这个 self-hosting 仓库中，该 fallback 根路径解析为 `C:\sources\ctx-open`
- 默认认知 branch 是 `main`
- `Auto-refresh` 默认开启，除非浏览器已经记住你把它关掉
- 浏览器会记住最近使用的 `Repository` 与 `Branch`
- 也会记住 `Auto-refresh` 偏好
- 左右面板可以通过垂直分隔条调整宽度，并按模式（`History`、`Split`、`Graph`）分别保存
- 面板也可以折叠成窄 rail 状态，这个状态同样会按模式保存

示例仓库：

- `C:\sources\ctx-open\examples\viewer-demo`
- `C:\ctx\workspace\ctx-self-host`

示例 override：

```powershell
$env:CTX_VIEWER_DEFAULT_REPOSITORY_PATH = "C:\ctx\workspace\ctx-self-host"
dotnet run --project .\Ctx.Viewer --urls http://127.0.0.1:5271
```

### 2. 左侧面板

左侧先显示总体摘要，然后显示 timeline。

主要内容：

- Summary cards
- Top Hypotheses
- Tasks
- History

`History` 现在是 branch-first：

- `working` 永远在最上面
- branches 单独分组
- 可按 `Newest first` / `Oldest first` 排序
- 每行显示 Description、Changes、Date、Author、Model、Commit

`Changes` 表示认知实体变化量，不是 Git 文件改动量。

### 3. 中央图谱

中央区域是 `Trace Graph`。

它显示：

- 当前 `Working context`
- 或某个选定 commit 的认知图谱

常见 focus preset：

- `All`
- `Working`
- `Thinking`
- `Closed`

常见过滤：

- `Focus`
- `Primary lineage only`
- `All active lines`
- `Show interpretation relations`
- `Task states`

当前 working 图的可读性规则：

- 优先显示当前 task 的最近战术线
- 若 task 隶属于某个 sub-goal，该 sub-goal 保持可见
- umbrella goals 不会在没有直接 task 指向时主导 working 图
- recently committed 的线可以被保留为上下文，而不是在 commit 后立即消失
- 新节点的绿色高亮会保持到用户点击为止，而不是按时间自动消失

### 4. 右侧详情面板

`Details` 现在使用标签页，而不是把所有 surface 堆叠在一起：

- `Details`
- `Origin`
- `Playbook`
- `Hypotheses`
- `Interpretations`

含义：

- `Details`：所选 commit 的元数据与原始节点详情
- `Origin`：当前焦点的紧凑 `CognitiveTrigger` 来源
- `Playbook`：当前焦点的紧凑 `OperationalRunbook` 指导
- `Hypotheses`：当前 ranking 与 freshness 状态
- `Interpretations`：branch-like hypothesis semantics 的细节

`Interpretations` 用来显示：

- sibling hypotheses
- lineage group
- branch state / branch role
- competing / merged / superseded relations
- direct vs shared evidence

## 关键阅读规则

### `working` 是什么

`working` 不是一个 commit。

它表示当前 `.ctx` 中还没有被关闭成 cognitive commit 的状态。

### `commit` 是什么

`ctx commit` 不是原始 thought stream。

它表示 durable cognitive state transition。

因此：

- `Working context` = cognition in motion
- `Commit history` = durable snapshots

如果某个状态不在 `Working context`，它就应该在 `Commit history`。

## 推荐使用方式

1. 先加载仓库
2. 从 `working` 开始看当前活动状态
3. 再查看最近的 cognitive commits
4. 如需理解 competing hypotheses，切到 `Interpretations`
5. 如需看来源或操作指导，切到 `Origin` 或 `Playbook`

## 结论

CTX Viewer 的目标不是编辑 `.ctx`。

它的目标是让你能快速看见：

- 当前在做什么
- 为什么在做
- 哪些线已经关闭
- 哪些解释彼此竞争
- 推理是如何演化成 durable history 的
