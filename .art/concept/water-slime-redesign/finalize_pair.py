#!/usr/bin/env python3
"""Finalize the WaterSlime idle/attack pair on one shared stage."""

import argparse
from collections import deque
from pathlib import Path

from PIL import Image


def remove_painted_checker(path: Path) -> Image.Image:
    image = Image.open(path).convert("RGBA")
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
        if x:
            queue.append((x - 1, y))
        if x + 1 < width:
            queue.append((x + 1, y))
        if y:
            queue.append((x, y - 1))
        if y + 1 < height:
            queue.append((x, y + 1))

    return image


def alpha_bounds(image: Image.Image) -> tuple[int, int, int, int]:
    bounds = image.getchannel("A").getbbox()
    if bounds is None:
        raise ValueError("image has no visible pixels")
    return bounds


def place_shared(image: Image.Image, bounds: tuple[int, int, int, int], scale: float) -> Image.Image:
    crop = image.crop(bounds)
    size = (round(crop.width * scale), round(crop.height * scale))
    crop = crop.resize(size, Image.Resampling.LANCZOS)
    canvas = Image.new("RGBA", (320, 224), (0, 0, 0, 0))
    canvas.alpha_composite(crop, (12, 216 - crop.height))
    return canvas


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("idle_source", type=Path)
    parser.add_argument("attack_source", type=Path)
    parser.add_argument("idle_output", type=Path)
    parser.add_argument("attack_output", type=Path)
    parser.add_argument("review_output", type=Path)
    args = parser.parse_args()

    idle = remove_painted_checker(args.idle_source)
    attack = remove_painted_checker(args.attack_source)
    idle_bounds = alpha_bounds(idle)
    attack_bounds = alpha_bounds(attack)

    # One source-pixel scale for both frames. The generator kept the body at the
    # same source scale; only the attack canvas widened for the projectile.
    scale = min(188 / (idle_bounds[2] - idle_bounds[0]), 192 / (idle_bounds[3] - idle_bounds[1]))
    idle_final = place_shared(idle, idle_bounds, scale)
    attack_final = place_shared(attack, attack_bounds, scale)

    for path, image in ((args.idle_output, idle_final), (args.attack_output, attack_final)):
        path.parent.mkdir(parents=True, exist_ok=True)
        image.save(path, optimize=True)

    # Magenta makes any retained checkerboard or pale fringe immediately visible.
    review = Image.new("RGBA", (640, 224), (255, 0, 255, 255))
    review.alpha_composite(idle_final, (0, 0))
    review.alpha_composite(attack_final, (320, 0))
    args.review_output.parent.mkdir(parents=True, exist_ok=True)
    review.save(args.review_output, optimize=True)

    print(f"idle_source={idle.size} bounds={idle_bounds}")
    print(f"attack_source={attack.size} bounds={attack_bounds}")
    print(f"shared_scale={scale:.6f} canvas=320x224 bottom_gap=8")


if __name__ == "__main__":
    main()
