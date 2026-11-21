#!/usr/bin/env bash
# Run one Python script inside a fresh, unattended Rhino session, then quit Rhino.
#   tools/rhino-profile/run.sh <script.py> [rhino-dev.sh options, e.g. --no-build]
# Pass script inputs as environment variables. Rhino must not be running.
# Exit codes: 0 ok; 1 the script raised (traceback printed); 2 Rhino ended without finishing (build failure or
# crash; see <script>.log); 3 timeout (DCT_TIMEOUT seconds, default 1800).
# Rhino occasionally ignores the idle-exit; if it has not quit 30 s after the script's .done marker, it is killed.
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
SCRIPT="$(cd "$(dirname "$1")" && pwd)/$(basename "$1")"; shift
BASE="${SCRIPT%.py}"; ERR="$BASE.error.txt"; DONE="$BASE.done"; LOG="$BASE.log"
rm -f "$ERR" "$DONE"
TIMEOUT="${DCT_TIMEOUT:-1800}"

DCT_EXIT_WHEN_DONE=1 DCT_PROFILE_SCRIPT="$SCRIPT" \
  DCT_MACRO="_-RunPythonScript \"$ROOT/tools/rhino-profile/run_safe.py\"" \
  "$ROOT/scripts/rhino-dev.sh" "$@" >"$LOG" 2>&1 &
PID=$!          # rhino-dev.sh execs Rhino, so this becomes Rhino's PID

start=$SECONDS; done_at=""; status=0
while kill -0 "$PID" 2>/dev/null; do
  if [[ -z "$done_at" && -f "$DONE" ]]; then done_at=$SECONDS; fi
  if [[ -n "$done_at" && $((SECONDS - done_at)) -ge 30 ]]; then
    echo "Rhino did not quit after the script finished; killing it." >&2
    kill "$PID" 2>/dev/null || true; sleep 3; kill -9 "$PID" 2>/dev/null || true
    break
  fi
  if [[ $((SECONDS - start)) -ge $TIMEOUT ]]; then
    echo "Timeout after ${TIMEOUT}s; killing Rhino." >&2
    kill "$PID" 2>/dev/null || true; sleep 3; kill -9 "$PID" 2>/dev/null || true
    status=3; break
  fi
  sleep 1
done
wait "$PID" 2>/dev/null || true

if [[ -f "$ERR" ]]; then cat "$ERR" >&2; exit 1; fi
if [[ $status -ne 0 ]]; then exit $status; fi
if [[ ! -f "$DONE" ]]; then echo "Rhino ended before the script finished. Log: $LOG" >&2; tail -20 "$LOG" >&2; exit 2; fi
rm -f "$DONE"
