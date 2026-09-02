# MagmaExplosion redesign

Issue: #564

The former sprite was flame-first: painterly fire, smoke-like edges, and many
small details. The replacement keeps the same compact eruption identity but
uses the HellfireDemon material language: a 5–7 piece dark basalt shell, broad
facets, and a restrained orange/cream glow visible through the gaps.

References used for rendering were `.art/anchors/master-v2/HellfireDemon.png`
and `.art/anchors/master-v2/MasterStyleKey.png`. The legacy sprite was used only
to preserve the subject identity and footprint.

`source.png` is the generated working source. `finalize.py` crops its alpha and
contains it on the existing 217x256 canvas without changing the Unity `.meta`,
GUID, pivot, or 100 PPU contract. `magenta-review.png` exposes pale fringes or
painted backgrounds; `MagmaExplosion-64.png` is the thumbnail check. The four
sequence sources are `frame-01-source.png` through `frame-04-source.png`; their
217x256 exports are connected by `SpriteFrameAnimator` at 0.1 seconds per frame
with looping disabled.

Validation:

- Production canvas: 217x256 RGBA.
- Alpha extrema: 0–255; all four canvas corners alpha 0.
- 64px export: 54x64 with the main shell and central glow still readable.
- No Unity Editor session was available for an in-scene render check.
