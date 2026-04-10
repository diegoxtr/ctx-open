# Reconstruccion del Hilo Cognitivo en CTX
Si un modelo de lenguaje y su agente pierden el contexto, esta es la herramienta que necesitas.

Este documento define como CTX deberia reconstruir formalmente el hilo del contexto a partir de artefactos estructurados.

La meta no es reconstruir una conversacion.

La meta es reconstruir:

- por que se abrio una linea de trabajo
- que hipotesis la justificaron
- que evidencia aparecio
- que decisiones se tomaron
- que conclusiones cerraron el bloque
- como evoluciono todo eso a traves de commits y branches

## Objetivo

Dar a CTX una forma canonica de responder esta pregunta:

`como llegamos al estado cognitivo actual`

Eso implica poder reconstruir:

- hilo local de una entidad
- hilo de una tarea o goal
- hilo de un branch
- hilo temporal entre commits
- hilo consolidado del repositorio

## Problema Actual

Hoy CTX ya tiene:

- entidades estructuradas
- relaciones
- commits cognitivos
- graph export
- lineage focalizado

Pero todavia no tiene una definicion formal de reconstruccion completa.

Eso genera este hueco:

- se puede inspeccionar el grafo
- se puede ver historia de commits
- pero no existe un algoritmo canonico para reconstruir una narrativa estructurada del razonamiento

## Principio Rector

El hilo cognitivo no debe salir de chat logs.

Debe reconstruirse exclusivamente desde:

- `Goal`
- `Task`
- `Hypothesis`
- `Evidence`
- `Decision`
- `Conclusion`
- `ContextCommit`
- `BranchReference`
- `Traceability`

## Que Es Un Hilo Cognitivo

Un hilo cognitivo es una secuencia estructurada y justificable de evolucion del conocimiento.

Tiene dos dimensiones simultaneas:

### 1. Dimension semantica

Responde:

- que artefactos se relacionan entre si
- cual fue la cadena de justificacion

Ejemplo:

```text
Goal
  -> Task
    -> Hypothesis
      -> Evidence
      -> Decision
        -> Conclusion
```

### 2. Dimension temporal

Responde:

- en que orden aparecieron o cambiaron esos artefactos
- en que commit quedaron registrados
- en que branch evolucionaron

Ejemplo:

```text
Commit A -> Commit B -> Commit C
```

## Tipos de Reconstruccion

CTX deberia soportar estas reconstrucciones formales.

### Reconstruccion por entidad

Entrada:

- `GoalId`
- `TaskId`
- `HypothesisId`
- `DecisionId`
- `ConclusionId`

Salida:

- artefacto foco
- antecedentes
- artefactos dependientes
- decisiones relacionadas
- conclusiones derivadas
- commits relevantes

### Reconstruccion por tarea

Responde:

- que problema intentaba resolver la tarea
- que hipotesis la abrieron
- que evidencia produjo
- que decisiones disparo
- si cerro o no con conclusion

### Reconstruccion por goal

Responde:

- que tareas lo implementaron
- que hipotesis dominaron
- que decisiones afectaron al goal
- si hay conclusiones suficientes para declararlo validado o no

### Reconstruccion por branch

Responde:

- cual es la linea de conocimiento dominante en esa rama
- que artefactos divergen respecto de `main`
- que decisiones son exclusivas del branch
- que commits introdujeron esas diferencias

### Reconstruccion temporal

Responde:

- como evoluciono una linea de conocimiento entre commits
- que nodos aparecieron
- que nodos cambiaron de estado
- que evidencia nueva movio una hipotesis o decision

## Modelo Canonico de Reconstruccion

La reconstruccion deberia devolver un artefacto formal, por ejemplo:

```json
{
  "focus": {},
  "semanticThread": [],
  "timeline": [],
  "branchContext": {},
  "openQuestions": [],
  "gaps": []
}
```

### Campo `focus`

El nodo o linea principal de la reconstruccion.

Ejemplo:

- `task:123`
- `goal:abc`
- `hypothesis:xyz`

### Campo `semanticThread`

Secuencia ordenada de artefactos relacionados por semantica.

Ejemplo:

```text
Goal -> Task -> Hypothesis -> Evidence -> Decision -> Conclusion
```

Cada paso deberia incluir:

- `entityType`
- `entityId`
- `relationship`
- `state`
- `summary`

### Campo `timeline`

Secuencia de eventos historicos relevantes.

Cada evento deberia incluir:

- commit id
- branch
- timestamp
- tipo de cambio
- artefacto afectado
- resumen del cambio

### Campo `branchContext`

Metadatos del branch:

- branch actual
- head commit
- commits relevantes
- divergencias si existen

### Campo `openQuestions`

Preguntas que el hilo no puede cerrar aun.

Ejemplos:

- decision sin evidencia suficiente
- hipotesis sin conclusion
- tarea sin decision final

### Campo `gaps`

Huecos estructurales detectados.

Ejemplos:

- evidencia faltante
- conclusion huerfana
- goal sin tareas
- hypothesis sin soporte

## Algoritmo Recomendado

### Paso 1. Elegir foco

El foco puede venir de:

- comando explicito
- nodo seleccionado en viewer
- branch actual
- head commit

### Paso 2. Expandir relaciones semanticas

Expandir el subgrafo relevante en ambos sentidos:

- hacia atras
- hacia adelante

Reglas de expansion:

- `Task` busca `Goal`, `Hypothesis`, `Evidence`, `Decision`, `Conclusion`
- `Hypothesis` busca `Task`, `Evidence`, `Decision`, `Conclusion`
- `Decision` busca `Hypothesis`, `Evidence`, `Conclusion`
- `Conclusion` busca `Decision`, `Evidence`, `Task`, `Goal`

### Paso 3. Ordenar por importancia semantica

La secuencia no debe ser solo topologica.

Debe priorizar:

1. justificadores directos
2. decisiones aceptadas
3. conclusiones aceptadas
4. evidencia mas fuerte
5. artefactos auxiliares

### Paso 4. Recuperar historia temporal

Para cada entidad incluida:

- buscar en que commit aparecio
- buscar commits donde cambio
- buscar branch relevante

### Paso 5. Detectar gaps

Ejemplos:

- `Hypothesis` sin `Evidence`
- `Decision` sin `Evidence`
- `Conclusion` sin `Decision`
- `Task` sin `Hypothesis`
- `Goal` sin `Task`

### Paso 6. Producir narrativa estructurada

La salida final no debe ser solo una lista de nodos.

Debe poder decir:

- origen
- evolucion
- sustento
- resultado
- huecos abiertos

## Salidas Recomendadas

CTX deberia poder producir el hilo en varios formatos.

### 1. JSON estructurado

Para automatizacion y LLMs.

Comando sugerido:

```powershell
ctx thread reconstruct --task <id> --format json
```

### 2. Markdown narrativo

Para humanos.

Comando sugerido:

```powershell
ctx thread reconstruct --goal <id> --format markdown
```

Salida esperada:

- contexto inicial
- hipotesis clave
- evidencia
- decisiones
- conclusion
- gaps

### 3. Mermaid

Para visualizacion.

Comando sugerido:

```powershell
ctx thread reconstruct --hypothesis <id> --format mermaid
```

### 4. Packet listo para agente

Para que otro modelo retome trabajo.

Comando sugerido:

```powershell
ctx thread reconstruct --task <id> --format packet
```

## Diferencia Entre Graph, Lineage y Thread

### `graph`

Muestra el universo relacional.

Pregunta que responde:

- `que nodos y relaciones existen`

### `lineage`

Muestra un subgrafo focalizado.

Pregunta que responde:

- `que rodea a este nodo`

### `thread`

Reconstruye una narrativa cognitiva justificada.

Pregunta que responde:

- `como llegamos a este estado y que significa`

Esa diferencia es clave.

## Integracion con Commits

La reconstruccion debe usar `ContextCommit` como columna vertebral temporal.

Cada paso del hilo deberia poder decir:

- primer commit donde aparece
- ultimo commit donde cambia
- commits intermedios relevantes
- branch en que vive

Esto permite:

- auditoria
- debugging cognitivo
- comparacion entre ramas
- reconstruccion reproducible

## Integracion con Branches

El branch no es solo una lista de commits.

En CTX, un branch deberia poder responder:

- que linea de pensamiento se exploro ahi
- que hipotesis quedaron exclusivas
- que decisiones divergen de `main`
- si las conclusiones fueron o no integradas

Por eso la reconstruccion formal deberia incluir `branchContext`.

## Uso con Agentes

Este es uno de los puntos mas fuertes de CTX.

Un agente no deberia recibir:

- chat completo
- logs crudos
- historia textual desordenada

Deberia recibir un `thread reconstruction packet` con:

- foco
- contexto inicial
- hipotesis principales
- evidencia relevante
- decisiones aceptadas
- conclusiones vigentes
- gaps abiertos
- commit actual

Eso permitiria retomar trabajo con mucha menos ambiguedad.

## MVP Recomendado

No hace falta resolver todo de una vez.

Orden recomendado:

1. definir el modelo `ContextThread`
2. implementar reconstruccion por `Task`
3. agregar salida `json`
4. agregar salida `markdown`
5. exponerlo en CLI
6. mostrarlo en viewer

Comando inicial recomendado:

```powershell
ctx thread reconstruct --task <id> --format markdown
```

## Decision Recomendada

CTX deberia incorporar formalmente una capacidad de reconstruccion del hilo cognitivo.

No como reemplazo de `graph` o `lineage`, sino como una capa superior que:

- sintetiza
- ordena
- justifica
- hace retomable el trabajo

## Impacto en el Producto

Si esto se implementa bien, CTX gana una propiedad muy fuerte:

- no solo guarda conocimiento estructurado
- puede reconstruir por que ese conocimiento termino en el estado actual

Eso vuelve a CTX mucho mas util para:

- auditoria
- continuidad entre sesiones
- trabajo multiagente
- handoff entre personas
- trazabilidad fuerte del razonamiento

