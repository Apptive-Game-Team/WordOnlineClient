#!/usr/bin/env python3
"""Export MagmaExplosion at its existing 217x256 Unity canvas."""

import argparse
from pathlib import Path
from PIL import Image


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("source", type=Path)
    parser.add_argument("output", type=Path)
    parser.add_argument("review", type=Path)
    args = parser.parse_args()

    image = Image.open(args.source).convert("RGBA")
    bounds = image.getchannel("A").getbbox()
    if bounds is None:
        raise SystemExit("source is fully transparent")
    image = image.crop(bounds)
    scale = min(217 / image.width, 256 / image.height)
    image = image.resize((round(image.width * scale), round(image.height * scale)), Image.Resampling.LANCZOS)
    canvas = Image.new("RGBA", (217, 256), (0, 0, 0, 0))
    canvas.alpha_composite(image, ((217 - image.width) // 2, 256 - image.height))
    args.output.parent.mkdir(parents=True, exist_ok=True)
    canvas.save(args.output, optimize=True)

    review = Image.new("RGBA", (434, 512), (255, 0, 255, 255))
    review.alpha_composite(canvas.resize((434, 512), Image.Resampling.NEAREST), (0, 0))
    args.review.parent.mkdir(parents=True, exist_ok=True)
    review.save(args.review, optimize=True)
    alpha = canvas.getchannel("A")
    print(f"source={bounds[2]-bounds[0]}x{bounds[3]-bounds[1]} output=217x256 alpha={alpha.getextrema()} corners={[alpha.getpixel(p) for p in ((0,0),(216,0),(0,255),(216,255))]}")


if __name__ == "__main__":
    main()
