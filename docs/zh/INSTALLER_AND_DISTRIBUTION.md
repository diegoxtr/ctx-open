# CTX 安装器与分发计划

如果语言模型及其代理丢失了上下文，这就是你需要的工具。

## 目标

为 CTX 提供跨平台安装器和便携分发策略，覆盖：

- Windows（x64、x86）
- macOS（Apple Silicon、Intel）
- Linux（x64、ARM64）

同时包含一段标准 prompt，用来把代理绑定到认知版本化工作流。

## 目标平台与架构

### Windows

- 安装器：签名可执行文件（MSI 或 EXE）
- 便携版：包含 `ctx.exe` 与支持文件的 zip
- 架构：x64、x86（如果运行时支持，也可选 ARM64）

### macOS

- 安装器：签名 `.pkg` 或 `.dmg`
- 便携版：包含 `ctx` 二进制的 tarball
- 架构：Apple Silicon 与 Intel

### Linux

- 安装器：包格式（deb、rpm）加 tarball
- 便携版：包含 `ctx` 的 tarball
- 架构：x64 与 ARM64

## 打包策略（基线）

1. 使用 `dotnet publish` 为每个 target 构建 self-contained 二进制
2. 为每个 OS/arch 生成一个 portable archive
3. 在 portable payload 之上再封装平台原生安装器

## 规范目录

打包根目录是：

- `distribution/`

具体资源位于：

- `distribution/targets.json`
- `distribution/version-manifest.json`
- `distribution/install-manifest.json`
- `distribution/agent-link/CTX_AGENT_LINK_PROMPT.txt`
- `distribution/windows/ctx.iss`
- `distribution/macos/package-macos.sh`
- `distribution/linux/package-linux.sh`
- `scripts/build-distribution.ps1`
- `scripts/install-ctx.ps1`
- `scripts/install-ctx.sh`
- `install.ps1`
- `install.sh`

## Agent-Link Prompt（必需）

每个分发包都应附带一段供代理使用的简短 prompt 片段：

```text
CTX is the system of record. Read CTX first, follow ctx next, and record evidence/decisions/conclusions before committing code.
Do not create work outside CTX without adding a task and hypothesis.
If you start planning from chat instead of CTX, stop, inspect CTX again, and continue from the repository state.
```

这段片段应放在：

- 安装器输出目录中，便于操作员复制
- 基础 prompt 模板 `prompts/CTX_BASE_PROMPT.md` 中

操作说明：

- 有些模型需要多次重复这段内容，才会停止把 chat 当作 planning surface
- 分发方应把这种重复视为必需的 bootstrap 步骤，而不是可选提示

## 更新流程

基线更新流程应支持：

- 安装器驱动的原地更新
- 便携安装的手动替换

## 安装 bootstrap 模式

安装 bootstrap 应支持两种操作模式：

1. `source`
从已 clone 或已 checkout 的仓库构建，然后把 CTX 安装到带有 launcher 与 helper prompt 资源的操作根目录。

2. `portable`
解压预构建的分发 bundle，然后把 CTX 安装到带有 launcher 与 helper prompt 资源的操作根目录。

安装脚本应使用共享 manifest，以便 install roots、helper prompt 位置和 launcher 布局在各平台之间保持一致。

## 用户入口

用户侧安装应优先提供每个 shell 只需 copy-paste 一次的入口脚本：

- Windows：`install.ps1`
- Linux/macOS：`install.sh`

这些 entrypoints 应该：

- 检测 CTX 是缺失、过时还是已是最新
- 选择 `install`、`update` 或 `repair`
- 从 GitHub Releases 解析最新已发布版本与匹配 asset
- 使用 `distribution/version-manifest.json` 仅作为仓库/API 配置加 asset-name 映射
- 将实际 filesystem/bootstrap 工作委派给 `scripts/install-ctx.ps1` 或 `scripts/install-ctx.sh`
- 在可能情况下全局暴露 `ctx`
  - Windows：加入 `User` 或 `Machine` PATH
  - Linux/macOS：在 `~/.local/bin` 或 `/usr/local/bin` 建立 symlink

## 分支与发布策略

安装器驱动的分发应遵循严格的已发布产物策略：

- `main`
  日常开发的主集成分支。
  它可以包含未发布工作，因此不应被公共 bootstrap 视为 install/update source。

- `release/x.y.z`
  某个具体版本候选的短生命周期稳定化分支。
  用它冻结范围、加固打包、验证安装器行为，并准备最终发布产物。

- `vX.Y.Z` tags
  不可变的发布 tag。
  GitHub Releases 应从这些 tags 创建，安装器也应把该发布 tag 视为 canonical version string。

- GitHub Releases
  公共 install/update 的真相来源。
  bootstrap 应只消费已发布 release 及其匹配产物，而不是 `main` 上的原始 commits 或未发布分支产物。

操作规则：

- 如果某项工作只存在于 `main`，它还不能通过公共 bootstrap 安装
- 如果工作已切到 `release/x.y.z` 但还未正式发布，它仍不是公共 update target
- 只有当 `vX.Y.Z` 被打 tag 且 GitHub Release 发布后，安装器才应检测并消费它

推荐流程：

1. 将持续工作合并进 `main`
2. 在准备发布时切出 `release/x.y.z`
3. 在 release 分支上验证 installer/distribution assets
4. 打 `vX.Y.Z` tag
5. 从该 tag 发布 GitHub Release 产物
6. 让 bootstrap 通过 GitHub Releases 检测到新的已发布版本

## 具体工具链

- portable payloads：通过 `scripts/build-distribution.ps1` 按 RID 执行 `dotnet publish`
- Windows EXE 安装器脚手架：Inno Setup，位于 `distribution/windows/ctx.iss`
- macOS 安装器脚手架：`pkgbuild` 与 `productbuild`，位于 `distribution/macos/package-macos.sh`
- Linux 包脚手架：tarball，加上可选的 `deb`/`rpm` closeout，位于 `distribution/linux/package-linux.sh`

## 版本化产物策略

- `distribution/` 是 packaging manifests、prompts 和 installer scaffolding 的版本化真相来源
- `artifacts/distribution/` 是 portable archives 的生成输出目录
- `artifacts/distribution/` 下展开后的 bundle 目录属于构建副产物，应保持在版本化状态之外
- 如果 portable archives 需要被有意版本化，应通过 Git LFS 管理，而不是作为普通 Git blobs

## 验证清单

- 每次构建都能成功运行 `ctx version`
- 二进制能在每个 OS/arch 上启动
- prompt 片段与二进制一起交付
- portable archives 输出在 `artifacts/distribution/` 下

## 仍待解决的问题

- 签名证书与 CI 签名流程
- viewer 是否应始终包含在默认 installer payload 中
- Linux 原生包应使用 `fpm` 还是发行版专用 pipelines

## 下一步

实现 manifest-driven 的安装脚本，然后先在 Windows 上验证 `source` 与 `portable` 两种安装流程，再把同样的布局扩展到 Linux 与 macOS。
