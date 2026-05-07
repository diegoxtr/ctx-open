# Estructura Interna de `.ctx/`
Si un modelo de lenguaje y su agente pierden el contexto, esta es la herramienta que necesitas.

Este documento describe la estructura de almacenamiento local de CTX.

La carpeta `.ctx/` es el repositorio cognitivo persistido en disco. Contiene la configuración, el estado activo, el historial reproducible y los artefactos operativos generados por el sistema.

## Objetivo

La estructura de `.ctx/` está diseñada para:

- persistir razonamiento estructurado, no conversaciones crudas
- permitir reproducibilidad de commits cognitivos
- separar estado de trabajo de snapshots inmutables
- soportar branching, diff y merge
- registrar ejecuciones, packets y métricas
- facilitar export, import, backup y auditoría

## Vista General

Estructura objetivo:

```text
.ctx/
  version.json
  config.json
  project.json
  HEAD
  branches/
  commits/
  graph/
  working/
  staging/
  runs/
  packets/
  index/
  metrics/
  providers/
  logs/
```

No todas las carpetas tienen que contener archivos desde el primer momento. Algunas se pueblan sólo cuando ciertos flujos son ejecutados.

## Archivos Base

### `version.json`

Describe la versión del formato de repositorio.

Responsabilidad:
- compatibilidad del repositorio
- control de formato persistido

Ejemplo:

```json
{
  "currentVersion": "1.0",
  "initializedAtUtc": "2026-04-07T20:12:05.4352656+00:00"
}
```

### `config.json`

Contiene la configuración general del repositorio.

Responsabilidad:
- provider por defecto
- providers disponibles
- límite de tokens por packet
- activación de tracking de métricas

Ejemplo:

```json
{
  "defaultProvider": "openai",
  "providers": [
    {
      "name": "openai",
      "defaultModel": "gpt-4.1",
      "endpoint": "https://api.openai.com/v1/responses",
      "enabled": true
    },
    {
      "name": "anthropic",
      "defaultModel": "claude-3-7-sonnet-latest",
      "endpoint": "https://api.anthropic.com/v1/messages",
      "enabled": true
    }
  ],
  "packetTokenLimit": 16000,
  "trackMetrics": true
}
```

### `project.json`

Representa la entidad `Project` del repositorio cognitivo.

Responsabilidad:
- identidad del proyecto
- branch por defecto
- estado del proyecto
- traceabilidad de creación

Ejemplo:

```json
{
  "name": "CTX",
  "description": "Cognitive Version Control System",
  "defaultBranch": "main",
  "state": 1,
  "id": {
    "value": "e92f9079e49a48a8a08bbeb324936ff6"
  },
  "trace": {
    "createdBy": "diegoxtr",
    "createdAtUtc": "2026-04-07T20:12:05.4303232+00:00",
    "updatedBy": null,
    "updatedAtUtc": null,
    "tags": [
      "project"
    ],
    "relatedIds": []
  }
}
```

### `HEAD`

Apunta al branch actual y al último commit conocido.

Formato actual:

```text
main:676790c259864667940b503e6c7e5008
```

Responsabilidad:
- branch actualmente checked out
- referencia rápida al commit actual

Regla:
- si no hay commit todavía, el valor de commit puede ser `null` a nivel lógico

## Directorios

### `branches/`

Contiene una referencia por branch.

Ejemplo:

```json
{
  "name": "main",
  "commitId": {
    "value": "676790c259864667940b503e6c7e5008"
  },
  "updatedAtUtc": "2026-04-07T20:13:13.6697558+00:00"
}
```

Responsabilidad:
- registrar punteros de branches
- soportar `branch`, `checkout` y `merge`

Relación con CLI:
- `ctx branch`
- `ctx checkout`
- `ctx merge`

### `commits/`

Contiene snapshots inmutables de `ContextCommit`.

Responsabilidad:
- historial reproducible
- base para `log`, `diff`, `merge`, `export`

Invariante:
- un commit no debe mutar después de persistido

Relación con CLI:
- `ctx commit`
- `ctx log`
- `ctx diff`

### `graph/`

Representa la vista materializada actual del grafo cognitivo.

Archivo actual observado:
- `graph/current-graph.json`

Responsabilidad:
- snapshot navegable del estado de dominio actual
- apoyo a inspección y futuras optimizaciones

Notas:
- hoy convive con `working/` como representación útil del estado operativo
- puede evolucionar hacia una proyección más especializada del dominio

### `working/`

Contiene el `WorkingContext` activo.

Archivo actual:
- `working/working-context.json`

Responsabilidad:
- estado mutable en progreso
- artefactos agregados pero no necesariamente committeados
- base para `status`, `context`, `run` y `commit`

Relación con CLI:
- `ctx status`
- `ctx goal add`
- `ctx task add`
- `ctx hypo add`
- `ctx decision add`
- `ctx evidence add`
- `ctx conclusion add`
- `ctx run`
- `ctx context`

### `staging/`

Contiene el estado listo para materializar en commit.

Archivo actual:
- `staging/staged-context.json`

Responsabilidad:
- persistencia del snapshot staging
- soporte a reproducibilidad entre working y commit

Notas:
- en esta base actual, el staging refleja el snapshot persistido por el motor de commit
- es una base razonable para introducir staging más fino en versiones futuras

### `runs/`

Contiene ejecuciones de IA persistidas.

Responsabilidad:
- registrar `Run`
- conservar outputs estructurados por ejecución
- servir inspección operativa y análisis posterior

Relación con CLI:
- `ctx run`
- `ctx run list`
- `ctx run show`

Notas:
- puede estar vacío si todavía no se ejecutó ninguna corrida

### `packets/`

Contiene `ContextPacket` generados por `ContextBuilder`.

Responsabilidad:
- persistir el contexto optimizado enviado a providers
- auditar qué información fue priorizada en una ejecución

Relación con CLI:
- `ctx context`
- `ctx packet list`
- `ctx packet show`

Notas:
- puede estar vacío si aún no se construyeron packets o runs

### `index/`

Directorio reservado para optimización de búsqueda y acceso.

Estado actual:
- contiene un `README.txt` placeholder

Responsabilidad:
- soporte a índices futuros
- aceleración de consulta local
- materialización secundaria del repositorio

### `metrics/`

Contiene métricas operativas y económicas.

Archivo actual:
- `metrics/usage.json`

Nota operativa para este repositorio self-hosted:
- `metrics/usage.json` se trata como telemetria runtime y puede excluirse de Git para evitar que comandos de lectura ensucien el working tree en cada ejecucion

Ejemplo:

```json
{
  "totalRuns": 0,
  "totalTokens": 0,
  "totalAcuCost": 0,
  "repeatedIterations": 0,
  "avoidedRedundancyCount": 0,
  "totalExecutionTime": "00:00:00"
}
```

Responsabilidad:
- costo acumulado
- tokens
- redundancia evitada
- tiempo total de ejecución

Relación con CLI:
- `ctx metrics show`

### `providers/`

Directorio reservado para metadatos, cachés o futuras proyecciones específicas por provider.

Estado actual:
- contiene un `README.txt` placeholder

Responsabilidad:
- desacoplar concerns de integración por proveedor
- permitir extensiones sin contaminar el estado de dominio principal

### `logs/`

Directorio reservado para diagnósticos y registros operativos.

Estado actual:
- contiene un `README.txt` placeholder

Responsabilidad:
- soporte a troubleshooting
- logging de operación local
- posible auditoría técnica

## Relación Conceptual Entre Directorios

Resumen operativo:

- `working/` = estado mutable actual
- `staging/` = snapshot listo o alineado para commit
- `commits/` = historial inmutable
- `branches/` + `HEAD` = navegación y referencia actual
- `runs/` + `packets/` = interacción con IA
- `metrics/` = observabilidad de costo y rendimiento
- `graph/` = proyección del estado cognitivo

## Flujo Típico de Escritura

### 1. Inicialización

`ctx init` crea:
- `version.json`
- `config.json`
- `project.json`
- `HEAD`
- estructura de carpetas
- `working/working-context.json`
- `graph/current-graph.json`
- `staging/staged-context.json`
- `metrics/usage.json`

### 2. Trabajo diario

Comandos como `goal add`, `task add`, `hypo add`, `decision add`, `evidence add`, `conclusion add` y `run` actualizan principalmente:
- `working/`
- `graph/`
- `runs/`
- `packets/`
- `metrics/`

### 3. Commit cognitivo

`ctx commit`:
- genera un snapshot inmutable en `commits/`
- actualiza `HEAD`
- actualiza el branch en `branches/`
- sincroniza `staging/`

### 4. Export e import

- `ctx export` serializa el estado portable del repositorio
- `ctx import` reconstruye `.ctx/` en otro entorno

## Invariantes Recomendados

Estas reglas deben mantenerse para preservar integridad:

- `HEAD` debe apuntar a un branch existente
- el branch actual debe existir en `branches/`
- `working-context.json` debe ser deserializable siempre
- los commits en `commits/` no deben mutar
- los IDs referenciados entre entidades deben existir
- `metrics/usage.json` debe seguir siendo acumulativo y consistente
- si un repositorio versiona `.ctx/` dentro de Git, conviene decidir explicitamente si `metrics/usage.json` pertenece al historial cognitivo o solo a telemetria operativa local
- `config.json` debe reflejar providers válidos y reconocidos por la aplicación

## Qué Puede Estar Vacío

Es normal que estas carpetas no tengan contenido útil al principio:

- `runs/`
- `packets/`
- `index/`
- `providers/`
- `logs/`

No es un error. Depende del flujo ejecutado y del nivel de madurez del repositorio.

## Qué No Debe Guardarse Como Fuente Primaria

Por diseño, `.ctx/` no debe usar como fuente principal:

- logs de chat crudos
- conversaciones completas sin estructura
- embeddings como verdad de dominio

La fuente primaria siempre debe ser el conjunto de entidades estructuradas del dominio.

## Documentos Relacionados

- [CLI_COMMANDS.md](../CLI_COMMANDS.md)
- [INSTALLATION_AND_USAGE_GUIDE.md](../INSTALLATION_AND_USAGE_GUIDE.md)
- [V1_FUNCTIONAL_SPEC.md](../V1_FUNCTIONAL_SPEC.md)
