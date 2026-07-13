# 2026-07-13 — BEPU gameplay physics integration

- Date: 2026-07-13
- GitHub Issue: #356
- Status: Complete

## Goal

Make vendored integer BEPU the sole gameplay movement/collision source, with deterministic bounds, collision/trigger events, and complete checkpoint state.

## Non-goals

- Render or write Unity `Transform` values; #352 owns the one-way renderer bridge.
- Implement magic-specific hit behavior or mob navigation.
- Add nondeterministic Unity Physics callbacks to simulation.

## Acceptance Criteria

- Physics bodies enter BEPU in object-ID order and advance at one fixed timestep.
- Position, orientation, linear velocity, and angular velocity are exposed and hashed.
- Collision/trigger events and world-bound outcomes are canonically ordered.
- Golden collision and replay fixtures produce identical hashes across worlds/runs.
- `GameScene.Simulation` remains free of `UnityEngine` references.

## Context / Constraints

- `bepuphysics1int` source commit `9237daa` is vendored as precompiled DLLs.
- Simulation is single-threaded (`Space(null)`) and uses `Fix64` only.
- Protocol positions are Unity ground coordinates `(x, z)`; BEPU simulation plane is `(x, y)`.
- Collider dimensions and trigger behavior are versioned prefab configuration.

## Approach (Checklist)
- [x] **Step 0: Recon** Inspect BEPU body/event APIs, current hash fields, prefab registry, and gameplay coordinate contract.
- [x] **Step 1: Implementation** Add immutable physics definitions, deterministic bounds, full body snapshots, and canonical collision/trigger event collection.
- [x] **Step 2: Tests** Add insertion-order, bounds, trigger, collision fixture, full checkpoint, and cross-run replay tests.
- [x] **Step 3: Rollout / Rollback** Update support docs, run Unity Edit Mode tests, and merge only into `feature/lockstep`.

## Affected Repositories / Contracts

- `WordOnlineClient` only.
- Server wire positions remain unchanged; Client explicitly maps `(x, z)` into its 2D BEPU plane.
- Canonical hashes change when physics config/event/checkpoint fields are added.

## Validation

- **Commands to run:** Unity 2022.3.34f1 Edit Mode tests; `dotnet msbuild GameScene.Simulation.Tests.csproj -t:Build`; `git diff --check`; `graphify update .`.
- **Expected output:** all Edit Mode tests pass, golden hashes match, no `UnityEngine` reference enters the simulation assembly.

## Risks & Rollback

- **Risks:** BEPU deferred event order is internal, so events must be normalized by object IDs; bounds correction after solve can change collision fixtures; old hashes intentionally become invalid.
- **Rollback steps:** revert the #356 squash merge from `feature/lockstep`; no server or persisted-data migration is involved.

## Open Questions

- Production field bounds are not yet present in the lockstep wire contract; use explicit deterministic config rather than reading a scene collider.

## Release Order

1. Merge #356 into Client `feature/lockstep`.
2. Build the renderer bridge (#352) as a one-way consumer of physics snapshots/events.
3. Merge `feature/lockstep` to `main` only after all lockstep issues finish.
