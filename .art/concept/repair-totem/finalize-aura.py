#!/usr/bin/env python3
"""Finalize a generated aura ring into a square production PNG.

Differs from finalize.py in one way that matters: the output canvas is square.
AuraRadiusScaler sizes the sprite by half its rect on each axis, so a square
canvas holding a round ring makes the drawn circle round in world units and
keeps the prefab's localScale uniform. A tight trim would hand back whatever
aspect the generator happened to draw.

Refuses a source without real alpha rather than matting one out, and reports the
two numbers the brief is checked against: how round the ring is, and how thick
its band is as a share of the diameter.
"""
import sys

from PIL import Image


def has_real_alpha(im):
    if im.mode != "RGBA":
        return False, f"mode is {im.mode}, not RGBA"
    if im.getextrema()[3][0] == 255:
        return False, "no pixel is transparent"
    width, height = im.size
    corners = [(0, 0), (width - 1, 0), (0, height - 1), (width - 1, height - 1)]
    alphas = [im.getpixel(corner)[3] for corner in corners]
    if any(alpha != 0 for alpha in alphas):
        return False, f"corner alphas are {alphas}, expected all 0"
    return True, ""


def band_share(im):
    """Band thickness over diameter, measured across the ring's horizontal centre."""
    alpha = im.getchannel("A").load()
    y = im.height // 2
    opaque = [x for x in range(im.width) if alpha[x, y] > 8]
    if not opaque:
        return None
    runs, start = [], opaque[0]
    for previous, current in zip(opaque, opaque[1:]):
        if current != previous + 1:
            runs.append((start, previous))
            start = current
    runs.append((start, opaque[-1]))
    diameter = opaque[-1] - opaque[0] + 1
    thickest = max(end - begin + 1 for begin, end in runs)
    return thickest / diameter, len(runs)


def main():
    if len(sys.argv) != 4:
        print(f"usage: {sys.argv[0]} <input.png> <output.png> <size>", file=sys.stderr)
        sys.exit(1)

    src_path, dst_path, size = sys.argv[1], sys.argv[2], int(sys.argv[3])
    im = Image.open(src_path).convert("RGBA")

    ok, reason = has_real_alpha(im)
    if not ok:
        print(f"REJECT: {src_path} has no real alpha ({reason})", file=sys.stderr)
        sys.exit(2)

    bbox = im.getbbox()
    if bbox is None:
        print(f"REJECT: {src_path} is fully transparent", file=sys.stderr)
        sys.exit(2)

    cropped = im.crop(bbox)
    width, height = cropped.size
    print(f"source ring bbox {width}x{height}, aspect {width / height:.3f}")

    # Pad the shorter axis symmetrically so the ring stays centred on a square canvas.
    side = max(width, height)
    square = Image.new("RGBA", (side, side), (0, 0, 0, 0))
    square.alpha_composite(cropped, ((side - width) // 2, (side - height) // 2))

    final = square.resize((size, size), Image.LANCZOS)
    final.save(dst_path)

    measured = band_share(final)
    if measured is not None:
        share, runs = measured
        print(f"band is {share:.1%} of the diameter, {runs} opaque runs across the centre line")
    transparent = sum(1 for a in final.getchannel("A").getdata() if a == 0)
    print(f"{dst_path}: {final.size[0]}x{final.size[1]} {final.mode}, "
          f"{transparent / (size * size):.1%} fully transparent")


if __name__ == "__main__":
    main()
