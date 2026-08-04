# Lockstep Game server magic parity

현재 Game server source의 concrete Spring magic bean 67개를 source of truth로 사용한다. DB magic catalog 불일치는 범위에서 제외한다.

| Stable ID | Server bean | Family | Root prefab | Catalogued | Unique behavior | Runtime wired | Parity fixture |
|---:|---|---|---|---|---|---|---|
| 1 | `aqua_archer` | Spawn | `AquaArcher` | yes | base AI | production pipeline | full replay |
| 2 | `bubble_generator` | Build | `BubbleGenerator` | yes | no | production pipeline | full replay |
| 3 | `bubble_spirit` | Spawn | `BubbleSpirit` | yes | aerial-only delayed splash | production pipeline | EditMode + full replay |
| 4 | `cannon` | Build | `GroundCannon` | yes | no | production pipeline | full replay |
| 5 | `chain_lightning` | Projectile | `ChainLightning` | yes | no | production pipeline | full replay |
| 6 | `chicken_commando` | Drop | `ChickenCommando` | yes | no | production pipeline | full replay |
| 7 | `cloud_dragon` | Spawn | `CloudDragon` | yes | base AI | production pipeline | full replay |
| 8 | `crater` | Build | `Crater` | yes | no | production pipeline | full replay |
| 9 | `dimension_toad` | Spawn | `DimensionToad` | yes | panic + infinite alternating tadpoles | production pipeline | EditMode + full replay |
| 10 | `electric_tower` | Build | `ElectricTower` | yes | chain attack | production pipeline | full replay |
| 11 | `ember_spirit_swarm` | Spawn | `EmberSpirit` | yes | base AI | production pipeline | full replay |
| 12 | `fire_drop` | Drop | `FireDrop` | yes | no | production pipeline | full replay |
| 13 | `fire_lord_spirit` | Spawn | `FireLordSpirit` | yes | keep distance + child spawn | production pipeline | full replay |
| 14 | `fire_shot` | Projectile | `FireShot` | yes | no | production pipeline | full replay |
| 15 | `fire_slime_nest` | Build | `FireSlime` | yes | base AI | production pipeline | full replay |
| 16 | `fire_spirit` | Spawn | `FireSpirit` | yes | two-area spray | production pipeline | EditMode + full replay |
| 17 | `frenzy_totem` | Drop | `FrenzyTotem` | yes | no | production pipeline | full replay |
| 18 | `healing_totem` | Build | `HealingTotem` | yes | no | production pipeline | full replay |
| 19 | `leafair` | Drop | `Leafair` | yes | no | production pipeline | full replay |
| 20 | `life_tree` | Build | `LifeTree` | yes | no | production pipeline | full replay |
| 21 | `lightning_drop` | Drop | `LightningDrop` | yes | no | production pipeline | full replay |
| 22 | `lightning_explosion` | Area | `ElectricExplode` | yes | no | production pipeline | full replay |
| 23 | `lightning_shot` | Projectile | `ElectricShot` | yes | no | production pipeline | full replay |
| 24 | `lightning_slime_nest` | Build | `ElectricSlime` | yes | base AI | production pipeline | full replay |
| 25 | `magma_explosion` | Area | `MagmaExplosion` | yes | no | production pipeline | full replay |
| 26 | `magma_spirit` | Spawn | `MagmaSpirit` | yes | target summon | production pipeline | full replay |
| 27 | `mana_well` | Build | `ManaWell` | yes | owner mana charge modifier | production pipeline | EditMode + full replay |
| 28 | `meteor_shower` | Drop | `MeteorShower` | yes | no | production pipeline | full replay |
| 29 | `mini_rock_swarm` | Spawn | `MiniRock` | yes | base AI | production pipeline | full replay |
| 30 | `nature_drop` | Drop | `NatureDrop` | yes | no | production pipeline | full replay |
| 31 | `nature_slime_nest` | Build | `PveNatureSlimeNest` | yes | no | production pipeline | full replay |
| 32 | `overgrowth` | Area | `Overgrowth` | yes | no | production pipeline | full replay |
| 33 | `pve_water_slime_nest` | Build | `PveWaterSlimeNest` | yes | no | production pipeline | full replay |
| 34 | `rallying_torch` | Drop | `RallyingTorch` | yes | no | production pipeline | full replay |
| 35 | `razor_gale` | Area | `RazorGale` | yes | no | production pipeline | full replay |
| 36 | `rock_drop` | Drop | `RockDrop` | yes | no | production pipeline | full replay |
| 37 | `rock_golem` | Spawn | `RockGolem` | yes | base AI | production pipeline | full replay |
| 38 | `rock_mage` | Spawn | `RockMage` | yes | deterministic second target | production pipeline | EditMode + full replay |
| 39 | `rock_rolling` | Projectile | `RockRolling` | yes | no | production pipeline | full replay |
| 40 | `rock_slime_nest` | Build | `RockSlime` | yes | base AI | production pipeline | full replay |
| 41 | `rock_turret` | Build | `RockTurret` | yes | delayed turret | production pipeline | full replay |
| 42 | `sand_storm` | Area | `SandStorm` | yes | no | production pipeline | full replay |
| 43 | `seed_spirit_swarm` | Spawn | `SeedSpirit` | yes | base AI | production pipeline | full replay |
| 44 | `shock_overload` | Area | `ShockOverload` | yes | no | production pipeline | full replay |
| 45 | `storm_rider` | Spawn | `StormRider` | yes | player priority | production pipeline | full replay |
| 46 | `thunder_bird_swarm` | Spawn | `ThunderBird` | yes | 3D hover/dive/drop-scaled damage | production pipeline | EditMode + full replay |
| 47 | `thunder_spirit` | Spawn | `ThunderSpirit` | yes | base AI | production pipeline | full replay |
| 48 | `tide_call` | Projectile | `TideCall` | yes | no | production pipeline | full replay |
| 49 | `tornado_strike` | Spawn | `TornadoStrike` | yes | no | production pipeline | full replay |
| 50 | `tower` | Build | `GroundTower` | yes | delayed splash | production pipeline | full replay |
| 51 | `towerback` | Build | `Towerback` | yes | no | production pipeline | full replay |
| 52 | `tree_golem` | Spawn | `TreeGolem` | yes | base AI | production pipeline | full replay |
| 53 | `vine_colony` | Build | `VineColony` | yes | no | production pipeline | full replay |
| 54 | `vine_fan` | Projectile | `Vine` | yes | no | production pipeline | full replay |
| 55 | `vine_spirit` | Spawn | `VineSpirit` | yes | base AI | production pipeline | full replay |
| 56 | `vine_toss` | Projectile | `Vine` | yes | no | production pipeline | full replay |
| 57 | `water_explosion` | Area | `WaterExplosion` | yes | no | production pipeline | full replay |
| 58 | `water_shot` | Projectile | `WaterShot` | yes | no | production pipeline | full replay |
| 59 | `water_slime_swarm` | Spawn | `WaterSlime` | yes | base AI | production pipeline | full replay |
| 60 | `will_o_wisp` | Projectile | `WillOWisp` | yes | no | production pipeline | full replay |
| 61 | `wind_blade` | Projectile | `WindBlade` | yes | no | production pipeline | full replay |
| 62 | `wind_drop` | Drop | `WindDrop` | yes | no | production pipeline | full replay |
| 63 | `wind_explosion` | Area | `WindExplode` | yes | no | production pipeline | full replay |
| 64 | `wind_slime_nest` | Build | `WindSlime` | yes | base AI | production pipeline | full replay |
| 65 | `wind_spirit` | Spawn | `WindSpirit` | yes | self destruct | production pipeline | full replay |
| 66 | `wind_totem` | Build | `WindTotem` | yes | no | production pipeline | full replay |
| 67 | `zap_mouse` | Spawn | `ZapMouse` | yes | threat panic + cooldown | production pipeline | EditMode + full replay |

## Port status

- Game server source inventory를 67 magic, 96 prefab, 120 component manifest로 고정했다.
- 모든 magic은 versioned production catalog에서 generic fallback 없이 생성된다.
- ready/start 계약은 실제 parameter 및 magic recipe cache version을 교환하고 peer 간
  불일치나 spectator의 stale cache를 simulation 시작 전에 거부한다.
- prefab physics, native/temp element, status, effect receiver, projectile, field, explosion,
  drop, build, spawn, special AI, PVE boss 동작은 `GameScene.Simulation`에서 fixed frame으로 실행된다.
- effect receiver는 server prefab의 실제 component 구성을 따른다. receiver가 없는 PVE boss와
  임시 object는 일반 status를 받지 않지만 server `ShockOverloadExplosion`의 direct stun 예외는
  동일하게 적용하며, ground/air target mask는 prefab 상수가 아니라 현재 Z 높이로 판정한다.
- `ResourceSimulation`, `MagicSimulation`, `MobSimulation`, `PveSessionSimulation`은 하나의
  `DeterministicMatchSimulation` frame pipeline과 하나의 BEPU int world를 공유한다.
- PVP, Practice bot, PVE bootstrap/result/script event가 production controller에 연결된다.
- PVE script event의 `speakerObjectId`는 simulation entity view로 해석되어 기존 머리 위
  말풍선에 표시되며, speaker view가 없는 경우에만 system message로 표시된다.
- fever mana/card acceleration, HP-based timeout result, peer result consensus,
  late-join history를 포함한 spectator replay-only mode와 persona-configured Practice bot이 연결된다.
- player/mob status, effects, gauges, ownership과 idle/attack aura는 기존 GameScene view에 전달된다.
- 67개 각각을 versioned magic/prefab/mob resolver로 실행하는 full production match replay가
  매 frame hash 일치를 검증한다.
- 96개 prefab 각각의 production lifecycle replay가 매 frame hash 일치를 검증하며,
  물리 경계 `Wall`만 명시적인 renderer 비대상이다.
- PVE는 relay가 전달한 실제 `scenarioId`만 사용한다. 누락 ID를 임의 scenario `1`로 대체하지 않고
  session 생성 단계에서 실패하며, 존재하지 않는 ID는 server scenario registry에서 거부한다.

## Validation evidence

- Unity EditMode: 165/165 pass
- Unity PlayMode: 7/7 pass
- Development WebGL: success (`loader.js`, `framework.js`, `.data`, `.wasm` generated)
- Game server `./gradlew test` and `./gradlew build`: success
- Unity가 자동 변경한 tracked `.meta`, DEV_BUILD 설정, TMP/Addressables 생성 파일과
  임시 WebGLInput workaround를 최종 diff에서 제거했다.
