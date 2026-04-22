# CTX - Release 1.0.5

发布日期：2026-04-16

版本：

- `1.0.5`

摘要：

- CTX 1.0 的稳定补丁版本。
- 冻结首条面向公开发布、具备 release-aware 安装与 bootstrap 的基线。

亮点：

- CTX 现在为 Windows 提供单入口 bootstrap（`install.ps1`），为 Linux/macOS 提供 `install.sh`，支持 install、update 与 repair 检测。
- 安装器现在会从 GitHub Releases 解析最新已发布版本与匹配的便携资产，而不是依赖仓库中硬编码版本字符串作为公开更新的唯一真相源。
- CLI helper 现在从 `prompts/CTX_HELPER_PROMPT.md` 动态加载项目上下文 prompt，使操作员与代理在规划前重新锚定到当前 repo 或 install root。
- distribution 打包现在会将 helper prompt 与规范 CTX 文档复制进安装 bundle，使安装后的环境自带运行指导。
- 公共文档现在描述 release branch strategy 和 public-safe 安装器流程，同时不泄露 private workspace 路径或临时 live-demo URL。
