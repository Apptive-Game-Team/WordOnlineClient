# 2026-07-23 — issue #377 사운드 교체 워크리스트

- Date: 2026-07-23
- GitHub Issue: #377
- Status: Draft
- Parent plan: `.plan/2026-07-20-issue-377-sfx-redesign.md` (gates/architecture)
- Style authority: `.plan/issues/2026-07-21-issue-377-sfx-style-guide.md`
  (miniature pop-up-book diorama Foley, 2026-07-23 revision)

## Goal

Replace every currently-bad sound with a listening-approved miniature-diorama
Foley asset, in a fixed order, using the 4-candidate + post-processing +
user-listening pipeline. Record which sounds are bad, why, and what replaces
them.

## User decisions (2026-07-23, recorded verbatim intent)

1. **Legacy game-object sounds are muted immediately** (before replacements
   exist). Silence over wrong identity. UI sounds (button/card/field) stay
   audible until each replacement is approved.
2. **Lobby BGM timbre: wooden-mallet marimba** 3–4 note round, long rests,
   very quiet.
3. **Card hover: very small paper brush**, clearly quieter than select, with
   hover cooldown. Not silent.
4. **Movement sounds only for heavy summons** — creatures whose magic recipe
   needs 4–5 cards. Everything else (including all other creatures) has no
   movement sound. Buildings are always silent movers.

Heavy-summon movement roster (from authoritative recipe seed
`database/migration/V032_20260723__backfill_dev_gameplay_parameters.sql`):

| Recipe cards | Magic | Runtime type | Element profile |
|---:|---|---|---|
| 5 | `magma_spirit` | `MagmaSpirit` | Fire |
| 4 | `fire_lord_spirit` | `FireLordSpirit` | Fire |
| 4 | `bubble_spirit` | `BubbleSpirit` | Water |
| 4 | `dimension_toad` | `DimensionToad` | Rock |
| 4 | `will_o_wisp` | `WillOWisp` | Nature |

(5-card `meteor_shower` / `tornado_strike` are transient spells — no
lifecycle movement by design.)

Profile consequence: movement is a profile-level slot, so heavy movers get
dedicated shared profiles (`FireCreatureHeavy`, `WaterCreatureHeavy`,
`RockCreatureHeavy`, `NatureCreatureHeavy`) with movement enabled; the base
creature profiles keep movement disabled. This uses the parent plan's
documented-exception rule instead of per-prefab overrides.

## Bad-sound inventory and verdicts

| Current asset | Where it plays | Why it is bad | Action |
|---|---|---|---|
| `Game/hit.wav` | every unit's **attack** via nested `HitSoundPlayer` in 7 Abstract prefabs | one sound for all attacks; misnamed; double volume attenuation bug | mute now; replaced by per-profile attack |
| `Game/Magic/shoot.wav` | `AbstractShot`, `ChainLightning`, `TideCall`, tutorial `FireShot` | same launch for every element | mute now; per-element release later (Gate G) |
| `Game/Magic/explosion.wav` | `AbstractExplode`, `ShockOverload`, `TornadoStrike` | shared explosion, oversized scale | mute now; per-element miniature impact later |
| `Game/Magic/wind.wav` | `SandStorm`, `RazorGale` | cross-concept reuse | mute now |
| `Game/Magic/drop.mp3` | `AbstractDrop` | 24 kHz MP3, ineligible format, generic | mute now |
| `Game/heal.wav` | `ManaWell`, `HealingTotem`, `WindTotem`, `LifeTree` | attack-vs-heal ownership ambiguous, stereo 24-bit | mute now; redesign with heal semantics |
| `Game/light_explode.wav` | `AbstractDrop`, effects | mixed event reuse | mute now |
| `Game/Magic/explode/*.wav` (5) | element explodes; `rock_explode` also covers leaf/water/overgrowth | wrong element assignments, 24-bit | stay unmapped/muted until per-element batches |
| `UI/wood_button_click.wav` | all buttons (`SoundAssets.ClickButton`) | legacy anchor, pre-diorama character | **replace first** (felt-damped wood knock) |
| `Game/Card/card_touch_v2.wav` | hover AND select AND tutorial | one clip owns three gestures | replace + split hover/select/deselect/draw |
| `Game/field_confirm.wav` | `FieldSelector` valid target | unapproved generated clip | replace with fingertip soil/grass tap |
| `Game/Card/draw_card.wav` | unwired | draw event has no owner yet | new draw asset + wiring per parent plan rules |
| `BGM/12 Pixel Tracks/Pixel 8.wav` | Lobby, Login, Register, ManageDeck, Result, Admin (`bgmClip`) | pixel-pack track, opposite of target mood | **replaced 2026-07-27** by `Diorama/lobby_marimba_round_v1.wav` |
| `BGM/12 Pixel Tracks/magic-book-bgm.wav` | MagicBook, Adventures, Adventure (`bgmClip`) | same pack, not in the diorama palette | **replaced 2026-07-27** by `Diorama/magicbook_wood_box_v1.wav` |
| `BGM/25 Rpg Game Tracks/lobby-bgm.wav`, `lobby-bgm2.wav` | stale BgmPlayer `AudioSource` clips in 6 scenes (never the `bgmClip`) | 22.6 MB each shipped to WebGL for nothing | references repointed 2026-07-27; files now unreferenced, deletion pending user decision |
| `BGM/in-game-bgm.wav` | GameScene | — (approved forest v4) | keep; still needs Unity/WebGL loop verification |

## Non-goals

- No changes to `GameSfxPlayer`/catalog architecture beyond the heavy-profile
  addition.
- No batch generation of 30+ assets; one event resolved before the next.
- No lobby music work before all interaction/creature gates pass.

## Approach (Checklist)

- [ ] **Step 0: 레거시 즉시 무음화 (code)** — add a `LegacySfxMuter` step in
      `ObjectSpawner` that, on every spawned object, disables nested
      `OnAttackSoundPlayer` components and stops/mutes `playOnAwake`
      `AudioSource`s. UI sounds untouched. One commit, trivially revertible.
      Also fixes nothing else — double-attenuation bug becomes moot for muted
      owners.
- [ ] **Step 1: 버튼** — `ui-wood` family: 4 candidates, 0.5 s generation →
      0.22 s trim, felt-damped wood knock; postprocess + probe gates; user
      listening; WAV; wire to `SoundAssets.ClickButton`.
- [ ] **Step 2: 카드** — separate assets: hover(작은 종이 스침, select보다
      작게), select, deselect, draw. Hover cooldown per ownership matrix.
      Draw wiring only if stable card identity exists (parent plan rule).
- [ ] **Step 3: 필드 확정** — fingertip soil/grass tap, `TrySendInput`
      success-only.
- [ ] **Step 4: FireCreature 수직 슬라이스** — convert approved
      `fire_flare_candidate_04` to WAV (re-approval), spawn(종이 팝업+점화),
      hit, death(소품 붕괴) batches; `FireSlime` fixture; activate whole
      profile per parent plan Gate B/D.
- [ ] **Step 5: StoneBuilding 슬라이스** — `GroundTower` fixture; spawn(돌
      내려놓기), hit, death(조약돌 쏟아짐); movement stays silent.
- [ ] **Step 6: 나머지 크리처 프로필** — Water, Nature, Lightning, Rock,
      Wind, Neutral; one event set per profile, representative prefab each.
- [ ] **Step 7: Heavy 이동음** — create 4 heavy profiles; movement clips
      (재질별, 450 ms cooldown) only for the 5 heavy summons; verify base
      profiles keep movement off.
- [ ] **Step 8: 건물/토템/룬 + transient 교체** — per parent plan Gates F/G;
      exact ownership snapshot before touching projectile audio.
- [x] **Step 9: 로비 마림바 돌림노래** — done 2026-07-27, ahead of Steps 4–8
      because the user asked for the BGM work first. 30 s loop, 4 candidates
      per family, loop-seam check, user listening approval. Also covered the
      MagicBook family, which the original plan omitted.
      - Lobby: candidate 03 approved → `Art/Sounds/BGM/Diorama/lobby_marimba_round_v1.wav`
      - MagicBook: candidate 04 approved → `Art/Sounds/BGM/Diorama/magicbook_wood_box_v1.wav`
      - Generation path: elevenlabs MCP was disconnected, so direct REST
        `POST /v1/music` (`music_v2`). The Music API has no `prompt_influence`
        and no `looping` field — design targets only, not applied.
      - `postprocess_candidate.py` was **not** used: its strip-silence and
        tail-fade steps destroy a loop. A loop-safe chain (mono → peak
        normalize → PCM_16 WAV, no trim, no fade) was used instead.
      - In-game level is **not** baked into the files (both sit at peak
        -6 dBFS). BGM must end up quieter than SFX — set that on the
        BgmPlayer `AudioSource`, not by re-normalizing an approved asset.
      - Remaining: Unity import/Play Mode/WebGL loop verification, and the
        1.04 s rest across the lobby loop point (0.52 s head + 0.52 s tail)
        if it proves audible in context.
- [ ] **Step 10: 최종 검증** — Unity compile, catalog validator, Play Mode,
      WebGL smoke, volume 0/50/100, reconnect/overlap; only then PR #378
      Ready.

Every audio step follows the create-game-audio skill pipeline: normalized
request → 4 candidates → `postprocess_candidate.py` → `audio_probe.py`
auto-reject → user listening → golden hash + manifest record → WAV →
Unity verification. No candidate is wired before listening approval.

## Richness expansion (2026-07-23, user request "좀 더 풍성하게")

Density comes from per-family material identity, not louder sounds. Additions
on top of Steps 4–9, all inside the miniature diorama style:

| New event | Direction | Owner | Priority |
|---|---|---|---|
| Spawn pop-up layer | every profile spawn = paper unfold + material arrival (already the spawn grammar; make it audible per family) | profile spawn slot | with each profile gate |
| Match result — win | warm ascending wooden knock triple, dry, no melody instrument other than wood | ResultScene enter | after Step 6 |
| Match result — lose | single low soft prop tip-over onto felt, short | ResultScene enter | after Step 6 |
| Card draw | paper slide + deck separation (Step 2 leftover; needs stable card identity) | hand reconciliation | when identity lands |
| Mana full cue | one very quiet wooden tick (optional; cut if it fights ambience) | mana UI | last, optional |
| Scene transition | tiny cloth air movement, near-silent (optional) | scene loader | last, optional |

Explicitly NOT added: blocked/invalid-action sounds (stay silent per
ownership matrix), hover variants per card, per-prefab unique clips.

## Validation

- **Commands to run:** `Tools > Sound > Create or Update Baseline Object SFX
  Catalog`, `Tools > Sound > Validate Object SFX Catalog`, Editor Play Mode
  vertical slice, WebGL smoke build.
- **Expected output:** validator 0 errors; muted legacy owners produce no
  audio; each approved event audible at volume 50; unknown types silent.

## Risks & Rollback

- **Risks:** total silence phase may feel dead during development (accepted
  by user decision 1); heavy-profile split adds 4 profiles to maintain;
  ElevenLabs miniature-scale prompts unproven for marimba round (may need a
  music-capable path instead of SFX endpoint).
- **Rollback steps:** revert the `LegacySfxMuter` commit to restore legacy
  audio; per-profile slot disable for any regressing family; git revert per
  migration commit (parent plan rollback rules apply).

## Open Questions

- Heal sound semantics: `WindTotem`/`LifeTree` reference `heal.wav` today —
  is heal feedback wanted at all in v1, or silent until proven need?
- Tutorial prefabs (`Tutorial/AquaArcher`, `Tutorial/FireShot`) — mute with
  the same Step 0 mechanism, or keep tutorial audio as-is until Step 6?
- Draw sound: if stable card identity is not available in reconciliation
  data, draw is dropped from #377 per parent plan — confirm when Step 2
  starts.
