# Lockstep E2E replay verification

`LockstepReplayVerifier` replays one confirmed input list into two independent worlds. Every frame retains `(frame, primaryHash, replicaHash)`. Divergence retains both hexadecimal hashes and the first mismatched frame.

`LockstepSessionGuard` retains terminal diagnostics for:

- confirmed-frame timeout
- hash mismatch
- transport disconnect
- protocol, simulation, config, parameter-data, or magic-data version mismatch

PVP golden replay runs two worlds for 120 checkpoints. Practice/PVE fixture starts with one network participant plus a negative local bot, emits a normal bot frame input, and reaches a deterministic result without peer hash.

Ready/start exchanges the exact cached parameter and magic recipe version tokens.
Human peers must report the same tokens, and every player or spectator verifies
the echoed tokens against its local cache before simulation or replay.

At a terminal state the runtime sends `(frame, post-frame hash, loser)` instead
of a future frame. Two-human sessions require exact server-side result
consensus; solo PVE/Practice trusts its sole human. The normal server `result`
payload remains the only transition to `ResultScene`.

Spectator는 자신의 authenticated frame route를 구독한 뒤 `spectate` 등록을 보낸다.
서버가 session start와 지금까지의 confirmed frames를 순서대로 재전송하므로 중간 입장도
frame 1부터 같은 simulation을 replay한다. Spectator는 ready/input/hash/result를 보내지 않는다.

Development WebGL verification uses `BuildScript.BuildDevWebGL`, which now sets both `DEV_BUILD` and Unity `BuildOptions.Development`.
