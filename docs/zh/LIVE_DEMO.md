# GitHub Live Demo

CTX can provide a GitHub-native live demo by combining:

- GitHub Codespaces for the runnable viewer
- a tracked `.ctx` example in the repository as the default dataset
- GitHub Pages as a static landing page that points to the live viewer URL and release downloads

Public demo entrypoints:

- Landing: `https://diegoxtr.github.io/ctx-open/`
- Demo notes: `https://diegoxtr.github.io/ctx-open/notes.html`
- Codespaces quickstart: `https://codespaces.new/diegoxtr/ctx-open?quickstart=1`

Demo repositories you can copy and paste:

- Codespaces default: `/workspaces/ctx-open/examples/ctx/agent-session-continuity`
- Codespaces alternate: `/workspaces/ctx-open/examples/ctx/catalog-cache-branch-merge`
- Codespaces alternate: `/workspaces/ctx-open/examples/ctx/critical-checkout-regression`

## Why This Split Exists

GitHub Pages is static hosting. It cannot run the CTX Viewer backend or inspect a `.ctx` repository by itself.

The live part therefore needs a runnable environment. For a GitHub-only delivery model, the best first surface is Codespaces:

- the repository already lives in GitHub
- the viewer can run inside the codespace
- port `5271` can be forwarded publicly
- the default repository path can point to a tracked example

## Canonical Demo Repository

The default live demo repository is:

- `examples/ctx/agent-session-continuity`

It fits the product thesis best because it demonstrates continuity across sessions instead of a one-shot screenshot.

## Codespaces Flow

The repository now includes:

- `.devcontainer/devcontainer.json`
- `scripts/ensure-dotnet-sdk.sh`
- `scripts/start-codespaces-demo.sh`
- `docs/live-demo/index.html`
- `.github/workflows/live-demo-pages.yml`

When the codespace starts:

1. `scripts/ensure-dotnet-sdk.sh` installs the SDK pinned in `global.json` when the base image does not already provide it.
2. `dotnet restore Ctx.sln` runs once when the environment is created.
3. `scripts/start-codespaces-demo.sh` starts the viewer on `0.0.0.0:5271`.
4. The script sets `CTX_VIEWER_DEFAULT_REPOSITORY_PATH` to `examples/ctx/agent-session-continuity`.
5. Codespaces forwards port `5271` publicly.

The live demo no longer depends on an SSH server feature inside the container. The priority is to keep Codespaces creation reliable for browser-first usage instead of adding extra provisioning risk.

## Manual Launch Inside A Codespace

If the demo needs to be restarted:

```bash
bash scripts/start-codespaces-demo.sh
```

The script writes logs to:

- `/tmp/ctx-viewer-codespaces.log`

If the viewer does not open, verify the local process inside the codespace first:

```bash
curl -I http://127.0.0.1:5271
cat /tmp/ctx-viewer-codespaces.log
```

## Public Landing

GitHub Pages should remain static.

Its job is:

- explain the thesis briefly
- link to the live demo URL
- link to release downloads
- show one or two screenshots

Pages should not try to host the viewer directly.

The repository now includes a static landing page at:

- `docs/live-demo/index.html`

and a deployment workflow at:

- `.github/workflows/live-demo-pages.yml`

The static artifact is built from:

- `docs/live-demo/*`
- `assets/screenshots/*`

## Delivery Model

- `GitHub Pages` = static landing
- `GitHub Codespaces` = live viewer
- tracked `.ctx` example = demo data

This keeps the first public demo GitHub-native without introducing another hosting provider yet.
