#!/usr/bin/env bash
# Build a throw-away snapshot of the plugin and launch Rhino + Grasshopper with it loaded.
# Editor-agnostic replacement for VS Code's F5.
#
#   scripts/rhino-dev.sh [options] [definition.gh]
#
#   definition.gh   open this Grasshopper file after launch (e.g. a fixture from tests/fixtures)
#   --rhino 8|9     Rhino version (default: 8)
#   --restart       quit a running Rhino first (it asks to save, like Cmd-Q)
#   --in-place      build into bin/Debug instead of a temp snapshot
#   --all-tfms      build net48 + net8.0 + net8.0-windows (default: net8.0, the only one Rhino on macOS loads)
#   --no-build      relaunch the most recent snapshot without building
#   --dry-run       build and print the launch command, but do not start Rhino
#
# Snapshots live in ${TMPDIR}dct-dev/<time>-<git sha>/ (the newest 5 are kept). Because each launch loads its
# own copy, you can keep editing and building while Rhino runs, and bin/Debug is never locked.
#
# Make sure no released DCT is installed (Package Manager or Grasshopper Libraries folder): identical component
# GUIDs would collide with the dev build.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT="$ROOT/DigitalCircularityToolkit.csproj"
SNAP_ROOT="${TMPDIR:-/tmp/}dct-dev"
RHINO_VER=8; RESTART=0; IN_PLACE=0; ALL_TFMS=0; NO_BUILD=0; DRY_RUN=0; GHFILE=""

while [[ $# -gt 0 ]]; do
  case "$1" in
    --rhino) RHINO_VER="$2"; shift 2 ;;
    --restart) RESTART=1; shift ;;
    --in-place) IN_PLACE=1; shift ;;
    --all-tfms) ALL_TFMS=1; shift ;;
    --no-build) NO_BUILD=1; shift ;;
    --dry-run) DRY_RUN=1; shift ;;
    -h|--help) sed -n '2,20p' "$0" | sed 's/^# \{0,1\}//'; exit 0 ;;
    -*) echo "unknown option $1" >&2; exit 1 ;;
    *) GHFILE="$(cd "$(dirname "$1")" && pwd)/$(basename "$1")"; shift ;;
  esac
done

APP="Rhino $RHINO_VER"
RHINO_BIN="/Applications/$APP.app/Contents/MacOS/Rhinoceros"
[[ -x "$RHINO_BIN" ]] || { echo "Rhino not found: $RHINO_BIN" >&2; exit 1; }
[[ -z "$GHFILE" || -f "$GHFILE" ]] || { echo "No such file: $GHFILE" >&2; exit 1; }

# ---- running Rhino? ----
if pgrep -xq Rhinoceros && [[ $DRY_RUN -eq 0 ]]; then
  if [[ $RESTART -eq 1 ]]; then
    echo "Quitting running Rhino (answer its save prompt if any)..."
    osascript -e "tell application \"$APP\" to quit" || true
    for _ in $(seq 1 120); do pgrep -xq Rhinoceros || break; sleep 0.5; done
    pgrep -xq Rhinoceros && { echo "Rhino is still running; giving up." >&2; exit 1; }
  else
    echo "Rhino is already running. Quit it, or pass --restart." >&2; exit 1
  fi
fi

# ---- build ----
if [[ $IN_PLACE -eq 1 ]]; then
  PKG_DIR="$ROOT/bin/Debug"
  OUT_ARGS=()
elif [[ $NO_BUILD -eq 1 ]]; then
  PKG_DIR="$(readlink "$SNAP_ROOT/latest" || true)/bin/Debug"
  [[ -d "$PKG_DIR" ]] || { echo "No previous snapshot to relaunch." >&2; exit 1; }
else
  SHA="$(git -C "$ROOT" rev-parse --short HEAD 2>/dev/null || echo nogit)"
  git -C "$ROOT" diff --quiet 2>/dev/null || SHA="$SHA-dirty"
  SNAP="$SNAP_ROOT/$(date +%Y%m%d-%H%M%S)-$SHA"
  mkdir -p "$SNAP"
  PKG_DIR="$SNAP/bin/Debug"
  OUT_ARGS=("-p:BaseOutputPath=$SNAP/bin/")
fi

if [[ $NO_BUILD -eq 0 ]]; then
  TFM_ARGS=(); [[ $ALL_TFMS -eq 1 ]] || TFM_ARGS=(-f net8.0)
  dotnet build "$PROJECT" -clp:NoSummary ${TFM_ARGS[@]+"${TFM_ARGS[@]}"} ${OUT_ARGS[@]+"${OUT_ARGS[@]}"}
  if [[ $IN_PLACE -eq 0 ]]; then
    ln -sfn "$SNAP" "$SNAP_ROOT/latest"
    # prune: keep the newest 5 snapshots
    ls -1dt "$SNAP_ROOT"/2* 2>/dev/null | tail -n +6 | while read -r old; do rm -rf "$old"; done
  fi
fi

# ---- launch ----
# Note: on macOS, commands queued after the Grasshopper command in a startup macro do not run, and
# `_-Grasshopper _Document _Open` is unreliable at startup, so files are opened from Python (scripts/open_gh.py).
# Automation (tools/rhino-profile) passes DCT_MACRO to replace the macro entirely and
# drives Grasshopper from Python instead.
MACRO="_Grasshopper"
if [[ -n "$GHFILE" ]]; then
  export DCT_OPEN_GH="$GHFILE"
  MACRO="_-RunPythonScript \"$ROOT/scripts/open_gh.py\""
fi
[[ -n "${DCT_MACRO:-}" ]] && MACRO="$DCT_MACRO"

echo "RHINO_PACKAGE_DIRS=$PKG_DIR"
echo "$RHINO_BIN -runscript=$MACRO"
[[ $DRY_RUN -eq 1 ]] && exit 0
RHINO_PACKAGE_DIRS="$PKG_DIR" exec "$RHINO_BIN" -nosplash "-runscript=$MACRO"
