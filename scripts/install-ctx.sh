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
PACKAGED_DOCS_MANIFEST="$REPO_ROOT/distribution/packaged-docs.txt"
PACKAGED_PROMPTS_MANIFEST="$REPO_ROOT/distribution/packaged-prompts.txt"

if [[ ! -f "$MANIFEST_PATH" ]]; then
  echo "Install manifest not found: $MANIFEST_PATH" >&2
  exit 1
fi

BIN_PATH="$INSTALL_ROOT/bin"
MCP_PATH="$INSTALL_ROOT/mcp"
ACP_PATH="$INSTALL_ROOT/acp"
VIEWER_PATH="$INSTALL_ROOT/viewer"
PROMPTS_PATH="$INSTALL_ROOT/prompts"
DOCS_PATH="$INSTALL_ROOT/docs"
METADATA_PATH="$INSTALL_ROOT/ctx-install.json"

copy_packaged_assets() {
  local source_root="$1"
  local target_root="$2"
  local manifest_path="$3"
  local strip_prefix="$4"

  if [[ ! -f "$manifest_path" ]]; then
    echo "Packaged asset manifest not found: $manifest_path" >&2
    exit 1
  fi

  while IFS= read -r relative_path || [[ -n "$relative_path" ]]; do
    relative_path="${relative_path//$'\r'/}"
    [[ -z "$relative_path" || "$relative_path" == \#* ]] && continue

    local source_path="$source_root/$relative_path"
    if [[ ! -f "$source_path" ]]; then
      echo "Required packaged asset not found: $source_path" >&2
      exit 1
    fi

    local target_relative="$relative_path"
    if [[ "$target_relative" == "$strip_prefix/"* ]]; then
      target_relative="${target_relative#"$strip_prefix/"}"
    else
      target_relative="$(basename "$target_relative")"
    fi

    mkdir -p "$(dirname "$target_root/$target_relative")"
    cp "$source_path" "$target_root/$target_relative"
  done < "$manifest_path"
}

validate_packaged_assets() {
  local target_root="$1"
  local manifest_path="$2"
  local strip_prefix="$3"
  local label="$4"

  while IFS= read -r relative_path || [[ -n "$relative_path" ]]; do
    relative_path="${relative_path//$'\r'/}"
    [[ -z "$relative_path" || "$relative_path" == \#* ]] && continue

    local target_relative="$relative_path"
    if [[ "$target_relative" == "$strip_prefix/"* ]]; then
      target_relative="${target_relative#"$strip_prefix/"}"
    else
      target_relative="$(basename "$target_relative")"
    fi

    if [[ ! -f "$target_root/$target_relative" ]]; then
      echo "Installed $label not found: $target_root/$target_relative" >&2
      exit 1
    fi
  done < "$manifest_path"
}

reset_install_layout() {
  mkdir -p "$INSTALL_ROOT"
  rm -rf "$BIN_PATH" "$MCP_PATH" "$ACP_PATH" "$VIEWER_PATH" "$PROMPTS_PATH" "$DOCS_PATH"
  mkdir -p "$BIN_PATH" "$MCP_PATH" "$ACP_PATH" "$VIEWER_PATH" "$PROMPTS_PATH" "$DOCS_PATH"
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

  cat > "$BIN_PATH/ctx-mcp" <<EOF
#!/usr/bin/env bash
set -euo pipefail
"$MCP_PATH/Ctx.Mcp" "\$@"
EOF
  chmod +x "$BIN_PATH/ctx-mcp"

  cat > "$BIN_PATH/ctx-agent-acp" <<EOF
#!/usr/bin/env bash
set -euo pipefail
"$ACP_PATH/Ctx.Agent.Acp" "\$@"
EOF
  chmod +x "$BIN_PATH/ctx-agent-acp"

  if [[ "$SKIP_VIEWER" == "1" ]]; then
    return
  fi

  cat > "$BIN_PATH/ctx-viewer" <<EOF
#!/usr/bin/env bash
set -euo pipefail
export CTX_INSTALL_ROOT="$INSTALL_ROOT"
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
  ln -sf "$BIN_PATH/ctx-mcp" "$target_dir/ctx-mcp"
  ln -sf "$BIN_PATH/ctx-agent-acp" "$target_dir/ctx-agent-acp"

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
  dotnet publish "$effective_repo/Ctx.Mcp/Ctx.Mcp.csproj" -c Release -o "$MCP_PATH"
  dotnet publish "$effective_repo/Ctx.Agent.Acp/Ctx.Agent.Acp.csproj" -c Release -o "$ACP_PATH"

  if [[ "$SKIP_VIEWER" != "1" ]]; then
    dotnet publish "$effective_repo/Ctx.Viewer/Ctx.Viewer.csproj" -c Release -o "$VIEWER_PATH"
  fi

  copy_packaged_assets "$effective_repo" "$DOCS_PATH" "$PACKAGED_DOCS_MANIFEST" "docs"
  copy_packaged_assets "$effective_repo" "$PROMPTS_PATH" "$PACKAGED_PROMPTS_MANIFEST" "prompts"
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

  if [[ -d "$extract_root/mcp" ]]; then
    cp -R "$extract_root/mcp/." "$MCP_PATH/"
  elif [[ -f "$extract_root/bin/Ctx.Mcp" ]]; then
    cp "$extract_root/bin/Ctx.Mcp" "$MCP_PATH/Ctx.Mcp"
  fi

  if [[ -d "$extract_root/acp" ]]; then
    cp -R "$extract_root/acp/." "$ACP_PATH/"
  elif [[ -f "$extract_root/bin/Ctx.Agent.Acp" ]]; then
    cp "$extract_root/bin/Ctx.Agent.Acp" "$ACP_PATH/Ctx.Agent.Acp"
  fi

  if [[ -d "$extract_root/viewer" ]]; then
    cp -R "$extract_root/viewer/." "$VIEWER_PATH/"
  fi

  if [[ -d "$extract_root/prompts" ]]; then
    cp -R "$extract_root/prompts/." "$PROMPTS_PATH/"
  else
    copy_packaged_assets "$REPO_ROOT" "$PROMPTS_PATH" "$PACKAGED_PROMPTS_MANIFEST" "prompts"
  fi

  if [[ -d "$extract_root/docs" ]]; then
    cp -R "$extract_root/docs/." "$DOCS_PATH/"
  else
    copy_packaged_assets "$REPO_ROOT" "$DOCS_PATH" "$PACKAGED_DOCS_MANIFEST" "docs"
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

validate_install_layout() {
  if [[ ! -x "$BIN_PATH/Ctx.Cli" ]]; then
    echo "Installed CLI executable not found or not executable: $BIN_PATH/Ctx.Cli" >&2
    exit 1
  fi

  if [[ ! -x "$MCP_PATH/Ctx.Mcp" ]]; then
    echo "Installed MCP executable not found or not executable: $MCP_PATH/Ctx.Mcp" >&2
    exit 1
  fi

  if [[ ! -x "$ACP_PATH/Ctx.Agent.Acp" ]]; then
    echo "Installed ACP executable not found or not executable: $ACP_PATH/Ctx.Agent.Acp" >&2
    exit 1
  fi

  if [[ "$SKIP_VIEWER" != "1" && ! -x "$VIEWER_PATH/Ctx.Viewer" ]]; then
    echo "Installed viewer executable not found or not executable: $VIEWER_PATH/Ctx.Viewer" >&2
    exit 1
  fi

  validate_packaged_assets "$DOCS_PATH" "$PACKAGED_DOCS_MANIFEST" "docs" "packaged documentation"
  validate_packaged_assets "$PROMPTS_PATH" "$PACKAGED_PROMPTS_MANIFEST" "prompts" "context prompt"
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
validate_install_layout
write_install_metadata "$MODE" "$SOURCE_ROOT" "$PROMPT_SOURCE"

echo "CTX installed to $INSTALL_ROOT via $MODE mode."
echo "CTX_INSTALL_ROOT=$INSTALL_ROOT"
echo "CTX_BIN_PATH=$BIN_PATH"
echo "CTX_MCP_PATH=$MCP_PATH"
echo "CTX_ACP_PATH=$ACP_PATH"
echo "CLI launcher: $BIN_PATH/ctx"
echo "MCP launcher: $BIN_PATH/ctx-mcp"
echo "ACP launcher: $BIN_PATH/ctx-agent-acp"
if [[ "$SKIP_VIEWER" != "1" ]]; then
  echo "Viewer launcher: $BIN_PATH/ctx-viewer"
fi
echo "Context docs: $DOCS_PATH"
append_path_guidance
