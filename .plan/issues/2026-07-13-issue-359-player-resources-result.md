# 2026-07-13 — Deterministic player resources and result state

- Date: 2026-07-13
- GitHub Issue: https://github.com/Apptive-Game-Team/WordOnlineClient/issues/359
- Owning repository: WordOnlineClient
- Status: Complete

## Goal

Add deterministic player, card deck/hand/selection, mana, timer, cast-validation, and result state to `GameScene.Simulation`, driven only by session bootstrap, confirmed inputs, and fixed simulation frames.

## Acceptance Criteria

- Session seed and ordered decks initialize two independent worlds identically.
- Draw, cast, mana spend/regeneration, timer, and result transitions use integer/frame arithmetic.
- Invalid casts leave deterministic state unchanged and produce a typed rejection result.
- Player/resource/result state participates in canonical checkpoint hashing.
- Same bootstrap and confirmed-input fixture produces identical hashes at every checkpoint.
- Edit Mode golden tests cover start, draw, selection, valid/invalid cast, mana, timer, and result.

## Non-goals

- Prefab/object lifecycle (#353), physics (#356), or magic effects (#355).
- Unity UI rendering (#352).
- Server-authoritative validation fallback.

## Context / Constraints

- Build on #360 typed raw confirmed-frame inputs and queue.
- Port behavior, not Java implementation accidents; all collection ordering must be explicit.
- `GameScene.Simulation` cannot reference UnityEngine or scene/data assemblies.
- Card and magic identifiers crossing the wire remain stable strings until deterministic config types are introduced.

## Affected Repositories and Contracts

- `WordOnlineClient`: simulation resource model, input translation boundary, canonical hash, Edit Mode fixtures.
- Read-only behavioral reference: Game server `feature/lockstep` legacy resource systems.
- No wire change expected; any missing deterministic config becomes an explicit follow-up contract issue.

## Approach

- [x] Recon server resource rules and client config identifiers
- [x] Define deterministic player/deck/hand/mana/timer/result state
- [x] Translate confirmed `useMagic` inputs into resource commands
- [x] Add canonical hashing
- [x] Add golden and twin-world tests
- [x] Focused validation
- [x] Compatibility and regression validation (Unity Edit Mode 14/14)
- [x] Release order and rollback check

## Validation

- Commands: Unity 2022.3.34f1 Edit Mode suite with local-only `WebGLInput` comment workaround; `git diff --check`; `graphify update .`.
- Manual checks: compare fixtures with legacy server rules and #360 wire payload fields.
- Expected results: twin worlds retain identical hashes; invalid operations cannot partially mutate state.

## Risks & Rollback

- Mana/card parameters are currently split across runtime config and legacy Java behavior; missing values must not be guessed.
- String card ordering can vary by comparer; use ordinal comparison and ordered lists.
- Rollback is branch/PR revert before Client integration merge; no runtime fallback exists.

## Release Order

1. #360 merged into Client `feature/lockstep` (complete).
2. Merge #359 into Client `feature/lockstep`.
3. Build #353 on the updated integration branch.

## Open Questions

- Exact mana regeneration and timer constants must come from versioned game config, not Unity frame time.
- Result authority/input shape is finalized in later Game #290; this issue models deterministic state transitions and typed claims only.
- Card selection is local visual intent, not confirmed gameplay state. Only the final confirmed `useMagic` recipe enters deterministic state and hashes.
