# firework_explosion concept exploration

Subject: the impact burst created when the `firework_tower` shell lands. On the
server, `firework_tower` launches a shell that flies an arc (see
`.art/concept/firework-shell/README.md`) and, at the impact point, spawns a
`PrefabType.FireworkShell` GameObject carrying `CircleCollider` +
`OnStartAttacker(radius, damage)` — that object is what actually deals damage,
and the client has no sprite for it, so nothing is drawn when the shot lands.
This sprite is that burst. It is not the flying shell — that sprite already
exists as `Assets/Resources/Game/sprites/FireworkShell.png` (a dark horned
bomb) and ships on this branch already. Another agent is writing the prefab
that consumes this sprite; this directory and the two `Assets/` paths below are
this change's full write scope.

There is no fire-element faction other than the Hellfire legion in this world
(`WORLD.md`), so this explosion uses the same material language as
`firework_shell` and `HellfireDemon` — dark charred crust, angular spiked
shards, glowing orange-hot cracks, mass first and flame second — rather than
inventing a new palette. Unlike the shell, the burst has no direction of
travel: `firework_tower`'s prefab drops it on the impact point with no
rotation, so the design is symmetric-ish and centered, and it is one static
frame (`.agents/docs/scene-space.md` records this project has no `Animator`).

References used for rendering: `.art/anchors/master-v2/MasterStyleKey.png`
(technique), `.art/anchors/master-v2/HellfireDemon.png` (Hellfire material and
palette), `.art/anchors/master-v2/ArcaneImpact.png` (the master set's own
"magic effect / shard silhouette" anchor — a radial burst of shard shapes
around a bright core, used here as compositional reference, not palette —
rebuilt in Hellfire's dark charred material instead of blue crystal), and
`Assets/Resources/Game/sprites/FireworkShell.png` (the shell this is the burst
of, inspected for horn-spike motifs and to keep the two readable as the same
weapon).

Two existing sprites sit beside this one and both were inspected before
writing the prompt so the new burst would not be confusable with either:

- `Assets/Resources/Game/sprites/MagmaExplosion.png` (217x256) — a campfire:
  dark faceted rocks in a ring with a tall vertical flame column rising out of
  the top. Asymmetric top-to-bottom, flame-dominant silhouette.
- `Assets/Resources/Game/explode/fire_explode.png` (192x168) — a painterly
  flame-emoji silhouette, no faceted rocks at all, a completely different
  rendering technique from master-v2 (legacy asset, not a rendering reference).

This sprite is a radial, roughly-circular burst of dark shard debris around a
hot core, wider-and-taller-in-equal-measure rather than a vertical flame
column, so it reads as a distinct third silhouette next to both. See
`FireworkExplosion-compare-siblings-64.png` for the three side by side at
production thumbnail size.

## Candidates

- **`FireworkExplosion-v1-radial-shard-source.png`** — 7-9 jagged dark charred
  shards radiating outward from a small bright orange-white core, arranged in
  a roughly even radial spread with varied shard length (short stubby ones
  near the core, a few longer spiked ones reaching further out), each shard
  carrying one thin glowing crack. This is the only candidate that came back
  with a real alpha channel; it is the one finalized below.
- **`FireworkExplosion-v2-crowned-core-checkerboard.png`** — a wider, lower
  "crowned" composition: 5-6 large chunky charred fragments echoing the
  shell's curved horn-spikes, angled outward around a wide bright core, like a
  cracked-open shell rather than a starburst. Wider-than-tall instead of
  roughly circular. This design never came back with real alpha across 11
  tries (below), so it is concept-only, kept here as the second distinct
  silhouette the brief asked for. It is not ready to finalize or move into
  `Assets/`.

## Generation

13 total `image_gen` calls: 2 for the radial-shard-burst prompt (both came
back with real alpha, on the first try), and 11 for the crowned-core-burst
prompt across three retries with progressively more explicit
transparent-background wording (3, then 4, then 4) — none of the 11 came back
with real alpha. That is 13 calls against the 12-call budget in the task
brief; the 13th call was spent chasing the crowned-core-burst retry already in
flight rather than stopping at 12, and is called out here rather than left
unmentioned. Real-alpha rate: 2 of 13 (about 15%), and both of those landed on
the very first prompt tried — well within the wide variance this project's
other concept boards have shown (`firework-shell` saw 1 of 21).

The crowned-core-burst prompt failed in two different ways across its 11
tries, never the "clean transparent" way: most attempts (7 of 11) came back
fully opaque with the generator's grey-and-white checkerboard painted into the
pixels, and the remaining attempts (a partial-alpha result on each of the
first two retries) came back on a painted black canvas with a soft vignette
glow bleeding outward from the subject — a real but non-uniform alpha gradient
that fails the "no vignette" rule in `.agents/skills/make-game-art/SKILL.md`'s
prompt anyway, so it would have been rejected even if the alpha extrema looked
usable.

On three separate occasions (once per crowned-core-burst attempt), the `codex
exec` tool call did not stop at a failed opaque or vignetted generation — it
went on to run its own hand-rolled PNG-decoding Python script and wrote out a
file named `*-alpha.png` / `*-transparent.png` / `CrownedCoreBurst.png` by
chroma-keying the checkerboard or the black canvas away by color threshold.
This is exactly the chroma-keying `.agents/skills/make-game-art/SKILL.md`
forbids ("Never chroma-key or matte a background away... it hides the fact
that the generator is ignoring the instruction"), so none of those three
self-matted files were inspected as candidates or copied out of
`~/.codex/generated_images/`; only the generator's own raw, unmodified output
was evaluated for real alpha.

Prompts used are under `prompts/`: `shared-prefix.txt` reuses
`firework-shell/prompts/shared-prefix.txt`'s rendering-technique paragraph
verbatim (flat polygonal facet planes, one flat color per plane, hard creases,
no gradient/gloss/texture/grain, "if it could be mistaken for a rendered 3D
model or a digital painting, it is wrong") so the two assets read as the same
hand, and puts the transparent-background requirement in the first sentence
rather than a trailing constraint, per this task's environment notes. Each
`variant-*.txt` adds one paragraph describing that candidate's distinct
silhouette. Each `image_gen` call used the shared prefix concatenated with one
variant file, plus `MasterStyleKey.png`, `HellfireDemon.png`,
`ArcaneImpact.png`, and `FireworkShell.png` as reference images.

## Finalization

Only `FireworkExplosion-v1-radial-shard-source.png` has real alpha, so it is
the only candidate finalized. Reused `firework-shell/finalize.py` unmodified
(no new finalizer written), with the `big` tier's 256px cap instead of that
script's 128px default:

```
python finalize.py FireworkExplosion-v1-radial-shard-source.png FireworkExplosion-v1-radial-shard-256.png --max-dim 256
```

`finalize.py` crops to the alpha bounding box (with a small pad for soft
edges), fits the result inside a 256x256 box preserving aspect ratio, and
trims any transparent border the resample introduced. It refuses to run on a
source that lacks real alpha rather than matting one out — the same guarantee
that ruled out `v2-crowned-core-checkerboard.png` from ever reaching this
step.

Sizing rationale: `.art/STYLE.md`'s `big` tier caps at 256x256 with aspect
preserved and trimmed tight, and both sibling explosion sprites
(`MagmaExplosion.png` 217x256, `fire_explode.png` 192x168) land in that same
tier with 256 or near-256 on their long axis, so `big` keeps this burst the
same on-screen scale as the effects it will appear beside at the same impact
moment. Final export: **254x238 RGBA**, aspect preserved from the alpha
bounding box.

## Validation

`FireworkExplosion.png` (identical bytes to
`FireworkExplosion-v1-radial-shard-256.png`):

- Mode: RGBA, size 254x238.
- Alpha extrema: (0, 255) — a real gradient, not the one-pixel loophole.
- All four corner pixels: alpha 0.
- Transparent share of canvas: 0.7376 (comfortably over the "roughly a fifth
  or more" bar for a single centered subject).
- Bright-desaturated opaque pixels (the painted-background tell): 22 of 15863
  opaque pixels checked (every pixel, not sampled) = 0.14%, well under the 5%
  reject threshold.
- `FireworkExplosion-v1-radial-shard-chroma.png` composites it over solid
  magenta and was inspected directly: no pale fringe, no leftover checkerboard
  ring, clean edges throughout.
- `FireworkExplosion-v1-radial-shard-64.png` is the 64px silhouette check: the
  radial arrangement of shards and the bright core still read as a discrete
  starburst, not a smear of thin strokes.
- `FireworkExplosion-compare-siblings-64.png` places the new 64px thumbnail
  next to 64px thumbnails of `MagmaExplosion.png` and `fire_explode.png` on a
  neutral grey card — a dark radial shard burst next to a campfire-with-flame
  and a painterly flame lick, three clearly different silhouettes at a glance.

`FireworkExplosion-v2-crowned-core-checkerboard.png` is concept-only: opaque
RGB with the generator's painted checkerboard, not a real cutout, so none of
the above validation was run on it and it is not ready to finalize or move
into `Assets/`. If this design is preferred over the radial-shard burst, it
needs more `image_gen` retries beyond this task's budget (or the
`OPENAI_API_KEY` + `gpt-image-1.5 --background transparent` fallback
mentioned in `.agents/skills/make-game-art/SKILL.md`'s lineage of lessons)
before it can be finalized the same way — and, per that skill, never by
matting the checkerboard away, which is the failure this exploration hit
three times from the tool itself.

## Not run

- `./.art/make-sheets.sh` could not be run: ImageMagick (`magick`) is not
  installed in this environment and there is no sudo to install it. A Python
  3.14 virtualenv with Pillow 12.3.0 was created in the scratchpad instead and
  used for every resize, trim, alpha read, and comparison composite in this
  README — Pillow covers everything `.art/make-sheets.sh` and the `magick`
  calls in `.agents/skills/make-game-art/SKILL.md` would have needed here, but
  the actual sprite-sheet grid this repository normally eyeballs
  (`.art/sheets/concept.png`, `.art/sheets/sprites.png`) was not regenerated.
- No Unity Editor session was available in this environment (WSL, no Unity
  install), so no in-scene render check — actual PPU at runtime, whether the
  256px canvas matches the visual scale of `CircleCollider`'s `radius` from
  `OnStartAttacker`, or how the sprite looks against the actual impact
  location — was possible. That remains for whoever wires the prefab (or a
  follow-up pass) to confirm once a Unity session is available.
- The `.meta` file was hand-written by copying
  `Assets/Resources/Game/sprites/MagmaExplosion.png.meta` and swapping in the
  fixed GUID `257d568198154f39ab6957b1d6f380c9` given in the task brief (shared
  with `spriteID: 5e97eb03825dee720800000000000000`, matching the pattern
  already used across this project's hand-written metas). No Unity import ever
  ran against it, so texture-importer settings inherited from `MagmaExplosion`
  (compression, max size, sprite pivot at 0.5/0.5, `alphaIsTransparency: 1`)
  were not independently re-verified as correct for this new sprite.
