# CTX Viewer 指南
????????????????,??????????

## 是什么

`CTX Viewer` 是一个用于查看 `.ctx` 仓库的可视化界面。

它不是编辑器，主要用于查看：

- 随时间变化的认知历史
- 当前分支
- 选中的提交或快照
- 目标、任务、假设、证据、决策、结论的图谱
- 假设的当前排名

可以把它理解为：

- 版本控制式历史
- 认知图谱检查器

## 界面区域说明

界面分为三大区域。

### 1. 顶部栏

元素：

- `Repository`
- `Branch`
- `Load`
- `Refresh`
- `Auto-refresh`

功能：

- `Repository`：存在 `.ctx/` 的本地路径
- `Branch`：要查看的认知分支
- `Load`：加载仓库并刷新界面
- `Refresh`：重新读取当前仓库，不刷新页面
- `Auto-refresh`：每隔几秒自动刷新

默认行为：

- 如果没有保存或输入仓库路径，viewer 会先检查 `CTX_VIEWER_DEFAULT_REPOSITORY_PATH` 或 `Viewer__DefaultRepositoryPath`
- 如果没有配置默认路径，viewer 会从项目最近的 `.git` 目录解析 fallback 根路径
- 在这个 self-host 仓库中，该 fallback 根路径解析为 `<repo-root>`
- 默认分支 `main`
- `Auto-refresh` 默认启用（浏览器记忆优先）
- 记住最后的仓库路径与分支
- 按模式 (`History`/`Split`/`Graph`) 记住面板宽度

示例路径：

- `<repo-root>\\examples\\viewer-demo`
- `<path-to-your-ctx-repository>`

## 2. 左侧面板

上方是摘要，下方是时间线。

### Summary cards

- `Branch`：当前分支
- `Head`：分支指向的提交
- `Branches`：分支数量
- `Timeline`：可见提交数量
- `Open Tasks`：未完成任务
- `Closed Tasks`：已完成任务
- `Nodes`：图谱节点数量
- `Edges`：图谱关系数量

### Top Hypotheses

显示最高 `score` 的假设：

- `score`：优先级
- 短 ID
- 假设文本
- `p`：概率
- `i`：影响

状态信息：

- `Last loaded`
- `Auto-refresh on/off`

说明：

- `impact`、`evidenceStrength`、`costToValidate` 为 `0` 的假设通常较旧
- 主要界面现在会按视口高度分配空间，滚动发生在面板内部

### Tasks

显示当前工作与已关闭任务：

- `Active`
- `Closed`

每项包含：

- `state`
- 短 ID
- 标题
- 关联 goal
- 假设数量
- 依赖（如有）

点击任务会：

- 跳到对应 Task 节点
- 回到 `working` 上下文
- 保证任务状态在过滤中可见

### History

`History` 为 branch-first 视图：

- 侧边 `Branches`
- `Order` 选择 `Newest first` 或 `Oldest first`
- 每个分支一个分组
- 每行固定列：
  - `Graph`
  - `Description`
  - `Changes`
  - `Date`
  - `Author`
  - `Model`
  - `Commit`

`Changes` 表示认知实体变化数量（非文件变化）。

每行会尽量显示主要认知路径：

- `Goal`
- `Task`
- `Hypothesis`
- `Decision`/`Conclusion`

### Evidence 详情

证据默认折叠，展开后显示标题与摘要。

### Split / Graph

`Split` 和 `Graph` 使用简化的提交导航器，保留：

- `Order`
- 分支过滤
- 选择提交
- 视觉车道

## `working` 的含义

`working` 不是提交，代表尚未提交的当前 `.ctx` 状态。

## 3. 中央面板：Trace Graph

显示选中提交或 `working` 的认知图谱。

包含 `Task states` 过滤和焦点预设：

- `All`
- `Working`
- `Thinking`
- `Closed`

这些预设可叠加，浏览器会记住：

- `Repository`
- `Branch`
- 焦点组合与状态选择

过滤只影响渲染，不会删除数据。

## 4. 右侧面板

### Commit

显示：

- 消息
- 完整 ID
- 分支
- 作者
- 模型
- 日期
- 快照
- 变化数量
- parents
- diff 摘要

### Model metadata

支持 `modelName` / `modelVersion`，可通过环境变量提供：

- `CTX_MODEL_NAME`
- `CTX_MODEL_VERSION`

### Node

点击节点可查看：

- `incoming`
- `outgoing`
- `connectedNodes`

## 分支的含义

分支代表认知轨迹，不只是代码差异：

- branch = 推理/执行轨迹
- task = 轨迹内具体工作
- hypothesis = 对任务的预期
- evidence/decision/conclusion = 该轨迹内的验证与闭环

建议检查：

1. `ctx audit`
2. `ctx graph lineage --goal <goalId>` / `--task <taskId>`
3. 短期分支应以结论和认知提交收尾

建议命名：

- `main`
- `feature/*`
- `research/*`
- `experiment/*`

## 当前限制

当前 Viewer 能做：

- 加载 `.ctx`
- 查看历史与图谱
- 查看假设排名
- 手动/自动刷新
- 浏览提交与节点

当前还不支持：

- UI 直接编辑
- 可视化 merge
- 冲突可视化
- 完整 Git 图布局
- 并排比较提交

