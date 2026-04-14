# Modelo de Trabajo y Priorizacion en CTX
Si un modelo de lenguaje y su agente pierden el contexto, esta es la herramienta que necesitas.

## Objetivo

Este documento define como CTX deberia razonar sobre trabajo nuevo, trabajo derivado y prioridades operativas sin caer en duplicacion o cambio de contexto innecesario.

La idea central es esta:

- no todo lo que aparece merece una `task` nueva
- no toda prioridad global debe ganarle a un bloqueo local
- no todo hallazgo es una hypothesis
- no todo trabajo relacionado debe abrir otro hilo

CTX necesita distinguir mejor entre:

- roadmap nuevo
- subtrabajo
- trabajo duplicado
- bloqueo
- follow-up cercano

## Taxonomia canonica

Antes de decidir si algo merece una `task`, CTX deberia distinguir estos niveles:

- `goal`
- `sub-goal`
- `issue`
- `gap`
- `task`
- `subtask`
- `blocker`
- `duplicate`
- `follow-up`
- `evidence`

La regla base es:

- `goal` define una linea estrategica durable
- `sub-goal` define una linea tactica debajo de un goal mas amplio
- `operational runbook` define conocimiento operativo reusable y compacto
- `issue` describe que algo molesta, falla o genera friccion
- `gap` nombra la brecha concreta entre el estado actual y el estado que deberia existir
- `task` define el trabajo para cerrar ese gap

Formula corta:

```text
goal -> sub-goal -> task
operational runbook -> guia de ejecucion
issue -> gap -> task
```

### `OperationalRunbook`

Un `OperationalRunbook` guarda conocimiento operativo recurrente y compacto, por ejemplo:

- flujos de publish local
- reglas de closeout Git
- troubleshooting recurrente
- guardrails reutilizables

Usarlo cuando el conocimiento:

- aplica mas de una vez
- debe guiar la ejecucion antes de que el agente improvise
- es demasiado importante para quedar enterrado solo en docs o evidencia pasada
- debe seguir siendo lo bastante chico como para entrar en un packet sin inflar mucho el costo

No usarlo para historial de una sola ejecucion.
Eso sigue perteneciendo a `evidence`, `decision`, `conclusion` y al cierre normal de la task.

### `CognitiveTrigger`

Un `CognitiveTrigger` guarda el origen compacto que abre o redirige una linea cognitiva.

Usarlo cuando:

- un mensaje del usuario abre una linea nueva
- una continuacion del agente redirige materialmente la linea
- un issue recurrente o una activacion de runbook pasa a formar parte del origen de la linea
- el origen debe quedar auditable sin depender del historial externo del chat

No usarlo para cada mensaje chico.
Mensajes como `ok`, `continua` o nudges triviales de continuidad no deberian convertirse en triggers independientes.

### `Goal`

Un `goal` es una linea estrategica que debe seguir teniendo sentido a traves de muchas tasks y commits.

Usarlo cuando el trabajo:

- abre o expande una direccion durable de producto, plataforma u operacion
- debe seguir siendo entendible aunque no haya una sola ejecucion activa
- es mas amplio que una rama tactica puntual

No conviene crear un `goal` nuevo cuando el trabajo solo es una rama tematica de una linea estrategica ya activa.

Los goals estrategicos pueden seguir en estado `Active` aunque no tengan una ejecucion diaria abierta debajo.
Eso no es un bug de cierre por si mismo. Significa que el goal sigue representando una linea durable de producto u operacion.

Regla de cierre:

- mantener un `goal` activo si sigue nombrando una linea estrategica viva
- cerrar o reemplazar un `goal` cuando describia un objetivo acotado que ya no es la linea activa
- no cerrar un goal estrategico solo para que el grafo se vea mas silencioso

### `Sub-goal`

Un `sub-goal` es una linea tactica debajo de un goal existente.

Usarlo cuando el trabajo:

- pertenece a un goal estrategico ya activo
- necesita su propio carril cognitivo porque el goal padre mezcla varios temas
- agrupa varias tasks relacionadas que no conviene colgar directo del goal paraguas

Es la capa correcta para cosas como una rama de UI debajo de un goal del viewer o una rama de packaging debajo de un goal de distribucion.

Regla de cierre:

- mantener un `sub-goal` activo mientras siga agrupando trabajo tactico actual
- cerrar un `sub-goal` cuando esa rama de ejecucion este terminada y ya no necesite su propio carril de trabajo
- conviene abrir un nuevo `sub-goal` antes que colgar tareas no relacionadas directo del goal paraguas

### Regla canonica de estructura

Cuando aparece trabajo nuevo, clasificarlo en este orden:

1. crear un `goal` si el trabajo abre o cambia una linea estrategica durable
2. crear un `sub-goal` si el trabajo pertenece a un goal existente pero necesita su propia rama tematica
3. crear una `task` si el trabajo es una unidad ejecutable concreta dentro de una linea ya existente
4. crear `subtask` solo si el trabajo existe principalmente para ayudar a cerrar una task padre actual

Operativamente:

- mantener abiertos los goals estrategicos
- abrir debajo de ellos las lineas nuevas de UI o producto como `sub-goals`
- apuntar las tasks nuevas a la linea tactica mas cercana y no al goal paraguas por defecto
- usar `ctx line open` cuando la intencion del operador sea "abrir una linea tactica aca y empezar a trabajar dentro de ella"

Regla para el viewer:

- el foco diario de `Working` deberia priorizar `task -> sub-goal -> goal padre`
- la linea tactica mas cercana deberia cargar la mayor parte del peso visual
- los umbrella goals estrategicos pueden seguir activos en el repositorio sin dominar la vista de trabajo
- un goal estrategico deberia aparecer en `Working` sobre todo como contexto liviano de la linea tactica activa, no como sujeto principal cuando no cuelga una task directa de el

### `Issue`

Un `issue` es cualquier problema, friccion o observacion negativa.

Ejemplos:

- el viewer pierde el foco visual al refrescar
- git emitio un `index.lock`
- una hypothesis quedo enlazada a la task equivocada

Un `issue` no implica automaticamente una task nueva.

### `Gap`

Un `gap` es una brecha puntual, nombrable y accionable entre:

- lo que el sistema hace hoy
- y lo que deberia hacer para que el flujo sea coherente

Ejemplo:

- actual: el viewer tiene presets de foco
- deberia: el viewer deberia conservar ese foco entre recargas
- gap: `graph focus does not persist across reloads`

Un `gap` ya tiene suficiente precision para justificar trabajo.
Pero sigue siendo una brecha, no la implementacion en si.

### `Task`

Una `task` es el trabajo ejecutable que cierra un gap o produce una unidad real de roadmap.

Ejemplo:

- gap: `the graph focus does not persist across reloads`
- task: `Persist graph focus selection in viewer`

### Regla de traduccion

Cuando aparece algo nuevo, CTX deberia traducirlo en este orden:

1. describir el `issue`
2. formular el `gap` especifico
3. decidir si hace falta `task`, `subtask`, `blocker` o solo `evidence`

No conviene saltar directo de un issue a una task si la brecha todavia no esta bien nombrada.

## Problema actual

Hoy CTX ya tiene:

- `goal`
- `task`
- `hypothesis`
- `evidence`
- `decision`
- `conclusion`
- scoring para hypotheses
- scoring para `ctx next`

Pero todavia le falta una capa para clasificar la naturaleza del trabajo cuando aparece una nueva necesidad.

Sin esa capa pueden pasar estas cosas:

- abrir tasks redundantes
- crear hypotheses para algo que en realidad es un bug local
- elegir una task de score alto aunque un bloqueo cercano haga mas racional quedarse en el hilo actual
- perder continuidad cognitiva por cambiar de contexto demasiado pronto

## Regla principal

Antes de crear trabajo nuevo, CTX deberia decidir primero que tipo de trabajo es.

Clasificacion propuesta:

- `NewTask`
- `Subtask`
- `Duplicate`
- `Blocker`
- `RelatedFollowup`
- `EvidenceOnly`

## Tipos de trabajo

### `NewTask`

Se usa cuando aparece una unidad real de roadmap.

Criterios:

- tiene valor propio para el producto
- no depende semanticamente de una task activa puntual
- merece cierre, evidencia y conclusion propios
- no es una simple extension local de trabajo ya abierto

### `Subtask`

Se usa cuando el trabajo existe para cerrar mejor una task padre.

Criterios:

- nace dentro de una task activa
- tiene resultado verificable propio
- bloquea o destraba la task padre
- no tiene sentido fuerte como item separado de roadmap

No conviene abrir `subtask` si:

- es un paso trivial de implementacion
- es solo una nota operativa
- es solo una evidencia

### `Duplicate`

Se usa cuando el nuevo trabajo ya esta representado por otra entidad existente.

Criterios:

- misma intencion sustantiva
- mismo problema base
- mismo outcome esperado
- solo cambia el wording o el momento en que fue observado

En ese caso no se crea otra task.

Se debe:

- enlazar con la entidad existente
- registrar evidencia adicional
- opcionalmente marcar la nueva propuesta como `duplicate-of`

### `Blocker`

Se usa cuando algo impide cerrar una task actual o cercana.

Criterios:

- el problema aparece durante la ejecucion de una task activa
- impide validar o terminar esa task
- resolverlo cerca tiene mas valor que saltar a otra prioridad global

`Blocker` no siempre implica una task nueva.
A veces solo implica:

- evidencia
- decision
- o una subtask puntual

### `RelatedFollowup`

Se usa cuando el trabajo no es duplicado, pero tampoco es una task nueva completamente independiente.

Criterios:

- expande una linea ya abierta
- refina una hypothesis
- agrega una mejora natural de un feature ya trabajado
- depende fuertemente del mismo contexto

### `EvidenceOnly`

Se usa cuando no hay trabajo nuevo real, solo conocimiento nuevo.

Ejemplos:

- una falla observada
- una limitacion del entorno
- una medicion
- una validacion de hipotesis

En ese caso debe registrarse como `evidence`, no como `task`.

## Regla de deduplicacion

Antes de crear una task nueva, CTX deberia ejecutar una verificacion conceptual como esta:

1. existe ya una `task` con objetivo equivalente
2. existe ya una `hypothesis` que cubre esta intencion
3. el problema aparecio dentro de una task activa
4. el nuevo item bloquea el cierre de una task actual
5. el trabajo es solo una extension local del hilo actual
6. el problema ya fue resuelto y solo falta cierre cognitivo

Resultado esperado:

- si ya existe una task equivalente: enlazar
- si nace dentro de una task activa: `subtask` o `blocker`
- si es continuidad natural: `related-followup`
- si solo agrega conocimiento: `evidence`
- si realmente abre roadmap nuevo: `new task`

## Regla de proximidad

La prioridad no deberia salir solo del score global.

CTX deberia considerar tambien la cercania al nodo activo.

Ejemplo:

- estas cerrando una task
- aparece un bug pequeno que bloquea su validacion
- otra task remota tiene score global mayor

En muchos casos, cambiar de contexto seria peor que resolver primero el bloqueo local.

Por eso conviene distinguir:

- `globalPriority`
- `executionPriority`

## Scoring propuesto

Hoy `ctx next` ya usa un score global razonable.

La siguiente evolucion deberia sumar una capa de ejecucion:

```text
executionScore =
  globalPriorityScore
+ proximityScore
+ unblockScore
- contextSwitchCost
- duplicationRisk
```

### `globalPriorityScore`

Lo que ya surge de:

- prioridad del goal
- estado de la task
- score de hypotheses relacionadas

### `proximityScore`

Mide que tan cerca esta el nuevo trabajo del nodo activo.

Ejemplos que suben proximidad:

- misma task
- mismo goal
- misma hypothesis
- mismo commit thread

### `unblockScore`

Sube si resolver ese trabajo destraba validacion o cierre inmediato.

### `contextSwitchCost`

Sube cuando moverse a ese trabajo implica perder continuidad cognitiva.

Ejemplos:

- cambiar de goal
- cambiar de area del producto
- abandonar una task con validacion casi cerrada

### `duplicationRisk`

Sube cuando el trabajo se parece mucho a una task, hypothesis o decision ya existente.

## Relaciones que faltan en el modelo

Para hacer esto bien, CTX deberia agregar relaciones mas explicitas entre entidades:

- `parent-task`
- `subtask-of`
- `blocks`
- `blocked-by`
- `duplicate-of`
- `follow-up-to`
- `refines`
- `discovered-during`

Con esas relaciones, el sistema puede explicar mejor por que un trabajo nuevo no merece otra task raiz.

## Regla operativa recomendada

Cuando aparece trabajo nuevo:

1. buscar si ya existe algo equivalente
2. si existe, enlazar en vez de duplicar
3. si nace dentro de una task activa, evaluar `subtask` o `blocker`
4. si solo agrega conocimiento, registrar `evidence`
5. solo crear `task` nueva si realmente abre otra unidad de roadmap

## Comandos futuros utiles

Para soportar este modelo, CTX podria incorporar comandos como:

- `ctx work classify`
- `ctx work attach --duplicate-of <id>`
- `ctx work attach --blocked-by <id>`
- `ctx task add --parent <taskId>`
- `ctx next --context <nodeId>`

Ejemplo:

```powershell
ctx next --context task:<taskId>
```

Eso permitiria elegir el siguiente paso no solo por score global, sino tambien por continuidad del hilo actual.

## Conclusion

La evolucion correcta de CTX no es solo mejorar el scoring.

Es pasar de:

- "que tiene mas score"

a:

- "que conviene hacer ahora, en este hilo, sin duplicar trabajo y sin romper continuidad cognitiva"

Eso requiere:

- clasificacion formal del trabajo
- deduplicacion antes de abrir tasks
- subtasks reales
- modelado explicito de blockers
- prioridad por cercania y capacidad de desbloqueo

