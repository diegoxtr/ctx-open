# Arquitectura Tecnica de CTX
Si un modelo de lenguaje y su agente pierden el contexto, esta es la herramienta que necesitas.

Este documento describe la arquitectura tecnica actual de CTX.

El objetivo es dejar claro:

- como esta organizada la solucion
- cual es la responsabilidad de cada capa
- como se conectan los componentes
- como fluye una operacion desde la CLI hasta la persistencia y los providers

CTX esta construido sobre `.NET 8` y sigue una organizacion modular inspirada en Clean Architecture y principios DDD.

## Vista General

La solucion se organiza en estos proyectos:

- `Ctx.Domain`
- `Ctx.Application`
- `Ctx.Core`
- `Ctx.Persistence`
- `Ctx.Providers`
- `Ctx.Infrastructure`
- `Ctx.Cli`
- `Ctx.Tests`

La regla principal es:

- el dominio define el lenguaje y los tipos
- la aplicacion define contratos y casos de uso
- el core implementa la logica critica
- persistence implementa almacenamiento local
- providers implementa integraciones con LLMs
- infrastructure arma el grafo de dependencias
- cli expone la interfaz de usuario

## Mapa de Capas

```text
CLI
  -> Application
    -> Core
      -> Domain
    -> Persistence
    -> Providers
  -> Infrastructure
```

Vista conceptual:

- `Ctx.Domain` no depende de otras capas
- `Ctx.Application` depende del dominio y expone abstracciones
- `Ctx.Core` implementa interfaces de aplicación usando dominio
- `Ctx.Persistence` implementa repositorios filesystem
- `Ctx.Providers` implementa providers intercambiables
- `Ctx.Infrastructure` compone implementaciones concretas
- `Ctx.Cli` consume `ICtxApplicationService`

## 1. Capa Domain

Proyecto:
- [Model.cs](../../Ctx.Domain/Model.cs)
- [Identifiers.cs](../../Ctx.Domain/Identifiers.cs)
- [Enums.cs](../../Ctx.Domain/Enums.cs)
- [DomainPrimitives.cs](../../Ctx.Domain/DomainPrimitives.cs)

Responsabilidad:
- definir el modelo de dominio
- definir IDs fuertes
- definir estados de ciclo de vida
- definir artefactos de diff, merge, métricas y export

Contiene:
- entidades como `Project`, `Goal`, `Task`, `Hypothesis`, `Decision`, `Evidence`, `Conclusion`
- artefactos como `Run`, `ContextPacket`, `WorkingContext`, `ContextCommit`
- soporte como `Traceability`, `TokenUsage`, `EntityReference`, `ContentSection`

Principio:
- esta capa no conoce CLI, archivos, HTTP ni providers concretos

## 2. Capa Application

Proyecto:
- [ICtxApplicationService.cs](../../Ctx.Application/ICtxApplicationService.cs)

Responsabilidad:
- definir contratos de casos de uso
- definir requests/responses
- definir interfaces de repositorio
- definir abstracciones de providers

Interfaces principales:
- `ICtxApplicationService`
- `IWorkingContextRepository`
- `ICommitRepository`
- `IBranchRepository`
- `IRunRepository`
- `IPacketRepository`
- `IMetricsRepository`
- `IAIProvider`
- `IAIProviderRegistry`
- `IContextBuilder`
- `IRunOrchestrator`
- `ICommitEngine`
- `IDiffEngine`
- `IMergeEngine`

Rol arquitectonico:
- es el boundary entre interfaz y lógica implementada

## 3. Capa Core

Proyecto:
- [CtxApplicationService.cs](../../Ctx.Core/CtxApplicationService.cs)
- [ContextBuilder.cs](../../Ctx.Core/ContextBuilder.cs)
- [RunOrchestrator.cs](../../Ctx.Core/RunOrchestrator.cs)
- [CommitEngine.cs](../../Ctx.Core/CommitEngine.cs)
- [DiffEngine.cs](../../Ctx.Core/DiffEngine.cs)
- [MergeEngine.cs](../../Ctx.Core/MergeEngine.cs)

Responsabilidad:
- implementar la lógica critica del producto
- coordinar repositorios y motores
- convertir comandos en operaciones de dominio persistidas

### `CtxApplicationService`

Es el servicio de aplicación principal.

Responsabilidad:
- orquestar casos de uso CLI
- validar referencias
- cargar y persistir estado
- delegar lógica específica a motores especializados

Ejemplos de casos de uso implementados:
- inicialización de repositorio
- alta de artefactos
- ejecución de runs
- commits
- diff
- branching y merge
- export/import
- doctor

### `ContextBuilder`

Responsabilidad:
- construir `ContextPacket`
- seleccionar goals, tasks, hypotheses, decisions, evidence y conclusions relevantes
- evitar secciones vacías
- generar fingerprint del contexto
- estimar tokens

Punto clave:
- no depende de embeddings como fuente primaria

### `RunOrchestrator`

Responsabilidad:
- construir packet usando `ContextBuilder`
- persistir packet
- resolver provider desde `IAIProviderRegistry`
- ejecutar provider
- persistir `Run`
- actualizar métricas

Punto clave:
- centraliza la interacción estructurada con IA

### `CommitEngine`

Responsabilidad:
- limpiar snapshot de working state
- generar hash reproducible del snapshot
- construir `ContextCommit`
- calcular diff respecto del commit anterior

Punto clave:
- es la base del versionado cognitivo reproducible

### `DiffEngine`

Responsabilidad:
- comparar estados cognitivos
- detectar cambios por tipo de artefacto
- producir un resumen interpretable

### `MergeEngine`

Responsabilidad:
- integrar ramas cognitivas
- detectar conflictos semánticos
- devolver `MergeResult`

## 4. Capa Persistence

Proyecto:
- [WorkingContextRepository.cs](../../Ctx.Persistence/WorkingContextRepository.cs)
- [CommitRepository.cs](../../Ctx.Persistence/CommitRepository.cs)
- [BranchRepository.cs](../../Ctx.Persistence/BranchRepository.cs)
- otros repositorios filesystem-backed

Responsabilidad:
- persistir el repositorio cognitivo local
- administrar la estructura `.ctx/`
- leer y escribir JSON
- encapsular paths y serialización filesystem

Implementaciones principales:
- `FileSystemWorkingContextRepository`
- `FileSystemCommitRepository`
- `FileSystemBranchRepository`
- `FileSystemRunRepository`
- `FileSystemPacketRepository`
- `FileSystemMetricsRepository`

Patrón usado:
- repositorios concretos filesystem-backed

Punto clave:
- la persistencia trabaja contra el contrato de `Ctx.Application`, no contra la CLI

## 5. Capa Providers

Proyecto:
- [ProviderInfrastructure.cs](../../Ctx.Providers/ProviderInfrastructure.cs)
- [OpenAiProvider.cs](../../Ctx.Providers/OpenAiProvider.cs)
- [AnthropicProvider.cs](../../Ctx.Providers/AnthropicProvider.cs)

Responsabilidad:
- abstraer ejecución contra proveedores LLM
- mantener providers intercambiables
- encapsular HTTP, autenticación y parseo de respuesta

Componentes:
- `AIProviderRegistry`
- `HttpAiProviderBase`
- `OpenAiProvider`
- `AnthropicProvider`

Punto clave:
- si faltan credenciales, hay fallback offline determinístico
- eso permite probar el flujo sin depender de red o API keys

## 6. Capa Infrastructure

Proyecto:
- [Bootstrapper.cs](../../Ctx.Infrastructure/Bootstrapper.cs)

Responsabilidad:
- composition root
- instanciar implementaciones concretas
- conectar dependencias entre capas
- definir opciones JSON para salida CLI

Composición actual:
- serializer
- clock
- hashing service
- repositorios filesystem
- engines
- provider registry
- `CtxApplicationService`

Punto clave:
- hoy usa wiring manual simple
- es suficiente para una base CLI local sin contenedor DI externo

## 7. Capa CLI

Proyecto:
- [Program.cs](../../Ctx.Cli/Program.cs)

Responsabilidad:
- parsear argumentos
- mapearlos a requests de aplicación
- serializar `CommandResult`
- devolver salida estructurada en JSON

Características:
- comandos estilo herramienta de control de versiones
- manejo simple de opciones y posicionales
- una única salida estructurada consistente

Punto clave:
- la CLI no contiene lógica de negocio compleja
- delega en `ICtxApplicationService`

## 8. Capa Tests

Proyecto:
- [ApplicationServiceTests.cs](../../Ctx.Tests/ApplicationServiceTests.cs)
- [ContextBuilderTests.cs](../../Ctx.Tests/ContextBuilderTests.cs)
- [CommitEngineTests.cs](../../Ctx.Tests/CommitEngineTests.cs)
- [DiffEngineTests.cs](../../Ctx.Tests/DiffEngineTests.cs)

Responsabilidad:
- validar motores centrales
- validar casos de uso críticos
- validar portabilidad, doctor, export/import y resúmenes CLI

Punto clave:
- cubre lo esencial del flujo interno
- todavía no reemplaza pruebas de producto más exigentes

## Dependencias Entre Capas

Resumen simplificado:

- `Ctx.Domain` no depende de nadie
- `Ctx.Application` depende de `Ctx.Domain`
- `Ctx.Core` depende de `Ctx.Application` y `Ctx.Domain`
- `Ctx.Persistence` depende de `Ctx.Application` y `Ctx.Domain`
- `Ctx.Providers` depende de `Ctx.Application` y `Ctx.Domain`
- `Ctx.Infrastructure` depende de todas las capas concretas necesarias para componer
- `Ctx.Cli` depende de `Ctx.Infrastructure` y `Ctx.Application`

Regla buscada:
- los detalles concretos cuelgan de abstracciones de aplicación

## Flujos End-to-End

## Flujo 1. `ctx init`

Recorrido:

1. `Ctx.Cli` parsea `init`
2. `Ctx.Cli` construye `InitRepositoryRequest`
3. `ICtxApplicationService.InitAsync` recibe el pedido
4. `CtxApplicationService` crea `Project`, `WorkingContext`, `RepositoryConfig`, `HeadReference` y `BranchReference`
5. `IWorkingContextRepository.InitializeAsync` crea `.ctx/` y escribe la estructura base
6. CLI imprime `CommandResult`

Resultado:
- repositorio cognitivo inicializado

## Flujo 2. `ctx goal add`

Recorrido:

1. CLI parsea opciones
2. `CtxApplicationService` carga `WorkingContext`
3. crea entidad `Goal`
4. marca `Dirty = true`
5. persiste `working/working-context.json`
6. actualiza `graph/current-graph.json`

Resultado:
- estado de trabajo actualizado, sin commit todavía

## Flujo 3. `ctx context`

Recorrido:

1. CLI invoca `ContextAsync`
2. `CtxApplicationService` carga `WorkingContext`
3. delega a `ContextBuilder.Build(...)`
4. `ContextBuilder` selecciona artefactos relevantes y arma secciones
5. devuelve `ContextPacket`
6. CLI lo serializa

Resultado:
- packet usable para iteración con IA

## Flujo 4. `ctx run`

Recorrido:

1. CLI construye `RunRequest`
2. `CtxApplicationService.RunAsync` carga `WorkingContext`
3. delega a `RunOrchestrator.ExecuteAsync`
4. `RunOrchestrator` construye y persiste `ContextPacket`
5. resuelve provider en `IAIProviderRegistry`
6. ejecuta `IAIProvider.ExecuteAsync`
7. crea y persiste `Run`
8. actualiza `MetricsSnapshot`
9. `CtxApplicationService` agrega el run al `WorkingContext`
10. persiste `working/`

Resultado:
- corrida registrada, packet persistido y métricas actualizadas

## Flujo 5. `ctx commit`

Recorrido:

1. CLI construye `CommitRequest`
2. `CtxApplicationService` carga `WorkingContext` y `HEAD`
3. si existe, carga commit anterior
4. delega a `CommitEngine.CreateCommit`
5. `CommitEngine` genera snapshot limpio, hash y diff
6. `CtxApplicationService` persiste commit en `commits/`
7. actualiza `working/`, `staging/`, `HEAD` y branch activo

Resultado:
- snapshot cognitivo reproducible persistido

## Flujo 6. `ctx diff`

Recorrido:

1. CLI envía IDs opcionales
2. `CtxApplicationService` resuelve commits o working context
3. usa `CommitEngine` y/o `MergeEngine` para calcular diff o conflictos
4. devuelve resumen estructurado

Resultado:
- cambios cognitivos explicables por tipo de artefacto

## Flujo 7. `ctx merge`

Recorrido:

1. CLI recibe branch fuente
2. `CtxApplicationService` carga `HEAD`, branch fuente y commit fuente
3. carga `WorkingContext` actual
4. delega a `MergeEngine.Merge`
5. persiste `MergedContext`
6. devuelve `MergeResult`

Resultado:
- integración de ramas con conflictos cognitivos explícitos si corresponde

## Decisiones Arquitectonicas Clave

### Persistencia local en filesystem

Elegida para:
- simplicidad operativa
- auditabilidad
- portabilidad
- facilidad de backup/export

### JSON como formato principal

Elegido para:
- inspección humana
- serialización simple
- portabilidad entre entornos

### CLI como interfaz principal

Elegida para:
- velocidad de iteración
- automatización
- cercanía con workflows de versionado

### Providers desacoplados

Elegidos así para:
- intercambiar OpenAI y Anthropic sin reescribir flujos
- permitir agregar más proveedores después

### Motores especializados en Core

Elegidos así para:
- aislar lógica crítica
- facilitar testeo
- evitar que `CtxApplicationService` se convierta en un objeto monolítico

## Extensibilidad

La arquitectura actual ya permite extender:

- nuevos providers implementando `IAIProvider`
- nuevas formas de persistencia implementando repositorios
- nuevos comandos CLI agregando mapping en `Program.cs`
- nuevos motores internos siguiendo interfaces de aplicación

## Límites Actuales

Puntos todavía simples en esta base:

- wiring manual en `Infrastructure`
- parseo CLI artesanal
- persistencia local sin sincronización remota
- sin resolución guiada de conflictos
- sin bus de eventos o background processing
- sin control de concurrencia avanzada

Eso no invalida la arquitectura base, pero marca el borde de V1 interna.

## Qué Hace Sólida Esta Base

La base actual ya separa correctamente:

- lenguaje de dominio
- contratos de aplicación
- lógica crítica
- detalles de persistencia
- detalles de integración con IA
- interfaz de usuario

Eso permite seguir evolucionando CTX sin colapsar toda la solución en una sola capa.

## Referencias Relacionadas

- [DOMAIN_MODEL.md](../DOMAIN_MODEL.md)
- [CTX_STRUCTURE.md](../CTX_STRUCTURE.md)
- [CLI_COMMANDS.md](../CLI_COMMANDS.md)
- [V1_FUNCTIONAL_SPEC.md](../V1_FUNCTIONAL_SPEC.md)
