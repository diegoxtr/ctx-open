# GitHub Live Demo

CTX can expose a GitHub-native live demo by combining:

- GitHub Codespaces for the runnable viewer
- a repo-hosted `.ctx` example for the default dataset
- GitHub Pages as a static landing page that points users to the live viewer URL and release downloads

Public demo entrypoints:

- Landing: `https://diegoxtr.github.io/ctx-open/`
- Demo notes: `https://diegoxtr.github.io/ctx-open/notes.html`
- Codespaces quickstart: `https://codespaces.new/diegoxtr/ctx-open?quickstart=1`
- Current live session (temporary): `https://fuzzy-space-sniffle-59rjpw4x7w37r55-5271.app.github.dev/`

Use the current live session link when you want to open the running public viewer directly. If that session expires, return to the landing page or the Codespaces quickstart link.

Demo repositories you can copy and paste:

- Codespaces default: `/workspaces/ctx-open/examples/ctx/agent-session-continuity`
- Codespaces alternate: `/workspaces/ctx-open/examples/ctx/catalog-cache-branch-merge`
- Codespaces alternate: `/workspaces/ctx-open/examples/ctx/critical-checkout-regression`

Expected viewer surfaces:

- `agent-session-continuity`: `Working` should show the ready handoff audit task, `Origin` should show the trigger that kept the handoff line open, and `Playbook` should show `Session continuity demo validation`.
- `catalog-cache-branch-merge`: `Working` should show the ready cache review checklist, `Origin` should show the trigger that kept the review line open, and `Playbook` should show `Cache strategy demo validation`.
- `critical-checkout-regression`: `Working` should show the ready post-fix monitoring task, `Origin` should show the trigger that kept monitoring visible, and `Playbook` should show `Checkout regression demo validation`.

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

The recommended public entrypoint is a `codespaces.new` quickstart link instead of the repository Codespaces tab, because the quickstart URL opens or resumes the codespace directly in the web client.

When the codespace starts:

1. `scripts/ensure-dotnet-sdk.sh` installs the SDK pinned in `global.json` when the base image does not already have it
2. `dotnet restore Ctx.sln` runs once on create
3. `scripts/start-codespaces-demo.sh` starts the viewer on `0.0.0.0:5271`
4. the script sets `CTX_VIEWER_DEFAULT_REPOSITORY_PATH` to `examples/ctx/agent-session-continuity`
5. Codespaces forwards port `5271` publicly

The live demo no longer depends on an SSH server feature inside the container. The goal is to keep Codespaces creation reliable for browser-first usage instead of adding extra provisioning risk.

## Manual launch inside a codespace

If needed, the demo can be relaunched manually:

```bash
bash scripts/start-codespaces-demo.sh
```

If a resumed or partially initialized codespace still does not start the viewer, run the full recovery block:

```bash
git pull --ff-only origin main
bash scripts/ensure-dotnet-sdk.sh
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$DOTNET_ROOT:$DOTNET_ROOT/tools:/usr/share/dotnet:$PATH"
dotnet restore Ctx.sln
bash scripts/start-codespaces-demo.sh
curl -I http://127.0.0.1:5271
```

The script writes logs to:

- `/tmp/ctx-viewer-codespaces.log`

If the viewer does not open, verify the local process inside the codespace first:

```bash
curl -I http://127.0.0.1:5271
cat /tmp/ctx-viewer-codespaces.log
```

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
