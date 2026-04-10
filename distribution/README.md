# CTX Distribution Assets

This directory is the canonical packaging root for CTX distributions.

It contains:

- cross-platform target metadata
- the agent-link prompt fragment that must ship with every build
- platform-specific installer scaffolding

## Layout

- `targets.json`
  Cross-platform distribution targets with runtime IDs, package formats, and packaging expectations.

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

The script publishes CTX for each target in `targets.json`, copies the agent-link prompt into the bundle, and emits archives under `artifacts/distribution/`.

Versioned artifact policy:

- keep manifests, prompts, and installer scaffolding in `distribution/`
- treat expanded bundle directories in `artifacts/distribution/` as build byproducts
- if shipped archives are committed, track them through Git LFS

Platform-native installers are scaffolded here but may require external tooling:

- Windows: Inno Setup
- macOS: codesign, pkgbuild, productbuild
- Linux: fpm or native packaging tools
