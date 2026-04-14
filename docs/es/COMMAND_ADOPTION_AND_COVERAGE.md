# Adopcion y Cobertura de Comandos
Si un modelo de lenguaje y su agente pierden el contexto, esta es la herramienta que necesitas.

Este documento resume como se esta usando realmente la CLI de CTX en el workspace self-hosted y que conviene hacer con los comandos poco usados o todavia frios.

La idea no es perseguir cobertura por cobertura.

La regla es:

- primero adoptar lo que mejora el flujo diario
- despues validar lo que destraba trabajo futuro
- recien despues cubrir superficies perifericas o raras

## Snapshot actual

Fuente:

- `ctx usage summary`
- `ctx usage coverage`

Estado observado en este repo:

- `totalKnownCommands`: `49`
- `usedCommandCount`: `31`
- `unusedCommandCount`: `21`
- `coveragePercentage`: `63.27`

## Comandos calientes

Estos ya son parte del flujo real del producto:

- `evidence add`
- `commit`
- `status`
- `audit`
- `conclusion add`
- `next`
- `hypo add`
- `hypo update`
- `task update`
- `graph lineage`
- `decision add`
- `task add`

Interpretacion:

- CTX ya se usa fuerte como sistema de trazabilidad, cierre cognitivo y consistencia
- el loop dominante hoy es:
  - observar
  - registrar evidence
  - actualizar state
  - cerrar con conclusion
  - commit cognitivo

## Comandos tibios

Tienen uso real, pero todavia no son parte del loop dominante:

- `log`
- `graph summary`
- `task list`
- `task show`
- `graph show`
- `goal list`
- `hypo rank`
- `hypo show`
- `usage summary`
- `usage coverage`
- `decision list`
- `diff`
- `metrics show`
- `thread reconstruct`
- `version`

Interpretacion:

- estas superficies existen y sirven
- pero todavia no son reflejo del flujo diario principal
- varias dependen de necesidades de inspeccion o debugging, no del cierre operativo normal

## Comandos frios o no usados

Superficies sin uso real todavia:

- `branch`
- `checkout`
- `conclusion show`
- `context`
- `doctor`
- `evidence list`
- `evidence show`
- `export`
- `goal add`
- `goal show`
- `graph export`
- `hypo list`
- `import`
- `init`
- `merge`
- `packet list`
- `packet show`
- `provider list`
- `run`
- `run list`
- `run show`

Interpretacion:

- algunos estan frios porque el flujo todavia no los necesita
- otros estan frios porque el producto todavia no los empuja desde el self-hosting real
- otros estan frios porque son capacidades de borde, no del camino feliz principal

## No todos los comandos frios son igual de importantes

Para priorizarlos, conviene separarlos por valor.

### Tier 1: alto valor cercano al flujo actual

Conviene validar primero:

- `goal add`
- `goal show`
- `hypo list`
- `evidence list`
- `evidence show`
- `conclusion show`
- `doctor`
- `context`

Motivo:

- estan muy cerca del flujo diario real
- cierran huecos de inspeccion y operacion
- pueden mejorar trabajo cotidiano sin introducir mucha complejidad nueva

### Tier 2: valor estructural para CTX como sistema completo

Conviene validar despues:

- `branch`
- `checkout`
- `merge`
- `graph export`
- `export`
- `import`

Motivo:

- son claves para el modelo completo de CTX
- pero hoy no forman parte del camino de self-hosting diario
- necesitan escenarios mas controlados y pruebas mas intencionales

### Tier 3: valor futuro o especializado

Conviene dejar mas adelante:

- `run`
- `run list`
- `run show`
- `provider list`
- `packet list`
- `packet show`

Motivo:

- dependen de proveedores, integraciones o workflows mas avanzados
- hoy no son la friccion principal del self-hosting real

### Tier 4: no priorizar por ahora

- `init`

Motivo:

- casi no aporta al repo actual ya inicializado
- su valor aparece mas en demos, onboarding o repos nuevos

## Regla de priorizacion recomendada

Cuando haya que elegir que comando frio trabajar primero, usar este orden:

1. comandos que reducen friccion en el flujo diario actual
2. comandos que completan una familia ya muy usada
3. comandos que destraban capacidades estructurales del sistema
4. comandos especializados o de integracion

No elegir por:

- cantidad bruta de comandos no usados
- deseo de subir el porcentaje de cobertura sin valor operativo

## Familias que hoy estan incompletas

### Goals

Hoy:

- `goal list` tiene uso
- `goal add` y `goal show` no

Lectura:

- la familia existe, pero no esta integrada del todo al flujo real

### Evidence

Hoy:

- `evidence add` es el comando mas usado de todo CTX
- `evidence list` y `evidence show` no se usan

Lectura:

- esta es una brecha muy clara
- la escritura esta adoptada, la inspeccion de evidence todavia no

### Conclusions

Hoy:

- `conclusion add` y `conclusion update` se usan
- `conclusion show` no

Lectura:

- la familia de cierre esta viva
- pero la superficie de lectura no entro al loop

### Branching

Hoy:

- `branch`, `checkout`, `merge` no se usan

Lectura:

- la semantica de branches existe en viewer y modelo
- pero el flujo real todavia no pasa por ramas cognitivas como practica cotidiana

### Providers y runs

Hoy:

- `run*` y `provider list` no se usan

Lectura:

- CTX esta operando fuerte como sistema cognitivo estructurado
- pero todavia no como orquestador rutinario de ejecuciones con modelos

## Plan recomendado

### Paso 1

Validar y documentar estas superficies:

- `evidence list`
- `evidence show`
- `goal add`
- `goal show`
- `conclusion show`

### Paso 2

Revisar si `context` y `doctor` deberian entrar al protocolo operativo normal.

### Paso 3

Armar demos o smoke tests controlados para:

- `branch`
- `checkout`
- `merge`
- `export`
- `import`

### Paso 4

ReciÃ©n despuÃ©s empujar:

- `run`
- `provider list`
- `packet list`
- `packet show`

## Regla final

Un comando no usado no es automaticamente deuda critica.

Es deuda prioritaria solo si:

- falta en una familia ya central del flujo real
- resolverlo reduce friccion cotidiana
- o destraba una capacidad estructural que CTX necesita para usarse sobre si mismo

