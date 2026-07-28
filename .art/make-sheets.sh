#!/usr/bin/env bash
# Regenerate art contact sheets.
#   .art/sheets/anchors.png  - frozen style anchors (feed these to image generation)
#   .art/sheets/sprites.png  - every live magic sprite (drift review)
#   .art/sheets/concept.png  - concept art variants, if any exist (style selection)
set -euo pipefail

cd "$(dirname "$0")/.."

FONT=/System/Library/Fonts/Helvetica.ttc
mkdir -p .art/sheets

magick montage -font "$FONT" -pointsize 12 -fill white \
  .art/anchors/*.png \
  -background '#303030' -tile 6x -geometry 160x160+8+8 -label '%t' \
  .art/sheets/anchors.png

magick montage -font "$FONT" -pointsize 10 -fill white \
  Assets/Resources/Game/sprites/*.png \
  -background '#303030' -tile 8x -geometry 128x128+6+6 -label '%t' \
  .art/sheets/sprites.png

if compgen -G '.art/concept/*.png' > /dev/null; then
  magick montage -font "$FONT" -pointsize 14 -fill white \
    .art/concept/*.png \
    -background '#303030' -tile 3x -geometry 320x320+10+10 -label '%t' \
    .art/sheets/concept.png
fi

magick identify .art/sheets/*.png
