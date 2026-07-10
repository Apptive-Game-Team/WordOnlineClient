# 2026-07-10 오브젝트 HP bar 정상화

- Date: 2026-07-10
- GitHub Issue: None
- Status: Complete

## Goal

Normalize served object HP bars so they reliably appear above objects at a readable size.

## Non-goals

Do not redesign the battle UI or change unrelated object rendering behavior.

## Context / Constraints

HP bars are implemented by `ServedObjectHpBar` with a world-space Canvas prefab under served object prefabs. Some prefab data uses a large local z offset and fixed tiny scale, which can make bars look missing, tiny, or detached depending on object transforms.

## Approach (Checklist)
- [x] **Step 0: Recon** (Inspect existing code, locate files)
- [x] **Step 1: Implementation** (Code changes, file paths)
- [x] **Step 2: Tests** (Unit tests, manual verification steps)
- [x] **Step 3: Rollout / Rollback** (Feature flags, migration steps)

## Validation
- **Commands to run:** `dotnet build Assembly-CSharp.csproj -v minimal`
- **Expected output:** C# project compiles without errors.

## Risks & Rollback
- **Risks:** Runtime anchoring may need small visual tuning if specific oversized or tiny sprites expect custom offsets.
- **Rollback steps:** Revert the `ServedObjectHpBar` script change and prefab normalization if needed.

## Open Questions
- None.
