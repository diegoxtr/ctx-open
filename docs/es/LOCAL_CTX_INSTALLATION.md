# Instalacion Local de CTX
Si un modelo de lenguaje y su agente pierden el contexto, esta es la herramienta que necesitas.

## Objetivo

Dejar CTX instalado localmente en `C:\ctx` para usar la CLI sin depender del workspace.

## Comando

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\publish-local.ps1
```

## Resultado esperado

- CLI publicada en `C:\ctx\bin`
- viewer publicado en `C:\ctx\viewer`
- comando `ctx` disponible al abrir una nueva terminal
- comando `ctx-viewer` disponible para levantar el viewer local

## Ubicaciones

- CLI: `C:\ctx\bin\Ctx.Cli.exe`
- Launcher CLI: `C:\ctx\bin\ctx.cmd`
- Viewer: `C:\ctx\viewer\Ctx.Viewer.exe`
- Launcher viewer: `C:\ctx\bin\ctx-viewer.cmd`

## Verificacion

```powershell
ctx version
ctx-viewer
```

## Nota sobre PATH

El script agrega `C:\ctx\bin` al `PATH` del usuario actual.

Si la terminal actual no lo toma automaticamente, abrir una terminal nueva.

