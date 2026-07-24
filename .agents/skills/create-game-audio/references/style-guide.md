# Arcane Casters Audio Style

Authoritative version: `.plan/issues/2026-07-21-issue-377-sfx-style-guide.md`.
This file is the working summary used during generation.

## Direction

**Pop-up-book diorama Foley.** The game shows a handcrafted tabletop diorama:
units unfold like pop-up-book pages, sprites are flat cartoon cutouts, the UI
is wood and paper. Every sound is therefore *real but miniature* — actual
physical materials recorded dry and close, at hand-prop scale. Magic may feel
uncanny, but its physical layer stays believable. Avoid toy-like, comedic,
glossy mobile-game, and exaggerated cartoon cues.

**Scale test (decides every candidate):** could the object making this sound
rest in an open hand?

- Explosions become unnecessary: a rock golem's death is a handful of pebbles
  spilling onto a table, not a detonation.
- No cinematic low end — hand-scale props have no sub-bass.
- Death/despawn = a prop collapsing, tipping, or scattering into its material.
- Spawn = one paper unfold/pop-up layer + one material arrival layer.

## Miniature Material Palette

| Family | Real hand-scale source |
|---|---|
| UI button | dense wood prop pressed, felt-damped short knock |
| Card | coated paper fiber plus felt/wood surface contact |
| Field confirm | fingertip tap on soil or dry grass, very small |
| Fire | match ignition, dry twig catching, small ember pops |
| Water | fingertip drips, slosh inside a small cup |
| Nature | dry leaf rub, seed pod, bent twig |
| Rock | pebbles knocking, gravel grind, stone set down |
| Lightning | real static discharge crackle on cloth (not a digital tick) |
| Wind | short air movement from cloth or a hand fan |
| Building | wooden peg / stone set-down on spawn; parts collapse on death; never movement |

## Event Map

| Context | Sound direction | Avoid |
|---|---|---|
| Wooden UI button | Short dry felt-damped wood press, small body resonance | Plastic tick, bubble pop, bright chime, desk mouse |
| Ground/mouse click | Tiny soil/grass fingertip contact for the target surface | Reusing the wooden button sound everywhere |
| Card hover/touch | Paper fiber, fingertip drag, light card flex | Coin, glass, metal edge, sparkle |
| Card select/place | Slightly firmer paper snap plus felt/wood contact | Casino flourish, magical bell |
| Prefab spawn | Paper pop-up unfold + family material arrival | Generic teleport sparkle on every prefab |
| Basic movement | Material-specific miniature movement, low repetition | Loud footsteps for hovering entities |
| Attack release | Source material motion and energy release at prop scale | Impact sound baked into the release |
| Hit | Target material impact; vary intensity | Same universal hit on all prefabs |
| Death/despawn | Prop collapse or scatter into component material | Explosion, comedic fall, victory jingle |
| Fire magic | Match-strike ignition, small flame catch, dry crackle | Sword clang, metallic ring, glass, chime, flamethrower scale |
| Lobby | Very quiet wooden-mallet (marimba) 3–4 note round, long rests | Metallic chime timbre, dark-fantasy score, dramatic melody |
| In-game field | Approved forest air v4 with intermittent wind (keep) | Constant loud wind, obvious short loop, music-forward mix |

## Cohesion

- Build each prefab family from the shared material palette above.
- Keep one approved anchor deterministic in v1; no runtime pitch randomization
  or layered accents to hide an unsuitable source clip.
- Separate release, travel, hit, and death sounds so gameplay timing remains editable.
- Reserve bright tonal cues for information that must cut through the mix.
- Keep the lobby musical identity calm and circular; keep the match primarily environmental.

## Review Gate

A candidate passes only after a human listening pass confirms:

- passes the hand-scale test — nothing implies an object bigger than a prop;
- no unintended metallic or chime transient;
- no cartoon or toy-like character;
- correct semantic timing for the event;
- acceptable repetition after at least ten rapid triggers;
- acceptable balance against ambience and other effects;
- no audible loop seam for looping clips;
- clean ending: the tail decays fully inside the file (no invented room ring,
  no boundary truncation click).
