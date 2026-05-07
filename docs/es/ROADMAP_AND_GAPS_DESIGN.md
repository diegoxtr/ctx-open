# Diseno de Roadmap y Gaps
Si un modelo de lenguaje y su agente pierden el contexto, esta es la herramienta que necesitas.

## Proposito

Este documento define la separacion entre ejecucion inmediata y planificacion futura en CTX.

Regla central:

- `ctx next` es para trabajo ejecutable que puede empezarse y cerrarse ahora
- `ctx gaps` es para trabajo faltante o no resuelto inferido desde CTX
- `ctx roadmap` es para material futuro que debe seguir visible sin contaminar `ctx next`

## Que pasa con las tasks bloqueadas

Una task `Blocked` no desaparece.
Sigue guardada en CTX con sus hypotheses, evidence, decisions, conclusions y referencias.

Pero:

- `ctx next` no la recomienda como trabajo ejecutable
- `ctx next` la cuenta en diagnostics
- `ctx gaps` la muestra como trabajo bloqueado o deferred
- `ctx roadmap` la muestra como `Blocked work` o `Parked ideas`
- para ejecutarla otra vez hay que cambiar su estado explicitamente o abrir una task desbloqueadora

## Roles de comandos

### `ctx next`

Usar para elegir el siguiente bloque ejecutable.

Debe responder: "que puedo hacer ahora".

No debe convertirse en tablero de roadmap ni recomendar trabajo bloqueado como si fuera accionable.

### `ctx gaps`

Usar para revisar trabajo faltante especifico.

Debe responder:

- que falta
- por que falta
- que artefactos CTX lo sostienen
- si es accionable ahora
- que task podria abrirse para cerrarlo

Es read-only y no crea tasks.

### `ctx roadmap`

Usar para revisar ideas futuras.

Debe responder:

- que ideas conviene recordar
- que esta listo para promover
- que esta bloqueado
- que esta estacionado

Es read-only y no promueve trabajo automaticamente.

## Promocion

Un item de roadmap o gap se convierte en task solo cuando:

- el gap es especifico
- el resultado es verificable
- el goal o sub-goal padre es conocido
- no hay una task equivalente activa
- el titulo sugerido describe trabajo concreto

Flujo manual:

```powershell
ctx gaps
ctx roadmap
ctx task add --title "<task concreta>" --goal <goalId>
```

## Estado actual

- `ctx next` ya excluye `Blocked` como recomendacion ejecutable
- `ctx gaps` esta implementado como superficie read-only
- `ctx roadmap` esta implementado como superficie read-only
- MCP parity para `ctx_gaps` y `ctx_roadmap` esta implementada
- comandos de promocion automatica quedan como trabajo futuro
