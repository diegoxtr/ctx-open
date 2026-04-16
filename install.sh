#!/usr/bin/env bash
set -euo pipefail

ACTION="${ACTION:-auto}"
MODE="${MODE:-auto}"
INSTALL_ROOT="${INSTALL_ROOT:-}"
MANIFEST_PATH="${MANIFEST_PATH:-}"
BUNDLE_METADATA_PATH="${BUNDLE_METADATA_PATH:-}"
BUNDLE_PATH="${BUNDLE_PATH:-}"
SOURCE_REPO_PATH="${SOURCE_REPO_PATH:-}"
REPO_URL="${REPO_URL:-}"
LINK_SCOPE="${LINK_SCOPE:-auto}"
SKIP_VIEWER="${SKIP_VIEWER:-0}"

SCRIPT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if [[ -z "$MANIFEST_PATH" ]]; then
  MANIFEST_PATH="$SCRIPT_ROOT/distribution/version-manifest.json"
fi

if [[ ! -f "$MANIFEST_PATH" ]]; then
  echo "Version manifest not found: $MANIFEST_PATH" >&2
  exit 1
fi

if [[ -z "$INSTALL_ROOT" ]]; then
  INSTALL_ROOT="$HOME/.local/share/ctx"
fi

MANIFEST_VERSION="$(python - <<'PY' "$MANIFEST_PATH"
import json,sys
with open(sys.argv[1], 'r', encoding='utf-8') as f:
    print(json.load(f)['repo']['latestReleaseApi'])
PY
)"

fetch_release_metadata() {
  if [[ -n "$BUNDLE_METADATA_PATH" ]]; then
    cat "$BUNDLE_METADATA_PATH"
    return
  fi

  curl -fsSL \
    -H "Accept: application/vnd.github+json" \
    -H "User-Agent: ctx-installer" \
    "$MANIFEST_VERSION"
}

RELEASE_JSON="$(fetch_release_metadata)"
RELEASE_VERSION="$(python - <<'PY' "$RELEASE_JSON"
import json,sys
payload=json.loads(sys.argv[1])
print(payload.get('tag_name','').lstrip('vV'))
PY
)"

if [[ -z "$REPO_URL" ]]; then
  REPO_URL="$(python - <<'PY' "$MANIFEST_PATH"
import json,sys
with open(sys.argv[1], 'r', encoding='utf-8') as f:
    print(json.load(f)['repo']['publicCloneUrl'])
PY
)"
fi

INSTALL_METADATA="$INSTALL_ROOT/ctx-install.json"
INSTALLED_VERSION=""
if [[ -f "$INSTALL_METADATA" ]]; then
  INSTALLED_VERSION="$(python - <<'PY' "$INSTALL_METADATA"
import json,sys
with open(sys.argv[1], 'r', encoding='utf-8') as f:
    print(json.load(f).get('version',''))
PY
)"
fi

if [[ "$ACTION" == "auto" ]]; then
  if [[ ! -f "$INSTALL_METADATA" ]]; then
    ACTION="install"
  elif [[ "$INSTALLED_VERSION" != "$RELEASE_VERSION" ]]; then
    ACTION="update"
  else
    ACTION="repair"
  fi
fi

if [[ "$MODE" == "auto" ]]; then
  if [[ -n "$BUNDLE_PATH" ]]; then
    MODE="portable"
  else
    MODE="source"
  fi
fi

if [[ "$MODE" == "portable" && -z "$BUNDLE_PATH" ]]; then
  OS_NAME="$(uname -s)"
  ARCH_NAME="$(uname -m)"

  case "$OS_NAME" in
    Linux) OS_KEY="linux" ;;
    Darwin) OS_KEY="osx" ;;
    *) echo "Unsupported OS: $OS_NAME" >&2; exit 1 ;;
  esac

  case "$ARCH_NAME" in
    x86_64|amd64) ARCH_KEY="x64" ;;
    arm64|aarch64) ARCH_KEY="arm64" ;;
    *) echo "Unsupported architecture: $ARCH_NAME" >&2; exit 1 ;;
  esac

  ASSET_KEY="${OS_KEY}-${ARCH_KEY}"

  ASSET_NAME="$(python - <<'PY' "$MANIFEST_PATH" "$ASSET_KEY"
import json,sys
with open(sys.argv[1], 'r', encoding='utf-8') as f:
    print(json.load(f)['assets'][sys.argv[2]])
PY
)"

  ASSET_URL="$(python - <<'PY' "$RELEASE_JSON" "$ASSET_NAME"
import json,sys
payload=json.loads(sys.argv[1])
asset_name=sys.argv[2]
for asset in payload.get('assets', []):
    if asset.get('name') == asset_name:
        print(asset.get('browser_download_url',''))
        break
else:
    raise SystemExit(f"Missing release asset: {asset_name}")
PY
)"

  DOWNLOAD_ROOT="$(mktemp -d)"
  BUNDLE_PATH="$DOWNLOAD_ROOT/$ASSET_NAME"
  curl -fsSL -H "Accept: application/octet-stream" -H "User-Agent: ctx-installer" "$ASSET_URL" -o "$BUNDLE_PATH"
fi

case "$ACTION" in
  install) echo "Installing CTX $RELEASE_VERSION..." ;;
  update) echo "Updating CTX from ${INSTALLED_VERSION:-unknown} to $RELEASE_VERSION..." ;;
  repair) echo "Repairing CTX $RELEASE_VERSION..." ;;
esac

echo "Mode: $MODE"
echo "Install root: $INSTALL_ROOT"
echo "Release tag: $(python - <<'PY' "$RELEASE_JSON"
import json,sys
print(json.loads(sys.argv[1]).get('tag_name',''))
PY
)"
if [[ "$MODE" == "portable" ]]; then
  echo "Portable asset: $BUNDLE_PATH"
fi

export MODE INSTALL_ROOT SOURCE_REPO_PATH REPO_URL BUNDLE_PATH SKIP_VIEWER VERSION_LABEL="$RELEASE_VERSION" LINK_SCOPE
"$SCRIPT_ROOT/scripts/install-ctx.sh"
echo "CTX bootstrap complete."
