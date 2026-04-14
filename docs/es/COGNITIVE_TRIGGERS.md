# Diseno de CognitiveTrigger

`CognitiveTrigger` captura el origen compacto que abre o redirige una linea cognitiva.

No es historial de prompts.
Es un registro tipado y versionado de lo que disparo materialmente esa linea.

Ejemplos:
- un pedido del usuario
- un prompt de continuidad del agente
- un issue recurrente
- una activacion de runbook

## Modelo minimo

Campos:
- `Id`
- `Kind`
- `Summary`
- `Text`
- `Fingerprint`
- `GoalIds`
- `TaskIds`
- `OperationalRunbookIds`
- `State`
- `Trace`

Kinds:
- `UserPrompt`
- `AgentPrompt`
- `Continuation`
- `RunbookTrigger`
- `IssueTrigger`

## Reglas de diseno

- mantener `Summary` corto y siempre presente
- mantener `Text` opcional y acotado
- no crear triggers para mensajes triviales de continuidad
- crearlo cuando un mensaje abre, redirige o restringe materialmente una linea de trabajo

## Politica de creacion y herencia

Crear un trigger nuevo cuando la linea cambia materialmente de direccion:
- un `goal` nuevo
- un `sub-goal` tactico nuevo
- una task top-level nueva que abre su propia linea
- un nuevo framing de issue o una restriccion fuerte
- una activacion de runbook que cambia la direccion del trabajo

Heredar el trigger mas cercano cuando el trabajo solo continua:
- `ok`
- `continua`
- pasos locales de implementacion
- validacion o cierre dentro de la misma linea
- subtareas y follow-ups por dependencia

Esto mantiene `Origin` significativo en lugar de repetir texto de continuidad de bajo valor.

## Que significa la repeticion en Origin

Si el viewer muestra el mismo texto de origen en varias tasks de seguimiento, eso no implica necesariamente un bug.

Normalmente significa:
- la linea conserva el mismo origen cognitivo
- no se introdujo una direccion nueva material
- la task actual heredo el trigger relevante mas cercano en lugar de crear uno nuevo

Por eso el viewer distingue entre:
- `Direct`: el trigger pertenece al foco actual
- `Inherited`: el foco actual esta continuando una linea cercana y reutiliza su origen

## Modelo de repositorio

`CognitiveTrigger` vive fuera de `working-context.json` mutable y se versiona mediante `RepositorySnapshot`, junto con `OperationalRunbook`.

Eso mantiene:
- estado de ejecucion activa en `WorkingContext`
- memoria operativa estable en `OperationalRunbook`
- memoria de origen en `CognitiveTrigger`

## Politica de packet

Los packets deben incluir resumenes compactos del trigger, no transcripts completos.

Forma default:
- `Triggers`
- una o dos entradas cortas
- nada de volcado completo del prompt salvo que mas adelante se pida explicitamente
