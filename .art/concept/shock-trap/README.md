# shock_trap concept exploration

Subject: the client sprite and prefab for `PrefabType.ShockTrap` (server key
`shock_trap`), a summoned building the game server already implements but the
client has never drawn. Server sources read for this task (sibling repository
`game`, branch `feature/521`, not checked out — read with `git show`):

- `.../magic/implement/build/twocard/ShockTrapMagic.java` — a two-card summon
  magic that places `PrefabType.ShockTrap`.
- `.../object/prefab/implement/build/ShockTrapPrefabInitializer.java` — gives
  the object `RigidBody`, `CircleCollider`, `DummyMob` (it has HP and can be
  destroyed), `ShockTrapDetector`, `TimedSelfDestroyer`,
  `BuildingEffectReceiver`, and element `LIGHTNING`.
- `.../object/component/magic/ShockTrapDetector.java` — arms on placement,
  waits for an enemy inside `radius`, counts down `triggerDelay`, then stuns
  everything still in range for `stunDuration`, reloads for `attackInterval`,
  and re-arms instead of destroying itself on trigger.

## Faction and catalog placement

`.art/MAGIC-CATALOG.md` has no `shock_trap` row yet (this is the first client
pass), but it fits directly into the existing "자연·정령 설치물" (nature/spirit
installation) group alongside `HealingTotem`, `FrenzyTotem`, and `WindTotem` —
소환물 (installation), not 소환수 (creature). `WORLD.md` assigns the World Tree
spirits to three sub-families: 전기(lightning) / 풀(nature) / 바람(wind). A
lightning-element trap belongs to the 전기 정령 sub-family, the same family as
`ThunderSpirit`, `ZapMouse`, and the approved `StormStag` (see
`.art/CONCEPT-BRIEF.md`'s "World Tree lightning spirit — Storm Stag" section).

This matters for material choice: `ElectricTower` and `FrenzyTotem`, named in
the task as the closest siblings, are the closest match for *prefab structure*
(a stationary building with a detection/attack loop, `RigidBody`,
`DummyMob`), but they are the wrong faction for *rendering material* —
`ElectricTower` is Human-faction cool masonry and steel (`STYLE.md`'s "Humans"
section), and `FrenzyTotem` is a Hellfire-legion secondary reference (dark
crimson, angular, tribal — see `STYLE.md`'s Hellfire section). Neither is a
World Tree spirit. The actual material reference is `WorldTreeSpirit.png`
(the canonical anchor for "World Tree spirit / foliage" per `ANCHORS.md`),
recolored from its default green/cream toward the lightning sub-family's own
palette `#FCFBD5` `#F6DB5F` `#D7A313` `#996B07` (`STYLE.md`, Spirits →
Lightning row) for the trap's crystal core, while keeping the wood itself in
its own muted cream/brown range.

## Silhouette direction

The installation family this belongs to already contains three tall vertical
totems (`FrenzyTotem`, `HealingTotem`, `RallyingTotem`, all 256px tall) and
one wide low one (`WindTotem`, 256×137, because of its wing span). A *trap* —
something an enemy walks into and gets caught by — reads better low and wide
than as another vertical totem, and a low/wide silhouette also keeps it from
being confused with the existing totem lineup at a glance. All three
generation variants below were written to be deliberately wider than tall.

## Candidates

- **`ShockTrap-rootring-v2a-source.png`** (chosen) — a low, flat ring of
  knotted World Tree roots coiled on the ground like an open trap jaw, three
  jagged broken-branch prongs (angular like lightning zigzags) around the
  outside, two curling root tendrils cradling one bright lightning crystal
  shard at the center. Reads unmistakably as a snare with a dangerous core,
  not a creature or a tower.
- **`ShockTrap-rootring-v2b-alt.png`** — same material and palette, but the
  two center root tendrils curl into a heart-shaped frame around a
  lower-set crystal, leaving more open negative space beneath it (a more
  literal "jaw" reading). Close enough to v2a that either would have worked;
  v2a's crystal sits higher and reads as brighter/more dangerous at a glance,
  so it was preferred.
- **`ShockTrap-stumpaltar-c-chromakey-artifact.png`** — a squat tree-stump
  altar with three upward lightning-fork prongs and a center crystal, closer
  to a small shrine than a ring trap. Kept as the record of the rejected
  third direction. This file is not a clean cutout: the generator came back
  with an opaque pale background, and the calling agent — before this
  session had forbidden it — wrote a throwaway chroma-key script against
  that background instead of just saving the raw result, which is exactly
  the anti-pattern `make-game-art/SKILL.md` warns against ("Do not
  chroma-key the background away: keying leaves a pale fringe"). The visible
  magenta fringe around the shapes in this file is that fringe. It is kept
  only as a record of the rejected silhouette direction, downsized to 640px
  wide since it is documentation only, and was never a finalize candidate.

The two `v2a`/`v2b` files are the actual `image_gen` output, resized and
alpha-cropped by `finalize.py` but otherwise unedited — no chroma-keying or
hand-painted background removal was applied to either.

## Generation

Two image reference inputs on every call: `.art/anchors/master-v2/MasterStyleKey.png`
(technique) and `.art/anchors/master-v2/WorldTreeSpirit.png` (World Tree
spirit wood/foliage material — the correct anchor per `ANCHORS.md`, not a
legacy sprite). The lightning palette was passed as hex text in the prompt,
never as an image reference, per `CONCEPT-BRIEF.md`'s convention.

`prompts/shared-prefix.txt` locks the transparency requirement (stated first,
before any technique language — see Validation below for why), rendering
technique, and the shared `shock_trap` subject identity; each
`prompts/variant-*.txt` adds one paragraph describing that candidate's
distinct silhouette. Root-ring and coiled-spring turned out to describe
almost the same shape once rendered (see v2a vs v2b above); stump-altar was
the one genuinely different silhouette of the three.

10 completed `image_gen` calls across two prompt structures (3 more calls
were sent but killed by a local process timeout before the agent reported a
result, and are not counted below since no output file exists to judge):

1. **First structure** (technique paragraph before the transparency
   requirement): the initial 3 calls (one per variant) plus 3 more retries of
   the root-ring variant alone, 6 completed calls total. All 6 came back
   without real alpha — 5 with a fully opaque painted checkerboard or solid
   pale background, and the 6th (the stump-altar variant's only attempt) is
   the chroma-key artifact described above. 0 real alpha out of 6.
2. **Second structure** (`shared-prefix.txt` as committed: the transparency
   requirement moved to the very first sentence, ahead of the technique and
   composition paragraphs, and the prompt shortened; also the calling
   instructions were changed to explicitly forbid any chroma-keying or other
   post-processing, in response to what produced the chroma-key artifact
   above): 4 calls, all on the root-ring variant. 3 came back with genuine
   alpha (`ShockTrap-rootring-v2a-source.png`, `-v2b-alt.png`, and a third
   near-duplicate of v2a not kept) and 1 came back opaque. 3 real alpha out
   of 4 — a large jump from the first structure's 0-for-6. The sample is too
   small to call this a firm rule for future assets, but leading with the
   transparency requirement and forbidding local post-processing are the two
   changes made between the two rounds worth recording here. The stump-altar
   direction was never re-tried with the second structure, so it has no
   alpha candidate.

No chroma-keying was applied to either finalized candidate
(`ShockTrap-rootring-v2a-source.png`, `-v2b-alt.png`) — both carry the
generator's own true alpha channel, untouched.

## Finalization

```
python finalize.py ShockTrap-rootring-v2a-source.png ../../../Assets/Resources/Game/sprites/ShockTrap.png --max-dim 256
```

`finalize.py` refuses to run on a source without real alpha, crops to the
alpha bounding box with a small pad, fits inside a 256×256 box preserving
aspect ratio, then trims any transparent border the resample introduced. It
never mattes a background away.

Sizing rationale: `ElectricTower.png`, `FrenzyTotem.png`, `HealingTotem.png`,
and `RallyingTotem.png` are all summoned buildings sized to the `big` tier
(long axis 256px per `STYLE.md`'s tier table) despite very different aspect
ratios (200×256 down to 109×256). `ShockTrap` is the same kind of summoned
building — a `DummyMob` with HP, not a small in-flight effect — so it takes
the same `big` tier. Final export: **254×164 RGBA**, aspect preserved from
the alpha bounding box, matching `WindTotem.png`'s low-and-wide convention
(256×137) rather than the tall totems' convention.

## Validation

`Assets/Resources/Game/sprites/ShockTrap.png` (254×164 RGBA):

- Mode: RGBA, size 254×164.
- All four corner pixels: alpha 0.
- Alpha extrema: (0, 255) — a real gradient at the edges, not the one-pixel
  loophole.
- Transparent share of canvas: 0.50 (a single centered subject, in the
  expected "roughly a fifth or more" range).
- Bright-desaturated opaque pixels (a painted-background tell): 0.02% of
  opaque pixels — effectively none.
- `ShockTrap-v2a-chroma.png` composites it over solid magenta — clean edges,
  no pale fringe, no leftover checkerboard ring.
- `ShockTrap-v2a-64.png` is the 64px silhouette check: the ring, the three
  prongs, and the bright crystal spike all still read clearly.
- `compare-64-vs-windtotem.png` places the three transparent candidates next
  to a 64px `WindTotem.png` thumbnail — all three read as a distinct
  crown/ring shape, clearly different from `WindTotem`'s winged silhouette.
- `compare-family.png` places the chosen sprite next to `WorldTreeSpirit.png`,
  `ThunderSpirit.png`, `StormStag.png`, `ElectricTower.png`, `FrenzyTotem.png`,
  and `WindTotem.png` at a shared thumbnail size: the wood matches
  `WorldTreeSpirit`'s cream/brown material, the crystal matches
  `ThunderSpirit`/`StormStag`'s lightning-gold palette, and the low/wide
  silhouette reads as neither a tower (`ElectricTower`) nor a totem pole
  (`FrenzyTotem`) nor a winged totem (`WindTotem`).

`ShockTrap-rootring-v2b-alt.png` and
`ShockTrap-stumpaltar-c-chromakey-artifact.png` are concept-only. The former
has real alpha and passes the same checks but was not chosen (see Candidates
above); the latter is a bad chroma-key capture, not a real cutout, and was
never finalized.

## Not run

- `./.art/make-sheets.sh` was not run — `magick` (ImageMagick) is not
  installed on this machine. The comparison sheets above were built with a
  Pillow virtual environment in the scratchpad instead, following the same
  intent (thumbnail grid, 64px check, side-by-side against siblings).
- No Unity Editor session was available in this environment (this checkout
  cannot be opened from WSL), so `Assets/Resources/Prefabs/ShockTrap.prefab`
  and its `.meta` were hand-authored by duplicating
  `Assets/Resources/Prefabs/ElectricTower.prefab`'s override set and scaling
  its HP-bar/shadow/TTL-bar transform overrides to `ShockTrap`'s own
  254×164px (1.905×1.23 world unit) size at the shared building PPU
  (133.333333). One override present in `ElectricTower.prefab` — a second
  `m_Sprite` modification targeting `fileID: 8788216132779933461` against
  `AbstractBuild.prefab` — was deliberately dropped: that `fileID` does not
  exist anywhere in the current `AbstractBuild.prefab` (confirmed by
  grepping the file), so Unity silently ignores it in every prefab that
  carries it (`ElectricTower`, `WindTotem`, `HealingTotem`, `LifeTree`,
  `RockTurret`, `GroundTower`, `BubbleGenerator`, `ManaWell` all have the
  same stale line) — the same class of issue `make-prefab/SKILL.md` already
  documents for `VineSpirit.prefab`'s `m_RemovedComponents` block. No actual
  behavior changes; this just avoids copying a known-dead override forward
  into a ninth prefab. The Editor still has to open
  `Assets/Resources/Prefabs/ShockTrap.prefab` once to confirm the HP bar,
  shadow, and TTL bar sit correctly against the new sprite.
