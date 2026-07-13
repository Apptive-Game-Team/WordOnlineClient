# Deterministic Simulation Contract

This document defines the client simulation boundary introduced by issue #347.
It is intentionally smaller than the gameplay port tracked by #348.

## Runtime boundary

- `GameScene.Simulation` has no Unity engine references.
- Unity input, networking, rendering, animation, audio, and interpolation stay outside the assembly.
- The simulation advances only through confirmed frames. Rendering must never mutate simulation state.

## Numeric and time contract

- Simulation rate is 20 Hz and BEPU `TimeStepDuration` is fixed to `Fix64(1/20)`.
- Simulation and physics values use `FixedMath.Net.Fix64` Q31.32 integers.
- Floating-point conversion is restricted to IO and rendering adapters.
- BEPU is initialized with a null parallel looper; physics always steps single-threaded.

## Ordering and identity contract

- Each world owns its entity ID counter; IDs start at zero and increase monotonically.
- Confirmed inputs are applied by `userId`, then input `sequence`.
- Protocol validation must reject duplicate `(frameNum, userId, sequence)` keys.
- Entities update and serialize by ascending entity ID.
- Future component systems must have explicit stable type and execution order IDs.

## Random contract

- One world-owned xoshiro128** stream is initialized from the signed session seed via SplitMix64.
- Rejection sampling is used for bounded integers.
- RNG state and draw count are included in every state hash.
- Gameplay systems must document their draw count per branch. Rendering may not draw from this stream.

## Hash contract

- State fields use explicit little-endian byte order and fixed field order.
- Current vertical slice uses FNV-1a 64-bit to expose deterministic drift cheaply.
- The hash includes frame, next entity ID, RNG state/draw count, entity count, and BEPU position, linear/angular velocity, orientation, and lifecycle state.
- Wire encoding and final hash algorithm must be frozen with Game #291 before protocol cutover.

## Physics decision gate

`bepuphysics1int` commit `9237daa68c3014fd7c2e93c6a99326ba5248d60b` is vendored as managed Release/AnyCPU assemblies for the spike. Production acceptance still requires:

- Unity 2022.3.34f1 Editor compilation;
- Development WebGL IL2CPP/AOT build and runtime;
- required overlap, raycast, collision-rule, and movement behavior;
- multithreading disabled and no float-based world setup;
- deterministic replay hashes across Editor and WebGL;
- acceptable step time and allocation at representative entity counts;
- license and source-vendoring approach recorded.

The upstream project documents a roughly 1000-unit world limit, slower performance than its float version, mandatory single-threading for determinism, and avoidance of floating-point setup. WordOnline must measure these constraints rather than assume compatibility.
