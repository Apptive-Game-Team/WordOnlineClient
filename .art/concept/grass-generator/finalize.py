#!/usr/bin/env python3
"""Finalize a grass_generator candidate that already carries real alpha.

Modeled on the firework-shell finalize.py from origin/feature/588
(.art/concept/firework-shell/finalize.py). It never mattes a painted
background away. It only works on a source that already has a true alpha
channel from the generator. If the source is opaque or carries a painted
checkerboard, this script refuses and says so -- regenerate instead.
"""

import argparse
import sys
from pathlib import Path

from PIL import Image


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("source", type=Path)
    parser.add_argument("output", type=Path)
    parser.add_argument("--max-dim", type=int, default=256, help="max canvas dimension (tier cap)")
    parser.add_argument("--pad", type=int, default=6, help="alpha-bbox padding before crop, in source pixels")
    args = parser.parse_args()

    image = Image.open(args.source).convert("RGBA")

    if image.getchannel("A").getextrema() == (255, 255):
        sys.exit(
            f"{args.source} has no real alpha (fully opaque). "
            "This script does not chroma-key backgrounds away; regenerate with "
            "the transparent-background request instead."
        )

    alpha = image.getchannel("A")
    thresholded = alpha.point(lambda a: 255 if a > 8 else 0)
    bbox = thresholded.getbbox()
    if bbox is None:
        sys.exit(f"{args.source} is fully transparent.")

    l, t, r, b = bbox
    l = max(0, l - args.pad)
    t = max(0, t - args.pad)
    r = min(image.width, r + args.pad)
    b = min(image.height, b + args.pad)
    cropped = image.crop((l, t, r, b))

    scale = min(args.max_dim / cropped.width, args.max_dim / cropped.height, 1.0)
    new_size = (max(1, round(cropped.width * scale)), max(1, round(cropped.height * scale)))
    resized = cropped.resize(new_size, Image.Resampling.LANCZOS)

    # Trim any fully-transparent border rows/columns introduced by resampling.
    post_bbox = resized.getchannel("A").point(lambda a: 255 if a > 8 else 0).getbbox()
    if post_bbox is not None and post_bbox != (0, 0, resized.width, resized.height):
        resized = resized.crop(post_bbox)

    args.output.parent.mkdir(parents=True, exist_ok=True)
    resized.save(args.output, optimize=True)
    print(f"saved {args.output} at {resized.size}, mode={resized.mode}")


if __name__ == "__main__":
    main()
