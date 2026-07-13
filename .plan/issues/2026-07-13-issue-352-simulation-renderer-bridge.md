# 2026-07-13 — Simulation snapshot Unity renderer bridge

- Date: 2026-07-13
- GitHub Issue: #352
- Status: Complete

## Goal

Render immutable simulation snapshots into Unity GameObjects and player UI through a one-way, visual-only interpolation boundary.

## Non-goals

- Feed Unity `Transform`, animation, or UI state back into simulation.
- Port magic/mob/PVE gameplay behavior.
- Remove the legacy authoritative handlers before #361.

## Context / Constraints

- `GameScene.Simulation` has `noEngineReferences`; snapshot creation stays pure C#.
- Unity rendering lives in a separate assembly that references simulation, never the reverse.
- Prefab IDs are explicit bindings and missing mappings fail fast.
- Protocol/simulation plane `(x, y)` maps to Unity ground `(x, z)`.

## Approach (Checklist)
- [x] **Step 0: Recon** Inspect simulation entity/resource state and legacy spawn/update/UI paths.
- [x] **Step 1: Implementation** Add immutable snapshots, prefab bindings, one-way GameObject lifecycle, visual interpolation, and player UI projection.
- [x] **Step 2: Tests** Add Edit Mode snapshot tests and Play Mode spawn/update/destroy/cleanup smoke tests.
- [x] **Step 3: Rollout / Rollback** Validate, document assembly direction, and merge only into `feature/lockstep`.

## Validation
- **Commands to run:** Unity Edit Mode and Play Mode tests; `git diff --check`; `graphify update .`.
- **Expected output:** snapshot/golden tests and renderer smoke tests pass; simulation assembly remains free of Unity references.

## Risks & Rollback
- **Risks:** scene unload can leave stale dictionaries; rendering an unknown prefab must abort visibly; interpolation must never alter snapshot data.
- **Rollback steps:** revert the #352 squash merge from `feature/lockstep`.

## Open Questions
- Legacy UI card prefabs still own presentation details; the adapter reconciles hand contents without making them simulation state.
