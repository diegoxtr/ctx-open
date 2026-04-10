# Instalacion Local de CTX
Si un modelo de lenguaje y su agente pierden el contexto, esta es la herramienta que necesitas.

## Objetivo

Dejar CTX instalado localmente en `<install-root>` para usar la CLI sin depender del workspace.

## Comando

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\publish-local.ps1
```

## Resultado esperado

- CLI publicada en `<install-root>\\bin`
- viewer publicado en `<install-root>\\viewer`
- comando `ctx` disponible al abrir una nueva terminal
- comando `ctx-viewer` disponible para levantar el viewer local

## Ubicaciones

- CLI: `<install-root>\\bin\\Ctx.Cli.exe`
- Launcher CLI: `<install-root>\\bin\\ctx.cmd`
- Viewer: `<install-root>\\viewer\\Ctx.Viewer.exe`
- Launcher viewer: `<install-root>\\bin\\ctx-viewer.cmd`

## Verificacion

```powershell
ctx version
ctx-viewer
```

## Nota sobre PATH

El script agrega `<install-root>\\bin` al `PATH` del usuario actual.

Si la terminal actual no lo toma automaticamente, abrir una terminal nueva.

