# CTX 安装与使用指南
????????????????,??????????

本文档说明如何在本地首次安装、编译并使用 CTX。

适用人群：

- 技术测试人员
- 开发者
- 架构师
- V1 早期用户

## 基本要求

- Windows 终端
- .NET SDK 8
- 仓库代码访问权限
- 工作目录读写权限

可选：

- `OPENAI_API_KEY`
- `ANTHROPIC_API_KEY`

未配置密钥也可使用离线 fallback 进行功能测试。

## 关键步骤

1. 验证 .NET：

```powershell
dotnet --version
```

2. 拉取代码并进入根目录：

```powershell
cd <repo-root>
```

3. 恢复、构建、测试：

```powershell
dotnet restore Ctx.sln
dotnet build Ctx.sln
dotnet test .\Ctx.Tests\Ctx.Tests.csproj
```

4. 运行 CLI：

```powershell
dotnet run --project .\Ctx.Cli -- status
```

5. 初始化仓库：

```powershell
dotnet run --project .\\Ctx.Cli -- init --name "CTX-DEMO" --description "First cognitive repo"
```

## 推荐首个流程

- `goal add`
- `task add`
- `hypo add`
- `evidence add`
- `decision add`
- `conclusion add`
- `run`
- `commit`

详细示例请参考英文版：

- [INSTALLATION_AND_USAGE_GUIDE.md](C:/sources/ctx-public/docs/INSTALLATION_AND_USAGE_GUIDE.md)

