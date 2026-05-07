# Modelo de Dominio de CTX
Si un modelo de lenguaje y su agente pierden el contexto, esta es la herramienta que necesitas.

Este documento describe el modelo de dominio actual de CTX.

Su objetivo es dejar explícitas:

- las entidades del sistema
- sus identificadores fuertes
- sus relaciones
- sus estados de ciclo de vida
- sus reglas de trazabilidad
- los agregados y artefactos operativos relevantes

CTX no modela conversaciones como fuente primaria. Modela artefactos cognitivos estructurados.

## Principios del Modelo

El modelo de dominio está diseñado para cumplir estas reglas:

- toda información importante debe ser estructurada
- cada artefacto relevante debe tener identidad fuerte
- las decisiones deben ser explícitas
- la evidencia debe ser referenciable
- los commits deben ser reproducibles
- la evolución del razonamiento debe poder compararse
- el estado de trabajo y el historial deben estar separados

## Identificadores Fuertes

Cada entidad principal usa un ID tipado, implementado como `record struct`.

Tipos actuales:

- `ProjectId`
- `GoalId`
- `TaskId`
- `HypothesisId`
- `DecisionId`
- `EvidenceId`
- `ConclusionId`
- `RunId`
- `ContextCommitId`
- `ContextPacketId`
- `WorkingContextId`

Características:

- encapsulan el valor string
- se generan con GUID en formato compacto
- evitan mezclar tipos de entidad por error

Ejemplo conceptual:

```csharp
public readonly record struct GoalId(string Value);
```

## Base Común de Entidades

Las entidades trazables del dominio heredan conceptualmente de:

```csharp
public abstract record CognitiveEntity<TId>(TId Id, Traceability Trace);
```

Esto implica:

- identidad fuerte
- trazabilidad obligatoria

## Traceability

La trazabilidad es un componente obligatorio para las entidades cognitivas principales.

Estructura:

- `CreatedBy`
- `CreatedAtUtc`
- `UpdatedBy`
- `UpdatedAtUtc`
- `Tags`
- `RelatedIds`

Objetivo:

- saber quién creó un artefacto
- cuándo fue creado o actualizado
- con qué etiquetas conceptuales se relaciona
- qué otros IDs se vinculan de forma secundaria

## Entidades Principales

### Project

Representa el proyecto cognitivo raíz.

Campos principales:

- `Id`
- `Name`
- `Description`
- `DefaultBranch`
- `State`
- `Trace`

Estado:
- usa `LifecycleState`

Responsabilidad:
- definir la identidad general del repositorio
- establecer el branch por defecto
- actuar como raíz conceptual del contexto

### Goal

Representa un objetivo explícito del trabajo cognitivo.

Campos principales:

- `Id`
- `Title`
- `Description`
- `Priority`
- `State`
- `Trace`
- `TaskIds`

Estado:
- usa `LifecycleState`

Relaciones:
- un `Goal` puede agrupar muchas `Task`

Responsabilidad:
- expresar intención de alto nivel
- organizar tareas vinculadas

### Task

Representa una unidad concreta de trabajo cognitivo.

Campos principales:

- `Id`
- `GoalId`
- `Title`
- `Description`
- `State`
- `Trace`
- `HypothesisIds`

Estado:
- usa `TaskExecutionState`

Valores actuales:
- `Draft`
- `Ready`
- `InProgress`
- `Blocked`
- `Done`

Relaciones:
- puede pertenecer a un `Goal`
- puede vincularse a muchas `Hypothesis`

Responsabilidad:
- modelar una acción o frente de trabajo específico

### Hypothesis

Representa una suposición o proposición a evaluar.

Campos principales:

- `Id`
- `Statement`
- `Rationale`
- `Confidence`
- `State`
- `Trace`
- `TaskIds`
- `EvidenceIds`

Estado:
- usa `HypothesisState`

Valores actuales:
- `Proposed`
- `UnderEvaluation`
- `Supported`
- `Refuted`
- `Archived`

Relaciones:
- puede vincularse a una o varias `Task`
- puede estar soportada por `Evidence`

Responsabilidad:
- explicitar ideas evaluables
- evitar razonamiento implícito o difuso

### Decision

Representa una decisión explícita del proceso de razonamiento.

Campos principales:

- `Id`
- `Title`
- `Rationale`
- `State`
- `Trace`
- `HypothesisIds`
- `EvidenceIds`

Estado:
- usa `DecisionState`

Valores actuales:
- `Proposed`
- `Accepted`
- `Rejected`
- `Superseded`

Relaciones:
- referencia `Hypothesis`
- referencia `Evidence`

Responsabilidad:
- dejar constancia explícita de elecciones
- conectar una decisión con su fundamento

### Evidence

Representa evidencia trazable que soporta o contradice otros artefactos.

Campos principales:

- `Id`
- `Title`
- `Summary`
- `Source`
- `Kind`
- `Confidence`
- `State`
- `Trace`
- `Supports`

Estado:
- usa `LifecycleState`

Tipo:
- usa `EvidenceKind`

Valores actuales de `EvidenceKind`:
- `Observation`
- `Benchmark`
- `Document`
- `Experiment`
- `ProviderOutput`

Relaciones:
- `Supports` contiene `EntityReference`
- puede apuntar a `Hypothesis`, `Decision` o `Task`

Responsabilidad:
- registrar sustento verificable
- evitar decisiones sin fundamento explícito

### Conclusion

Representa una conclusión consolidada.

Campos principales:

- `Id`
- `Summary`
- `State`
- `Trace`
- `DecisionIds`
- `EvidenceIds`

Estado:
- usa `ConclusionState`

Valores actuales:
- `Draft`
- `Accepted`
- `Superseded`

Relaciones:
- referencia `Decision`
- referencia `Evidence`

Responsabilidad:
- condensar un resultado razonado
- dejar un cierre explícito de una línea de trabajo

### Run

Representa una ejecución de IA sobre un `ContextPacket`.

Campos principales:

- `Id`
- `Provider`
- `Model`
- `State`
- `StartedAtUtc`
- `CompletedAtUtc`
- `PacketId`
- `Usage`
- `PromptFingerprint`
- `Summary`
- `Artifacts`
- `Trace`

Estado:
- usa `RunState`

Valores actuales:
- `Planned`
- `Running`
- `Completed`
- `Failed`

Relaciones:
- referencia un `ContextPacket`
- produce `RunArtifact`

Responsabilidad:
- registrar una interacción estructurada con un provider
- medir costo, tokens y duración
- capturar salida útil para evolución del contexto

### ContextPacket

Representa un paquete optimizado de contexto para una ejecución.

Campos principales:

- `Id`
- `ProjectId`
- `CreatedAtUtc`
- `Purpose`
- `Fingerprint`
- `EstimatedTokens`
- `GoalIds`
- `TaskIds`
- `HypothesisIds`
- `DecisionIds`
- `EvidenceIds`
- `ConclusionIds`
- `Sections`

Responsabilidad:
- seleccionar información relevante
- reducir redundancia
- dejar trazado qué se envió al provider

Notas:
- no hereda de `CognitiveEntity<TId>` en la implementación actual
- funciona como artefacto estructurado y persistible

### WorkingContext

Representa el estado mutable actual del repositorio.

Campos principales:

- `Id`
- `RepositoryVersion`
- `CurrentBranch`
- `HeadCommitId`
- `Dirty`
- `Project`
- `Goals`
- `Tasks`
- `Hypotheses`
- `Decisions`
- `Evidence`
- `Conclusions`
- `Runs`
- `Trace`

Responsabilidad:
- concentrar el estado en progreso
- servir de base para `status`, `context`, `run` y `commit`
- permitir reconstruir el grafo cognitivo actual

Método relevante:

- `ToGraph()` construye un `ContextGraph`

### ContextCommit

Representa un snapshot inmutable del estado cognitivo.

Campos principales:

- `Id`
- `Branch`
- `Message`
- `ParentIds`
- `CreatedAtUtc`
- `SnapshotHash`
- `Diff`
- `Snapshot`
- `Trace`

Responsabilidad:
- persistir historia reproducible
- capturar el diff asociado al snapshot
- habilitar `log`, `diff`, `branching` y `merge`

Regla:
- una vez persistido, debe tratarse como inmutable

## Artefactos de Soporte

### ContextGraph

Es una proyección del estado cognitivo completo.

Contiene:

- `Project`
- `Goals`
- `Tasks`
- `Hypotheses`
- `Decisions`
- `Evidence`
- `Conclusions`
- `Runs`

Responsabilidad:
- representar el grafo de dominio de forma compacta

### RunArtifact

Describe un artefacto producido por una ejecución.

Campos:

- `ArtifactType`
- `Title`
- `Content`
- `References`

Responsabilidad:
- extraer salidas estructuradas desde el provider

### TokenUsage

Modela consumo de ejecución.

Campos:

- `InputTokens`
- `OutputTokens`
- `AcuCost`
- `Duration`

Propiedad derivada:

- `TotalTokens`

### ContentSection

Es una sección textual estructurada dentro de un `ContextPacket`.

Campos:

- `Title`
- `Content`
- `References`

### EntityReference

Referencia liviana a otra entidad del dominio.

Campos:

- `EntityType`
- `EntityId`

Uso:
- evidencia soportada
- artifacts de run
- secciones de contexto

## Diff y Merge Cognitivo

### ContextDiffChange

Representa un cambio puntual detectado entre estados.

Campos:

- `ChangeType`
- `EntityType`
- `EntityId`
- `Summary`

### CognitiveConflict

Representa un conflicto cognitivo explícito.

Campos:

- `EntityType`
- `EntityId`
- `ConflictType`
- `CurrentSummary`
- `IncomingSummary`

Responsabilidad:
- explicar divergencias semánticas relevantes

### ContextDiff

Agrupa cambios por tipo de entidad.

Campos:

- `FromCommitId`
- `ToCommitId`
- `Decisions`
- `Hypotheses`
- `Evidence`
- `Tasks`
- `Conclusions`
- `Conflicts`
- `Summary`

Responsabilidad:
- permitir inspección de evolución del razonamiento

### MergeResult

Resultado de una operación de merge.

Campos:

- `MergedContext`
- `Conflicts`
- `AutoMerged`
- `Summary`

Responsabilidad:
- expresar el resultado de integración entre ramas cognitivas

## Configuración y Repositorio

### ProviderConfiguration

Describe un provider disponible.

Campos:

- `Name`
- `DefaultModel`
- `Endpoint`
- `Enabled`

### RepositoryConfig

Configuración operativa del repositorio.

Campos:

- `DefaultProvider`
- `Providers`
- `PacketTokenLimit`
- `TrackMetrics`

### RepositoryVersion

Describe la versión del formato persistido.

Campos:

- `CurrentVersion`
- `InitializedAtUtc`

### HeadReference

Puntero al branch actual y su commit visible.

Campos:

- `Branch`
- `CommitId`

### BranchReference

Describe un branch persistido.

Campos:

- `Name`
- `CommitId`
- `UpdatedAtUtc`

## Observabilidad y Portabilidad

### MetricsSnapshot

Resume uso y costo acumulado.

Campos:

- `TotalRuns`
- `TotalTokens`
- `TotalAcuCost`
- `RepeatedIterations`
- `AvoidedRedundancyCount`
- `TotalExecutionTime`

### DoctorCheck

Resultado individual de diagnóstico.

Campos:

- `Name`
- `Status`
- `Detail`

### DoctorReport

Reporte completo de diagnóstico del entorno y repositorio.

Campos:

- `ProductVersion`
- `WorkingDirectory`
- `RepositoryDetected`
- `Checks`

### RepositoryExport

Snapshot portable para export/import.

Campos:

- `ProductVersion`
- `RepositoryVersion`
- `Config`
- `Head`
- `WorkingContext`
- `Metrics`
- `Branches`
- `Commits`

## Relaciones Principales

Relaciones actuales del dominio:

- `Project` contiene el contexto general del repositorio
- `Goal` agrupa `Task`
- `Task` referencia opcionalmente un `Goal`
- `Task` referencia `Hypothesis`
- `Hypothesis` referencia `Task`
- `Hypothesis` referencia `Evidence`
- `Decision` referencia `Hypothesis`
- `Decision` referencia `Evidence`
- `Conclusion` referencia `Decision`
- `Conclusion` referencia `Evidence`
- `Run` referencia `ContextPacket`
- `WorkingContext` agrega todas las entidades activas
- `ContextCommit` encapsula un `WorkingContext` snapshot

## Estados de Ciclo de Vida

### `LifecycleState`

Usado por:

- `Project`
- `Goal`
- `Evidence`

Valores:

- `Draft`
- `Active`
- `Validated`
- `Completed`
- `Superseded`
- `Archived`

### `TaskExecutionState`

Usado por:

- `Task`

Valores:

- `Draft`
- `Ready`
- `InProgress`
- `Blocked`
- `Done`

### `HypothesisState`

Usado por:

- `Hypothesis`

Valores:

- `Proposed`
- `UnderEvaluation`
- `Supported`
- `Refuted`
- `Archived`

### `DecisionState`

Usado por:

- `Decision`

Valores:

- `Proposed`
- `Accepted`
- `Rejected`
- `Superseded`

### `ConclusionState`

Usado por:

- `Conclusion`

Valores:

- `Draft`
- `Accepted`
- `Superseded`

### `RunState`

Usado por:

- `Run`

Valores:

- `Planned`
- `Running`
- `Completed`
- `Failed`

## Agregados Operativos

Aunque la implementación actual usa records persistidos y servicios de aplicación, conceptualmente hay agregados claros:

- `WorkingContext` como agregado operativo principal
- `ContextCommit` como agregado histórico inmutable
- `Run` como agregado de ejecución
- `ContextPacket` como artefacto derivado de contexto

## Reglas de Integridad Relevantes

Estas reglas importan para mantener coherencia:

- los IDs referenciados deben existir en el contexto
- `Decision` no debe referenciar hipótesis o evidencias inexistentes
- `Conclusion` no debe referenciar decisiones o evidencias inexistentes
- `Evidence.Supports` debe usar referencias válidas
- `Run.PacketId` debe apuntar a un packet persistido
- `WorkingContext.HeadCommitId` debe ser consistente con `HEAD`
- `ContextCommit.SnapshotHash` debe representar exactamente el snapshot persistido

## Qué Hace Valioso Este Modelo

Este modelo permite:

- versionar pensamiento estructurado
- comparar evolución cognitiva
- justificar decisiones
- rastrear evidencia
- reconstruir el estado completo del razonamiento
- exportar e importar el repositorio sin perder semántica central

## Referencias Relacionadas

- [CTX_STRUCTURE.md](../CTX_STRUCTURE.md)
- [CLI_COMMANDS.md](../CLI_COMMANDS.md)
- [V1_FUNCTIONAL_SPEC.md](../V1_FUNCTIONAL_SPEC.md)
