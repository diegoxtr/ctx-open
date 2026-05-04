# CTX CLI 命令
如果语言模型及其代理丢失了上下文，这就是你需要的工具。

本文档描述 `<repo-root>` 中当前公开的 CTX CLI 表面。

CTX 返回结构化 JSON，基本格式如下：

```json
{
  "success": true,
  "message": "short summary",
  "data": {}
}
```

## 约定

- 在认知仓库根目录运行命令。
- 本地源码开发通常使用：

```powershell
dotnet run --project .\Ctx.Cli -- <command>
```

- 已安装版本通常使用：

```powershell
ctx <command>
```

- `<goalId>`、`<taskId>`、`<hypothesisId>`、`<commitId>` 等 ID 来自之前命令的输出。
- 多数变更会先写入 `.ctx/working`、`.ctx/staging`、`.ctx/graph`，然后在后续认知提交中进入 durable history。

## 首次启动流程

不要从命令大全开始。
先判断你当前处于哪种仓库状态。

## 操作状态机

CTX 应作为一个小型状态机来操作，而不是无序命令清单。

| 当前状态 | 含义 | 下一条命令 |
|---|---|---|
| 尚未初始化 CTX 仓库 | 当前目录还没有 `.ctx/` | `ctx init --name "<project>"` |
| 已有 CTX 仓库且存在开放工作 | 仍有活动任务线 | `ctx next` |
| 已有 CTX 仓库且存在待收尾的认知变更 | `Working context` 仍然 dirty | `ctx closeout` |
| 已到 durable boundary | 当前块足够稳定，应进入 history | `ctx commit -m "<durable result>"` |
| 已有 CTX 仓库但没有开放工作 | 没有活跃 tasks，需要看 gap 或确认 closure | `ctx next` |

### 已存在的 CTX 仓库

当项目根目录已经有 `.ctx/` 时，先运行：

```powershell
ctx
```

然后按当前状态建议的下一条命令继续。只有在需要更深检查时，才使用 `ctx status`、`ctx audit`、`ctx graph summary` 或 `ctx log`。

### 新认知项目

当 `.ctx/` 尚不存在时，先初始化：

```powershell
ctx init --name "<project>"
```

之后分两种情况：

- 已有现成资料：

```powershell
ctx bootstrap map --from <path>
ctx bootstrap apply --from <path>
ctx next
```

- 纯新项目 / greenfield：

```powershell
ctx goal add --title "<goal>"
ctx task add --title "<task>" --goal <goalId>
ctx hypo add --statement "<hypothesis>" --task <taskId>
ctx next
```

## 最小操作循环

对大多数代理而言，正确的循环是：

```powershell
ctx
ctx next
```

在 `Working context` 中工作，然后：

```powershell
ctx closeout
ctx commit -m "<durable result>"
```

`ctx commit` 不是“想法日志”，而是 durable cognitive state transition。

## 核心入口命令

### `ctx`

无参数时显示 helper-first 操作指引，并指向规范命令参考。

```powershell
dotnet run --project .\Ctx.Cli --
```

### `ctx version`

显示产品版本与仓库格式版本。

```powershell
dotnet run --project .\Ctx.Cli -- version
```

### `ctx init`

在当前目录初始化认知仓库。

常用参数：

- `--name <project>`
- `--description <text>`
- `--branch <name>`

```powershell
dotnet run --project .\Ctx.Cli -- init --name "CTX Demo" --description "Sample repo" --branch main
```

### `ctx goal update`

更新 goal 的元数据或生命周期状态。

常用参数：

- `--title <text>`
- `--description <text>`
- `--priority <n>`
- `--state <Draft|Active|Validated|Completed|Superseded|Archived>`

```powershell
dotnet run --project .\Ctx.Cli -- goal update <goalId> --state Completed
```

### `ctx status`

显示当前仓库状态。

包括：

- 当前 branch
- `HEAD`
- `dirty`
- goals、tasks、hypotheses、decisions、evidence、conclusions、runs 的数量
- dirty 时的紧凑 pending 预览

```powershell
dotnet run --project .\Ctx.Cli -- status
```

### `ctx audit`

运行认知一致性审计。

建议把它当作深度检查表面，而不是默认启动入口。

### `ctx next`

根据当前仓库状态推荐下一块工作。

这是默认操作循环中的主入口之一。

### `ctx closeout`

解释当前 `working` 与 `HEAD` 之间还差什么。

用于：

- 提交前检查
- 判断是否已经跨过 durable boundary
- 决定是否应执行 `ctx commit`

### `ctx commit`

创建认知提交。

语义规则：

- `Working context` 保存活动推理
- `ctx commit` 保存 durable cognitive state transition
- 关闭 task 可以提示 commit，但不会自动强制 commit

## 其余命令

完整命令参考以英文版为准；本页优先保证：

- 启动模型清晰
- 状态机清晰
- 新仓库与已有仓库分流清晰
- `ctx`、`ctx next`、`ctx closeout`、`ctx commit` 的最小循环清晰

如果你需要完整选项、示例或较冷命令，请优先参考英文版：

- [CLI_COMMANDS.md](../CLI_COMMANDS.md)
