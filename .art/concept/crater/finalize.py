from pathlib import Path
from PIL import Image

root = Path(__file__).parent
source = Image.open(root / "source.png").convert("RGBA")
target_size = (256, 169)
scaled = source.resize(target_size, Image.Resampling.LANCZOS)

# The generator returned a clean alpha cutout for this pass. Keep only the
# resize and a lossless export so the shipped sprite preserves that edge.
scaled.save(root / "Crater.png", optimize=True)

review = Image.new("RGBA", target_size, (220, 32, 150, 255))
review.alpha_composite(scaled)
review.save(root / "review-magenta.png", optimize=True)

corners = [scaled.getpixel(p)[3] for p in ((0, 0), (255, 0), (0, 168), (255, 168))]
assert corners == [0, 0, 0, 0], corners
print("saved", root / "Crater.png", "size", scaled.size, "corners", corners)
