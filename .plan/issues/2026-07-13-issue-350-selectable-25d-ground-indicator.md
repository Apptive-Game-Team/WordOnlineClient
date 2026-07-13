# 2026-07-13 — Fix 2.5D Selectable Hit Testing and Ground Projection Indicator

- Date: 2026-07-13
- GitHub Issue: https://github.com/Apptive-Game-Team/WordOnlineClient/issues/350
- Status: Implemented; manual Play Mode verification pending

## Goal

- Make pointer hover/click follow the visible `SpriteRenderer` reliably under the 2.5D camera, including rotated, scaled, animated, and y != 0 objects.
- Make selection collider ownership deterministic instead of accidentally reusing an unrelated child collider.
- Visualize the selected world position and its ground projection `(x, 0, z)` with a vertical line so the selected x/z coordinate is unambiguous.
- Preserve the existing `CardInputSender` protocol while sending an object's actual world position and using `(x, 0, z)` only for ground visualization.

## Non-goals

- Do not change server DTOs, STOMP destinations, range calculations, or magic validation behavior.
- Do not redesign the 2.5D camera or mass-rewrite prefab hierarchies.
- Do not replace every gameplay collider with a new physics abstraction.
- Do not change unrelated `feature/345` audio work or carry its dirty workspace changes into this branch.

## Context / Constraints

- Owning repository: `WordOnlineClient` (`client/`); no cross-repository contract change is expected.
- Work branch: `fix/350`, based on local `main` at `70ce401`, in a dedicated Git worktree.
- Current `Selectable.Awake()` finds a `SpriteRenderer`, creates or reuses a `BoxCollider` on the renderer GameObject, sizes it from `sprite.bounds`, and adds a `PhysicsRaycaster` to the camera.
- Attaching the collider to the renderer GameObject should share rotation, so different tilt is a hypothesis, not a confirmed root cause. Actual ray hits, hierarchy, layer masks, animation, scale, and existing collider ownership must be observed before choosing a fix.
- Existing dirty changes in the primary `feature/345` worktree overlap `Selectable.cs` and `FieldSelector.cs`; they are user-owned and must remain untouched.
- Procedural indicators are rendered through `SkillIndicatorShapeRenderer`; reuse its material/sorting behavior where practical rather than restoring deleted indicator prefabs.
- Main, tutorial, and debug/secondary selection flows must be checked. Only change secondary flows that reproduce or consume the same contract.
- Unity versions documented in repo files disagree (`2022.3.34f1` in `AGENTS.md`, `2022.3.22f1` in `CLAUDE.md`); validate against `ProjectSettings/ProjectVersion.txt` before Editor testing.

## Approach (Checklist)

- [x] **Step 0: Recon** (Inspect existing code, locate files)
  - [ ] Reproduce in `GameScene` with at least one ground object and one y != 0 object; record which sprite regions miss hover/click. (Play Mode pending)
  - [x] Inspect `PhysicsRaycaster` setup, prefab object layers, renderer hierarchy, and current input boundary.
  - [x] Compare `Selectable`, `SpriteRenderer`, existing `BoxCollider2D`, and motion transforms in representative prefab YAML.
  - [x] Identify the unstable contract: a separate runtime `BoxCollider` used `sprite.bounds` once and did not track renderer-local bounds changes.
  - [x] Inspect `FieldSelector`, `TutorialFieldSelector`, `SkillIndicatorShapeRenderer`, and `LineSkillIndicator` to preserve existing procedural visual conventions.
  - [x] Confirm field clicks already produce ground coordinates while object clicks must preserve the selected object's world y.
- [x] **Step 1: Implementation** (Code changes, file paths)
  - [x] Refactor `Assets/Scripts/GameScene/ServedObjectComponent/Selectable.cs` to own a dedicated `SelectableHitbox` child under the selected renderer.
  - [x] Synchronize collider center/size from `SpriteRenderer.localBounds` only when bounds change.
  - [x] Keep `PhysicsRaycaster` setup deterministic and warn when no renderer exists.
  - [x] Add `Assets/Scripts/GameScene/SelectionGroundIndicator.cs` to draw the selected position to `(x, 0, z)`.
  - [x] Connect hover lifecycle and hide the line on exit, click, disable, destroy, or owner replacement.
  - [ ] If the field aim cursor can represent y != 0, update `Assets/Scripts/GameScene/FieldSelector.cs` to keep world cursor and ground projection as separate values. Mirror only contract-equivalent behavior in `Assets/Scripts/TutorialScene/Battle/TutorialFieldSelector.cs`.
  - [x] Preserve actual object `(x, y, z)` input and keep ground projection local to the indicator.
  - [x] Restrict UI blocking to `GraphicRaycaster` hits so the `PhysicsRaycaster` ground hit does not disable field selection.
- [ ] **Step 2: Tests** (Unit tests, manual verification steps)
  - [ ] Add Edit Mode tests under `Assets/Tests/` for collider target resolution and renderer-local bounds mapping if those rules can be isolated without scene dependencies.
  - [ ] Add tests for projection endpoints: `(x, y, z)` maps to `(x, 0, z)`, y == 0 hides or collapses the vertical line, and cleanup is idempotent.
  - [ ] In Unity, verify top/center/bottom clicks on rotated ground and airborne sprites, including during idle animation and after sprite replacement.
  - [ ] Verify overlapping sprite/collider cases select the expected object and UI pointer blocking remains unchanged.
  - [ ] Verify indicator endpoints retain identical x/z, correct world-space y, visible sorting, and cleanup across hover/click/cancel/disable/destroy.
  - [ ] Smoke-test tutorial/debug selection paths if recon finds shared behavior.
- [ ] **Step 3: Rollout / Rollback** (Feature flags, migration steps)
  - [ ] Ship as client-only behavior with no migration or server release ordering.
  - [ ] Capture before/after Scene/Game view evidence for the PR and document tested prefab types.
  - [ ] Keep collider fix and indicator addition as separable commits when implementation boundaries remain clean.
  - [ ] Roll back through commit revert; no persistent data or protocol rollback required.

## Validation

- **Commands to run:**
  - `cat ProjectSettings/ProjectVersion.txt`
  - Unity Editor compile using the version declared by `ProjectSettings/ProjectVersion.txt`
  - Unity Test Runner Edit Mode/Play Mode tests added for issue #350
  - `git diff --check`
  - `git status --short`
- **Expected output:**
  - Unity scripts compile without errors.
  - Added tests pass; no existing test regression.
  - Visible sprite regions reliably produce hover/click events in all listed manual cases.
  - y != 0 selection shows one line with endpoints `(x, y, z)` and `(x, 0, z)`; line disappears on every exit path.
  - Input payload remains compatible with existing `CardInputSender`/server expectations.
  - `git diff --check` prints no errors and diff contains only issue #350 files.

## Risks & Rollback

- **Risks:**
  - Enlarging or thickening colliders can make transparent sprite regions or overlapping objects capture unintended clicks.
  - Reusing gameplay colliders may change physics behavior; selection collider must remain trigger-only and ownership-specific.
  - Rebuilding bounds too often can add avoidable per-frame allocations/CPU cost across many objects.
  - LineRenderer/procedural indicator sorting may place the vertical line behind sprites or range meshes in the 2.5D camera.
  - Flattening `y` at the wrong boundary can change server targeting semantics; recon must confirm current contract.
  - Tutorial/debug flows may have different cursor lifecycles and should not be mechanically coupled without evidence.
- **Rollback steps:**
  - Revert indicator commit independently if visual lifecycle/sorting regresses while keeping validated hit-testing fix.
  - Revert collider commit if click ordering or prefab physics regress.
  - No database, server, asset migration, or feature-flag cleanup is required.

## Open Questions

- Resolved: show the vertical line while hovering the active target, before input is committed.
- Resolved: project to world `y = 0`, matching the requested acceptance baseline.
- Resolved: object selection sends actual world y; only field selection and the visual ground endpoint use ground coordinates.
- Which prefabs reproduce the miss most consistently? Record them during Editor recon and use them as the PR manual test matrix.
