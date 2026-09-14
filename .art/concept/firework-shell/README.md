# firework_shell concept exploration

Subject: the fire-element shell that `firework_tower` launches (see the
`FireworkShell` prefab at `Assets/Resources/Projectiles/FireworkShell.prefab`,
currently a placeholder wearing `FireShot.png`). It flies a roughly-3-unit
parabola after launch and explodes on landing.

`firework_tower` has no lore entry in `WORLD.md` yet. There is no fire-element
faction other than the Hellfire legion in this world, and `FireShot.png` is
already on `STYLE.md`'s Hellfire redesign list ("지옥불 군단의 뿔·갑각 모티프로
통일"), so these candidates use the Hellfire legion's material language —
dark charred crust, angular spikes, glowing orange cracks, mass first and
flame second — rather than inventing a new palette.

`ProjectileUtil.GetRotation` aims the sprite along the straight start-to-end
screen-space line and never updates it during flight (see
`Assets/Scripts/GameScene/Object/Projectile/ProjectileUtil.cs`), so all three
candidates are designed to read as "a shell in flight" from a single fixed
facing rather than relying on rotation to sell the arc: each has a clear nose
pointed in the direction of travel (right) and a short trailing flame/ember
tail behind it, not a curved streak.

References used for rendering: `.art/anchors/master-v2/MasterStyleKey.png`
(technique) and `.art/anchors/master-v2/HellfireDemon.png` (Hellfire material
and palette). `FireShot.png` was inspected for scale/role only, never as a
rendering reference — it is a painterly gradient flame sprite in a completely
different technique from the master-v2 faceted-papercraft style, which is
exactly why it is on the redesign list.

## Candidates

- **`FireworkShell-v1-spiked-pod.png`** — compact egg-shaped shell with 3-4
  jagged spike plates and a star-shaped glowing crack, short blocky ember
  trail. Reads as a spiky seed pod / mine.
- **`FireworkShell-v2-finned-rocket.png`** — elongated conical shell with a
  pointed nose, tail fins, and 3 small flame flags trailing straight back.
  Reads most unambiguously as "artillery shell in flight" of the three.
- **`FireworkShell-v3-horned-bomb-source.png`** — round bomb shell with four
  curved horn spikes (echoing `HellfireDemon`'s horns) and one bright
  star-shaped crack on the front face, tight ember trail. This is the only
  candidate that came back with a real alpha channel; see Validation below.

The 21 raw generations behind these three — 10 tries for the spiked-pod
prompt, 8 for finned-rocket, 3 for horned-bomb — are deliberately not committed.
They are 36 MB, and no other directory under `.art/concept/` keeps its rejects,
so carrying them would more than double what this repository stores for concept
art. The one attempt that mattered, the third horned-bomb try, is the only
generation that came back with a real alpha channel and is kept unmodified as
`FireworkShell-v3-horned-bomb-source.png`. The prompts under `prompts/`
reproduce all three.

## Generation

21 total `image_gen` calls across the three prompts (10 spiked-pod, 8
finned-rocket, 3 horned-bomb — the horned-bomb prompt hit real alpha on its
third try, so it stopped there while the other two kept retrying). 1 of 21
came back with a real alpha channel — worse than the roughly 1-in-5 rate this
project usually sees, within normal variance for a small sample. No
chroma-keying or background matting was
applied to any candidate; the checkerboard-painted ones are kept exactly as
generated and are not production-ready.

Prompts used are under `prompts/`: `shared-prefix.txt` locks rendering
technique (flat polygonal facet planes, one flat color per plane, hard
creases, no gradient/gloss/texture/grain, "if it could be mistaken for a
rendered 3D model or a digital painting, it is wrong"), transparent
background, and the Hellfire shell identity; each `variant-*.txt` adds one
paragraph describing that candidate's distinct silhouette. Each `image_gen`
call used the shared prefix concatenated with one variant file, plus
`MasterStyleKey.png` and `HellfireDemon.png` as reference images.

## Finalization

Only `FireworkShell-v3-horned-bomb-source.png` has real alpha, so it is the
only candidate finalized to a production canvas:

```
python finalize.py FireworkShell-v3-horned-bomb-source.png FireworkShell-v3-horned-bomb-128.png
```

`finalize.py` crops to the alpha bounding box (with a small pad for soft
edges), fits the result inside a 128x128 box preserving aspect ratio, and
trims any transparent border the resample introduced. It refuses to run on a
source that lacks real alpha rather than matting one out.

Sizing rationale: the existing shot-family sprites
(`Assets/Resources/Game/shoot/water_shoot.png` 192x61,
`Assets/Resources/Game/shoot/leaf_shoot.png` 192x74,
`Assets/Resources/Game/shoot/lightning_shoot.png` 192x51, and
`Assets/Resources/Game/sprites/FireShot.png` 256x88) are all long, thin flame
streaks — most of the family's long axis is 192px. `firework_shell` is a
discrete rounded object rather than a streak (alpha bbox aspect ratio ~1.18,
nearly square), so fitting it to the family's *long axis* would make it look
oversized relative to how little of its own canvas a streak sprite fills.
Its role — a small single in-flight projectile, not a creature — puts it in
the `small` tier (max 128x128) from `STYLE.md`'s tier table, which is the
better fit for a compact, roughly-square silhouette. Final export:
**128x109 RGBA**, aspect preserved from the alpha bounding box.

## Validation

`FireworkShell-v3-horned-bomb-128.png`:

- Mode: RGBA, size 128x109.
- All four corner pixels: alpha 0.
- Alpha extrema: (0, 255) — a real gradient, not the one-pixel loophole.
- Transparent share of canvas: 0.48 (a single centered subject, not a tiling
  strip, so this is in the expected "roughly a fifth or more" range).
- Bright-desaturated opaque pixels (a painted-background tell): 0 out of
  6815 opaque pixels.
- `FireworkShell-v3-horned-bomb-chroma.png` composites it over solid magenta
  — no pale fringe, no leftover checkerboard ring.
- `FireworkShell-v3-horned-bomb-64.png` is the 64px silhouette check: the two
  horn spikes and the bright crack still read as a discrete dark shell.
- `compare-fireshot-vs-shell-64.png` places the 64px shell next to a 64px
  `FireShot.png` thumbnail — a thin bright streak next to a chunky dark
  spiked shell, clearly two different things at a glance.

`FireworkShell-v1-spiked-pod.png` and `FireworkShell-v2-finned-rocket.png` are
concept-only: both are opaque RGB with the generator's painted checkerboard,
not a real cutout, so none of the above validation was run on them and they
are not ready to finalize or move into `Assets/`. If either design is
preferred over the horned-bomb, it needs more `image_gen` retries (or the
`OPENAI_API_KEY` + `gpt-image-1.5 --background transparent` fallback
mentioned in `.agents/skills/make-game-art/SKILL.md`'s lineage of lessons)
before it can be finalized the same way.

## Not run

- `./.art/make-sheets.sh` was run; see its output for the concept sheet
  (`.art/sheets/concept.png`).
- No Unity Editor session was available in this environment, so no in-scene
  render check (actual PPU, ground contact, or in-flight rotation) was
  possible. That remains for whoever moves an approved candidate into
  `Assets/`.
