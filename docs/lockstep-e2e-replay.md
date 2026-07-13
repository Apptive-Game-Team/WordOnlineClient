# Lockstep E2E replay verification

`LockstepReplayVerifier` replays one confirmed input list into two independent worlds. Every frame retains `(frame, primaryHash, replicaHash)`. Divergence retains both hexadecimal hashes and the first mismatched frame.

`LockstepSessionGuard` retains terminal diagnostics for:

- confirmed-frame timeout
- hash mismatch
- transport disconnect
- protocol, simulation, or config version mismatch

PVP golden replay runs two worlds for 120 checkpoints. Practice/PVE fixture starts with one network participant plus a negative local bot, emits a normal bot frame input, and reaches a deterministic result without peer hash.

Development WebGL verification uses `BuildScript.BuildDevWebGL`, which now sets both `DEV_BUILD` and Unity `BuildOptions.Development`.
