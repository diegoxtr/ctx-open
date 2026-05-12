# CTX 1.0.11 - Notas De Release

Fecha de release:

- `2026-04-28`

Version:

- `1.0.11`

## Resumen

CTX 1.0.11 es un patch publico de documentacion y alineacion de release despues de la publicacion MCP 1.0.10. Mantiene inmutable el tag publicado `v1.0.10`, mueve la guia MCP local y las mejoras del README a una nueva linea de release, y alinea los links publicos del demo con la version vigente.

## Highlights

- El README publico explica CTX como memoria cognitiva durable mas alla de la memoria del proveedor/modelo.
- El README publico documenta la exposicion MCP local stdio, modo read-only por defecto, write mode protegido y limites actuales.
- Los links del demo publico apuntan a la linea de release actual.
- La guia publica rapida de MCP local queda incluida en la rama de release.
- El changelog corrige el orden de las secciones 1.0.8 y 1.0.7.

## Validacion

- `ctx version` informa `1.0.11`.
- `dotnet build Ctx.sln --no-restore` pasa.
- `dotnet test Ctx.Tests\Ctx.Tests.csproj --no-restore` pasa.
- `Ctx.Mcp` compila como parte de la solucion.
- El repo publico no incluye estado local `.ctx`.
