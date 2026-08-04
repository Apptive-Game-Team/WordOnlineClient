using System.Collections.Generic;
using FixMath.NET;
using GameScene.Simulation.Mob;
using NUnit.Framework;

namespace GameScene.Simulation.Tests
{
    public sealed class ProductionMobDefinitionResolverTests
    {
        private static readonly string[] Slimes =
        {
            "ember_spirit", "seed_spirit", "water_slime", "fire_slime",
            "electric_slime", "rock_slime", "wind_slime", "mini_rock", "leaf_slime"
        };
        private static readonly string[] Melee = { "rock_golem", "tree_golem" };
        private static readonly string[] Ranged =
            { "aqua_archer", "thunder_spirit", "cloud_dragon", "vine_spirit", "fire_child_spirit" };

        [Test]
        public void ResolvesServerArchetypeAndVersionedParameters()
        {
            ProductionMobDefinitionResolver resolver = new ProductionMobDefinitionResolver(Parameters());

            Assert.That(resolver.TryResolve("FireSlime", out MobDefinition slime), Is.True);
            Assert.That(slime.Archetype, Is.EqualTo(MobArchetype.Slime));
            Assert.That(slime.MaxHealth, Is.EqualTo(30));
            Assert.That(slime.Speed, Is.EqualTo((Fix64)2));
            Assert.That(slime.AttackRange, Is.EqualTo((Fix64)1.25m));
            Assert.That(slime.AttackCooldownFrames, Is.EqualTo(21));
            Assert.That(slime.TargetLayer, Is.EqualTo(MobTargetLayer.Ground));

            Assert.That(resolver.TryResolve("AquaArcher", out MobDefinition ranged), Is.True);
            Assert.That(ranged.Archetype, Is.EqualTo(MobArchetype.ProjectileRange));
            Assert.That(ranged.AttackRange, Is.EqualTo((Fix64)7));
            Assert.That(ranged.TargetLayer, Is.EqualTo(MobTargetLayer.Any));
            Assert.That(resolver.TryResolve("LifeTree", out MobDefinition totem), Is.True);
            Assert.That(totem.Archetype, Is.EqualTo(MobArchetype.Totem));
            Assert.That(totem.AreaHealthDelta, Is.EqualTo(-3));
            Assert.That(totem.AreaEffectIntervalFrames, Is.EqualTo(20));
            Assert.That(resolver.TryResolve("Crater", out MobDefinition crater), Is.True);
            Assert.That(crater.Archetype, Is.EqualTo(MobArchetype.Dummy));
            Assert.That(crater.LifetimeFrames, Is.EqualTo(160));
            Assert.That(crater.PeriodicSpawnPrefabId, Is.EqualTo("CraterEmber"));
            Assert.That(crater.PeriodicSpawnIntervalFrames, Is.EqualTo(20));
            Assert.That(resolver.TryResolve("WindTotem", out MobDefinition windTotem), Is.True);
            Assert.That(windTotem.PushForce, Is.EqualTo((Fix64)1.5m));
            Assert.That(windTotem.PushRangeX, Is.EqualTo((Fix64)4));
            Assert.That(resolver.TryResolve("ChickenCommando", out MobDefinition chicken), Is.True);
            Assert.That(chicken.TargetLayer, Is.EqualTo(MobTargetLayer.Ground));
            Assert.That(chicken.SpawnDelayFrames, Is.GreaterThan(0));
            Assert.That(chicken.InitialHeight, Is.EqualTo((Fix64)10));
            Assert.That(chicken.Gravity, Is.EqualTo((Fix64)3));
            Assert.That(resolver.TryResolve("GroundTower", out MobDefinition tower), Is.True);
            Assert.That(tower.Archetype, Is.EqualTo(MobArchetype.Tower));
            Assert.That(tower.AttackDelayFrames, Is.EqualTo(4));
            Assert.That(tower.SplashRadius, Is.EqualTo(Fix64.One));
            Assert.That(tower.TargetLayer, Is.EqualTo(MobTargetLayer.Air));
            Assert.That(resolver.TryResolve("ElectricTower", out MobDefinition lightningTower), Is.True);
            Assert.That(lightningTower.Archetype, Is.EqualTo(MobArchetype.LightningTower));
            Assert.That(lightningTower.ChainCount, Is.EqualTo(2));
            Assert.That(resolver.TryResolve("MagmaSpirit", out MobDefinition summoner), Is.True);
            Assert.That(summoner.Archetype, Is.EqualTo(MobArchetype.Summoner));
            Assert.That(summoner.SummonPrefabId, Is.EqualTo("MagmaFist"));
            Assert.That(summoner.AuraStatusId, Is.EqualTo("burn"));
            Assert.That(summoner.TargetLayer, Is.EqualTo(MobTargetLayer.Ground));
            Assert.That(resolver.TryResolve("WindSpirit", out MobDefinition selfDestruct), Is.True);
            Assert.That(selfDestruct.Archetype, Is.EqualTo(MobArchetype.SelfDestruct));
            Assert.That(selfDestruct.ExplosionRadius, Is.EqualTo((Fix64)7));
            Assert.That(resolver.TryResolve("FireLordSpirit", out MobDefinition keepDistance), Is.True);
            Assert.That(keepDistance.Archetype, Is.EqualTo(MobArchetype.KeepDistance));
            Assert.That(keepDistance.PeriodicSpawnLimit, Is.EqualTo(5));
            Assert.That(resolver.TryResolve("StormRider", out MobDefinition playerPriority), Is.True);
            Assert.That(playerPriority.Archetype, Is.EqualTo(MobArchetype.PlayerPriorityMelee));
            Assert.That(playerPriority.TargetLayer, Is.EqualTo(MobTargetLayer.Ground));
            Assert.That(resolver.TryResolve("RockMage", out MobDefinition doubleTarget), Is.True);
            Assert.That(doubleTarget.Archetype, Is.EqualTo(MobArchetype.DoubleTarget));
            Assert.That(doubleTarget.AttackRange, Is.EqualTo((Fix64)7));
            Assert.That(resolver.TryResolve("FireSpirit", out MobDefinition spraying), Is.True);
            Assert.That(spraying.Archetype, Is.EqualTo(MobArchetype.Spraying));
            Assert.That(spraying.AttackRange, Is.EqualTo((Fix64)7));
            Assert.That(spraying.AuraStatusId, Is.EqualTo("burn"));
            Assert.That(spraying.TargetLayer, Is.EqualTo(MobTargetLayer.Ground));
            Assert.That(resolver.TryResolve("ZapMouse", out MobDefinition coward), Is.True);
            Assert.That(coward.Archetype, Is.EqualTo(MobArchetype.Coward));
            Assert.That(coward.PanicDurationFrames, Is.EqualTo(40));
            Assert.That(resolver.TryResolve("DimensionToad", out MobDefinition nonAttacking), Is.True);
            Assert.That(nonAttacking.Archetype, Is.EqualTo(MobArchetype.NonAttackingCoward));
            Assert.That(nonAttacking.Damage, Is.Zero);
            Assert.That(nonAttacking.PeriodicSpawnLimit, Is.Zero);
            Assert.That(nonAttacking.SecondaryPeriodicSpawnPrefabId, Is.EqualTo("LightningTadpole"));
            Assert.That(resolver.TryResolve("ThunderBird", out MobDefinition thunderBird), Is.True);
            Assert.That(thunderBird.Archetype, Is.EqualTo(MobArchetype.ThunderBird));
            Assert.That(thunderBird.AttackRange, Is.EqualTo((Fix64)7));
            Assert.That(resolver.TryResolve("FireTadpole", out MobDefinition fireTadpole), Is.True);
            Assert.That(fireTadpole.Archetype, Is.EqualTo(MobArchetype.Melee));
            Assert.That(fireTadpole.LifetimeFrames, Is.EqualTo(120));
            Assert.That(resolver.TryResolve("LightningTadpole", out MobDefinition lightningTadpole), Is.True);
            Assert.That(lightningTadpole.LifetimeFrames, Is.EqualTo(120));
            Assert.That(resolver.TryResolve("BubbleSpirit", out MobDefinition bubble), Is.True);
            Assert.That(bubble.Archetype, Is.EqualTo(MobArchetype.BubbleSplash));
            Assert.That(bubble.IsAerial, Is.True);
            Assert.That(bubble.TargetLayer, Is.EqualTo(MobTargetLayer.Air));
            Assert.That(resolver.TryResolve("ManaWell", out MobDefinition manaWell), Is.True);
            Assert.That(manaWell.ManaChargePercent, Is.EqualTo((Fix64)0.8m));
            Assert.That(manaWell.LifetimeFrames, Is.EqualTo(200));
            Assert.That(resolver.TryResolve("BubbleGenerator", out MobDefinition bubbleGenerator), Is.True);
            Assert.That(bubbleGenerator.Archetype, Is.EqualTo(MobArchetype.BubbleGenerator));
            Assert.That(bubbleGenerator.LifetimeFrames, Is.EqualTo(240));
            Assert.That(resolver.TryResolve("GroundCannon", out MobDefinition cannon), Is.True);
            Assert.That(cannon.AttackDelayFrames, Is.EqualTo(10));
            Assert.That(cannon.LifetimeFrames, Is.EqualTo(1200));
            Assert.That(resolver.TryResolve("VineColony", out MobDefinition colony), Is.True);
            Assert.That(colony.Archetype, Is.EqualTo(MobArchetype.StationarySummoner));
            Assert.That(colony.SummonPrefabId, Is.EqualTo("Vine"));
            Assert.That(resolver.TryResolve("Towerback", out MobDefinition towerback), Is.True);
            Assert.That(towerback.Archetype, Is.EqualTo(MobArchetype.DualAttack));
            Assert.That(towerback.SecondaryDamage, Is.EqualTo(5));
            Assert.That(resolver.TryResolve("PveNatureSlimeNest", out MobDefinition natureBoss), Is.True);
            Assert.That(natureBoss.Archetype, Is.EqualTo(MobArchetype.PveBoss));
            Assert.That(natureBoss.PeriodicSpawnPrefabId, Is.EqualTo("LeafSlime"));
            Assert.That(natureBoss.PeriodicSpawnIntervalFrames, Is.EqualTo(201));
            Assert.That(resolver.TryResolve("PveVineWitch", out MobDefinition vineWitch), Is.True);
            Assert.That(vineWitch.SelectSinglePeriodicPrefab, Is.True);
            Assert.That(vineWitch.PeriodicSpawnBatchCount, Is.EqualTo(2));
            Assert.That(vineWitch.PrimaryBossMagicName, Is.EqualTo("vine_toss"));
            Assert.That(vineWitch.SecondaryBossMagicName, Is.EqualTo("vine_colony"));
            Assert.That(vineWitch.RageBossMagicName, Is.EqualTo("vine_fan"));
            AssertDestructibleSpawner(resolver, "FireSummon", "FireSlime");
            AssertDestructibleSpawner(resolver, "ElectricSummon", "ElectricSlime");
            AssertDestructibleSpawner(resolver, "RockSummon", "RockSlime");
            AssertDestructibleSpawner(resolver, "WindSummon", "WindSlime");
        }

        [Test]
        public void MissingRequiredServerParameterFailsInsteadOfUsingFallback()
        {
            List<MobParameterValue> parameters = Parameters();
            parameters.RemoveAll(value => value.ObjectName == "fire_slime" && value.ParameterName == "hp");

            Assert.Throws<System.InvalidOperationException>(() =>
                new ProductionMobDefinitionResolver(parameters));
        }

        [Test]
        public void ResolvesEveryServerPrefabThatOwnsDeterministicMobBehavior()
        {
            string[] prefabIds =
            {
                "AquaArcher", "BubbleGenerator", "BubbleSpirit", "ChickenCommando",
                "CloudDragon", "Crater", "DimensionToad", "ElectricSlime", "ElectricTower",
                "EmberSpirit", "FireChildSpirit", "FireLordSpirit", "FireSlime", "FireSpirit",
                "FireSummon", "FireTadpole", "GroundCannon", "GroundTower", "HealingTotem",
                "LeafSlime", "LifeTree", "LightningTadpole", "MagmaSpirit", "ManaWell",
                "MiniRock", "PveNatureSlimeNest", "PveVineColony", "PveVineWitch",
                "PveWaterSlimeNest", "RockGolem", "RockMage", "RockSlime", "RockSummon",
                "RockTurret", "SeedSpirit", "StormRider", "ThunderBird", "ThunderSpirit",
                "Towerback", "TreeGolem", "VineColony", "VineSpirit", "WaterSlime",
                "WindSlime", "WindSpirit", "WindSummon", "WindTotem", "ZapMouse",
                "ElectricSummon"
            };
            ProductionMobDefinitionResolver resolver =
                new ProductionMobDefinitionResolver(Parameters());

            Assert.That(prefabIds, Has.Length.EqualTo(49));
            for (int index = 0; index < prefabIds.Length; index++)
                Assert.That(resolver.TryResolve(prefabIds[index], out _), Is.True,
                    prefabIds[index]);
            Assert.That(resolver.TryResolve("WaterShot", out _), Is.False);
        }

        internal static List<MobParameterValue> Parameters()
        {
            List<MobParameterValue> values = new List<MobParameterValue>();
            for (int index = 0; index < Slimes.Length; index++) Add(values, Slimes[index], false);
            for (int index = 0; index < Melee.Length; index++) Add(values, Melee[index], false);
            for (int index = 0; index < Ranged.Length; index++) Add(values, Ranged[index], true);
            AddStationary(values, "crater", 8);
            values.Add(Value("crater", "attack_interval", 1));
            AddStationary(values, "wind_totem", 8);
            values.Add(Value("wind_totem", "attack_interval", 1));
            values.Add(Value("wind_totem", "damage", 2));
            values.Add(Value("wind_totem", "push_force", 1.5f));
            values.Add(Value("wind_totem", "push_range_x", 4));
            values.Add(Value("wind_totem", "push_range_y", 3));
            Add(values, "chicken_commando", false);
            values.Add(Value("chicken_commando", "spawn_height", 10));
            values.Add(Value("chicken_commando", "fall_gravity", 3));
            AddTotem(values, "life_tree");
            AddTotem(values, "healing_totem");
            AddStationaryAttacker(values, "rock_turret", false);
            AddStationaryAttacker(values, "ground_tower", true);
            AddLightningTower(values);
            AddSpecial(values, "magma_spirit");
            values.Add(Value("magma_spirit", "sub_attack_range", 2));
            AddSpecial(values, "wind_spirit");
            AddKeepDistance(values);
            Add(values, "storm_rider", false);
            Add(values, "rock_mage", true);
            Add(values, "fire_spirit", true);
            values.Add(Value("fire_spirit", "sub_attack_range", 2));
            AddCoward(values, "zap_mouse", true);
            AddCoward(values, "dimension_toad", false);
            Add(values, "thunder_bird", true);
            AddTimedMelee(values, "fire_tadpole");
            AddTimedMelee(values, "lightning_tadpole");
            AddBubble(values);
            AddManaWell(values);
            AddBubbleGenerator(values);
            AddStationaryAttacker(values, "ground_cannon", false);
            AddVineColony(values);
            AddTowerback(values);
            AddPveBoss(values, "pve_nature_slime_nest");
            AddPveBoss(values, "pve_water_slime_nest");
            AddPveBoss(values, "pve_vine_witch");
            AddSpawner(values, "fire_summon");
            AddSpawner(values, "electric_summon");
            AddSpawner(values, "rock_summon");
            AddSpawner(values, "wind_summon");
            return values;
        }

        private static void AssertDestructibleSpawner(ProductionMobDefinitionResolver resolver,
            string prefabId, string spawnPrefabId)
        {
            Assert.That(resolver.TryResolve(prefabId, out MobDefinition definition), Is.True);
            Assert.That(definition.Archetype, Is.EqualTo(MobArchetype.Dummy));
            Assert.That(definition.PeriodicSpawnPrefabId, Is.EqualTo(spawnPrefabId));
            Assert.That(definition.PeriodicSpawnIntervalFrames, Is.EqualTo(41));
            Assert.That(definition.PeriodicSpawnLimit, Is.Zero);
            Assert.That(definition.PeriodicSelfDamage, Is.EqualTo(1));
        }

        private static void Add(List<MobParameterValue> values, string objectName, bool ranged)
        {
            values.Add(Value(objectName, "radius", 0.75f));
            values.Add(Value(objectName, "speed", 2));
            values.Add(Value(objectName, "damage", 4));
            values.Add(Value(objectName, "attack_interval", 1));
            values.Add(Value(objectName, "hp", 30));
            if (ranged) values.Add(Value(objectName, "attack_range", 7));
            if (objectName == "tree_golem")
            {
                values.Add(Value(objectName, "heal_amount", 3));
                values.Add(Value(objectName, "heal_interval", 2));
            }
        }

        private static MobParameterValue Value(string objectName, string parameterName, float value) =>
            new MobParameterValue(objectName, parameterName, (Fix64)value);

        private static void AddStationary(List<MobParameterValue> values, string objectName, float duration)
        {
            values.Add(Value(objectName, "radius", 0.75f));
            values.Add(Value(objectName, "hp", 20));
            if (objectName == "crater") values.Add(Value(objectName, "duration", duration));
        }

        private static void AddTotem(List<MobParameterValue> values, string objectName)
        {
            values.Add(Value(objectName, "radius", 0.75f));
            values.Add(Value(objectName, "hp", 20));
            values.Add(Value(objectName, "damage", -3));
            values.Add(Value(objectName, "range", 4));
            values.Add(Value(objectName, "attack_interval", 1));
            values.Add(Value(objectName, "duration", 8));
        }

        private static void AddStationaryAttacker(List<MobParameterValue> values,
            string objectName, bool duration)
        {
            values.Add(Value(objectName, "radius", 0.75f));
            values.Add(Value(objectName, "hp", 20));
            values.Add(Value(objectName, "damage", 5));
            values.Add(Value(objectName, "attack_range", 6));
            values.Add(Value(objectName, "attack_interval", 1));
            if (duration) values.Add(Value(objectName, "duration", 8));
        }

        private static void AddLightningTower(List<MobParameterValue> values)
        {
            AddStationaryAttacker(values, "electric_tower", true);
            values.Add(Value("electric_tower", "chain_damage", 3));
            values.Add(Value("electric_tower", "chain_count", 2));
            values.Add(Value("electric_tower", "chain_radius", 2));
        }

        private static void AddSpecial(List<MobParameterValue> values, string objectName)
        {
            values.Add(Value(objectName, "radius", 0.75f));
            values.Add(Value(objectName, "speed", 2));
            values.Add(Value(objectName, "damage", 6));
            values.Add(Value(objectName, "attack_interval", 1));
            values.Add(Value(objectName, "attack_range", 7));
            values.Add(Value(objectName, "hp", 30));
        }

        private static void AddKeepDistance(List<MobParameterValue> values)
        {
            values.Add(Value("fire_lord_spirit", "radius", 0.75f));
            values.Add(Value("fire_lord_spirit", "speed", 2));
            values.Add(Value("fire_lord_spirit", "hp", 30));
        }

        private static void AddCoward(List<MobParameterValue> values, string objectName, bool damage)
        {
            values.Add(Value(objectName, "radius", 0.75f));
            values.Add(Value(objectName, "speed", 2));
            values.Add(Value(objectName, "attack_interval", 1));
            values.Add(Value(objectName, "detection_range", 3));
            values.Add(Value(objectName, "panic_duration", 2));
            values.Add(Value(objectName, "hp", 30));
            if (damage) values.Add(Value(objectName, "damage", 4));
        }

        private static void AddTimedMelee(List<MobParameterValue> values, string objectName)
        {
            Add(values, objectName, false);
            values.Add(Value(objectName, "duration", 6));
        }

        private static void AddBubble(List<MobParameterValue> values)
        {
            Add(values, "bubble_spirit", true);
            values.Add(Value("bubble_spirit", "projectile_speed", 8));
        }

        private static void AddManaWell(List<MobParameterValue> values)
        {
            values.Add(Value("mana_well", "radius", 0.75f));
            values.Add(Value("mana_well", "hp", 20));
            values.Add(Value("mana_well", "attack_interval", 1));
            values.Add(Value("mana_well", "damage", 2));
        }

        private static void AddBubbleGenerator(List<MobParameterValue> values)
        {
            values.Add(Value("bubble_generator", "radius", 0.65f));
            values.Add(Value("bubble_generator", "hp", 18));
            values.Add(Value("bubble_generator", "attack_range", 4));
            values.Add(Value("bubble_generator", "attack_interval", 1.2f));
            values.Add(Value("bubble_generator", "duration", 12));
        }

        private static void AddVineColony(List<MobParameterValue> values)
        {
            AddStationaryAttacker(values, "vine_colony", false);
            values.Add(Value("vine_colony", "duration", 10));
        }

        private static void AddTowerback(List<MobParameterValue> values)
        {
            AddStationaryAttacker(values, "towerback", false);
            values.Add(Value("towerback", "sub_speed", 1));
            values.Add(Value("towerback", "sub_damage", 3));
            values.Add(Value("towerback", "sub_attack_range", 1));
        }

        private static void AddPveBoss(List<MobParameterValue> values, string objectName)
        {
            values.Add(Value(objectName, "radius", 1));
            values.Add(Value(objectName, "hp", 200));
        }

        private static void AddSpawner(List<MobParameterValue> values, string objectName)
        {
            values.Add(Value(objectName, "radius", 0.75f));
            values.Add(Value(objectName, "hp", 20));
        }
    }
}
