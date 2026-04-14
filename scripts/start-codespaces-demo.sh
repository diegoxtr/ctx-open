#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PORT="${PORT:-5271}"
PID_FILE="/tmp/ctx-viewer-codespaces.pid"
LOG_FILE="/tmp/ctx-viewer-codespaces.log"
DEMO_REPOSITORY="${CTX_LIVE_DEMO_REPOSITORY_PATH:-$ROOT_DIR/examples/ctx/agent-session-continuity}"

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

echo "CTX Viewer live demo started"
echo "Port: $PORT"
echo "Default repository: $DEMO_REPOSITORY"
echo "Log: $LOG_FILE"
