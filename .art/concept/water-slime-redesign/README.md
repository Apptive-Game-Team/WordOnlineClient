# Water Slime Swarm redesign

Issue: #560

## Why it changed

The previous production sprite used a glossy mobile-illustration treatment:
strong outlines, many small bubbles, smooth gradients, and specular highlights.
Those choices conflicted with the project's large matte cut-paper shapes and
three-band value structure.

## Art direction

- Preserve the water-slime identity and make the `Swarm` name literal with
  one leader and two overlapping companions.
- Use `.art/anchors/master-v2/WaterSlime.png` for gel construction and
  `.art/anchors/master-v2/MasterStyleKey.png` for the project-wide rendering
  language.
- Keep broad cyan facets, restrained internal highlights, simple oval eyes,
  no outer contour, and no spirit-like wisps.

## Files

- `source.png`: AI-generated working source. It is not loaded by Unity.
- `finalize.ps1`: deterministic crop, contain, resize, and transparent-canvas
  export used for the production sprite.
- `preview.png`: 512x448 review render using the production framing.
- `WaterSlimeSwarm-64.png`: tight 64 px readability check.
- `WaterSlimeSwarm-tight.png`: tight alpha-bound inspection render.

Production mapping:

- `WaterSlimeSwarm.png`: three-character summon-magic artwork, 256x224.
- `WaterSlime.png`: one summoned creature in its base pose, 320x224.
- `WaterSlimeAttackSpit.png`: the same creature's spit pose, 320x224.

The individual pair uses 200 PPU, Bottom Center pivots, one shared source-pixel
scale, and one shared bottom baseline. `WaterSlime.prefab` now uses the new
single-character base sprite while retaining the existing attack swap.

The built-in generator returned a painted checkerboard twice for the base pose
and once for the attack pose. `finalize_pair.py` therefore removes only bright,
near-grey background pixels flood-connected to a canvas edge. This is a
controlled fallback rather than chroma-keying arbitrary internal pixels. The
magenta review image is retained so pale fringe or a missed checker tile is
visible during review.

## Validation

- Production canvas: 256x224 RGBA.
- Tight visible bounds: 237x208.
- Alpha extrema: 0-255.
- All four tight-preview corners: alpha 0.
- The three-body silhouette and eyes remain distinct at 64 px.
- Individual base and attack frames: 320x224 RGBA, shared 0.248021 source scale.
- Individual frame bottom gap: 8 px in both frames.
- `pair-magenta-review.png` shows no retained checkerboard rectangle.
