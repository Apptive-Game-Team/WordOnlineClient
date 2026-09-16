#!/usr/bin/env python3
"""Finalize a generated concept image into a production-ready sprite.

Crops to the alpha bounding box (with a small pad for soft edges), fits the
result inside a target box preserving aspect ratio, and trims any transparent
border the resample introduced. Refuses to run on a source that lacks real
alpha rather than matting one out.
"""
import sys
from PIL import Image


def has_real_alpha(im: Image.Image) -> bool:
    if im.mode != "RGBA":
        return False
    alpha = im.split()[-1]
    lo, hi = alpha.getextrema()
    if lo != 0:
        return False
    w, h = im.size
    corners = [im.getpixel((0, 0)), im.getpixel((w - 1, 0)),
               im.getpixel((0, h - 1)), im.getpixel((w - 1, h - 1))]
    if not all(c[3] == 0 for c in corners):
        return False
    return True


def clean_halo(im: Image.Image, threshold: int) -> Image.Image:
    """Cut a soft glow/halo/vignette fringe out of the alpha channel.

    Some generations bake a soft radial glow into the alpha around the solid
    subject instead of true flat transparency. Anything below `threshold` is
    treated as that fringe and forced to 0; the remaining range is remapped
    to 0-255 so the true edge keeps a couple of anti-aliased pixels.
    """
    r, g, b, a = im.split()
    lut = [0 if v < threshold else round((v - threshold) * 255 / (255 - threshold))
           for v in range(256)]
    a = a.point(lut)
    return Image.merge("RGBA", (r, g, b, a))


def main():
    if len(sys.argv) not in (4, 5):
        print("usage: finalize.py <source.png> <out.png> <max_edge> [halo_threshold]")
        sys.exit(1)
    src_path, out_path, max_edge = sys.argv[1], sys.argv[2], int(sys.argv[3])
    halo_threshold = int(sys.argv[4]) if len(sys.argv) == 5 else 0
    im = Image.open(src_path).convert("RGBA")

    if not has_real_alpha(im):
        print(f"REJECT: {src_path} does not have real alpha "
              f"(mode={im.mode}, corners not fully transparent)")
        sys.exit(2)

    if halo_threshold > 0:
        im = clean_halo(im, halo_threshold)

    bbox = im.getbbox()
    if bbox is None:
        print(f"REJECT: {src_path} is fully transparent")
        sys.exit(2)
    pad = 4
    l, t, r, b = bbox
    l = max(0, l - pad)
    t = max(0, t - pad)
    r = min(im.width, r + pad)
    b = min(im.height, b + pad)
    cropped = im.crop((l, t, r, b))

    w, h = cropped.size
    scale = max_edge / max(w, h)
    new_w, new_h = max(1, round(w * scale)), max(1, round(h * scale))
    resized = cropped.resize((new_w, new_h), Image.LANCZOS)

    bbox2 = resized.getbbox()
    final = resized.crop(bbox2) if bbox2 else resized

    final.save(out_path)
    print(f"OK: {out_path} {final.size} {final.mode}")


if __name__ == "__main__":
    main()
