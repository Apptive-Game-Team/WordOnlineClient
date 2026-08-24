#!/usr/bin/env python3
"""Compose the lightning cloud sprite and the strike frames on one shared canvas.

Canvas is 320x640 at PPU 160, so a sprite drawn with a centre pivot on an object
standing at server height y=2 covers world y=0..4 exactly: cloud at the top,
bolt running down to the ground.
"""

import argparse
from pathlib import Path

from PIL import Image

CANVAS = (320, 640)
CLOUD_BOX = (312, 240)   # max cloud size inside the canvas
CLOUD_TOP = 4            # px from the canvas top
BOLT_OVERLAP = 24        # px the bolt starts above the cloud underside
BOLT_MAX_WIDTH = 216

# reveal fraction, alpha multiplier, white core mix
FRAMES = [
    (0.38, 1.00, 0.10),
    (0.72, 1.00, 0.10),
    (1.00, 1.00, 0.38),
    (1.00, 0.78, 0.12),
    (1.00, 0.34, 0.00),
]
TIP_FEATHER = 14         # px of alpha ramp at the descending tip


def trimmed(path):
    image = Image.open(path).convert("RGBA")
    bounds = image.getchannel("A").getbbox()
    if bounds is None:
        raise SystemExit(f"{path} is fully transparent")
    return image.crop(bounds)


def fit(image, box):
    image = image.copy()
    image.thumbnail(box, Image.Resampling.LANCZOS)
    return image


def place_cloud(canvas, cloud):
    cloud = fit(cloud, CLOUD_BOX)
    x = (CANVAS[0] - cloud.width) // 2
    canvas.alpha_composite(cloud, (x, CLOUD_TOP))
    return CLOUD_TOP + cloud.height


def scale_bolt(bolt, height):
    width = round(bolt.width * height / bolt.height)
    if width > BOLT_MAX_WIDTH:
        width = BOLT_MAX_WIDTH
    return bolt.resize((width, height), Image.Resampling.LANCZOS)


def reveal(bolt, fraction):
    """Keep the top `fraction` of the bolt, fading out at the descending tip."""
    if fraction >= 1.0:
        return bolt
    bolt = bolt.copy()
    alpha = bolt.getchannel("A")
    pixels = alpha.load()
    cut = round(bolt.height * fraction)
    for y in range(cut, bolt.height):
        ramp = max(0.0, 1.0 - (y - cut) / TIP_FEATHER)
        if ramp <= 0.0:
            for x in range(bolt.width):
                pixels[x, y] = 0
            continue
        for x in range(bolt.width):
            pixels[x, y] = round(pixels[x, y] * ramp)
    bolt.putalpha(alpha)
    return bolt


def styled(bolt, alpha_scale, white_mix):
    bolt = bolt.copy()
    if white_mix > 0.0:
        white = Image.new("RGBA", bolt.size, (255, 255, 255, 255))
        white.putalpha(bolt.getchannel("A"))
        bolt = Image.blend(bolt, white, white_mix)
    if alpha_scale < 1.0:
        alpha = bolt.getchannel("A").point(lambda v: round(v * alpha_scale))
        bolt.putalpha(alpha)
    return bolt


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--cloud", type=Path, required=True)
    parser.add_argument("--bolt", type=Path, required=True)
    parser.add_argument("--cloud-out", type=Path, required=True)
    parser.add_argument("--frame-out", type=Path, required=True,
                        help="frame path pattern containing {index}")
    args = parser.parse_args()

    cloud_source = trimmed(args.cloud)
    bolt_source = trimmed(args.bolt)

    cloud_canvas = Image.new("RGBA", CANVAS, (0, 0, 0, 0))
    cloud_bottom = place_cloud(cloud_canvas, cloud_source)
    args.cloud_out.parent.mkdir(parents=True, exist_ok=True)
    cloud_canvas.save(args.cloud_out, optimize=True)
    print(f"{args.cloud_out.name} {cloud_canvas.width}x{cloud_canvas.height} "
          f"cloud_bottom={cloud_bottom}px")

    bolt_top = max(0, cloud_bottom - BOLT_OVERLAP)
    bolt = scale_bolt(bolt_source, CANVAS[1] - bolt_top)
    bolt_x = (CANVAS[0] - bolt.width) // 2

    for index, (fraction, alpha_scale, white_mix) in enumerate(FRAMES):
        canvas = Image.new("RGBA", CANVAS, (0, 0, 0, 0))
        canvas.alpha_composite(styled(reveal(bolt, fraction), alpha_scale, white_mix),
                               (bolt_x, bolt_top))
        out = Path(str(args.frame_out).format(index=index))
        out.parent.mkdir(parents=True, exist_ok=True)
        canvas.save(out, optimize=True)
        tip = bolt_top + round(bolt.height * fraction)
        print(f"{out.name} {canvas.width}x{canvas.height} reveal={fraction} "
              f"tip={tip}px alpha={alpha_scale} white={white_mix}")


if __name__ == "__main__":
    main()
