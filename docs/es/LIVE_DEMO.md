# Demo En Vivo En GitHub

CTX puede exponer una demo en vivo nativa de GitHub combinando:

- GitHub Codespaces para el viewer ejecutable
- un ejemplo `.ctx` versionado en el repo como dataset por defecto
- GitHub Pages como landing estatica que apunte a la URL viva del viewer y a las descargas

Puntos de entrada publicos del demo:

- Landing: `https://diegoxtr.github.io/ctx-open/`
- Notas del demo: `https://diegoxtr.github.io/ctx-open/notes.html`
- Quickstart de Codespaces: `https://codespaces.new/diegoxtr/ctx-open?quickstart=1`
- Sesion live actual (temporal): `https://fuzzy-space-sniffle-59rjpw4x7w37r55-5271.app.github.dev/`

Usa la URL de la sesion live actual cuando quieras entrar directo al viewer publico en ejecucion. Si la sesion expira, vuelve a la landing o al quickstart de Codespaces.

Repositorios demo para copiar y pegar:

- Codespaces por defecto: `/workspaces/ctx-open/examples/ctx/agent-session-continuity`
- Codespaces alternativo: `/workspaces/ctx-open/examples/ctx/catalog-cache-branch-merge`
- Codespaces alternativo: `/workspaces/ctx-open/examples/ctx/critical-checkout-regression`

Que deberia mostrar cada demo:

- `agent-session-continuity`: `Working` debe mostrar una task de auditoria de handoff en `Ready`, `Origin` debe mostrar el trigger que dejo abierta esa linea, y `Playbook` debe mostrar `Session continuity demo validation`.
- `catalog-cache-branch-merge`: `Working` debe mostrar una checklist de revision de cache en `Ready`, `Origin` debe mostrar el trigger que dejo visible esa linea, y `Playbook` debe mostrar `Cache strategy demo validation`.
- `critical-checkout-regression`: `Working` debe mostrar una task de monitoreo post-fix en `Ready`, `Origin` debe mostrar el trigger que mantuvo visible ese seguimiento, y `Playbook` debe mostrar `Checkout regression demo validation`.

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

La demo en vivo ya no depende de una feature de SSH dentro del contenedor. La prioridad es que Codespaces cree el entorno de forma confiable para uso en navegador, sin agregar riesgo extra de provisioning.

## Lanzamiento Manual Dentro Del Codespace

Si hace falta relanzar la demo:

```bash
bash scripts/start-codespaces-demo.sh
```

Si un codespace reanudado o parcialmente inicializado todavia no levanta el viewer, ejecuta este bloque completo de recuperacion:

```bash
git pull --ff-only origin main
bash scripts/ensure-dotnet-sdk.sh
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$DOTNET_ROOT:$DOTNET_ROOT/tools:/usr/share/dotnet:$PATH"
dotnet restore Ctx.sln
bash scripts/start-codespaces-demo.sh
curl -I http://127.0.0.1:5271
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
