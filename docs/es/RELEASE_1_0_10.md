# CTX 1.0.10 - Notas De Release

Fecha de release:

- `2026-04-27`

Version:

- `1.0.10`

## Resumen

CTX 1.0.10 prepara la linea publica para agentes compatibles con MCP, mejora la coherencia visual de CTX Viewer y ordena la instalacion local de CLI, viewer y servidor MCP.

## Novedades

- `Ctx.Mcp` como servidor MCP local por stdio.
- Modo MCP `read-only` para inspeccion segura y modo `write` controlado para crear artefactos cognitivos.
- CTX Viewer abre por primera vez en modo Split.
- El historial del viewer carga los ultimos 20 commits y permite cargar mas por boton o scroll.
- El grafo por commit queda centrado en la tarea cognitiva capturada por ese commit.
- Los builds locales del viewer muestran `local-version`.
- Los instaladores publican `ctx-mcp` y rutas parseables de instalacion.

## Validacion

- `ctx version` informa `1.0.10`.
- `dotnet build Ctx.sln --no-restore` pasa.
- `dotnet test Ctx.Tests\Ctx.Tests.csproj --no-restore` pasa.
- `Ctx.Mcp` compila y queda disponible mediante `ctx-mcp`.
- Los scripts de instalacion de PowerShell y Bash parsean correctamente.
- El repositorio publico no incluye estado `.ctx` privado.
