# Topologia del Trabajo y Scoring de Ejecucion
Si un modelo de lenguaje y su agente pierden el contexto, esta es la herramienta que necesitas.

Este documento captura una regla operativa para CTX: no alcanza con modelar entidades. Tambien hay que modelar la naturaleza del trabajo nuevo y decidir si conviene abrir una task, una subtask, un blocker o solo adjuntar evidencia a una linea ya existente.

## Vista operativa recomendada

Para operar CTX, la entrada principal deberia priorizar listas antes que un grafo gigante:

- `History`
- `Changed Entities`
- `Tasks`
- `Hypotheses`
- `Graph`

El grafo sirve para profundidad. La lista sirve para operar.

## Clases de trabajo nuevo

Cuando aparece un nuevo hallazgo, CTX deberia clasificarlo primero dentro de una de estas clases:

- `NewTask`
- `Subtask`
- `Duplicate`
- `Blocker`
- `RelatedFollowup`

La clasificacion cambia la accion correcta:

- un bug encontrado dentro de una task activa suele ser `Blocker` o `Subtask`
- un item ya discutido en otra linea suele ser `Duplicate` o `RelatedFollowup`
- una unidad nueva de roadmap real puede justificar `NewTask`

## Regla para subtasks

Crear `Subtask` solo si:

- el trabajo es necesario para cerrar una task padre
- tiene un resultado verificable propio
- puede bloquear o destrabar a la task padre
- no tiene sentido como task independiente del roadmap

No crear `Subtask` si:

- es solo una nota operativa menor
- es solo una evidencia
- es solo un paso trivial de implementacion

## Jerarquia de artefactos

- `task`: unidad de roadmap
- `subtask`: unidad de ejecucion dentro de una task
- `evidence`: hallazgo verificable
- `hypothesis`: por que vale la pena hacer el trabajo

## Regla de deduplicacion antes de crear trabajo

Antes de abrir una task nueva, CTX deberia chequear:

1. si ya existe una task con objetivo parecido
2. si ya existe una hypothesis que explique lo mismo
3. si el item bloquea una task activa
4. si es una continuacion local del nodo actual
5. si ya fue resuelto y solo falta cierre cognitivo

Resultados posibles:

- crear task nueva
- crear subtask bajo la task actual
- enlazar como blocker
- enlazar como duplicate
- adjuntar evidencia a una task existente
- adjuntar follow-up a una hypothesis existente

## Cercania y continuidad operativa

El siguiente paso no deberia salir solo de prioridad global. Tambien deberia combinar:

- prioridad global
- cercania al nodo activo
- poder de desbloqueo
- costo de cambio de contexto
- riesgo de duplicacion

Una formula util para esto es:

`executionScore = priorityScore + proximityScore + unblockScore - contextSwitchCost - duplicationRisk`

## Relaciones que CTX deberia explicitar

- `parent-task`
- `blocks`
- `blocked-by`
- `duplicate-of`
- `follow-up-to`
- `refines`
- `discovered-during`

Estas relaciones permiten distinguir si algo nace dentro de otra task, si la bloquea, si la duplica o si solo refina una hypothesis previa.

## Comandos utiles a futuro

- `ctx work classify`
- `ctx work attach`
- `ctx work dedupe`
- `ctx next --context <nodeId>`

Ejemplos:

- `ctx next --context task:<id>`
- `ctx work classify --title "Fix viewer closed tasks visibility"`
- `ctx work attach --duplicate-of task:<id>`
- `ctx work attach --blocked-by task:<id>`
- `ctx task add --parent <taskId>`

## Regla simple

Antes de crear trabajo nuevo:

- si ya existe un nodo equivalente, enlazar
- si nace dentro de una task activa, usar `Subtask` o `Blocker`
- si amplia una linea ya abierta, usar `RelatedFollowup`
- crear `NewTask` solo si abre una unidad real de roadmap

## Conclusiones

CTX necesita una capa fuerte de topologia del trabajo, no solo scoring plano. El sistema deberia responder menos a "que tiene mas score" y mas a "que conviene hacer ahora, en este hilo, sin duplicar ni romper continuidad".
