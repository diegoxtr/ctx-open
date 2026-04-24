# Almacen barrial con reglas raras

Este ejemplo esta pensado para mostrar CTX en una charla.

La idea es partir de una carpeta comun, con HTML, CSS, JavaScript y notas de negocio, y convertirla en un repositorio cognitivo que se pueda inspeccionar en CTX Viewer.

El caso simula un almacen barrial que tiene reglas de venta poco obvias:

- la yerba con descuento no se vende despues de las 18:00
- el fiado solo se permite si el cliente no compro bebidas energizantes
- la promo de pan y queso se cancela cuando llueve
- el envio gratis requiere cupon capicua y ticket mayor a 5000
- el pedido con hielo y velas no se bloquea, pero debe mostrar advertencia logistica

El punto de la demo no es la app. El punto es mostrar que CTX puede leer archivos normales de negocio y proponer una primera linea cognitiva:

`Proyecto -> Objetivo -> Tarea -> Hipotesis -> Evidencia`

Luego la persona que opera CTX revisa esa linea, agrega evidencia manual desde el codigo, acepta o rechaza hipotesis, toma una decision y cierra una conclusion.

## Archivos

- `index.html`: interfaz minima del almacen
- `src/app.js`: reglas de negocio implementadas
- `src/styles.css`: estilos simples
- `fuente/reglas-negocio.md`: contexto de negocio escrito como producto
- `fuente/notas-del-duenio.md`: notas informales que tambien deben alimentar el mapa
- `docs/demo-paso-a-paso.md`: guion de comandos para la charla
- `docs/resultado-esperado.md`: que deberia verse en CTX Viewer

## Uso rapido

Abrir `index.html` en un navegador.

No requiere build, npm ni servidor.

## Uso con CTX

Desde esta carpeta:

```powershell
C:\ctx\bin\ctx.cmd init --name "Almacen Barrial Reglas Demo"
C:\ctx\bin\ctx.cmd bootstrap apply --from .\fuente --mode project --max-files 8
C:\ctx\bin\ctx.cmd status
C:\ctx\bin\ctx.cmd graph summary
C:\ctx\bin\ctx.cmd audit
```

Despues abrir CTX Viewer en:

```text
http://127.0.0.1:5271
```

y cargar la carpeta de este ejemplo.

La guia de comandos queda fuera del bootstrap inicial para no contaminar las hipotesis con instrucciones operativas. Primero se mapea la carpeta `fuente`, que contiene solo conocimiento del almacen; despues se agrega evidencia desde el codigo.
