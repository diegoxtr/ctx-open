# Grafo Cognitivo y Linaje en CTX
Si un modelo de lenguaje y su agente pierden el contexto, esta es la herramienta que necesitas.

Este documento define cÃ³mo CTX deberÃ­a representar, exportar y visualizar el grafo cognitivo del trabajo.

La idea central es simple:

- CTX no solo debe guardar artefactos cognitivos;
- CTX debe poder mostrar cÃ³mo esos artefactos se relacionan;
- CTX debe hacer visible la evoluciÃ³n del razonamiento a travÃ©s del tiempo.

## Objetivo

Agregar a CTX una capacidad de linaje cognitivo que permita:

- visualizar objetivos y subobjetivos;
- seguir la cadena entre tareas, hipÃ³tesis, evidencia, decisiones y conclusiones;
- detectar huecos de razonamiento;
- inspeccionar evoluciÃ³n entre commits;
- construir una vista similar al grafo de commits, pero aplicada al conocimiento.

## Problema Que Resuelve

Hoy CTX ya puede persistir y versionar artefactos estructurados.

Pero todavÃ­a cuesta responder visualmente preguntas como:

- quÃ© objetivo originÃ³ esta tarea;
- quÃ© hipÃ³tesis respaldan esta decisiÃ³n;
- quÃ© evidencia sostiene esta conclusiÃ³n;
- quÃ© conclusiones quedaron huÃ©rfanas;
- quÃ© parte del razonamiento cambiÃ³ entre dos commits;
- cuÃ¡l es la lÃ­nea de conocimiento dominante en un branch.

El grafo cognitivo resuelve eso.

## QuÃ© Es el Grafo Cognitivo

Es una proyecciÃ³n relacional del estado cognitivo actual o de un commit determinado.

Debe modelar:

- nodos
- relaciones
- estados
- metadatos
- evoluciÃ³n temporal

## Nodos Principales

Los nodos iniciales del grafo deben ser:

- `Project`
- `Goal`
- `Task`
- `Hypothesis`
- `Evidence`
- `Decision`
- `Conclusion`
- `Run`
- `ContextPacket`
- `ContextCommit`

## Relaciones Principales

Relaciones ya presentes en el modelo:

- `Project -> Goal`
- `Goal -> Task`
- `Task -> Hypothesis`
- `Hypothesis -> Evidence`
- `Hypothesis -> Decision`
- `Evidence -> Decision`
- `Decision -> Conclusion`
- `Evidence -> Conclusion`
- `Run -> ContextPacket`
- `ContextCommit -> WorkingContext snapshot`

Relaciones que conviene explicitar mejor para el grafo:

- `Goal -> Goal` para subobjetivos
- `Task -> Task` para dependencias
- `Conclusion -> Goal` para impacto o cierre
- `RunArtifact -> EntityReference` como relaciÃ³n visualizable
- `ContextCommit -> ContextCommit` como historia y branching

## Modelo Conceptual del Linaje

Una lÃ­nea de conocimiento tÃ­pica deberÃ­a poder verse asÃ­:

```text
Goal
  -> Task
    -> Hypothesis
      -> Evidence
      -> Decision
        -> Conclusion
```

Y una lÃ­nea operativa deberÃ­a poder verse asÃ­:

```text
Task
  -> ContextPacket
    -> Run
      -> RunArtifact
        -> Decision / Evidence / Conclusion
```

Y una lÃ­nea temporal asÃ­:

```text
Commit A -> Commit B -> Commit C
```

## Casos de Uso del Grafo

### 1. Ver estructura del trabajo

Ejemplo:

- quÃ© goals existen
- quÃ© tareas cuelgan de cada goal
- cuÃ¡les no tienen hipÃ³tesis asociadas

### 2. Ver razonamiento justificativo

Ejemplo:

- una decisiÃ³n deberÃ­a mostrar
  - hipÃ³tesis relacionadas
  - evidencia relacionada
  - conclusiones derivadas

### 3. Detectar huecos

Ejemplo:

- decisiones sin evidencia
- hipÃ³tesis sin tareas
- conclusiones sin decisiones
- goals sin tareas

### 4. Ver evoluciÃ³n

Ejemplo:

- quÃ© parte del grafo cambiÃ³ entre dos commits
- quÃ© nodos fueron agregados, removidos o modificados

### 5. Analizar un branch

Ejemplo:

- comparar una lÃ­nea de razonamiento entre `main` y una rama experimental

## DiseÃ±o Recomendado por Etapas

## Etapa 1: ExportaciÃ³n del Grafo

Objetivo:

- generar una proyecciÃ³n exportable sin depender todavÃ­a de UI visual

Comandos sugeridos:

- `ctx graph export --format json`
- `ctx graph export --format mermaid`
- `ctx graph export --format dot`
- `ctx graph export --commit <commitId>`

Salida JSON sugerida:

```json
{
  "nodes": [],
  "edges": [],
  "metadata": {}
}
```

Estructura base sugerida:

- `nodes`
  - `id`
  - `type`
  - `label`
  - `state`
  - `metadata`
- `edges`
  - `from`
  - `to`
  - `relationship`
  - `metadata`

Valor:

- ya permite visualizar con herramientas externas
- mantiene independencia de frontend

## Etapa 2: InspecciÃ³n por CLI

Objetivo:

- consultar el grafo desde la terminal

Comandos sugeridos:

- `ctx graph show`
- `ctx graph focus --goal <id>`
- `ctx graph focus --task <id>`
- `ctx graph lineage --hypothesis <id>`
- `ctx graph lineage --decision <id>`
- `ctx graph diff <commitA> <commitB>`

Valor:

- anÃ¡lisis local rÃ¡pido
- usable en automatizaciÃ³n

## Etapa 3: VisualizaciÃ³n Interactiva

Objetivo:

- navegar el grafo visualmente

Capacidades deseables:

- zoom y paneo
- filtros por tipo
- filtros por estado
- color por branch o estado
- vista por commit
- expansiÃ³n/colapso de subÃ¡rboles
- resaltar cambios entre commits

Opciones tÃ©cnicas:

- Mermaid para vistas simples
- Graphviz para export
- UI web con D3, Cytoscape o React Flow para una versiÃ³n interactiva

## Cambios de Dominio Recomendados

Para que el grafo sea realmente fuerte, conviene enriquecer algunas relaciones.

### 1. Subobjetivos

Agregar a `Goal`:

- `ParentGoalId`

Estado actual:
- implementado en el dominio y representado en el grafo como relaciÃƒÂ³n `subgoal`

Eso permitirÃ­a:

- objetivos
- subobjetivos
- Ã¡rboles de objetivos

### 2. Dependencias entre tareas

Agregar a `Task`:

- `DependsOnTaskIds`

Estado actual:
- implementado en el dominio y representado en el grafo como relaciÃƒÂ³n `depends-on`

Eso permitirÃ­a:

- ver bloqueos
- planificar secuencias
- representar flujos reales

### 3. VÃ­nculo de conclusiones con objetivos

Agregar a `Conclusion`:

- `GoalIds`
- o `TaskIds`

Eso permitirÃ­a:

- mostrar quÃ© objetivo quedÃ³ respaldado por una conclusiÃ³n

### 4. Relaciones tipadas en run artifacts

Hoy `RunArtifact` tiene referencias. Conviene enriquecer eso con semÃ¡ntica mÃ¡s clara:

- `supports`
- `refutes`
- `summarizes`
- `proposes`

Eso mejora mucho el grafo.

## Modelo de ExportaciÃ³n Recomendado

Ejemplo conceptual:

```json
{
  "nodes": [
    {
      "id": "goal:1",
      "type": "Goal",
      "label": "Validar V1",
      "state": "Active",
      "metadata": {
        "priority": 1
      }
    }
  ],
  "edges": [
    {
      "from": "goal:1",
      "to": "task:1",
      "relationship": "contains"
    }
  ],
  "metadata": {
    "branch": "main",
    "headCommitId": "abc123",
    "generatedAtUtc": "2026-04-08T18:00:00Z"
  }
}
```

## Visualizaciones Iniciales Recomendadas

Las primeras vistas Ãºtiles serÃ­an:

### Vista 1: JerarquÃ­a de trabajo

```text
Project
  -> Goals
    -> Tasks
```

### Vista 2: Cadena de razonamiento

```text
Task
  -> Hypothesis
    -> Evidence
    -> Decision
      -> Conclusion
```

### Vista 3: Linaje temporal

```text
Commit
  -> Commit
  -> Commit
```

### Vista 4: Linaje operativo

```text
ContextPacket
  -> Run
    -> Artifact
```

## MÃ©tricas Derivadas del Grafo

Una vez implementado, el grafo habilita mÃ©tricas muy valiosas:

- goals sin tareas
- tareas sin hipÃ³tesis
- hipÃ³tesis sin evidencia
- decisiones sin evidencia
- conclusiones sin decisiones
- nodos huÃ©rfanos
- profundidad media del razonamiento
- densidad del grafo
- cobertura de justificaciÃ³n

Eso puede convertirse en un valor muy fuerte del producto.

## RelaciÃ³n con el Producto

Esto no es un extra cosmÃ©tico.

Es parte de la propuesta central de CTX porque:

- hace visible el conocimiento estructurado;
- diferencia CTX de un simple historial de prompts;
- permite auditar razonamiento;
- conecta commits con conocimiento;
- abre la puerta a una futura UI muy potente.

## Roadmap Recomendado

Orden sugerido:

1. documentar el modelo de grafo
2. agregar relaciones faltantes al dominio
3. implementar `ctx graph export --format json`
4. implementar `ctx graph export --format mermaid`
5. implementar `ctx graph lineage`
6. agregar vista visual interactiva en una etapa posterior

## DecisiÃ³n Recomendada

SÃ­, CTX deberÃ­a incorporar formalmente un grafo cognitivo y de linaje.

No como reemplazo del repositorio estructurado, sino como proyecciÃ³n de alto valor sobre Ã©l.

