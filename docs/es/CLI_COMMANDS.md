# Comandos CLI de CTX
Si un modelo de lenguaje y su agente pierden el contexto, esta es la herramienta que necesitas.

Este documento describe la superficie actual de la CLI de CTX en `C:\sources\ctx-open`.

CTX devuelve salida estructurada en JSON con este formato base:

```json
{
  "success": true,
  "message": "texto resumido",
  "data": {}
}
```

## Convenciones

- Ejecutar los comandos desde la carpeta del repositorio cognitivo.
- En desarrollo local, el formato habitual es:

```powershell
dotnet run --project .\Ctx.Cli -- <comando>
```

- En instalacion local publicada:

```powershell
ctx <comando>
```

- IDs como `<goalId>`, `<taskId>`, `<hypothesisId>` o `<commitId>` se obtienen desde comandos previos.
- Las listas multiples usan valores separados por coma.
- La mayoria de los cambios mutantes impactan `.ctx/working`, `.ctx/staging`, `.ctx/graph` y eventualmente un commit cognitivo posterior.

## Comandos Generales

### `ctx`

Sin argumentos, muestra ayuda basica.

```powershell
dotnet run --project .\Ctx.Cli --
```

### `ctx version`

Muestra la version del producto y del formato de repositorio.

```powershell
dotnet run --project .\Ctx.Cli -- version
```

### `ctx init`

Inicializa un repositorio cognitivo en la carpeta actual.

Opciones:
- `--name <project>`
- `--description <text>`
- `--branch <name>`

```powershell
dotnet run --project .\Ctx.Cli -- init --name "CTX Demo" --description "Repositorio de prueba" --branch main
```

### `ctx status`

Muestra el estado actual del repositorio.

Incluye:
- branch actual
- `HEAD`
- estado `dirty`
- conteos de goals, tasks, hypotheses, decisions, evidence, conclusions y runs
- cuando esta `dirty`, una vista acotada de pendientes con:
  - resumen compacto del diff
  - cantidad de artefactos pendientes
  - hasta cinco items pendientes
  - una accion siguiente sugerida hacia `ctx closeout`

```powershell
dotnet run --project .\Ctx.Cli -- status
```

### `ctx doctor`

Ejecuta un diagnostico tecnico del entorno y del repositorio actual.

Valida:
- version de producto
- presencia de `.ctx/`
- `HEAD`
- working context
- metricas
- providers configurados
- presencia de credenciales de entorno

```powershell
dotnet run --project .\Ctx.Cli -- doctor
```

### `ctx audit`

Ejecuta una auditoria de consistencia cognitiva sobre el workspace actual.

Hoy detecta:
- tasks sin hypothesis
- tasks `Done` sin conclusion aceptada
- hypothesis sin evidence
- hypothesis abiertas ligadas solo a tasks cerradas
- decisions `Accepted` sin `rationale` o sin `evidence`
- conclusions `Draft` ligadas a tasks `Done`

Devuelve:
- `consistencyScore`
- lista de `issues`
- `severity`, `issueType`, `entityType`, `entityId`
- `suggestedAction`
- resumen por conteo

Uso recomendado:
- correrlo antes de `ctx next`
- usarlo para limpiar deuda cognitiva y evitar roadmap stale

```powershell
dotnet run --project .\Ctx.Cli -- audit
```

### `ctx next`

Prioriza el siguiente bloque de trabajo a partir del estado actual del repositorio.

Devuelve:
- candidato recomendado
- score compuesto
- factores del score
- ranking de candidatos
- diagnostico con:
  - conteos por estado de task
  - conteos de gaps elegibles
  - conteos de exclusiones de gaps
  - guia corta de recuperacion cuando no hay recomendacion

Tipos actuales:
- `Task`
- `Gap`

Reglas importantes:
- prioriza tasks abiertas cuando existen
- puede promover gaps desde hypotheses fuertes cuando no hay tasks abiertas
- solo hypotheses en estado `Proposed` o `UnderEvaluation` son elegibles como `Gap`
- hypotheses ya cerradas por conclusions aceptadas no deberian volver a salir como siguiente paso
- telemetria de metricas malformada no debe romper `ctx next` ni otros comandos normales del CLI

Factores actuales para tasks:
- `stateScore`
- `goalPriorityScore`
- `hypothesisScore`
- `dependencyReadinessScore`

```powershell
dotnet run --project .\Ctx.Cli -- next
```

### `ctx check`

Verifica si el thread de una task esta listo para un cognitive commit.

Opciones:
- `--task <taskId>` para forzar una task especifica

Si no se pasa `--task`, CTX resuelve la task foco mediante:
- la unica task en `InProgress`, cuando eso es no ambiguo
- en otro caso, la task mejor rankeada por `ctx next`
- en ultimo caso, la unica task abierta restante

Devuelve:
- id y titulo de la task enfocada
- motivo de seleccion
- conteos de hypotheses, evidence, decisions y conclusions vinculadas
- conteos de decisions y conclusions aceptadas
- `readyForCommit`
- items faltantes de cierre
- `runbookSuggestions` compactos cuando existen `OperationalRunbook` relevantes para el hilo enfocado
- `additionalRunbooksAvailable` cuando hacen match mas runbooks que los permitidos por el limite de sugerencias
- guia corta para el siguiente paso de closeout

```powershell
dotnet run --project .\Ctx.Cli -- check
dotnet run --project .\Ctx.Cli -- check --task <taskId>
```

### `ctx closeout`

Explica que separa todavia al estado `working` del `HEAD`.

Devuelve:
- `dirty`
- si existen cambios cognitivos pendientes
- un resumen compacto del diff
- artefactos pendientes con `changeType`, `entityType`, `entityId` y `summary`
- una guia corta para el closeout cognitivo
- `microCloseout` opcional cuando el delta pendiente es lo bastante chico como para tratarlo como un cierre chico y no como un bloque nuevo completo

Uso tipico:
- correrlo despues de registrar evidence o conclusions
- correrlo antes de `ctx commit`
- correrlo antes del commit Git cuando no esta claro si CTX ya quedo cerrado

```powershell
dotnet run --project .\Ctx.Cli -- closeout
```

### `ctx preflight`

Ejecuta un preflight operativo compacto para una operacion critica antes de ejecutarla o antes del closeout Git.

Opciones:
- `--operation <git-closeout|publish-local|viewer-validation|recover-index-lock>`
- `--goal <goalId>` opcional
- `--task <taskId>` opcional

Devuelve:
- id y label normalizados de la operacion
- scope resuelto (`workspace`, `goal:<id>` o `task:<id>`)
- `runbookSuggestions` compactos
- `additionalRunbooksAvailable` cuando existen mas matches que los permitidos por el limite
- guia corta para la operacion critica especifica

Uso tipico:
- antes de commit o push Git: `ctx preflight --operation git-closeout`
- antes de publish local: `ctx preflight --operation publish-local`
- cuando Git queda bloqueado por `index.lock`: `ctx preflight --operation recover-index-lock`

```powershell
dotnet run --project .\Ctx.Cli -- preflight --operation git-closeout
dotnet run --project .\Ctx.Cli -- preflight --operation publish-local --task <taskId>
```

### `ctx runbook add`

Agrega un `OperationalRunbook` compacto para procedimientos recurrentes, troubleshooting, politicas o guardrails.

Opciones:
- `--title <text>`
- `--kind Procedure|Troubleshooting|Policy|Guardrail`
- `--when <text>`
- `--trigger <text>` repetible
- `--precondition <text>` repetible
- `--do <text>` repetible
- `--verify <text>` repetible
- `--signal <text>` repetible
- `--escalate <text>` repetible
- `--reference <text>` repetible
- `--goal <goalId>` repetible
- `--task <taskId>` repetible

Reglas de diseño:
- mantener el runbook compacto
- resumir el camino operativo en vez de duplicar docs largas
- preferir scripts, comandos y referencias canonicas
- mantener `Preconditions`, `FailureSignals` y `EscalationBoundary` cortos y chequeables

```powershell
dotnet run --project .\Ctx.Cli -- runbook add --title "Local publish" --kind Procedure --trigger publish-local --when "Use when refreshing the installed local viewer" --precondition "No installed CTX binary is locked" --do "Run scripts/publish-local.ps1" --verify "Viewer responds locally" --signal "Failed to copy Ctx.Viewer.exe" --escalate "Stop retrying publish if binaries remain locked" --reference "scripts/publish-local.ps1"
```

### `ctx runbook list`

Lista `OperationalRunbook` guardados para el repositorio actual.

```powershell
dotnet run --project .\Ctx.Cli -- runbook list
```

### `ctx runbook show <runbookId>`

Muestra un `OperationalRunbook` especifico.

```powershell
dotnet run --project .\Ctx.Cli -- runbook show <runbookId>
```

### `ctx trigger add`

Agrega un `CognitiveTrigger` compacto para el origen de una linea cognitiva.

Opciones:
- `--summary <text>`
- `--kind UserPrompt|AgentPrompt|Continuation|RunbookTrigger|IssueTrigger`
- `--text <text>`
- `--goal <goalId>` repetible
- `--task <taskId>` repetible
- `--runbook <runbookId>` repetible

```powershell
dotnet run --project .\Ctx.Cli -- trigger add --kind UserPrompt --summary "Fix viewer collapse interaction" --text "The collapse button feels broken and should move to a cleaner rail pattern." --task <taskId>
```

### `ctx trigger list`

Lista `CognitiveTrigger` guardados para el repositorio actual.

```powershell
dotnet run --project .\Ctx.Cli -- trigger list
```

### `ctx trigger show <triggerId>`

Muestra un `CognitiveTrigger` especifico.

```powershell
dotnet run --project .\Ctx.Cli -- trigger show <triggerId>
```

## Grafo Cognitivo

### `ctx graph summary`

Devuelve una vista rapida del grafo cognitivo actual.

Incluye:
- branch y `HEAD`
- conteo total de nodos y edges
- conteo por tipo de entidad
- focos disponibles para `lineage`

```powershell
dotnet run --project .\Ctx.Cli -- graph summary
```

### `ctx graph show <nodeId>`

Devuelve un nodo puntual del grafo y sus conexiones inmediatas.

Acepta:
- un ID crudo, por ejemplo `<hypothesisId>`
- o un ID completo, por ejemplo `Hypothesis:<hypothesisId>`

Incluye:
- `node`
- `incoming`
- `outgoing`
- `connectedNodes`

```powershell
dotnet run --project .\Ctx.Cli -- graph show <hypothesisId>
dotnet run --project .\Ctx.Cli -- graph show Hypothesis:<hypothesisId>
```

### `ctx graph export`

Exporta la proyeccion del grafo cognitivo.

Opciones:
- `--format json|mermaid`
- `--commit <commitId>`

Formatos actuales:
- `json`
- `mermaid`

```powershell
dotnet run --project .\Ctx.Cli -- graph export --format json
dotnet run --project .\Ctx.Cli -- graph export --format mermaid
dotnet run --project .\Ctx.Cli -- graph export --format json --commit <commitId>
```

### `ctx graph lineage`

Devuelve un subgrafo focalizado para navegar linaje semantico.

Focos soportados:
- `--goal <id>`
- `--task <id>`
- `--hypothesis <id>`
- `--decision <id>`
- `--conclusion <id>`

Opciones comunes:
- `--format json|mermaid`
- `--output <path>`

Casos tipicos:

```powershell
dotnet run --project .\Ctx.Cli -- graph lineage --goal <goalId>
dotnet run --project .\Ctx.Cli -- graph lineage --task <taskId>
dotnet run --project .\Ctx.Cli -- graph lineage --hypothesis <hypothesisId> --format mermaid
dotnet run --project .\Ctx.Cli -- graph lineage --decision <decisionId> --output .\tmp\decision-lineage.json
```

## Reconstruccion de Hilo

### `ctx thread reconstruct --task <id>`

Reconstruye un hilo cognitivo formal para una task.

Salida actual:
- `focus`
- `semanticThread`
- `timeline`
- `branchContext`
- `openQuestions`
- `gaps`

Opciones:
- `--task <id>` requerido
- `--format json|markdown`

Estado actual:
- el foco soportado hoy es `task`
- `json` es la salida estructurada
- `markdown` es la salida narrativa legible

```powershell
dotnet run --project .\Ctx.Cli -- thread reconstruct --task <taskId>
dotnet run --project .\Ctx.Cli -- thread reconstruct --task <taskId> --format markdown
```

## Goals

### `ctx goal add`

Crea un objetivo.

Opciones:
- `--title <text>` requerido
- `--description <text>`
- `--priority <n>`
- `--parent <goalId>`

```powershell
dotnet run --project .\Ctx.Cli -- goal add --title "Ship first CVCS core" --priority 1
dotnet run --project .\Ctx.Cli -- goal add --title "Improve viewer" --parent <goalId>
```

### `ctx line open`

Abre una linea tactica de trabajo debajo de un goal existente creando un goal hijo y, opcionalmente, la primera task debajo de ese goal hijo.

Opciones:
- `--goal <goalId>` goal padre requerido
- `--title <text>` titulo requerido de la linea tactica
- `--description <text>`
- `--priority <n>` por defecto usa la prioridad del goal padre
- `--task-title <text>` titulo opcional de la primera task
- `--task-description <text>`

```powershell
dotnet run --project .\Ctx.Cli -- line open --goal <goalId> --title "Viewer working-focus UX"
dotnet run --project .\Ctx.Cli -- line open --goal <goalId> --title "Viewer working-focus UX" --task-title "Reduce umbrella-goal noise in Working view"
```

Notas:
- usarlo cuando el goal estrategico debe seguir activo pero el trabajo nuevo necesita su propia linea cognitiva tactica
- es un flujo de conveniencia sobre `goal add --parent` mas `task add --goal <subGoalId>`

### `ctx goal list`

Lista goals del repositorio.

```powershell
dotnet run --project .\Ctx.Cli -- goal list
```

### `ctx goal show <goalId>`

Muestra un goal especifico.

```powershell
dotnet run --project .\Ctx.Cli -- goal show <goalId>
```

## Tasks

### `ctx task add`

Crea una task.

Opciones:
- `--title <text>` requerido
- `--description <text>`
- `--goal <goalId>`
- `--depends-on <taskId,taskId>`
- `--parent <taskId>`

```powershell
dotnet run --project .\Ctx.Cli -- task add --title "Implement commit engine" --goal <goalId>
dotnet run --project .\Ctx.Cli -- task add --title "Render graph panel" --goal <goalId> --depends-on <taskId>
dotnet run --project .\Ctx.Cli -- task add --title "Fix graph selection edge case" --parent <taskId>
```

Notas:
- `--parent` registra la nueva task como subtask del task indicado
- si omites `--goal`, la subtask hereda el goal del padre
- no se permite combinar `--parent` con un `--goal` distinto al goal del padre

### `ctx task update <taskId>`

Actualiza una task.

Opciones:
- `--title <text>`
- `--description <text>`
- `--state Draft|Ready|InProgress|Blocked|Done`

```powershell
dotnet run --project .\Ctx.Cli -- task update <taskId> --state Done
```

### `ctx task list`

Lista tasks.

```powershell
dotnet run --project .\Ctx.Cli -- task list
```

### `ctx task show <taskId>`

Muestra una task puntual.

```powershell
dotnet run --project .\Ctx.Cli -- task show <taskId>
```

## Hypotheses

### `ctx hypo add`

Crea una hypothesis.

Opciones:
- `--statement <text>` requerido
- `--rationale <text>`
- `--task <taskId>`
- `--probability <0..1>` o `--confidence <0..1>`
- `--impact <0..1>`
- `--evidence-strength <0..1>`
- `--cost-to-validate <0..1>`

Notas:
- `confidence` y `probability` convergen al mismo valor
- CTX calcula `score` automaticamente

```powershell
dotnet run --project .\Ctx.Cli -- hypo add --statement "Structured commits reduce repeated iterations" --task <taskId>
dotnet run --project .\Ctx.Cli -- hypo add --statement "A compact timeline will improve readability" --task <taskId> --probability 0.8 --impact 0.9 --evidence-strength 0.5 --cost-to-validate 0.2
```

### `ctx hypo update <hypothesisId>`

Actualiza una hypothesis.

Opciones:
- `--statement <text>`
- `--rationale <text>`
- `--probability <0..1>` o `--confidence <0..1>`
- `--impact <0..1>`
- `--evidence-strength <0..1>`
- `--cost-to-validate <0..1>`
- `--state Proposed|UnderEvaluation|Supported|Refuted|Archived`

Uso tipico:
- promover hypotheses cerradas despues de validar evidence
- archivar hypotheses stale

```powershell
dotnet run --project .\Ctx.Cli -- hypo update <hypothesisId> --state Supported
```

### `ctx hypo rank`

Devuelve hypotheses ordenadas por score.

Util para:
- priorizacion de validacion
- inspeccion de gaps
- UI y planning

```powershell
dotnet run --project .\Ctx.Cli -- hypo rank
```

### `ctx hypo list`

Lista hypotheses.

```powershell
dotnet run --project .\Ctx.Cli -- hypo list
```

### `ctx hypo show <hypothesisId>`

Muestra una hypothesis especifica.

```powershell
dotnet run --project .\Ctx.Cli -- hypo show <hypothesisId>
```

## Evidence

### `ctx evidence add`

Agrega evidencia.

Opciones:
- `--title <text>` requerido
- `--summary <text>`
- `--source <text>`
- `--kind Observation|Experiment|Document|Validation`
- `--confidence <0..1>`
- `--supports <entityType:id,...>`

`--supports` acepta referencias como:
- `hypothesis:<hypothesisId>`
- otras entidades soportadas por el modelo

```powershell
dotnet run --project .\Ctx.Cli -- evidence add --title "Benchmark" --summary "Supports the current hypothesis" --source "pilot" --kind Experiment --supports hypothesis:<hypothesisId>
```

### `ctx evidence list`

Lista evidence.

```powershell
dotnet run --project .\Ctx.Cli -- evidence list
```

### `ctx evidence show <evidenceId>`

Muestra evidence especifica.

```powershell
dotnet run --project .\Ctx.Cli -- evidence show <evidenceId>
```

## Decisions

### `ctx decision add`

Agrega una decision.

Opciones:
- `--title <text>` requerido
- `--rationale <text>`
- `--state Proposed|Accepted|Rejected`
- `--hypothesis <id,id>` o `--hypotheses <id,id>`
- `--evidence <id,id>`

```powershell
dotnet run --project .\Ctx.Cli -- decision add --title "Adopt structured commits" --rationale "Reduces drift" --state Accepted --hypotheses <hypothesisId> --evidence <evidenceId>
```

### `ctx decision list`

Lista decisions.

```powershell
dotnet run --project .\Ctx.Cli -- decision list
```

### `ctx decision show <decisionId>`

Muestra una decision especifica.

```powershell
dotnet run --project .\Ctx.Cli -- decision show <decisionId>
```

## Conclusions

### `ctx conclusion add`

Agrega una conclusion.

Opciones:
- `--summary <text>` requerido
- `--state Draft|Accepted|Rejected`
- `--decision <id,id>` o `--decisions <id,id>`
- `--evidence <id,id>`
- `--goal <id,id>` o `--goals <id,id>`
- `--task <id,id>` o `--tasks <id,id>`

```powershell
dotnet run --project .\Ctx.Cli -- conclusion add --summary "Proceed with structured commits" --state Accepted --decisions <decisionId> --evidence <evidenceId> --tasks <taskId>
```

### `ctx conclusion update <conclusionId>`

Actualiza una conclusion existente.

Opciones:
- `--summary <text>`
- `--state Draft|Accepted|Rejected`

```powershell
dotnet run --project .\Ctx.Cli -- conclusion update <conclusionId> --state Accepted
```

### `ctx conclusion list`

Lista conclusions.

```powershell
dotnet run --project .\Ctx.Cli -- conclusion list
```

### `ctx conclusion show <conclusionId>`

Muestra una conclusion especifica.

```powershell
dotnet run --project .\Ctx.Cli -- conclusion show <conclusionId>
```

## Contexto y Ejecucion

### `ctx context`

Construye un contexto operativo resumido para una corrida.

Opciones:
- `--purpose <text>`
- `--goal <goalId>`
- `--task <taskId>`

Cuando existen `OperationalRunbook` relevantes, CTX inyecta hasta `2` resumenes compactos en el packet y expone el resto como `Additional runbooks available`.

```powershell
dotnet run --project .\Ctx.Cli -- context --purpose "Prepare architecture review" --goal <goalId>
```

### `ctx run`

Ejecuta una corrida de IA usando el contexto actual.

Opciones:
- `--provider openai|anthropic`
- `--model <model>`
- `--purpose <text>`
- `--goal <goalId>`
- `--task <taskId>`

Notas:
- si no hay credenciales, CTX usa fallback offline deterministico

```powershell
dotnet run --project .\Ctx.Cli -- run --provider openai --model gpt-4.1 --purpose "Evaluate next step" --task <taskId>
```

### `ctx run list`

Lista corridas registradas.

```powershell
dotnet run --project .\Ctx.Cli -- run list
```

### `ctx run show <runId>`

Muestra una corrida especifica.

```powershell
dotnet run --project .\Ctx.Cli -- run show <runId>
```

## Versionado Cognitivo

### `ctx commit`

Genera un commit cognitivo del estado actual.

Opciones:
- `-m <message>`
- `--message <message>`

```powershell
dotnet run --project .\Ctx.Cli -- commit -m "seed cognitive graph"
```

### `ctx log`

Muestra el historial del branch actual.

Incluye:
- branch
- cantidad de commits
- resumen
- lista de commits

```powershell
dotnet run --project .\Ctx.Cli -- log
```

### `ctx diff [fromCommitId] [toCommitId]`

Calcula diferencias cognitivas.

Casos:
- sin parametros: working context vs base
- un parametro: desde commit dado hasta working context
- dos parametros: entre ambos commits

Resume cambios en:
- decisions
- hypotheses
- evidence
- tasks
- conclusions
- conflicts

```powershell
dotnet run --project .\Ctx.Cli -- diff
dotnet run --project .\Ctx.Cli -- diff <fromCommitId>
dotnet run --project .\Ctx.Cli -- diff <fromCommitId> <toCommitId>
```

### `ctx branch <name>`

Crea un branch nuevo desde el `HEAD` actual.

```powershell
dotnet run --project .\Ctx.Cli -- branch feature-context-ranking
```

### `ctx checkout <name>`

Cambia al branch indicado y actualiza el working context.

```powershell
dotnet run --project .\Ctx.Cli -- checkout main
```

### `ctx merge <sourceBranch>`

Integra otra rama en la rama actual.

Notas:
- si hay divergencia cognitiva, devuelve conflictos explicitos

```powershell
dotnet run --project .\Ctx.Cli -- merge feature-context-ranking
```

## Inspeccion Operativa

### `ctx packet list`

Lista packets generados.

```powershell
dotnet run --project .\Ctx.Cli -- packet list
```

### `ctx packet show <packetId>`

Muestra un packet especifico.

```powershell
dotnet run --project .\Ctx.Cli -- packet show <packetId>
```

### `ctx provider list`

Lista providers configurados.

```powershell
dotnet run --project .\Ctx.Cli -- provider list
```

### `ctx metrics show`

Muestra metricas acumuladas.

Incluye:
- runs totales
- tokens
- costo ACU
- tiempo total
- redundancia evitada
- invocaciones por comando

```powershell
dotnet run --project .\Ctx.Cli -- metrics show
```

### `ctx usage summary`

Resume el uso real de la CLI por comando.

Incluye:
- total de invocaciones
- comandos usados
- frecuencia por comando
- exitos y fallos
- comandos todavia no usados

```powershell
dotnet run --project .\Ctx.Cli -- usage summary
```

### `ctx usage coverage`

Compara el catalogo conocido de comandos contra la telemetria real.

Incluye:
- comandos conocidos
- comandos usados
- comandos no usados
- porcentaje de cobertura
- listas de `usedCommands` y `unusedCommands`

```powershell
dotnet run --project .\Ctx.Cli -- usage coverage
```

## Portabilidad

### `ctx export`

Exporta el repositorio actual a un snapshot JSON portable.

Opciones:
- `--output <path>`

```powershell
dotnet run --project .\Ctx.Cli -- export --output .\tmp\ctx-export.json
```

### `ctx import`

Importa un snapshot exportado.

Opciones:
- `--input <path>`

```powershell
dotnet run --project .\Ctx.Cli -- import --input .\tmp\ctx-export.json
```

## Flujo Recomendado

Flujo corto de trabajo cognitivo:

```powershell
dotnet run --project .\Ctx.Cli -- status
dotnet run --project .\Ctx.Cli -- audit
dotnet run --project .\Ctx.Cli -- next
dotnet run --project .\Ctx.Cli -- closeout
dotnet run --project .\Ctx.Cli -- goal add --title "Validar flujo"
dotnet run --project .\Ctx.Cli -- task add --title "Ejecutar caso end-to-end" --goal <goalId>
dotnet run --project .\Ctx.Cli -- hypo add --statement "CTX reduce retrabajo" --task <taskId>
dotnet run --project .\Ctx.Cli -- evidence add --title "Pilot result" --summary "Supports the current hypothesis" --supports hypothesis:<hypothesisId>
dotnet run --project .\Ctx.Cli -- conclusion add --summary "Proceed" --state Accepted --tasks <taskId>
dotnet run --project .\Ctx.Cli -- commit -m "close working block"
```

Flujo de inspeccion y trazabilidad:

```powershell
dotnet run --project .\Ctx.Cli -- graph summary
dotnet run --project .\Ctx.Cli -- graph lineage --task <taskId> --format mermaid
dotnet run --project .\Ctx.Cli -- thread reconstruct --task <taskId> --format markdown
dotnet run --project .\Ctx.Cli -- log
dotnet run --project .\Ctx.Cli -- diff
```

