# GitHub 在线演示

CTX 可以通过以下组合提供一个 GitHub 原生的在线演示：

- 使用 GitHub Codespaces 运行可执行的 viewer
- 使用仓库内跟踪的 `.ctx` 示例作为默认数据集
- 使用 GitHub Pages 作为静态落地页，指向在线 viewer URL 和 release 下载

## 为什么要这样拆分

GitHub Pages 只能提供静态托管。它不能自己运行 CTX Viewer 后端，也不能直接检查 `.ctx` 仓库。

因此，在线部分必须放在可执行环境里。对于纯 GitHub 的第一版交付，最合适的 surface 是 Codespaces：

- 仓库本来就在 GitHub 上
- viewer 可以直接在 codespace 中运行
- 端口 `5271` 可以公开转发
- 默认仓库路径可以指向一个已跟踪的 example

## 标准演示仓库

默认在线演示仓库是：

- `examples/ctx/agent-session-continuity`

它最符合产品核心论点，因为它展示的是多会话连续性，而不是一次性截图。

## Codespaces 流程

仓库现在包含：

- `.devcontainer/devcontainer.json`
- `scripts/start-codespaces-demo.sh`
- `docs/live-demo/index.html`
- `.github/workflows/live-demo-pages.yml`

当 codespace 连接后：

1. 创建环境时先运行 `dotnet restore Ctx.sln`
2. `scripts/start-codespaces-demo.sh` 在 `0.0.0.0:5271` 启动 viewer
3. 脚本把 `CTX_VIEWER_DEFAULT_REPOSITORY_PATH` 设为 `examples/ctx/agent-session-continuity`
4. Codespaces 将 `5271` 端口公开转发

## 在 codespace 中手动启动

如果需要重新启动演示：

```bash
bash scripts/start-codespaces-demo.sh
```

日志写入：

- `/tmp/ctx-viewer-codespaces.log`

## 公共落地页

GitHub Pages 应保持静态。

它的职责是：

- 简要解释产品论点
- 链接到在线演示 URL
- 链接到 release 下载
- 展示一两张截图

Pages 不应该直接托管 viewer。

仓库现在包含一个静态落地页：

- `docs/live-demo/index.html`

以及一个部署工作流：

- `.github/workflows/live-demo-pages.yml`

静态制品由以下内容构建：

- `docs/live-demo/*`
- `assets/screenshots/*`

## 交付模型

- `GitHub Pages` = 静态落地页
- `GitHub Codespaces` = 在线 viewer
- 已跟踪的 `.ctx` example = 演示数据

这样第一版公开演示就能保持 GitHub 原生，而不必立刻引入额外托管服务。
