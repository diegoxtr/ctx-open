# Prompt para ejecutar la demo paso a paso

Usa CTX para todo. Trabaja solo dentro de esta carpeta:

```text
examples\ctx\almacen-barrial-reglas
```

Quiero que hagas la demo paso a paso para ver como se forma el grafo en CTX Viewer.

No hagas todo de una.

Primero verifica que no exista `.ctx`. Si existe, avisame y no sigas.
Luego ejecuta:

```powershell
C:\ctx\bin\ctx.cmd init --name "Almacen Barrial Reglas Demo"
```

Detenete y decime que deberia verse en el viewer.

Despues, cuando te diga "siguiente", corre:

```powershell
C:\ctx\bin\ctx.cmd bootstrap apply --from .\fuente --mode project --max-files 8
```

y volve a detenerte.

Despues revisamos las hipotesis que salieron. Quiero que confirmes si hablan del conocimiento del almacen, por ejemplo margen, fiado, energizantes, lluvia, logistica, envio o advertencias. Si hablan de CTX o de la demo, marca eso como problema.

Despues agregamos evidencia desde `src/app.js`, tomamos una decision, cerramos una conclusion y recien al final hacemos `ctx commit`.
