# firework_tower concept exploration

Subject: `PrefabType.FireworkTower`, the FIRE-element building the server added
in `FireworkTowerPrefabInitializer` (see
`/home/yunseong/dev/arcane-casters/game`, branch `feature/521`). It has
`RigidBody`, `CircleCollider`, `DummyMob` (HP, destructible), a
`FireworkLauncher` that lobs a `FireworkShell` on an arc every
`ATTACK_INTERVAL`, `TimedSelfDestroyer`, and `BuildingEffectReceiver`. The
client had neither a sprite nor a prefab for it, so `ObjectSpawner`'s
`Resources.Load<GameObject>($"Prefabs/{type}")` was spawning an empty
GameObject.

`firework_tower` has no lore entry in `WORLD.md` yet, same as its shell. There
is no fire-element faction other than the Hellfire legion in this world, so
this exploration uses the Hellfire legion's material language — dark charred
crust, angular spikes, glowing orange cracks, mass first and flame second —
rather than inventing a new palette. The `firework_shell` concept exploration
(`.art/concept/firework-shell/README.md`, prompts at
`.art/concept/firework-shell/prompts/shared-prefix.txt`) already settled this
magic's visual language for the projectile side; this exploration keeps the
same crust colors, crack-glow colors, and horn motif so the tower and the
shell it launches read as one family.

References used for rendering: `.art/anchors/master-v2/MasterStyleKey.png`
(technique) and `.art/anchors/master-v2/HellfireDemon.png` (Hellfire material
and palette), plus two secondary shape references that are not production
anchors: `.art/concept/hellfire/RallyingTotem-v1.png` (the closest existing
Hellfire *installation* concept — a charred column with a cauldron mouth and
vertical crack seams, still on `PRODUCTION-STATUS.md`'s candidate list, not
yet shipped) and the finalized `FireworkShell.png` sprite from
`origin/feature/588` (for the horn and star-crack motifs the tower's mouth
echoes, so it reads as "the thing that just loaded this shell").
`RallyingTotem-v1.png` and `FireworkShell.png` were used for subject/material
continuity only, never as rendering-technique references — technique comes
only from `MasterStyleKey.png`.

`firework_tower`'s closest existing shipped siblings are `GroundTower` and
`GroundCannon` (both plain `AbstractBuild` sprite-only prefabs with no special
client wiring), which is also why the production prefab duplicates
`GroundCannon.prefab` rather than being built from scratch.

## Candidates

- **`FireworkTower-v1-horned-mortar-source.png`** — squat charred tower
  tapering upward, two pairs of curved horns (echoing `HellfireDemon`) framing
  a wide open mortar bowl at the top with a glowing shell/ember core and star
  crack nested inside (echoing `FireworkShell`), vertical glowing crack seams
  down the shaft (echoing `RallyingTotem`), jagged stepped rock base. Came
  back with a real alpha channel on the first `image_gen` call.
- **`FireworkTower-v2-spiked-brazier.png`** — blocky three-segment drum tower
  with a crown of short jagged spikes around the bowl instead of curved horns,
  and stubby buttress spikes on the sides. Also came back with real alpha on
  the first call, but the spike crown crowds the bowl rim more than v1's two
  clean horn pairs, so its silhouette reads slightly busier at 64px. Concept
  only; not finalized or moved into `Assets/`.

2 of 2 `image_gen` calls in this exploration came back with real alpha on the
first try — both variants used the same four reference images (see
Generation below), well above the roughly 1-in-5 rate `firework_shell` saw. No
chroma-keying or background matting was applied to either candidate.

## Generation

2 total `image_gen` calls, one per variant prompt, each on its first attempt.
Prompts used are under `prompts/`: `shared-prefix.txt` locks rendering
technique (flat polygonal facet planes, one flat color per plane, hard
creases, no gradient/gloss/texture/grain), transparent background, and the
`firework_tower` Hellfire-siege-structure identity; each `variant-*.txt` adds
one paragraph describing that candidate's distinct silhouette. Each call used
the shared prefix concatenated with one variant file, plus `MasterStyleKey.png`,
`HellfireDemon.png`, `RallyingTotem-v1.png`, and `FireworkShell.png` as
reference images.

## Finalization

`FireworkTower-v1-horned-mortar-source.png` was chosen over v2 for its
cleaner two-horn silhouette (see Validation below) and finalized:

```
python finalize.py FireworkTower-v1-horned-mortar-source.png FireworkTower-v1-horned-mortar-256.png --max-dim 256
```

`finalize.py` crops to the alpha bounding box (with a small pad for soft
edges), fits the result inside a 256x256 box preserving aspect ratio, and
trims any transparent border the resample introduced. It refuses to run on a
source that lacks real alpha rather than matting one out.

Sizing rationale: the closest shipped siblings are buildings, not creatures —
`Tower.png` (215x256), `Cannon.png` (256x225), and `FrenzyTotem.png`
(163x256) are all `big` tier (max 256x256 per `STYLE.md`'s tier table) and all
hit 256 on their long axis. `firework_tower` is a chunky standalone structure
like those three, not a small in-flight object like `firework_shell`, so it
uses the `big` tier too. Final export: **181x254 RGBA**, aspect preserved from
the alpha bounding box — close to `FrenzyTotem`'s proportions (both are tall,
narrow Hellfire installations).

## Validation

`FireworkTower-v1-horned-mortar-256.png`:

- Mode: RGBA, size 181x254.
- All four corner pixels: alpha 0.
- Alpha extrema: (0, 255) — a real gradient, not the one-pixel loophole.
- Transparent share of canvas: 0.31 (a single centered subject, in the
  expected "roughly a fifth or more" range).
- Bright-desaturated opaque pixels (a painted-background tell): 5 out of
  31,877 opaque pixels (0.016%), well under the 5% reject threshold.
- `FireworkTower-v1-horned-mortar-chroma.png` composites it over solid
  magenta — no pale fringe, no leftover checkerboard ring.
- `FireworkTower-v1-horned-mortar-64.png` is the 64px silhouette check: the
  paired horns and glowing bowl still read as a distinct dark structure with
  fire showing through it.
- `compare-hellfire-64.png` places the 64px tower next to 64px thumbnails of
  `HellfireDemon.png` (master anchor), the shipped `FrenzyTotem.png`, and the
  `FireworkShell.png` it launches — all four read as the same dark-crust,
  orange-crack material family at a glance, and the tower is clearly a
  distinct silhouette from all three, not a recolor of any of them.

`FireworkTower-v2-spiked-brazier.png` is concept-only: it also has real alpha
(verified: alpha extrema (0, 255), all four corners 0, transparent share 0.61,
bright-desaturated share 0.004%) but was not finalized because its silhouette
is busier at 64px than v1's. It is kept here in case v1 needs revisiting.

## Not run

- `./.art/make-sheets.sh` requires ImageMagick's `magick`, which is not
  installed on this machine (see `.agents/skills/make-game-art/SKILL.md`'s
  environment notes). All resize/trim/alpha/comparison work in this
  exploration used a Pillow virtual environment in the scratchpad instead;
  `compare-hellfire-64.png` in this directory stands in for the sheet-based
  comparison step.
- No Unity Editor session was available in this environment, so no in-scene
  render check (actual PPU, ground contact, HP bar / TTL bar height) was
  possible. The pull request calls this out explicitly.
