# CTX Installer and Distribution Plan
If a language model and its agent lose context, this is the tool you need.

## Objective

Provide a cross-platform installer and a portable distribution strategy for CTX across:

- Windows (x64, x86)
- macOS (Apple Silicon, Intel)
- Linux (x64, ARM64)

Include a standard prompt section that binds agents to the cognitive versioner workflow.

## Target Platforms and Architectures

### Windows

- Installer: signed executable (MSI or EXE).
- Portable: zip with `ctx.exe` and supporting files.
- Architectures: x64, x86 (and optionally ARM64 if runtime supports it).

### macOS

- Installer: signed `.pkg` or `.dmg`.
- Portable: tarball with `ctx` binary.
- Architectures: Apple Silicon and Intel.

### Linux

- Installer: package formats (deb, rpm) plus tarball.
- Portable: tarball with `ctx`.
- Architectures: x64 and ARM64.

## Packaging Strategy (Baseline)

1. Build self-contained binaries per target using `dotnet publish`.
2. Produce a portable archive per OS/arch.
3. Wrap a platform-native installer on top of the portable payload.

## Canonical Directory

The packaging root is:

- `distribution/`

Concrete assets live in:

- `distribution/targets.json`
- `distribution/version-manifest.json`
- `distribution/install-manifest.json`
- `distribution/agent-link/CTX_AGENT_LINK_PROMPT.txt`
- `distribution/windows/ctx.iss`
- `distribution/macos/package-macos.sh`
- `distribution/linux/package-linux.sh`
- `scripts/build-distribution.ps1`
- `scripts/install-ctx.ps1`
- `scripts/install-ctx.sh`
- `install.ps1`
- `install.sh`

## Agent-Link Prompt (Required)

Every distribution should ship a short prompt fragment that is used by agents when operating CTX:

```
CTX is the system of record. Read CTX first, follow ctx next, and record evidence/decisions/conclusions before committing code.
Do not create work outside CTX without adding a task and hypothesis.
If you start planning from chat instead of CTX, stop, inspect CTX again, and continue from the repository state.
```

This fragment should be placed in:

- the installer output directory (so operators can copy it),
- and the base prompt template (`prompts/CTX_BASE_PROMPT.md`).

Operational note:

- some models need this fragment repeated more than once before they stop treating chat as the planning surface
- distributors should treat that repetition as a required bootstrap step, not as optional guidance

## Update Flow

Baseline update flow should support:

- in-place update (installer-driven), and
- manual replacement for portable installs.

## Install Bootstrap Modes

The install bootstrap should support two operational modes:

1. `source`
Build from a cloned or already-checked-out repository, then install CTX into an operational root with launchers and helper prompt assets.

2. `portable`
Unpack a prebuilt distribution bundle, then install CTX into an operational root with launchers and helper prompt assets.

The install scripts should use a shared manifest so install roots, helper prompt locations, and launcher layout stay aligned across platforms.

## User Entry Point

User-facing installation should prefer a single copy-paste entry script per shell:

- Windows: `install.ps1`
- Linux/macOS: `install.sh`

Those entrypoints should:

- detect whether CTX is absent, outdated, or already current
- choose `install`, `update`, or `repair`
- resolve the latest published version and matching asset from GitHub Releases
- use `distribution/version-manifest.json` only as repository/API configuration plus asset-name mapping
- delegate the actual filesystem/bootstrap work to `scripts/install-ctx.ps1` or `scripts/install-ctx.sh`
- expose `ctx` globally when possible
  Windows: `User` or `Machine` PATH
  Linux/macOS: symlink in `~/.local/bin` or `/usr/local/bin`

## Branch And Release Strategy

Installer-driven distribution should follow a strict published-artifact strategy:

- `main`
  Primary integration branch for daily development.
  It may contain unreleased work and must not be treated as an install/update source by the public bootstrap.

- `release/x.y.z`
  Short-lived stabilization branch for a specific release candidate.
  Use it to freeze scope, harden packaging, verify installer behavior, and prepare the final release assets.

- `vX.Y.Z` tags
  Immutable release tags.
  GitHub Releases should be created from these tags, and the installer should treat the published tag as the canonical version string.

- GitHub Releases
  Source of truth for public install/update decisions.
  The bootstrap should consume only published releases and matching assets, never raw `main` commits or unreleased branch artifacts.

Operational rule:

- if work exists only in `main`, it is not installable through the public bootstrap yet
- if work is cut into `release/x.y.z` but not released, it is still not a public update target
- once `vX.Y.Z` is tagged and its GitHub Release is published, the installer may detect and consume it

Recommended flow:

1. Merge ongoing work into `main`.
2. Cut `release/x.y.z` when preparing publication.
3. Validate installer/distribution assets on the release branch.
4. Tag `vX.Y.Z`.
5. Publish GitHub Release assets from that tag.
6. Let the bootstrap detect the new published version through GitHub Releases.

## Concrete Toolchain

- Portable payloads: `dotnet publish` per RID via `scripts/build-distribution.ps1`
- Windows EXE installer scaffold: Inno Setup via `distribution/windows/ctx.iss`
- macOS installer scaffold: `pkgbuild` and `productbuild` via `distribution/macos/package-macos.sh`
- Linux package scaffold: tarball plus optional `deb`/`rpm` closeout via `distribution/linux/package-linux.sh`

## Versioned Artifact Policy

- `distribution/` is the versioned source of truth for packaging manifests, prompts, and installer scaffolding.
- `artifacts/distribution/` is the generated output location for portable archives.
- Expanded bundle directories under `artifacts/distribution/` are build byproducts and should stay out of versioned state.
- If portable archives are intentionally versioned, they should be tracked through Git LFS rather than regular Git blobs.

## Verification Checklist

- Each build runs `ctx version` successfully.
- The binary launches on each OS/arch.
- The prompt fragment is shipped alongside the binary.
- Portable archives are emitted under `artifacts/distribution/`.

## Remaining Open Questions

- Signing certificates and CI signing flow.
- Whether viewer should always ship in the default installer payload.
- Whether Linux native packages should be built with `fpm` or distro-specific pipelines.

## Next Step

Implement the manifest-driven install scripts, then validate `source` and `portable` installation flows on Windows first and extend the same layout to Linux and macOS.

