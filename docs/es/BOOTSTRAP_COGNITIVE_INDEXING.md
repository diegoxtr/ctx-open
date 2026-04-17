# Bootstrap de Indexacion Cognitiva

CTX no debe tratar el bootstrap como una extraccion plana de entidades.

El objetivo del bootstrap es reconstruir un hilo cognitivo provisional desde material existente.

Debe ayudar a responder:

- que problema parece estar intentando resolver el material
- que hipotesis parecen sostenerlo
- que evidencia funciona como soporte
- que incertidumbres siguen abiertas

## Superficies iniciales

Comando de inspeccion:

```powershell
ctx bootstrap map --from <path> [--mode auto|article|project] [--max-files <n>]
```

Comando de promocion conservadora:

```powershell
ctx bootstrap apply --from <path> [--mode auto|article|project] [--max-files <n>] [--parent-goal <goalId>]
```

## Regla de diseno

Bootstrap debe preservar la idea, no solo la entidad.

Mal bootstrap:

- `Goal exists`
- `Task exists`
- `Hypothesis exists`

Buen bootstrap:

- `parece que el material intenta resolver X`
- `parece apoyarse en la hipotesis Y`
- `estos extractos funcionan como evidencia para Y`
- `estas preguntas siguen abiertas`

## Regla de persistencia

`ctx bootstrap map` no debe escribir CTX durable.

Debe devolver un mapa provisional y revisable.

`ctx bootstrap apply` existe para abrir una linea provisional sin fingir que el bootstrap ya entendio toda la verdad del material.

Debe:

- seleccionar solo el hilo candidato mas fuerte
- abrir un goal provisional
- crear una task de revision
- promover una cantidad acotada de hypotheses y evidence
- dejar todo marcado como bootstrap/provisional

No debe:

- importar todo el arbol fuente a CTX
- aceptar decisions automaticamente
- cerrar la linea como si ya estuviera validada

El seguimiento correcto despues de `apply` es revisar, validar, rechazar o fortalecer esa linea antes de cualquier decision aceptada o conclusion de cierre.

