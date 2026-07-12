# 2026-07-12 — Motion Sounds

- Date: 2026-07-12
- GitHub Issue: #345
- Owning repository: Apptive-Game-Team/WordOnlineClient (`client/`)
- Status: Implemented; Unity play-mode audio review pending

## Goal

Synchronize restrained CC0 movement sounds with the existing DOTween idle motions so units feel grounded without turning a populated board into continuous noise.

## Acceptance Criteria

- Hopping, Crawl, Waddle, and Stumble motions trigger a fitting sound at a repeatable point in their tween cycle.
- Prefabs can override motion sound by material: heavy, squish, magma, or wind.
- Attack aura release plays an element-specific existing game sound for fire, water, rock, wind, lightning, and nature.
- Wind Totem reuses the existing long wind asset as a low-volume loop.
- Playback follows `SoundData.gameVolume`, varies pitch slightly, and limits overlapping idle sounds.
- Shared code owns playback behavior; motion controllers only declare timing and sound character.
- Audio files are CC0, trimmed to only used assets, and documented with source and license.
- Existing prefab motion serialization remains compatible.

## Non-goals

- Attack, hit, magic, UI, or BGM replacement outside elemental aura release.
- Server or protocol changes.
- Adding sound to the one-time `PopIn` spawn animation.
- Redesigning the project's full audio architecture or settings UI.

## Context / Constraints

- Unity 2022 LTS and WebGL compatibility must be preserved.
- Existing motion controllers start infinite DOTween sequences in `Awake`.
- Many units can exist simultaneously, so an unbounded sound per tween loop would create noise and voice pressure.
- Current user changes in `FieldSelector`, `Selectable`, and `KeyInputSetting` are unrelated and must remain untouched.
- `main` already contains two unpublished local commits; branch/PR cleanup may be needed before publication.

## Affected Repositories and Contracts

- `client/` only.
- Internal contract: motion controllers supply a cycle duration and sound profile to shared playback code.
- No network, persistence, DTO, prefab-type, or server contract changes.

## Approach

- [x] Recon
- [x] Implementation
- [x] Focused validation
- [ ] Compatibility and regression validation
- [x] Release order and rollback check

1. Inspect exact tween cycle durations and prefab usage.
2. Select a small set of CC0 Kenney and OpenGameArt sounds for generic, heavy, squish, and magma movement.
3. Add a reusable motion-sound component with one-shot playback, pitch variation, and a global concurrency/cooldown guard.
4. Wire repeating motion controllers to the shared component and add prefab-level material overrides.
5. Reuse the existing long wind asset as a loop for Wind Totem.
6. Add element-specific audio to attack aura release while preserving each clip's audio tail.
7. Add Unity metadata and third-party asset documentation.
8. Compile the generated C# project when available; inspect serialized references and run Graphify update.

## Validation

- Commands:
  - `dotnet build Assembly-CSharp.csproj --no-restore` when the generated project is current.
  - `git diff --check`
  - `graphify update .` from monorepo root.
- Manual checks:
  - Spawn at least two units for every affected motion type.
  - Confirm sound aligns with contact/compression, respects game volume, and stops when objects are destroyed.
  - Confirm Wind Totem loops quietly without restarting at every stumble contact.
  - Trigger all six attack auras and confirm release sound starts immediately and its tail is not cut off.
  - Fill a board with units and confirm idle sounds remain sparse and do not clip.
- Expected results:
  - No compile errors or missing references.
  - Audible but subtle movement feedback with no sound storm.

## Risks & Rollback

- Risk: frequent looping sounds become fatiguing. Mitigate with low per-source volume, randomized phase, and global throttling.
- Risk: tween timing changes later and audio drifts. Keep timing declaration adjacent to the controller call and document coupling.
- Risk: WebGL voice count/performance. Use one AudioSource per moving object with `PlayOneShot` plus global throttling; no per-cycle allocations after startup.
- Rollback: remove motion-sound component/code and imported CC0 clips; original tween calls remain unchanged.

## Release Order

- Client-only release. No dependency ordering.

## Open Questions

- Confirm final clip-to-motion mapping and perceived volume in Unity play mode.
