#!/usr/bin/env python3
"""Compose the player's base and attack frames onto one shared production canvas.

The player sprite is swapped between two drawings at runtime without touching the
transform, so both frames have to sit on the same canvas at the same scale with the
feet on the same pixel row. The two source drawings never come back that way, so one
scale and one anchor are derived from the base frame and applied to both.

The canvas numbers are the ones the shipped `PlayerCharacterBase.png` already uses,
so the swap keeps the on-screen size and the ground contact it has today:
2048x2048, feet at y=1679, body centred on x=1117, character 1140px tall from the
ground to the top of the head. The raised staff reaches above that; only the
character is measured, because the staff is what moves between the frames.
"""

import argparse
from pathlib import Path

from PIL import Image

CANVAS = 2048
FEET_Y = 1679
CENTER_X = 1117
CHARACTER_HEIGHT = 1140

# A row belongs to the character, not the staff, once its opaque run is this much of
# the figure's full width. The staff is a narrow column; the head and hair are not.
HEAD_WIDTH_SHARE = 0.25
# The horizontal anchor is taken from the lower part of the figure so a raised or
# thrust staff cannot drag it sideways.
BODY_BAND_SHARE = 0.6


def opaque_bounds_per_row(alpha):
    width, height = alpha.size
    pixels = alpha.load()
    for y in range(height):
        left = None
        right = None
        for x in range(width):
            if pixels[x, y] > 8:
                if left is None:
                    left = x
                right = x
        yield y, left, right


def measure(image):
    alpha = image.getchannel("A")
    bbox = alpha.getbbox()
    if bbox is None:
        raise SystemExit("frame is fully transparent")
    figure_width = bbox[2] - bbox[0]
    rows = [(y, left, right) for y, left, right in opaque_bounds_per_row(alpha)
            if left is not None]

    head_top = next(y for y, left, right in rows
                    if right - left >= figure_width * HEAD_WIDTH_SHARE)
    feet_y = bbox[3]

    band_start = feet_y - int((feet_y - head_top) * BODY_BAND_SHARE)
    band = [(left, right) for y, left, right in rows if band_start <= y <= feet_y]
    body_center = (min(l for l, _ in band) + max(r for _, r in band)) / 2

    return {
        "bbox": bbox,
        "head_top": head_top,
        "feet_y": feet_y,
        "character_height": feet_y - head_top,
        "body_center": body_center,
    }


def place(image, scale, body_center, feet_y):
    width = max(1, round(image.width * scale))
    height = max(1, round(image.height * scale))
    scaled = image.resize((width, height), Image.Resampling.LANCZOS)

    canvas = Image.new("RGBA", (CANVAS, CANVAS), (0, 0, 0, 0))
    left = round(CENTER_X - body_center * scale)
    top = round(FEET_Y - feet_y * scale)
    canvas.alpha_composite(scaled, (left, top))
    return canvas


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("base", type=Path)
    parser.add_argument("attack", type=Path)
    parser.add_argument("--out-base", type=Path, required=True)
    parser.add_argument("--out-attack", type=Path, required=True)
    args = parser.parse_args()

    base = Image.open(args.base).convert("RGBA")
    attack = Image.open(args.attack).convert("RGBA")

    base_metrics = measure(base)
    attack_metrics = measure(attack)
    scale = CHARACTER_HEIGHT / base_metrics["character_height"]

    for image, metrics, out in (
        (base, base_metrics, args.out_base),
        (attack, attack_metrics, args.out_attack),
    ):
        composed = place(image, scale, metrics["body_center"], metrics["feet_y"])
        out.parent.mkdir(parents=True, exist_ok=True)
        composed.save(out, optimize=True)
        bbox = composed.getchannel("A").getbbox()
        print(f"{out.name} {composed.width}x{composed.height} bbox={bbox} "
              f"feet_gap={CANVAS - bbox[3]} scale={scale:.4f}")

    print(f"character height {CHARACTER_HEIGHT}px, feet y={FEET_Y}, centre x={CENTER_X}")


if __name__ == "__main__":
    main()
