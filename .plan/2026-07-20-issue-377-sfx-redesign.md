# Issue #377 SFX Redesign Plan

## Status

- Plan review: PASS
- Fast review: NONPASS, findings incorporated
- Medium review: NONPASS, findings incorporated
- Heavy review: PASS
- Pull request #378 remains Draft and marked for rework.
- No implementation expansion or Ready transition before auditory approval and Unity validation.

## Goal

Replace the broad, mechanically assigned sounds in PR #378 with a curated,
auditable SFX system for UI, cards, field targeting, object lifecycle events,
and projectiles.

Unique audio per prefab is not required. Prefabs share a small number of
approved sound-family profiles. Every runtime type must still map explicitly to
a shared profile or to intentional silence.

The audible identity, elemental palette, candidate prompt rules, and approval
rubric are defined in
`.plan/issues/2026-07-21-issue-377-sfx-style-guide.md`. A technically valid
profile cannot be activated unless its clips also pass that style guide.

## Problems in the Current Draft

- Every spawn uses the same drop sound.
- Every movement uses the same wind sound.
- Existing prefab attack sounds are disabled and replaced by generic attack sounds.
- Name-keyword matching can assign the wrong element.
- Generated button and field MP3 files were committed without auditory approval.
- Static buildings and transient spell objects can receive inappropriate lifecycle sounds.
- Static validation passed, but Unity compilation and listening tests were not performed.

## Authoritative Runtime Type Contract

`CreatedObjectDto.type` is the runtime lookup key. `ObjectSpawner` resolves it
with `Resources.Load<GameObject>($"Prefabs/{type}")`. Therefore, top-level
prefab names under `Assets/Resources/Prefabs` form the client contract snapshot.

An Editor validator must compare the prefab snapshot and catalog in both
directions:

- prefab without catalog row: fail
- catalog row without prefab or documented server alias: fail
- duplicate runtime type: fail
- enabled event without a valid clip: fail

Server-only aliases discovered in representative match DTO logs must have
explicit catalog rows pointing to an existing shared profile. Unknown runtime
types stay silent and log once per distinct type.

## Catalog Design

Use a flat ScriptableObject design. Do not build an inheritance or runtime rule
engine.

### `ObjectSfxProfile`

One asset represents a proven shared sound family, such as `FireCreature`,
`RockBuilding`, or `TransientSpell`.

Fields:

- stable profile ID
- element and archetype metadata for planning/editor filtering only
- enabled event slots
- direct `AudioClip` references
- per-event volume
- fixed pitch
- movement cooldown

All clip, volume, pitch, and cooldown values belong to the profile. No
per-prefab overrides in v1. A prefab needing a real audible exception receives
a new shared profile with documented justification.

### `ObjectSfxCatalog`

Contains mapping rows:

- runtime type
- shared profile reference, or
- `intentionalSilent`

Approximately 93 rows are expected, but only a small set of shared profiles.
Load one catalog from `Resources/Sound/Config`.

Missing catalog, profile, or required clip means silence at runtime. Never play
a Generic fallback sound for an unknown type.

## Phase 0: Restore a Safe Audible Baseline

Retain from PR #378:

- `GlobalButtonSoundPlayer` coverage
- exclusion preventing duplicate sound on `ButtonBase`
- `CardUI`, `CardUIZoom`, and `TutorialCardUI` touch wiring
- `FieldSelector` valid-target hook structure
- `SoundAssets` caching cleanup
- initial HP snapshot guard

Disable or revert before new listening approval:

- automatic `ServedObjectSfxController` attachment in `ObjectSpawner`
- `PositionUpdater.OnMoved` callback
- `ServedObject.OnMoved`
- name-based `SoundConceptResolver`
- generic drop sound on every spawn
- generic wind sound on every movement
- forced replacement of prefab attack sounds
- broad automatic death mapping

Restore existing `OnAttackSoundPlayer` components as event owners.

The generated `field_click.mp3` and `wood_button_click_v2.mp3` are rejected
candidates. Remove them from runtime Resources or move them to a local,
untracked audition folder. Restore the known prior button clip temporarily.
Field targeting remains silent until a candidate is approved. Do not delete
previously approved legacy assets.

Phase 0 ends with no orphaned runtime audio and no new broad lifecycle sounds.

## Event Ownership

### UI buttons

- A `ButtonBase` instance owns its click sound.
- `GlobalButtonSoundPlayer` handles an interactable Unity `Button` only when no
  parent `ButtonBase` exists.
- Add `DisableGlobalButtonSfx` for explicit opt-out.
- Disabled buttons and hover do not play click sounds.

### Cards

- `CardUI` owns select and deselect.
- `CardUIZoom` owns hover and applies a hover cooldown.
- Tutorial card behavior follows the same approved touch profile.
- Draw sound belongs to the hand reconciliation/creation path, not card click.
- Initial hand snapshot is silent.
- Each newly instantiated card queues one draw sound with at least 80 ms spacing.
- If stable card identity cannot be obtained from the current reconciliation
  data, create a linked follow-up and explicitly remove draw from #377 acceptance
  before implementation. Never fake draw detection in `CardUI.Awake`.

### Field selection

Add `CardInputSender.TrySendInput(Vector3)` returning `true` only when:

- field-select mode is active
- client is not waiting for a response
- ground raycast succeeded
- pointer is not over UI or a selectable object
- sending was initiated

`FieldSelector` plays the field sound only when this method returns `true`.
Out-of-range input uses the clamped preview position. Blocked, canceled,
UI-overlap, invalid-raycast, and failed-send actions remain silent. A later
server rejection does not retract immediate local feedback.

### Prefab and projectile events

- Existing prefab `OnAttackSoundPlayer` owns attack while present.
- Existing projectile launch, flight, and impact components keep ownership.
- A profile attack slot is used only when no legacy owner exists.
- The lifecycle controller plays only profile-enabled, non-projectile events.
- Coverage validation fails when two components own the same event.

## Lifecycle Semantics

- Spawn plays once after successful object registration and only when
  `playSpawnPresentation` is true.
- Reconnect/snapshot creation with `playSpawnPresentation=false` is silent.
- First HP snapshot establishes baseline and emits no hit or heal sound.
- Later HP decrease emits hit when enabled.
- Later HP increase emits heal when enabled.
- Hit followed by destruction may emit hit and death.
- Death is one-shot.
- `Initialize` resets one-shot and cooldown state for future pooling.
- `OnDisable` and `OnDestroy` unsubscribe safely.
- Static archetypes explicitly disable movement.
- Shot, Drop, Explode, and Field profiles explicitly define applicable events.

## Voice and Overlap Policy

UI sounds bypass game-object voice limits.

`GameSfxPlayer` owns short game one-shots with these initial caps:

- movement: 2
- spawn/death: 3
- attack: 4
- hit/heal: 4
- overall game voices: 10

Priority order:

1. death
2. hit/heal
3. attack
4. spawn
5. movement

Admission and eviction are deterministic:

1. Remove completed voices from accounting.
2. If the request's category cap is full, select the oldest active voice in
   that category with strictly lower priority. If none exists, drop the new
   request.
3. Evaluate the overall cap after hypothetically removing the category victim.
   If still full, select the oldest remaining active game voice with strictly
   lower priority. If none exists, drop the new request and keep all existing
   voices.
4. Stop selected victims only after both checks succeed, then play the request.
5. Equal-priority voices are never evicted; ties drop the new request.

Tests cover category-only pressure, overall-only pressure, simultaneous caps,
equal-priority ties, oldest-voice selection, and all-or-nothing behavior when
the second admission check fails.

Movement also has a 450 ms per-object cooldown. Random pitch is disabled in v1
so tests and auditory comparisons remain deterministic.

## Audio Technical Standard

Final runtime assets:

- WAV PCM, 16-bit
- 44.1 kHz
- mono
- no loop unless profile explicitly documents ambience
- leading/trailing silence at most 10 ms
- true peak at most -1 dBFS
- no clipping
- loudness and frequency targets are owned exclusively by
  `.plan/issues/2026-07-21-issue-377-sfx-style-guide.md`; this implementation
  plan does not define competing mix targets

Unity import settings for short clips:

- Force To Mono
- 2D playback
- Decompress On Load for clips up to 1.2 seconds
- preload enabled
- normalization disabled

Candidate MP3 files must not become final runtime assets. WebGL smoke testing
must verify import and playback.

## Asset Inventory and Generation

1. Listen only to existing SFX referenced by current owners or eligible for
   activation under issue #377, then record usable event/family assignments.
2. Approve the family matrix before generating assets.
3. For each missing family event, generate only A/B/C candidates for one event
   at a time and resolve that event before generating the next set.
4. Do not generate candidates per prefab.
5. Do not generate 30+ sounds before the vertical slice passes.

For every approved asset, record:

- filename and SHA-256
- source/provider
- prompt, model, and generation date when generated
- license/provenance
- rejected candidate names
- approver and approval date

Changing an approved file hash requires renewed approval.

## Auditory Approval Gate

Approver: user/dev-yunseong.

Playback comparison:

- A/B/C candidates presented through a Unity audition scene
- UI and game volume both set to 50
- headphones and speakers checked
- each event compared in isolation and under expected overlap
- external candidates remain in gitignored `.sfx-work/issue-377`; an Editor-only
  audition tool reads them without importing them into runtime Assets

Approval/rejection is recorded in
`.plan/issues/2026-07-21-issue-377-sfx-asset-manifest.md`.

No candidate enters runtime mapping before approval.

## Vertical Slice

Do not expand the catalog or generate a large asset set until this slice passes.

### Button

- click: required
- hover: silent
- disabled click: silent
- approved replacement must sound materially different from rejected v2

### `FireSlime` using `FireCreature`

- spawn: required
- movement: required
- attack: existing legacy owner; profile attack silent
- hit: required
- heal: only after a real post-baseline HP increase
- death: required, one-shot

### `GroundTower` using `RockBuilding`

- spawn: required
- movement: silent
- attack: existing legacy/projectile owner when present; otherwise explicit
  approved profile decision
- hit: required
- heal: silent by default
- death: required, one-shot

Test rapid spawning, overlapping movement, hit plus death, and volume values
0/50/100. User approves both required sounds and intentional silence decisions.

## Delivery Phases

1. Inventory existing sounds and current owners.
2. Produce runtime-type snapshot, ownership table, shared-profile matrix, and
   asset manifest.
3. Apply Phase 0 cleanup in a dedicated commit.
4. Build and approve the vertical slice.
5. Finalize UI, card, and field interactions.
6. Migrate creatures by approved shared profile.
7. Migrate buildings, totems, runes, and nests.
8. Migrate transient spells and projectiles without duplicating projectile audio.
9. Run final Unity and WebGL validation.
10. Update Draft PR #378 with exact coverage and approval records.

Each migration commit includes its profile-matrix delta, listening result, and
Play Mode checklist. Group commits remain independently revertible.

## Automated Validation

- prefab snapshot and catalog match in both directions
- observed DTO aliases are mapped
- duplicate runtime type rejected
- enabled slot requires a clip
- intentional silence accepted
- missing/unknown type remains silent and logs once per type
- no duplicate event owner
- voice cap and priority behavior deterministic
- initial HP snapshot silent
- death one-shot
- teardown unsubscribes listeners
- initialization resets lifecycle state

Do not create 93 separate audio playback tests. Test catalog coverage and shared
profile behavior instead.

## Manual Validation

- Unity domain reload and compile
- zero new Console errors
- EditMode validation suite
- GameScene vertical-slice Play Mode test
- TutorialScene card interaction test
- headphone and speaker auditory approval
- volume and mute at 0/50/100
- rapid spawn/movement and overlapping hit/death
- scene reload and teardown
- final WebGL smoke build after full mapping

## Rollback

- Disable individual profile event slots.
- Map a runtime type to intentional silence.
- Keep legacy components and approved assets until each migration group passes.
- Revert each migration group commit independently.
- Never delete the last known-good approved asset during migration.

## Definition of Done

- Every runtime prefab key and observed DTO alias maps to a shared profile or
  intentional silence.
- No duplicate/stale catalog rows.
- No event has duplicate ownership.
- Unknown types fail safely with silence.
- Static objects do not emit movement sounds.
- Initial HP snapshot emits no hit/heal sound.
- Volume, mute, cooldown, and voice caps work.
- Only approved WAV hashes are active runtime assets.
- User auditory approval is recorded.
- Unity compilation, Console, EditMode, GameScene, TutorialScene, and WebGL
  validation all pass.
- PR #378 remains Draft until every required gate above passes.

## Review Record

### Accepted fast-review findings

- Defined authoritative runtime key and bidirectional snapshot validation.
- Chose concrete ScriptableObject catalog behavior and silent fallback.
- Defined exact Phase 0 baseline.
- Defined field submission, draw, lifecycle, voice, technical, and approval semantics.
- Specified exact vertical-slice objects and event expectations.

### Accepted medium-review findings

- Reduced 93 individual profiles to 93 mappings over a small shared-profile set.
- Removed runtime element/archetype composition and override hierarchy.
- Replaced Generic fallback with safe silence.
- Preserved legacy event owners during incremental migration.
- Limited automated testing to catalog/profile behavior.

### Rejected feedback

- None.

### Heavy review

- PASS
