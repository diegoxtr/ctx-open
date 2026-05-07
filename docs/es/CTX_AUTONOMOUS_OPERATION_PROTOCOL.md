# Protocolo De Operacion Autonoma Con CTX
Si un modelo de lenguaje y su agente pierden el contexto, esta es la herramienta que necesitas.

## Objetivo

Este documento define como debe trabajar un modelo sobre CTX sin depender de direccion manual constante del usuario.

La idea es simple:

- CTX no es una bitacora posterior
- CTX es el sistema cognitivo que define que sigue

El modelo debe avanzar en base a:

- goals
- tasks
- hypotheses
- evidence
- decisions
- conclusions
- commits cognitivos

## Principio rector

El siguiente paso de trabajo no debe salir del chat.
Debe salir del estado actual del repositorio CTX.

Si el modelo ya tiene suficiente contexto en CTX, debe:

- inspeccionar
- elegir
- ejecutar
- registrar
- cerrar

sin esperar instrucciones del usuario para cada microdecision.

## Regla principal

Antes de tocar codigo o documentacion, el modelo debe responder internamente estas preguntas:

1. cual es el goal activo mas importante
2. que tasks abiertas u operativas cuelgan de ese goal
3. que hypothesis justifican el siguiente paso
4. que evidence falta para validar o rechazar esas hypothesis
5. cual es el bloque mas pequeÃ±o que se puede cerrar hoy

## Regla de no bloqueo

Si CTX ya contiene suficiente informacion para determinar el siguiente paso, el modelo no debe pedir confirmacion al usuario.

Debe asumir continuidad operativa y avanzar.

No requiere confirmacion para continuar.

No debe esperar que el usuario vuelva a escribir `continua`.

La continuidad es el estado por defecto mientras no aparezca una pausa, redireccion o bloqueo real.

Solo debe frenar si aparece uno de estos casos:

- accion destructiva o riesgosa
- decision estrategica con impacto real de producto, legal o comercial
- falta de credenciales o acceso externo
- ambiguedad que no puede resolverse leyendo CTX ni el codigo

Reglas explicitas:

- `si el siguiente paso ya se deduce de goals, tasks, hypotheses, evidence, decisions o conclusions, no preguntar`
- `si no hay un bloqueo real, continuar sin confirmacion explicita del usuario`
- `si el bloqueo es solo de habito conversacional, ignorarlo y continuar`
- `si hace falta alineacion externa real, recien ahi preguntar`

## Ciclo operativo obligatorio

### 1. Inspeccion inicial

Siempre empezar con:

```powershell
ctx status
ctx graph summary
ctx log
ctx audit
ctx next
```

Si hace falta foco extra:

```powershell
ctx graph lineage --goal <goalId>
ctx graph lineage --task <taskId>
ctx graph lineage --hypothesis <hypothesisId>
```

## 2. Seleccion del siguiente paso

El modelo debe elegir el siguiente bloque con esta prioridad:

1. tareas ya abiertas que destraben el goal principal
2. hipotesis de mayor score o mayor impacto
3. fricciones observadas repetidas en evidence
4. gaps entre conclusion actual y producto real

Si `ctx audit` detecta inconsistencia importante, primero debe corregirse esa deuda si afecta la fiabilidad de `ctx next`.

Si `ctx next` ya devuelve una recomendacion valida y no hay contradiccion fuerte en evidence o decisions, esa recomendacion debe ser el siguiente bloque por defecto.

No abrir trabajo nuevo si ya hay una tarea activa suficientemente clara.

## Regla de foco

Las tareas deben resolverse una por una.

Eso significa:

- elegir una task activa desde CTX
- cerrarla con evidence, conclusion y commit cognitivo
- recien despues abrir o ejecutar la siguiente

No dispersar trabajo entre varias tareas abiertas al mismo tiempo salvo que CTX ya registre explicitamente una dependencia o bloqueo que lo justifique.

Regla estricta adicional:

- no empezar la implementacion de una segunda tarea mientras la tarea actual no tenga evidence, conclusion, commit cognitivo y commit Git
- despues de cada tarea cerrada, re-inspeccionar CTX y elegir exactamente una siguiente tarea
- si no hay una tarea abierta valida, registrar primero el gap como task antes de seguir

## Regla de secuencia

La secuencia por defecto es lineal y autonoma:

1. cerrar la tarea actual en CTX
2. commitear cognitivamente ese cierre
3. commitear Git el cambio real
4. re-inspeccionar CTX
5. tomar la siguiente tarea abierta o el gap mas fuerte si no hay una tarea valida

No saltear esta secuencia.
No combinar dos tareas en un mismo bloque por comodidad.

Despues de cerrar una tarea, el modelo debe empezar la siguiente automaticamente desde CTX sin esperar una nueva instruccion del usuario.

El modelo no debe pedir permiso para pasar al siguiente paso cuando CTX ya marca cual es.

Si existe una sola tarea abierta, esa es la siguiente.

Si hay varias tareas abiertas, se elige esta prioridad:

1. la que desbloquea el goal principal
2. la que corrige una friccion repetida
3. la que valida la hipotesis mas fuerte todavia abierta

No abrir una nueva tarea por costumbre si CTX ya tiene una tarea abierta suficiente.

## 3. Apertura de contexto si falta estructura

Si el siguiente paso todavia no existe en CTX, el modelo debe crearlo.

Minimo:

```powershell
ctx goal add --title "<objetivo>"
ctx task add --title "<tarea>" --goal <goalId>
ctx hypo add --statement "<hipotesis>" --task <taskId>
```

No se debe empezar trabajo importante sin:

- task
- hypothesis o decision que lo justifique

## 4. Ejecucion del trabajo

Mientras trabaja, el modelo debe:

- leer el estado actual del codigo
- hacer el cambio minimo util
- validar
- registrar resultados

Regla operativa sobre `.ctx`:

- no editar archivos dentro de `.ctx` manualmente como camino normal de trabajo
- la via por defecto para mutar `.ctx` debe ser siempre `ctx ...`
- editar `.ctx` directo solo se admite como ultimo recurso de recuperacion o cuando existe un bloqueo real que no puede resolverse desde el producto
- si se llega a editar `.ctx` directo, registrar esa excepcion como `evidence` y explicar por que el flujo normal no alcanzaba

## 5. Registro de evidencia

Toda observacion que cambie la direccion del trabajo debe entrar como `evidence`.

Esto incluye:

- un test que falla
- un test que pasa y valida una hipotesis
- una limitacion del modelo actual
- una friccion de UX
- un problema de encoding
- un comando mal usado
- drift entre release e instalacion
- ambiguedad de documentacion

Formato:

```powershell
ctx evidence add --title "<titulo>" --summary "<hallazgo concreto>" --source "<fuente>" --kind Observation
```

Si valida una hipotesis:

```powershell
ctx evidence add --title "<titulo>" --summary "<validacion>" --source "<fuente>" --kind Experiment --supports hypothesis:<hypothesisId>
```

## 6. Registro de decisiones

Una `decision` debe registrarse cuando el modelo fija una regla o selecciona una opcion entre varias.

Ejemplos:

- elegir una formula de scoring
- decidir priorizar viewer sobre CLI
- adoptar una estructura de branch
- elegir un formato de export

Formato:

```powershell
ctx decision add --title "<decision>" --rationale "<motivo>" --state Accepted --hypotheses <id> --evidence <id>
```

## 7. Registro de conclusion

Cada bloque debe cerrar con una `conclusion` que diga:

- que se logro
- que se valido
- que sigue abierto

Formato:

```powershell
ctx conclusion add --summary "<conclusion>" --state Accepted --evidence <id> --goals <goalId> --tasks <taskId>
```

## 8. Commit cognitivo

Al cerrar un bloque coherente:

```powershell
ctx commit -m "<resultado del bloque>"
```

Regla:

- no pasar demasiado tiempo sin commit cognitivo
- cada bloque de trabajo sustantivo debe terminar con commit cognitivo

## 9. Commit de codigo

Despues del commit cognitivo:

- hacer commit Git del cambio real
- pushear si corresponde
- ejecutar Git en serie, no en paralelo
- si aparece `.git/index.lock`, verificar primero si hay procesos `git.exe` vivos y limpiar solo locks huerfanos
- usar `scripts/repair-git-lock.ps1` como preflight de closeout cuando el lock reaparece

Regla estricta de lock Git:

- no intentar `git commit` ni `git push` mientras exista `.git/index.lock`
- no registrar el patron como "despues limpio el lock" y seguir igual
- primero resolver el lock con preflight seguro
- recien despues continuar el closeout Git
- si el lock es fresco o hay procesos `git.exe` vivos, no borrarlo a la fuerza
- tratar `.git/index.lock` como bloqueo operativo real hasta demostrar que es huerfano

Orden recomendado:

1. validar trabajo
2. evidence
3. decision o conclusion
4. commit cognitivo
5. commit Git
6. push

Preflight recomendado para closeout Git:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\repair-git-lock.ps1
git add ...
git commit -m "..."
git push origin main
```

## Como elegir que sigue sin ayuda del usuario

Si el usuario dice `continua` o incluso si no agrega ninguna nueva instruccion pero CTX ya permite seguir, el modelo debe usar este algoritmo:

1. inspeccionar `ctx status`, `ctx log`, `ctx graph summary`, `ctx audit`
2. identificar el goal mas activo o mas estrategico
3. elegir una task abierta o implicita que:
   - aumente valor del producto
   - reduzca friccion
   - valide una hipotesis importante
4. comprobar si esa task ya esta representada en CTX
5. si no lo esta, crearla
6. ejecutar el menor bloque que produzca evidencia real
7. cerrar con evidence, conclusion y commit cognitivo

## Cuando NO preguntar al usuario

No hace falta preguntar para:

- elegir el siguiente bug o mejora obvia
- registrar evidence
- cerrar commits cognitivos
- mejorar documentacion necesaria
- continuar una linea ya abierta en CTX
- validar un cambio con build, test o viewer
- ejecutar el siguiente paso ya implicado por CTX aunque el usuario no lo haya repetido
- corregir una friccion ya registrada como evidence

## Cuando SI preguntar al usuario

Hay que preguntar solo si aparece uno de estos casos:

- conflicto de producto real entre dos direcciones validas
- necesidad de credenciales o acceso externo
- decision legal o comercial sensible
- cambio destructivo o migracion riesgosa
- ambiguedad que no puede resolverse leyendo CTX ni el codigo

## Regla de uso correcto de hypothesis

Una hypothesis no es una tarea.
Una hypothesis explica por que vale la pena hacer algo o que se espera demostrar.

Ejemplos correctos:

- `A ranked list of hypotheses will improve daily planning decisions`
- `Viewer branch semantics reduce confusion during self-hosting`

Ejemplos incorrectos:

- `Implement command X`
- `Update README`

Eso ultimo son tareas, no hipotesis.

## Regla para fallas

Toda falla debe registrarse.

Ejemplos:

- comando mal invocado
- endpoint que devuelve 404
- proceso que no arranca
- patch que falla por encoding
- diferencia entre repo fuente y release instalada

No guardar chat crudo.
Guardar el hecho operativo y su impacto.

## Regla para prioridades

Si hay varias opciones, priorizar:

1. lo que desbloquea trabajo futuro
2. lo que valida una hipotesis fuerte
3. lo que mejora usabilidad del producto
4. lo que reduce friccion repetida
5. lo que mejora la capacidad de CTX para usarse sobre si mismo

## Meta final

El modelo debe comportarse como un operador autonomo de CTX.

Eso significa:

- leer el estado cognitivo
- trabajar desde ese estado
- producir evidencia
- dejar trazabilidad
- decidir el siguiente paso con criterio

No esperar direccion manual cuando el repositorio CTX ya contiene suficiente informacion para continuar.

No esperar una palabra clave de reanudacion para seguir.

## Instruccion permanente para el operador

El operador debe tratar este protocolo como una instruccion activa sobre su propio comportamiento.

No alcanza con documentarlo:

- debe internalizarlo
- debe releerlo cuando detecte que vuelve a pedir confirmaciones innecesarias
- debe corregir inmediatamente ese patron y seguir desde CTX

Frase de control:

`Si CTX ya sabe que sigue, yo tambien debo saberlo y avanzar.`

