# GitHub 在线演示

CTX 可以通过以下组合提供一个 GitHub 原生的在线演示：

- 使用 GitHub Codespaces 运行可执行的 viewer
- 使用仓库中受版本管理的 `.ctx` 示例作为默认数据集
- 使用 GitHub Pages 作为静态落地页，指向 live viewer URL 和 release 下载

公共演示入口：

- 落地页：`https://diegoxtr.github.io/ctx-open/`
- 演示说明：`https://diegoxtr.github.io/ctx-open/notes.html`
- Codespaces 快速入口：`https://codespaces.new/diegoxtr/ctx-open?quickstart=1`

如果存在临时公开的 viewer session，就把该 URL 发布到落地页或演示说明中。如果没有临时公开 session，就使用落地页或 Codespaces quickstart 链接。

## 公共截图应该传达什么

公共截图不应该只是 UI 壁纸。它们必须一眼说明三件事：

- CTX Viewer 可以保持当前工作线可见，而不会把它压平成一份待办列表。
- CTX Viewer 可以把 durable commit 检查为结构化推理线程，而不只是日志中的一行。
- CTX Viewer 可以在同一会话中并排展示 interpretations、evidence 和 commit context。

当前规范截图是：

- `assets/screenshots/ctx-viewer-working-context.jpg`
- `assets/screenshots/ctx-viewer-commit-thread.jpg`

刷新这些截图时，应优先选择当前公开 viewer surface，而不是旧布局或已经过时的界面。

可直接复制粘贴使用的 demo 仓库：

- Codespaces 默认：`/workspaces/ctx-open/examples/ctx/agent-session-continuity`
- Codespaces 备选：`/workspaces/ctx-open/examples/ctx/catalog-cache-branch-merge`
- Codespaces 备选：`/workspaces/ctx-open/examples/ctx/critical-checkout-regression`

各 demo 预期展示：

- `agent-session-continuity`：`Working` 应显示 ready handoff audit task，`Origin` 应显示保持 handoff line 开放的 trigger，`Playbook` 应显示 `Session continuity demo validation`
- `catalog-cache-branch-merge`：`Working` 应显示 ready cache review checklist，`Origin` 应显示保持 review line 开放的 trigger，`Playbook` 应显示 `Cache strategy demo validation`
- `critical-checkout-regression`：`Working` 应显示 ready post-fix monitoring task，`Origin` 应显示保持 monitoring 可见的 trigger，`Playbook` 应显示 `Checkout regression demo validation`

## 为什么要这样拆分

GitHub Pages 是静态托管。它不能直接运行 CTX Viewer backend，也不能自己检查一个 `.ctx` 仓库。

因此，live 部分必须依赖一个可运行环境。对于完全基于 GitHub 的交付模型，最合适的第一层 surface 就是 Codespaces：

- 仓库已经存在于 GitHub
- viewer 可以直接在 codespace 内运行
- `5271` 端口可以公开转发
- 默认仓库路径可以指向一个受跟踪的示例

## 规范 demo 仓库

默认 live demo 仓库是：

- `examples/ctx/agent-session-continuity`

它最符合产品 thesis，因为它展示的是跨 session continuity，而不是一次性的静态截图。

## Codespaces 流程

仓库现在包含：

- `.devcontainer/devcontainer.json`
- `scripts/ensure-dotnet-sdk.sh`
- `scripts/start-codespaces-demo.sh`
- `docs/live-demo/index.html`
- `.github/workflows/live-demo-pages.yml`

当 codespace 启动时：

1. `scripts/ensure-dotnet-sdk.sh` 会在基础镜像尚未提供 SDK 时安装 `global.json` 固定的版本
2. `dotnet restore Ctx.sln` 会在环境创建时执行一次
3. `scripts/start-codespaces-demo.sh` 会在 `0.0.0.0:5271` 上启动 viewer
4. 脚本会把 `CTX_VIEWER_DEFAULT_REPOSITORY_PATH` 设为 `examples/ctx/agent-session-continuity`
5. Codespaces 会把 `5271` 端口公开转发

live demo 不再依赖 container 内的 SSH server 功能。优先目标是保持 Codespaces 创建对 browser-first 使用尽可能稳定，而不是引入额外 provisioning 风险。

## 在 codespace 内手动启动

如果需要重启 demo：

```bash
bash scripts/start-codespaces-demo.sh
```

如果恢复后的 codespace 或部分初始化的 codespace 仍未启动 viewer，请运行完整恢复块：

```bash
git pull --ff-only origin main
bash scripts/ensure-dotnet-sdk.sh
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$DOTNET_ROOT:$DOTNET_ROOT/tools:/usr/share/dotnet:$PATH"
dotnet restore Ctx.sln
bash scripts/start-codespaces-demo.sh
curl -I http://127.0.0.1:5271
```

脚本会将日志写入：

- `/tmp/ctx-viewer-codespaces.log`

如果 viewer 没有打开，先在 codespace 内验证本地进程：

```bash
curl -I http://127.0.0.1:5271
cat /tmp/ctx-viewer-codespaces.log
```

## 公共落地页

GitHub Pages 应保持静态。

它的职责是：

- 简要解释 thesis
- 链接 live demo URL
- 链接 release 下载
- 显示一到两张截图

Pages 不应试图直接托管 viewer。

仓库现在包含：

- 静态落地页：`docs/live-demo/index.html`
- 部署工作流：`.github/workflows/live-demo-pages.yml`

静态产物构建自：

- `docs/live-demo/*`
- `assets/screenshots/*`

## 交付模型

- `GitHub Pages` = 静态落地页
- `GitHub Codespaces` = live viewer
- 受跟踪的 `.ctx` 示例 = demo 数据

这样可以在不引入额外托管商的前提下，让第一版公共 demo 保持 GitHub-native。
