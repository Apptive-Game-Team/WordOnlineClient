"""Renders .art/concept/repair-totem/aura-preview.png.

No Unity Editor can open this checkout from WSL, so the RepairAura child added to
RepairTotem.prefab is checked by reproducing what the camera does to it instead of by
looking at the scene. GameScene.unity's camera is rotated 45 degrees about X, and a
ServedObject's sprite carries no rotation, so a sprite in the world XY plane is squashed
vertically by cos45 and left alone horizontally. That is the whole projection.

Run with a Pillow virtualenv; ImageMagick is not installed in this environment.
"""

import math

from PIL import Image, ImageDraw

TILT = math.cos(math.radians(45))
RADIUS = 4.0                                # repair_totem.radius, V079 migration
TOTEM_WIDTH, TOTEM_HEIGHT = 99 / 133.333333, 235 / 133.333333
GROUND = (203, 255, 234, 255)
INK = (30, 60, 45, 255)

ring = Image.open("Assets/Art/Images/Effect/Aura/nature_aura.png").convert("RGBA")
totem = Image.open("Assets/Resources/Game/sprites/RepairTotem.png").convert("RGBA")


def fade(image, alpha):
    faded = image.copy()
    faded.putalpha(image.getchannel("A").point(lambda value: int(value * alpha)))
    return faded


def world_sprite(image, world_width, world_height, pixels_per_unit):
    return image.resize(
        (max(1, round(world_width * pixels_per_unit)),
         max(1, round(world_height * pixels_per_unit * TILT))),
        Image.LANCZOS)


def draw_totem(panel, x, y, pixels_per_unit, alpha):
    aura = world_sprite(fade(ring, alpha), 2 * RADIUS, 2 * RADIUS, pixels_per_unit)
    panel.alpha_composite(aura, (x - aura.width // 2, y - aura.height // 2))
    body = world_sprite(totem, TOTEM_WIDTH, TOTEM_HEIGHT, pixels_per_unit)
    panel.alpha_composite(body, (x - body.width // 2, y - body.height // 2))


# The two ends of IdleAuraEffect's alpha tween: 0.5 on the SpriteRenderer, times a
# minAlphaMultiplier of 0.65 at the trough.
CLOSE_SCALE, CLOSE_WIDTH, CLOSE_HEIGHT = 78, 700, 520
close_up = Image.new("RGBA", (CLOSE_WIDTH * 2 + 8, CLOSE_HEIGHT), (255, 255, 255, 255))
for index, (alpha, caption) in enumerate(
        [(0.5, "pulse peak  alpha 0.50"), (0.325, "pulse trough  alpha 0.50 x 0.65")]):
    panel = Image.new("RGBA", (CLOSE_WIDTH, CLOSE_HEIGHT), GROUND)
    draw_totem(panel, CLOSE_WIDTH // 2, CLOSE_HEIGHT // 2 + 30, CLOSE_SCALE, alpha)
    ImageDraw.Draw(panel).text((16, 14), caption, fill=INK)
    close_up.alpha_composite(panel, (index * (CLOSE_WIDTH + 8), 0))

# The same aura on the whole playfield. MagicInputHandler clamps casts to X 0..18 and
# Z 0..10, and the ground is the world XZ plane, so world Z maps to screen Y by sin45.
FIELD_SCALE = 46
field_width, field_height = round(18 * FIELD_SCALE), round(10 * FIELD_SCALE * TILT)
field = Image.new("RGBA", (field_width + 2, field_height + 60), (255, 255, 255, 255))
ground = Image.new("RGBA", (field_width, field_height), GROUND)
grid = ImageDraw.Draw(ground)
for grid_x in range(0, 19, 2):
    grid.line([(grid_x * FIELD_SCALE, 0), (grid_x * FIELD_SCALE, field_height)],
              fill=(170, 225, 200, 255))
for grid_z in range(0, 11, 2):
    grid.line([(0, grid_z * FIELD_SCALE * TILT), (field_width, grid_z * FIELD_SCALE * TILT)],
              fill=(170, 225, 200, 255))
draw_totem(ground, round(6.0 * FIELD_SCALE), round(5.0 * FIELD_SCALE * TILT), FIELD_SCALE, 0.5)
field.alpha_composite(ground, (1, 50))
ImageDraw.Draw(field).text((8, 16), "the same aura on the full 18 x 10 field, 2-unit grid",
                           fill=INK)

sheet = Image.new("RGBA",
                  (max(close_up.width, field.width), close_up.height + field.height + 8),
                  (255, 255, 255, 255))
sheet.alpha_composite(close_up, (0, 0))
sheet.alpha_composite(field, (0, close_up.height + 8))
sheet.convert("RGB").save(".art/concept/repair-totem/aura-preview.png")
