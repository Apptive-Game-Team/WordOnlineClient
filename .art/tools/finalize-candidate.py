#!/usr/bin/env python3
"""Trim and resize a transparent art candidate for production review."""

import argparse
from pathlib import Path

from PIL import Image


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("input", type=Path)
    parser.add_argument("output", type=Path)
    parser.add_argument("--max-size", type=int, required=True)
    args = parser.parse_args()

    image = Image.open(args.input).convert("RGBA")
    bounds = image.getchannel("A").getbbox()
    if bounds is None:
        raise SystemExit("candidate is fully transparent")

    image = image.crop(bounds)
    image.thumbnail((args.max_size, args.max_size), Image.Resampling.LANCZOS)

    # LANCZOS resamples alpha, so a transparent pixel beside a soft edge can come
    # back as a 1 and the corner check then fails on a pixel nobody can see. Floor
    # it and re-crop, or a clean cutout reads as a dirty one.
    image.putalpha(image.getchannel("A").point(lambda value: 0 if value < 8 else value))
    bounds = image.getchannel("A").getbbox()
    if bounds is not None:
        image = image.crop(bounds)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    image.save(args.output, optimize=True)

    alpha = image.getchannel("A")
    corners = ((0, 0), (image.width - 1, 0), (0, image.height - 1),
               (image.width - 1, image.height - 1))
    print(
        f"{args.output.name} PNG {image.width}x{image.height} RGBA "
        f"alpha={alpha.getextrema()} "
        f"corners={[alpha.getpixel(point) for point in corners]}"
    )


if __name__ == "__main__":
    main()
