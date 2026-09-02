#!/usr/bin/env python3
"""Export MagmaExplosion at its existing 217x256 Unity canvas."""

import argparse
from collections import deque
from pathlib import Path
from PIL import Image

def remove_painted_checker(image: Image.Image) -> Image.Image:
    pixels = image.load()
    width, height = image.size
    visited = bytearray(width * height)
    queue = deque()

    def is_background(x: int, y: int) -> bool:
        red, green, blue, _ = pixels[x, y]
        return min(red, green, blue) >= 225 and max(red, green, blue) - min(red, green, blue) <= 14

    for x in range(width):
        queue.extend(((x, 0), (x, height - 1)))
    for y in range(height):
        queue.extend(((0, y), (width - 1, y)))
    while queue:
        x, y = queue.popleft()
        index = y * width + x
        if visited[index] or not is_background(x, y):
            continue
        visited[index] = 1
        pixels[x, y] = (0, 0, 0, 0)
        if x: queue.append((x - 1, y))
        if x + 1 < width: queue.append((x + 1, y))
        if y: queue.append((x, y - 1))
        if y + 1 < height: queue.append((x, y + 1))
    return image


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("source", type=Path)
    parser.add_argument("output", type=Path)
    parser.add_argument("review", type=Path)
    args = parser.parse_args()

    image = remove_painted_checker(Image.open(args.source).convert("RGBA"))
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
