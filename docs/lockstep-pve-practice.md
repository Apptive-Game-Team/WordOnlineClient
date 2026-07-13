# Lockstep PVE and Practice

- `START_PVE_SCENARIO` resolves only against a checked-in `PveScenarioCatalog` with exact `configVersion`.
- Scenario events use `(frame, sequence)` total ordering and spawn through `SimulationWorld`.
- Negative bot user IDs are local actors. `LockstepParticipantPolicy` excludes them from network frame quorum and peer-hash requirements.
- `PracticeBot` uses session-seeded deterministic random and emits ordinary `ConfirmedInputMessage` / `USE_CARD` inputs.
- Solo PVE and Practice require one non-negative participant, but never wait for a second peer.
- PVE result is simulation state: all objectives destroyed means win; frame limit means loss.

Production scenario data must be exported into versioned definitions before release. Runtime database order, wall-clock timers, and `ThreadLocalRandom` are forbidden simulation inputs.
