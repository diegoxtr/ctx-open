# Resultado esperado en CTX Viewer

## Antes de `bootstrap apply`

El grafo debe mostrar solo:

```text
Project
```

Resumen esperado:

- nodos: 1
- edges: 0
- timeline: 0
- head: working o vacio

## Despues de `bootstrap apply`

El grafo deberia mostrar una linea parecida a:

```text
Project -> Goal -> Task -> Hypothesis -> Evidence
```

La cantidad exacta puede variar si cambia el algoritmo de bootstrap, pero deberian aparecer:

- una tarea de revision
- hipotesis propuestas sobre reglas del almacen
- evidencia documental tomada de `fuente/reglas-negocio.md` y `fuente/notas-del-duenio.md`

## Despues de la revision

El grafo deberia crecer hacia:

```text
Project -> Goal -> Task -> Hypothesis -> Evidence -> Decision -> Conclusion
```

La diferencia importante es semantica:

- `bootstrap apply` propone
- la persona revisa
- la evidencia valida
- la decision acepta una direccion
- la conclusion cierra el aprendizaje

Si las hipotesis hablan de CTX, de la guia o de la mecanica de la demo, el material fuente esta contaminado con metatexto. En ese caso, correr bootstrap solo sobre `fuente` y revisar que `fuente` describa el negocio, no la herramienta.

## Despues del commit CTX

En el panel izquierdo deberia aparecer un commit en `main`.

El estado esperado:

- `dirty: false`
- `timeline: 1`
- `head`: id del commit CTX
- `audit`: sin errores

## Filtros que pueden ocultar el grafo

Si no se ve nada, revisar los filtros de estado de tareas.

Si la tarea quedo en `Done` y el filtro `Done` esta desmarcado, el viewer puede mostrar:

```text
No graph nodes match the selected task states.
```

Para una charla conviene mantener visible `Ready` o activar tambien `Done`.
