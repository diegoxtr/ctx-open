# CTX 安装与使用指南
如果语言模型及其代理丢失了上下文，这就是你需要的工具。

## 1. 目标

本文档说明如何在本地环境中首次安装、更新、修复、构建并使用 CTX。

适用人群：

- 技术测试者
- 开发者
- 架构师
- V1 早期用户

## 2. 基本要求

开始前请确认：

- Windows 与可用终端
- 已安装 .NET SDK 8
- 可以访问仓库源码
- 工作目录具备读写权限

可选：

- `OPENAI_API_KEY`
- `ANTHROPIC_API_KEY`

如果未配置密钥，CTX 仍可使用离线 fallback 进行功能测试。

## 3. 验证 .NET

```powershell
dotnet --version
```

期望结果：

- `8.x`

## 4. 获取代码

如果已经有仓库，进入项目根目录：

```powershell
cd <repo-root>
```

## 5. 推荐安装流程

推荐使用单入口 bootstrap 脚本。

Windows：

```powershell
powershell -ExecutionPolicy Bypass -File .\install.ps1
```

Linux/macOS：

```bash
bash ./install.sh
```

bootstrap 会：

- 判断 `install`、`update` 或 `repair`
- 选择合适的 source 或 portable 流程
- 把 helper/docs 资源复制到安装根目录
- 尽可能把 `ctx` 暴露为全局命令
- 以 GitHub Releases 作为公开安装 / 更新的真相源

## 6. 恢复、构建、测试

```powershell
dotnet restore Ctx.sln
dotnet build Ctx.sln
dotnet test .\Ctx.Tests\Ctx.Tests.csproj
```

## 7. 运行 CLI

```powershell
dotnet run --project .\Ctx.Cli -- status
```

如果当前目录还没有认知仓库，请先 `init`。

## 8. 打开仓库后的第一判断

第一步先判断当前仓库是：

- 已存在的 CTX 仓库（已经有 `.ctx/`）
- 还是一个尚未认知初始化的新项目

### 已存在的 CTX 仓库

```powershell
ctx
```

状态驱动规则：

- 如果 `ctx` 表示存在待收尾认知变更：
  - 运行 `ctx closeout`
- 如果 `ctx` 表示有开放工作：
  - 运行 `ctx next`
- 如果 `ctx` 表示已到 durable boundary：
  - 运行 `ctx commit -m "<durable result>"`
- 如果 `ctx` 表示没有开放工作：
  - 运行 `ctx next` 检查 gap 或 closure 状态

只有在需要更深检查时，才用 `ctx status` 与 `ctx audit`。

### 新项目

```powershell
ctx init --name "<project>"
```

然后：

- 如果已有现成资料，优先使用 bootstrap：

```powershell
ctx bootstrap map --from <path>
ctx bootstrap apply --from <path>
```

- 否则手工创建第一个 goal / task / hypothesis。

## 9. 创建认知仓库

```powershell
dotnet run --project <repo-root>\Ctx.Cli -- init --name "CTX-DEMO" --description "First cognitive repo"
```

期望：

- 创建 `.ctx/`
- 生成配置文件
- 当前 branch 为 `main`

## 10. 首个推荐流程

### 第一步：创建 goal

```powershell
dotnet run --project <repo-root>\Ctx.Cli -- goal add --title "Define testing strategy" --description "Prepare a technical pilot"
```

### 第二步：创建 task

```powershell
dotnet run --project <repo-root>\Ctx.Cli -- task add --title "Evaluate CLI flow" --description "Validate core commands"
```

### 第三步：创建 hypothesis

```powershell
dotnet run --project <repo-root>\Ctx.Cli -- hypo add --statement "Structured flow improves traceability" --rationale "State is persisted in artifacts"
```

### 第四步：记录 evidence

```powershell
dotnet run --project <repo-root>\Ctx.Cli -- evidence add --title "Initial test" --summary "Structure helps resume context" --source "manual evaluation" --kind Observation --supports hypothesis:<hypothesisId>
```

### 第五步：记录 decision

```powershell
dotnet run --project <repo-root>\Ctx.Cli -- decision add --title "Use CTX in pilot" --rationale "Traceability is sufficient for pilot" --state Accepted --hypotheses <hypothesisId> --evidence <evidenceId>
```

### 第六步：记录 conclusion

```powershell
dotnet run --project <repo-root>\Ctx.Cli -- conclusion add --summary "Approve internal pilot usage" --state Accepted --decisions <decisionId> --evidence <evidenceId>
```

### 第七步：执行 run

```powershell
dotnet run --project <repo-root>\Ctx.Cli -- run --provider openai --purpose "Review pilot risks"
```

### 第八步：创建 cognitive commit

```powershell
dotnet run --project <repo-root>\Ctx.Cli -- commit -m "first end-to-end flow"
```

## 11. 常用命令

### 状态与导航

```powershell
dotnet run --project .\Ctx.Cli -- status
dotnet run --project .\Ctx.Cli -- log
dotnet run --project .\Ctx.Cli -- diff
```

### 认知工件

```powershell
dotnet run --project .\Ctx.Cli -- goal list
dotnet run --project .\Ctx.Cli -- task list
dotnet run --project .\Ctx.Cli -- hypo list
dotnet run --project .\Ctx.Cli -- decision list
dotnet run --project .\Ctx.Cli -- evidence list
dotnet run --project .\Ctx.Cli -- conclusion list
```

### 运维检查

```powershell
dotnet run --project .\Ctx.Cli -- provider list
dotnet run --project .\Ctx.Cli -- run list
dotnet run --project .\Ctx.Cli -- packet list
dotnet run --project .\Ctx.Cli -- metrics show
```

## 12. 本地生成结构

初始化后，CTX 会创建：

- `.ctx/version.json`
- `.ctx/config.json`
- `.ctx/project.json`
- `.ctx/HEAD`
- `.ctx/branches/`
- `.ctx/commits/`
- `.ctx/graph/`
- `.ctx/working/`
- `.ctx/staging/`
- `.ctx/runs/`
- `.ctx/packets/`
- `.ctx/index/`
- `.ctx/metrics/`
- `.ctx/providers/`
- `.ctx/logs/`

安装后的 CTX 根目录还会带有：

- `bin/`
- `prompts/CTX_HELPER_PROMPT.md`
- `prompts/CTX_AGENT_PROMPT.md`
- `docs/CTX_VIEWER_GUIDE.md`
- `docs/CTX_AUTONOMOUS_OPERATION_PROTOCOL.md`
- `ctx-install.json`

## 13. 使用建议

- 每个测试场景放在独立目录
- 认知提交消息要清晰
- 接受重要 decision 之前先记录 evidence
- 用 `packet list` 与 `run list` 回顾迭代
- 每个场景结束后查看 `metrics show`

## 14. 常见问题

### 构建失败

检查：

- `dotnet --version` 是否为 `8.x`
- 是否存在 `global.json`
- restore 是否完成

### 没有 provider key

这不是阻塞：

- CTX 可以用离线 fallback 运行

### ID 太难看懂

建议：

- 先用 `list`
- 再用 `show`
- 从需要的工件复制 `id.value`

## 15. 正确使用的标准

如果用户完成了以下动作，就算正确操作了 CTX：

- 初始化仓库
- 创建认知工件
- 至少执行过一次 `run`
- 至少创建过一次认知提交
- 从 CLI 检查过结果

## 16. 下一步推荐阅读

继续阅读：

- `docs/V1_PLAN.md`
- `docs/PILOT_TESTING_GUIDE.md`
- `README.md`
