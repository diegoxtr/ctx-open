# CTX Local Installation
If a language model and its agent lose context, this is the tool you need.

## Objective

Install CTX locally in `C:\ctx` so the CLI can be used without depending on the repository workspace.

## Command

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\publish-local.ps1
```

## Expected result

- CLI published to `C:\ctx\bin`
- viewer published to `C:\ctx\viewer`
- `ctx` available in a new terminal session
- `ctx-viewer` available to launch the local viewer

## Locations

- CLI: `C:\ctx\bin\Ctx.Cli.exe`
- CLI launcher: `C:\ctx\bin\ctx.cmd`
- viewer: `C:\ctx\viewer\Ctx.Viewer.exe`
- viewer launcher: `C:\ctx\bin\ctx-viewer.cmd`

## Verification

```powershell
ctx version
ctx-viewer
```

## PATH note

The publish script adds `C:\ctx\bin` to the current user's `PATH`.

If the current terminal does not pick that up automatically, open a new terminal.
