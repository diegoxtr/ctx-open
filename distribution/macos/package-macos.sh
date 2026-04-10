#!/usr/bin/env bash
set -euo pipefail

# Scaffold for macOS portable and installer packaging.
# Expected input:
#   artifacts/distribution/osx-<arch>/bundle
#
# Expected tools for native installer closeout:
#   codesign
#   pkgbuild
#   productbuild
#
# Portable closeout can use:
#   tar -czf ctx-osx-<arch>.tar.gz bundle

echo "Use scripts/build-distribution.ps1 to publish the portable payload first."
echo "Then package bundle/ into tar.gz and optionally sign/build pkg or dmg."
