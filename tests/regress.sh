#!/usr/bin/env bash
# Regression check of the current working tree against the recorded goldens, in an unattended Rhino session.
#
#   tests/regress.sh                 build, (generate missing fixtures,) record to tests/out/<time>, compare
#   tests/regress.sh --update        same, then copy the recording over tests/goldens (after reviewing the diff!)
#   tests/regress.sh --regen         regenerate the fixtures first (after changing tests/rhino/make_fixtures.py)
#   tests/regress.sh --only a;b      only these fixtures (names without .gh)
#   tests/regress.sh --no-build      skip the build (rhino-dev.sh option)
#
# Rhino must not be running. Fixtures: tests/fixtures/generated (git-ignored). Goldens: tests/goldens/*.json.gz
# (tracked). Recordings: tests/out (git-ignored). Compare any two recordings: tests/compare.py A B.
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
UPDATE=0; PASS=()
export DCT_FIXTURE_DIR="$ROOT/tests/fixtures/generated"
while [[ $# -gt 0 ]]; do
  case "$1" in
    --update) UPDATE=1; shift ;;
    --regen) export DCT_FIXTURES_FORCE=1; shift ;;
    --only) export DCT_FIXTURES="$2"; shift 2 ;;
    --no-build) PASS+=(--no-build); shift ;;
    -h|--help) sed -n '2,12p' "$0" | sed 's/^# \{0,1\}//'; exit 0 ;;
    *) echo "unknown option $1" >&2; exit 1 ;;
  esac
done
export DCT_RECORD_DIR="$ROOT/tests/out/$(date +%Y%m%d-%H%M%S)"
mkdir -p "$DCT_RECORD_DIR"
ln -sfn "$DCT_RECORD_DIR" "$ROOT/tests/out/latest"
"$ROOT/tools/rhino-profile/run.sh" "$ROOT/tests/rhino/session.py" ${PASS[@]+"${PASS[@]}"}
echo "recorded: $DCT_RECORD_DIR"
STATUS=0
if [[ -d "$ROOT/tests/goldens" ]] && ls "$ROOT/tests/goldens"/*.json.gz >/dev/null 2>&1; then
  python3 "$ROOT/tests/compare.py" ${DCT_FIXTURES:+--partial} "$ROOT/tests/goldens" "$DCT_RECORD_DIR" || STATUS=$?
else
  echo "no goldens yet (use --update to create them)"
fi
if [[ $UPDATE -eq 1 ]]; then
  mkdir -p "$ROOT/tests/goldens"
  for f in "$DCT_RECORD_DIR"/*.json; do gzip -9 -n -c "$f" > "$ROOT/tests/goldens/$(basename "$f").gz"; done
  echo "goldens updated from $DCT_RECORD_DIR"
  STATUS=0
fi
exit $STATUS
