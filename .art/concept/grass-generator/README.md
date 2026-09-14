# grass_generator concept exploration

Subject: the client sprite/prefab for `PrefabType.GrassGenerator`
(`grass_generator_prefab`), a NATURE-element building the server already
implements (see `GrassGeneratorMagic`, `GrassGeneratorPrefabInitializer`, and
`GrassSpread` in the sibling `game` repository, branch `feature/521`). Once
planted it takes root and, every `ATTACK_INTERVAL` for as long as it stands,
buds off a `LeafField` patch in a ring around itself (capped at `QUANTITY`
live fields at once). It also carries `RigidBody`, `CircleCollider`,
`DummyMob` (has HP, can be destroyed), `TimedSelfDestroyer`, and
`BuildingEffectReceiver` — a stationary building, not a mobile creature.
`LeafField` already has its own client prefab and sprite; this exploration is
only for the generator itself.

Faction: 세계수 풀 정령 (World Tree grass spirit), per `WORLD.md`. Its closest
existing siblings by role are `SeedNest` and `LifeTree` — both are stationary
nature buildings without a face, matching `GrassGenerator`'s own
`DummyMob`/building archetype rather than the friendly-eyed "spirit" creature
archetype `STYLE.md` reserves for mobile spirits like `ThunderSpirit` or
`WindSpirit`. **`SeedNest.png` and `LifeTree.png` predate the frozen
master-v2 style** (2026-07-28) — they render as a smooth vector-gradient
illustration, not the current faceted papercraft technique — so per
`make-game-art/SKILL.md` they were used only for subject identity (nest
hollow, glowing trunk knot, general squat-mound scale), never as a rendering
reference. Rendering technique was locked from `MasterStyleKey.png`
(technique) and `WorldTreeSpirit.png` (World Tree foliage/bark material,
palette, and light).

## Candidates

- **`GrassGenerator-v1-leaf-whorl-mound-*.png`** (selected) — a squat dome of
  stacked faceted leaf whorls over a woody root base, with two curled root
  tendrils breaking the silhouette left and right (echoing `WorldTreeSpirit`'s
  branches), a small pale-green glowing seed-core nested in a front hollow
  (echoing `SeedNest`'s nest hollow / `LifeTree`'s glowing trunk knot at a
  small scale), and two small sprouting leaf buds low on the mound to read as
  "actively budding off growth" rather than a static bush.
- **`GrassGenerator-v2-budding-root-stump-*.png`** (alternate, not selected)
  — a gnarled woody stump with a leafy cap and several small curled fiddlehead
  shoots poking from cracks around its midsection. Also came back with a real
  alpha channel and passes the same validation as the selected candidate, so
  it is a viable second direction, not a reject. It was not chosen because it
  reads bark-first / stump-first: `GrassGenerator`'s entire gameplay point is
  spreading *leaf* fields, and this silhouette's brown wood dominates over
  its green foliage, putting it closer to the trunk language `LifeTree`,
  `EvilEnt`, and `TreeGolem` already use than to a distinct "leaf generator"
  read. The leaf-whorl-mound candidate foregrounds foliage instead and reads
  more clearly at a glance.

## Generation

3 `image_gen` calls total: 1 for the leaf-whorl-mound prompt (real alpha on
the first try) and 2 for the budding-stump prompt (the first came back fully
opaque with a painted grey/white checkerboard baked into the pixels; an
explicit retry naming that exact failure came back with real alpha). That is
2 of 3 calls with real alpha, in line with this project's usual variance.
Both reference images (`MasterStyleKey.png`, `WorldTreeSpirit.png`) were
attached to every call. No chroma-keying or background matting was applied to
either the opaque reject or either successful candidate — the opaque
first-attempt file was discarded rather than kept, matching this repository's
convention of not carrying every rejected raw generation (see the
`firework-shell` concept for precedent).

Prompts are under `prompts/`: `shared-prefix.txt` locks the rendering
technique (flat polygonal facet planes, hard creases, no gradient/gloss/
texture/grain, no outer contour, upper-left light), transparent background,
no-face building composition, and the `grass_generator` subject identity
(nature palette, `WorldTreeSpirit` material reference, `SeedNest`/`LifeTree`
subject-identity-only reference); `variant-a-leaf-whorl-mound.txt` and
`variant-b-budding-stump.txt` each add one paragraph describing that
candidate's distinct silhouette.

### Alpha-channel dust — a real defect, and how it was fixed

`GrassGenerator-v1-leaf-whorl-mound-source.png` passed the corner-alpha and
alpha-extrema checks, but `check_alpha.py`'s bright-desaturated-pixel share
climbed sharply after cropping to production size (0.024 at full 1254x1254
resolution -> 0.168 after `finalize.py`'s crop-and-resize to 256px). Visual
inspection of the raw alpha channel
(`GrassGenerator-v1-alpha-channel-before-denoise.png`) showed why: a sparse
field of isolated single/few-pixel specks with **full alpha=255** scattered
across the entire nominally-transparent background, like faint TV static —
not a solid painted background, so `check_alpha.py`'s corner and
transparent-share checks both still passed, but enough of it to matter once
cropped tight. This was not a fabricated background to key out; it was
generator noise sitting on top of an otherwise genuine cutout.

`denoise_alpha.py` fixes this without touching any real subject pixel: it
thresholds alpha at 128, finds the one large connected blob (the subject) via
`scipy.ndimage.label`, dilates that blob by 6px to protect soft
anti-aliased edges, and zeroes alpha to 0 everywhere outside the dilated
region — alpha *inside* the region is left byte-for-byte unchanged. On the
source image this zeroed 15,513 stray pixels (against a kept subject blob of
709,965px out of 1,572,516 total, across 3,718 disconnected noise
components). Re-running `finalize.py` on the denoised source dropped the
256px-crop bright-desaturated share to 0.008, and the final production PNG
carries no visible speckle when composited over magenta
(`GrassGenerator-v1-leaf-whorl-mound-chroma.png`).

## Finalization

```
python finalize.py GrassGenerator-v1-leaf-whorl-mound-denoised.png GrassGenerator-v1-leaf-whorl-mound-256.png --max-dim 256 --pad 4
```

`finalize.py` (same shape as the `firework-shell` script it's modeled on)
crops to the alpha bounding box with a small pad, fits the result inside a
256x256 box preserving aspect ratio, and trims any transparent border the
resample introduced. It refuses to run on a source with no real alpha.

Sizing rationale: `GrassGenerator` is a stationary NATURE building at roughly
the same scale class as `SeedNest` (256x179, a squat nest/mound) and
`LifeTree` (256x207, a full tree) — both `Assets/Resources/Prefabs/*.prefab`
built on `AbstractBuild`. `STYLE.md`'s tier table caps a building-scale asset
at `256x256` ("big"), which fits a squat, roughly-square dome silhouette
better than the `128`/`192` tiers meant for projectiles and small props.
Final export: **255x244 RGBA**, aspect preserved from the (denoised) alpha
bounding box.

## Validation

`GrassGenerator-v1-leaf-whorl-mound-256.png` (the file copied to
`Assets/Resources/Game/sprites/GrassGenerator.png`):

- Mode: RGBA, size 255x244.
- All four corner pixels: alpha 0.
- Alpha extrema: (0, 255) — a real gradient, not the one-pixel loophole.
- Transparent share of canvas: 0.355 (single centered subject, within the
  "roughly a fifth or more" expectation).
- Bright-desaturated opaque pixels (a painted-background tell): 0.008 (321 of
  40,108 opaque pixels) — below the 5% reject threshold, and manually
  confirmed these are the mound's own pale bark/seed-core highlight facets,
  not background residue (see `GrassGenerator-v1-leaf-whorl-mound-chroma.png`
  below).
- `GrassGenerator-v1-leaf-whorl-mound-chroma.png` composites it over solid
  magenta — no pale fringe, no leftover checkerboard or speckle.
- `GrassGenerator-v1-leaf-whorl-mound-64.png` is the 64px silhouette check:
  the dome shape, side root-tendril breaks, and the glowing core still read
  clearly as a distinct leafy mound.
- `compare-sheet.png` places the finalized sprite beside `MasterStyleKey.png`,
  `WorldTreeSpirit.png`, the legacy `SeedNest.png`/`LifeTree.png`, and
  `RockGolem.png`: it matches the faceted papercraft technique and World Tree
  foliage/bark material of the master anchors, is visibly built with a
  different (and correct, current) render technique than the legacy
  `SeedNest`/`LifeTree` art it takes subject identity from, and does not read
  as the stone/moss rock-golem faction.

`GrassGenerator-v2-budding-root-stump-source.png` also has real alpha and
passes the same corner/extrema/transparent-share checks (0 corner alpha,
extrema (0, 255), transparent share 0.618, bright-desaturated share 0.0000
before crop), but was not finalized to production size since it was not the
selected candidate.

## Client integration

- `Assets/Resources/Game/sprites/GrassGenerator.png` /
  `GrassGenerator.png.meta` — the finalized 255x244 sprite. The `.meta` is a
  byte-identical duplicate of `LifeTree.png.meta` (same `spritePixelsToUnits:
  133.333333`, same shared `spriteID`) with only the `guid` replaced by a
  fresh, uniqueness-checked 32-hex value, per `make-prefab/SKILL.md`'s
  hand-authoring rules.
- `Assets/Resources/Prefabs/GrassGenerator.prefab` /
  `GrassGenerator.prefab.meta` — a `PrefabInstance` of
  `Assets/Resources/Prefabs/Abstract/AbstractBuild.prefab`, built by
  duplicating `SeedNest.prefab` (the closer sibling in silhouette: a squat
  mound, not a tall tree) and changing only: the root `m_Name` to
  `GrassGenerator` (the prefab filename the server's `PrefabType.GrassGenerator`
  enum name resolves to through `Resources.Load<GameObject>($"Prefabs/{type}")`
  in `ObjectSpawner.InstantiateGameObject`), the `SpriteRenderer.m_Sprite`
  reference to the new sprite's guid, and the `BoxCollider2D`'s
  `m_SpriteTilingProperty.oldSize` bookkeeping field to the new sprite's
  world size (1.9125 x 1.83 at `spritePixelsToUnits: 133.333333`; that field
  is inert since `m_AutoTiling: 0`, kept only for consistency with the
  sibling prefabs). The HP-bar/TTL-bar/shadow transform overrides were kept
  identical to `SeedNest.prefab`'s values as a starting point — they are
  cosmetic UI placement, not gameplay-affecting, and are exactly the kind of
  thing the Unity Editor still needs to open once to verify (see below).

## Not run

- `./.art/make-sheets.sh` was not run: `magick` (ImageMagick) is not
  installed in this environment, so all resize/trim/alpha/contact-sheet work
  used a Pillow virtualenv (`venv/`, not committed) instead, per this
  project's environment notes. `compare-sheet.png` here serves the same
  comparison purpose as the script's `concept.png` output.
- No Unity Editor session was available in this environment (WSL cannot open
  this checkout), so the prefab was hand-authored per
  `make-prefab/SKILL.md`'s hand-authoring section and has not been opened or
  visually confirmed in-scene. In particular the HP-bar/TTL-bar/shadow
  transform positions, and whether `spritePixelsToUnits: 133.333333` gives
  `GrassGenerator` the right on-ground footprint next to other buildings,
  still need an Editor pass.
