# CTX CLI 命令
????????????????,??????????

本文档描述 `<repo-root>` 中当前 CTX CLI 的命令表面。

CTX 返回结构化 JSON 输出，基本格式为：

```json
{
  "success": true,
  "message": "short summary",
  "data": {}
}
```

## 约定

- 从认知仓库目录运行命令。
- 本地开发常用格式：

```powershell
dotnet run --project .\Ctx.Cli -- <command>
```

- 已发布本地安装：

```powershell
ctx <command>
```

- `<goalId>`, `<taskId>`, `<hypothesisId>`, `<commitId>` 等来自先前命令输出。
- 多值列表使用逗号分隔。
- 变更通常影响 `.ctx/working`、`.ctx/staging`、`.ctx/graph`，并在后续认知提交中落盘。

## 通用命令

### `ctx`

无参数时显示基础帮助。

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

选项：
- `--name <project>`
- `--description <text>`
- `--branch <name>`

```powershell
dotnet run --project .\Ctx.Cli -- init --name "CTX Demo" --description "Sample repo" --branch main
```

### `ctx status`

显示当前仓库状态：

- 当前 branch
- `HEAD`
- `dirty` 状态
- goals/tasks/hypotheses/decisions/evidence/conclusions/runs 计数

```powershell
dotnet run --project .\Ctx.Cli -- status
```

### `ctx doctor`

运行环境诊断：

- 产品版本
- `.ctx/` 是否存在
- `HEAD`
- working context
- metrics
- provider 配置
- 环境凭证

```powershell
dotnet run --project .\Ctx.Cli -- doctor
```

### `ctx audit`

认知一致性审计。会检查：

- 无 hypothesis 的 task
- `Done` 但无 Accepted conclusion 的 task
- 无 evidence 的 hypothesis
- 仅关联已关闭 task 的开放 hypothesis
- `Accepted` 但无 `rationale`/`evidence` 的 decision
- 关联 `Done` task 的 `Draft` conclusion

```powershell
dotnet run --project .\Ctx.Cli -- audit
```

### `ctx next`

根据当前状态推荐下一步：

- `Task`
- `Gap`

```powershell
dotnet run --project .\Ctx.Cli -- next
```

## 认知图谱

### `ctx graph summary`

```powershell
dotnet run --project .\Ctx.Cli -- graph summary
```

### `ctx graph show <nodeId>`

```powershell
dotnet run --project .\Ctx.Cli -- graph show <hypothesisId>
dotnet run --project .\Ctx.Cli -- graph show Hypothesis:<hypothesisId>
```

### `ctx graph export`

```powershell
dotnet run --project .\Ctx.Cli -- graph export --format json
dotnet run --project .\Ctx.Cli -- graph export --format mermaid
dotnet run --project .\Ctx.Cli -- graph export --format json --commit <commitId>
```

### `ctx graph lineage`

```powershell
dotnet run --project .\Ctx.Cli -- graph lineage --goal <goalId>
dotnet run --project .\Ctx.Cli -- graph lineage --task <taskId>
dotnet run --project .\Ctx.Cli -- graph lineage --hypothesis <hypothesisId> --format mermaid
dotnet run --project .\Ctx.Cli -- graph lineage --decision <decisionId> --output .\tmp\decision-lineage.json
```

## 线程重构

### `ctx thread reconstruct --task <id>`

```powershell
dotnet run --project .\Ctx.Cli -- thread reconstruct --task <taskId>
dotnet run --project .\Ctx.Cli -- thread reconstruct --task <taskId> --format markdown
```

## Goals / Tasks / Hypotheses / Evidence / Decisions / Conclusions

请参考英文版 [CLI_COMMANDS.md](C:/sources/ctx-public/docs/CLI_COMMANDS.md) 的完整示例与选项。

## 可移植性

```powershell
dotnet run --project .\Ctx.Cli -- export --output .\tmp\ctx-export.json
dotnet run --project .\Ctx.Cli -- import --input .\tmp\ctx-export.json
```

