# FireShot redesign

Issue: #566

The former projectile was a smooth white-hot flame/comet. The replacement keeps
the same fast right-facing horizontal footprint but uses the HellfireDemon
material language: a dark faceted shell, one broad orange magma seam, and a
short ember-colored rear facet.

Rendering references were `.art/anchors/master-v2/HellfireDemon.png` and
`.art/anchors/master-v2/MasterStyleKey.png`; the legacy sprite was used only to
preserve projectile identity and flight direction.

`source.png` is the generated working source. `finalize.py` crops its alpha and
contains it on the existing 256x88 canvas without changing the Unity `.meta`,
GUID, pivot, or 133.333333 PPU contract. `magenta-review.png` checks for a
painted background or pale fringe, and `FireShot-64.png` is the thumbnail check.

Validation:

- Production canvas: 256x88 RGBA.
- Alpha extrema: 0–255; all four canvas corners alpha 0.
- 64px export: 64x23 with the right-facing shard and magma seam readable.
- The existing `FireShot.prefab` reference remains valid because the PNG GUID
  is preserved in place.
