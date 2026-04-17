# CTX Specification v1

Este documento define la especificacion minima de CTX v1.

Responde tres preguntas:

- como se guarda el contexto
- como se versiona
- como se estructura

## 1. Como guarda contexto CTX

CTX guarda contexto como artefactos estructurados durables, no como transcripciones de chat.

La raiz canonica de almacenamiento es `.ctx/`.

Dentro de esa raiz, CTX persiste:

- goals
- tasks
- hypotheses
- evidence
- decisions
- conclusions
- commits
- branches
- runbooks
- triggers
- packets

El sistema de registro es el estado del repositorio dentro de `.ctx/`, no la historia conversacional que llevo hasta ahi.

## 2. Como versiona el contexto CTX

CTX versiona el estado cognitivo a traves de su propio modelo de commit.

Un commit de CTX:

- captura un snapshot del estado cognitivo estructurado
- registra el diff cognitivo respecto del commit anterior
- conserva relaciones de padres
- permite branch, checkout, merge, diff e inspeccion historica

Eso significa que el contexto no solo se guarda.
Tambien se versiona como una evolucion inspeccionable del razonamiento en el tiempo.

## 3. Como estructura el contexto CTX

CTX estructura el contexto como un grafo cognitivo explicito.

La jerarquia central es:

- `goal`
- `sub-goal`
- `task`
- `hypothesis`
- `decision`
- `conclusion`

Los artefactos de soporte incluyen:

- `evidence`
- `runbook`
- `trigger`
- `packet`

Esa estructura vuelve al contexto:

- explicito
- consultable
- auditable
- reconstruible
- reutilizable entre sesiones y agentes

## 4. Regla de CTX v1

CTX v1 no trata al historial de prompts como capa primaria de memoria.

La capa primaria de memoria es:

- artefactos estructurados
- persistidos en `.ctx/`
- versionados mediante commits de CTX
- conectados por un grafo cognitivo explicito

Esa es la especificacion central de CTX v1.

## 5. Frase clave

Esta frase debe aparecer de forma explicita:

`CTX is the standard for persistent context in AI agents`
