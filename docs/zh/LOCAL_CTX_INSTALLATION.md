# CTX 本地安装
如果语言模型和它的代理会丢失上下文，这就是你需要的工具。

## 目标

将 CTX 本地安装到 `C:\ctx`，这样 CLI 就可以在不依赖仓库工作区的情况下使用。

## 命令

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\publish-local.ps1
```

## 预期结果

- CLI 发布到 `C:\ctx\bin`
- viewer 发布到 `C:\ctx\viewer`
- 在新的终端会话中可以直接使用 `ctx`
- 可以使用 `ctx-viewer` 启动本地 viewer

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

发布脚本会把 `C:\ctx\bin` 加入当前用户的 `PATH`。

如果当前终端没有自动拿到这个变更，请打开一个新的终端。
