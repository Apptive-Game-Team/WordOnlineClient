#!/usr/bin/env python3
"""Compose the player's body frames onto one shared production canvas.

`PlayerStaffPoseController` swaps `SpriteRenderer.sprite` between the frames and
touches no transform, so every frame has to sit on one canvas, at one scale, with
the feet on the same row and the standing width around the same column. The
source drawings never come back that way, so one anchor is derived here and
applied to all of them.

The canvas is sized so the player occupies the screen space the shipped
`Assets/Resources/Game/player.png` occupies: that sprite is 192x170 at 100 pixels
per unit with `alignment: 7` (bottom centre), its character is 166px tall, and
its feet sit on the sprite origin. So the output keeps the character 1.66 units
tall and puts the feet on the pivot, and the pixels-per-unit value is picked to
reach that with no resampling of the scale-reference frame.

Two traps this script exists to avoid:

* `Assets/Art/Images/Customize/PlayerCharacterBase.png` was NOT what the game
  drew. `Assets/Resources/Prefabs/Player.prefab` pointed its `PlayerImage`
  renderer at `Assets/Resources/Game/player.png`. Check the `m_Sprite` guid in
  the prefab before believing a file name.
* A head top cannot be found automatically on every frame. The raised frame
  holds a staff above the head, so both a width-share rule and a width-jump rule
  pick the wrong row there. Pass the measured row with `--head-top`.
* Frames drawn in one generation run share a scale; a frame drawn later does
  not, and comes back with the character filling a different share of its
  canvas. Pass `--rescale` for those, and only those — rescaling a frame that
  already matches would resample it for nothing, and would also flatten the
  small pose differences (the thrust crouches 3% lower than the raised stand)
  that belong to the drawing.

Usage:

    finalize-player-frames.py \\
        --frame lowered=src.png:out.png \\
        --frame raised=src.png:out.png \\
        --frame thrust=src.png:out.png \\
        --scale-frame raised --head-top raised=377 \\
        --crystal raised=300,60,520,400 --crystal thrust=950,700,1145,880
"""

import argparse
from pathlib import Path

from PIL import Image

ALPHA_THRESHOLD = 8
# The shipped player.png this output has to match: 100 pixels per unit, a 166px
# character, and the feet on the sprite origin.
OLD_PIXELS_PER_UNIT = 100
OLD_CHARACTER_HEIGHT = 166
OLD_CANVAS = (192, 170)
# The horizontal anchor is the standing width, read from the bottom of the
# character so a raised or thrust staff cannot drag it sideways.
FEET_BAND_SHARE = 0.12
MARGIN = 8


def is_crystal(pixel):
    """The staff crystal's lit core. The cape is blue too, and darker."""
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
        "crystal": cluster_center(image, is_crystal, crystal_window) if crystal_window else None,
    }


def parse_frame(text):
    name, _, paths = text.partition("=")
    source, _, destination = paths.partition(":")
    if not name or not source or not destination:
        raise argparse.ArgumentTypeError(f"expected name=source.png:out.png, got {text!r}")
    return name, Path(source), Path(destination)


def parse_named_int(text):
    name, _, value = text.partition("=")
    return name, int(value)


def parse_named_window(text):
    name, _, value = text.partition("=")
    numbers = tuple(int(part) for part in value.split(","))
    if len(numbers) != 4:
        raise argparse.ArgumentTypeError(f"expected name=x0,y0,x1,y1, got {text!r}")
    return name, numbers


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--frame", type=parse_frame, action="append", required=True,
                        metavar="NAME=SOURCE:OUTPUT")
    parser.add_argument("--scale-frame", required=True,
                        help="frame whose character height sets the pixels-per-unit "
                             "value, so that frame is placed without resampling")
    parser.add_argument("--head-top", type=parse_named_int, action="append", default=[],
                        metavar="NAME=ROW",
                        help="row of the top of the hair; needed wherever something "
                             "is drawn above the head, because the alpha channel "
                             "cannot tell a raised staff from hair")
    parser.add_argument("--rescale", action="append", default=[], metavar="NAME",
                        help="resample this frame so its character height matches "
                             "the scale frame; for frames drawn in a separate run")
    parser.add_argument("--crystal", type=parse_named_window, action="append", default=[],
                        metavar="NAME=X0,Y0,X1,Y1",
                        help="where to look for the staff crystal, for frames whose "
                             "tip carries the element auras")
    args = parser.parse_args()

    head_tops = dict(args.head_top)
    crystals = dict(args.crystal)
    frames = []
    for name, source, destination in args.frame:
        image = Image.open(source).convert("RGBA")
        frames.append((name, image, destination,
                       measure(image, head_tops.get(name), crystals.get(name))))

    scale_metrics = next((m for name, _, _, m in frames if name == args.scale_frame), None)
    if scale_metrics is None:
        raise SystemExit(f"--scale-frame {args.scale_frame!r} is not one of the frames")

    target_character_height = scale_metrics["character_height"]
    for index, (name, image, destination, metrics) in enumerate(frames):
        if name not in args.rescale:
            continue
        factor = target_character_height / metrics["character_height"]
        resized = image.resize((max(1, round(image.width * factor)),
                                max(1, round(image.height * factor))),
                               Image.Resampling.LANCZOS)
        head_top = round(metrics["head_top"] * factor) if head_tops.get(name) is not None else None
        window = crystals.get(name)
        if window is not None:
            window = tuple(round(value * factor) for value in window)
        frames[index] = (name, resized, destination, measure(resized, head_top, window))
        print(f"{name:8} rescaled x{factor:.4f} to match {args.scale_frame}")

    # No resampling: the pixels-per-unit value is derived from the scale frame so
    # its character already has the size it needs at scale 1.
    pixels_per_unit = round(
        scale_metrics["character_height"] * OLD_PIXELS_PER_UNIT / OLD_CHARACTER_HEIGHT)

    lefts, rights, tops = [], [], []
    for _, _, _, metrics in frames:
        x0, y0, x1, _ = metrics["bbox"]
        lefts.append(x0 - metrics["feet_center"])
        rights.append(x1 - metrics["feet_center"])
        tops.append(y0 - metrics["feet_y"])

    anchor_x = round(-min(lefts)) + MARGIN
    anchor_y = round(-min(tops)) + MARGIN
    canvas_width = anchor_x + round(max(rights)) + MARGIN + 1
    canvas_height = anchor_y + MARGIN + 1

    for name, image, destination, metrics in frames:
        canvas = Image.new("RGBA", (canvas_width, canvas_height), (0, 0, 0, 0))
        offset = (anchor_x - round(metrics["feet_center"]), anchor_y - metrics["feet_y"])
        canvas.alpha_composite(image, offset)
        destination.parent.mkdir(parents=True, exist_ok=True)
        canvas.save(destination, optimize=True)
        metrics["offset"] = offset
        print(f"{name:8} {destination.name} placed at {offset} "
              f"head_top={metrics['head_top']} feet={metrics['feet_y']} "
              f"character={metrics['character_height']}px "
              f"feet_center={metrics['feet_center']}")

    def to_local(point, metrics):
        x = (point[0] + metrics["offset"][0] - anchor_x) / pixels_per_unit
        y = (anchor_y - (point[1] + metrics["offset"][1])) / pixels_per_unit
        return round(x, 3), round(y, 3)

    sprite_size = (round(canvas_width / pixels_per_unit, 3),
                   round(canvas_height / pixels_per_unit, 3))
    old_object_size = max(OLD_CANVAS) / OLD_PIXELS_PER_UNIT

    print()
    print(f"canvas {canvas_width}x{canvas_height}")
    print(f"spritePixelsToUnits: {pixels_per_unit}")
    print(f"alignment: 9  spritePivot: {{x: {anchor_x / canvas_width:.4f}, "
          f"y: {(canvas_height - anchor_y) / canvas_height:.4f}}}")
    print(f"character height {scale_metrics['character_height'] / pixels_per_unit:.3f} units "
          f"(player.png: {OLD_CHARACTER_HEIGHT / OLD_PIXELS_PER_UNIT})")
    print(f"m_Size: {{x: {sprite_size[0]}, y: {sprite_size[1]}}}")
    for name, _, _, metrics in frames:
        if metrics["crystal"] is not None:
            print(f"StaffAura localPosition for {name}: {to_local(metrics['crystal'], metrics)}")
    print(f"_effectScaleReferenceHeight to hold today's effect size: "
          f"{max(sprite_size) / (old_object_size / 1.2):.3f}")


if __name__ == "__main__":
    main()
