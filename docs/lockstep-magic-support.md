# Lockstep production magic support

The current Game server source contains 67 concrete Spring magic beans. Their
bean names are the wire/config identity and every name is registered in
`ServerMagicManifest` and `ProductionMagicCatalog`; missing, duplicate, or
unknown config aborts setup. `docs/lockstep-production-parity.md` is the
canonical per-magic matrix.

| Deterministic family | Production magic IDs/names | Behavior |
| --- | --- | --- |
| Spawn | Single, swarm, aerial, summon-chain and PVE boss summons | Stable per-cast RNG, ordered spawn IDs, full mob registration |
| Projectile | Shot, chain, push, rolling, piercing, vine line/fan and mind control | Fixed velocity, BEPU contact/overlap, damage/status and deterministic destroy |
| Area | Instant, persistent, periodic, moving tornado, overgrowth and shock overload | Entity-ID ordered 3D overlap, damage, field spawn and timed status |
| Build | Towers, nests, totems, mana/heal structures, crater and colony | Ordered spawn, detector/attack/heal/spawner behavior and timed lifecycle |
| Drop | Element drops, frenzy, rally, leafair, meteor and chicken | Deterministic vertical physics, collision/below-ground triggers and ordered impact |

Tuning fields (`spawnCount`, lifetime, damage, range, radius, speed, status) are part of the versioned deterministic config. The checked-in defaults exercise the behavior contract; production tuning changes require a `configVersion` change.

Randomness never uses a shared global stream. Each cast derives its stream from `(sessionSeed, userId, requestId, magicId)`, so unrelated casts and arrival order cannot perturb its result.

## Component mapping

| Production component family | Deterministic representation |
| --- | --- |
| `MagicComponent`, `Shot`, `ChainShot`, `PushShot`, `WindBladeShot`, `VineToss`, `RollingRock` | Projectile instance with fixed velocity, canonical BEPU contact, damage/status, and lifetime |
| `Drop`, `Explode`, `WaterExplode`, `OvergrowthExplosion`, `ShockOverloadExplosion`, `LightningStrike` | Drop/area instance with ordered radius query and lifetime |
| `Spawner`, `PathSpawner`, `RandomAreaSpawner`, `SequentialLineSpawner`, `LimitedSequenceSpawner`, `CraterSpawner` | Stable cast-local RNG plus index-ordered spawn IDs |
| `Damageable`, `DamageInterceptor`, `GaugeComponent`, `PlayerHealthComponent` | Entity-ID keyed combat state with clamped health and canonical hash |
| `EffectProvider`, `AreaEffectProvider`, `KnockbackEffectProvider`, receiver/apply-policy classes | Ordinal status ID plus deterministic duration refresh policy |
| `AttackAura`, `Bubble`, `Burn`, `DOT`, `Frenzy`, `IdleAura`, `Inspired`, `Knockback`, `Panic`, `Shock`, `Slow`, `Snared`, `Stun`, `Wet` status classes | Ordered timed status entries; behavior consumers read these entries in later movement/attack systems |
| `TimedSelfDestroyer`, timed behavior components | Frame-counted instance lifecycle and idempotent world destroy |
| Mob detectors/state machines/pathfinders/attackers | `MobSimulation` archetypes with deterministic target order, A*, cooldown and attacks |
