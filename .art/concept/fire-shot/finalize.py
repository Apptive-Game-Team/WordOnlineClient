#!/usr/bin/env python3
"""Export FireShot on its existing 256x88 projectile canvas."""

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
    scale = min(240 / image.width, 76 / image.height)
    image = image.resize((round(image.width * scale), round(image.height * scale)), Image.Resampling.LANCZOS)
    canvas = Image.new("RGBA", (256, 88), (0, 0, 0, 0))
    canvas.alpha_composite(image, ((256 - image.width) // 2, (88 - image.height) // 2))
    args.output.parent.mkdir(parents=True, exist_ok=True)
    canvas.save(args.output, optimize=True)

    review = Image.new("RGBA", (512, 176), (255, 0, 255, 255))
    review.alpha_composite(canvas.resize((512, 176), Image.Resampling.NEAREST))
    args.review.parent.mkdir(parents=True, exist_ok=True)
    review.save(args.review, optimize=True)
    alpha = canvas.getchannel("A")
    print(f"source={bounds[2]-bounds[0]}x{bounds[3]-bounds[1]} output=256x88 alpha={alpha.getextrema()} corners={[alpha.getpixel(p) for p in ((0,0),(255,0),(0,87),(255,87))]}")


if __name__ == "__main__":
    main()
