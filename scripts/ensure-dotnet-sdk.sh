#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
INSTALL_DIR="${DOTNET_INSTALL_DIR:-$HOME/.dotnet}"
REQUIRED_SDK="$(python3 - "$ROOT_DIR/global.json" <<'PY'
import json, sys
with open(sys.argv[1], "r", encoding="utf-8") as handle:
    payload = json.load(handle)
print(payload["sdk"]["version"])
PY
)"

export DOTNET_ROOT="$INSTALL_DIR"
export PATH="$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH"

if dotnet --list-sdks 2>/dev/null | grep -q "^${REQUIRED_SDK} "; then
  exit 0
fi

TMP_SCRIPT="$(mktemp)"
trap 'rm -f "$TMP_SCRIPT"' EXIT

curl -fsSL https://dot.net/v1/dotnet-install.sh -o "$TMP_SCRIPT"
bash "$TMP_SCRIPT" --install-dir "$INSTALL_DIR" --version "$REQUIRED_SDK"

PROFILE_SNIPPET='export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH"'

if [[ -f "$HOME/.bashrc" ]]; then
  if ! grep -Fq 'export DOTNET_ROOT="$HOME/.dotnet"' "$HOME/.bashrc"; then
    printf '\n%s\n' "$PROFILE_SNIPPET" >> "$HOME/.bashrc"
  fi
else
  printf '%s\n' "$PROFILE_SNIPPET" > "$HOME/.bashrc"
fi
