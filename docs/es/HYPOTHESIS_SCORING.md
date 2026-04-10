# Ponderacion de Hipotesis
Si un modelo de lenguaje y su agente pierden el contexto, esta es la herramienta que necesitas.

## Objetivo

Dar una forma simple y operativa de priorizar hipotesis segun probabilidad, impacto y soporte.

## Problema actual

Hoy CTX guarda `confidence`, pero no alcanza para decidir:

- cual hipotesis conviene validar primero
- cual tiene mas valor potencial
- cual ya esta debilitada por evidencia contradictoria

## Modelo propuesto v1

Cada hipotesis deberia poder evaluarse con cuatro dimensiones:

1. `probability`
   que tan probable parece ser verdadera

2. `impact`
   que tanto cambia el producto si resulta cierta

3. `evidenceStrength`
   cuanta evidencia estructurada la sostiene hoy

4. `costToValidate`
   cuanto cuesta validarla

## Formula simple sugerida

```text
hypothesisScore = (probability * 0.35) + (impact * 0.35) + (evidenceStrength * 0.20) - (costToValidate * 0.10)
```

Escala sugerida:

- `0.0` a `1.0`

## Lectura del score

- `0.80 - 1.00` prioritaria
- `0.60 - 0.79` importante
- `0.40 - 0.59` secundaria
- `< 0.40` baja prioridad o necesita redefinicion

## Como estimar cada componente

### probability

- intuicion experta
- evidencia previa
- repeticion del patron

### impact

- ahorro de tokens
- ahorro de tiempo
- mejora de trazabilidad
- reduccion de retrabajo

### evidenceStrength

- numero de evidencias
- calidad de la fuente
- consistencia entre evidencias

### costToValidate

- tiempo
- complejidad tecnica
- necesidad de usuarios reales

## Siguiente paso de producto

Implementar en CTX:

- nuevos campos de scoring para `Hypothesis`
- recalculo simple desde CLI o core
- visualizacion del score en viewer
- filtros por hipotesis de alta prioridad

