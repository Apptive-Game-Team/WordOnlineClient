# 2026-07-13 — Deterministic magic and effect components

- Date: 2026-07-13
- GitHub Issue: #355
- Status: Complete

## Goal

Represent the complete production magic catalog through deterministic behavior families and run spawn, projectile, area, damage, status, and timed lifecycle logic inside the client simulation.

## Non-goals

- Port mob decision/pathfinding behavior owned by #358.
- Render visual effects directly from simulation.
- Read live database or Unity prefab configuration during a match.

## Context / Constraints

- Production currently has 67 canonical magic IDs and server-side subclasses.
- Magic configuration must be versioned and complete before session start.
- Per-cast RNG derives from session seed plus stable cast identifiers, never wall clock or collection order.
- Entity and effect iteration uses stable IDs and ordinal string ordering.

## Approach (Checklist)
- [x] **Step 0: Recon** Inspect server catalog, magic families, component types, input range/bounds, and client simulation boundaries.
- [x] **Step 1: Implementation** Add complete catalog, stable RNG streams, deterministic cast commands, timed instances, damage/status state, and state hashing.
- [x] **Step 2: Tests** Add representative fixtures per family, catalog completeness, RNG isolation, unordered-input, timed lifecycle, and full replay tests.
- [x] **Step 3: Rollout / Rollback** Update support matrix, run Unity tests, and merge only into `feature/lockstep`.

## Validation
- **Commands to run:** Unity Edit Mode tests; `git diff --check`; `graphify update .`.
- **Expected output:** full suite passes, all canonical magic IDs are registered exactly once, replay hashes match.

## Risks & Rollback
- **Risks:** family defaults are protocol configuration and must change with `configVersion`; server class behavior not represented in the matrix must fail review rather than silently fall back.
- **Rollback steps:** revert the #355 squash merge from `feature/lockstep`.

## Open Questions
- Exact production tuning remains config-versioned; this issue establishes deterministic behavior and checked-in catalog coverage rather than copying mutable database values into code.
