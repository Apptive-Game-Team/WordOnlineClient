# Issue #377 SFX Asset Manifest

## Status

- Inventory snapshot: 2026-07-21
- Auditory approval: none
- Authority: `.plan/issues/2026-07-21-issue-377-sfx-style-guide.md`
- Runtime activation rule: a clip stays provisional until it passes isolated and in-game listening.

`Referenced by` records serialized Unity references. Clips loaded by a Resources
string are recorded under `Runtime owner` even when no GUID reference exists.

| Asset | Runtime owner / serialized references | Current event | Technical format | Style status | Eligible target | SHA-256 |
|---|---|---|---|---|---|---|
| `UI/wood_button_click.wav` | `SoundAssets.ClickButton`; `ButtonBase`; `GlobalButtonSoundPlayer` | button click | PCM 16-bit, 44.1 kHz, mono | Needs audition; legacy anchor only | UI / button | `8494153e…236a` |
| `Game/Card/draw_card.wav` | `SoundAssets.DrawCard`; currently not played | card draw | PCM 16-bit, 44.1 kHz, mono | Needs audition; unreferenced | Card / draw | `3d394d6d…ac56` |
| `Game/Card/touch_card.wav` | `CardUI`, `CardUIZoom`, `TutorialCardUI`; game/tutorial/spectating scenes | hover/select touch | PCM 16-bit, 44.1 kHz, mono | Needs audition; one clip currently owns multiple gestures | Card / hover or select, not both unless approved | `d978347a…9065` |
| `Game/Magic/drop.mp3` | `AbstractDrop.prefab` | drop/cast | MP3, 24 kHz, joint stereo | Ineligible final format; needs audition | Transient spell / drop only | `27fedbb5…e976` |
| `Game/Magic/explosion.wav` | `ShockOverload`, `AbstractExplode`, `TornadoStrike` | attack/explode | PCM 16-bit, 44.1 kHz, mono | Needs audition; mixed ownership | Neutral transient / explode | `7f370458…f7d8` |
| `Game/Magic/shoot.wav` | `ChainLightning`, `TideCall`, `AbstractShot`, tutorial `FireShot` | launch/attack | PCM 16-bit, 44.1 kHz, mono | Needs audition; cross-element reuse | Neutral projectile / launch | `78d92f60…4ebc` |
| `Game/Magic/wind.mp3` | `SandStorm`, `RazorGale` | cast/attack | MP3, 24 kHz, joint stereo | Ineligible final format; needs audition | Wind spell / attack | `bb96b301…b293` |
| `Game/Magic/explode/fire_explode.wav` | `MagmaExplosion`, `FireExplode`, `MagmaFist` | impact/explode | PCM 24-bit, 44.1 kHz, mono | Requires conversion if approved; needs audition | Fire / impact or death | `1f7f4afe…5245` |
| `Game/Magic/explode/lightning_explode.wav` | unreferenced | impact/explode | PCM 24-bit, 44.1 kHz, mono | Requires conversion if approved; needs audition | Lightning / impact | `e4e3c395…ca9f` |
| `Game/Magic/explode/rock_explode.wav` | `Overgrowth`, `LeafExplode`, `RockExplode`, `WaterExplosion` | impact/explode | PCM 24-bit, 44.1 kHz, mono | Needs audition; known concept mismatch risk | Rock / impact only | `9d9adbff…5bf` |
| `Game/Magic/explode/water_explode.wav` | `WaterExplode` | impact/explode | PCM 24-bit, 44.1 kHz, mono | Requires conversion if approved; needs audition | Water / impact | `accf9c9a…98c4` |
| `Game/Magic/explode/wind_explode.wav` | `WindExplode` | impact/explode | PCM 24-bit, 44.1 kHz, mono | Requires conversion if approved; needs audition | Wind / impact | `de2c5907…0ba2` |
| `Game/arrow_shot.wav` | unreferenced | intended launch | PCM 24-bit, 44.1 kHz, stereo | Requires mono/16-bit conversion if approved; needs audition | Physical projectile / launch | `135d0d1e…d1d3` |
| `Game/heal.wav` | `ManaWell`, `HealingTotem`, `WindTotem`, `LifeTree` | attack/heal | PCM 24-bit, 44.1 kHz, stereo | Needs audition; attack versus heal ownership ambiguous | Nature/arcane / heal | `3a92a257…2f3` |
| `Game/hit.wav` | `HitSoundPlayer.prefab`; tutorial `FireShot`, `AquaArcher` | attack and/or hit | PCM 16-bit, 44.1 kHz, mono | Needs audition; semantic ownership conflict | Generic target / hit only if approved | `45a7507e…c82` |
| `Game/light_explode.wav` | `AbstractDrop`, `Effects/Explode` | drop/explode | PCM 16-bit, 44.1 kHz, mono | Needs audition; mixed event reuse | Neutral magic / small impact | `8befb3f7…d39d` |

## Immediate Findings

- No clip is approved by this inventory alone.
- `drop.mp3` and `wind.mp3` cannot ship as final assets.
- Seven WAV files are 24-bit; three of those are stereo. Final approved copies
  must be PCM 16-bit, 44.1 kHz, mono without overwriting the source candidate.
- `rock_explode.wav` currently spans rock, nature, and water concepts. It must
  not remain a universal impact.
- `hit.wav` is attached to the shared attack player while its name implies a
  target hit. Ownership must be resolved before the vertical slice.
- Asset provenance and license are not recorded in the repository. Record both
  before accepting or replacing a clip.

## Approval Record Template

For every accepted clip, append a row containing candidate ID, source/tool,
license, prompt or recording note, editor, processing chain, isolated-listening
result, Unity-context result, approver, and approval date. A hash change
invalidates the prior approval.

## 2026-07-21 Generated Implementation Set

These clips were generated through ElevenLabs `text_to_sound_effects` as raw
44.1 kHz PCM, wrapped as mono 16-bit WAV, silence-trimmed, capped to the style
guide duration, given a 10 ms fade-out, and peak-balanced by event role. They
are runtime candidates pending in-game auditory approval; provenance is
ElevenLabs generation under issue #377.

| Runtime asset family | Files | Runtime use |
|---|---|---|
| Interaction | `wood_button_click_v3`, `card_touch_v2`, `field_confirm` | all buttons, card touch/hover, valid field submission |
| Creature body | `creature_spawn`, `creature_move`, `creature_hit`, `creature_death` | mobile creature lifecycle |
| Building body | `building_spawn`, `building_hit`, `building_death` | static building lifecycle |
| Shared state | `generic_attack`, `heal` | attack only without legacy owner; confirmed healing |
| Element accents | `fire_accent`, `water_accent`, `nature_accent`, `lightning_accent`, `rock_accent`, `wind_accent` | layered under spawn/attack/hit/death according to the explicit 93-type map |

The connector could not play audio because PortAudio is unavailable, and Unity
MCP was unreachable. Therefore these remain candidates even though they are
fully wired; the Draft PR cannot become Ready before a human listening pass.
