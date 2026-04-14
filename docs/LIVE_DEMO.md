# GitHub Live Demo

CTX can expose a GitHub-native live demo by combining:

- GitHub Codespaces for the runnable viewer
- a repo-hosted `.ctx` example for the default dataset
- GitHub Pages as a static landing page that points users to the live viewer URL and release downloads

## Why this split exists

GitHub Pages is static hosting. It cannot run the CTX Viewer backend or inspect a `.ctx` repository by itself.

The live part therefore needs a runnable environment. For a GitHub-only delivery model, the best first surface is Codespaces:

- the repo already lives in GitHub
- the viewer can run inside the codespace
- port `5271` can be forwarded publicly
- the default repository can point to a tracked example

## Canonical demo repository

The default live demo repository is:

- `examples/ctx/agent-session-continuity`

It fits the product thesis best because it demonstrates multi-session continuity instead of a one-shot screenshot.

## Codespaces flow

The repository now includes:

- `.devcontainer/devcontainer.json`
- `scripts/start-codespaces-demo.sh`
- `docs/live-demo/index.html`
- `.github/workflows/live-demo-pages.yml`

When the codespace starts:

1. `dotnet restore Ctx.sln` runs once on create
2. `scripts/start-codespaces-demo.sh` starts the viewer on `0.0.0.0:5271`
3. the script sets `CTX_VIEWER_DEFAULT_REPOSITORY_PATH` to `examples/ctx/agent-session-continuity`
4. Codespaces forwards port `5271` publicly

The devcontainer also includes an SSH server feature so the codespace can be operated remotely through GitHub CLI when needed.

## Manual launch inside a codespace

If needed, the demo can be relaunched manually:

```bash
bash scripts/start-codespaces-demo.sh
```

The script writes logs to:

- `/tmp/ctx-viewer-codespaces.log`

## Public landing

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

## Delivery model

- `GitHub Pages` = static landing
- `GitHub Codespaces` = live viewer
- tracked `.ctx` example = demo data

This keeps the first public demo GitHub-native without introducing another hosting provider.
