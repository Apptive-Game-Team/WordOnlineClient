# Lockstep prefab/component support matrix

This file is the fail-fast allowlist for deterministic simulation config. A Unity prefab existing under `Resources` does not make it deterministic. Only IDs registered in `SimulationPrefabRegistry` may enter the simulation.

| Simulation prefab | Deterministic components | Lifecycle | Gameplay behavior |
| --- | --- | --- | --- |
| `Default` | `Transform`, `PhysicsBody` | Supported | Physics slice only |
| `Player` | `Transform`, `PhysicsBody`, `PlayerState` | Supported | Resources in #359; full player bridge pending #352 |
| Magic, rune, field, drop, projectile | `Transform`, `PhysicsBody`, magic instance state | Supported | Deterministic families in #355 |
| Mob, nest, build, tower | — | Rejected | Planned #358 |
| PVE scenario object | — | Rejected | Planned #354 |
| Visual/UI/effect prefab | Not simulation state | N/A | Renderer bridge planned #352 |

All supported simulation prefabs use versioned `Fix64` radius/mass/trigger physics definitions. Bounds, full BEPU body state, and canonical collision/trigger events participate in checkpoints. Protocol `(x, z)` maps to the simulation `(x, y)` plane; Unity `Transform` never feeds state back.

Component order in each definition is canonical state order. Duplicate prefab/component IDs and unknown prefab IDs are configuration errors. They must abort setup instead of silently substituting a Unity prefab.
