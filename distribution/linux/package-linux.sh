#!/usr/bin/env bash
set -euo pipefail

# Scaffold for Linux portable and native package closeout.
# Expected input:
#   artifacts/distribution/linux-<arch>/bundle
#
# Portable closeout:
#   tar -czf ctx-linux-<arch>.tar.gz bundle
#
# Native package options:
#   fpm for deb/rpm
#   or distro-native build pipelines

echo "Use scripts/build-distribution.ps1 to publish the portable payload first."
echo "Then package bundle/ into tar.gz and optionally build deb/rpm artifacts."
