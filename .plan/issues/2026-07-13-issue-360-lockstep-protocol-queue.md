# 2026-07-13 — Lockstep protocol handshake and confirmed-frame queue

- Date: 2026-07-13
- GitHub Issue: https://github.com/Apptive-Game-Team/WordOnlineClient/issues/360
- Owning repository: WordOnlineClient
- Status: Implemented; Unity suite blocked by pre-existing `WebGLInput` compile error

## Goal

Implement the Game #291 wire contract in Unity: exact-version ready handshake, typed session/abort/confirmed-frame payloads, strict frame ordering, and duplicate confirmed-frame rejection before gameplay systems consume inputs.

## Acceptance Criteria

- Non-spectator clients subscribe before sending `ClientReady` with exact protocol, simulation, and config versions.
- `lockstepSessionStart`, `confirmedFrame`, and `lockstepAbort` deserialize without stringly-typed dispatch outside the protocol adapter.
- Confirmed frames apply exactly once in ascending frame order; duplicate, stale, and gapped delivery has explicit behavior and tests.
- Previous-frame hashes and next-frame local inputs serialize to the Game #291 contract.
- Abort/version mismatch returns to lobby without authoritative fallback or reconnect resume.
- Edit Mode protocol and queue tests pass.

## Non-goals

- Player/card/mana gameplay simulation (#359).
- Prefab lifecycle or renderer integration (#353/#352).
- Restoring old server-state handlers as a fallback.

## Context / Constraints

- Unity version follows `ProjectSettings/ProjectVersion.txt`; module docs contain stale patch-version text.
- Runtime namespace remains `GameScene.Simulation`.
- `JsonUtility` cannot directly deserialize dictionaries; peer hashes need a wire representation or a narrow parser boundary.
- Game server sessions do not support frame resume after disconnect.
- Existing `StompConnector` automatically resubscribes; game-session reconnect behavior must instead fail closed.

## Affected Repositories and Contracts

- `WordOnlineClient`: protocol DTOs, STOMP game flow, confirmed-frame queue, tests.
- Consumes `WordOnlineServer#291`: `/app/game/lockstep/ready/{sessionId}/{userId}`, `/app/game/lockstep/input/{sessionId}/{userId}`, and frame-info destinations.
- No server changes expected. Any wire mismatch blocks implementation and must be corrected in the integration branches together.

## Approach

- [x] Recon
- [x] Add protocol constants/DTOs and JSON adapter
- [x] Add ordered confirmed-frame queue with duplicate/stale/gap policy
- [x] Wire subscription → ready → session start → confirmed frame flow
- [x] Serialize next-frame submissions with previous checkpoint hash
- [x] Fail closed on abort/disconnect/version mismatch
- [x] Focused validation (new scripts compile; queue tests added)
- [ ] Compatibility and regression validation (blocked by existing `Assets/Scripts/Global/KeyInputSetting.cs` missing `WebGLInput`)
- [x] Release order and rollback check

## Validation

- Commands: Unity Edit Mode tests for `GameScene.Simulation.Tests`; Unity compile/IL post-processing log check; `git diff --check`.
- Manual checks: verify subscription precedes ready send; inspect JSON fixtures against Game #291 protocol doc.
- Expected results: ordered frames delivered once, gaps held, stale duplicates ignored, mismatch/abort never enters simulation.

## Risks & Rollback

- JSON field shape drift can silently default values under `JsonUtility`; fixture tests must assert every required field.
- Reconnect controller currently assumes resubscription is valid; lockstep game sessions require lobby fallback instead.
- Rollback means reverting this issue branch before it merges into Client `feature/lockstep`; there is no runtime authoritative fallback.

## Release Order

1. Merge Game #291 into Game `feature/lockstep` (complete).
2. Merge this issue into Client `feature/lockstep`.
3. Build #359 and later simulation issues on updated Client integration branch.
4. Merge Client/Game integration branches to main only after #348 and Game #290 complete.

## Open Questions

- Confirm whether config version is a content hash or an explicit release identifier; client implementation treats it as an opaque exact-match string.
- Peer-hash map is diagnostic only for the client queue; state comparison remains server-side.
