# 2026-07-01 — Updated Object DTO effects list

- Date: 2026-07-01
- GitHub Issue: None
- Status: Draft

## Goal

Change the game object update contract so `UpdatedObjectDto` receives `effects` as a list of strings instead of a single `effect` string, then render multiple active effects together on the served object. Stacked effects should stay readable by applying partial transparency to each spawned effect instance.

## Non-goals

- Do not redesign the object update pipeline.
- Do not rename unrelated DTO fields or refactor frame handling.
- Do not edit existing effect prefab assets unless runtime alpha application proves insufficient.
- Do not change one-shot hit/heal/damage effects unless they share the persistent object-effect path.

## Context / Constraints

- Current delta flow: `DeltaFrameHandler` -> `ObjectUpdater.UpdateObject` -> `ServedObject.UpdateObject`.
- Current DTO field: `UpdatedObjectDto.effect` and `SnapshotObjectDto.effect`.
- Current render path: `ServedObject.SetEffect(string effect)` keeps one `_effectInstance`, destroys it when effect becomes `"None"` or empty, then instantiates `Resources/Prefabs/Effects/{effect}` under `GetActualTransform()`.
- Existing effect prefabs mix `SpriteRenderer` and UI `Graphic`/`CanvasRenderer` style assets. Runtime alpha handling should cover renderer types rather than relying on prefab edits.
- Unity `JsonUtility` maps public serializable fields by exact field name, so server payload must send `effects` for `UpdatedObjectDto.effects` to populate.
- Snapshot also sends `effects`, so both delta and snapshot DTOs should use the same `List<string> effects` contract.
- Object update DTOs are sent when any game object field changes. That does not imply `effects` changed, so runtime effect rendering should compare the normalized effects list before destroying and recreating instances.

## Approach (Checklist)

- [x] **Step 0: Recon** (Inspect existing code, locate files)
  - Confirmed DTOs: `Assets/Scripts/GameScene/Dto/UpdatedObjectDto.cs`, `Assets/Scripts/GameScene/Dto/SnapshotObjectDto.cs`.
  - Confirmed update/render path: `Assets/Scripts/GameScene/Handler/DeltaFrameHandler.cs`, `Assets/Scripts/GameScene/Object/ObjectUpdater.cs`, `Assets/Scripts/GameScene/ServedObjectComponent/ServedObject.cs`.
  - Confirmed effect prefab path: `Assets/Resources/Prefabs/Effects`.
- [x] **Step 1: DTO contract**
  - Replace `UpdatedObjectDto.effect` with `List<string> effects`.
  - Replace `SnapshotObjectDto.effect` with `List<string> effects`.
  - Copy `snapshotObjectDto.effects` into `UpdatedObjectDto.effects` in the snapshot conversion constructor.
- [x] **Step 2: Runtime render model**
  - Replace `_effectInstance` with a dedicated `ServedObjectEffectRenderer` that owns active effect instances.
  - Keep `ServedObject` responsible only for forwarding `updatedObjectDto.effects`.
  - Normalize null, empty, `"None"`, and blank strings to no active effects.
  - Keep a cached normalized effects list and rebuild instances only when the normalized list changes.
- [x] **Step 3: Transparency**
  - Apply alpha after instantiation to all child `SpriteRenderer` components.
  - Apply alpha to child UI `Graphic` components when present.
  - Use a fixed alpha constant, likely `0.65f`, so several effects overlap without fully hiding the object or each other.
  - Keep original RGB and only multiply or set alpha; avoid prefab asset mutations.
- [x] **Step 4: Sorting / transform**
  - Parent every effect under `GetActualTransform()`.
  - Keep local position/rotation reset and existing `ApplyEffectScale`.
  - Preserve prefab ordering by input list order; no sorting unless server contract needs deterministic priority.
- [x] **Step 5: Tests** (Unit tests, manual verification steps)
  - Add focused Edit Mode tests only if existing Unity test assembly is already available or easy to add without project churn.
  - Otherwise validate with compile plus manual/editor scenario using payloads containing zero, one, and multiple effects.
- [x] **Step 6: Rollout / Rollback**
  - Coordinate server/client deployment because `effect` -> `effects` is a JSON contract change.
  - Roll back by restoring single `effect` DTO field and single-instance render path.

## Validation

- **Commands to run:**
  - `dotnet build Assembly-CSharp.csproj --no-restore`
  - Optional Unity batchmode compile/build if no Editor lock exists: `Unity -batchmode -quit -projectPath . -executeMethod BuildScript.BuildDevWebGL -logFile -`
  - `git diff --check`
- **Expected output:**
  - C# compile succeeds. Current run hung after the MSBuild banner and was terminated after ~2 minutes.
  - No whitespace errors in touched files. Full `git diff --check` is currently blocked by pre-existing trailing whitespace in modified effect `.meta` files.
  - Manual scene check: object with `effects: ["Burn", "Wet", "Shock"]` shows all three effects overlapped with reduced alpha; `effects: []`, null, or `["None"]` clears active persistent effects.
  - Repeated update DTOs with the same `effects` list do not destroy/recreate active effect instances.

## Risks & Rollback

- **Risks:**
  - Mixed renderer types in prefabs may require alpha support beyond `SpriteRenderer` and `Graphic`.
  - Effects can become visually stale if the server changes effects but does not send an update DTO for that object. This must be guaranteed server-side or handled by always including effect changes as object changes.
- **Rollback steps:** `git revert` this change or restore `UpdatedObjectDto.effect`, `_effectInstance`, and `SetEffect(string effect)`.

## Open Questions

- None for current implementation plan.
