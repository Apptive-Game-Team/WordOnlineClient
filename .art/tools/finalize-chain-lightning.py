from pathlib import Path

from PIL import Image, ImageFilter


SOURCE = Path(".art/concept/chain-lightning/chain-lightning-v1-source.png")
OUTPUT = Path("Assets/Resources/Game/sprites/ChainLightning.png")
REVIEW = Path(".art/sheets/chain-lightning-review.png")


def alpha_from_chroma(image: Image.Image) -> Image.Image:
    rgb = image.convert("RGB")
    chroma = Image.new("L", rgb.size)
    pixels = getattr(rgb, "get_flattened_data", rgb.getdata)()
    chroma.putdata([max(pixel) - min(pixel) for pixel in pixels])

    # Generated background is a painted neutral checkerboard. Preserve colored
    # lightning edges while rejecting neutral squares and paper wrinkles.
    support = chroma.point(lambda value: 255 if value >= 10 else 0)
    support = support.filter(ImageFilter.MaxFilter(7))
    alpha = chroma.point(
        lambda value: 0 if value <= 3 else 255 if value >= 22 else (value - 3) * 255 // 19
    )
    return Image.composite(alpha, Image.new("L", rgb.size), support)


def main() -> None:
    source = Image.open(SOURCE).convert("RGBA")
    source.putalpha(alpha_from_chroma(source))
    bbox = source.getchannel("A").getbbox()
    if bbox is None:
        raise RuntimeError("No foreground remained after background extraction")

    left, top, right, bottom = bbox
    padding = 8
    cropped = source.crop(
        (
            max(0, left - padding),
            max(0, top - padding),
            min(source.width, right + padding),
            min(source.height, bottom + padding),
        )
    )
    cropped.thumbnail((256, 256), Image.Resampling.LANCZOS)
    cropped.save(OUTPUT, optimize=True)

    preview = cropped.copy()
    preview.thumbnail((64, 64), Image.Resampling.LANCZOS)
    review = Image.new("RGBA", (384, 192), (255, 0, 255, 255))
    review.alpha_composite(cropped, ((256 - cropped.width) // 2, (192 - cropped.height) // 2))
    review.alpha_composite(preview, (288 + (64 - preview.width) // 2, 64 + (64 - preview.height) // 2))
    REVIEW.parent.mkdir(parents=True, exist_ok=True)
    review.convert("RGB").save(REVIEW, optimize=True)

    alpha = cropped.getchannel("A")
    histogram = alpha.histogram()
    corners = [
        alpha.getpixel((0, 0)),
        alpha.getpixel((cropped.width - 1, 0)),
        alpha.getpixel((0, cropped.height - 1)),
        alpha.getpixel((cropped.width - 1, cropped.height - 1)),
    ]
    print(f"output={cropped.size} mode={cropped.mode}")
    print(f"alpha_extrema={alpha.getextrema()} corners={corners}")
    print(f"transparent_ratio={histogram[0] / (cropped.width * cropped.height):.3f}")


if __name__ == "__main__":
    main()
