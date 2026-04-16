#!/usr/bin/env bash
set -euo pipefail

MODE="${MODE:-source}"
INSTALL_ROOT="${INSTALL_ROOT:-$HOME/.local/share/ctx}"
SOURCE_REPO_PATH="${SOURCE_REPO_PATH:-}"
REPO_URL="${REPO_URL:-https://github.com/diegoxtr/ctx-open.git}"
BUNDLE_PATH="${BUNDLE_PATH:-}"
VIEWER_URL="${VIEWER_URL:-http://127.0.0.1:5271}"
VERSION_LABEL="${VERSION_LABEL:-dev}"
LINK_SCOPE="${LINK_SCOPE:-auto}"
SKIP_VIEWER="${SKIP_VIEWER:-0}"

SCRIPT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_ROOT/.." && pwd)"
MANIFEST_PATH="$REPO_ROOT/distribution/install-manifest.json"

if [[ ! -f "$MANIFEST_PATH" ]]; then
  echo "Install manifest not found: $MANIFEST_PATH" >&2
  exit 1
fi

BIN_PATH="$INSTALL_ROOT/bin"
VIEWER_PATH="$INSTALL_ROOT/viewer"
PROMPTS_PATH="$INSTALL_ROOT/prompts"
DOCS_PATH="$INSTALL_ROOT/docs"
METADATA_PATH="$INSTALL_ROOT/ctx-install.json"

reset_install_layout() {
  mkdir -p "$INSTALL_ROOT"
  rm -rf "$BIN_PATH" "$VIEWER_PATH" "$PROMPTS_PATH" "$DOCS_PATH"
  mkdir -p "$BIN_PATH" "$VIEWER_PATH" "$PROMPTS_PATH" "$DOCS_PATH"
}

write_install_metadata() {
  local mode_name="$1"
  local source_root="$2"
  local prompt_source="$3"

  cat > "$METADATA_PATH" <<EOF
{
  "installedAtUtc": "$(date -u +"%Y-%m-%dT%H:%M:%SZ")",
  "installRoot": "$INSTALL_ROOT",
  "version": "$VERSION_LABEL",
  "mode": "$mode_name",
  "sourceRoot": "$source_root",
  "helperPromptSource": "$prompt_source",
  "viewerUrl": "$VIEWER_URL"
}
EOF
}

write_launchers() {
  cat > "$BIN_PATH/ctx" <<EOF
#!/usr/bin/env bash
set -euo pipefail
"$BIN_PATH/Ctx.Cli" "\$@"
EOF
  chmod +x "$BIN_PATH/ctx"

  if [[ "$SKIP_VIEWER" == "1" ]]; then
    return
  fi

  cat > "$BIN_PATH/ctx-viewer" <<EOF
#!/usr/bin/env bash
set -euo pipefail
cd "$VIEWER_PATH"
"$VIEWER_PATH/Ctx.Viewer" --urls "$VIEWER_URL"
EOF
  chmod +x "$BIN_PATH/ctx-viewer"
}

link_binaries() {
  if [[ "$LINK_SCOPE" == "none" ]]; then
    return
  fi

  local target_dir=""
  case "$LINK_SCOPE" in
    auto)
      if [[ -w "/usr/local/bin" ]]; then
        target_dir="/usr/local/bin"
      else
        target_dir="$HOME/.local/bin"
      fi
      ;;
    global)
      target_dir="/usr/local/bin"
      ;;
    user)
      target_dir="$HOME/.local/bin"
      ;;
    *)
      echo "Unsupported LINK_SCOPE: $LINK_SCOPE" >&2
      exit 1
      ;;
  esac

  mkdir -p "$target_dir"
  ln -sf "$BIN_PATH/ctx" "$target_dir/ctx"

  if [[ "$SKIP_VIEWER" != "1" ]]; then
    ln -sf "$BIN_PATH/ctx-viewer" "$target_dir/ctx-viewer"
  fi

  LINK_TARGET_DIR="$target_dir"
}

install_from_source() {
  local effective_repo="$SOURCE_REPO_PATH"

  if [[ -z "$effective_repo" ]]; then
    effective_repo="$(mktemp -d)"
    git clone "$REPO_URL" "$effective_repo"
  fi

  dotnet publish "$effective_repo/Ctx.Cli/Ctx.Cli.csproj" -c Release -o "$BIN_PATH"

  if [[ "$SKIP_VIEWER" != "1" ]]; then
    dotnet publish "$effective_repo/Ctx.Viewer/Ctx.Viewer.csproj" -c Release -o "$VIEWER_PATH"
  fi

  cp "$effective_repo/prompts/CTX_HELPER_PROMPT.md" "$PROMPTS_PATH/CTX_HELPER_PROMPT.md"
  cp "$effective_repo/prompts/CTX_AGENT_PROMPT.md" "$PROMPTS_PATH/CTX_AGENT_PROMPT.md"
  cp "$effective_repo/docs/CTX_VIEWER_GUIDE.md" "$DOCS_PATH/CTX_VIEWER_GUIDE.md"
  cp "$effective_repo/docs/CTX_AUTONOMOUS_OPERATION_PROTOCOL.md" "$DOCS_PATH/CTX_AUTONOMOUS_OPERATION_PROTOCOL.md"
  printf '%s' "$effective_repo"
}

install_from_portable() {
  if [[ -z "$BUNDLE_PATH" || ! -f "$BUNDLE_PATH" ]]; then
    echo "Portable bundle not found: $BUNDLE_PATH" >&2
    exit 1
  fi

  local extract_root
  extract_root="$(mktemp -d)"

  if [[ "$BUNDLE_PATH" == *.zip ]]; then
    unzip -q "$BUNDLE_PATH" -d "$extract_root"
  else
    tar -xzf "$BUNDLE_PATH" -C "$extract_root"
  fi

  cp -R "$extract_root/bin/." "$BIN_PATH/"

  if [[ -d "$extract_root/viewer" ]]; then
    cp -R "$extract_root/viewer/." "$VIEWER_PATH/"
  fi

  if [[ -f "$extract_root/prompts/CTX_HELPER_PROMPT.md" ]]; then
    cp "$extract_root/prompts/CTX_HELPER_PROMPT.md" "$PROMPTS_PATH/CTX_HELPER_PROMPT.md"
  else
    cp "$REPO_ROOT/prompts/CTX_HELPER_PROMPT.md" "$PROMPTS_PATH/CTX_HELPER_PROMPT.md"
  fi

  if [[ -f "$extract_root/prompts/CTX_AGENT_PROMPT.md" ]]; then
    cp "$extract_root/prompts/CTX_AGENT_PROMPT.md" "$PROMPTS_PATH/CTX_AGENT_PROMPT.md"
  else
    cp "$REPO_ROOT/prompts/CTX_AGENT_PROMPT.md" "$PROMPTS_PATH/CTX_AGENT_PROMPT.md"
  fi

  if [[ -f "$extract_root/docs/CTX_VIEWER_GUIDE.md" ]]; then
    cp "$extract_root/docs/CTX_VIEWER_GUIDE.md" "$DOCS_PATH/CTX_VIEWER_GUIDE.md"
  else
    cp "$REPO_ROOT/docs/CTX_VIEWER_GUIDE.md" "$DOCS_PATH/CTX_VIEWER_GUIDE.md"
  fi

  if [[ -f "$extract_root/docs/CTX_AUTONOMOUS_OPERATION_PROTOCOL.md" ]]; then
    cp "$extract_root/docs/CTX_AUTONOMOUS_OPERATION_PROTOCOL.md" "$DOCS_PATH/CTX_AUTONOMOUS_OPERATION_PROTOCOL.md"
  else
    cp "$REPO_ROOT/docs/CTX_AUTONOMOUS_OPERATION_PROTOCOL.md" "$DOCS_PATH/CTX_AUTONOMOUS_OPERATION_PROTOCOL.md"
  fi

  printf '%s' "$extract_root"
}

append_path_guidance() {
  if [[ -n "${LINK_TARGET_DIR:-}" ]]; then
    echo
    echo "Command links created in: $LINK_TARGET_DIR"
    if [[ "$LINK_TARGET_DIR" == "$HOME/.local/bin" ]]; then
      echo "Add this to your shell profile if needed:"
      echo "  export PATH=\"$HOME/.local/bin:\$PATH\""
    fi
    return
  fi

  cat <<EOF

Add this to your shell profile if needed:
  export PATH="$BIN_PATH:\$PATH"
EOF
}

reset_install_layout

if [[ "$MODE" == "source" ]]; then
  SOURCE_ROOT="$(install_from_source)"
  PROMPT_SOURCE="$SOURCE_ROOT/prompts/CTX_HELPER_PROMPT.md"
elif [[ "$MODE" == "portable" ]]; then
  SOURCE_ROOT="$(install_from_portable)"
  PROMPT_SOURCE="$PROMPTS_PATH/CTX_HELPER_PROMPT.md"
else
  echo "Unsupported MODE: $MODE" >&2
  exit 1
fi

write_launchers
link_binaries
write_install_metadata "$MODE" "$SOURCE_ROOT" "$PROMPT_SOURCE"

echo "CTX installed to $INSTALL_ROOT via $MODE mode."
echo "CLI launcher: $BIN_PATH/ctx"
if [[ "$SKIP_VIEWER" != "1" ]]; then
  echo "Viewer launcher: $BIN_PATH/ctx-viewer"
fi
echo "Context docs: $DOCS_PATH"
append_path_guidance
