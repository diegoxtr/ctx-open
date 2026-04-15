# Demo En Vivo En GitHub

CTX puede exponer una demo en vivo nativa de GitHub combinando:

- GitHub Codespaces para el viewer ejecutable
- un ejemplo `.ctx` versionado en el repo como dataset por defecto
- GitHub Pages como landing estatica que apunte a la URL viva del viewer y a las descargas

## Por Que Existe Esta Separacion

GitHub Pages es hosting estatico. No puede ejecutar el backend de CTX Viewer ni inspeccionar un repositorio `.ctx` por si solo.

Por eso la parte viva necesita un entorno ejecutable. Si queremos una entrega puramente GitHub, la mejor primera surface es Codespaces:

- el repo ya vive en GitHub
- el viewer puede correr dentro del codespace
- el puerto `5271` puede exponerse publicamente
- el repositorio por defecto puede apuntar a un example versionado

## Repositorio Demo Canonico

El repositorio demo por defecto es:

- `examples/ctx/agent-session-continuity`

Es el que mejor encaja con la tesis del producto porque demuestra continuidad entre sesiones, no solo una captura estatica.

## Flujo En Codespaces

El repo ahora incluye:

- `.devcontainer/devcontainer.json`
- `scripts/ensure-dotnet-sdk.sh`
- `scripts/start-codespaces-demo.sh`
- `docs/live-demo/index.html`
- `.github/workflows/live-demo-pages.yml`

Cuando el codespace inicia:

1. `scripts/ensure-dotnet-sdk.sh` instala el SDK fijado en `global.json` cuando la imagen base todavia no lo trae.
2. `dotnet restore Ctx.sln` corre una vez al crear el entorno.
3. `scripts/start-codespaces-demo.sh` levanta el viewer en `0.0.0.0:5271`.
4. El script configura `CTX_VIEWER_DEFAULT_REPOSITORY_PATH` con `examples/ctx/agent-session-continuity`.
5. Codespaces expone publicamente el puerto `5271`.

El devcontainer tambien incluye soporte SSH para que el codespace pueda operarse remotamente a traves de GitHub CLI cuando haga falta.

## Lanzamiento Manual Dentro Del Codespace

Si hace falta relanzar la demo:

```bash
bash scripts/start-codespaces-demo.sh
```

El script escribe logs en:

- `/tmp/ctx-viewer-codespaces.log`

Si el viewer no abre, primero valida el proceso local dentro del codespace:

```bash
curl -I http://127.0.0.1:5271
cat /tmp/ctx-viewer-codespaces.log
```

## Landing Publica

GitHub Pages debe seguir siendo estatico.

Su funcion es:

- explicar brevemente la tesis
- enlazar a la URL viva de la demo
- enlazar a las descargas de releases
- mostrar una o dos capturas

Pages no debe intentar hostear el viewer directamente.

El repositorio ahora incluye una landing estatica en:

- `docs/live-demo/index.html`

y un workflow de despliegue en:

- `.github/workflows/live-demo-pages.yml`

El artefacto estatico se arma desde:

- `docs/live-demo/*`
- `assets/screenshots/*`

## Modelo De Entrega

- `GitHub Pages` = landing estatica
- `GitHub Codespaces` = viewer en vivo
- example `.ctx` versionado = datos de demo

Asi la primera demo publica queda 100% GitHub sin meter otro proveedor todavia.
