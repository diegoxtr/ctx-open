# Plan de instalador y distribucion de CTX
Si un modelo de lenguaje y su agente pierden el contexto, esta es la herramienta que necesitas.

## Objetivo

Proveer un instalador cross-platform y una estrategia de distribucion portable para CTX en:

- Windows (x64, x86)
- macOS (Apple Silicon, Intel)
- Linux (x64, ARM64)

Tambien debe incluir una seccion de prompt estandar que ate a los agentes al flujo del versionador cognitivo.

## Plataformas y arquitecturas objetivo

### Windows

- Instalador: ejecutable firmado (MSI o EXE).
- Portable: zip con `ctx.exe` y archivos de soporte.
- Arquitecturas: x64, x86 (y opcionalmente ARM64 si el runtime lo soporta).

### macOS

- Instalador: `.pkg` o `.dmg` firmado.
- Portable: tarball con binario `ctx`.
- Arquitecturas: Apple Silicon e Intel.

### Linux

- Instalador: formatos de paquete (`deb`, `rpm`) mas tarball.
- Portable: tarball con `ctx`.
- Arquitecturas: x64 y ARM64.

## Estrategia de packaging base

1. Generar binarios self-contained por target con `dotnet publish`.
2. Producir un archivo portable por OS/arch.
3. Envolver un instalador nativo por plataforma sobre ese payload portable.

## Directorio canonico

La raiz de packaging es:

- `distribution/`

Los assets concretos viven en:

- `distribution/targets.json`
- `distribution/agent-link/CTX_AGENT_LINK_PROMPT.txt`
- `distribution/windows/ctx.iss`
- `distribution/macos/package-macos.sh`
- `distribution/linux/package-linux.sh`
- `scripts/build-distribution.ps1`

## Agent-Link Prompt requerido

Cada distribucion deberia incluir un fragmento de prompt corto que los agentes usen al operar CTX:

```text
CTX is the system of record. Read CTX first, follow ctx next, and record evidence/decisions/conclusions before committing code.
Do not create work outside CTX without adding a task and hypothesis.
If you start planning from chat instead of CTX, stop, inspect CTX again, and continue from the repository state.
```

Este fragmento deberia colocarse en:

- el directorio de salida del instalador, para que el operador pueda copiarlo
- y el template base de prompt (`prompts/CTX_BASE_PROMPT.md`)

Nota operativa:

- algunos modelos necesitan que este fragmento se repita mas de una vez antes de dejar de tratar el chat como superficie de planificacion
- los distribuidores deberian considerar esa repeticion como un paso de bootstrap requerido, no como guidance opcional

## Flujo de actualizacion

El flujo base de actualizacion deberia soportar:

- update in-place manejado por instalador
- reemplazo manual para instalaciones portables

## Toolchain concreto

- Payloads portables: `dotnet publish` por RID via `scripts/build-distribution.ps1`
- Scaffold de instalador EXE para Windows: Inno Setup via `distribution/windows/ctx.iss`
- Scaffold de instalador macOS: `pkgbuild` y `productbuild` via `distribution/macos/package-macos.sh`
- Scaffold de paquete Linux: tarball mas cierre opcional `deb`/`rpm` via `distribution/linux/package-linux.sh`

## Politica de artefactos versionados

- `distribution/` es la fuente de verdad versionada para manifests de packaging, prompts y scaffolding de instaladores.
- `artifacts/distribution/` es la ubicacion de salida generada para los archivos portables.
- Los directorios de bundle expandidos dentro de `artifacts/distribution/` son subproductos de build y deben quedar fuera del estado versionado.
- Si los archivos portables se versionan intencionalmente, deberian rastrearse con Git LFS y no como blobs normales de Git.

## Checklist de verificacion

- Cada build ejecuta `ctx version` correctamente.
- El binario arranca en cada OS/arch.
- El fragmento de prompt se distribuye junto al binario.
- Los archivos portables se emiten bajo `artifacts/distribution/`.

## Preguntas abiertas

- Certificados de firma y flujo de firma en CI.
- Si el viewer deberia distribuirse siempre en el payload por defecto del instalador.
- Si los paquetes nativos de Linux deberian construirse con `fpm` o con pipelines especificos por distro.

## Siguiente paso

Ejecutar el script de build portable para los targets soportados inicialmente y validar los archivos emitidos en cada plataforma.
