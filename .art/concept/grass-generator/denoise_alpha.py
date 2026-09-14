#!/usr/bin/env python3
"""Remove isolated single-pixel alpha 'dust' noise the generator sometimes
leaves scattered across an otherwise correctly transparent background,
without touching the real subject's alpha at all.

This is NOT chroma-keying or matting out a painted background: the source
already has a genuine alpha channel and a genuine transparent region (see
check_alpha.py). It just has scattered stray full-alpha grey speckle pixels
outside the subject silhouette (visible in the alpha channel itself as
TV-static noise). This script finds the single large connected blob that is
the subject, dilates it a few pixels to keep soft anti-aliased edges intact,
and zeroes alpha everywhere outside that dilated region. Alpha *inside* the
region is left byte-for-byte untouched.
"""
import argparse
import sys
from pathlib import Path

import numpy as np
from PIL import Image
from scipy import ndimage


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("source", type=Path)
    parser.add_argument("output", type=Path)
    parser.add_argument("--strong-threshold", type=int, default=128)
    parser.add_argument("--dilate", type=int, default=6)
    args = parser.parse_args()

    im = Image.open(args.source).convert("RGBA")
    arr = np.array(im)
    alpha = arr[:, :, 3]

    strong = alpha >= args.strong_threshold
    labeled, n = ndimage.label(strong, structure=np.ones((3, 3)))
    if n == 0:
        sys.exit(f"{args.source}: no pixels above alpha {args.strong_threshold}; nothing to keep.")

    sizes = ndimage.sum(strong, labeled, range(1, n + 1))
    main_label = int(np.argmax(sizes)) + 1
    main_mask = labeled == main_label
    dilated = ndimage.binary_dilation(main_mask, iterations=args.dilate)

    removed = int(np.sum((~dilated) & (alpha > 0)))
    new_alpha = np.where(dilated, alpha, 0)
    arr[:, :, 3] = new_alpha

    out = Image.fromarray(arr, "RGBA")
    args.output.parent.mkdir(parents=True, exist_ok=True)
    out.save(args.output)
    print(f"saved {args.output}; zeroed {removed} stray-alpha pixels outside the dilated main blob "
          f"(kept blob size {int(sizes[main_label - 1])}px, {n} components total)")


if __name__ == "__main__":
    main()
