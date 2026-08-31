from collections import deque
from pathlib import Path
import sys

import numpy as np
from PIL import Image


def clear_connected_neutral_background(image: Image.Image) -> Image.Image:
    rgba = np.asarray(image.convert("RGBA")).copy()
    rgb = rgba[:, :, :3].astype(np.int16)
    maximum = rgb.max(axis=2)
    minimum = rgb.min(axis=2)
    neutral = (maximum - minimum <= 16) & (minimum >= 220)

    height, width = neutral.shape
    outside = np.zeros((height, width), dtype=bool)
    queue: deque[tuple[int, int]] = deque()
    for x in range(width):
        queue.append((0, x))
        queue.append((height - 1, x))
    for y in range(height):
        queue.append((y, 0))
        queue.append((y, width - 1))

    while queue:
        y, x = queue.popleft()
        if outside[y, x] or not neutral[y, x]:
            continue
        outside[y, x] = True
        if y:
            queue.append((y - 1, x))
        if y + 1 < height:
            queue.append((y + 1, x))
        if x:
            queue.append((y, x - 1))
        if x + 1 < width:
            queue.append((y, x + 1))

    rgba[outside, 3] = 0
    return Image.fromarray(rgba, "RGBA")


def fit_sprite(source: Path, destination: Path, width: int, height: int, remove_checker: bool) -> None:
    image = Image.open(source).convert("RGBA")
    if remove_checker:
        image = clear_connected_neutral_background(image)

    alpha = image.getchannel("A")
    bounds = alpha.getbbox()
    if bounds is None:
        raise ValueError(f"{source} has no visible pixels")

    cropped = image.crop(bounds)
    margin = max(8, min(width, height) // 32)
    available_width = width - margin * 2
    available_height = height - margin * 2
    scale = min(available_width / cropped.width, available_height / cropped.height)
    resized = cropped.resize(
        (max(1, round(cropped.width * scale)), max(1, round(cropped.height * scale))),
        Image.Resampling.LANCZOS,
    )
    canvas = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    x = (width - resized.width) // 2
    y = height - margin - resized.height
    canvas.alpha_composite(resized, (x, y))
    destination.parent.mkdir(parents=True, exist_ok=True)
    canvas.save(destination, optimize=True)


if __name__ == "__main__":
    if len(sys.argv) != 6:
        raise SystemExit("usage: script.py SOURCE DESTINATION WIDTH HEIGHT REMOVE_CHECKER")
    fit_sprite(
        Path(sys.argv[1]),
        Path(sys.argv[2]),
        int(sys.argv[3]),
        int(sys.argv[4]),
        sys.argv[5] == "1",
    )
