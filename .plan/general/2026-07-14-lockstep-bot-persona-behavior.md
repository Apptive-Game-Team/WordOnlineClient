# 2026-07-14 — Lockstep Bot Persona Behavior

- Date: 2026-07-14
- GitHub Issue: None (continuation of lockstep implementation)
- Owning repository: WordOnlineClient
- Status: Implemented; Unity EditMode execution pending

## Goal

Apply every lockstep bot persona field to deterministic client-side decisions.

## Acceptance Criteria

- `tier` changes deterministic decision quality instead of being passive metadata.
- `counterAggression` changes counter-aware recipe scoring and is clamped to `[0, 1]`.
- Identical seed, persona, frame, hand, mana, and world produce identical bot input and state hash.
- Persona behavior is covered by EditMode tests and the lockstep replay remains stable.

## Non-goals

- Restoring server-authoritative bot execution.
- Changing bot persona persistence or administration APIs.
- Adding new magic recipes or combat balance data.

## Context / Constraints

- The server remains a relay and sends normalized persona configuration in `LockstepBotConfigDto`.
- The previous server `tierSkillBonus` added the same value to every candidate and therefore did not alter selection.
- Client decisions must avoid wall-clock time, unordered iteration, and non-deterministic random sources.

## Affected Repositories and Contracts

- WordOnlineClient: consumes `tier` and `counterAggression` in `PracticeBot`.
- WordOnlineServer: existing session-start DTO contract remains unchanged; serialization tests provide boundary coverage.

## Approach

- [x] Recon
- [x] Implementation
- [x] Focused validation
- [ ] Compatibility and regression validation
- [x] Release order and rollback check

Use a canonical tier-to-skill mapping and the bot's seeded deterministic RNG to choose between the highest-scored candidate and a lower-ranked valid candidate. Add a stable elemental counter score derived from recipe cards and target prefab identity, scaled by normalized counter aggression. Include both persona values in the bot state hash.

## Validation

- Commands: Unity EditMode tests for `PvePracticeSimulationTests` and `LockstepE2EReplayTests`; full EditMode suite when the Editor is available.
- Manual checks: start Practice Debug twice with the same persona and confirm identical decisions/hashes.
- Expected results: tier/aggression tests differ where intended while same-input replay remains identical.

## Risks & Rollback

- Balance risk: tier and counter weights change bot choices. Keep mappings explicit and testable.
- Determinism risk: consuming RNG in different branches can desync. Use fixed candidate ordering and hash all persona/state inputs.
- Rollback: revert Client behavior while retaining the backward-compatible server DTO fields.

## Release Order

The current server contract already includes both fields. Publish the Client behavior after the lockstep server branch; merge both only with the complete lockstep release.

## Open Questions

- Final balance values may need playtest tuning after functional verification.
