# Demo En Vivo En GitHub

CTX puede exponer una demo en vivo nativa de GitHub combinando:

- GitHub Codespaces para el viewer ejecutable
- un ejemplo `.ctx` versionado en el repo como dataset por defecto
- GitHub Pages como landing estática que apunte a la URL viva del viewer y a las descargas

## Por qué existe esta separación

GitHub Pages es hosting estático. No puede ejecutar el backend de CTX Viewer ni inspeccionar un repositorio `.ctx` por sí solo.

Por eso la parte viva necesita un entorno ejecutable. Si queremos una entrega puramente GitHub, la mejor primera surface es Codespaces:

- el repo ya vive en GitHub
- el viewer puede correr dentro del codespace
- el puerto `5271` puede exponerse públicamente
- el repositorio por defecto puede apuntar a un example versionado

## Repositorio demo canónico

El repositorio demo por defecto es:

- `examples/ctx/agent-session-continuity`

Es el que mejor encaja con la tesis del producto porque demuestra continuidad entre sesiones, no solo una captura estática.

## Flujo en Codespaces

El repo ahora incluye:

- `.devcontainer/devcontainer.json`
- `scripts/start-codespaces-demo.sh`
- `docs/live-demo/index.html`
- `.github/workflows/live-demo-pages.yml`

Cuando el codespace inicia:

1. `dotnet restore Ctx.sln` corre una vez al crear el entorno
2. `scripts/start-codespaces-demo.sh` levanta el viewer en `0.0.0.0:5271`
3. el script configura `CTX_VIEWER_DEFAULT_REPOSITORY_PATH` con `examples/ctx/agent-session-continuity`
4. Codespaces expone públicamente el puerto `5271`

El devcontainer también incluye un servidor SSH para que el codespace pueda operarse remotamente a través de GitHub CLI cuando haga falta.

## Lanzamiento manual dentro del codespace

Si hace falta relanzar la demo:

```bash
bash scripts/start-codespaces-demo.sh
```

El script escribe logs en:

- `/tmp/ctx-viewer-codespaces.log`

## Landing pública

GitHub Pages debe seguir siendo estático.

Su función es:

- explicar brevemente la tesis
- enlazar a la URL viva de la demo
- enlazar a las descargas de releases
- mostrar una o dos capturas

Pages no debe intentar hostear el viewer directamente.

El repositorio ahora incluye una landing estática en:

- `docs/live-demo/index.html`

y un workflow de despliegue en:

- `.github/workflows/live-demo-pages.yml`

El artefacto estático se arma desde:

- `docs/live-demo/*`
- `assets/screenshots/*`

## Modelo de entrega

- `GitHub Pages` = landing estática
- `GitHub Codespaces` = viewer en vivo
- example `.ctx` versionado = datos de demo

Así la primera demo pública queda 100% GitHub sin meter otro proveedor todavía.
