# Lockstep production magic support

The canonical database catalog contains 67 IDs (`1..68`, excluding retired ID `34`). Every ID is registered in `ProductionMagicCatalog`; missing or duplicate config aborts setup.

| Deterministic family | Production magic IDs/names | Behavior |
| --- | --- | --- |
| Spawn | 1 ember_spirit_swarm, 2 water_slime_swarm, 3 mini_rock_swarm, 4 seed_spirit_swarm, 5 wind_spirit, 28 aqua_archer, 29 rock_golem, 30 storm_rider, 31 thunder_spirit, 32 fire_spirit, 39 magma_spirit, 42 wind_slime_swarm, 43 tornado_strike, 46 cloud_dragon, 47 thunder_bird_swarm, 48 tree_golem, 49 vine_spirit, 51 rock_mage, 55 chicken_commando, 58 zap_mouse, 64 fire_lord_spirit, 65 dimension_toad, 68 bubble_spirit | Stable per-cast offset stream and ordered spawn IDs |
| Projectile | 6 water_shot, 7 fire_shot, 8 tide_call, 9 chain_lightning, 10 vine_toss, 11 rock_rolling, 12 wind_blade, 13 nature_shot, 21 will_o_wisp, 33 rock_shot, 45 lightning_shot | Fixed velocity, BEPU contact damage/status, deterministic destroy |
| Area | 19 magma_explosion, 20 water_explosion, 22 nature_explosion, 35 lightning_drop, 36 wind_explosion, 37 rock_explosion, 38 fire_explosion, 41 sand_storm, 44 meteor_shower, 54 rock_drop, 56 overgrowth, 59 rallying_torch, 60 lightning_explosion, 61 razor_gale, 67 shock_overload | Entity-ID ordered radius query, damage and timed status |
| Build | 14 fire_slime_nest, 15 water_slime_nest, 16 life_tree, 17 rock_turret, 18 wind_totem, 23 frenzy_totem, 25 cannon, 26 tower, 27 mana_well, 40 healing_totem, 50 vine_colony, 52 pve_nature_slime_nest, 53 pve_vine, 57 crater, 62 bubble_generator, 63 electric_tower, 66 towerback | Ordered spawn and timed component lifecycle |
| Drop | 24 leafair | Targeted ordered spawn and timed lifecycle |

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
| Mob detectors/state machines/pathfinders/attackers | Owned by #358; not magic component behavior |
