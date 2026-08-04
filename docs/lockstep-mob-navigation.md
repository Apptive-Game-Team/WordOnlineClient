# Deterministic mob navigation

`GameScene.Simulation.Mob` replaces server-side mob timing, target selection, and path generation for lockstep sessions.

## Protocol rules

- State timers store simulation frame numbers. Wall-clock time and Unity `Update` are not inputs.
- Target candidates are compared by squared Fix64 distance, then entity ID.
- Navigation uses integer cells and four orthogonal neighbors.
- A* neighbors are generated North, East, South, West.
- Open-set total order is `f cost`, `heuristic`, grid row (`Y`), then column (`X`).
- Movement is emitted as reserved deterministic `SetVelocity` inputs; no accumulated velocity is used.
- Mob state and current path participate in canonical state hashing.

## Server mapping

| Server responsibility | Lockstep simulation |
| --- | --- |
| `StateMachineMob` / `BehaviorMob` | `MobSimulation` and `MobBehaviorState` |
| `ClosestEnemyDetector` | squared-distance selection with entity-ID tie-break |
| `SimplePathFinder` | `DeterministicAStar` |
| float `attackInterval` / detector timer | absolute simulation frame deadlines |
| `RigidBody.addVelocity` | per-frame `SetVelocity` |

The production 18×10 arena grid, bounds, prefab-specific balance definitions,
status-modified cooldowns, aerial height, and PVE boss behavior are constructed
by the versioned production factories. The navigation grid is immutable after
session construction so every peer hashes the same topology.
