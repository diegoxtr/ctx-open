# CTX 技术架构

如果语言模型及其代理丢失了上下文，这就是你需要的工具。

本文描述 CTX 当前的技术架构。

目标：

- 解释解决方案如何组织
- 说明每一层的职责
- 说明组件如何连接
- 说明一个 CLI 操作如何流向持久化与 providers

CTX 基于 `.NET 8` 构建，并遵循受 Clean Architecture 与 DDD 启发的模块化结构。

## 总览

解决方案中的项目：

- `Ctx.Domain`
- `Ctx.Application`
- `Ctx.Core`
- `Ctx.Persistence`
- `Ctx.Providers`
- `Ctx.Infrastructure`
- `Ctx.Cli`
- `Ctx.Tests`

主规则：

- domain 定义语言与类型
- application 定义 contracts 与 use cases
- core 实现关键逻辑
- persistence 实现本地存储
- providers 实现 LLM 集成
- infrastructure 负责组装依赖
- CLI 暴露接口

## 层次图

```text
CLI
  -> Application
    -> Core
      -> Domain
    -> Persistence
    -> Providers
  -> Infrastructure
```

概念视图：

- `Ctx.Domain` 不依赖任何其他层
- `Ctx.Application` 依赖 Domain，并暴露抽象
- `Ctx.Core` 使用 Domain 实现 Application 接口
- `Ctx.Persistence` 实现 filesystem repositories
- `Ctx.Providers` 实现可替换的 providers
- `Ctx.Infrastructure` 负责组合具体实现
- `Ctx.Cli` 消费 `ICtxApplicationService`

## 1. Domain 层

文件：

- [Model.cs](../../Ctx.Domain/Model.cs)
- [Identifiers.cs](../../Ctx.Domain/Identifiers.cs)
- [Enums.cs](../../Ctx.Domain/Enums.cs)

职责：

- 定义领域模型
- 强 ID
- 生命周期状态
- diff / merge / metrics / export 产物

原则：

- 这一层不理解 CLI、文件系统、HTTP 或具体 providers

## 2. Application 层

文件：

- [ICtxApplicationService.cs](../../Ctx.Application/ICtxApplicationService.cs)

职责：

- 定义 use-case contracts
- 定义 requests / responses
- 定义 repository interfaces
- 定义 provider abstractions

关键接口：

- `ICtxApplicationService`
- `IWorkingContextRepository`
- `ICommitRepository`
- `IBranchRepository`
- `IRunRepository`
- `IPacketRepository`
- `IMetricsRepository`
- `IAIProvider`
- `IAIProviderRegistry`
- `IContextBuilder`
- `IRunOrchestrator`
- `ICommitEngine`
- `IDiffEngine`
- `IMergeEngine`

## 3. Core 层

文件：

- [CtxApplicationService.cs](../../Ctx.Core/CtxApplicationService.cs)
- [ContextBuilder.cs](../../Ctx.Core/ContextBuilder.cs)
- [RunOrchestrator.cs](../../Ctx.Core/RunOrchestrator.cs)
- [CommitEngine.cs](../../Ctx.Core/CommitEngine.cs)
- [DiffEngine.cs](../../Ctx.Core/DiffEngine.cs)
- [MergeEngine.cs](../../Ctx.Core/MergeEngine.cs)

职责：

- 实现核心产品逻辑
- 协调 repositories 与 engines
- 将命令转换为持久化领域操作

## 4. Persistence 层

职责：

- 持久化本地认知仓库
- 管理 `.ctx/` 结构
- 读写 JSON
- 封装 filesystem paths 与序列化细节

主要实现：

- `FileSystemWorkingContextRepository`
- `FileSystemCommitRepository`
- `FileSystemBranchRepository`
- `FileSystemRunRepository`
- `FileSystemPacketRepository`
- `FileSystemMetricsRepository`

## 5. Providers 层

职责：

- 抽象 LLM 执行
- 让 providers 保持可替换
- 封装 HTTP / auth / response parsing

组件：

- `AIProviderRegistry`
- `HttpAiProviderBase`
- `OpenAiProvider`
- `AnthropicProvider`

说明：

- 缺失 credentials 时会触发确定性的 offline fallback

## 6. Infrastructure 层

职责：

- composition root
- 实例化具体实现
- 组装依赖
- 为 CLI 输出设置 JSON 选项

## 7. CLI 层

职责：

- 解析参数
- 映射到 application requests
- 序列化 `CommandResult`
- 输出结构化 JSON

说明：

- CLI 只应保留极少量业务逻辑

## 8. Tests

测试覆盖：

- core engines
- 关键 use cases
- portability、doctor、export/import、CLI summaries

## 端到端流程摘要

- `ctx init`：创建基础仓库结构
- `ctx goal add`：更新 working context 与 graph
- `ctx context`：构建一个 `ContextPacket`
- `ctx run`：执行 provider，持久化 run 与 metrics
- `ctx commit`：生成不可变 snapshot 与 diff
- `ctx diff`：比较 commits 或 working state
- `ctx merge`：集成分支并处理 cognitive conflicts

## 关键架构决策

- 使用本地文件系统持久化，以保持简单与可移植
- 使用 JSON 作为主格式
- 采用 CLI-first 接口，便于自动化
- 用 provider abstraction 保持可移植性
- 在 Core 中使用专门 engines，以保持逻辑聚焦

## 当前限制

- Infrastructure 中仍有手工 wiring
- CLI parsing 仍主要是手写
- 目前只支持本地持久化
- 没有 guided conflict resolution
- 没有后台处理
- 并发控制仍有限

## 相关参考

- [DOMAIN_MODEL.md](../DOMAIN_MODEL.md)
- [CTX_STRUCTURE.md](../CTX_STRUCTURE.md)
- [CLI_COMMANDS.md](../CLI_COMMANDS.md)
- [V1_FUNCTIONAL_SPEC.md](../V1_FUNCTIONAL_SPEC.md)
