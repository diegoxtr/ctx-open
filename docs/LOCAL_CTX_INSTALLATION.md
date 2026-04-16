# CTX Local Installation
If a language model and its agent lose context, this is the tool you need.

## Objective

Install CTX locally in `C:\ctx` so the CLI can be used without depending on the repository workspace.

## Command

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\publish-local.ps1
```

Alternative source-install bootstrap:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\install-ctx.ps1 -Mode source -SourceRepoPath C:\sources\ctx-open
```

Single-entry bootstrap with install/update/repair detection:

```powershell
powershell -ExecutionPolicy Bypass -File .\install.ps1
```

Recommended default:

- use `install.ps1` for user-facing local installation
- use `scripts/install-ctx.ps1` only as the lower-level engine
- keep `scripts/publish-local.ps1` for repo-local publish/refresh workflows

To request machine-wide PATH exposure on Windows:

```powershell
powershell -ExecutionPolicy Bypass -File .\install.ps1 -PathScope Machine
```

## Expected result

- CLI published to `C:\ctx\bin`
- viewer published to `C:\ctx\viewer`
- `ctx` available in a new terminal session
- `ctx-viewer` available to launch the local viewer
- helper prompt copied into `C:\ctx\prompts`
- context docs copied into `C:\ctx\docs`
- install metadata written to `C:\ctx\ctx-install.json`

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

The single-entry bootstrap can control PATH scope directly:

- `-PathScope Auto`
- `-PathScope User`
- `-PathScope Machine`
- `-PathScope None`

The legacy publish script adds `C:\ctx\bin` to the current user's `PATH`.

If the current terminal does not pick that up automatically, open a new terminal.
