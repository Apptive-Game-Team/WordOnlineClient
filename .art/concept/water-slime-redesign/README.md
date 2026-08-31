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

The production result is
`Assets/Resources/Game/sprites/WaterSlimeSwarm.png` at 256x224. Its existing
Unity `.meta`, GUID, pivot, and 200 pixels-per-unit contract are unchanged.

## Validation

- Production canvas: 256x224 RGBA.
- Tight visible bounds: 237x208.
- Alpha extrema: 0-255.
- All four tight-preview corners: alpha 0.
- The three-body silhouette and eyes remain distinct at 64 px.
