# CTX 本地安装

如果语言模型及其代理丢失了上下文，这就是你需要的工具。

## 目标

将 CTX 本地安装到 `C:\ctx`，使 CLI 可以在不依赖仓库工作区的情况下使用。

## 命令

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\publish-local.ps1
```

替代的 source-install bootstrap：

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\install-ctx.ps1 -Mode source -SourceRepoPath C:\sources\ctx-open
```

带 install/update/repair 检测的单入口 bootstrap：

```powershell
powershell -ExecutionPolicy Bypass -File .\install.ps1
```

推荐默认方式：

- 用户侧本地安装使用 `install.ps1`
- `scripts/install-ctx.ps1` 只作为更底层的 engine
- `scripts/publish-local.ps1` 保留给 repo-local publish / refresh workflows

若要在 Windows 上请求 machine-wide PATH 暴露：

```powershell
powershell -ExecutionPolicy Bypass -File .\install.ps1 -PathScope Machine
```

## 预期结果

- CLI 发布到 `C:\ctx\bin`
- viewer 发布到 `C:\ctx\viewer`
- 在新的终端会话中可直接使用 `ctx`
- 可以使用 `ctx-viewer` 启动本地 viewer
- helper prompt 会复制到 `C:\ctx\prompts`
- context docs 会复制到 `C:\ctx\docs`
- 安装元数据会写入 `C:\ctx\ctx-install.json`

## 位置

- CLI：`C:\ctx\bin\Ctx.Cli.exe`
- CLI 启动器：`C:\ctx\bin\ctx.cmd`
- viewer：`C:\ctx\viewer\Ctx.Viewer.exe`
- viewer 启动器：`C:\ctx\bin\ctx-viewer.cmd`

## 验证

```powershell
ctx version
ctx-viewer
```

## PATH 说明

单入口 bootstrap 可以直接控制 PATH scope：

- `-PathScope Auto`
- `-PathScope User`
- `-PathScope Machine`
- `-PathScope None`

旧的 publish 脚本会把 `C:\ctx\bin` 加入当前用户的 `PATH`。

如果当前终端没有自动拿到这个变化，请打开一个新的终端。
