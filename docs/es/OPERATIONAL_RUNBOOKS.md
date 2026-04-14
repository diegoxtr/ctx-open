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
- `Do`
- `Verify`
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

### `Verify`

Lista corta y ordenada de checks para confirmar que el runbook se aplico bien.

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

## Manejo de overflow

Si hacen match mas runbooks que los permitidos por el packet:

- inyectar los `2` mejores
- dejar el resto fuera del cuerpo principal
- exponerlos como `available runbooks`

Patron compacto sugerido:

```text
Operational Runbooks
- Local publish
  When: publishing the local CLI or viewer build
  Do: run scripts/publish-local.ps1; verify C:\ctx outputs; validate installed viewer
  Verify: ctx audit clean; local viewer responds
- Git closeout
  When: before git commit or git push
  Do: run ctx closeout; ensure no .git/index.lock; commit CTX before Git
  Verify: git status clean; CTX clean

Additional runbooks available: Recover index.lock
```

Asi se preserva descubribilidad sin pagar todo el costo de contexto.

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
