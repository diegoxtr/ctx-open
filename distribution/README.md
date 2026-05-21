# CTX Distribution Assets

This directory is the canonical packaging root for CTX distributions.

It contains:

- cross-platform target metadata
- release/version metadata for user-facing bootstrap
- install layout metadata for source and portable bootstrap flows
- the agent-link prompt fragment that must ship with every build
- platform-specific installer scaffolding

## Layout

- `targets.json`
  Cross-platform distribution targets with runtime IDs, package formats, and packaging expectations.

- `version-manifest.json`
  Release/version metadata plus default asset paths used by the public bootstrap scripts for install/update/repair.

- `install-manifest.json`
  Shared install-root, layout, and helper-prompt metadata used by the source/portable bootstrap scripts.

- `packaged-docs.txt`
  Canonical list of documentation files that must ship in portable bundles and installed roots.

- `packaged-prompts.txt`
  Canonical list of prompt files that must ship in portable bundles and installed roots.

- `agent-link/CTX_AGENT_LINK_PROMPT.txt`
  Prompt fragment that binds agents to CTX as the system of record.

- `windows/ctx.iss`
  Inno Setup scaffold for a Windows EXE installer.

- `macos/package-macos.sh`
  Packaging scaffold for macOS portable and signed installer output.

- `linux/package-linux.sh`
  Packaging scaffold for Linux tarball, deb, and rpm output.

## Build Flow

Portable bundles are produced by:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build-distribution.ps1
```

The script publishes CTX for each target in `targets.json`, copies the agent-link prompt, packaged documentation, and packaged prompts into the bundle, and emits archives under `artifacts/distribution/`.

Current portable bundle layout:

```text
bin/
mcp/
acp/
viewer/
prompts/
docs/
distribution/
```

The `docs/` and `prompts/` payloads are driven by `distribution/packaged-docs.txt` and `distribution/packaged-prompts.txt`. Release validation must fail if a console-referenced document or prompt is missing from those manifests, the portable bundle, or the installed root.

The install scripts turn that bundle into an operational install root with `ctx`, `ctx-mcp`, `ctx-agent-acp`, and optional `ctx-viewer` launchers.

Install bootstrap flows are provided by:

- `install.ps1`
- `install.sh`
- `scripts/install-ctx.ps1`
- `scripts/install-ctx.sh`

Those scripts support:

- single-entry `install/update/repair` bootstrap for copy-paste console usage
- `source` mode: clone or reuse a repo checkout, then publish into an operational install root
- `portable` mode: unpack a prebuilt distribution bundle into an operational install root

Versioned artifact policy:

- keep manifests, prompts, and installer scaffolding in `distribution/`
- treat expanded bundle directories in `artifacts/distribution/` as build byproducts
- if shipped archives are committed, track them through Git LFS

Platform-native installers are scaffolded here but may require external tooling:

- Windows: Inno Setup
- macOS: codesign, pkgbuild, productbuild
- Linux: fpm or native packaging tools
