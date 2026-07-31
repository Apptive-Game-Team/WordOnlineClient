# 2026-07-17 — 2.5D Paper Unit and Building Spawn Animation

- Date: 2026-07-17
- GitHub Issue: https://github.com/Apptive-Game-Team/WordOnlineClient/issues/374
- Status: Draft

## Goal

- Make ground units appear to rise from a lying paper-doll pose when spawned.
- Give buildings a distinct, heavier entrance: ground puffs first, then an upright reveal.
- Preserve server-authoritative position, collision, selection, shadow, and UI behavior throughout the animation.
- Reuse abstract prefab inheritance so shared behavior is configured once per object family.

## Non-goals

- No game-server, DTO, protocol, or database changes.
- No per-unit bespoke spawn animations.
- No new character or building artwork.
- No changes to projectile, rune, field, drop, or explosion spawn behavior.

## Context / Constraints

- Owning repository: `Apptive-Game-Team/WordOnlineClient` only.
- `ObjectSpawner` instantiates every server-managed object, while visual and movement behavior lives on `ServedObject` prefab hierarchies.
- Existing `SpawnPopInEffect` and `PopInMotionController` use DOTween; the new presentation should reuse that dependency and avoid parallel animation systems.
- Animate only a dedicated visual transform returned or owned by the prefab. Do not rotate the root transform because position sync, colliders, raycast selection, HP UI, shadows, and team indicators depend on stable world structure.
- Building classification should come from `AbstractBuild` prefab inheritance or an explicit presentation component, not fragile prefab-name matching.
- Ground units currently span `AbstractMeleeMob`, `AbstractRangeMob`, and `AbstractSlime`. `AbstractAerialMob` needs separate review because a lying-on-ground entrance may conflict with airborne placement.
- Current checkout contains unrelated unstaged user changes. Implementation and staging must exclude them.

## Approach (Checklist)

- [ ] **Step 0: Recon** — Inspect `ObjectSpawner`, `ServedObject.GetActualTransform()`, existing DOTween spawn/motion components, and all Abstract prefab inheritance. Confirm which visual child has a ground-aligned pivot and list affected prefab families.
- [ ] **Step 1: Shared presentation component** — Replace or extend the scale-only spawn effect with a reusable DOTween sequence that snapshots/restores local rotation, scale, and position; kills linked tweens on disable/destroy; and supports a bottom-pivot paper stand-up profile.
- [ ] **Step 2: Ground-unit entrance** — Configure applicable ground-unit Abstract prefabs to start nearly flat around the local X axis, rise from their base, overshoot slightly, and settle without moving the root/collider.
- [ ] **Step 3: Building entrance** — Configure `AbstractBuild` with a short pre-roll, procedural dust/puff presentation near the footprint, compressed/low visual pose, and slower weighted stand-up/settle timing. Keep generated particles visual-only and self-cleaning.
- [ ] **Step 4: Prefab coverage** — Verify representative variants inherit the correct profile. Exclude aerial units and non-ServedObject effect prefabs unless inspection proves they need the same behavior.
- [ ] **Step 5: Tests** — Add Edit Mode coverage for profile/sequence state restoration where feasible. In Play Mode, spawn at least one melee/range/slime unit and one building for each side, then verify selection, server position updates, shadow, HP bar, and destruction during animation.
- [ ] **Step 6: Rollout / Rollback** — Ship as a client-only visual change. Roll back by reverting the new component/configuration commit; no data or protocol rollback needed.

## Validation

- **Commands to run:**
  - Open with Unity `2022.3.34f1` as specified by `client/AGENTS.md` and wait for compilation.
  - Run relevant Edit Mode tests through Unity Test Framework if added.
  - Run GameScene in Play Mode and use debug prefab spawning for representative unit/building variants.
  - Check Unity Console for errors and DOTween warnings after spawn, early destruction, scene unload, and repeated spawning.
  - `git diff --check`
  - `git status --short` and confirm unrelated pre-existing files remain outside issue staging.
- **Expected output:**
  - No C# compile errors, test failures, missing references, or orphaned tween warnings.
  - Units visibly rise from a lying paper pose; buildings puff then rise with distinct timing.
  - Root world position and selection collider remain stable during the full sequence.
  - Existing attack/movement animation begins from the restored authored transform state.

## Risks & Rollback

- **Risks:** Existing motion controllers may start in `Awake` and compete for the same visual transform; prefab pivots may differ; right-side flipping/rotation may change the perceived fold direction; initial sync updates may arrive during the entrance; procedural puffs may sort behind or in front of the wrong sprite; snapshot-created objects may replay entrance effects after reconnect.
- **Mitigations:** Give spawn presentation explicit transform ownership, sequence motion after state capture, preserve authored local values, configure sorting relative to the main SpriteRenderer, test both masters, and confirm whether snapshot reconciliation should animate or appear immediately.
- **Rollback steps:** Revert the issue commit or remove the presentation components from Abstract prefabs. No server rollback or asset migration required.

## Open Questions

- Should full snapshot/reconnect-created objects play the spawn animation, or only delta-frame creations?
- Should aerial mobs use a separate drop/float entrance or keep their current spawn behavior?
- Should buildings remain selectable during the dust pre-roll, or become visually and interactively available only when the rise begins?
