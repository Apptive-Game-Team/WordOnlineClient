using System;
using System.Collections.Generic;
using FixMath.NET;
using GameScene.Simulation.Objects;

namespace GameScene.Simulation.Mob
{
    public readonly struct MobParameterValue
    {
        public string ObjectName { get; }
        public string ParameterName { get; }
        public Fix64 Value { get; }

        public MobParameterValue(string objectName, string parameterName, Fix64 value)
        {
            ObjectName = objectName ?? throw new ArgumentNullException(nameof(objectName));
            ParameterName = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
            Value = value;
        }
    }

    public sealed class ProductionMobDefinitionResolver : IMobDefinitionResolver
    {
        private const int FramesPerSecond = 20;
        private const int DetectIntervalFrames = FramesPerSecond + 1;
        private static readonly IReadOnlyDictionary<string, Entry> Entries =
            new Dictionary<string, Entry>(StringComparer.Ordinal)
            {
                ["EmberSpirit"] = new Entry("ember_spirit", MobArchetype.Slime,
                    SimulationPrefabRegistry.FireFieldPrefabId, SimulationPrefabRegistry.FireFieldPrefabId,
                    targetLayer: MobTargetLayer.Ground),
                ["SeedSpirit"] = new Entry("seed_spirit", MobArchetype.Slime,
                    SimulationPrefabRegistry.LeafFieldPrefabId, SimulationPrefabRegistry.LeafFieldPrefabId,
                    targetLayer: MobTargetLayer.Ground),
                ["LeafSlime"] = new Entry("leaf_slime", MobArchetype.Slime,
                    SimulationPrefabRegistry.LeafFieldPrefabId, targetLayer: MobTargetLayer.Ground),
                ["WaterSlime"] = new Entry("water_slime", MobArchetype.Slime,
                    SimulationPrefabRegistry.WaterFieldPrefabId, SimulationPrefabRegistry.WaterFieldPrefabId,
                    targetLayer: MobTargetLayer.Ground),
                ["FireSlime"] = new Entry("fire_slime", MobArchetype.Slime,
                    SimulationPrefabRegistry.FireFieldPrefabId, SimulationPrefabRegistry.FireFieldPrefabId,
                    targetLayer: MobTargetLayer.Ground),
                ["ElectricSlime"] = new Entry("electric_slime", MobArchetype.Slime,
                    null, SimulationPrefabRegistry.ElectricFieldPrefabId,
                    targetLayer: MobTargetLayer.Ground),
                ["RockSlime"] = new Entry("rock_slime", MobArchetype.Slime,
                    targetLayer: MobTargetLayer.Ground),
                ["WindSlime"] = new Entry("wind_slime", MobArchetype.Slime,
                    targetLayer: MobTargetLayer.Ground),
                ["MiniRock"] = new Entry("mini_rock", MobArchetype.Slime,
                    targetLayer: MobTargetLayer.Ground),
                ["RockGolem"] = new Entry("rock_golem", MobArchetype.Melee,
                    targetLayer: MobTargetLayer.Ground),
                ["TreeGolem"] = new Entry("tree_golem", MobArchetype.Melee,
                    SimulationPrefabRegistry.LeafFieldPrefabId, null, true,
                    targetLayer: MobTargetLayer.Ground),
                ["AquaArcher"] = new Entry("aqua_archer", MobArchetype.ProjectileRange),
                ["ThunderSpirit"] = new Entry("thunder_spirit", MobArchetype.ProjectileRange,
                    isAerial: true, targetLayer: MobTargetLayer.Ground),
                ["CloudDragon"] = new Entry("cloud_dragon", MobArchetype.ProjectileRange,
                    isAerial: true, auraStatusId: "wet", auraRangeParameter: "attack_range"),
                ["VineSpirit"] = new Entry("vine_spirit", MobArchetype.ProjectileRange,
                    attackStatusId: "snared"),
                ["FireChildSpirit"] = new Entry("fire_child_spirit", MobArchetype.ProjectileRange,
                    isAerial: true),
                ["RockMage"] = new Entry("rock_mage", MobArchetype.DoubleTarget),
                ["FireSpirit"] = new Entry("fire_spirit", MobArchetype.Spraying,
                    attackStatusId: "burn", auraStatusId: "burn",
                    auraRangeParameter: "sub_attack_range", targetLayer: MobTargetLayer.Ground),
                ["ThunderBird"] = new Entry("thunder_bird", MobArchetype.ThunderBird,
                    isAerial: true, targetLayer: MobTargetLayer.Ground)
            };

        private readonly Dictionary<string, MobDefinition> definitions =
            new Dictionary<string, MobDefinition>(StringComparer.Ordinal);

        public ProductionMobDefinitionResolver(IReadOnlyList<MobParameterValue> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                throw new InvalidOperationException("Versioned game parameters must be loaded before mob simulation starts");
            foreach (KeyValuePair<string, Entry> pair in Entries)
                definitions.Add(pair.Key, CreateDefinition(parameters, pair.Value));
            AddStationaryDefinitions(parameters);
        }

        public bool TryResolve(string prefabId, out MobDefinition definition)
        {
            if (prefabId != null && definitions.TryGetValue(prefabId, out definition)) return true;
            definition = null;
            return false;
        }

        private static MobDefinition CreateDefinition(IReadOnlyList<MobParameterValue> parameters, Entry entry)
        {
            Fix64 radius = Required(parameters, entry.ObjectName, "radius");
            bool configuredRange = entry.Archetype == MobArchetype.ProjectileRange ||
                entry.Archetype == MobArchetype.DoubleTarget || entry.Archetype == MobArchetype.Spraying;
            configuredRange = configuredRange || entry.Archetype == MobArchetype.ThunderBird;
            Fix64 attackRange = configuredRange
                ? Required(parameters, entry.ObjectName, "attack_range") : radius + (Fix64)0.5m;
            return new MobDefinition(
                Required(parameters, entry.ObjectName, "speed"),
                (Fix64)1024,
                attackRange,
                ToInt(Required(parameters, entry.ObjectName, "damage")),
                StrictElapsedFrames(Required(parameters, entry.ObjectName, "attack_interval")),
                DetectIntervalFrames,
                ToInt(Required(parameters, entry.ObjectName, "hp")),
                radius,
                entry.Archetype,
                entry.PathSpawnPrefabId,
                entry.PathSpawnPrefabId == null ? Fix64.Zero : Fix64.One,
                entry.DeathSpawnPrefabId,
                entry.HasSelfHeal ? ToInt(Required(parameters, entry.ObjectName, "heal_amount")) : 0,
                entry.HasSelfHeal ? StrictElapsedFrames(Required(parameters, entry.ObjectName, "heal_interval")) : 0,
                isAerial: entry.IsAerial, targetLayer: entry.TargetLayer,
                attackStatusId: entry.AttackStatusId,
                auraStatusId: entry.AuraStatusId,
                auraRange: entry.AuraRangeParameter == null ? Fix64.Zero :
                    Required(parameters, entry.ObjectName, entry.AuraRangeParameter),
                auraIntervalFrames: entry.AuraStatusId == null ? 0 : StrictElapsedFrames(Fix64.One));
        }

        private void AddStationaryDefinitions(IReadOnlyList<MobParameterValue> parameters)
        {
            AddCrater(parameters);
            AddChickenCommando(parameters);
            Fix64 windLifetime = Required(parameters, "wind_totem", "hp") *
                Required(parameters, "wind_totem", "attack_interval") /
                Required(parameters, "wind_totem", "damage");
            AddWindTotem(parameters, AtLeastElapsedFrames(windLifetime));
            AddTotem(parameters, "LifeTree", "life_tree");
            AddTotem(parameters, "HealingTotem", "healing_totem");
            AddStationaryAttacker(parameters, "RockTurret", "rock_turret", MobArchetype.Turret,
                AtLeastElapsedFrames(Required(parameters, "rock_turret", "hp")), Fix64.Zero,
                MobTargetLayer.Ground);
            AddStationaryAttacker(parameters, "GroundTower", "ground_tower", MobArchetype.Tower,
                AtLeastElapsedFrames(Required(parameters, "ground_tower", "duration")), Fix64.One,
                MobTargetLayer.Air);
            AddLightningTower(parameters);
            AddSummoner(parameters, "MagmaSpirit", "magma_spirit", "MagmaFist");
            AddSelfDestruct(parameters, "WindSpirit", "wind_spirit");
            AddKeepDistance(parameters);
            AddPlayerPriorityMelee(parameters, "StormRider", "storm_rider");
            AddCoward(parameters, "ZapMouse", "zap_mouse", MobArchetype.Coward, Fix64.Zero);
            AddCoward(parameters, "DimensionToad", "dimension_toad", MobArchetype.NonAttackingCoward,
                (Fix64)4, "FireTadpole", "LightningTadpole");
            AddTimedMelee(parameters, "FireTadpole", "fire_tadpole");
            AddTimedMelee(parameters, "LightningTadpole", "lightning_tadpole");
            AddBubbleSpirit(parameters);
            AddManaWell(parameters);
            AddBubbleGenerator(parameters);
            AddStationaryAttacker(parameters, "GroundCannon", "ground_cannon", MobArchetype.Turret,
                FramesPerSecond * 60, Fix64.Zero, MobTargetLayer.Ground,
                AtLeastElapsedFrames((Fix64)0.5m));
            AddStationarySummoner(parameters);
            AddTowerback(parameters);
            AddPveBoss(parameters, "PveNatureSlimeNest", "pve_nature_slime_nest",
                "LeafSlime", null, null, 1, false);
            AddPveBoss(parameters, "PveWaterSlimeNest", "pve_water_slime_nest",
                "WaterSlime", null, null, 1, false);
            AddPveBoss(parameters, "PveVineColony", "vine_colony",
                "LeafSlime", "WaterSlime", "VineSpirit", 2, true,
                (Fix64)7, AtLeastElapsedFrames((Fix64)2.5m), "vine_toss");
            AddPveBoss(parameters, "PveVineWitch", "pve_vine_witch",
                "LeafSlime", "WaterSlime", "VineSpirit", 2, true,
                (Fix64)7, FramesPerSecond * 8, "vine_toss", "vine_colony", "vine_fan");
            AddDestructibleSpawner(parameters, "FireSummon", "fire_summon", "FireSlime");
            AddDestructibleSpawner(parameters, "ElectricSummon", "electric_summon", "ElectricSlime");
            AddDestructibleSpawner(parameters, "RockSummon", "rock_summon", "RockSlime");
            AddDestructibleSpawner(parameters, "WindSummon", "wind_summon", "WindSlime");
        }

        private void AddDummy(IReadOnlyList<MobParameterValue> parameters,
            string prefabId, string objectName, int lifetimeFrames)
        {
            definitions.Add(prefabId, new MobDefinition(
                Fix64.Zero, Fix64.Zero, Fix64.Zero, 0, 1, DetectIntervalFrames,
                ToInt(Required(parameters, objectName, "hp")),
                Required(parameters, objectName, "radius"), MobArchetype.Dummy,
                lifetimeFrames: lifetimeFrames));
        }

        private void AddCrater(IReadOnlyList<MobParameterValue> parameters)
        {
            const string objectName = "crater";
            definitions.Add("Crater", new MobDefinition(
                Fix64.Zero, Fix64.Zero, Fix64.Zero, 0, 1, DetectIntervalFrames,
                ToInt(Required(parameters, objectName, "hp")),
                Required(parameters, objectName, "radius"), MobArchetype.Dummy,
                lifetimeFrames: AtLeastElapsedFrames(Required(parameters, objectName, "duration")),
                periodicSpawnPrefabId: "CraterEmber",
                periodicSpawnIntervalFrames: AtLeastElapsedFrames(
                    Required(parameters, objectName, "attack_interval"))));
        }

        private void AddChickenCommando(IReadOnlyList<MobParameterValue> parameters)
        {
            const string objectName = "chicken_commando";
            Fix64 fallSeconds = Fix64.Sqrt((Fix64)2 * Required(parameters, objectName, "spawn_height") /
                Required(parameters, objectName, "fall_gravity"));
            definitions.Add("ChickenCommando", new MobDefinition(
                Required(parameters, objectName, "speed"), (Fix64)1024,
                Required(parameters, objectName, "radius") * (Fix64)2,
                ToInt(Required(parameters, objectName, "damage")),
                StrictElapsedFrames(Required(parameters, objectName, "attack_interval")),
                DetectIntervalFrames, ToInt(Required(parameters, objectName, "hp")),
                Required(parameters, objectName, "radius"), MobArchetype.Melee,
                targetLayer: MobTargetLayer.Ground,
                spawnDelayFrames: AtLeastElapsedFrames(fallSeconds),
                initialHeight: Required(parameters, objectName, "spawn_height"),
                gravity: Required(parameters, objectName, "fall_gravity")));
        }

        private void AddWindTotem(IReadOnlyList<MobParameterValue> parameters, int lifetimeFrames)
        {
            const string objectName = "wind_totem";
            definitions.Add("WindTotem", new MobDefinition(
                Fix64.Zero, Fix64.Zero, Fix64.Zero, 0, 1, DetectIntervalFrames,
                ToInt(Required(parameters, objectName, "hp")),
                Required(parameters, objectName, "radius"), MobArchetype.Dummy,
                lifetimeFrames: lifetimeFrames,
                pushForce: Required(parameters, objectName, "push_force"),
                pushRangeX: Required(parameters, objectName, "push_range_x"),
                pushRangeY: Required(parameters, objectName, "push_range_y")));
        }

        private void AddTotem(IReadOnlyList<MobParameterValue> parameters, string prefabId, string objectName)
        {
            definitions.Add(prefabId, new MobDefinition(
                Fix64.Zero, Fix64.Zero, Fix64.Zero, 0, 1, DetectIntervalFrames,
                ToInt(Required(parameters, objectName, "hp")),
                Required(parameters, objectName, "radius"), MobArchetype.Totem,
                areaHealthDelta: ToSignedInt(Required(parameters, objectName, "damage")),
                areaEffectRange: Required(parameters, objectName, "range"),
                areaEffectIntervalFrames: AtLeastElapsedFrames(Required(parameters, objectName, "attack_interval")),
                lifetimeFrames: AtLeastElapsedFrames(Required(parameters, objectName, "duration"))));
        }

        private void AddStationaryAttacker(IReadOnlyList<MobParameterValue> parameters,
            string prefabId, string objectName, MobArchetype archetype,
            int lifetimeFrames, Fix64 splashRadius,
            MobTargetLayer targetLayer = MobTargetLayer.Any, int attackDelayFrames = 0)
        {
            definitions.Add(prefabId, new MobDefinition(
                Fix64.Zero, (Fix64)1024, Required(parameters, objectName, "attack_range"),
                ToInt(Required(parameters, objectName, "damage")),
                AtLeastElapsedFrames(Required(parameters, objectName, "attack_interval")),
                DetectIntervalFrames, ToInt(Required(parameters, objectName, "hp")),
                Required(parameters, objectName, "radius"), archetype,
                lifetimeFrames: lifetimeFrames,
                attackDelayFrames: attackDelayFrames == 0 ? AtLeastElapsedFrames((Fix64)0.2m) : attackDelayFrames,
                splashRadius: splashRadius, targetLayer: targetLayer));
        }

        private void AddLightningTower(IReadOnlyList<MobParameterValue> parameters)
        {
            const string objectName = "electric_tower";
            definitions.Add("ElectricTower", new MobDefinition(
                Fix64.Zero, (Fix64)1024, Required(parameters, objectName, "attack_range"),
                ToInt(Required(parameters, objectName, "damage")),
                AtLeastElapsedFrames(Required(parameters, objectName, "attack_interval")),
                DetectIntervalFrames, ToInt(Required(parameters, objectName, "hp")),
                Required(parameters, objectName, "radius"), MobArchetype.LightningTower,
                lifetimeFrames: AtLeastElapsedFrames(Required(parameters, objectName, "duration")),
                attackDelayFrames: AtLeastElapsedFrames((Fix64)0.2m),
                chainDamage: ToInt(Required(parameters, objectName, "chain_damage")),
                chainCount: ToInt(Required(parameters, objectName, "chain_count")),
                chainRadius: Required(parameters, objectName, "chain_radius")));
        }

        private void AddSummoner(IReadOnlyList<MobParameterValue> parameters,
            string prefabId, string objectName, string summonPrefabId)
        {
            Fix64 radius = Required(parameters, objectName, "radius");
            definitions.Add(prefabId, new MobDefinition(
                Required(parameters, objectName, "speed"), (Fix64)1024,
                Required(parameters, objectName, "attack_range"), 0,
                StrictElapsedFrames(Required(parameters, objectName, "attack_interval")),
                DetectIntervalFrames, ToInt(Required(parameters, objectName, "hp")), radius,
                MobArchetype.Summoner, summonPrefabId: summonPrefabId,
                targetLayer: MobTargetLayer.Ground,
                auraStatusId: "burn",
                auraRange: Required(parameters, objectName, "sub_attack_range"),
                auraIntervalFrames: StrictElapsedFrames(Fix64.One)));
        }

        private void AddSelfDestruct(IReadOnlyList<MobParameterValue> parameters,
            string prefabId, string objectName)
        {
            Fix64 radius = Required(parameters, objectName, "radius");
            definitions.Add(prefabId, new MobDefinition(
                Required(parameters, objectName, "speed"), (Fix64)1024,
                radius + (Fix64)0.3m,
                ToInt(Required(parameters, objectName, "damage")), 1,
                DetectIntervalFrames, ToInt(Required(parameters, objectName, "hp")), radius,
                MobArchetype.SelfDestruct,
                explosionRadius: Required(parameters, objectName, "attack_range"),
                isAerial: true, targetLayer: MobTargetLayer.Air));
        }

        private void AddKeepDistance(IReadOnlyList<MobParameterValue> parameters)
        {
            const string objectName = "fire_lord_spirit";
            definitions.Add("FireLordSpirit", new MobDefinition(
                Required(parameters, objectName, "speed"), (Fix64)1024, (Fix64)4,
                0, 1, DetectIntervalFrames,
                ToInt(Required(parameters, objectName, "hp")),
                Required(parameters, objectName, "radius"), MobArchetype.KeepDistance,
                periodicSpawnPrefabId: "FireChildSpirit",
                periodicSpawnIntervalFrames: AtLeastElapsedFrames((Fix64)5),
                periodicSpawnLimit: 5, isAerial: true));
        }

        private void AddPlayerPriorityMelee(IReadOnlyList<MobParameterValue> parameters,
            string prefabId, string objectName)
        {
            Fix64 radius = Required(parameters, objectName, "radius");
            definitions.Add(prefabId, new MobDefinition(
                Required(parameters, objectName, "speed"), (Fix64)1024,
                radius + (Fix64)0.5m, ToInt(Required(parameters, objectName, "damage")),
                StrictElapsedFrames(Required(parameters, objectName, "attack_interval")),
                DetectIntervalFrames, ToInt(Required(parameters, objectName, "hp")), radius,
                MobArchetype.PlayerPriorityMelee, targetLayer: MobTargetLayer.Ground));
        }

        private void AddCoward(IReadOnlyList<MobParameterValue> parameters, string prefabId,
            string objectName, MobArchetype archetype, Fix64 attackRange,
            string periodicSpawnPrefabId = null, string secondaryPeriodicSpawnPrefabId = null)
        {
            Fix64 radius = Required(parameters, objectName, "radius");
            if (attackRange == Fix64.Zero) attackRange = radius + (Fix64)0.5m;
            definitions.Add(prefabId, new MobDefinition(
                Required(parameters, objectName, "speed"), Required(parameters, objectName, "detection_range"), attackRange,
                archetype == MobArchetype.NonAttackingCoward ? 0 : ToInt(Required(parameters, objectName, "damage")),
                StrictElapsedFrames(Required(parameters, objectName, "attack_interval")),
                DetectIntervalFrames, ToInt(Required(parameters, objectName, "hp")),
                radius, archetype,
                periodicSpawnPrefabId: periodicSpawnPrefabId,
                periodicSpawnIntervalFrames: periodicSpawnPrefabId == null ? 0 : FramesPerSecond * 10,
                periodicSpawnLimit: 0,
                panicDurationFrames: AtLeastElapsedFrames(Required(parameters, objectName, "panic_duration")),
                panicCooldownFrames: FramesPerSecond * 10,
                panicSpeedMultiplier: (Fix64)2,
                secondaryPeriodicSpawnPrefabId: secondaryPeriodicSpawnPrefabId,
                targetLayer: MobTargetLayer.Ground));
        }

        private void AddTimedMelee(IReadOnlyList<MobParameterValue> parameters,
            string prefabId, string objectName)
        {
            Fix64 radius = Required(parameters, objectName, "radius");
            definitions.Add(prefabId, new MobDefinition(
                Required(parameters, objectName, "speed"), (Fix64)1024,
                radius + (Fix64)0.5m, ToInt(Required(parameters, objectName, "damage")),
                StrictElapsedFrames(Required(parameters, objectName, "attack_interval")),
                DetectIntervalFrames, ToInt(Required(parameters, objectName, "hp")), radius,
                MobArchetype.Melee,
                lifetimeFrames: AtLeastElapsedFrames(Required(parameters, objectName, "duration")),
                targetLayer: MobTargetLayer.Ground));
        }

        private void AddBubbleSpirit(IReadOnlyList<MobParameterValue> parameters)
        {
            const string objectName = "bubble_spirit";
            definitions.Add("BubbleSpirit", new MobDefinition(
                Required(parameters, objectName, "speed"), (Fix64)1024,
                Required(parameters, objectName, "attack_range"),
                ToInt(Required(parameters, objectName, "damage")),
                StrictElapsedFrames(Required(parameters, objectName, "attack_interval")),
                DetectIntervalFrames, ToInt(Required(parameters, objectName, "hp")),
                Required(parameters, objectName, "radius"), MobArchetype.BubbleSplash,
                isAerial: true, targetLayer: MobTargetLayer.Air,
                projectileSpeed: Required(parameters, objectName, "projectile_speed")));
        }

        private void AddManaWell(IReadOnlyList<MobParameterValue> parameters)
        {
            const string objectName = "mana_well";
            Fix64 lifetime = Required(parameters, objectName, "hp") *
                Required(parameters, objectName, "attack_interval") /
                Required(parameters, objectName, "damage");
            definitions.Add("ManaWell", new MobDefinition(
                Fix64.Zero, Fix64.Zero, Fix64.Zero, 0, 1, DetectIntervalFrames,
                ToInt(Required(parameters, objectName, "hp")),
                Required(parameters, objectName, "radius"), MobArchetype.Dummy,
                lifetimeFrames: AtLeastElapsedFrames(lifetime), manaChargePercent: (Fix64)0.8m));
        }

        private void AddBubbleGenerator(IReadOnlyList<MobParameterValue> parameters)
        {
            const string objectName = "bubble_generator";
            definitions.Add("BubbleGenerator", new MobDefinition(
                Fix64.Zero, Fix64.Zero, Required(parameters, objectName, "attack_range"),
                0, AtLeastElapsedFrames(Required(parameters, objectName, "attack_interval")),
                DetectIntervalFrames, ToInt(Required(parameters, objectName, "hp")),
                Required(parameters, objectName, "radius"), MobArchetype.BubbleGenerator,
                lifetimeFrames: AtLeastElapsedFrames(Required(parameters, objectName, "duration"))));
        }

        private void AddStationarySummoner(IReadOnlyList<MobParameterValue> parameters)
        {
            const string objectName = "vine_colony";
            definitions.Add("VineColony", new MobDefinition(
                Fix64.Zero, (Fix64)1024, Required(parameters, objectName, "attack_range"),
                0, AtLeastElapsedFrames(Required(parameters, objectName, "attack_interval")),
                DetectIntervalFrames, ToInt(Required(parameters, objectName, "hp")),
                Required(parameters, objectName, "radius"), MobArchetype.StationarySummoner,
                lifetimeFrames: AtLeastElapsedFrames(Required(parameters, objectName, "duration")),
                summonPrefabId: "Vine", targetLayer: MobTargetLayer.Ground));
        }

        private void AddTowerback(IReadOnlyList<MobParameterValue> parameters)
        {
            const string objectName = "towerback";
            definitions.Add("Towerback", new MobDefinition(
                Required(parameters, objectName, "sub_speed"), (Fix64)1024,
                Required(parameters, objectName, "sub_attack_range"),
                ToInt(Required(parameters, objectName, "sub_damage")),
                StrictElapsedFrames(Required(parameters, objectName, "attack_interval")),
                DetectIntervalFrames, ToInt(Required(parameters, objectName, "hp")),
                Required(parameters, objectName, "radius"), MobArchetype.DualAttack,
                targetLayer: MobTargetLayer.Ground,
                secondaryDamage: ToInt(Required(parameters, objectName, "damage")),
                secondaryAttackRange: Required(parameters, objectName, "attack_range"),
                secondaryAttackCooldownFrames: StrictElapsedFrames(
                    Required(parameters, objectName, "attack_interval")),
                secondaryAttackDelayFrames: AtLeastElapsedFrames((Fix64)0.2m)));
        }

        private void AddPveBoss(IReadOnlyList<MobParameterValue> parameters,
            string prefabId, string objectName, string firstSpawn, string secondSpawn,
            string thirdSpawn, int batchCount, bool selectSingle,
            Fix64 attackRange = default, int attackCooldownFrames = 1,
            string primaryMagic = null, string secondaryMagic = null, string rageMagic = null)
        {
            definitions.Add(prefabId, new MobDefinition(
                Fix64.Zero, primaryMagic == null ? Fix64.Zero : (Fix64)1024,
                attackRange, 0, attackCooldownFrames, DetectIntervalFrames,
                ToInt(Required(parameters, objectName, "hp")),
                Required(parameters, objectName, "radius"), MobArchetype.PveBoss,
                periodicSpawnPrefabId: firstSpawn,
                periodicSpawnIntervalFrames: StrictElapsedFrames((Fix64)10),
                periodicSpawnLimit: 0,
                secondaryPeriodicSpawnPrefabId: secondSpawn,
                tertiaryPeriodicSpawnPrefabId: thirdSpawn,
                periodicSpawnBatchCount: batchCount,
                selectSinglePeriodicPrefab: selectSingle,
                targetLayer: MobTargetLayer.Ground,
                primaryBossMagicName: primaryMagic,
                secondaryBossMagicName: secondaryMagic,
                rageBossMagicName: rageMagic));
        }

        private void AddDestructibleSpawner(IReadOnlyList<MobParameterValue> parameters,
            string prefabId, string objectName, string spawnPrefabId)
        {
            definitions.Add(prefabId, new MobDefinition(
                Fix64.Zero, Fix64.Zero, Fix64.Zero, 0, 1, DetectIntervalFrames,
                ToInt(Required(parameters, objectName, "hp")),
                Required(parameters, objectName, "radius"), MobArchetype.Dummy,
                periodicSpawnPrefabId: spawnPrefabId,
                periodicSpawnIntervalFrames: StrictElapsedFrames((Fix64)2),
                periodicSpawnLimit: 0, periodicSelfDamage: 1));
        }

        private static int StrictElapsedFrames(Fix64 seconds) =>
            checked((int)Fix64.Floor(seconds * FramesPerSecond) + 1);

        private static int AtLeastElapsedFrames(Fix64 seconds)
        {
            Fix64 frames = seconds * FramesPerSecond;
            int floor = checked((int)Fix64.Floor(frames));
            return frames == (Fix64)floor ? floor : checked(floor + 1);
        }

        private static int ToInt(Fix64 value) => checked((int)(value + (Fix64)0.5m));
        private static int ToSignedInt(Fix64 value) => value >= Fix64.Zero
            ? ToInt(value) : -ToInt(-value);

        private static Fix64 Required(IReadOnlyList<MobParameterValue> parameters,
            string objectName, string parameterName)
        {
            for (int index = 0; index < parameters.Count; index++)
            {
                MobParameterValue value = parameters[index];
                if (string.Equals(value.ObjectName, objectName, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(value.ParameterName, parameterName, StringComparison.OrdinalIgnoreCase)) return value.Value;
            }
            throw new InvalidOperationException("Missing deterministic mob parameter: " + objectName + "." + parameterName);
        }

        private readonly struct Entry
        {
            public string ObjectName { get; }
            public MobArchetype Archetype { get; }
            public string PathSpawnPrefabId { get; }
            public string DeathSpawnPrefabId { get; }
            public bool HasSelfHeal { get; }
            public bool IsAerial { get; }
            public MobTargetLayer TargetLayer { get; }
            public string AttackStatusId { get; }
            public string AuraStatusId { get; }
            public string AuraRangeParameter { get; }
            public Entry(string objectName, MobArchetype archetype,
                string pathSpawnPrefabId = null, string deathSpawnPrefabId = null, bool hasSelfHeal = false,
                bool isAerial = false, MobTargetLayer targetLayer = MobTargetLayer.Any,
                string attackStatusId = null, string auraStatusId = null,
                string auraRangeParameter = null)
            {
                ObjectName = objectName; Archetype = archetype;
                PathSpawnPrefabId = pathSpawnPrefabId; DeathSpawnPrefabId = deathSpawnPrefabId;
                HasSelfHeal = hasSelfHeal; IsAerial = isAerial; TargetLayer = targetLayer;
                AttackStatusId = attackStatusId;
                AuraStatusId = auraStatusId; AuraRangeParameter = auraRangeParameter;
            }
        }
    }
}
