#!/usr/bin/env python3
"""Finalize a raw generated sprite into a production-ready transparent PNG.

Crops to the alpha bounding box (with a small pad for soft edges), fits the
result inside a max-size box preserving aspect ratio, and trims any
transparent border the resample introduced. Refuses to run on a source that
lacks real alpha rather than matting one out.
"""
import sys
from PIL import Image


def has_real_alpha(im: Image.Image) -> bool:
    if im.mode != "RGBA":
        return False
    extrema = im.getextrema()[3]
    if extrema[0] == 255:
        return False
    w, h = im.size
    corners = [(0, 0), (w - 1, 0), (0, h - 1), (w - 1, h - 1)]
    corner_alphas = [im.getpixel(c)[3] for c in corners]
    if any(a != 0 for a in corner_alphas):
        return False
    return True


def main():
    if len(sys.argv) != 4:
        print(f"usage: {sys.argv[0]} <input.png> <output.png> <max_size>", file=sys.stderr)
        sys.exit(1)

    src_path, dst_path, max_size = sys.argv[1], sys.argv[2], int(sys.argv[3])
    im = Image.open(src_path).convert("RGBA")

    if not has_real_alpha(im):
        print(f"REJECT: {src_path} has no real alpha (painted background likely)", file=sys.stderr)
        sys.exit(2)

    bbox = im.getbbox()
    if bbox is None:
        print(f"REJECT: {src_path} is fully transparent", file=sys.stderr)
        sys.exit(2)

    pad = 4
    l, t, r, b = bbox
    l = max(0, l - pad)
    t = max(0, t - pad)
    r = min(im.width, r + pad)
    b = min(im.height, b + pad)
    cropped = im.crop((l, t, r, b))

    w, h = cropped.size
    scale = max_size / max(w, h)
    new_w, new_h = max(1, round(w * scale)), max(1, round(h * scale))
    resized = cropped.resize((new_w, new_h), Image.LANCZOS)

    bbox2 = resized.getbbox()
    final = resized.crop(bbox2) if bbox2 else resized

    final.save(dst_path)
    print(f"{dst_path}: {final.size[0]}x{final.size[1]} {final.mode}")


if __name__ == "__main__":
    main()
