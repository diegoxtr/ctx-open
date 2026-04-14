# Guia CTX Viewer
Si un modelo de lenguaje y su agente pierden el contexto, esta es la herramienta que necesitas.

## Que es

`CTX Viewer` es una interfaz visual para inspeccionar un repositorio `.ctx`.

No es un editor. Su funcion principal es ayudarte a ver:

- el historial cognitivo en el tiempo
- la rama que estas mirando
- el commit o snapshot seleccionado
- el grafo de objetivos, tareas, hipotesis, evidencia, decisiones y conclusiones
- el ranking actual de hipotesis

Piensalo como una mezcla entre:

- un historial tipo control de versiones
- un inspector de grafo cognitivo

## Que significa cada parte de la pantalla

La pantalla esta dividida en tres zonas principales.

### 1. Barra superior

Elementos:

- `Repository`
- `Branch`
- boton `Load`
- boton `Refresh`
- toggle `Auto-refresh`

Que hace cada uno:

- `Repository`: es la ruta local donde existe una carpeta `.ctx/`
- `Branch`: es la rama cognitiva que queres inspeccionar
- `Load`: carga el repositorio y refresca toda la vista
- `Refresh`: vuelve a leer el repositorio actual sin recargar la pagina
- `Auto-refresh`: vuelve a cargar automaticamente la vista cada pocos segundos

Comportamiento por defecto actual:

- si no hay una ruta guardada ni ingresada, el viewer primero revisa `CTX_VIEWER_DEFAULT_REPOSITORY_PATH` o `Viewer__DefaultRepositoryPath`
- si no existe una ruta configurada, el viewer resuelve la raiz por defecto desde el `.git` mas cercano del proyecto
- en este repositorio self-host esa raiz de fallback resuelve a `C:\sources\ctx-public`
- la branch cognitiva por defecto es `main`
- `Auto-refresh` arranca activado por defecto salvo que el navegador ya recuerde que lo apagaste
- despues recuerda el ultimo `Repository` y la ultima `Branch` usados en el navegador
- tambien recuerda la preferencia de `Auto-refresh`
- los paneles laterales se pueden redimensionar con los divisores verticales y esa anchura se guarda por modo (`History`, `Split`, `Graph`)
- los paneles laterales tambien se pueden colapsar en rails compactos desde los divisores verticales y ese estado se guarda por modo

Ejemplo:

- `C:\sources\ctx-public\examples\viewer-demo`
- `C:\ctx\workspace\ctx-self-host`

Ejemplo de override:

```powershell
$env:CTX_VIEWER_DEFAULT_REPOSITORY_PATH = "C:\ctx\workspace\ctx-self-host"
dotnet run --project .\Ctx.Viewer --urls http://127.0.0.1:5271
```

## 2. Panel izquierdo

Este panel resume el estado general y despues muestra la timeline.

### Summary Cards

Las tarjetas de arriba significan:

- `Branch`: la rama actualmente cargada
- `Head`: el commit al que apunta esa rama
- `Branches`: cuantas ramas existen en el repositorio
- `Timeline`: cuantos commits visibles hay en la historia
- `Open Tasks`: cuantas tareas siguen abiertas
- `Closed Tasks`: cuantas tareas ya quedaron cerradas
- `Nodes`: cuantos nodos tiene el grafo del snapshot seleccionado
- `Edges`: cuantas relaciones tiene ese grafo

### Top Hypotheses

Esta seccion muestra las hipotesis con mayor `score`.

Cada item muestra:

- `score`: la prioridad calculada
- el ID corto de la hipotesis
- el texto de la hipotesis
- `p`: probability
- `i`: impact

Debajo del titulo tambien aparecen:

- `Last loaded`: cuando se cargo por ultima vez la vista
- `Auto-refresh off` o `Auto-refresh on`: estado actual de frescura del viewer

Interpretacion:

- si una hipotesis aparece arriba, hoy es una de las mas importantes para mirar o validar
- si los valores `impact`, `evidenceStrength` o `costToValidate` estan en `0`, probablemente esa hipotesis es vieja y todavia no fue enriquecida con el modelo nuevo
- las superficies principales del viewer ahora reparten la altura disponible del viewport y dejan el scroll dentro de cada panel, para que `History`, `Split` y `Graph` no se desbalanceen al cambiar de modo
- cuando entras a `Working context`, el grafo ahora prioriza la linea tactica mas cercana de cada task activa; si una task pertenece a un sub-goal, ese sub-goal queda visible y los umbrella goals no se expanden salvo que la task apunte a ellos directamente

### Tasks

Esta seccion muestra explicitamente donde se esta trabajando y que ya fue cerrado.

Se divide en:

- `Active`: tareas que siguen abiertas
- `Closed`: tareas que ya estan `Done`

Cada item muestra:

- `state`
- ID corto
- titulo de la tarea
- goal al que pertenece
- cantidad de hypotheses asociadas
- dependencias si existen

Cada item tambien funciona como accion de foco:

- si haces click en una task, el viewer salta al nodo `Task` correspondiente
- si estabas mirando un commit viejo, vuelve al contexto `working`
- si el grafo estaba oculto en `History`, cambia a una vista con grafo visible
- si el estado de esa task estaba filtrado, lo vuelve a incluir para que la task no desaparezca del grafo

Interpretacion:

- si queres saber en que se esta trabajando ahora, miras `Active`
- si queres saber que lineas ya quedaron cerradas, miras `Closed`
- esta vista evita tener que deducir el trabajo solo desde commits o desde el grafo

## 3. Panel derecho

El panel `Details` ahora usa tabs en lugar de apilar todas las superficies al mismo tiempo:

- `Details`: metadata del commit seleccionado mas detalle crudo del nodo
- `Origin`: procedencia compacta de `CognitiveTrigger` para la task, goal o foco de commit actual
- `Playbook`: guia compacta de `OperationalRunbook` para la task, goal o foco de commit actual
- `Hypotheses`: ranking actual mas estado de frescura

La tab activa queda recordada localmente, asi que el viewer vuelve a la ultima superficie de inspeccion que estabas usando.

La seccion `Playbook` esta pensada para ser compacta:

- muestra hasta dos runbooks principales
- mantiene `When`, `Do`, `Verify` y `References` cortos
- los matches extra quedan colapsados bajo `Additional runbooks available`
- no convierte runbooks en nodos del grafo; la guia queda al costado del grafo en lugar de competir con el

La tab `Origin` usa dos estados de procedencia:

- `Direct`: el foco actual tiene su propio trigger
- `Inherited`: el foco actual esta continuando la linea cognitiva compatible mas cercana y reutiliza ese origen

Por eso, que se repita el mismo texto en `Origin` a veces es el comportamiento correcto. Si una task es una continuacion y no abrio una direccion nueva material, el viewer debe seguir mostrando ese origen heredado en lugar de inventar uno nuevo.

Regla de visibilidad estrategica vs tactica:

- `Working` deberia enfatizar visualmente la `Task` actual y su `Sub-goal` mas cercano
- un `Goal` estrategico padre puede seguir mostrandose como contexto
- los umbrella goals que siguen activos como lineas largas de producto no deberian dominar el working graph cuando no hay una task directa en foco colgando de ellos
- eso significa que un goal estrategico puede seguir siendo correcto en el repositorio y al mismo tiempo quedar visualmente degradado en el viewer

### History

Debajo aparece la historia principal del repositorio.

En `History`, la vista ahora es `branch-first`.

Eso significa que no ves una sola timeline plana.
Ves:

- una lista lateral de `Branches`
- un selector `Order` para `Newest first` o `Oldest first`
- grupos de commits separados por branch
- una tabla densa por branch
- detalle del commit seleccionado al hacer click

Orden de lectura:

1. primero aparece `working`
2. al costado elegis que branches queres ver
3. despues cada branch visible tiene su propio bloque
4. cada fila muestra columnas fijas:
   - `Graph`: carriles y heads
   - `Description`: branch, goal principal, mensaje y ruta cognitiva resumida
   - `Changes`: cantidad de entidades cognitivas tocadas y desglose corto
   - `Date`: fecha/hora exacta
   - `Author`: quien genero ese commit cognitivo
   - `Model`: `modelName` y `modelVersion` si existen, o `not recorded` si la traza no los tiene
   - `Commit`: ID corto

Interpretacion:

- `Changes` no representa archivos modificados como en Git
- representa entidades CTX afectadas por el commit:
  - `task`
  - `hypo`
  - `ev`
  - `dec`
  - `con`

Eso hace que la historia sea mas escaneable y te diga no solo "cuando hubo un commit", sino tambien "cuanto movio cognitivamente ese commit"

Ademas, cada fila ahora intenta mostrar la linea cognitiva mas importante del commit:

- `Goal`
- `Task`
- `Hypothesis`
- `Decision` o `Conclusion`

La idea es que puedas leer rapidamente:

- para que existio ese cambio
- en que tarea impacta
- cual fue la justificacion
- si el commit cambia direccion o cierra algo

### Evidence en detalle

La evidencia se muestra colapsada en el panel de detalle del commit.

Esto permite responder "por que se cerro" sin saturar la vista:

- por defecto, se ve el resumen del commit
- al expandir `Evidence`, se muestran los hallazgos con titulo y resumen

La columna lateral de branches sirve para:

- mostrar solo ciertas ramas
- comparar una branch principal contra otra secundaria
- ocultar ramas que meten ruido
- mantener la historia separada por hilo cognitivo real

El selector `Order` sirve para:

- ver primero lo mas nuevo, que es el comportamiento normal de trabajo diario
- o invertir la lectura para reconstruir una linea desde el origen
- mantener esa preferencia guardada en el navegador junto con el repo y la branch

Al hacer click sobre una fila:

- el commit queda seleccionado
- el panel derecho muestra su descripcion completa
- se cargan mensaje, branch, fecha, autor, modelo, snapshot, parents y diff cognitivo
- tambien aparece una seccion `Cognitive Path` con:
  - goals afectados
  - tasks afectadas
  - hypotheses afectadas
  - decisions afectadas
  - conclusions afectadas

### Split y Graph

`Split` y `Graph` no usan la historia branch-first completa.

En esos modos, el panel izquierdo cambia a un navegador compacto de commits:

- mantiene `Order`
- mantiene filtros de branch
- deja seleccionar commits
- conserva los carriles visuales
- pero reduce la densidad para no robar espacio al grafo

Interpretacion:

- `History`: modo para leer y recorrer historia
- `Split`: modo para alternar entre commit y grafo sin perder demasiado ancho
- `Graph`: modo para priorizar el grafo y dejar la historia solo como selector compacto
- el grafo central pasa a mostrar el snapshot de ese commit

### Que significa `working`

`working` no es un commit.

Representa el estado actual del `.ctx` que todavia no fue cerrado en un commit cognitivo.

Sirve para ver:

- el grafo presente
- lo que estas construyendo ahora

## 3. Panel central

Se llama `Trace Graph`.

Muestra el grafo cognitivo del commit seleccionado o del estado `working`.

Arriba del grafo ahora tambien existe un filtro `Task states`.

Encima de esos checkboxes tambien hay presets de foco:

- `All`
- `Working`
- `Thinking`
- `Closed`

Que significa cada uno:

- `All`: muestra todos los hilos y no limita por foco semantico
- `Working`: muestra `Ready`, `InProgress` y `Blocked`
- `Thinking`: muestra `Draft` y `Ready`
- `Closed`: muestra solo `Done`

Ahora esos presets no son excluyentes entre si:

- podes activar `Working` + `Thinking` al mismo tiempo
- podes sumar `Closed` si queres mezclar trabajo vivo con contexto ya resuelto
- `All` vuelve al modo sin restriccion de foco

El viewer recuerda tres cosas en el navegador:

- el ultimo `Repository`
- la ultima `Branch`
- la combinacion de foco y la mezcla de estados seleccionada

Entonces, si refrescas o recargas la pagina, vuelve al mismo foco de trabajo o de exploracion en lugar de resetear a `All`.

Ese filtro te deja mostrar u ocultar el subgrafo asociado a tareas en estos estados:

- `Draft`
- `Ready`
- `InProgress`
- `Blocked`
- `Done`

Como leerlo:

- si dejas solo `InProgress` y `Ready`, el grafo se concentra en el trabajo activo o inmediato
- si usas `Working`, haces eso mismo en un click
- si usas `Thinking`, ves lo que todavia esta en exploracion o preparacion
- si combinas `Working` + `Thinking`, ves una mezcla de ejecucion y exploracion sin perder el recorte manual por estados
- si sacas `Done`, desaparece gran parte del razonamiento historico ya cerrado
- si activas `Done`, recuperas tambien el contexto cerrado para revisar decisiones pasadas

Importante:

- el filtro no borra informacion del repositorio
- solo cambia que parte del grafo se renderiza
- las tareas filtradas arrastran consigo sus hypotheses, evidence, decisions y conclusions conectadas
- nodos compartidos por tareas no visibles dejan de reexpandir hilos ajenos
- tareas no seleccionadas no vuelven a entrar al grafo por rebote a traves de goals, evidence o decisions compartidas

Cada columna agrupa tipos de nodo:

- `Project`
- `Goal`
- `Task`
- `Hypothesis`
- `Evidence`
- `Decision`
- `Conclusion`
- `Run`
- `ContextPacket`

Cada caja es un nodo.
Cada linea es una relacion.

Ejemplos de relaciones:

- `Goal -> Task`
- `Task -> Hypothesis`
- `Hypothesis -> Evidence`
- `Hypothesis -> Decision`
- `Decision -> Conclusion`

Si un nodo es `Hypothesis`, puede mostrar un badge con `score`.
Los nodos `Task`, `Decision` y `Conclusion` pueden mostrar badges compactos de estado, para que el grafo no sugiera que solo las hypotheses tienen metadata inline relevante.

Eso significa:

- que esa hipotesis ya tiene ponderacion calculada
- y que hoy puede compararse con otras hipotesis

## 4. Panel derecho

Tiene dos partes.

### Commit

Cuando clickeas un commit, ves:

- mensaje
- ID completo
- branch del commit
- autor
- modelo
- fecha
- snapshot hash
- cantidad total de cambios cognitivos
- parents
- resumen del diff
- listas de cambios por tipo

Los `parents` son importantes porque muestran de que commit viene ese commit.

Si haces click en un parent, navegas hacia atras en la historia.

### Metadata de modelo

CTX ahora soporta metadata opcional de:

- `modelName`
- `modelVersion`

Esa metadata vive en `Traceability`.

Importante:

- no todos los commits viejos la van a tener
- si el runtime no la provee, el viewer muestra `not recorded`
- hoy se puede poblar desde variables de entorno explicitas:
  - `CTX_MODEL_NAME`
  - `CTX_MODEL_VERSION`

### Node

Cuando clickeas un nodo del grafo, ves:

- el nodo seleccionado
- `incoming`: relaciones que llegan al nodo
- `outgoing`: relaciones que salen del nodo
- `connectedNodes`: nodos directamente conectados

Esto sirve para responder preguntas como:

- de que tarea nacio esta hipotesis
- que evidencia respalda esta decision
- que conclusion produjo esta decision

## Como leer los branches

Esta es la parte mas importante.

Un `branch` en CTX funciona parecido a una rama de control de versiones, pero aplicado al contexto cognitivo.

No representa codigo distinto solamente.
Representa una linea distinta de evolucion del razonamiento.

## Consistencia semantica entre branches, hypotheses y trabajo

Una branch define una linea de pensamiento separada, no un tipo de entidad.

Reglas claras:

- branch = trayectoria de razonamiento o ejecucion (experimento, investigacion, variante)
- task = trabajo concreto dentro de esa trayectoria
- hypothesis = justificacion o expectativa asociada a una task
- evidence/decision/conclusion = validacion y cierre dentro de esa misma linea

Implicaciones:

- no abrir branches solo para "otra hipotesis"; la hipotesis vive dentro de una branch
- usar branches cuando el razonamiento no debe mezclarse con `main`
- evitar mezclar tareas con direcciones contradictorias en la misma branch

Chequeo recomendado de consistencia:

1. `ctx audit` para consistencia estructural
2. `ctx graph lineage --goal <goalId>` y `ctx graph lineage --task <taskId>` para validar narrativa
3. si una branch se usa solo para un experimento corto, cerrarla con conclusion clara y commit cognitivo

Convencion sugerida de naming:

- `main`: linea principal
- `feature/*`: mejoras de producto
- `research/*`: validacion o investigacion
- `experiment/*`: pruebas de corta vida

### Que significa una branch en CTX

Una branch te permite trabajar una variante sin mezclarla de inmediato con otra.

Ejemplos:

- `main`: linea principal de pensamiento y trabajo
- `feature/ux-timeline`: exploracion enfocada en una mejora del viewer
- `research/validation`: linea de investigacion o prueba

En otras palabras:

- `main` = estado principal
- otra branch = experimento, alternativa o linea de trabajo separada

## Como se ve una branch en el viewer

En la timeline:

- cada commit tiene un badge con su branch de origen
- algunas ramas pueden apuntar al mismo commit
- si una rama esta en `HEAD`, aparece como badge adicional

Esto NO significa necesariamente que haya varias lineas dibujadas como Git grafico completo.

En esta version del viewer:

- la timeline esta organizada por ramas
- cada rama recibe un color o carril visual
- el commit indica en que rama fue creado
- el viewer no calcula todavia un grafo de merges tan sofisticado como Sourcetree

Entonces, cuando veas un branch badge:

- te dice en que linea de trabajo se produjo ese commit

Cuando veas varios badges:

- te dice que mas de una rama hoy referencia ese mismo commit

## Ejemplo simple de branches

Supongamos esto:

1. estas en `main`
2. haces un commit cognitivo
3. creas `feature/ux-timeline`
4. cambias a esa rama
5. haces dos commits
6. volvÃ©s a `main`

Que significa eso:

- `main` conserva su linea principal
- `feature/ux-timeline` avanza por separado
- el viewer te deja cargar cualquiera de las dos ramas desde el selector `Branch`

Cuando cambias el selector `Branch`:

- cambia la rama base de la timeline
- cambia el `Head` mostrado
- cambia el snapshot por defecto que estas inspeccionando

## Diferencia entre branch y commit

Un error comun es confundirlos.

- `commit`: una fotografia puntual y reproducible del contexto
- `branch`: un puntero a una linea de commits

O sea:

- el commit es el snapshot
- la branch es la linea viva que va avanzando

## Diferencia entre branch y working

- `branch`: historia ya registrada
- `working`: estado actual no cerrado en commit

Podrias estar en `main`, pero el `working` contener cambios cognitivos todavia no consolidados.

## Flujo recomendado para usar el viewer

1. abrir el viewer
2. cargar la ruta del repositorio
3. mirar las summary cards
4. revisar el ranking de hipotesis
5. seleccionar un commit de interes
6. leer el diff del commit
7. mirar el grafo de ese snapshot
8. clickear nodos relevantes para inspeccionar conexiones
9. cambiar de branch si queres comparar otra linea de trabajo

## Como interpretar la demo `viewer-demo`

En la demo incluida:

- `main` representa la linea principal
- `feature/ux-timeline` representa una rama de mejora de UX
- `research/validation` representa una linea de validacion o investigacion

La idea de esa demo es mostrar que CTX no solo guarda artefactos, sino tambien caminos alternativos de evolucion del razonamiento.

## Limitaciones actuales del viewer

Hoy el viewer:

- carga repositorios locales `.ctx`
- muestra timeline
- muestra grafo por snapshot
- muestra ranking de hipotesis
- permite refresh manual y auto-refresh opcional
- deja navegar commits y nodos

Hoy el viewer no hace todavia:

- edicion directa desde la UI
- merge visual
- resolucion visual de conflictos
- layout avanzado tipo Git graph completo
- comparacion visual simultanea de dos commits

## Forma corta de entenderlo

Si queres una version corta:

- izquierda: historia, prioridades y tareas abiertas/cerradas
- centro: mapa cognitivo
- derecha: detalle exacto del commit y del nodo

Y sobre branches:

- una branch es una linea separada de evolucion del contexto
- no es otro repositorio
- no es otro proyecto
- es otra trayectoria posible dentro del mismo repositorio cognitivo

