# Diseno de OperationalRunbook
Si un modelo de lenguaje y su agente pierden el contexto, esta es la herramienta que necesitas.

## Objetivo

Definir una entidad de primer nivel, compacta y barata, para conocimiento operativo recurrente que pueda entrar en los packets de CTX sin inflar materialmente el costo de contexto.

`OperationalRunbook` sirve para capturar:

- procedimientos recurrentes
- troubleshooting recurrente
- politicas operativas
- guardrails que deben aplicarse antes de que el agente derive

No reemplaza tasks, docs ni scripts.
Agrega una capa operativa compacta que orienta al agente hacia el camino canonico antes de improvisar.

## Por que CTX lo necesita

Hoy CTX ya guarda:

- trabajo cognitivo activo en `working`
- estado durable mediante goals, tasks, hypotheses, evidence, decisions y conclusions
- prompts, scripts y docs fuera del modelo de packet

Lo que todavia falta es una superficie estructurada para conocimiento operativo recurrente como:

- como publicar localmente
- cuando se permite el closeout Git
- como reaccionar ante `.git/index.lock`
- como validar el viewer local despues del publish

Eso no deberia vivir solo como prosa larga en docs ni depender de que el agente lo redescubra desde chat.

## Modelo de versionado

`OperationalRunbook` no forma parte del `WorkingContext` mutable.

En cambio, CTX lo versiona mediante `RepositorySnapshot`:

- `working-context.json` queda enfocado en el estado cognitivo activo de ejecucion
- `.ctx/runbooks/` guarda la memoria operativa estable en disco
- `ContextCommit.Snapshot` ahora captura ambos:
  - `WorkingContext`
  - `Runbooks`

Asi CTX puede versionar memoria operativa recurrente sin contaminar el modelo del workspace en progreso.

## Regla de diseno

`OperationalRunbook` debe mantenerse:

- compacto
- descriptivo
- barato de seleccionar
- barato de inyectar en packets

Si un runbook crece demasiado, debe resumir y apuntar a referencias canonicas en vez de duplicarlas.

## Entidad minima

Campos propuestos:

- `Id`
- `Title`
- `Kind`
- `Triggers`
- `WhenToUse`
- `Preconditions`
- `Do`
- `Verify`
- `FailureSignals`
- `EscalationBoundary`
- `References`
- `GoalIds`
- `TaskIds`
- `State`
- `Trace`

### `Title`

Nombre corto orientado al operador.

Ejemplos:

- `Local publish`
- `Git closeout`
- `Recover index.lock`

### `Kind`

Valores minimos permitidos:

- `Procedure`
- `Troubleshooting`
- `Policy`
- `Guardrail`

### `Triggers`

Strings compactos de activacion.

Ejemplos:

- `publish-local`
- `git-commit`
- `git-push`
- `index.lock`
- `viewer`

En la primera version no hace falta un DSL complejo.

### `WhenToUse`

Una frase corta de activacion.

Ejemplo:

- `Use when publishing the local CLI or viewer build.`

### `Do`

Lista corta y ordenada de acciones.

Guia dura:

- preferir `3-5` items
- cada item debe ser corto
- usar comandos o paths canonicos, no prosa larga

### `Preconditions`

Lista corta y ordenada de condiciones que ya deberian cumplirse antes de seguir el runbook.

Guia dura:

- preferir `2-4` items
- mantenerlos binarios y chequeables
- usarlos para frenar deriva antes de ejecutar

### `Verify`

Lista corta y ordenada de checks para confirmar que el runbook se aplico bien.

### `FailureSignals`

Lista corta de sintomas concretos que activan troubleshooting o guardrails.

Ejemplos:

- `.git/index.lock`
- `Failed to copy Ctx.Viewer.exe`
- `127.0.0.1:5271 does not respond`

### `EscalationBoundary`

Lista corta que describe cuando el runbook debe detenerse y devolver el control al operador en vez de seguir forzando recuperacion.

Ejemplos:

- `No borrar el lock si git.exe sigue corriendo`
- `No seguir reintentando publish mientras el binario instalado siga en uso`

### `References`

Paths o comandos canonicos de soporte.

Ejemplos:

- `docs/LOCAL_CTX_INSTALLATION.md`
- `scripts/publish-local.ps1`
- `ctx audit`
- `ctx closeout`

### `GoalIds` y `TaskIds`

Scope explicito minimo:

- vacio + vacio = runbook global
- `GoalIds` = alcance estrategico/tactico
- `TaskIds` = alcance exacto de ejecucion

### `State`

Ciclo de vida minimo:

- `Active`
- `Archived`

## Que no es un OperationalRunbook

No es:

- un reemplazo de `Task`
- un reemplazo de `Evidence`
- un documento procedimental largo
- un historial de lo que ocurrio en una sola ejecucion

Reglas:

- usar `Task` para trabajo ejecutable
- usar `Evidence` para hechos observados
- usar docs/scripts para el procedimiento detallado canonico
- usar `OperationalRunbook` para guia operativa reusable y compacta

## Politica de inyeccion en packets

El packet no debe incluir todos los runbooks que hagan match.

Limite duro por defecto:

- incluir como maximo `2` runbooks en el packet principal

Motivo:

- menor costo en tokens
- menor interferencia entre instrucciones
- mejor foco operativo

## Orden de seleccion

Cuando mas de un runbook hace match, rankear en este orden:

1. match exacto por `TaskId`
2. match por `GoalId`
3. match exacto de trigger contra el `purpose` del packet
4. `Guardrail` antes que `Procedure` cuando exista riesgo operativo
5. `Troubleshooting` solo cuando exista una senal real de falla
6. prioridad manual estable o titulo como desempate final

Todos los runbooks activos unidos a la tarea seleccionada entran al packet principal. El limite normal de `2` runbooks aplica solo para matches de fallback por goal, triggers y guardrails globales.

## Manejo de overflow

Si hacen match mas runbooks que los permitidos por el packet:

- inyectar todos los runbooks unidos explicitamente a la tarea
- completar con los mejores matches restantes hasta el limite normal
- dejar el resto fuera del cuerpo principal
- exponerlos como `available runbooks`

Patron compacto sugerido:

```text
Operational Runbooks
- Local publish
  When: publishing the local CLI or viewer build
  Preconditions: release build exists; installed binaries are not locked
  Do: run scripts/publish-local.ps1; verify C:\ctx outputs; validate installed viewer
  Verify: ctx audit clean; local viewer responds
- Git closeout
  When: before git commit or git push
  Preconditions: ctx audit clean; closeout reviewed
  Do: run ctx closeout; ensure no .git/index.lock; commit CTX before Git
  Verify: git status clean; CTX clean
  Escalate: switch to lock recovery if index.lock appears

Additional runbooks available: Recover index.lock
```

Asi se preserva descubribilidad sin pagar todo el costo de contexto.

## Contrato para agentes

Los agentes deben tratar `runbookSuggestions` como la lista canonica de playbooks para el packet CTX actual.

Usa esta regla en prompts, instrucciones MCP y adaptadores ACP:

```text
Read runbookSuggestions before acting.
For every returned runbook:
- check Preconditions
- follow applicable Do steps
- validate with Verify
- stop at EscalationBoundary if a failure signal appears
Do not infer playbooks from chat memory or titles.
```

Si `runbookSuggestions` esta vacio, el agente continua desde la tarea recomendada, el context packet y la guidance. Si no esta vacio, el agente debe mencionar que playbooks aplican antes de ejecutar trabajo.

Para `ctx plan`, `data.runbookSuggestions` es la lista efectiva y autoritativa de playbooks del turno.
Se selecciona desde el packet contextual focalizado y se espeja en `data.next.runbookSuggestions` por compatibilidad, asi el agente no tiene que reconciliar dos listas distintas.

## Adjuntar y verificar un runbook de tarea

Usa este flujo cuando un playbook debe acompanar siempre a una tarea especifica:

```powershell
ctx runbook list
ctx runbook attach <runbookId> --task <taskId>
ctx plan --task <taskId> --purpose "continue this task"
ctx check --task <taskId>
```

La verificacion es intencionalmente simple: el `purpose` no incluye el trigger del runbook. Si el runbook aparece igual en `runbookSuggestions`, la relacion por tarea esta manejando la seleccion correctamente.

Resultado esperado:

- los matches directos por `TaskId` aparecen antes que goal, trigger y guardrails globales
- todos los runbooks activos unidos directamente a la tarea aparecen en la lista principal de sugerencias
- el mismo runbook puede seguir unido a su tarea original y tambien a una tarea nueva que reutiliza el procedimiento
- detach elimina solo esa relacion; no borra el runbook

```powershell
ctx runbook detach <runbookId> --task <taskId>
```

## Activacion por falla

Algunos runbooks no deberian entrar nunca por defecto.

Ejemplo:

- `Recover index.lock` solo deberia entrar si existe el lock o si ya hubo una falla Git relevante

Eso mantiene el troubleshooting dormido hasta que realmente haga falta.

## Direccion de persistencia

Para separar conocimiento operativo de trabajo cognitivo mutable, la direccion preferida es:

- `.ctx/runbooks/`

Asi los runbooks quedan fuera de `working-context.json` pero siguen disponibles para construir packets.

## Primeros runbooks que CTX deberia traer

## Ejemplo: pase de consistencia de documentacion

`Documentation consistency pass` se usa cuando el operador dice "documenta todo", "deja todo coherente" o pide revisar todos los MD/TXT.

Contrato operativo compacto:

```text
Operational Runbook
- Documentation consistency pass
  When: hay que alinear README, docs, prompts, ejemplos, release notes y referencias localizadas
  Preconditions: CTX planificado; repo objetivo explicito; limite de publicacion claro
  Do: inventariar .md/.txt; buscar comandos stale y paths locales; actualizar docs canonicos primero; alinear es/zh; actualizar notas de release y CHANGELOG cuando cambie el alcance de release
  Verify: la documentacion de comandos lista la superficie CLI vigente; los idiomas no contradicen el canon; no quedan hard paths locales; ctx audit limpio
  Escalate: detenerse antes de publicar si el pedido queda fuera del alcance; detenerse antes de publicar docs con paths locales o notas sensibles
```

Reglas:

- partir de los docs canonicos en ingles y despues alinear espanol y chino
- revisar obligatoriamente `README.md`, `docs/CLI_COMMANDS.md`, `docs/TECHNICAL_INDEX.md`, `docs/OPERATIONAL_RUNBOOKS.md`, `CHANGELOG.md` y el `docs/RELEASE_*.md` actual
- escanear examples y prompts, pero no reescribirlos si no contradicen el contrato canonico
- no afirmar MCP parity para comandos CLI-only hasta que existan las herramientas MCP
- mantener futuro y bloqueados en `ctx gaps` / `ctx roadmap`, no como recomendacion ejecutable de `ctx next`

- `Local publish`
- `Git closeout`
- `Recover index.lock`
- `Viewer local validation`

## Postura de implementacion

Este documento define la version minima de alto valor:

- entidad compacta
- seccion compacta en packet
- ranking determinista
- limite duro de overflow
- referencias canonicas en lugar de prosa duplicada

Cualquier cosa mas compleja deberia agregarse solo si el uso real demuestra que este modelo compacto no alcanza.
