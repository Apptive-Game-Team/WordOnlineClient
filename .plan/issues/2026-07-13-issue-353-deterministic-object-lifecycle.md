# 2026-07-13 — Deterministic object and prefab lifecycle

- Date: 2026-07-13
- GitHub Issue: #353
- Status: Complete

## Goal

Give every simulated object a deterministic identity, prefab definition, and ordered component lifecycle so all clients produce the same initial world and state hash.

## Non-goals

- Port prefab-specific gameplay, physics shapes, magic, mob AI, or rendering behavior.
- Load Unity `GameObject` or `Resources` from the deterministic assembly.
- Keep an authoritative-client fallback path.

## Acceptance Criteria

- Bootstrap sequence fixes initial object IDs regardless of arrival order.
- Identical spawn/destroy replays do not mutate canonical state.
- Production prefab/component support is explicit and unknown config fails fast.
- Lifecycle state participates in a stable golden hash.

## Context / Constraints

- This is pure C# under `GameScene.Simulation`; it cannot reference `UnityEngine`.
- Bootstrap events may arrive unordered, but their server-issued sequence defines object IDs.
- Prefab and component names are protocol/config identifiers and use ordinal comparison.
- Unknown or inconsistent configuration must fail before advancing simulation state.
- The PR targets the long-lived `feature/lockstep` integration branch.

## Approach (Checklist)
- [x] **Step 0: Recon** Inspect `SimulationWorld`, canonical hashing, bootstrap protocol, tests, and production prefab inventory.
- [x] **Step 1: Implementation** Add immutable prefab definitions/registry, ordered component instances, explicit spawn/destroy lifecycle, and ordered bootstrap application.
- [x] **Step 2: Tests** Add lifecycle golden tests for ordering, idempotency, fail-fast behavior, component order, and canonical hashes; run Unity Edit Mode tests.
- [x] **Step 3: Rollout / Rollback** Check in the support matrix, publish to `feature/353`, and merge only into `feature/lockstep`; rollback with a squash-merge revert.

## Affected Repositories / Contracts

- `WordOnlineClient`: deterministic object lifecycle and tests.
- `WordOnlineServer`: no change; consumes the merged #292 bootstrap contract (`LeftPlayer`/`RightPlayer`).
- Canonical state hash layout changes because prefab/component lifecycle fields are now included.

## Validation
- **Commands to run:** Unity 2022.3.34f1 Edit Mode tests; `git diff --check`; `graphify update .` from the monorepo root.
- **Expected output:** all Edit Mode tests pass; lifecycle golden hashes are stable; no whitespace errors.

## Risks & Rollback
- **Risks:** changing canonical state layout intentionally invalidates prior hashes; mismatched prefab catalogs reject session startup; PVE bootstrap remains unavailable until #354.
- **Rollback steps:** revert the #353 squash merge from `feature/lockstep`; no persisted data or server rollout is changed by this issue.

## Open Questions
- PVE `START_PVE_SCENARIO` config loading remains assigned to #354 and is intentionally rejected until that deterministic registry exists.

## Release Order

1. Merge this PR into Client `feature/lockstep` only.
2. Complete dependent prefab behavior issues on the same integration branch.
3. Merge the completed lockstep branch to `main` as one release.
