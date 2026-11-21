#!/usr/bin/env bash
# Render component icons from vector sources to the 24x24 PNGs Grasshopper 1 embeds.
#
#   Icons/<ClassName>.svg | .pdf   ->   Resources/Icons/<ClassName>.png
#
#   scripts/icons.sh                 # render every source that is newer than its PNG
#   scripts/icons.sh Icons/Foo.svg   # render specific sources (always)
#   scripts/icons.sh --force         # re-render everything
#
# Called automatically by `dotnet build` (target GenerateIcons in the csproj) on macOS/Linux.
# Tools: rsvg-convert (SVG) and pdftocairo (PDF): `brew install librsvg poppler`; falls back to `sips` for PDF.
# Generated PNGs are committed, so Windows/CI builds do not need these tools.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SRC_DIR="$ROOT/Icons"
OUT_DIR="$ROOT/Resources/Icons"
SIZE=24
mkdir -p "$OUT_DIR"

render() {
  local src="$1" name out
  name="$(basename "${src%.*}")"
  out="$OUT_DIR/$name.png"
  case "${src##*.}" in
    svg)
      command -v rsvg-convert >/dev/null || { echo "icons.sh: rsvg-convert missing (brew install librsvg); skipping $src" >&2; return 0; }
      rsvg-convert -w "$SIZE" -h "$SIZE" --keep-aspect-ratio -o "$out.tmp" "$src" ;;
    pdf)
      if command -v pdftocairo >/dev/null; then
        pdftocairo -png -singlefile -scale-to "$SIZE" -transp "$src" "$out.tmp" && mv "$out.tmp.png" "$out.tmp"
      elif command -v sips >/dev/null; then
        sips -s format png -Z "$SIZE" "$src" --out "$out.tmp" >/dev/null
      else
        echo "icons.sh: no PDF rasterizer (brew install poppler); skipping $src" >&2; return 0
      fi ;;
    *) echo "icons.sh: unsupported source $src" >&2; return 0 ;;
  esac
  mv -f "$out.tmp" "$out"   # atomic replace: parallel builds never read a half-written file
  echo "icon: $(basename "$src") -> Resources/Icons/$name.png"
}

force=0
sources=()
for arg in "$@"; do
  case "$arg" in
    --force) force=1 ;;
    *) sources+=("$arg") ;;
  esac
done

if [[ ${#sources[@]} -gt 0 ]]; then
  for s in "${sources[@]}"; do render "$s"; done
  exit 0
fi

shopt -s nullglob
for src in "$SRC_DIR"/*.svg "$SRC_DIR"/*.pdf; do
  out="$OUT_DIR/$(basename "${src%.*}").png"
  if [[ $force -eq 1 || ! -f "$out" || "$src" -nt "$out" ]]; then
    render "$src"
  fi
done
