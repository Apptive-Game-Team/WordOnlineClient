using System;
using System.Collections.Generic;
using System.Linq;
using FixMath.NET;
using GameScene.Simulation.Core;
using GameScene.Simulation.Magic;
using GameScene.Simulation.Objects;
using GameScene.Simulation.Physics;
using NUnit.Framework;

namespace GameScene.Simulation.Tests
{
    public sealed class MagicSimulationTests
    {
        [Test]
        public void ProductionCatalogContainsEveryCanonicalMagicExactlyOnce()
        {
            ProductionMagicCatalog catalog = ProductionMagicCatalog.Create();
            int[] ids = catalog.Definitions.Select(value => value.Id).OrderBy(value => value).ToArray();
            int[] expected = Enumerable.Range(1, 67).ToArray();

            Assert.That(ids, Is.EqualTo(expected));
            Assert.That(catalog.Definitions.Select(value => value.Name).Distinct().Count(), Is.EqualTo(67));

            SimulationPrefabRegistry prefabs = SimulationPrefabRegistry.CreateProduction();
            Assert.That(catalog.Definitions.All(value => prefabs.GetRequired(value.PrefabId) != null), Is.True);
        }

        [TestCase("chain_lightning", MagicExecutionKind.ChainShot)]
        [TestCase("rock_rolling", MagicExecutionKind.RollingRock)]
        [TestCase("tide_call", MagicExecutionKind.PushShot)]
        [TestCase("will_o_wisp", MagicExecutionKind.MindControlShot)]
        [TestCase("wind_blade", MagicExecutionKind.PiercingShot)]
        [TestCase("overgrowth", MagicExecutionKind.Overgrowth)]
        [TestCase("razor_gale", MagicExecutionKind.PeriodicArea)]
        [TestCase("sand_storm", MagicExecutionKind.PersistentArea)]
        [TestCase("tornado_strike", MagicExecutionKind.MovingTornado)]
        public void SpecialMagicUsesServerComponentExecutionKind(string magicName,
            MagicExecutionKind expected)
        {
            Assert.That(ProductionMagicCatalog.Create().GetRequired(magicName).ExecutionKind,
                Is.EqualTo(expected));
        }

        [Test]
        public void ProductionPrefabManifestContainsEveryServerPrefabTypeExactlyOnce()
        {
            Assert.That(ServerPrefabManifest.All.Count, Is.EqualTo(96));
            Assert.That(ServerPrefabManifest.All.Distinct().Count(), Is.EqualTo(96));
            SimulationPrefabRegistry prefabs = SimulationPrefabRegistry.CreateProduction();
            Assert.That(ServerPrefabManifest.All.All(value => prefabs.GetRequired(value) != null), Is.True);
        }

        [Test]
        public void VersionedPrefabRegistryUsesServerRadiusMassAndTriggerContract()
        {
            SimulationPrefabRegistry prefabs = SimulationPrefabRegistry.CreateProduction(new[]
            {
                new SimulationPrefabParameterValue("player", "radius", (Fix64)0.7m),
                new SimulationPrefabParameterValue("player", "mass", (Fix64)3),
                new SimulationPrefabParameterValue("fire_shot", "radius", (Fix64)0.2m)
            });
            SimulationWorld world = new SimulationWorld(1, prefabs);
            SimulationEntity player = world.Spawn("Player", 10, new SimVector2((Fix64)2, (Fix64)2));
            SimulationEntity shot = world.Spawn("FireShot", 10, new SimVector2((Fix64)4, (Fix64)2));

            Assert.That(player.Radius, Is.EqualTo(Fix64.One),
                "server player collider is a fixed radius independent of parameter rows");
            Assert.That(player.Mass, Is.EqualTo((Fix64)3));
            Assert.That(player.IsTrigger, Is.False);
            Assert.That(shot.Radius, Is.EqualTo((Fix64)0.2m));
            Assert.That(shot.IsTrigger, Is.True);
        }

        [Test]
        public void ServerComponentManifestContainsCurrentSourceInventoryExactlyOnce()
        {
            Assert.That(ServerComponentManifest.All.Count, Is.EqualTo(120));
            Assert.That(ServerComponentManifest.All.Distinct().Count(), Is.EqualTo(120));
            Assert.That(ServerComponentManifest.Implemented.All(ServerComponentManifest.All.Contains), Is.True);
            Assert.That(ServerComponentManifest.Implemented.Count, Is.EqualTo(120));
        }

        [Test]
        public void ProductionEffectReceiversMatchServerPrefabComponents()
        {
            SimulationWorld world = new SimulationWorld(1, SimulationPrefabRegistry.CreateProduction());
            SimulationEntity common = world.Spawn("FireSlime", 20, new SimVector2((Fix64)4, (Fix64)5));
            SimulationEntity building = world.Spawn("Crater", 20, new SimVector2((Fix64)5, (Fix64)5));
            SimulationEntity none = world.Spawn("PveVineWitch", 20, new SimVector2((Fix64)6, (Fix64)5));
            MagicSimulation simulation = new MagicSimulation(2, world, ProductionMagicCatalog.Create());
            simulation.RegisterCombatant(common.Id);
            simulation.RegisterCombatant(building.Id);
            simulation.RegisterCombatant(none.Id);

            simulation.ApplyExternalStatus(common.Id, "wet", 60);
            simulation.ApplyExternalStatus(building.Id, "snared", 60);
            simulation.ApplyExternalStatus(building.Id, "burn", 60);
            simulation.ApplyExternalStatus(none.Id, "burn", 60);
            simulation.ApplyExternalStatus(none.Id, "shock-overload", 160);

            Assert.That(simulation.Combatants[common.Id].Statuses.ContainsKey("wet"), Is.True);
            Assert.That(simulation.Combatants[building.Id].Statuses.ContainsKey("building-snare-dot"), Is.True);
            Assert.That(simulation.Combatants[building.Id].Statuses.ContainsKey("burn-dot"), Is.False);
            Assert.That(simulation.Combatants[none.Id].Statuses.Keys,
                Is.EqualTo(new[] { "shock-overload" }));
            for (int frame = 0; frame < 60; frame++)
                simulation.Step(Array.Empty<MagicCastCommand>());
            Assert.That(simulation.Combatants[building.Id].Health, Is.EqualTo(100));
        }

        [Test]
        public void ProductionBehaviorMobMarkersMatchRallyingTorchServerTypeBoundary()
        {
            SimulationPrefabRegistry prefabs = SimulationPrefabRegistry.CreateProduction();
            string[] behaviorMobs =
            {
                "FireSlime", "EmberSpirit", "SeedSpirit", "RockGolem", "AquaArcher",
                "FireSpirit", "ThunderBird", "DimensionToad", "PveVineWitch"
            };
            string[] nonBehaviorMobs =
            {
                "Player", "GroundCannon", "ManaWell", "HealingTotem", "LifeTree",
                "RockTurret", "WindTotem", "GroundTower", "ElectricTower",
                "FireLordSpirit", "Crater", "BubbleGenerator"
            };

            Assert.That(behaviorMobs.All(prefabId => prefabs.GetRequired(prefabId)
                .ComponentIds.Contains(SimulationPrefabRegistry.BehaviorMobComponentId)), Is.True);
            Assert.That(nonBehaviorMobs.All(prefabId => !prefabs.GetRequired(prefabId)
                .ComponentIds.Contains(SimulationPrefabRegistry.BehaviorMobComponentId)), Is.True);
            Assert.That(prefabs.GetRequired("FireSlime").ComponentIds,
                Does.Contain(SimulationPrefabRegistry.RigidBodyComponentId)
                    .And.Contain(SimulationPrefabRegistry.ZPhysicsComponentId));
            Assert.That(prefabs.GetRequired("Crater").ComponentIds,
                Does.Contain(SimulationPrefabRegistry.RigidBodyComponentId));
            Assert.That(prefabs.GetRequired("Crater").ComponentIds,
                Does.Not.Contain(SimulationPrefabRegistry.ZPhysicsComponentId));
            Assert.That(prefabs.GetRequired("Player").ComponentIds,
                Does.Not.Contain(SimulationPrefabRegistry.RigidBodyComponentId));
            Assert.That(prefabs.GetRequired("Player").ComponentIds,
                Does.Not.Contain(SimulationPrefabRegistry.ZPhysicsComponentId));
        }

        [TestCase("ember_spirit_swarm", "EmberSpirit")]
        [TestCase("water_slime_swarm", "WaterSlime")]
        [TestCase("mini_rock_swarm", "MiniRock")]
        [TestCase("seed_spirit_swarm", "SeedSpirit")]
        [TestCase("thunder_bird_swarm", "ThunderBird")]
        [TestCase("fire_lord_spirit", "FireLordSpirit")]
        [TestCase("dimension_toad", "DimensionToad")]
        public void SummonMagicUsesServerRootPrefab(string magicName, string expectedPrefabId)
        {
            MagicSimulation simulation = Create();

            simulation.Step(new[] { Cast(10, 1, magicName, 5, 5) });

            Assert.That(simulation.World.Entities[2].PrefabId, Is.EqualTo(expectedPrefabId));
        }

        [Test]
        public void SpawnEventsContainStableEntityAndDefinitionAndResetEachFrame()
        {
            MagicSimulation simulation = Create();

            simulation.Step(new[] { Cast(10, 1, "fire_spirit", 5, 5) });

            Assert.That(simulation.SpawnedThisFrame.Count, Is.EqualTo(1));
            Assert.That(simulation.SpawnedThisFrame[0].EntityId, Is.EqualTo(2));
            Assert.That(simulation.SpawnedThisFrame[0].OwnerUserId, Is.EqualTo(10));
            Assert.That(simulation.SpawnedThisFrame[0].Definition.Name, Is.EqualTo("fire_spirit"));

            simulation.Step(Array.Empty<MagicCastCommand>());
            Assert.That(simulation.SpawnedThisFrame, Is.Empty);
        }

        [Test]
        public void SpawnFamilyUsesStablePerCastRandomStream()
        {
            MagicSimulation ascending = Create();
            MagicSimulation descending = Create();
            MagicCastCommand left = Cast(10, 4, "ember_spirit_swarm", 5, 5);
            MagicCastCommand right = Cast(20, 2, "ember_spirit_swarm", 7, 5);

            ascending.Step(new[] { left, right });
            descending.Step(new[] { right, left });

            Assert.That(descending.CalculateStateHash(), Is.EqualTo(ascending.CalculateStateHash()));
            Assert.That(ascending.World.Entities.Count, Is.EqualTo(8));
        }

        [Test]
        public void AreaFamilyAppliesDamageAndTimedStatusInEntityOrder()
        {
            MagicSimulation simulation = Create();

            simulation.Step(new[] { Cast(10, 1, "magma_explosion", 7, 5) });
            Assert.That(simulation.Combatants[1].Health, Is.EqualTo(100));
            for (int frame = 1; frame < 11; frame++)
                simulation.Step(Array.Empty<MagicCastCommand>());

            Assert.That(simulation.Combatants[1].Health, Is.EqualTo(90));
            Assert.That(simulation.Combatants[1].Statuses["burn"], Is.EqualTo(49),
                "collision effect starts while the delayed explosion is arming");
            Assert.That(simulation.Combatants[0].Health, Is.EqualTo(100));
        }

        [Test]
        public void BubbleShieldConsumesFirstPositiveMagicDamage()
        {
            MagicSimulation simulation = Create();
            simulation.ResolveBubbleShield(1, 160);

            simulation.Step(new[] { Cast(10, 1, "magma_explosion", 7, 5) });
            for (int frame = 1; frame < 11; frame++)
                simulation.Step(Array.Empty<MagicCastCommand>());

            Assert.That(simulation.Combatants[1].Health, Is.EqualTo(100));
            Assert.That(simulation.Combatants[1].BubbleShieldFrames, Is.Zero);
        }

        [Test]
        public void BurnAndWetCancelWhileBurnDotKeepsServerLifetime()
        {
            SimulationWorld world = new SimulationWorld(123, SimulationPrefabRegistry.CreateProduction());
            world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 10,
                new SimVector2((Fix64)2, (Fix64)5));
            world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 20,
                new SimVector2((Fix64)7, (Fix64)5));
            ProductionMagicCatalog catalog = new ProductionMagicCatalog(new[]
            {
                new MagicDefinition(1, "test_fire", "MagmaExplosion", MagicBehaviorFamily.Area,
                    MagicExecutionKind.Explosion, 1, 1, 0, (Fix64)6, (Fix64)2, Fix64.Zero,
                    "burn", 60),
                new MagicDefinition(2, "test_water", "WaterExplosion", MagicBehaviorFamily.Area,
                    MagicExecutionKind.Explosion, 1, 1, 0, (Fix64)6, (Fix64)2, Fix64.Zero,
                    "wet", 60)
            });
            MagicSimulation simulation = new MagicSimulation(456, world, catalog);

            simulation.Step(new[] { Cast(10, 1, "test_fire", 7, 5) });
            simulation.Step(new[] { Cast(10, 2, "test_water", 7, 5) });

            Assert.That(simulation.Combatants[1].Statuses.ContainsKey("burn"), Is.False);
            Assert.That(simulation.Combatants[1].Statuses.ContainsKey("wet"), Is.False);
            Assert.That(simulation.Combatants[1].Statuses.ContainsKey("burn-dot"), Is.True);
            for (int frame = 0; frame < 18; frame++)
                simulation.Step(Array.Empty<MagicCastCommand>());
            Assert.That(simulation.Combatants[1].Health, Is.EqualTo(99));
        }

        [Test]
        public void BossCastUsesExplicitCasterEntityWithoutPlayerObject()
        {
            SimulationWorld world = new SimulationWorld(321, SimulationPrefabRegistry.CreateProduction());
            SimulationEntity boss = world.Spawn("PveVineWitch", -1,
                new SimVector2((Fix64)5, (Fix64)5));
            MagicSimulation simulation = new MagicSimulation(654, world, ProductionMagicCatalog.Create());

            simulation.Step(new[]
            {
                new MagicCastCommand(-1, int.MinValue, "vine_toss",
                    new SimVector2((Fix64)8, (Fix64)5), boss.Id)
            });

            Assert.That(world.Entities[1].PrefabId, Is.EqualTo("Vine"));
            Assert.That(world.Entities[1].OwnerUserId, Is.EqualTo(-1));
        }

        [Test]
        public void VineTossSpawnsSixVinesOverDeterministicScheduleAndHitsTargetOnce()
        {
            MagicSimulation simulation = Create();

            simulation.Step(new[] { Cast(10, 1, "vine_toss", 8, 5) });
            Assert.That(simulation.World.Entities.Count, Is.EqualTo(3));
            for (int frame = 1; frame < 13; frame++)
                simulation.Step(Array.Empty<MagicCastCommand>());

            Assert.That(simulation.World.Entities.Count, Is.EqualTo(8));
            Assert.That(simulation.World.Entities.Skip(2).All(entity => entity.PrefabId == "Vine"), Is.True);
            Assert.That(simulation.Combatants[1].Health, Is.EqualTo(90));
            Assert.That(simulation.Combatants[1].Statuses.ContainsKey("snared"), Is.True);
        }

        [Test]
        public void VineFanSpawnsThreeIndependentSixVineLines()
        {
            MagicSimulation simulation = Create();

            simulation.Step(new[] { Cast(10, 1, "vine_fan", 8, 5) });
            for (int frame = 1; frame < 13; frame++)
                simulation.Step(Array.Empty<MagicCastCommand>());

            Assert.That(simulation.World.Entities.Count, Is.EqualTo(20));
        }

        [Test]
        public void FallingDropActivatesOnlyAfterDeterministicFallFrames()
        {
            MagicSimulation simulation = CreateWith(new MagicDefinition(
                1, "test_drop", "FireDrop", MagicBehaviorFamily.Drop,
                MagicExecutionKind.DropImpact, 1, 3, 10, (Fix64)6, (Fix64)2,
                Fix64.Zero, activationFrames: 3, effectRadius: (Fix64)2));

            simulation.Step(new[] { Cast(10, 1, "test_drop", 7, 5) });
            Assert.That(simulation.Combatants[1].Health, Is.EqualTo(100));
            Assert.That(simulation.World.Entities[2].Position3D.Z, Is.GreaterThan(Fix64.Zero));
            Assert.That(simulation.World.Entities[2].Position3D.Z, Is.LessThan((Fix64)10));
            simulation.Step(Array.Empty<MagicCastCommand>());

            Assert.That(simulation.Combatants[1].Health, Is.EqualTo(90));
            Assert.That(simulation.World.Entities[2].IsDestroyed, Is.True);
            Assert.That(simulation.World.Entities[2].Position3D.Z, Is.EqualTo(Fix64.Zero));
        }

        [Test]
        public void FallingDropWithoutPhysicalCollisionExpiresWithoutAreaDamage()
        {
            MagicSimulation simulation = CreateWith(new MagicDefinition(
                1, "test_drop", "FireDrop", MagicBehaviorFamily.Drop,
                MagicExecutionKind.DropImpact, 1, 3, 10, (Fix64)6, (Fix64)2,
                Fix64.Zero, activationFrames: 3, effectRadius: (Fix64)4));

            simulation.Step(new[] { Cast(10, 1, "test_drop", 5, 5) });
            simulation.Step(Array.Empty<MagicCastCommand>());
            simulation.Step(Array.Empty<MagicCastCommand>());

            Assert.That(simulation.Combatants[1].Health, Is.EqualTo(100));
            Assert.That(simulation.World.Entities[2].IsDestroyed, Is.True);
        }

        [Test]
        public void LightningColumnUsesServerVerticalBoxInsteadOfSphereDistance()
        {
            MagicSimulation simulation = CreateWith(new MagicDefinition(
                1, "test_area", "FireExplode", MagicBehaviorFamily.Area,
                MagicExecutionKind.LightningColumn, 1, 0, 10, (Fix64)6, (Fix64)2,
                Fix64.Zero));
            SimulationEntity aerial = simulation.World.Spawn("Player", 20,
                new SimVector3((Fix64)5, (Fix64)5, (Fix64)5));
            simulation.RegisterCombatant(aerial.Id, 100);

            simulation.Step(new[] { Cast(10, 1, "test_area", 5, 5) });

            Assert.That(simulation.Combatants[aerial.Id].Health, Is.EqualTo(90));
        }

        [Test]
        public void WaterExplosionAddsVerticalImpulseOnlyToServerZPhysicsPrefabs()
        {
            MagicDefinition definition = new MagicDefinition(
                1, "test_water", "WaterExplosion", MagicBehaviorFamily.Area,
                MagicExecutionKind.Explosion, 1, 2, 1, (Fix64)10, (Fix64)4,
                Fix64.Zero, verticalImpulse: (Fix64)5);
            SimulationWorld world = new SimulationWorld(123,
                SimulationPrefabRegistry.CreateProduction());
            SimulationEntity player = world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 10,
                new SimVector2((Fix64)2, (Fix64)5));
            SimulationEntity slime = world.Spawn("FireSlime", 20,
                new SimVector2((Fix64)5, (Fix64)5));
            SimulationEntity crater = world.Spawn("Crater", 20,
                new SimVector2((Fix64)7, (Fix64)5));
            MagicSimulation simulation = new MagicSimulation(1, world,
                new ProductionMagicCatalog(new[] { definition }));
            simulation.RegisterCombatant(player.Id, 100);
            simulation.RegisterCombatant(slime.Id, 100);
            simulation.RegisterCombatant(crater.Id, 100);

            simulation.Step(new[] { Cast(10, 1, "test_water", 5, 5) });
            simulation.Step(Array.Empty<MagicCastCommand>());
            simulation.Step(Array.Empty<MagicCastCommand>());

            Assert.That(slime.Position3D.Z, Is.GreaterThan(Fix64.Zero));
            Assert.That(player.Position3D.Z, Is.EqualTo(Fix64.Zero));
            Assert.That(crater.Position3D.Z, Is.EqualTo(Fix64.Zero));
        }

        [Test]
        public void MagicDamageUsesNativeAndTemporaryWetElements()
        {
            SimulationPrefabRegistry prefabs = new SimulationPrefabRegistry(new[]
            {
                new SimulationPrefabDefinition("Player", "Transform", "PhysicsBody"),
                new SimulationPrefabDefinition("NatureTarget",
                    new SimulationPhysicsDefinition((Fix64)0.5m, Fix64.One),
                    SimulationElement.Nature, "Transform", "PhysicsBody",
                    SimulationPrefabRegistry.CommonEffectReceiverComponentId),
                new SimulationPrefabDefinition("FireExplode", "Transform", "PhysicsBody")
            });
            SimulationWorld world = new SimulationWorld(123, prefabs);
            world.Spawn("Player", 10, new SimVector2((Fix64)2, (Fix64)5));
            SimulationEntity target = world.Spawn("NatureTarget", 20,
                new SimVector2((Fix64)5, (Fix64)5));
            MagicDefinition definition = new MagicDefinition(
                1, "test_fire", "FireExplode", MagicBehaviorFamily.Area,
                MagicExecutionKind.LightningColumn, 1, 0, 10, (Fix64)6, (Fix64)2,
                Fix64.Zero);
            MagicSimulation simulation = new MagicSimulation(1, world,
                new ProductionMagicCatalog(new[] { definition }));
            simulation.RegisterCombatant(0, 100);
            simulation.RegisterCombatant(target.Id, 100);

            simulation.Step(new[] { Cast(10, 1, "test_fire", 5, 5) });
            Assert.That(simulation.Combatants[target.Id].Health, Is.EqualTo(95));

            simulation.ApplyExternalStatus(target.Id, "wet", 60);
            simulation.Step(new[] { Cast(10, 2, "test_fire", 5, 5) });
            Assert.That(simulation.Combatants[target.Id].Health, Is.EqualTo(82));
        }

        [Test]
        public void LightningColumnHitsImmediatelyAndFrenzyAffectsBothTeamsAfterLanding()
        {
            MagicSimulation lightning = CreateWith(new MagicDefinition(
                1, "test_lightning", "LightningDrop", MagicBehaviorFamily.Drop,
                MagicExecutionKind.LightningColumn, 1, 2, 10, (Fix64)6, (Fix64)2,
                Fix64.Zero));
            lightning.Step(new[] { Cast(10, 1, "test_lightning", 7, 5) });
            Assert.That(lightning.Combatants[1].Health, Is.EqualTo(90));

            MagicSimulation frenzy = CreateWith(new MagicDefinition(
                1, "test_frenzy", "FrenzyTotem", MagicBehaviorFamily.Drop,
                MagicExecutionKind.FrenzyDrop, 1, 3, 0, (Fix64)6, (Fix64)1,
                Fix64.Zero, "frenzy", 200, 3, (Fix64)10));
            frenzy.Step(new[] { Cast(10, 1, "test_frenzy", 5, 5) });
            frenzy.Step(Array.Empty<MagicCastCommand>());
            frenzy.Step(Array.Empty<MagicCastCommand>());

            Assert.That(frenzy.Combatants[0].Statuses.ContainsKey("frenzy"), Is.True);
            Assert.That(frenzy.Combatants[1].Statuses.ContainsKey("frenzy"), Is.True);
        }

        [Test]
        public void MeteorShowerUsesSeededPeriodicSecondarySpawns()
        {
            MagicDefinition definition = new MagicDefinition(
                1, "test_meteor", "MeteorShower", MagicBehaviorFamily.Drop,
                MagicExecutionKind.MeteorShower, 1, 5, 0, (Fix64)6, Fix64.Zero,
                Fix64.Zero, effectRadius: (Fix64)2, effectFrames: 5,
                effectIntervalFrames: 2, secondaryPrefabId: "MeteorDrop");
            MagicSimulation first = CreateWith(definition);
            MagicSimulation second = CreateWith(definition);

            for (int frame = 0; frame < 5; frame++)
            {
                IReadOnlyList<MagicCastCommand> casts = frame == 0
                    ? new[] { Cast(10, 1, "test_meteor", 5, 5) }
                    : Array.Empty<MagicCastCommand>();
                first.Step(casts); second.Step(casts);
                Assert.That(second.CalculateStateHash(), Is.EqualTo(first.CalculateStateHash()));
            }
            Assert.That(first.World.Entities.Count, Is.EqualTo(5));
            Assert.That(first.World.Entities[3].PrefabId, Is.EqualTo("MeteorDrop"));
            Assert.That(first.World.Entities[4].PrefabId, Is.EqualTo("MeteorDrop"));
        }

        [Test]
        public void OvergrowthTransformsAlliedSeedsOrSpawnsFallbackFormation()
        {
            MagicDefinition definition = new MagicDefinition(
                1, "test_overgrowth", "Overgrowth", MagicBehaviorFamily.Area,
                MagicExecutionKind.Overgrowth, 1, 2, 0, (Fix64)6, (Fix64)2,
                Fix64.Zero, secondaryPrefabId: "SeedSpirit", secondarySpawnCount: 3);
            SimulationWorld world = new SimulationWorld(123, SimulationPrefabRegistry.CreateProduction());
            world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 10, new SimVector2((Fix64)2, (Fix64)5));
            world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 20, new SimVector2((Fix64)8, (Fix64)5));
            SimulationEntity seed = world.Spawn("SeedSpirit", 10, new SimVector2((Fix64)5, (Fix64)5));
            MagicSimulation transformed = new MagicSimulation(1, world,
                new ProductionMagicCatalog(new[] { definition }));

            transformed.Step(new[] { Cast(10, 1, "test_overgrowth", 5, 5) });
            transformed.Step(Array.Empty<MagicCastCommand>());

            Assert.That(seed.IsDestroyed, Is.True);
            Assert.That(world.Entities.Last().PrefabId, Is.EqualTo("TreeGolem"));

            MagicSimulation fallback = CreateWith(definition);
            fallback.Step(new[] { Cast(10, 1, "test_overgrowth", 5, 5) });
            fallback.Step(Array.Empty<MagicCastCommand>());
            Assert.That(fallback.World.Entities.Count(entity => entity.PrefabId == "SeedSpirit"), Is.EqualTo(3));
        }

        [Test]
        public void PeriodicAreaUsesConfiguredFrameCadence()
        {
            MagicSimulation simulation = CreateWith(new MagicDefinition(
                1, "test_periodic", "RazorGale", MagicBehaviorFamily.Area,
                MagicExecutionKind.PeriodicArea, 1, 5, 3, (Fix64)6, (Fix64)2,
                Fix64.Zero, effectFrames: 5, effectIntervalFrames: 2));

            for (int frame = 0; frame < 5; frame++)
                simulation.Step(frame == 0
                    ? new[] { Cast(10, 1, "test_periodic", 7, 5) }
                    : Array.Empty<MagicCastCommand>());

            Assert.That(simulation.Combatants[1].Health, Is.EqualTo(94));
        }

        [Test]
        public void TornadoCapturesBothTeamsUsingServerOrbitAndVictimScaledDamage()
        {
            MagicDefinition definition = new MagicDefinition(
                1, "test_tornado", "TornadoStrike", MagicBehaviorFamily.Area,
                MagicExecutionKind.MovingTornado, 1, 8, 3, (Fix64)10, (Fix64)1.5m,
                (Fix64)8, effectFrames: 8, effectIntervalFrames: 2);
            SimulationWorld world = new SimulationWorld(123,
                SimulationPrefabRegistry.CreateProduction());
            world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 10,
                new SimVector2((Fix64)2, (Fix64)5));
            SimulationEntity ally = world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 10,
                new SimVector2((Fix64)7, (Fix64)4.9m));
            SimulationEntity enemy = world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 20,
                new SimVector2((Fix64)7, (Fix64)5.1m));
            MagicSimulation simulation = new MagicSimulation(1, world,
                new ProductionMagicCatalog(new[] { definition }));
            simulation.RegisterCombatant(ally.Id, 100);
            simulation.RegisterCombatant(enemy.Id, 100);

            simulation.Step(new[] { Cast(10, 1, "test_tornado", 7, 5) });

            SimulationEntity tornado = world.Entities[3];
            Assert.That(ally.Position.X, Is.GreaterThan(tornado.Position.X),
                "server stores center minus victim, so the first orbit step crosses the center");
            Assert.That(enemy.Position.X, Is.GreaterThan(tornado.Position.X));
            Assert.That(ally.Position3D.Z, Is.GreaterThan((Fix64)0.39m).And.LessThan((Fix64)0.41m));
            Assert.That(enemy.Position3D.Z, Is.GreaterThan((Fix64)0.39m).And.LessThan((Fix64)0.41m));

            simulation.Step(Array.Empty<MagicCastCommand>());

            Assert.That(simulation.Combatants[ally.Id].Health, Is.EqualTo(93));
            Assert.That(simulation.Combatants[enemy.Id].Health, Is.EqualTo(93));
        }

        [Test]
        public void ChainShotHitsReservedOverlappingTargetAfterServerDelay()
        {
            MagicDefinition definition = new MagicDefinition(
                1, "test_chain", "ChainLightning", MagicBehaviorFamily.Projectile,
                MagicExecutionKind.ChainShot, 1, 80, 12, (Fix64)10, (Fix64)0.25m,
                (Fix64)8, effectRadius: (Fix64)3, minimumDamage: 2, maximumHits: 5);
            SimulationWorld world = new SimulationWorld(123,
                SimulationPrefabRegistry.CreateProduction());
            world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 10,
                new SimVector2((Fix64)2, (Fix64)5));
            SimulationEntity firstTarget = world.Spawn("Crater", 20,
                new SimVector2((Fix64)4, (Fix64)5));
            SimulationEntity reservedTarget = world.Spawn("GroundCannon", 20,
                new SimVector2((Fix64)4.1m, (Fix64)5));
            MagicSimulation simulation = new MagicSimulation(1, world,
                new ProductionMagicCatalog(new[] { definition }));
            simulation.RegisterCombatant(firstTarget.Id, 100);
            simulation.RegisterCombatant(reservedTarget.Id, 100);

            for (int frame = 0; frame < 12 && simulation.Combatants[firstTarget.Id].Health == 100; frame++)
                simulation.Step(frame == 0
                    ? new[] { Cast(10, 1, "test_chain", 4, 5) }
                    : Array.Empty<MagicCastCommand>());

            Assert.That(simulation.Combatants[firstTarget.Id].Health, Is.LessThan(100));
            Assert.That(simulation.Combatants[reservedTarget.Id].Health, Is.EqualTo(100));

            for (int frame = 0; frame < 4; frame++)
                simulation.Step(Array.Empty<MagicCastCommand>());

            Assert.That(simulation.Combatants[reservedTarget.Id].Health, Is.LessThan(100));
        }

        [Test]
        public void RollingRockUsesArenaBoundsAndExpiresOnThirdBounce()
        {
            MagicDefinition definition = new MagicDefinition(
                1, "test_rock", "RockRolling", MagicBehaviorFamily.Projectile,
                MagicExecutionKind.RollingRock, 1, 200, 10, (Fix64)20, (Fix64)0.5m,
                (Fix64)8, maximumHits: 2);
            SimulationWorld world = new SimulationWorld(123,
                SimulationPrefabRegistry.CreateProduction());
            world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 10,
                new SimVector2((Fix64)17, (Fix64)5));
            MagicSimulation simulation = new MagicSimulation(1, world,
                new ProductionMagicCatalog(new[] { definition }));

            simulation.Step(new[] { Cast(10, 1, "test_rock", 18, 5) });
            SimulationEntity rock = world.Entities[1];
            for (int frame = 0; frame < 3 && rock.Velocity.X > Fix64.Zero; frame++)
                simulation.Step(Array.Empty<MagicCastCommand>());

            Assert.That(rock.Velocity.X, Is.LessThan(Fix64.Zero));
            Assert.That(rock.Position.X, Is.LessThan((Fix64)18 - rock.Radius));

            for (int frame = 0; frame < 60 && rock.Velocity.X < Fix64.Zero; frame++)
                simulation.Step(Array.Empty<MagicCastCommand>());
            Assert.That(rock.Velocity.X, Is.GreaterThan(Fix64.Zero));

            for (int frame = 0; frame < 60 && !rock.IsDestroyed; frame++)
                simulation.Step(Array.Empty<MagicCastCommand>());
            Assert.That(rock.IsDestroyed, Is.True);
        }

        [Test]
        public void WindBladeKnockbackUsesServerCollisionDistanceOverBladeRadius()
        {
            MagicDefinition definition = new MagicDefinition(
                1, "test_blade", "WindBlade", MagicBehaviorFamily.Projectile,
                MagicExecutionKind.PiercingShot, 1, 30, 9, (Fix64)10, (Fix64)0.5m,
                (Fix64)12, "knockback", 10);
            SimulationPrefabRegistry prefabs = new SimulationPrefabRegistry(new[]
            {
                new SimulationPrefabDefinition(SimulationPrefabRegistry.PlayerPrefabId,
                    new SimulationPhysicsDefinition(Fix64.One, Fix64.One),
                    "Transform", "PhysicsBody", "PlayerState"),
                new SimulationPrefabDefinition("WindBlade",
                    new SimulationPhysicsDefinition((Fix64)0.5m, Fix64.One, true),
                    "Transform", "PhysicsBody", "MagicState"),
                new SimulationPrefabDefinition("BigTarget",
                    new SimulationPhysicsDefinition((Fix64)2, Fix64.One),
                    "Transform", "PhysicsBody", "MobState",
                    SimulationPrefabRegistry.CommonEffectReceiverComponentId,
                    SimulationPrefabRegistry.RigidBodyComponentId,
                    SimulationPrefabRegistry.ZPhysicsComponentId)
            });
            SimulationWorld world = new SimulationWorld(123, prefabs);
            world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 10,
                new SimVector2((Fix64)2, (Fix64)5));
            SimulationEntity target = world.Spawn("BigTarget", 20,
                new SimVector2((Fix64)5, (Fix64)5));
            MagicSimulation simulation = new MagicSimulation(1, world,
                new ProductionMagicCatalog(new[] { definition }));
            simulation.RegisterCombatant(target.Id, 100);

            for (int frame = 0; frame < 8 && simulation.Combatants[target.Id].Health == 100; frame++)
                simulation.Step(frame == 0
                    ? new[] { Cast(10, 1, "test_blade", 5, 5) }
                    : Array.Empty<MagicCastCommand>());

            Assert.That(simulation.Combatants[target.Id].Health, Is.LessThan(100));
            Assert.That(target.Position.X, Is.GreaterThan((Fix64)5.6m));
        }

        [Test]
        public void MindControlTransfersEnemySummonOwnershipAndStateHash()
        {
            MagicDefinition definition = new MagicDefinition(
                1, "test_mind", "WillOWisp", MagicBehaviorFamily.Projectile,
                MagicExecutionKind.MindControlShot, 1, 0, 0, (Fix64)10, (Fix64)0.5m,
                (Fix64)12);
            SimulationPrefabRegistry prefabs = SimulationPrefabRegistry.CreateProduction();
            SimulationWorld firstWorld = new SimulationWorld(123, prefabs);
            firstWorld.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 10,
                new SimVector2((Fix64)2, (Fix64)5));
            SimulationEntity firstTarget = firstWorld.Spawn("FireSlime", 20,
                new SimVector2((Fix64)5, (Fix64)5));
            MagicSimulation first = new MagicSimulation(7, firstWorld,
                new ProductionMagicCatalog(new[] { definition }));
            first.RegisterCombatant(firstTarget.Id, 30);

            SimulationWorld secondWorld = new SimulationWorld(123, prefabs);
            secondWorld.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 10,
                new SimVector2((Fix64)2, (Fix64)5));
            SimulationEntity secondTarget = secondWorld.Spawn("FireSlime", 20,
                new SimVector2((Fix64)5, (Fix64)5));
            MagicSimulation second = new MagicSimulation(7, secondWorld,
                new ProductionMagicCatalog(new[] { definition }));
            second.RegisterCombatant(secondTarget.Id, 30);

            for (int frame = 0; frame < 10; frame++)
            {
                IReadOnlyList<MagicCastCommand> casts = frame == 0
                    ? new[] { Cast(10, 1, "test_mind", 5, 5) }
                    : Array.Empty<MagicCastCommand>();
                first.Step(casts); second.Step(casts);
                Assert.That(second.CalculateStateHash(), Is.EqualTo(first.CalculateStateHash()));
            }

            Assert.That(firstTarget.OwnerUserId, Is.EqualTo(10));
            Assert.That(secondTarget.OwnerUserId, Is.EqualTo(10));
        }

        [TestCase("Player", false)]
        [TestCase("GroundCannon", false)]
        [TestCase("ManaWell", false)]
        [TestCase("HealingTotem", false)]
        [TestCase("LifeTree", false)]
        [TestCase("RockTurret", false)]
        [TestCase("WindTotem", true)]
        [TestCase("GroundTower", true)]
        [TestCase("ElectricTower", true)]
        public void MindControlMatchesServerExcludedMobComponents(string prefabId,
            bool expectedTransfer)
        {
            MagicDefinition definition = new MagicDefinition(
                1, "test_mind", "WillOWisp", MagicBehaviorFamily.Projectile,
                MagicExecutionKind.MindControlShot, 1, 20, 0, (Fix64)10, (Fix64)0.5m,
                (Fix64)12);
            SimulationWorld world = new SimulationWorld(123,
                SimulationPrefabRegistry.CreateProduction());
            world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 10,
                new SimVector2((Fix64)2, (Fix64)5));
            SimulationEntity target = world.Spawn(prefabId, 20,
                new SimVector2((Fix64)5, (Fix64)5));
            MagicSimulation simulation = new MagicSimulation(7, world,
                new ProductionMagicCatalog(new[] { definition }));
            simulation.RegisterCombatant(target.Id, 100);

            for (int frame = 0; frame < 12; frame++)
                simulation.Step(frame == 0
                    ? new[] { Cast(10, 1, "test_mind", 5, 5) }
                    : Array.Empty<MagicCastCommand>());

            Assert.That(target.OwnerUserId, Is.EqualTo(expectedTransfer ? 10 : 20));
        }

        [Test]
        public void ElementFieldAppliesStatusExpiresAndTransformsDeterministically()
        {
            SimulationWorld world = new SimulationWorld(123, SimulationPrefabRegistry.CreateProduction());
            world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 10,
                new SimVector2((Fix64)2, (Fix64)5));
            SimulationEntity target = world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 20,
                new SimVector2((Fix64)7, (Fix64)5));
            SimulationEntity fireField = world.Spawn("FireField", 10,
                new SimVector2((Fix64)7, (Fix64)5));
            MagicSimulation simulation = new MagicSimulation(5, world, ProductionMagicCatalog.Create());

            simulation.Step(Array.Empty<MagicCastCommand>());
            Assert.That(simulation.Combatants[target.Id].Statuses.ContainsKey("burn"), Is.True);
            for (int frame = 1; frame < 60; frame++)
                simulation.Step(Array.Empty<MagicCastCommand>());
            Assert.That(fireField.IsDestroyed, Is.True);

            SimulationEntity leaf = world.Spawn("LeafField", 10,
                new SimVector2((Fix64)7, (Fix64)5));
            simulation.Step(Array.Empty<MagicCastCommand>());
            simulation.Step(new[] { Cast(10, 1, "magma_explosion", 7, 5) });
            for (int frame = 1; frame < 11; frame++)
                simulation.Step(Array.Empty<MagicCastCommand>());

            Assert.That(leaf.IsDestroyed, Is.True);
            Assert.That(world.Entities.Last().PrefabId, Is.EqualTo("FireField"));
            Assert.That(world.Entities.Last().OwnerUserId, Is.Zero);
        }

        [Test]
        public void CraterEmberUsesSeededTravelThenCreatesNeutralFireField()
        {
            SimulationWorld firstWorld = new SimulationWorld(123, SimulationPrefabRegistry.CreateProduction());
            firstWorld.Spawn("CraterEmber", 10, new SimVector2((Fix64)5, (Fix64)5));
            MagicSimulation first = new MagicSimulation(7, firstWorld, ProductionMagicCatalog.Create());
            SimulationWorld secondWorld = new SimulationWorld(123, SimulationPrefabRegistry.CreateProduction());
            secondWorld.Spawn("CraterEmber", 10, new SimVector2((Fix64)5, (Fix64)5));
            MagicSimulation second = new MagicSimulation(7, secondWorld, ProductionMagicCatalog.Create());

            for (int frame = 0; frame < 20; frame++)
            {
                first.Step(Array.Empty<MagicCastCommand>());
                second.Step(Array.Empty<MagicCastCommand>());
                Assert.That(second.CalculateStateHash(), Is.EqualTo(first.CalculateStateHash()));
                if (frame == 0)
                    Assert.That(firstWorld.Entities[0].Position3D.Z, Is.GreaterThan(Fix64.Zero));
            }

            Assert.That(firstWorld.Entities.Any(value => value.PrefabId == "FireField" &&
                value.OwnerUserId == 0), Is.True);
        }

        [Test]
        public void RuneCollisionExplodesAgainstAllCombatantsInRadius()
        {
            SimulationWorld world = new SimulationWorld(123, SimulationPrefabRegistry.CreateProduction());
            SimulationEntity left = world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 10,
                new SimVector2((Fix64)5, (Fix64)5));
            SimulationEntity right = world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 20,
                new SimVector2((Fix64)6, (Fix64)5));
            SimulationEntity rune = world.Spawn("FireRune", 0, new SimVector2((Fix64)5, (Fix64)5));
            MagicSimulation simulation = new MagicSimulation(7, world, ProductionMagicCatalog.Create());

            simulation.Step(Array.Empty<MagicCastCommand>());

            Assert.That(rune.IsDestroyed, Is.True);
            Assert.That(simulation.Combatants[left.Id].Health, Is.EqualTo(95));
            Assert.That(simulation.Combatants[right.Id].Health, Is.EqualTo(95));
        }

        [Test]
        public void ProjectileExpiresButSpawnAndBuildLifetimeBelongsToPrefabComponents()
        {
            MagicSimulation projectile = Create();
            projectile.Step(new[] { Cast(10, 1, "fire_shot", 7, 5) });
            for (int frame = 0; frame < 20; frame++) projectile.Step(Array.Empty<MagicCastCommand>());
            Assert.That(projectile.Combatants[1].Health, Is.LessThan(100));

            MagicSimulation build = Create();
            build.Step(new[] { Cast(10, 1, "fire_slime_nest", 5, 5), Cast(20, 1, "leafair", 7, 5) });
            int spawnedCount = build.World.Entities.Count;
            for (int frame = 0; frame < 40; frame++) build.Step(Array.Empty<MagicCastCommand>());
            Assert.That(build.World.Entities.Count, Is.EqualTo(spawnedCount));
            Assert.That(build.World.Entities[2].IsDestroyed, Is.False,
                "build lifetime belongs to prefab component");
            Assert.That(build.World.Entities[3].IsDestroyed, Is.True,
                "drop object keeps configured magic lifetime");
        }

        [Test]
        public void FullMagicReplayProducesSameHashAtEveryFrame()
        {
            MagicSimulation first = Create();
            MagicSimulation second = Create();
            for (int frame = 0; frame < 80; frame++)
            {
                IReadOnlyList<MagicCastCommand> casts = frame == 0
                    ? new[] { Cast(20, 9, "magma_explosion", 5, 5), Cast(10, 3, "ember_spirit_swarm", 5, 5), Cast(10, 4, "fire_shot", 7, 5) }
                    : Array.Empty<MagicCastCommand>();
                first.Step(casts); second.Step(casts);
                Assert.That(second.CalculateStateHash(), Is.EqualTo(first.CalculateStateHash()), "frame " + frame);
            }
        }

        [Test]
        public void EveryServerMagicProducesSameHashAtEveryReplayFrame()
        {
            MagicSimulation first = Create();
            MagicSimulation second = Create();
            IReadOnlyList<ServerMagicDescriptor> magics = ServerMagicManifest.All;
            int totalFrames = magics.Count * 5 + 220;
            for (int frame = 0; frame < totalFrames; frame++)
            {
                IReadOnlyList<MagicCastCommand> casts = Array.Empty<MagicCastCommand>();
                if (frame % 5 == 0 && frame / 5 < magics.Count)
                {
                    int index = frame / 5;
                    long userId = index % 2 == 0 ? 10 : 20;
                    casts = new[] { Cast(userId, index + 1, magics[index].Name,
                        userId == 10 ? 6 : 4, 5) };
                }
                first.Step(casts);
                second.Step(casts);
                Assert.That(second.CalculateStateHash(), Is.EqualTo(first.CalculateStateHash()),
                    "frame " + frame);
            }
        }

        [Test]
        public void EveryServerMagicRunsThroughVersionedProductionCatalogWithoutFallback()
        {
            IReadOnlyList<MagicParameterValue> parameters = CompleteMagicParameters();
            MagicSimulation first = CreateVersioned(parameters);
            MagicSimulation second = CreateVersioned(parameters);
            for (int frame = 0; frame < ServerMagicManifest.All.Count * 5 + 220; frame++)
            {
                IReadOnlyList<MagicCastCommand> casts = Array.Empty<MagicCastCommand>();
                if (frame % 5 == 0 && frame / 5 < ServerMagicManifest.All.Count)
                {
                    int index = frame / 5;
                    long userId = index % 2 == 0 ? 10 : 20;
                    casts = new[] { Cast(userId, index + 1,
                        ServerMagicManifest.All[index].Name, userId == 10 ? 6 : 4, 5) };
                }
                first.Step(casts); second.Step(casts);
                Assert.That(second.CalculateStateHash(), Is.EqualTo(first.CalculateStateHash()),
                    "versioned frame " + frame);
            }
            Assert.That(ProductionMagicCatalog.Create(parameters).Definitions.Count, Is.EqualTo(67));
        }

        [Test]
        public void VersionedCatalogRejectsMissingDirectDamageParameter()
        {
            IReadOnlyList<MagicParameterValue> parameters = CompleteMagicParameters()
                .Where(value => !(string.Equals(value.ObjectName, "fire_shot", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(value.ParameterName, "damage", StringComparison.OrdinalIgnoreCase)))
                .ToArray();

            Assert.Throws<InvalidOperationException>(() => ProductionMagicCatalog.Create(parameters));
        }

        private static MagicSimulation Create()
        {
            SimulationWorld world = new SimulationWorld(123, SimulationPrefabRegistry.CreateProduction());
            world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 10, new SimVector2((Fix64)2, (Fix64)5));
            world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 20, new SimVector2((Fix64)7, (Fix64)5));
            return new MagicSimulation(987654321, world, ProductionMagicCatalog.Create());
        }

        private static MagicSimulation CreateWith(params MagicDefinition[] definitions)
        {
            SimulationWorld world = new SimulationWorld(123, SimulationPrefabRegistry.CreateProduction());
            world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 10, new SimVector2((Fix64)2, (Fix64)5));
            world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 20, new SimVector2((Fix64)7, (Fix64)5));
            return new MagicSimulation(987654321, world, new ProductionMagicCatalog(definitions));
        }

        private static MagicSimulation CreateVersioned(IReadOnlyList<MagicParameterValue> parameters)
        {
            List<SimulationPrefabParameterValue> prefabParameters = parameters.Select(value =>
                new SimulationPrefabParameterValue(value.ObjectName, value.ParameterName, value.Value)).ToList();
            SimulationWorld world = new SimulationWorld(123,
                SimulationPrefabRegistry.CreateProduction(prefabParameters));
            world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 10,
                new SimVector2((Fix64)2, (Fix64)5));
            world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 20,
                new SimVector2((Fix64)7, (Fix64)5));
            return new MagicSimulation(987654321, world, ProductionMagicCatalog.Create(parameters));
        }

        internal static IReadOnlyList<MagicParameterValue> CompleteMagicParameters()
        {
            HashSet<string> objectNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "spawn", "shoot", "explode", "build", "drop", "leaf_drop", "leaf_slime", "water_slime" };
            foreach (string prefabId in ServerPrefabManifest.All) objectNames.Add(ParameterName(prefabId));
            string[] parameterNames =
            {
                "radius", "mass", "range", "attack_range", "speed", "damage", "duration",
                "attack_interval", "quantity", "min_damage", "buff_duration", "z_force"
            };
            List<MagicParameterValue> values = new List<MagicParameterValue>();
            foreach (string objectName in objectNames)
                for (int index = 0; index < parameterNames.Length; index++)
                    values.Add(new MagicParameterValue(objectName, parameterNames[index],
                        ParameterValue(parameterNames[index])));
            return values;
        }

        private static Fix64 ParameterValue(string parameterName)
        {
            switch (parameterName)
            {
                case "quantity": return (Fix64)3;
                case "duration": return (Fix64)8;
                case "attack_interval": return Fix64.One;
                case "speed": return (Fix64)8;
                case "damage": return (Fix64)10;
                case "min_damage": return (Fix64)2;
                case "buff_duration": return (Fix64)10;
                case "z_force": return (Fix64)5;
                case "mass": return Fix64.One;
                case "range": return (Fix64)6;
                default: return (Fix64)1.5m;
            }
        }

        private static string ParameterName(string prefabId)
        {
            if (prefabId == "Leafair") return "leaf_drop";
            List<char> value = new List<char>();
            for (int index = 0; index < prefabId.Length; index++)
            {
                char current = prefabId[index];
                if (index > 0 && char.IsUpper(current) &&
                    (!char.IsUpper(prefabId[index - 1]) ||
                     index + 1 < prefabId.Length && char.IsLower(prefabId[index + 1]))) value.Add('_');
                value.Add(char.ToLowerInvariant(current));
            }
            return new string(value.ToArray());
        }

        private static MagicCastCommand Cast(long userId, int requestId, string magicName, int x, int y) =>
            new MagicCastCommand(userId, requestId, magicName, new SimVector2((Fix64)x, (Fix64)y));
    }
}
