# Lockstep renderer boundary

The dependency direction is one-way:

`GameScene.Simulation` → immutable snapshots → `GameScene.Simulation.Rendering` → Unity GameObjects/UI

- Simulation never references `UnityEngine`, `Transform`, animation, or UI.
- Renderer copies `Fix64` snapshot values to Unity floats only at the visual boundary.
- Interpolation changes only instantiated view transforms; it cannot mutate snapshots or the simulation world.
- Prefab IDs use an explicit binding table. Missing or duplicate bindings fail fast.
- Destroyed entities remove their binding, and bridge destruction clears all remaining instances so scene reload starts empty.
- Player mana/hand UI is projected from a detached `PlayerResourceSnapshot` through `ISimulationPlayerUi`.

Protocol/simulation coordinates `(x, y)` render on Unity's ground plane as `(x, z)`. The simulation `z` value maps to Unity height.
