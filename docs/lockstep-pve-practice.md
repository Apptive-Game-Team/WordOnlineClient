# Lockstep PVE and Practice

- `START_PVE_SCENARIO` resolves only against a checked-in `PveScenarioCatalog` with exact `configVersion`.
- Scenario events use `(frame, sequence)` total ordering and spawn through `SimulationWorld`.
- Negative bot user IDs are local actors. `LockstepParticipantPolicy` excludes them from network frame quorum and peer-hash requirements.
- `PracticeBot` uses session-seeded deterministic random and server-provided
  persona reaction/thinking frames. It mirrors recipe scoring, offensive versus
  placement choice, nearest in-range targeting, mana waiting, and card cycling,
  then emits ordinary `ConfirmedInputMessage` / `useMagic` inputs.
- Solo PVE and Practice require one non-negative participant, but never wait for a second peer.
- Bot-only sessions are rejected because client lockstep has no simulation host.
- PVE result is simulation state: all objectives destroyed means win; frame limit means loss.
- PVE script event는 frame 기준으로 한 번만 발생한다. presentation adapter는
  `speakerObjectId`를 renderer binding으로 찾아 해당 객체의 기존 speech bubble에
  순서대로 표시하고, 표시할 객체가 없을 때만 system message로 fallback한다.

Production scenarios 11–14 are checked into `ProductionPveScenarioCatalog` in
canonical `(frame, sequence)` order. Runtime database order, wall-clock timers,
and `ThreadLocalRandom` are forbidden simulation inputs.
