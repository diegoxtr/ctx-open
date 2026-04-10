# CTX Local Installation
If a language model and its agent lose context, this is the tool you need.

## Objective

Install CTX locally in `<install-root>` so the CLI can be used without depending on the repository workspace.

## Command

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\publish-local.ps1
```

## Expected result

- CLI published to `<install-root>\\bin`
- viewer published to `<install-root>\\viewer`
- `ctx` available in a new terminal session
- `ctx-viewer` available to launch the local viewer

## Locations

- CLI: `<install-root>\\bin\\Ctx.Cli.exe`
- CLI launcher: `<install-root>\\bin\\ctx.cmd`
- viewer: `<install-root>\\viewer\\Ctx.Viewer.exe`
- viewer launcher: `<install-root>\\bin\\ctx-viewer.cmd`

## Verification

```powershell
ctx version
ctx-viewer
```

## PATH note

The publish script adds `<install-root>\\bin` to the current user's `PATH`.

If the current terminal does not pick that up automatically, open a new terminal.
