# 2026-07-02 — Skill 범위 indicator 도형화

- Date: 2026-07-02
- GitHub Issue: #336
- Status: Implemented

## Goal

Replace the current image-heavy field selection indicators with simpler geometric visuals that fit the game better and stay accurate to runtime range/radius values.

Target visuals:
- caster range: filled translucent disk centered on the caster
- circle skill: filled translucent disk at the selected position
- line skill: filled translucent beam from caster toward the aim point
- aim icon: small filled translucent disk at the selected point

## Non-goals

- Do not change magic validation, server range/radius values, or input behavior.
- Do not redesign card UI, object team indicators, or debug gizmos unless needed for consistency.
- Do not hand-edit generated Unity metadata beyond intentional prefab/script changes.

## Context / Constraints

- `FieldSelector` owns field selection preview lifecycle in `Assets/Scripts/GameScene/FieldSelector.cs`.
- `rangeObject`, `lineSkillIndicator`, and `circleSkillIndicator` are prefab references in both `Assets/Scenes/GameScene.unity` and `Assets/Scenes/InteractiveTutorialScene.unity`.
- `aimObject` also points at the shared `AimIcon` prefab; runtime code disables its current sprite and draws the same filled procedural shape style.
- Current visual prefabs live under `Assets/Resources/Prefabs/*RangeIndicator.prefab`.
- Current sprites live under `Assets/Art/Images/Indicator/`:
  - `skillRangeIndicator.png`: 512x512 translucent textured image for caster range
  - `circleRangeIndicator.png`: 512x298 non-square image for circle AoE
  - `lineRangeIndicator.png`: 1500x300 gradient/white line image for shoot range
- Existing scaling logic depends on `SpriteRenderer.sprite.bounds`, so changing sprites alone can preserve behavior but keeps visual quality tied to raster assets.
- Project design system favors warm game UI with turquoise as primary interaction color; world-space indicators should remain subtle and not obscure objects.

## Approach (Checklist)

- [x] **Step 0: Recon** (Inspect existing code, locate files)
  - Confirmed `FieldSelector` instantiates and toggles indicator prefabs.
  - Confirmed `CircleSkillIndicator` and `LineSkillIndicator` only scale/position SpriteRenderer-based prefabs.
  - Confirmed scene references point to shared prefabs, so prefab-level changes affect game and tutorial scenes.
- [x] **Step 1: Implementation** (Code changes, file paths)
  - Add a small procedural world-space indicator component, likely `Assets/Scripts/GameScene/SkillIndicatorShapeRenderer.cs`.
  - Use simple generated meshes for filled translucent circle and beam shapes.
  - Update `CircleSkillIndicator` to drive a geometric circle/disk by radius instead of sprite bounds.
  - Update `LineSkillIndicator` to drive a geometric beam by start/target/range while preserving current rotation behavior.
  - Update field selectors to configure aim icons through the same procedural shape renderer.
  - Route the existing indicator prefabs through procedural components at runtime instead of editing scene references.
  - Keep prefab names and `FieldSelector` references stable to avoid scene reference churn.
- [x] **Step 2: Tests** (Unit tests, manual verification steps)
  - Check Unity console for script compile errors/warnings. (Done)
  - Runtime NRE found through console and fixed.
  - Visual scene verification remains a follow-up manual check because field select mode depends on live game flow.
- [x] **Step 3: Rollout / Rollback** (Feature flags, migration steps)
  - Rollback by reverting the new component and prefab changes; no data/server migration needed.

## Validation

- **Commands to run:**
  - Unity compile through Editor after script changes. (Done)
  - `read_console` through Unity MCP for errors after compilation. (Done: 0 errors)
  - Optional: scene screenshots through Unity MCP from `GameScene` and `InteractiveTutorialScene`.
- **Expected output:**
  - No compile errors.
  - Indicators appear as crisp simple geometry at all runtime scales.
  - Range clamp and click input behavior remain unchanged.

## Risks & Rollback

- **Risks:**
  - Procedural mesh/LineRenderer sorting may differ from SpriteRenderer sorting and hide behind units/effects.
  - World-space thickness may look too heavy or too thin at different camera zooms.
  - LineRenderer material/shader choice must work in WebGL.
  - Prefab edits may touch Unity YAML broadly if done carelessly.
- **Rollback steps:**
  - Revert `SkillIndicatorShapeRenderer.cs`, related script changes, and the three indicator prefabs.
  - Keep original PNG assets unless explicitly removed later.

## Open Questions

- Should shoot-line preview length show full magic range, or only caster-to-current-aim distance?
- Should indicators use one neutral turquoise style for all magics, or vary by element/type later?
- Should the aim icon also be simplified in the same pass, or left for a separate visual cleanup?
