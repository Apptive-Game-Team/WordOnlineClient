# 2026-07-23 — 효과음 사운드 스타일 가이드

- Date: 2026-07-23
- Revised: 2026-08-10 (issue #475) — metal exception, hellfire material correction
- GitHub Issue: #377
- Status: Revised after listening feedback — review pending

## Goal

Define one audible language for every SFX added by issue #377. UI, cards,
field interaction, creatures, buildings, and magic must sound like parts of the
same game even when they use different elemental materials.

The target identity is **grounded close-up fantasy Foley**:

- physically believable wood, paper, stone, soil, water, cloth, air, ember, and
  electrical sources
- magical through restrained natural energy, not tonal sparkles or synthetic spectacle
- short and readable during crowded combat
- calm and serious without becoming dark fantasy or cinematic
- dry and close, as if a real small object acts on the game board in front of the player

Tone decision for v1: **realistic, restrained, and material-first**. The visuals
may be stylized, but the audio must not sound cute, toy-like, comic, bouncy, or
playfully exaggerated. Serious moments gain weight through real material mass,
not trailer bass or metallic fantasy accents. Player and enemy ownership remains
visual-only in v1.

## Non-goals

- cinematic trailer sound design
- large-scale battlefield simulation
- unique sound files for every prefab
- voice, creature dialogue, or UI narration
- procedural synthesis or a runtime DSP framework
- automatic acceptance based only on waveform metrics

## Context / Constraints

- Current Draft PR #378 contains mechanically reused sounds that do not fit many objects.
- Sound-family profiles will be shared by archetype and concept.
- Final approval requires listening in Unity, not only file inspection.
- ElevenLabs may generate candidates, but generated output is never accepted automatically.
- Final runtime one-shots use WAV PCM 16-bit, 44.1 kHz, mono. The approved
  in-game ambience is an explicit PCM 16-bit, 48 kHz, mono loop exception whose
  reviewed hash is preserved.
- UI and game volumes remain independently controlled by `SoundData`.
- This guide supplements the implementation plan; it does not replace event ownership rules.

## Sonic North Star

Use this sentence to judge every candidate:

> A real small object made of wood, paper, stone, plant fiber, liquid, ember, or
> air was touched, moved, struck, or released at close range.

A candidate fails when it sounds imported from another genre even if it is
technically polished.

### Miniature scale rule (2026-07-23)

The game presents itself as a handcrafted pop-up-book diorama:
`PopupBookVisualPresenter` unfolds units like book pages, sprites are flat
cartoon cutouts, and the UI is wood and paper. The audio matches that identity
by keeping every source **real but miniature**. One judgment sentence decides
scale:

> Could the object making this sound rest in an open hand?

Consequences:

- Explosions are not restrained; they are unnecessary. A rock golem's death is
  a handful of pebbles spilling onto a table, not a detonation.
- Cinematic low end disappears naturally because hand-scale props have no
  sub-bass.
- Death and despawn read as a prop collapsing, tipping, or scattering into its
  component material.
- Spawn may combine one paper pop-up/unfold layer with one material arrival
  layer, staying inside the three-layer grammar below. The pop-up layer ties
  audio directly to the spawn presentation.
- The lobby's musical identity is a quiet wooden-mallet (marimba/xylophone
  with felt mallets) three-to-four-note round with long rests — wooden timbre,
  no metallic chime, barely above the ambience.

### Elemental material palette (miniature sources)

| Family | Real hand-scale source |
|---|---|
| UI button | dense wood prop pressed, felt-damped short knock |
| Card | coated paper fiber plus felt/wood surface contact |
| Field confirm | fingertip tap on soil or dry grass, very small |
| Spawn (shared) | paper unfold layer + family material arrival |
| Fire (hellfire legion) | a dry paper husk cracking open, embers leaking from the split |
| Water | fingertip drips, slosh inside a small cup |
| Nature | dry leaf rub, seed pod, bent twig |
| Rock | pebbles knocking, gravel grind, stone set down |
| Lightning | real static discharge crackle on cloth (not a digital tick) |
| Wind | short air movement from cloth or a hand fan |
| Human structure | bronze coupling seating, a plate taking load — non-ringing metal only |
| Dimension wanderer | one contact, then the same contact again 0.1 s later, slightly lower and thinner |
| Building | wooden peg placement or stone set-down; collapse of parts on death; never movement |

Fire was `match ignition, dry twig catching, small ember pops` until 2026-08-10.
The art direction states that every fire unit is a demon of the burning legion,
**not a fire spirit**, so ignition is the wrong gesture — the husk cracking is
the right one. See issue #475.

### Required character

- **Tactile:** recognizable physical contact at the start.
- **Compact:** one clear gesture, little unused tail.
- **Material-led:** wood, paper, stone, water, leaf, cloth, glass, or electricity.
- **Magic-supported:** an elemental component is allowed only when it sounds
  like the real source and reinforces the gesture.
- **Readable:** event remains identifiable when several sounds overlap.
- **Restrained:** no sound should imply a scale larger than the object on screen.

### Forbidden character

- cinematic bass drops, sub booms, or trailer impacts
- guns, metal weapon clashes, machinery, or industrial hydraulics
- science-fiction lasers, plasma weapons, digital beeps, or hologram sweeps
- EDM risers, musical stingers, chord progressions, or obvious melody
- long cathedral reverb, huge spaces, or distant battlefield ambience
- realistic gore, bone breaks, wet flesh, screams, or animal pain
- harsh white-noise wind, piercing electricity, or brittle glass fatigue
- identical explosion treatment for spawn, attack, hit, and death
- bells, coins, swords, metallic chimes, glass chimes, or tonal pings unless the
  visible source explicitly contains that material
- cartoon pops, boings, toy mechanisms, comedy impacts, or exaggerated squash
- radio crackle, speaker breakup, codec chatter, digital ticks, or electrical
  interference that is not part of the depicted event

### Metal exception — human structures only (2026-08-10)

The metal ban above carries an escape clause: a material is allowed when the
visible source explicitly contains it. Exactly one faction qualifies.

`.art/magic/pages/` describes human magic-civilization structures as **cold
stone, steel-blue metal, and bronze couplings** — `GroundCannon`, `GroundTower`,
`RallyingTotem`, and the human half of `Towerback`. Those are the only places
metal may be heard, and only under both conditions:

- **Non-ringing metal.** Bronze being tightened, a coupling seating, a plate
  taking load. Never a bell, chime, blade, or coin. The `audio_probe.py`
  metallic filter (`ring_prominence_db >= 12 && hf_decay >= 0.15 s`) still
  applies unchanged — a candidate that rings is still rejected.
- **Structures only.** Human infantry (`ChickenCommando`) carries leather,
  cloth, and wood. No metal on a soldier.

Every other faction keeps the ban absolute. The rock golem tribe is the strict
case: its art direction requires it to read as **warm stone and moss, distinct
from human grey machinery**, so metal there is zero regardless of context.

## Sound Construction Grammar

Each clip uses at most three layers. More layers require a documented exception.

1. **Transient:** identifies the interaction in the first 10–40 ms.
2. **Material body:** communicates object or elemental material.
3. **Natural energy tail:** short flame, air, liquid, grit, leaf, or electrical
   decay; optional and non-tonal.

Default balance:

- transient: 40–55% of perceived identity
- material body: 35–50%
- natural energy tail: 0–20%

The elemental layer must not hide the event. A fire hit must first read as a
hit, then as fire. A water button must still read as a button if such a themed
button is ever introduced.

Tonal accents are disabled by default. Avoid bells, chimes, chords, melody, and
metallic resonances. Tails end cleanly rather than fading through unrelated
actions.

## Interaction Style

### UI button

Identity: warm, crafted wooden mechanism.

- transient: rounded wood contact, not a sharp plastic mouse click
- body: small hollow wood clack
- release: optional quiet mechanical tick
- duration: 120–260 ms
- pitch: low-mid; stable across all screens
- hover: silent
- disabled click: silent
- avoid: desk mouse, keyboard key, door knock, block impact, cartoon boing

All standard buttons use one approved anchor sound. Variants are allowed only
after the anchor passes and must preserve the same material and loudness.

### Card touch

Identity: coated paper card moving over felt or wood.

- hover: light paper/felt brush, 80–160 ms
- select: firmer paper tap or short slide, 100–220 ms
- deselect: same family with slightly softer release
- draw: paper slide plus small deck separation, 180–350 ms
- avoid: page tearing, plastic sheet, cash register, whip, large book page

Hover must be quieter than select. Repeated hover should not create a constant
paper storm; cooldown and pointer transition rules apply.

### Field target confirmation

Identity: fingertip or token confirming a location on an earthy magical board.

- transient: light dry tap
- body: tiny soil, felt, or wood-board contact
- energy tail: optional muted soil or air response under 80 ms
- duration: 80–180 ms
- pitch: above button body so field and button remain distinguishable
- avoid: wooden button clack, camera shutter, rock drop, footstep, UI beep

This sound communicates successful input submission, not damage or spell cast.

## Elemental Material Palette

### Fire (burning legion)

Core materials: dry paper husk, ember, charcoal, slow lava pressure.

- transient: a thick husk splitting, never an ignition and never a metallic snap
- body: embers leaking from the split under low slow pressure
- tail: very short air-fed ember decay
- frequency character: warm midrange, highs pressed down so only embers remain
- avoid: match strike, gasoline blast, pressure explosion, fireplace loop, huge
  explosion, dragon roar, the “화르륵” flare, sword-like “챙”, bell, coin, metal,
  or glass

Every fire unit belongs to the burning legion and is a demon, not a fire spirit.
Ignition reads as a spirit catching light; the husk reads as a demon opening.
Revised 2026-08-10 (issue #475) — the previous text asked for ignition and a
“화르륵” flare, both of which the concept document had already forbidden.

### Water

Core materials: droplet, bubble membrane, shallow splash, rounded glass.

- transient: soft droplet or bubble pop
- body: compact liquid movement
- tail: short glassy shimmer without melody
- frequency character: clear high-mid with little low-end weight
- avoid: ocean wave, toilet/plumbing, heavy rain bed, ice unless explicitly themed

### Nature

Core materials: leaf, seed pod, flexible wood, vine fiber, soft soil.

- transient: seed tick or twig flex
- body: leaf/fiber movement
- tail: subtle organic rustle
- frequency character: warm, textured midrange
- avoid: using generic wind as nature, jungle ambience, bird calls, tree-fall boom

### Lightning

Core materials: static snap, tight electrical arc, charged glass.

- transient: extremely short electrical crack
- body: controlled buzzing pulse
- tail: tiny charge decay
- frequency character: bright and fast, with harsh bands controlled
- avoid: science-fiction laser, taser realism, long mains hum, piercing 3–6 kHz fatigue

### Rock

Core materials: stone knock, grit, ceramic fracture, compact earth.

- transient: dense stone contact
- body: small grit or rubble movement
- tail: short low-mid resonance
- frequency character: heaviest family, but no sub-bass
- avoid: mountain collapse, metal clang, concrete demolition, long rolling boulder

### Wind

Core materials: cloth pass, narrow air cut, feather, paper edge.

- transient: soft air displacement or cloth flick
- body: shaped short whoosh
- tail: almost none for repeated movement
- frequency character: light broadband energy with softened highs
- avoid: raw white noise, storm ambience, vacuum suction, jet engine, long whoosh tail

### Generic arcane

Core materials: muted glass, ceramic rune, felt-damped chime, soft energy pulse.

- use only for genuinely neutral magic
- keep tonal content to one muted note
- avoid: defaulting unknown prefabs to this family; unknown remains silent
- avoid: casino sparkle, achievement fanfare, digital notification

## Archetype Treatment

Element defines material accent. Archetype defines scale, weight, and motion.

### Creature

- compact organic or crafted body movement under the element
- movement is intermittent, never a continuous loop for grounded creatures
- attacks emphasize gesture and contact rather than vocalization
- death sounds like loss of magical structure, not a universal explosion

### Slime

- softer, rounder transient than other creatures
- small elastic or moist body allowed, but never gross or sticky-realistic
- elemental accent stays subtle

### Aerial creature or spirit

- lighter body and shorter low-frequency content
- movement may use sparse cloth/air accents
- do not loop a loud wind bed

### Building, tower, totem, rune, or nest

- spawn emphasizes placement, assembly, or magical anchoring
- movement is silent
- attack retains structural weight but must not sound like a creature gesture
- hit uses wood/stone/ceramic body according to profile
- death uses collapse or energy release scaled to the sprite, not a cinematic blast

### Projectile

Treat launch, flight, and impact as different events.

- launch: short release gesture
- flight: silent by default; loop only when identity would otherwise be lost
- impact: material contact plus elemental accent
- do not duplicate caster attack or target hit sounds

### Transient spell object

- play only the events needed to explain the spell
- no artificial movement, hit, heal, or death sounds just because lifecycle hooks exist
- `Drop`, `Explode`, and `Field` use explicit profiles with intentional silence

## Event Style

### Spawn

Meaning: magical arrival or construction completion.

- shape: brief energy onset followed by material settle
- creature: 250–650 ms
- building: 400–900 ms
- transient spell: often silent when cast/impact already explains arrival
- avoid: reusing death explosion backward, generic item drop for everything

### Movement

Meaning: readable displacement, not constant activity.

- shape: tiny step, brush, roll, flutter, or body shift
- duration: 80–260 ms
- sparse cadence with minimum 450 ms per object
- static archetypes: silent
- avoid: one universal wind clip

### Attack

Meaning: action release by attacker.

- shape: 20–80 ms preparation cue plus short release gesture
- duration: 150–600 ms
- projectile impact remains separate
- preserve approved legacy attack sounds until replacement family passes
- avoid: playing target hit sound as attacker sound

### Hit

Meaning: damage confirmed on target.

- shape: dry contact first, elemental accent second
- duration: 100–350 ms
- target archetype determines body material
- repeated hits must remain tolerable
- avoid: full explosion for ordinary damage

### Heal

Meaning: confirmed HP recovery after initial state.

- shape: soft upward energy gesture without a melodic phrase
- duration: 250–600 ms
- quieter and smoother than hit
- avoid: angel choir, reward jingle, bright notification bell

### Death

Meaning: object loses structure and leaves play.

- shape: decisive break/collapse followed by short elemental decay
- duration: 350–1000 ms
- stronger than hit but proportional to object size
- no shared universal explosion
- avoid: long tail masking the next turn or action

## Mix Hierarchy

Perceived priority:

1. direct player UI confirmation
2. death and major spell resolution
3. confirmed hit and heal
4. attack
5. spawn
6. movement and hover

This hierarchy is event-only in v1 and matches `GameSfxPlayer`. Player and enemy
ownership does not change priority, volume, brightness, or profile selection.
UI remains outside the game-voice cap.

Initial loudness targets measured on exported candidates:

- UI button/select: approximately -18 LUFS, true peak <= -1 dBFS
- card hover/movement: approximately -22 LUFS
- field confirmation: approximately -20 LUFS
- attack/hit: approximately -20 LUFS
- spawn/heal/death: approximately -21 to -19 LUFS by scale

Short-SFX LUFS is only a guardrail. Final balance is decided in the Unity mix.

Frequency constraints:

- remove unnecessary energy below 80 Hz
- reserve 100–250 Hz weight for rock, buildings, and important deaths
- control repeated harshness between 2.5–6 kHz
- avoid stacking bright tails across water, lightning, and arcane sounds

## Background Audio

Background audio supports space and pacing; it never competes with interaction
or combat feedback.

### In-game forest ambience

- approved anchor: intermittent natural forest air with candidate v4 as the
  current golden source
- mostly quiet woodland air with audible wind arriving and receding irregularly
- no obvious musical pitch, rhythm, percussion, radio crackle, speaker breakup,
  hard edit, or repeating gust at the loop boundary
- wind is more present than the earlier v3 candidate, but silence and low-air
  intervals remain part of the loop
- game SFX must remain readable above the ambience at normal game volume

### Lobby music

- very quiet, calm, repetitive round-like motif
- light and neutral rather than dark fantasy, heroic, ominous, or sentimental
- sparse arrangement, no trailer percussion, large choir, heavy bass, or bright
  lead instrument
- the loop may be noticed when listening for it, but should recede during UI use
- remains provisional until a dedicated four-candidate listening pass is approved

## Variation Policy

- v1 uses exactly one approved anchor per profile event plus cooldown and voice limits.
- Runtime random pitch and multi-clip variants are disabled in issue #377.
- Deterministic variants require a stable identity/sequence contract and profile
  schema changes, so they belong to a follow-up issue after repetition testing
  proves the need.
- Element and archetype sections are production guidance, not instructions to
  create their Cartesian product as profiles.
- Approve the initial small shared-profile list before asset production. Add a
  profile only when multiple runtime types have a repeated audible need that no
  approved profile can express.

## Naming and Folder Convention

Final assets:

`Assets/Resources/Sound/Game/Sfx/<family>/<event>/<family>_<event>_<variant>.wav`

Examples:

- `ui/button/ui_button_wood_click_01.wav`
- `card/select/card_select_paper_01.wav`
- `fire_creature/hit/fire_creature_hit_01.wav`
- `stone_building/death/stone_building_death_01.wav`

Candidate assets stay outside Unity `Assets` in a gitignored workspace folder:

`.sfx-work/issue-377/<family>/<event>/`

An Editor-only audition tool loads external WAV candidates from this folder by
local file URI. Candidates receive no Unity `.meta` files and are never staged.
Only the approved, normalized WAV is copied into runtime `Assets/Resources`.

Candidate naming:

`<family>_<event>_candidate_<a|b|c|d>_<yyyymmdd>.<ext>`

## ElevenLabs Prompt Standard

Prompt order:

1. event and game scale
2. archetype and physical material
3. elemental transient/body/tail sequence
4. exact duration target
5. mix perspective and loudness character
6. negative constraints

Template:

> [Material/source] performing [action/event] in [environment/space], heard from
> [distance/perspective]. [Standard audio tags]. Sequence: [temporal onset],
> then [material body], then [natural decay]. [duration] seconds, realistic,
> dry, close-up mono game SFX. No music, no voice, no long reverb, no cinematic
> boom, no cartoon character, no metal or tonal chime unless physically shown,
> no radio crackle, no speaker breakup, no clipping.

For ElevenLabs Sound Effects, use explicit duration, `looping=false` for
one-shots, and target `prompt_influence≈0.7` whenever the connector exposes it.
Ambience uses an explicit duration and `looping=true`. The currently connected
MCP does not expose prompt influence, so that limitation must be recorded for
every generated batch rather than silently treated as satisfied.

Every prompt must name the actual material. Prompts such as “epic fire sound”
or “cool magic effect” are rejected before generation.

## Candidate Evaluation Rubric

Four mandatory gates:

- event is immediately clear
- scale, material, and overall style fit the game
- overlap remains readable and repetition remains comfortable
- user/dev-yunseong explicitly approves the candidate

Any forbidden-character violation fails immediately. Optional 1–5 notes for
material identity, elemental identity, mix fit, or candidate comparison may be
recorded, but no numeric average is required.

Approval records include short reasons, not only “A selected.” Rejection reasons
become negative constraints for the next generation batch.

## Approach (Checklist)

- [ ] **Step 0: Recon**
  - [ ] Inventory only SFX referenced by current owners or eligible for activation
        under issue #377.
  - [ ] Mark each clip as approved anchor, reusable candidate, or rejected.
  - [ ] Record current duplicate owners and bad mappings from PR #378.
- [ ] **Step 1: Style anchors**
  - [ ] Evaluate existing assets for the current event first.
  - [ ] If none passes, produce A/B/C/D candidates for that one event only.
  - [ ] Evaluate and approve or reject that event before moving to the next event.
  - [ ] Complete button, then `FireCreature`, then `StoneBuilding` events
        sequentially; never batch the full vertical slice candidate set.
  - [ ] Evaluate candidates with the mandatory gates in Unity.
  - [ ] Record user approval and hashes in the asset manifest.
- [ ] **Step 2: Implementation**
  - [ ] Activate only approved anchors in shared profiles.
  - [ ] Apply mix hierarchy and voice policy from the implementation plan.
- [ ] **Step 3: Tests**
  - [ ] Compare isolation and the specified overlap scenarios below.
  - [ ] Test volume 0/50/100 on headphones and speakers.
  - [ ] Verify repeated hover, movement, and hit sounds do not cause fatigue.
  - [ ] Verify WebGL playback matches Editor closely enough.
- [ ] **Step 4: Rollout / Rollback**
  - [ ] Migrate one sound family per reviewable commit.
  - [ ] Keep the previous approved anchor until replacement passes.
  - [ ] Map rejected/unready families to intentional silence.

## Validation

- **Commands to run:**
  - audio format, duration, channels, peak, clipping, silence, and SHA-256 checks
  - Unity EditMode asset-manifest validation
  - Unity Play Mode audition scene
  - final WebGL smoke build
- **Expected output:**
  - every active clip matches technical requirements
  - every active hash exists in the approved manifest
  - all four mandatory evaluation gates pass with recorded approval
  - no forbidden sound character appears in active profiles
  - event identity remains clear under representative overlap

### Required overlap scenarios

1. **Interaction:** for 30 seconds, card hover transitions at 4 per second,
   button clicks at 2 per second, and valid field confirmations every 2 seconds.
2. **Fire creature:** `FireSlime` spawn, two movement voices, approved profile
   attack, hit, and death exercised within a 1-second window; verify the
   inherited legacy attack owner stays disabled and silent.
3. **Stone building:** `GroundTower` spawn, attack/projectile, hit, and death
   exercised within a 1-second window; movement stays silent.
4. **Voice pressure:** fill each category cap and the overall cap, then submit
   lower, equal, and higher-priority requests to verify deterministic admission.

Pass when every required event remains identifiable, priority ordering matches
the plan, no unintended duplicate plays, and no audible clipping or distortion occurs.

### Fatigue scenarios

- card hover: 4 transitions per second for 30 seconds
- movement: 2 active voices at the 450 ms cooldown for 60 seconds
- hit: 4 hits per second for 30 seconds

Pass requires no clipping, no piercing frequency buildup, no volume escalation,
and explicit user approval that the sequence does not create an immediate need
to mute or substantially lower the category. Failure returns the anchor to
candidate selection; variants are not introduced as a workaround in v1.

### Editor and WebGL parity

Record the same vertical-slice sequence in Editor and WebGL.

- no event may be missing, duplicated, stuck, or reordered
- no event may have a clearly perceptible timing regression against its visual cue
- relative priority and balance must not collapse between Editor and WebGL
- neither playback may clip or produce codec distortion judged unacceptable by
  the approver during the same listening setup

Any violation blocks Ready status even when Editor playback passes.

## Risks & Rollback

- **Risk:** Elemental accents become more prominent than event meaning.
  - **Mitigation:** transient-first grammar and rubric event-clarity gate.
- **Risk:** Too many shared profiles fragment the style.
  - **Mitigation:** new profile requires audible justification and anchor comparison.
- **Risk:** Too few profiles make unrelated objects sound identical.
  - **Mitigation:** require a demonstrated repeated use case, then evaluate a new
    shared profile in a separate follow-up issue; v1 does not add variants.
- **Risk:** Generated candidates sound polished alone but fail in combat.
  - **Mitigation:** isolation plus overlap approval is mandatory.
- **Risk:** Loudness metrics create false confidence.
  - **Mitigation:** metrics are guardrails; Unity listening remains final authority.
- **Rollback steps:**
  - disable the affected profile event
  - map unapproved family to intentional silence
  - restore the last approved anchor by hash
  - revert the family-specific commit

## Open Questions

- None before vertical-slice production. Existing clips have no pre-approved
  status; the inventory and mandatory gates decide whether one becomes the first
  anchor.

## Review Record

- Fast first pass: NONPASS; decision gates, overlap, fatigue, WebGL, and variant
  identity findings incorporated.
- Medium first pass: NONPASS; v1 variants, loudness authority, candidate volume,
  candidate storage, profile growth, rubric, and inventory findings incorporated.
- Fast re-review: PASS, followed by a final cross-plan priority finding.
- Medium re-review: PASS.
- Cross-plan priority conflict resolved with event-only v1 hierarchy.
- Fast final review: PASS.
- Medium final review: PASS.
- Heavy final review: PASS.
