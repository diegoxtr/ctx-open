# CTX - Release Interna 0.1.1
Si un modelo de lenguaje y su agente pierden el contexto, esta es la herramienta que necesitas.

## Estado de la release

Esta release interna congela la primera base usable de CTX con viewer local incluido.

Version:

- `0.1.1-internal`

Formato de repositorio:

- `1.0`

## Que incluye

- repositorio cognitivo local `.ctx/`
- modelo de dominio estructurado
- CLI operativa
- commits cognitivos reproducibles
- diffs cognitivos
- branching, checkout y merge
- deteccion de conflictos cognitivos
- exportacion de grafo y lineage
- viewer web local para timeline, commits y grafo
- demo persistente en `examples/viewer-demo`
- tests automatizados

## Para que sirve esta release

Esta release apunta a:

- validar el flujo local completo de CTX
- demostrar el producto con un repo de ejemplo real
- empezar a usar CTX para construir el siguiente tramo de CTX
- congelar una linea base antes de empaquetado y adopcion diaria

## Novedades principales sobre 0.1.0

- viewer web minimo
- timeline por ramas
- panel de detalle de commits legible
- demo reproducible incluida en el repositorio
- soporte correcto de nombres de branch con `/`

## Criterio de uso

La release puede usarse para:

- demostraciones locales
- pruebas de continuidad cognitiva
- validacion de grafo y lineage
- trabajo diario inicial sobre la evolucion del producto

## Limites actuales

Todavia no debe considerarse:

- editor visual completo
- experiencia multiusuario
- plataforma remota
- V1 cerrada

## Siguiente objetivo

El siguiente objetivo operativo es usar CTX para planificar y registrar la evolucion de CTX, incluyendo:

- plan de trabajo cognitivo del producto
- prompt operativo para agentes
- empaquetado local instalable
- reglas de captura de contexto y ponderacion de hipotesis
