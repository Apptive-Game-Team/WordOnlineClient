# repair_totem concept exploration

Subject: the client sprite and prefab for `PrefabType.RepairTotem`
(`repair_totem` magic, issue #629). The server side already ships it —
`RepairTotemMagic` (`src/main/java/.../magic/implement/build/RepairTotemMagic.java`)
and `RepairTotemPrefabInitializer` (`.../prefab/implement/build/RepairTotemPrefabInitializer.java`)
on the sibling `game` repository, branch `feature/521` — but the client had no
sprite or prefab, so `ObjectSpawner.InstantiateGameObject`'s
`Resources.Load<GameObject>($"Prefabs/{type}")` returned null and an empty
`GameObject` spawned with nothing drawn.

## What the server says it is

`RepairTotemPrefabInitializer` gives it `RigidBody`, a non-trigger
`CircleCollider`, `DummyMob` (HP, no attack), its own `RepairAura`, its own
`TimedSelfDestroyer`, and `BuildingEffectReceiver`, and sets its element to
`NATURE`. `RepairAura`'s Javadoc is explicit about what it does *not* do: it
calls `TimedSelfDestroyer.freeze()` on every allied object in range each tick,
so allied buildings stop aging while it stands — it does not rewind elapsed
time, it does not heal HP, and it never touches its own `TimedSelfDestroyer`.
So the concept is a small nature totem that halts *decay*, not a healer and
not a combatant. `WORLD.md`'s 세계수의 정령 section lists 풀 정령 (grass
spirits) as one of the three World Tree spirit lines, and `STYLE.md` assigns
the nature palette (`#B8CE59 #9DA74D #768738 #384823`) to that lineage.

## References used for rendering

`.art/anchors/master-v2/MasterStyleKey.png` (technique) and
`.art/anchors/master-v2/WorldTreeSpirit.png` (World Tree spirit material —
warm pale wood, layered leaf facets, curled wood-grain) were the only two
images attached to every `image_gen` call, per `make-game-art/SKILL.md`'s
rule that a selected master style is rendered from the frozen anchors, never
from legacy faction art.

`HealingTotem.prefab`, `RallyingTotem.png`, and `FrenzyTotem.png` were
inspected for **subject identity and prefab structure only** — a squat
totem/post standing on the ground with a glowing focal point at the top, the
general shape convention this game's other `Totem` objects share — never as
rendering references. All three are legacy painterly-gradient sprites, not
master-v2 faceted art (`HealingTotem.png` is 188x256 with soft radial
gradients and no flat facets at all; see `compare-vs-anchors-and-siblings.png`
below), and `RallyingTotem` is explicitly on `STYLE.md`'s Hellfire redesign
list. Copying their technique would have propagated the exact style drift
`make-game-art/SKILL.md` exists to prevent.

## Candidates

- **`RepairTotem-v1-carved-post-source.png`** (chosen) — an upright carved
  wood post, wider at a gnarled root base and tapering toward the top,
  wrapped twice by a curled vine band echoing `WorldTreeSpirit`'s swirled
  wood-grain, capped with a faceted pale-green leaf-bud crystal that reads as
  the totem's glowing focus. Silhouette is a tall narrow post, matching the
  general proportions `RallyingTotem`/`FrenzyTotem` use for this game's other
  `Totem` objects (see the aspect-ratio note under Finalization) while
  carrying nature material instead of their hellfire one.
- **`RepairTotem-v3-seed-crown-source.png`** (alternate, not used) — two
  stacked rounded seed-pod shapes, the lower wider than the upper, bound by
  thick roots, with the top pod cracked open and pale-green shoots growing
  from the crack. This is a valid, cleanly-cut real-alpha generation (see
  Validation) and a reasonable alternate reading of "repair" as regrowth
  sealing a wound, but its squat mound silhouette reads less like this game's
  established `Totem` shape language than the carved-post candidate, so it
  was not carried forward. Kept here in case a future pass prefers it.
- **`variant-v2-mended-roots.txt`** (prompt kept, image discarded) — a
  cracked stone core visibly stitched together by living roots and vines,
  the most literal reading of "repair" of the three. Its `image_gen` call
  came back as a plain opaque RGB with a painted checkerboard, and rather
  than accept that, the generating agent wrote its own flood-fill script and
  baked in a synthetic alpha channel — exactly the chroma-keying
  `make-game-art/SKILL.md` forbids ("Do not chroma-key the background away:
  keying leaves a pale fringe, and it hides the fact that the generator is
  ignoring the instruction"). That output is **not** committed here; only its
  prompt is kept. Regenerating it needs the same no-post-processing
  instruction described below from the first attempt, not just a retry.

## Generation

3 initial `image_gen` calls, one per variant, each with `MasterStyleKey.png`
and `WorldTreeSpirit.png` attached. 2 of 3 (`v1`, `v3`) came back as genuine
transparent RGBA on the first try; `v2` came back opaque with a painted
checkerboard, and the agent chroma-keyed it itself instead of retrying —
discovered by checking `im.mode` and all four corner alpha values with
Pillow, not by looking at the preview (a self-keyed cutout looks identical to
a real one at a glance). `v1` was then re-run once with an explicit added
rule — "do not write, compile, or run any script or tool of your own to
remove, key out, mask, or otherwise modify a background or alpha channel...
if a generation comes back painted, that attempt is a failure, call
`image_gen` again" — and the second attempt (`RepairTotem-v1-carved-post-source.png`)
came back with real alpha with no post-processing involved. This addendum is
worth reusing verbatim for the next asset generated in this repository; see
`prompts/shared-prefix.txt`'s sibling files for the exact wording used.

## Finalization

```
python finalize.py RepairTotem-v1-carved-post-source.png RepairTotem-v1-carved-post-256.png 256
```

`finalize.py` crops to the alpha bounding box (with a small pad for soft
edges), fits the result inside a max-size box preserving aspect ratio, and
trims any transparent border the resample introduces — the same faint,
near-zero-alpha stray pixels near the raw canvas edges that inflated the
*first* bounding box away from the real subject on this generation. It
refuses to run on a source that lacks real alpha rather than matting one out.

Sizing rationale: `repair_totem` is a stationary building with HP, like
`HealingTotem` (`Assets/Resources/Game/sprites/HealingTotem.png`, 188x256),
so it belongs in `STYLE.md`'s `big` tier (max `256x256`), not the small/middle
tiers used for projectiles or icons. The alpha bounding box of the chosen
source is nearly square (aspect ratio ~0.96), but a second bounding-box pass
after the resize (see above) trims it to the actual carved-post silhouette.
Final export: **99x235 RGBA**, aspect preserved throughout. Its aspect ratio
(0.42) lands close to `RallyingTotem.png`'s (109x256, 0.43), consistent with
this game's other tall-post `Totem` sprites.

## Validation

`RepairTotem-v1-carved-post-256.png` (the file promoted to
`Assets/Resources/Game/sprites/RepairTotem.png`):

- Mode: RGBA, size 99x235.
- All four corner pixels: alpha 0.
- Alpha extrema: (0, 255) — a real gradient, not the one-pixel loophole.
- Transparent share of canvas: 0.50 (a single centered subject, in the
  expected "roughly a fifth or more" range).
- Bright-desaturated opaque pixels (a painted-background tell): 22 out of
  11,683 opaque pixels sampled (0.19%), well under the 5% reject line.
- `RepairTotem-v1-carved-post-chroma.png` composites it over solid magenta —
  no pale fringe, no leftover checkerboard ring.
- `RepairTotem-v1-carved-post-64.png` is the 64px silhouette check: the
  vine-wrapped post and the glowing crystal cap still read clearly as one
  connected shape.
- `compare-vs-anchors-and-siblings.png` places it next to `WorldTreeSpirit`
  and `RockGolem` (current master-v2 anchors) and next to the legacy
  `HealingTotem`/`RallyingTotem`/`FrenzyTotem` sprites at matched height —
  the new sprite's flat facet planes and hard creases visibly match the two
  anchors and visibly differ from the three legacy sprites' soft painterly
  gradients, confirming it followed the *current* master style rather than
  its nearest gameplay sibling's rendering technique.

`RepairTotem-v3-seed-crown-source.png` also has real alpha (corners at 0,
extrema (0, 255)) and passed the same transparent-share and bright-desaturated
checks, but was not finalized to a production canvas since it was not the
chosen direction.

## Making the aura visible

`RepairAura` is the whole point of the object and nothing on screen said where it
reached. The radius is on the wire already — `RepairAura.start` calls
`gameObject.drawCircle(Vector3.ZERO, radius, GizmoCategory.AreaOfEffect)` — but
`ServedObjectGizmoRenderer` is inside `#if UNITY_EDITOR`, so a player never sees it.

`RepairTotem.prefab` therefore carries a `RepairAura` child: a ring sprite at alpha
0.5 and sorting order 6, so it sits above the shadow (5) and under every body sprite
(10), driven by the existing `IdleAuraEffect` for a slow breathing pulse, and sized by
the new `AuraRadiusScaler`. That component reads `repair_totem.radius` out of the
parameter table `ParametersDataSource` caches — the same table the server reads — so
the drawn circle follows the migration value instead of a number copied into the
prefab. It falls back to 4.0 with a warning when the table has not been fetched.

The ring is a sprite standing in the world XY plane while the aura it stands for is a
circle lying on the ground XZ plane. Those project to the same on-screen ellipse only
because this camera is tilted exactly 45 degrees, where sine and cosine are equal; the
reasoning and what breaks if the camera ever moves are written up in
`.agents/docs/scene-space.md`.

### The first attempt reused `nature_aura.png`, and that was wrong

The first version of this scaled the existing `Assets/Art/Images/Effect/Aura/nature_aura.png`
to 6.25x. It was reverted for two separate reasons, both worth keeping written down:

- **Technique.** `nature_aura.png` is legacy art with a heavy dark outer contour on
  every leaf. `STYLE.md` says "No outer contour line. Forms separate by value, not by
  stroke." Blowing it up to 8 world units made that forbidden contour roughly six times
  heavier than any other line on screen. This PR had already been careful not to use the
  legacy `HealingTotem`/`RallyingTotem`/`FrenzyTotem` sprites as rendering references,
  and then put a legacy ring on top of the master-v2 sprite anyway.
- **Meaning.** `ANIMATION-ASSETS.md`'s 오라 의미 table binds `nature_aura.png` to
  `NatureIdleAura` and `NatureAttackAura`. Those are *status* auras: what element state
  a unit is under. An area-of-effect radius is a different statement, and sharing one
  image makes the same wreath mean two things. `ANIMATION-ASSETS.md` now has a
  범위 표시 오라 section drawing that line, and area markers live under their own
  `Assets/Art/Images/Effect/AreaOfEffect/` folder.

### Candidates

Three directions were generated, each one `image_gen` call with
`MasterStyleKey.png` and `WorldTreeSpirit.png` attached, prompts in
`prompts/aura-shared-prefix.txt` plus `prompts/aura-variant-*.txt`. The shared prefix
leads with the transparency requirement rather than trailing it, and raises "no outer
contour line" to the most important rule for this asset, since an outline is exactly
what was being removed. `aura-candidates.png` compares all three at the size they are
actually drawn — radius 4 on a 2-unit grid with the totem in the middle — plus a 64px
readability chip.

- **`RepairAura-v1-leaf-facets-source.png`** (chosen) — twelve large flat leaf kites
  laid around the circle, each two facet planes split by one hard crease, alternating
  light and dark nature greens, with transparent gaps between them. Band measures 10.2%
  of the diameter, which is what the brief asked for, and it is the quietest of the
  three behind gameplay.
- **`RepairAura-v2-grass-blades-source.png`** (alternate, not used) — low grass tufts
  standing around the circle. A clean real-alpha generation and a good look, but it is
  built from 12 to 14 tufts of several blades each, so at 8 units the parts get small
  and the ring starts to read as scatter rather than as one boundary. Kept in case a
  future pass wants the more organic reading.
- **v3, a woven root band** (prompt kept, image not committed) — a closed pale-wood
  rope hoop with four leaf clusters. Its first `image_gen` call came back opaque and the
  generating agent then ran a connected-component script to manufacture an alpha channel,
  which is the chroma-keying `make-game-art/SKILL.md` forbids; that output was discarded
  rather than accepted, and a second call produced genuine alpha. The clean version was
  still not used: an unbroken brown band is the heaviest of the three, and its wood tone
  competes with the totem's own bark instead of reading as an aura.

### Finalizing

`finalize-aura.py` differs from `finalize.py` in one way that matters: it pads the
shorter axis to a square before resizing, instead of trimming tight. `AuraRadiusScaler`
scales each axis by half the sprite rect on that axis, so a square canvas is what makes
the drawn circle round in world units and keeps the prefab's `localScale` uniform at
6.25. The chosen source cropped to 1230x1207 (aspect 1.019), so squaring it moved almost
nothing. The finished sprite is 512x512 RGBA at PPU 400, 83.9% fully transparent, band
10.3% of the diameter, all four corners alpha 0, no bright-desaturated block and no
stray saturated pixels.

`preview-aura.py` renders `aura-preview.png` by applying the camera's 45° projection by
hand: the pulse at both ends of its alpha tween, and the same aura placed on the full
18 x 10 field with a 2-unit grid, so the radius can be counted off the grid. The ring
spans x 2..10 and z 1..9 around a totem at (6, 5), which is radius 4.

### Known limit

A `SpriteRenderer` is not clipped to the field. `SkillIndicatorShapeRenderer` clips its
polygon to X 0..18 and Z 0..10; this ring does not, so a totem placed near a board edge
draws leaves past it. Moving the visual to a mesh is the fix if that ever matters.

## Not run

- `./.art/make-sheets.sh` was not run — `magick` (ImageMagick) is not
  installed in this environment; all resize/trim/alpha/comparison work above
  used a Pillow virtualenv in the scratchpad instead, per this repository's
  environment notes.
- No Unity Editor session was available in this environment (WSL cannot open
  this checkout), so `Assets/Resources/Prefabs/RepairTotem.prefab` was
  hand-authored by duplicating `HealingTotem.prefab`'s override set and has
  not been opened or visually confirmed in-scene — ground contact, HP-bar and
  TTL-bar placement, and collider fit against the new sprite's silhouette all
  still need one Editor pass.
