#!/usr/bin/env python3
"""Compose the player's base and attack frames onto one shared production canvas.

`AttackSpriteSwapController` swaps `SpriteRenderer.sprite` and touches no
transform, so both frames have to sit on one canvas, at one scale, with the feet
on the same row and the standing width around the same column. The two source
drawings never come back that way, so one anchor is derived here and applied to
both.

The canvas is sized so the new player occupies the screen space the shipped
`Assets/Resources/Game/player.png` occupies today: that sprite is 192x170 at 100
pixels per unit with `alignment: 7` (bottom centre), its character is 166px tall,
and its feet sit on the sprite origin. So the output keeps the character 166
world units/100 tall and puts the feet on the pivot, and the pixels-per-unit
value is picked to reach that with no resampling of the base frame.

Two traps this script exists to avoid:

* `Assets/Art/Images/Customize/PlayerCharacterBase.png` is NOT what the game
  draws. `Assets/Resources/Prefabs/Player.prefab` points its `PlayerImage`
  renderer at `Assets/Resources/Game/player.png`. Replacing the Customize file
  changes nothing on screen.
* A head top cannot be found automatically on these frames. The base frame holds
  a raised staff above the head and the attack frame's topmost pixels are hair,
  so both a width-share rule and a width-jump rule pick the wrong row. Pass the
  measured row with --base-head-top.
"""

import argparse
from pathlib import Path

from PIL import Image

ALPHA_THRESHOLD = 8
# The shipped player.png this output has to match: 100 pixels per unit, a 166px
# character, and the feet on the sprite origin.
OLD_PIXELS_PER_UNIT = 100
OLD_CHARACTER_HEIGHT = 166
# The horizontal anchor is the standing width, read from the bottom of the
# character so a raised or thrust staff cannot drag it sideways.
FEET_BAND_SHARE = 0.12
MARGIN = 8

# The staff crystal, so the prefab can park the element auras on it. The cape is
# blue too, and darker; these bounds pick the crystal's lit core only.
def is_crystal(pixel):
    red, green, blue, alpha = pixel
    return alpha > 200 and blue > 230 and blue - red > 150 and blue - green > 80


def row_spans(image):
    alpha = image.getchannel("A").load()
    width, height = image.size
    spans = []
    for y in range(height):
        left = right = None
        for x in range(width):
            if alpha[x, y] > ALPHA_THRESHOLD:
                if left is None:
                    left = x
                right = x
        spans.append((left, right))
    return spans


def cluster_center(image, predicate, window):
    pixels = image.load()
    xs = []
    ys = []
    for y in range(window[1], window[3]):
        for x in range(window[0], window[2]):
            if predicate(pixels[x, y]):
                xs.append(x)
                ys.append(y)
    if not xs:
        raise SystemExit(f"no pixels matched inside {window}")
    return ((min(xs) + max(xs)) / 2, (min(ys) + max(ys)) / 2)


def measure(image, head_top, crystal_window):
    spans = row_spans(image)
    filled = [y for y, (left, _) in enumerate(spans) if left is not None]
    content_top, feet_y = filled[0], filled[-1]
    left = min(l for l, _ in spans if l is not None)
    right = max(r for _, r in spans if r is not None)

    if head_top is None:
        head_top = content_top
    character_height = feet_y - head_top

    band_top = feet_y - round(character_height * FEET_BAND_SHARE)
    band = [(l, r) for y, (l, r) in enumerate(spans)
            if l is not None and band_top <= y <= feet_y]
    feet_center = (min(l for l, _ in band) + max(r for _, r in band)) / 2

    return {
        "bbox": (left, content_top, right, feet_y),
        "head_top": head_top,
        "feet_y": feet_y,
        "character_height": character_height,
        "feet_center": feet_center,
        "crystal": cluster_center(image, is_crystal, crystal_window),
    }


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("base", type=Path)
    parser.add_argument("attack", type=Path)
    parser.add_argument("--out-base", type=Path, required=True)
    parser.add_argument("--out-attack", type=Path, required=True)
    parser.add_argument("--base-head-top", type=int, default=None,
                        help="row of the top of the hair in the base frame; the "
                             "raised staff sits above it, so it cannot be found "
                             "from the alpha channel alone")
    parser.add_argument("--base-crystal-window", type=int, nargs=4,
                        metavar=("X0", "Y0", "X1", "Y1"), required=True)
    parser.add_argument("--attack-crystal-window", type=int, nargs=4,
                        metavar=("X0", "Y0", "X1", "Y1"), required=True)
    args = parser.parse_args()

    base = Image.open(args.base).convert("RGBA")
    attack = Image.open(args.attack).convert("RGBA")
    base_metrics = measure(base, args.base_head_top, args.base_crystal_window)
    attack_metrics = measure(attack, None, args.attack_crystal_window)

    # No resampling: the pixels-per-unit value is derived from the base frame so
    # the character already has the size it needs at scale 1.
    pixels_per_unit = round(
        base_metrics["character_height"] * OLD_PIXELS_PER_UNIT / OLD_CHARACTER_HEIGHT)

    frames = ((base, base_metrics, args.out_base),
              (attack, attack_metrics, args.out_attack))

    lefts, rights, tops = [], [], []
    for _, metrics, _ in frames:
        x0, y0, x1, _ = metrics["bbox"]
        lefts.append(x0 - metrics["feet_center"])
        rights.append(x1 - metrics["feet_center"])
        tops.append(y0 - metrics["feet_y"])

    anchor_x = round(-min(lefts)) + MARGIN
    anchor_y = round(-min(tops)) + MARGIN
    canvas_width = anchor_x + round(max(rights)) + MARGIN + 1
    canvas_height = anchor_y + MARGIN + 1

    for image, metrics, out in frames:
        canvas = Image.new("RGBA", (canvas_width, canvas_height), (0, 0, 0, 0))
        offset = (anchor_x - round(metrics["feet_center"]),
                  anchor_y - metrics["feet_y"])
        canvas.alpha_composite(image, offset)
        out.parent.mkdir(parents=True, exist_ok=True)
        canvas.save(out, optimize=True)
        metrics["offset"] = offset
        print(f"{out.name} {canvas_width}x{canvas_height} placed at {offset} "
              f"head_top={metrics['head_top']} feet={metrics['feet_y']} "
              f"character={metrics['character_height']}px "
              f"feet_center={metrics['feet_center']}")

    def to_local(point, metrics):
        x = (point[0] + metrics["offset"][0] - anchor_x) / pixels_per_unit
        y = (anchor_y - (point[1] + metrics["offset"][1])) / pixels_per_unit
        return round(x, 3), round(y, 3)

    pivot_x = anchor_x / canvas_width
    pivot_y = (canvas_height - anchor_y) / canvas_height
    sprite_size = (round(canvas_width / pixels_per_unit, 3),
                   round(canvas_height / pixels_per_unit, 3))
    old_sprite_height = max(192, 170) / OLD_PIXELS_PER_UNIT

    print()
    print(f"spritePixelsToUnits: {pixels_per_unit}")
    print(f"alignment: 9  spritePivot: {{x: {pivot_x:.4f}, y: {pivot_y:.4f}}}")
    print(f"character height {base_metrics['character_height'] / pixels_per_unit:.3f} "
          f"units (player.png: {OLD_CHARACTER_HEIGHT / OLD_PIXELS_PER_UNIT})")
    print(f"m_Size: {{x: {sprite_size[0]}, y: {sprite_size[1]}}}")
    print(f"StaffAura idle localPosition:   {to_local(base_metrics['crystal'], base_metrics)}")
    print(f"StaffAura attack localPosition: {to_local(attack_metrics['crystal'], attack_metrics)}")
    print(f"_effectScaleReferenceHeight to hold today's effect size: "
          f"{max(sprite_size) / (old_sprite_height / 1.2):.3f}")


if __name__ == "__main__":
    main()
