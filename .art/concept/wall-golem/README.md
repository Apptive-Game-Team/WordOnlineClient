# wall_golem concept exploration

Subject: the `wall_golem` magic (`PrefabType.WallGolem`, server component
`WallGolemMagic` / `WallGolemPrefabInitializer` on `feature/521` in the sibling
`game` repository). The server already ships this unit — `RigidBody`,
`ZPhysics`, `CircleCollider`, `RockGolemMob` (melee, `TargetMask.GROUND` only,
stats from parameters), `RockDeathRemnant`, `CommonEffectReceiver`, element
`ROCK` — but the client has no sprite or prefab for it, so
`ObjectSpawner.InstantiateGameObject`'s `Resources.Load<GameObject>("Prefabs/WallGolem")`
returns nothing and an invisible object spawns.

`WallGolemPrefabInitializer` is structurally identical to
`RockGolemPrefabInitializer` — same components in the same order, only the
parameter key and `PrefabType` differ — so this is a top-tier stat variant of
`RockGolem` within the same faction, not a new archetype. `WORLD.md` has no
separate lore entry for a "wall golem"; it belongs to 돌 골렘 부족 (the rock
golem tribe), the same faction as `RockGolem` and `RockMage`, so these
candidates reuse that faction's established material language rather than
inventing one.

References used for rendering: `.art/anchors/master-v2/MasterStyleKey.png`
(technique) and `.art/anchors/master-v2/RockGolem.png` (rock golem tribe
material — blocky stacked stone, visible block seams, moss growing on the
stone, small head, tan `#CDC8B8`/`#A3A29D`/`#8A857A` and moss
`#55572D`/`#374725`). `.art/concept/rock-golems-lineup.png` was inspected for
lore/silhouette ideas only, never as a rendering reference — it predates the
selected master-v2 technique and is a painterly render, not flat facets.

Since `RockGolemMob` is a melee unit, the design goal was to keep the tribe's
material identity while giving `WallGolem` a silhouette that reads as
"top-tier tanker" and is not confusable with `RockGolem` at a glance — the
name calls for a broader, flatter, wall-like body rather than another rounded
ape-shaped torso.

## Candidates

- **`WallGolem-v1-slab-shoulders-opaque.png`** — two oversized flat slab
  plates on the shoulders/upper back, squared off like battlement merlons,
  with a narrower torso and a small head peeking out between them. Reads as
  "a short stretch of fortress wall standing on two legs." Kept only as a
  concept-only opaque PNG (see Validation below) — not finalized.
- **`WallGolem-v2-monolith-body-256.png`** — the entire torso is one flat
  rectangular slab of stacked stone blocks, flat front and top, square
  shoulders, with a small head recessed into the top edge and short stubby
  limbs. Reads unambiguously as "an upright stone wall segment with legs."
  This is the only candidate that came back with a real alpha channel and is
  the finalized, production-ready sprite.

The prompts under `prompts/` reproduce both: `shared-prefix.txt` locks
rendering technique (flat polygonal facet planes, hard creases, no
gradient/gloss/texture, three-quarter camera facing right, "if it could be
mistaken for a rendered 3D model or a digital painting, it is wrong"),
transparent background, and the shared rock-golem-tribe identity and palette;
`variant-a-slab-shoulders.txt` and `variant-b-monolith-body.txt` each add one
paragraph describing that candidate's distinct silhouette. Each `image_gen`
call used the shared prefix concatenated with one variant file, plus
`MasterStyleKey.png` and `RockGolem.png` as reference images.

## Generation

4 total `image_gen` calls: 3 tries for the slab-shoulders prompt (all three
came back fully opaque with a painted checkerboard background), 1 try for the
monolith-body prompt (real alpha on the first attempt). 1 of 4 came back with
a real alpha channel — in line with the roughly 1-in-5 rate this project
usually sees. No chroma-keying or background matting was applied to either
candidate; the opaque slab-shoulders attempt is kept exactly as generated and
is not production-ready. Its other two opaque tries were discarded rather than
kept — they are near-duplicates of the kept one and would only add bytes
without adding information.

## Finalization

Only `WallGolem-v2-monolith-body-source.png` has real alpha, so it is the only
candidate finalized to a production canvas:

```
python finalize.py WallGolem-v2-monolith-body-source.png WallGolem-v2-monolith-body-256.png --max-dim 256
```

`finalize.py` crops to the alpha bounding box (with a small pad for soft
edges), fits the result inside a 256x256 box preserving aspect ratio, and
trims any transparent border the resample introduced. It refuses to run on a
source that lacks real alpha rather than matting one out.

Sizing rationale: `RockGolem.png`, the closest sibling and the unit
`WallGolemPrefabInitializer` shares every component with, ships at 256x244 —
the `big` tier ceiling from `STYLE.md`'s tier table (`small` 128, `middle`
192, `big` 256). `WallGolem` is a top-tier variant of the same creature, so it
belongs in the same tier. Final export: **226x254 RGBA**, aspect preserved
from the alpha bounding box.

## Validation

`WallGolem-v2-monolith-body-256.png` (moved into
`Assets/Resources/Game/sprites/WallGolem.png`):

- Mode: RGBA, size 226x254.
- All four corner pixels: alpha 0.
- Alpha extrema: (0, 255) — a real gradient, not the one-pixel loophole.
- Alpha bbox: `(0, 0, 226, 254)` — fills the trimmed canvas, confirming the
  crop was tight.
- Transparent share of canvas: 0.31 (a single centered subject, in the
  expected "roughly a fifth or more" range from the skill's cutout check).
- Bright-desaturated opaque pixels (a painted-background tell): 2 out of
  38550 opaque pixels (0.005%) — negligible, almost certainly anti-aliased
  edge pixels, well under the skill's 5% reject threshold.
- `WallGolem-chroma.png` composites it over solid magenta — no pale fringe,
  no leftover checkerboard ring.
- `WallGolem-64-review.png` is the 64px silhouette check: the blocky
  stacked-stone torso, moss patches, and small head still read as a discrete
  golem at that size.
- `compare-master-rockgolem-wallgolem.png` places `MasterStyleKey`,
  `RockGolem`, and `WallGolem` at the same rendered height: the technique
  (flat facets, three value bands, no outline, oval eyes, upper-left light)
  and the tribe palette (tan stone + moss) match across all three, while
  `WallGolem`'s flat rectangular block-stack torso is clearly distinct from
  `RockGolem`'s rounded ape-shaped body at a glance.

`WallGolem-v1-slab-shoulders-opaque.png` is concept-only: it is opaque RGB
with the generator's painted checkerboard, not a real cutout, so none of the
above validation was run on it and it is not ready to finalize or move into
`Assets/`. If the shoulder-slab silhouette is preferred over the monolith body
later, it needs more `image_gen` retries before it can be finalized the same
way.

## Prefab

`Assets/Resources/Prefabs/WallGolem.prefab` duplicates
`Assets/Resources/Prefabs/RockGolem.prefab` (itself a `PrefabInstance` of
`Assets/Resources/Prefabs/Abstract/AbstractMeleeMob.prefab`) with the smallest
override set that differs from it:

- Both `m_Name` overrides changed to `WallGolem`.
- The body `SpriteRenderer.m_Sprite` override points at the new
  `WallGolem.png` sprite instead of `RockGolem.png`.
- `m_SpriteTilingProperty.oldSize` updated to `{2.26, 2.54}` to match the new
  sprite's actual pixel size (226x254 / 100 PPU) instead of `RockGolem.png`'s
  `{2.56, 2.44}`. This field is inert editor bookkeeping — the base prefab has
  `m_AutoTiling: 0`, so it does not affect the collider's runtime size — but
  it is corrected here for the same reason `RockGolem.prefab` records its own
  sprite's dimensions.
- `RockGolem.prefab`'s `swapSprite`/`duration` overrides on
  `OnAttackSpriteSwapper` (which point at `RockGolem2.png`, the attack-frame
  swap, per `ANIMATION-ASSETS.md`) are **not** carried over: this task ships
  only one base-pose sprite, no `WallGolem2.png` attack frame, so those two
  fields are left unset and fall back to the abstract base's default
  (`swapSprite: {fileID: 0}`, no swap on attack). Adding an attack frame later
  needs a second `image_gen` pass (base frame attached, instruction to move
  only one limb, per the make-game-art skill) and is out of scope here.
- Everything else — the shadow child's scale/position (`{2.5, 0.5}` /
  `y: 0.29`) and the `ServedObjectHpBar`'s scale/position (`{2, 2}` /
  `y: 1.71`) — is copied unchanged from `RockGolem.prefab`. Assumption: with
  no server-side visual scale spec and a similar final sprite aspect ratio
  (226x254 vs. RockGolem's 256x244), matching the closest sibling 1:1 is the
  safest default. If `WallGolem` is meant to visually tower over `RockGolem`
  once opened in the Editor, these two overrides are the ones to grow.

The Unity Editor could not open this checkout (WSL case-sensitive volume), so
the prefab and both `.meta` files were hand-written, following
`.agents/skills/make-prefab/SKILL.md`'s hand-authoring section — the Editor
still has to open `WallGolem.prefab` once to confirm it imports cleanly.

## Not run

- `./.art/make-sheets.sh` requires `magick` (ImageMagick), which is not
  installed on this machine; a Python/Pillow venv in the scratchpad was used
  instead for every resize/trim/alpha check above, plus the ad hoc comparison
  sheet `compare-master-rockgolem-wallgolem.png`.
- No Unity Editor session was available, so no in-scene render check (actual
  PPU, ground contact, collider fit) was possible. That remains for whoever
  opens the prefab next.
