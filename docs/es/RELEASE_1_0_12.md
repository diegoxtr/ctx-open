# CTX 1.0.12 - Notas De Release

Fecha de release:

- `2026-04-30`

Version:

- `1.0.12`

## Resumen

CTX 1.0.12 mejora la conexion local de agentes y deja la documentacion publica mas clara para instalar, conectar MCP y validar el estado con `ctx_plan`.

El flujo recomendado es: instalar CTX, apuntar `ctx-mcp` a un repositorio, empezar en modo read-only, llamar `ctx_plan` y recien despues habilitar write mode si el operador quiere que el agente cree artefactos CTX.

## Highlights

- La pagina estatica incluye snippets para Claude Code, Claude Desktop, VS Code/Copilot y Codex.
- `ctx plan` y MCP `ctx_plan` quedan como smoke test recomendado porque devuelven branch, estado dirty, proximo trabajo, contexto y runbooks en una sola respuesta.
- `ctx prompt list` / `ctx prompts` agrega una linea cronologica estable de prompts y triggers.
- `ctx operational review` y el `ctx preflight` ampliado ayudan a convertir problemas operativos repetidos en mejoras de runbooks.

## Agregado

- `ctx prompt list` y alias `ctx prompts`.
- `ctx plan` para planificacion compacta al iniciar un turno de agente.
- `ctx operational review` para detectar problemas repetidos y sugerir mejoras de runbooks.
- Bloques especificos para Claude en la pagina MCP estatica:
  - comando para Claude Code
  - JSON de Claude Desktop
  - `.mcp.json` por proyecto
  - prompt de prueba con `ctx_plan`

## Cambiado

- La landing publica ahora presenta la conexion MCP local como camino principal para agentes.
- La guia MCP enfatiza read-only primero, validacion con `ctx_plan` y write mode explicito.
- `ctx preflight` ahora incluye guia sobre problemas operativos repetidos cuando alcanzan el umbral configurado.
- Los ejemplos evitan rutas locales de una maquina concreta cuando alcanza con rutas relativas o placeholders neutrales.

## Validacion

- `ctx version` informa `1.0.12`.
- `dotnet build Ctx.sln --no-restore` pasa.
- `dotnet test Ctx.Tests\Ctx.Tests.csproj --no-restore` pasa.
- `dotnet test Ctx.Tests\Ctx.Tests.csproj --no-restore --filter "ListPromptTimelineAsync|CommandCoverage"` pasa.
- Los snippets JSON de MCP, ACP y CLI parsean.
- `git ls-files .ctx` no devuelve archivos para el root del repositorio publico.
