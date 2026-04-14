# CTX 安装器与分发计划
如果语言模型和它的代理会丢失上下文，这就是你需要的工具。

## 目标

为 CTX 提供跨平台安装器和便携分发策略，覆盖：

- Windows（x64、x86）
- macOS（Apple Silicon、Intel）
- Linux（x64、ARM64）

同时包含一段标准 prompt，用来把代理绑定到认知版本化工作流。

## 目标平台与架构

### Windows

- 安装器：签名可执行文件（MSI 或 EXE）
- 便携版：包含 `ctx.exe` 和支持文件的 zip
- 架构：x64、x86（如果运行时支持，也可选 ARM64）

### macOS

- 安装器：签名 `.pkg` 或 `.dmg`
- 便携版：包含 `ctx` 二进制的 tarball
- 架构：Apple Silicon 与 Intel

### Linux

- 安装器：`deb`、`rpm` 等包格式以及 tarball
- 便携版：包含 `ctx` 的 tarball
- 架构：x64 与 ARM64

## 基线打包策略

1. 使用 `dotnet publish` 为每个 target 构建 self-contained 二进制
2. 为每个 OS/arch 生成一个便携归档
3. 在便携 payload 之上再封装平台原生安装器

## 规范目录

打包根目录是：

- `distribution/`

具体资源位于：

- `distribution/targets.json`
- `distribution/agent-link/CTX_AGENT_LINK_PROMPT.txt`
- `distribution/windows/ctx.iss`
- `distribution/macos/package-macos.sh`
- `distribution/linux/package-linux.sh`
- `scripts/build-distribution.ps1`

## 必需的 Agent-Link Prompt

每个分发包都应该附带一段供代理使用的短 prompt：

```text
CTX is the system of record. Read CTX first, follow ctx next, and record evidence/decisions/conclusions before committing code.
Do not create work outside CTX without adding a task and hypothesis.
If you start planning from chat instead of CTX, stop, inspect CTX again, and continue from the repository state.
```

这段 prompt 应放在：

- 安装器输出目录中，便于操作员复制
- 基础 prompt 模板 `prompts/CTX_BASE_PROMPT.md` 中

操作说明：

- 有些模型需要多次重复这段内容，才会停止把 chat 当作规划界面
- 分发方应把这种重复当作必需的 bootstrap 步骤，而不是可选提示

## 更新流程

基线更新流程应支持：

- 安装器驱动的原地更新
- 便携安装的手动替换

## 具体工具链

- 便携 payload：通过 `scripts/build-distribution.ps1` 为每个 RID 执行 `dotnet publish`
- Windows EXE 安装器脚手架：Inno Setup，位于 `distribution/windows/ctx.iss`
- macOS 安装器脚手架：`pkgbuild` 与 `productbuild`，位于 `distribution/macos/package-macos.sh`
- Linux 包脚手架：tarball，加上可选的 `deb`/`rpm` 封装，位于 `distribution/linux/package-linux.sh`

## 版本化产物策略

- `distribution/` 是 packaging manifests、prompts 和安装器脚手架的版本化单一来源
- `artifacts/distribution/` 是便携归档的生成输出目录
- `artifacts/distribution/` 下展开后的 bundle 目录属于构建副产品，不应进入版本化状态
- 如果便携归档需要被有意版本化，应通过 Git LFS 管理，而不是作为普通 Git blob

## 验证清单

- 每次构建都能成功运行 `ctx version`
- 二进制可在每个 OS/arch 上启动
- prompt 片段和二进制一起交付
- 便携归档输出在 `artifacts/distribution/` 下

## 仍待解决的问题

- 签名证书与 CI 签名流程
- viewer 是否应始终包含在默认安装 payload 中
- Linux 原生包是否应使用 `fpm` 还是发行版专用流水线

## 下一步

对最初支持的 targets 运行便携构建脚本，并在各平台上验证生成的归档。
