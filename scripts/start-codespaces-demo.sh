#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PORT="${PORT:-5271}"
PID_FILE="/tmp/ctx-viewer-codespaces.pid"
LOG_FILE="/tmp/ctx-viewer-codespaces.log"
DEMO_REPOSITORY="${CTX_LIVE_DEMO_REPOSITORY_PATH:-$ROOT_DIR/examples/ctx/agent-session-continuity}"

bash "$ROOT_DIR/scripts/ensure-dotnet-sdk.sh"
export DOTNET_ROOT="${DOTNET_INSTALL_DIR:-$HOME/.dotnet}"
export PATH="$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH"

if [[ -f "$PID_FILE" ]]; then
  EXISTING_PID="$(cat "$PID_FILE")"
  if [[ -n "$EXISTING_PID" ]] && kill -0 "$EXISTING_PID" 2>/dev/null; then
    echo "CTX Viewer demo is already running on port $PORT"
    echo "Demo repository: $DEMO_REPOSITORY"
    echo "Log: $LOG_FILE"
    exit 0
  fi
fi

export CTX_VIEWER_DEFAULT_REPOSITORY_PATH="$DEMO_REPOSITORY"

nohup dotnet run --project "$ROOT_DIR/Ctx.Viewer/Ctx.Viewer.csproj" --urls "http://0.0.0.0:$PORT" >"$LOG_FILE" 2>&1 &
echo $! > "$PID_FILE"

for _ in {1..30}; do
  if curl -fsS "http://127.0.0.1:$PORT/" >/dev/null 2>&1; then
    echo "CTX Viewer live demo started"
    echo "Port: $PORT"
    echo "Default repository: $DEMO_REPOSITORY"
    echo "Log: $LOG_FILE"
    exit 0
  fi

  if ! kill -0 "$(cat "$PID_FILE")" 2>/dev/null; then
    break
  fi

  sleep 1
done

echo "CTX Viewer live demo failed to start" >&2
echo "Port: $PORT" >&2
echo "Default repository: $DEMO_REPOSITORY" >&2
echo "Log: $LOG_FILE" >&2
echo "Last log lines:" >&2
tail -n 20 "$LOG_FILE" >&2 || true
exit 1
