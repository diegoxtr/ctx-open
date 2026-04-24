# Demo paso a paso con CTX

Esta guia esta pensada para ejecutar en vivo durante una charla.

## 1. Mostrar la carpeta comun

Abrir la carpeta:

```powershell
cd C:\sources\ctx-private\examples\ctx\almacen-barrial-reglas
rg --files
```

Explicar:

- todavia no hay repositorio cognitivo
- solo hay una app web simple y documentos de negocio
- CTX todavia no sabe nada de este proyecto

## 2. Crear el repositorio cognitivo

```powershell
C:\ctx\bin\ctx.cmd init --name "Almacen Barrial Reglas Demo"
```

Verificar:

```powershell
C:\ctx\bin\ctx.cmd status
C:\ctx\bin\ctx.cmd graph summary
```

Lectura esperada:

- `dirty: false`
- `nodes: 1`
- solo existe el nodo `Project`

## 3. Generar el primer mapa cognitivo

```powershell
C:\ctx\bin\ctx.cmd bootstrap apply --from .\fuente --mode project --max-files 8
```

El bootstrap se ejecuta sobre `fuente` para que la primera lectura se concentre en el caso de negocio. El codigo se usa despues como evidencia manual.

Verificar:

```powershell
C:\ctx\bin\ctx.cmd status
C:\ctx\bin\ctx.cmd graph summary
C:\ctx\bin\ctx.cmd audit
```

Lectura esperada:

- aparece un `Goal`
- aparece una `Task` de revision
- aparecen hipotesis propuestas
- aparece evidencia tomada de README, reglas de negocio o codigo
- el estado queda `dirty: true` porque todavia falta revision humana

## 4. Abrir la vista

En CTX Viewer:

```text
http://127.0.0.1:5271
```

Cargar esta carpeta:

```text
C:\sources\ctx-private\examples\ctx\almacen-barrial-reglas
```

Mostrar el grafo:

```text
Project -> Goal -> Task -> Hypothesis -> Evidence
```

Explicar que esto no es un commit todavia: es contexto de trabajo.

## 5. Revisar una hipotesis

Elegir una hipotesis detectada por bootstrap, por ejemplo:

```text
Las reglas del almacen son restricciones de negocio, no detalles de UI.
```

Agregar evidencia manual desde el codigo:

```powershell
C:\ctx\bin\ctx.cmd evidence add --title "El codigo implementa reglas de negocio" --summary "src/app.js implementa bloqueos, descuentos, envio gratis y advertencias segun reglas del almacen, por lo que las condiciones del dominio no son solo texto descriptivo." --source "src/app.js evaluarPedido" --kind Validation --supports hypothesis:<hypothesisId>
```

Promover la hipotesis:

```powershell
C:\ctx\bin\ctx.cmd hypo update <hypothesisId> --state Supported --confidence 0.8 --evidence-strength 0.8
```

## 6. Tomar una decision

```powershell
C:\ctx\bin\ctx.cmd decision add --title "Usar el mapa bootstrap como base de explicacion" --rationale "La revision confirma que CTX encontro reglas de negocio reales y que el codigo contiene evidencia para sostenerlas." --state Accepted --hypotheses <hypothesisId> --evidence <evidenceId>
```

## 7. Cerrar una conclusion

```powershell
C:\ctx\bin\ctx.cmd conclusion add --summary "El ejemplo demuestra que CTX puede transformar una carpeta comun en un grafo cognitivo revisable, y que la persona operadora puede convertir inferencias provisorias en conocimiento validado." --state Accepted --decisions <decisionId> --evidence <evidenceId> --goals <goalId> --tasks <taskId>
```

## 8. Crear el commit cognitivo

```powershell
C:\ctx\bin\ctx.cmd closeout
C:\ctx\bin\ctx.cmd commit -m "Mapear reglas de almacen barrial"
C:\ctx\bin\ctx.cmd status
C:\ctx\bin\ctx.cmd audit
```

Explicar:

- el commit de CTX no es un commit de Git
- captura una version durable del razonamiento
- despues de esto la linea aparece en el historial del viewer

## 9. Mensaje para la charla

Frase corta:

> CTX no reemplaza el juicio humano. Genera una primera lectura estructurada, y despues la persona revisa, valida, decide y concluye.
