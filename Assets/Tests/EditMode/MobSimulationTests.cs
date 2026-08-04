using System;
using System.Collections.Generic;
using System.Linq;
using FixMath.NET;
using GameScene.Simulation.Core;
using GameScene.Simulation.Mob;
using GameScene.Simulation.Objects;
using GameScene.Simulation.Physics;
using NUnit.Framework;

namespace GameScene.Simulation.Tests
{
    public sealed class MobSimulationTests
    {
        [Test]
        public void AStarUsesExplicitTotalOrderForEqualRoutes()
        {
            NavigationGrid grid = new NavigationGrid(3, 3, new[] { new GridPosition(1, 1) });
            DeterministicAStar pathFinder = new DeterministicAStar(grid);

            GridPosition[] path = pathFinder.FindPath(new GridPosition(0, 1), new GridPosition(2, 1)).ToArray();

            Assert.That(path, Is.EqualTo(new[]
            {
                new GridPosition(0, 1), new GridPosition(0, 0), new GridPosition(1, 0),
                new GridPosition(2, 0), new GridPosition(2, 1)
            }));
        }

        [Test]
        public void BlockedCellInputOrderDoesNotChangePath()
        {
            GridPosition[] ascending = { new GridPosition(1, 1), new GridPosition(2, 1), new GridPosition(2, 2) };
            GridPosition[] descending = ascending.Reverse().ToArray();
            DeterministicAStar first = new DeterministicAStar(new NavigationGrid(5, 5, ascending));
            DeterministicAStar second = new DeterministicAStar(new NavigationGrid(5, 5, descending));

            Assert.That(second.FindPath(new GridPosition(0, 1), new GridPosition(4, 1)),
                Is.EqualTo(first.FindPath(new GridPosition(0, 1), new GridPosition(4, 1))));
        }

        [Test]
        public void EqualDistanceTargetUsesLowestEntityId()
        {
            MobSimulation simulation = CreateThreeMobSimulation(out MobAgentState self, out _, out _);

            simulation.PrepareFrameInputs();

            Assert.That(self.TargetEntityId, Is.EqualTo(1));
        }

        [Test]
        public void AttackCooldownUsesSimulationFramesOnly()
        {
            SimulationWorld world = new SimulationWorld(1);
            MobSimulation simulation = new MobSimulation(world, new NavigationGrid(10, 10));
            MobDefinition definition = Definition(attackRange: 2, cooldownFrames: 3);
            MobAgentState attacker = simulation.Register(world.Spawn(10, At(2, 2)).Id, definition);
            MobAgentState target = simulation.Register(world.Spawn(20, At(3, 2)).Id, definition);

            simulation.Step();
            Assert.That(attacker.State, Is.EqualTo(MobBehaviorState.Attacking));
            Assert.That(target.Health, Is.EqualTo(100));
            simulation.Step();
            Assert.That(target.Health, Is.EqualTo(100));
            simulation.Step();
            Assert.That(target.Health, Is.EqualTo(90));
            simulation.Step(); simulation.Step();
            Assert.That(target.Health, Is.EqualTo(90));
            simulation.Step();
            Assert.That(target.Health, Is.EqualTo(80));
        }

        [Test]
        public void RepeatedReplayProducesSamePathAndHashEveryFrame()
        {
            MobSimulation first = CreateThreeMobSimulation(out _, out _, out _);
            MobSimulation second = CreateThreeMobSimulation(out _, out _, out _);

            for (int frame = 0; frame < 50; frame++)
            {
                first.Step(); second.Step();
                Assert.That(second.CalculateStateHash(), Is.EqualTo(first.CalculateStateHash()), "frame " + frame);
                Assert.That(second.Agents[0].Path, Is.EqualTo(first.Agents[0].Path), "path frame " + frame);
            }
        }

        [Test]
        public void DefinitionDifferenceChangesInitialHash()
        {
            SimulationWorld firstWorld = new SimulationWorld(7);
            SimulationWorld secondWorld = new SimulationWorld(7);
            MobSimulation first = new MobSimulation(firstWorld, new NavigationGrid(5, 5));
            MobSimulation second = new MobSimulation(secondWorld, new NavigationGrid(5, 5));
            first.Register(firstWorld.Spawn(10, At(1, 1)).Id, Definition(1, 3));
            second.Register(secondWorld.Spawn(10, At(1, 1)).Id, Definition(2, 3));

            Assert.That(second.CalculateStateHash(), Is.Not.EqualTo(first.CalculateStateHash()));
        }

        [Test]
        public void PassiveComponentsHealAndSpawnPathDeterministically()
        {
            SimulationPrefabRegistry prefabs = new SimulationPrefabRegistry(new[]
            {
                new SimulationPrefabDefinition("Mob", "Transform", "PhysicsBody"),
                new SimulationPrefabDefinition("Field", "Transform", "PhysicsBody")
            });
            SimulationWorld world = new SimulationWorld(4, prefabs);
            MobSimulation simulation = new MobSimulation(world, new NavigationGrid(20, 10));
            MobDefinition attackerDefinition = new MobDefinition(
                (Fix64)40, (Fix64)20, (Fix64)1, 10, 1, 20, 100, (Fix64)0.5m,
                MobArchetype.Slime, "Field", (Fix64)1, null, 5, 2);
            MobDefinition targetDefinition = Definition(1, 20);
            MobAgentState attacker = simulation.Register(
                world.Spawn("Mob", 10, At(2, 2)).Id, attackerDefinition);
            simulation.Register(world.Spawn("Mob", 20, At(10, 2)).Id, targetDefinition);
            simulation.ApplyDamage(attacker.EntityId, 50);

            simulation.Step();

            Assert.That(world.Entities.Count, Is.EqualTo(3));
            Assert.That(world.Entities[2].PrefabId, Is.EqualTo("Field"));
            Assert.That(attacker.Health, Is.EqualTo(50));

            simulation.Step();
            Assert.That(attacker.Health, Is.EqualTo(55));
        }

        [Test]
        public void AreaEffectProviderEmitsStatusAtDeterministicIntervalAndColliderBoundary()
        {
            SimulationWorld world = new SimulationWorld(41,
                new SimulationPrefabRegistry(new[]
                {
                    new SimulationPrefabDefinition(SimulationPrefabRegistry.DefaultPrefabId,
                        "Transform", "PhysicsBody",
                        SimulationPrefabRegistry.CommonEffectReceiverComponentId)
                }));
            MobSimulation simulation = new MobSimulation(world, new NavigationGrid(10, 10));
            MobDefinition aura = new MobDefinition(
                Fix64.Zero, Fix64.Zero, Fix64.Zero, 0, 1, 20, 100, (Fix64)0.5m,
                auraStatusId: "burn", auraRange: (Fix64)2, auraIntervalFrames: 2);
            MobDefinition target = Definition(1, 20);
            simulation.Register(world.Spawn(10, At(2, 2)).Id, aura);
            MobAgentState boundary = simulation.Register(world.Spawn(20,
                new SimVector2((Fix64)4.25m, (Fix64)2)).Id, target);

            simulation.Step();
            Assert.That(simulation.GeneratedStatusEffects, Is.Empty);
            simulation.Step();

            Assert.That(simulation.GeneratedStatusEffects.Any(effect =>
                effect.TargetEntityId == boundary.EntityId && effect.StatusId == "burn"), Is.True);
        }

        [Test]
        public void ChickenCommandoStartsAtConfiguredHeightAndFallsWithConfiguredGravity()
        {
            SimulationWorld world = new SimulationWorld(42);
            MobSimulation simulation = new MobSimulation(world, new NavigationGrid(10, 10));
            SimulationEntity entity = world.Spawn(10, At(2, 2));
            simulation.Register(entity.Id, new MobDefinition(
                Fix64.Zero, Fix64.Zero, Fix64.Zero, 0, 1, 20, 100, (Fix64)0.5m,
                spawnDelayFrames: 20, initialHeight: (Fix64)10, gravity: (Fix64)3));

            Assert.That(entity.Position3D.Z, Is.EqualTo((Fix64)10));
            simulation.Step();
            Assert.That(entity.LinearVelocity.Z,
                Is.EqualTo(-(Fix64)3 * SimulationWorld.FixedDeltaTime));
            simulation.Step();
            Assert.That(entity.Position3D.Z, Is.LessThan((Fix64)10));
        }

        [Test]
        public void DeathComponentSpawnsConfiguredPrefabBeforeDestroy()
        {
            SimulationPrefabRegistry prefabs = new SimulationPrefabRegistry(new[]
            {
                new SimulationPrefabDefinition("Mob", "Transform", "PhysicsBody"),
                new SimulationPrefabDefinition("Field", "Transform", "PhysicsBody")
            });
            SimulationWorld world = new SimulationWorld(5, prefabs);
            MobSimulation simulation = new MobSimulation(world, new NavigationGrid(10, 10));
            MobDefinition attacker = new MobDefinition(Fix64.Zero, (Fix64)20, (Fix64)1, 10, 1, 20);
            MobDefinition target = new MobDefinition(
                Fix64.Zero, (Fix64)20, (Fix64)1, 0, 1, 20, 5, (Fix64)0.5m,
                MobArchetype.Slime, null, Fix64.Zero, "Field");
            simulation.Register(world.Spawn("Mob", 10, At(2, 2)).Id, attacker);
            simulation.Register(world.Spawn("Mob", 20, At(3, 2)).Id, target);

            simulation.Step();

            Assert.That(world.Entities[1].IsDestroyed, Is.True);
            Assert.That(world.Entities[2].PrefabId, Is.EqualTo("Field"));
        }

        [Test]
        public void TotemHealsAlliesOnFrameIntervalAndLifetimeExpires()
        {
            SimulationWorld world = new SimulationWorld(6);
            MobSimulation simulation = new MobSimulation(world, new NavigationGrid(10, 10));
            MobDefinition totem = new MobDefinition(
                Fix64.Zero, Fix64.Zero, Fix64.Zero, 0, 1, 20, 20, (Fix64)0.5m,
                MobArchetype.Totem, areaHealthDelta: -4, areaEffectRange: (Fix64)3,
                areaEffectIntervalFrames: 2, lifetimeFrames: 3);
            MobDefinition ally = Definition(1, 20);
            MobAgentState source = simulation.Register(world.Spawn(10, At(2, 2)).Id, totem);
            MobAgentState near = simulation.Register(world.Spawn(10, At(3, 2)).Id, ally);
            MobAgentState enemy = simulation.Register(world.Spawn(20, At(3, 3)).Id, ally);
            simulation.ApplyDamage(near.EntityId, 10);
            simulation.ApplyDamage(enemy.EntityId, 10);

            simulation.Step();
            Assert.That(near.Health, Is.EqualTo(90));
            simulation.Step();
            Assert.That(near.Health, Is.EqualTo(94));
            Assert.That(enemy.Health, Is.EqualTo(90));
            simulation.Step();
            Assert.That(world.Entities[source.EntityId].IsDestroyed, Is.True);
        }

        [Test]
        public void TowerSchedulesDelayedSplashDamageInEntityOrder()
        {
            SimulationWorld world = new SimulationWorld(7);
            MobSimulation simulation = new MobSimulation(world, new NavigationGrid(12, 12));
            MobDefinition tower = new MobDefinition(
                Fix64.Zero, (Fix64)20, (Fix64)8, 10, 1, 20, 100, (Fix64)0.5m,
                MobArchetype.Tower, attackDelayFrames: 2, splashRadius: (Fix64)1);
            MobDefinition target = Definition(1, 20);
            simulation.Register(world.Spawn(10, At(2, 2)).Id, tower);
            MobAgentState center = simulation.Register(world.Spawn(20, At(5, 2)).Id, target);
            MobAgentState nearby = simulation.Register(world.Spawn(20, At(6, 2)).Id, target);
            MobAgentState far = simulation.Register(world.Spawn(20, At(9, 2)).Id, target);

            simulation.Step();
            Assert.That(center.Health, Is.EqualTo(100));
            simulation.Step();
            Assert.That(center.Health, Is.EqualTo(100));
            simulation.Step();

            Assert.That(center.Health, Is.EqualTo(90));
            Assert.That(nearby.Health, Is.EqualTo(90));
            Assert.That(far.Health, Is.EqualTo(100));
        }

        [Test]
        public void LightningTowerChainsToNearestUnhitTargets()
        {
            SimulationWorld world = new SimulationWorld(8);
            MobSimulation simulation = new MobSimulation(world, new NavigationGrid(12, 12));
            MobDefinition tower = new MobDefinition(
                Fix64.Zero, (Fix64)20, (Fix64)8, 10, 1, 20, 100, (Fix64)0.5m,
                MobArchetype.LightningTower, chainDamage: 4, chainCount: 2, chainRadius: (Fix64)2);
            MobDefinition target = Definition(1, 20);
            simulation.Register(world.Spawn(10, At(2, 2)).Id, tower);
            MobAgentState first = simulation.Register(world.Spawn(20, At(5, 2)).Id, target);
            MobAgentState second = simulation.Register(world.Spawn(20, At(6, 2)).Id, target);
            MobAgentState third = simulation.Register(world.Spawn(20, At(7, 2)).Id, target);
            MobAgentState far = simulation.Register(world.Spawn(20, At(10, 2)).Id, target);

            simulation.Step();

            Assert.That(first.Health, Is.EqualTo(90));
            Assert.That(second.Health, Is.EqualTo(96));
            Assert.That(third.Health, Is.EqualTo(96));
            Assert.That(far.Health, Is.EqualTo(100));
        }

        [Test]
        public void SummonerSpawnsConfiguredPrefabAtTargetPosition()
        {
            SimulationPrefabRegistry prefabs = new SimulationPrefabRegistry(new[]
            {
                new SimulationPrefabDefinition("Mob", "Transform", "PhysicsBody"),
                new SimulationPrefabDefinition("MagmaFist", "Transform", "PhysicsBody")
            });
            SimulationWorld world = new SimulationWorld(9, prefabs);
            MobSimulation simulation = new MobSimulation(world, new NavigationGrid(10, 10));
            MobDefinition summoner = new MobDefinition(
                Fix64.Zero, (Fix64)20, (Fix64)4, 0, 1, 20, 100, (Fix64)0.5m,
                MobArchetype.Summoner, summonPrefabId: "MagmaFist");
            simulation.Register(world.Spawn("Mob", 10, At(2, 2)).Id, summoner);
            MobAgentState target = simulation.Register(world.Spawn("Mob", 20, At(3, 2)).Id, Definition(1, 20));

            simulation.Step();

            Assert.That(world.Entities[2].PrefabId, Is.EqualTo("MagmaFist"));
            Assert.That(world.Entities[2].Position, Is.EqualTo(world.Entities[target.EntityId].Position));
            Assert.That(world.Entities[2].OwnerUserId, Is.Zero);
        }

        [Test]
        public void SelfDestructExplosionDamagesAllNearbyMobsIncludingAllies()
        {
            SimulationWorld world = new SimulationWorld(10);
            MobSimulation simulation = new MobSimulation(world, new NavigationGrid(10, 10));
            MobDefinition bomb = new MobDefinition(
                Fix64.Zero, (Fix64)20, (Fix64)0.8m, 12, 1, 20, 20, (Fix64)0.5m,
                MobArchetype.SelfDestruct, explosionRadius: (Fix64)2);
            MobDefinition target = Definition(1, 20);
            MobAgentState source = simulation.Register(world.Spawn(10, At(2, 2)).Id, bomb);
            MobAgentState enemy = simulation.Register(world.Spawn(20, At(3, 2)).Id, target);
            MobAgentState ally = simulation.Register(world.Spawn(10, At(4, 2)).Id, target);

            simulation.Step();

            Assert.That(world.Entities[source.EntityId].IsDestroyed, Is.True);
            Assert.That(enemy.Health, Is.EqualTo(88));
            Assert.That(ally.Health, Is.EqualTo(88));
        }

        [Test]
        public void PlayerPriorityIgnoresCloserNonPlayerTarget()
        {
            SimulationPrefabRegistry prefabs = new SimulationPrefabRegistry(new[]
            {
                new SimulationPrefabDefinition("Mob", "Transform", "PhysicsBody"),
                new SimulationPrefabDefinition(SimulationPrefabRegistry.PlayerPrefabId, "Transform", "PhysicsBody")
            });
            SimulationWorld world = new SimulationWorld(11, prefabs);
            MobSimulation simulation = new MobSimulation(world, new NavigationGrid(12, 12));
            MobDefinition priority = new MobDefinition(
                (Fix64)2, (Fix64)20, (Fix64)1, 5, 2, 20, archetype: MobArchetype.PlayerPriorityMelee);
            MobAgentState source = simulation.Register(world.Spawn("Mob", 10, At(2, 2)).Id, priority);
            simulation.Register(world.Spawn("Mob", 20, At(3, 2)).Id, Definition(1, 20));
            MobAgentState player = simulation.Register(
                world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 20, At(8, 2)).Id,
                new MobDefinition(Fix64.Zero, Fix64.Zero, Fix64.Zero, 0, 1, 20,
                    archetype: MobArchetype.Dummy));

            simulation.PrepareFrameInputs();

            Assert.That(source.TargetEntityId, Is.EqualTo(player.EntityId));
        }

        [Test]
        public void LimitedSequenceSpawnerUsesStableScheduleAndLimit()
        {
            SimulationPrefabRegistry prefabs = new SimulationPrefabRegistry(new[]
            {
                new SimulationPrefabDefinition("Mob", "Transform", "PhysicsBody"),
                new SimulationPrefabDefinition("Child", "Transform", "PhysicsBody")
            });
            SimulationWorld world = new SimulationWorld(12, prefabs);
            MobSimulation simulation = new MobSimulation(world, new NavigationGrid(10, 10));
            MobDefinition spawner = new MobDefinition(
                Fix64.Zero, Fix64.Zero, (Fix64)4, 0, 1, 20, archetype: MobArchetype.KeepDistance,
                periodicSpawnPrefabId: "Child", periodicSpawnIntervalFrames: 2, periodicSpawnLimit: 2);
            simulation.Register(world.Spawn("Mob", 10, At(5, 5)).Id, spawner);

            for (int frame = 0; frame < 6; frame++) simulation.Step();

            Assert.That(world.Entities.Count, Is.EqualTo(3));
            Assert.That(world.Entities[1].SpawnFrame, Is.EqualTo(2));
            Assert.That(world.Entities[2].SpawnFrame, Is.EqualTo(4));
            Assert.That(world.Entities[1].OwnerUserId, Is.EqualTo(10));
        }

        [Test]
        public void DestructibleSpawnerPaysHealthForEverySpawnUntilDestroyed()
        {
            SimulationPrefabRegistry prefabs = new SimulationPrefabRegistry(new[]
            {
                new SimulationPrefabDefinition("Nest", "Transform", "PhysicsBody"),
                new SimulationPrefabDefinition("Child", "Transform", "PhysicsBody")
            });
            SimulationWorld world = new SimulationWorld(120, prefabs);
            MobSimulation simulation = new MobSimulation(world, new NavigationGrid(10, 10));
            MobDefinition spawner = new MobDefinition(
                Fix64.Zero, Fix64.Zero, Fix64.Zero, 0, 1, 20, maxHealth: 2,
                archetype: MobArchetype.Dummy, periodicSpawnPrefabId: "Child",
                periodicSpawnIntervalFrames: 1, periodicSelfDamage: 1);
            MobAgentState source = simulation.Register(world.Spawn("Nest", 10, At(5, 5)).Id, spawner);

            simulation.Step();
            Assert.That(source.Health, Is.EqualTo(1));
            simulation.Step();

            Assert.That(world.Entities[source.EntityId].IsDestroyed, Is.True);
            Assert.That(world.Entities.Count, Is.EqualTo(3));
            Assert.That(world.Entities[1].OwnerUserId, Is.EqualTo(10));
            Assert.That(world.Entities[2].OwnerUserId, Is.EqualTo(10));
            Assert.That(world.Entities[1].IsDestroyed, Is.False);
            Assert.That(world.Entities[2].IsDestroyed, Is.False);
        }

        [Test]
        public void DoubleTargetDamagesNearestSecondEnemy()
        {
            SimulationWorld world = new SimulationWorld(13);
            MobSimulation simulation = new MobSimulation(world, new NavigationGrid(20, 10));
            MobDefinition attacker = new MobDefinition(
                Fix64.Zero, (Fix64)20, (Fix64)8, 10, 1, 20, archetype: MobArchetype.DoubleTarget);
            MobDefinition target = Definition(1, 20);
            simulation.Register(world.Spawn(10, At(2, 2)).Id, attacker);
            MobAgentState first = simulation.Register(world.Spawn(20, At(4, 2)).Id, target);
            MobAgentState second = simulation.Register(world.Spawn(20, At(5, 2)).Id, target);
            MobAgentState third = simulation.Register(world.Spawn(20, At(7, 2)).Id, target);

            simulation.Step();

            Assert.That(first.Health, Is.EqualTo(90));
            Assert.That(second.Health, Is.EqualTo(90));
            Assert.That(third.Health, Is.EqualTo(100));
        }

        [Test]
        public void DoubleTargetHitsPrimaryTwiceWithoutSecondEnemy()
        {
            SimulationWorld world = new SimulationWorld(14);
            MobSimulation simulation = new MobSimulation(world, new NavigationGrid(20, 10));
            MobDefinition attacker = new MobDefinition(
                Fix64.Zero, (Fix64)20, (Fix64)8, 10, 1, 20, archetype: MobArchetype.DoubleTarget);
            simulation.Register(world.Spawn(10, At(2, 2)).Id, attacker);
            MobAgentState target = simulation.Register(world.Spawn(20, At(4, 2)).Id, Definition(1, 20));

            simulation.Step();

            Assert.That(target.Health, Is.EqualTo(80));
        }

        [Test]
        public void SprayingAttackDamagesUnionOfBothOverlapAreasIncludingAllies()
        {
            SimulationWorld world = new SimulationWorld(15);
            MobSimulation simulation = new MobSimulation(world, new NavigationGrid(20, 10));
            MobDefinition spray = new MobDefinition(
                Fix64.Zero, (Fix64)20, (Fix64)4, 7, 1, 20, 100, (Fix64)0.5m,
                MobArchetype.Spraying);
            MobDefinition target = Definition(1, 20);
            MobAgentState source = simulation.Register(world.Spawn(10, At(2, 2)).Id, spray);
            MobAgentState first = simulation.Register(world.Spawn(20, At(4, 2)).Id, target);
            MobAgentState ally = simulation.Register(world.Spawn(10, At(7, 2)).Id, target);
            MobAgentState second = simulation.Register(world.Spawn(20, At(8, 2)).Id, target);
            MobAgentState outside = simulation.Register(world.Spawn(20, At(12, 2)).Id, target);

            simulation.Step();

            Assert.That(source.Health, Is.EqualTo(93));
            Assert.That(first.Health, Is.EqualTo(93));
            Assert.That(ally.Health, Is.EqualTo(93));
            Assert.That(second.Health, Is.EqualTo(93));
            Assert.That(outside.Health, Is.EqualTo(100));
        }

        [Test]
        public void CowardFleesNearestUnitThenResumesObjectiveAfterCooldown()
        {
            SimulationPrefabRegistry prefabs = new SimulationPrefabRegistry(new[]
            {
                new SimulationPrefabDefinition("Mob", "Transform", "PhysicsBody"),
                new SimulationPrefabDefinition(SimulationPrefabRegistry.PlayerPrefabId, "Transform", "PhysicsBody")
            });
            SimulationWorld world = new SimulationWorld(16, prefabs);
            MobSimulation simulation = new MobSimulation(world, new NavigationGrid(20, 10));
            MobDefinition coward = new MobDefinition(
                (Fix64)2, (Fix64)3, (Fix64)1, 4, 2, 20, archetype: MobArchetype.Coward,
                panicDurationFrames: 2, panicCooldownFrames: 3, panicSpeedMultiplier: (Fix64)2);
            MobAgentState source = simulation.Register(world.Spawn("Mob", 10, At(5, 5)).Id, coward);
            simulation.Register(world.Spawn("Mob", 20, At(4, 5)).Id, Definition(1, 20));
            MobAgentState objective = simulation.Register(
                world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 20, At(12, 5)).Id,
                new MobDefinition(Fix64.Zero, Fix64.Zero, Fix64.Zero, 0, 1, 20,
                    archetype: MobArchetype.Dummy));

            simulation.Step();

            Assert.That(source.PanicUntilFrame, Is.EqualTo(2));
            Assert.That(world.Entities[source.EntityId].Velocity.X, Is.EqualTo((Fix64)4));
            simulation.Step();
            simulation.Step();
            Assert.That(source.PanicCooldownUntilFrame, Is.EqualTo(5));
            Assert.That(source.TargetEntityId, Is.EqualTo(objective.EntityId));
        }

        [Test]
        public void InfiniteSequenceSpawnerAlternatesConfiguredPrefabs()
        {
            SimulationPrefabRegistry prefabs = new SimulationPrefabRegistry(new[]
            {
                new SimulationPrefabDefinition("DimensionToad",
                    new SimulationPhysicsDefinition((Fix64)0.5m, Fix64.One, true),
                    "Transform", "PhysicsBody"),
                new SimulationPrefabDefinition("FireTadpole",
                    new SimulationPhysicsDefinition((Fix64)0.5m, Fix64.One, true),
                    "Transform", "PhysicsBody"),
                new SimulationPrefabDefinition("LightningTadpole",
                    new SimulationPhysicsDefinition((Fix64)0.5m, Fix64.One, true),
                    "Transform", "PhysicsBody")
            });
            SimulationWorld world = new SimulationWorld(17, prefabs);
            MobSimulation simulation = new MobSimulation(world, new NavigationGrid(20, 10));
            MobDefinition toad = new MobDefinition(
                Fix64.Zero, Fix64.Zero, (Fix64)4, 0, 1, 20,
                archetype: MobArchetype.NonAttackingCoward,
                periodicSpawnPrefabId: "FireTadpole", periodicSpawnIntervalFrames: 1,
                periodicSpawnLimit: 0, panicDurationFrames: 2, panicCooldownFrames: 3,
                panicSpeedMultiplier: (Fix64)2, secondaryPeriodicSpawnPrefabId: "LightningTadpole");
            simulation.Register(world.Spawn("DimensionToad", 10, At(5, 5)).Id, toad);

            simulation.Step();
            simulation.Step();
            simulation.Step();

            Assert.That(world.Entities[1].PrefabId, Is.EqualTo("FireTadpole"));
            Assert.That(world.Entities[2].PrefabId, Is.EqualTo("LightningTadpole"));
            Assert.That(world.Entities[3].PrefabId, Is.EqualTo("FireTadpole"));
            Assert.That(world.Entities[1].Position.X,
                Is.GreaterThan((Fix64)5.14m).And.LessThan((Fix64)5.16m));
            Assert.That(world.Entities[2].Position.X,
                Is.GreaterThan((Fix64)5.49m).And.LessThan((Fix64)5.51m));
            Assert.That(world.Entities[3].Position.X,
                Is.GreaterThan((Fix64)5.84m).And.LessThan((Fix64)5.86m));
            Assert.That(world.Entities[1].Position3D.Z,
                Is.GreaterThan((Fix64)(-0.18m)).And.LessThan((Fix64)(-0.17m)));
        }

        [Test]
        public void ThunderBirdFloatsDivesAndScalesDamageByDropHeight()
        {
            SimulationWorld world = new SimulationWorld(18);
            MobSimulation simulation = new MobSimulation(world, new NavigationGrid(20, 10));
            MobDefinition bird = new MobDefinition(
                (Fix64)2, (Fix64)20, (Fix64)7, 20, 1, 4, 100, (Fix64)0.5m,
                MobArchetype.ThunderBird);
            MobAgentState source = simulation.Register(world.Spawn(10, At(3, 3)).Id, bird);
            MobAgentState target = simulation.Register(world.Spawn(20, At(6, 3)).Id, Definition(1, 20));

            for (int frame = 0; frame < 160 && target.Health == 100; frame++) simulation.Step();

            Assert.That(target.Health, Is.LessThan(100));
            Assert.That(target.Health, Is.GreaterThanOrEqualTo(80));
            Assert.That(source.State, Is.EqualTo(MobBehaviorState.Floating));
            Assert.That(world.Entities[source.EntityId].Position3D.Z, Is.LessThan((Fix64)3));
        }

        [Test]
        public void ThunderBirdReplayProducesSameVerticalStateHash()
        {
            MobSimulation first = CreateThunderBirdSimulation();
            MobSimulation second = CreateThunderBirdSimulation();

            for (int frame = 0; frame < 120; frame++)
            {
                first.Step();
                second.Step();
                Assert.That(second.CalculateStateHash(), Is.EqualTo(first.CalculateStateHash()), "frame " + frame);
            }
        }

        [Test]
        public void BubbleSpiritDelaysSplashAndOnlyDamagesAerialEnemies()
        {
            SimulationWorld world = new SimulationWorld(20);
            MobSimulation simulation = new MobSimulation(world, new NavigationGrid(20, 10));
            MobDefinition bubble = new MobDefinition(
                Fix64.Zero, (Fix64)20, (Fix64)8, 10, 20, 4, 100, (Fix64)0.5m,
                MobArchetype.BubbleSplash, isAerial: true, targetLayer: MobTargetLayer.Air,
                projectileSpeed: (Fix64)10);
            MobDefinition air = new MobDefinition(
                Fix64.Zero, Fix64.Zero, Fix64.Zero, 0, 20, 4, 100, (Fix64)0.5m,
                MobArchetype.Dummy, isAerial: true);
            MobDefinition ground = new MobDefinition(
                Fix64.Zero, Fix64.Zero, Fix64.Zero, 0, 20, 4, 100, (Fix64)0.5m,
                MobArchetype.Dummy);
            simulation.Register(world.Spawn(10, At(2, 2)).Id, bubble);
            MobAgentState center = simulation.Register(world.Spawn(20, At(5, 2)).Id, air);
            MobAgentState nearby = simulation.Register(world.Spawn(20, At(6, 2)).Id, air);
            MobAgentState grounded = simulation.Register(world.Spawn(20, At(5, 3)).Id, ground);

            for (int frame = 0; frame < 90; frame++) simulation.Step();

            Assert.That(center.Health, Is.LessThan(100));
            Assert.That(nearby.Health, Is.EqualTo(center.Health));
            Assert.That(grounded.Health, Is.EqualTo(100));
        }

        [Test]
        public void BubbleGeneratorShieldsNearestUnshieldedAllyAndBlocksFirstDamage()
        {
            SimulationWorld world = new SimulationWorld(21, new SimulationPrefabRegistry(new[]
            {
                new SimulationPrefabDefinition(SimulationPrefabRegistry.DefaultPrefabId,
                    "Transform", "PhysicsBody",
                    SimulationPrefabRegistry.CommonEffectReceiverComponentId)
            }));
            MobSimulation simulation = new MobSimulation(world, new NavigationGrid(20, 10));
            MobDefinition generator = new MobDefinition(
                Fix64.Zero, Fix64.Zero, (Fix64)4, 0, 20, 4, 100, (Fix64)0.5m,
                MobArchetype.BubbleGenerator);
            MobDefinition target = Definition(1, 20);
            simulation.Register(world.Spawn(10, At(2, 2)).Id, generator);
            MobAgentState nearest = simulation.Register(world.Spawn(10, At(3, 2)).Id, target);
            MobAgentState farther = simulation.Register(world.Spawn(10, At(5, 2)).Id, target);
            simulation.Register(world.Spawn(20, At(3, 3)).Id, target);

            simulation.Step();

            Assert.That(nearest.BubbleShieldFrames, Is.EqualTo(159));
            Assert.That(farther.BubbleShieldFrames, Is.Zero);
            simulation.ApplyDamage(nearest.EntityId, 10);
            Assert.That(nearest.Health, Is.EqualTo(100));
            Assert.That(nearest.BubbleShieldFrames, Is.Zero);
            simulation.ApplyDamage(nearest.EntityId, 10);
            Assert.That(nearest.Health, Is.EqualTo(90));
        }

        [Test]
        public void BubbleGeneratorIncludesAllyWhoseColliderOverlapsAttackRange()
        {
            SimulationWorld world = new SimulationWorld(22, new SimulationPrefabRegistry(new[]
            {
                new SimulationPrefabDefinition(SimulationPrefabRegistry.DefaultPrefabId,
                    "Transform", "PhysicsBody",
                    SimulationPrefabRegistry.CommonEffectReceiverComponentId)
            }));
            MobSimulation simulation = new MobSimulation(world, new NavigationGrid(20, 10));
            MobDefinition generator = new MobDefinition(
                Fix64.Zero, Fix64.Zero, (Fix64)4, 0, 20, 4, 100, (Fix64)0.5m,
                MobArchetype.BubbleGenerator);
            MobDefinition ally = Definition(1, 20);
            simulation.Register(world.Spawn(10, At(2, 2)).Id, generator);
            MobAgentState boundary = simulation.Register(world.Spawn(10,
                new SimVector2((Fix64)6.25m, (Fix64)2)).Id, ally);

            simulation.Step();

            Assert.That(boundary.BubbleShieldFrames, Is.EqualTo(159));
        }

        [Test]
        public void StationarySummonerSpawnsOwnedPrefabAtTarget()
        {
            SimulationPrefabRegistry prefabs = new SimulationPrefabRegistry(new[]
            {
                new SimulationPrefabDefinition("Colony", "Transform", "PhysicsBody"),
                new SimulationPrefabDefinition("Mob", "Transform", "PhysicsBody"),
                new SimulationPrefabDefinition("Vine", "Transform", "PhysicsBody")
            });
            SimulationWorld world = new SimulationWorld(22, prefabs);
            MobSimulation simulation = new MobSimulation(world, new NavigationGrid(20, 10));
            MobDefinition colony = new MobDefinition(
                Fix64.Zero, (Fix64)20, (Fix64)6, 0, 1, 4, 100, (Fix64)0.5m,
                MobArchetype.StationarySummoner, summonPrefabId: "Vine",
                targetLayer: MobTargetLayer.Ground);
            simulation.Register(world.Spawn("Colony", 10, At(2, 2)).Id, colony);
            MobAgentState target = simulation.Register(world.Spawn("Mob", 20, At(5, 2)).Id, Definition(1, 20));

            simulation.Step();

            Assert.That(world.Entities[2].PrefabId, Is.EqualTo("Vine"));
            Assert.That(world.Entities[2].OwnerUserId, Is.EqualTo(10));
            Assert.That(world.Entities[2].Position, Is.EqualTo(world.Entities[target.EntityId].Position));
        }

        [Test]
        public void DualAttackUsesGroundPrimaryAndIndependentDelayedAirAttack()
        {
            SimulationWorld world = new SimulationWorld(23);
            MobSimulation simulation = new MobSimulation(world, new NavigationGrid(20, 10));
            MobDefinition towerback = new MobDefinition(
                Fix64.Zero, (Fix64)20, (Fix64)2, 3, 1, 4, 100, (Fix64)0.5m,
                MobArchetype.DualAttack, targetLayer: MobTargetLayer.Ground,
                secondaryDamage: 5, secondaryAttackRange: (Fix64)6,
                secondaryAttackCooldownFrames: 4, secondaryAttackDelayFrames: 2);
            MobDefinition ground = Definition(1, 20);
            MobDefinition air = new MobDefinition(
                Fix64.Zero, Fix64.Zero, Fix64.Zero, 0, 20, 4, 100, (Fix64)0.5m,
                MobArchetype.Dummy, isAerial: true);
            simulation.Register(world.Spawn(10, At(2, 2)).Id, towerback);
            MobAgentState groundTarget = simulation.Register(world.Spawn(20, At(3, 2)).Id, ground);
            MobAgentState airTarget = simulation.Register(world.Spawn(20, At(5, 2)).Id, air);

            simulation.Step();
            Assert.That(groundTarget.Health, Is.EqualTo(97));
            Assert.That(airTarget.Health, Is.EqualTo(100));
            simulation.Step();
            Assert.That(airTarget.Health, Is.EqualTo(100));
            for (int frame = 0; frame < 45; frame++) simulation.Step();
            Assert.That(airTarget.Health, Is.LessThan(100));
        }

        [Test]
        public void PveBossSelectsSeededSpawnTypeOnceAndSpawnsDeterministicBurst()
        {
            MobSimulation first = CreatePveBossSimulation(24, out MobAgentState firstBoss);
            MobSimulation second = CreatePveBossSimulation(24, out MobAgentState secondBoss);

            Assert.That(secondBoss.SelectedPeriodicSpawnPrefabId,
                Is.EqualTo(firstBoss.SelectedPeriodicSpawnPrefabId));
            Assert.That(first.World.RandomDrawCount, Is.EqualTo(1));
            first.Step();
            second.Step();

            Assert.That(second.CalculateStateHash(), Is.EqualTo(first.CalculateStateHash()));
            Assert.That(first.World.Entities.Count, Is.EqualTo(3));
            Assert.That(first.World.Entities[1].PrefabId, Is.EqualTo(firstBoss.SelectedPeriodicSpawnPrefabId));
            Assert.That(first.World.Entities[2].PrefabId, Is.EqualTo(firstBoss.SelectedPeriodicSpawnPrefabId));
            Assert.That(first.World.Entities[1].Position.X, Is.EqualTo((Fix64)5.25m));
            Assert.That(first.World.Entities[2].Position.X, Is.EqualTo((Fix64)5.75m));
        }

        [Test]
        public void VineWitchRageGeneratesFourSeededBossCastsAtNineFrameIntervals()
        {
            SimulationWorld world = new SimulationWorld(25);
            MobSimulation simulation = new MobSimulation(world, new NavigationGrid(20, 10));
            MobDefinition witch = new MobDefinition(
                Fix64.Zero, (Fix64)1024, (Fix64)7, 0, 160, 4, 200, (Fix64)1,
                MobArchetype.PveBoss, targetLayer: MobTargetLayer.Ground,
                primaryBossMagicName: "vine_toss", secondaryBossMagicName: "vine_colony",
                rageBossMagicName: "vine_fan");
            MobAgentState boss = simulation.Register(world.Spawn(-1, At(5, 5)).Id, witch);
            simulation.Register(world.Spawn(10, At(7, 5)).Id, Definition(1, 20));
            simulation.ApplyDamage(boss.EntityId, 100);
            int rageCasts = 0;

            for (int frame = 0; frame < 30; frame++)
            {
                simulation.Step();
                if (simulation.GeneratedMagicCasts.Count > 0)
                {
                    Assert.That(simulation.GeneratedMagicCasts[0].MagicName, Is.EqualTo("vine_fan"));
                    Assert.That(simulation.GeneratedMagicCasts[0].CasterEntityId, Is.EqualTo(boss.EntityId));
                    rageCasts++;
                }
            }

            Assert.That(rageCasts, Is.EqualTo(4));
            Assert.That(boss.RageActivated, Is.True);
            Assert.That(boss.RageCastsRemaining, Is.Zero);
        }

        [Test]
        public void StatusesDriveDeterministicMovementStunAndFrenzyOwnership()
        {
            SimulationWorld world = new SimulationWorld(26);
            MobSimulation simulation = new MobSimulation(world, new NavigationGrid(20, 10));
            SimulationEntity sourceEntity = world.Spawn(10, At(2, 5));
            MobAgentState source = simulation.Register(sourceEntity.Id, Definition(1, 6));
            simulation.Register(world.Spawn(20, At(8, 5)).Id, Definition(1, 6));

            simulation.ResolveStatuses(source.EntityId, new Dictionary<string, int>
            {
                { "snared", 60 }
            });
            IReadOnlyList<SimulationInput> slowed = simulation.PrepareFrameInputs();
            SimulationInput slowedInput = slowed.First(value => value.EntityId == source.EntityId);
            Fix64 slowedMagnitude = Fix64.Sqrt(
                slowedInput.Value.X * slowedInput.Value.X + slowedInput.Value.Y * slowedInput.Value.Y);
            Assert.That(slowedMagnitude, Is.EqualTo(Fix64.One));

            simulation.ResolveStatuses(source.EntityId, new Dictionary<string, int>
            {
                { "shock", 10 }, { "frenzy", 200 }
            });
            SimulationInput stunned = simulation.PrepareFrameInputs()
                .First(value => value.EntityId == source.EntityId);
            Assert.That(stunned.Value, Is.EqualTo(SimVector2.Zero));
            Assert.That(sourceEntity.OwnerUserId, Is.Zero);

            simulation.ResolveStatuses(source.EntityId, new Dictionary<string, int>());
            Assert.That(sourceEntity.OwnerUserId, Is.EqualTo(10));
        }

        [Test]
        public void WindTotemAddsConfiguredForwardVelocityInsideBox()
        {
            SimulationWorld world = new SimulationWorld(27,
                SimulationPrefabRegistry.CreateProduction());
            MobSimulation simulation = new MobSimulation(world, new NavigationGrid(20, 10));
            simulation.ConfigurePlayerSides(10, 20);
            MobDefinition totem = new MobDefinition(
                Fix64.Zero, Fix64.Zero, Fix64.Zero, 0, 1, 20,
                archetype: MobArchetype.Dummy, pushForce: (Fix64)1.5m,
                pushRangeX: (Fix64)4, pushRangeY: (Fix64)3);
            MobDefinition dummy = new MobDefinition(
                Fix64.Zero, Fix64.Zero, Fix64.Zero, 0, 1, 20,
                archetype: MobArchetype.Dummy);
            simulation.Register(world.Spawn("WindTotem", 10, At(12, 5)).Id, totem);
            MobAgentState target = simulation.Register(world.Spawn("FireSlime", 20, At(14, 5)).Id, dummy);
            MobAgentState lateralOutside = simulation.Register(world.Spawn("FireSlime", 20, At(14, 6)).Id, dummy);

            IReadOnlyList<SimulationInput> inputs = simulation.PrepareFrameInputs();
            SimulationInput pushed = inputs.First(value => value.EntityId == target.EntityId);
            SimulationInput ignored = inputs.First(value => value.EntityId == lateralOutside.EntityId);

            Assert.That(pushed.Value.X, Is.EqualTo((Fix64)1.5m));
            Assert.That(pushed.Value.Y, Is.EqualTo(Fix64.Zero));
            Assert.That(ignored.Value, Is.EqualTo(SimVector2.Zero));
        }

        [Test]
        public void RallyUsesServerBehaviorMobComponentIncludingTowerback()
        {
            SimulationWorld world = new SimulationWorld(28,
                SimulationPrefabRegistry.CreateProduction());
            MobSimulation simulation = new MobSimulation(world, new NavigationGrid(20, 10));
            MobDefinition dualAttack = new MobDefinition(
                Fix64.Zero, Fix64.Zero, Fix64.Zero, 0, 1, 20,
                archetype: MobArchetype.DualAttack);
            MobDefinition keepDistance = new MobDefinition(
                Fix64.Zero, Fix64.Zero, Fix64.Zero, 0, 1, 20,
                archetype: MobArchetype.KeepDistance);
            MobAgentState towerback = simulation.Register(
                world.Spawn("Towerback", 10, At(5, 5)).Id, dualAttack);
            MobAgentState fireLord = simulation.Register(
                world.Spawn("FireLordSpirit", 10, At(6, 5)).Id, keepDistance);

            simulation.ResolveRally(towerback.EntityId, 99);
            simulation.ResolveRally(fireLord.EntityId, 99);

            Assert.That(towerback.RallySourceEntityId, Is.EqualTo(99));
            Assert.That(fireLord.RallySourceEntityId, Is.EqualTo(-1));
        }

        [Test]
        public void KeepDistanceStopsAtServerPreferredRangeToleranceEveryFrame()
        {
            SimulationWorld world = new SimulationWorld(29);
            MobSimulation simulation = new MobSimulation(world, new NavigationGrid(20, 10));
            MobDefinition keepDistance = new MobDefinition(
                (Fix64)2, (Fix64)20, (Fix64)4, 0, 1, 20, 100, (Fix64)0.5m,
                MobArchetype.KeepDistance);
            MobDefinition targetDefinition = new MobDefinition(
                Fix64.Zero, Fix64.Zero, Fix64.Zero, 0, 1, 20, 100, (Fix64)0.5m,
                MobArchetype.Dummy);
            MobAgentState source = simulation.Register(
                world.Spawn(10, new SimVector2((Fix64)2, (Fix64)5)).Id, keepDistance);
            simulation.Register(
                world.Spawn(20, new SimVector2((Fix64)6.7m, (Fix64)5)).Id,
                targetDefinition);

            SimulationInput input = simulation.PrepareFrameInputs()
                .First(value => value.EntityId == source.EntityId);

            Assert.That(input.Value, Is.EqualTo(SimVector2.Zero));
            Assert.That(source.State, Is.EqualTo(MobBehaviorState.Idle));
        }

        private static MobSimulation CreatePveBossSimulation(long seed, out MobAgentState boss)
        {
            SimulationPrefabRegistry prefabs = new SimulationPrefabRegistry(new[]
            {
                new SimulationPrefabDefinition("Boss", "Transform", "PhysicsBody"),
                new SimulationPrefabDefinition("LeafSlime", "Transform", "PhysicsBody"),
                new SimulationPrefabDefinition("WaterSlime", "Transform", "PhysicsBody"),
                new SimulationPrefabDefinition("VineSpirit", "Transform", "PhysicsBody")
            });
            SimulationWorld world = new SimulationWorld(seed, prefabs);
            MobSimulation simulation = new MobSimulation(world, new NavigationGrid(20, 10));
            boss = simulation.Register(world.Spawn("Boss", -1, At(5, 5)).Id, new MobDefinition(
                Fix64.Zero, Fix64.Zero, Fix64.Zero, 0, 1, 4, 200, (Fix64)1,
                MobArchetype.PveBoss, periodicSpawnPrefabId: "LeafSlime",
                periodicSpawnIntervalFrames: 1, periodicSpawnLimit: 0,
                secondaryPeriodicSpawnPrefabId: "WaterSlime",
                tertiaryPeriodicSpawnPrefabId: "VineSpirit", periodicSpawnBatchCount: 2,
                selectSinglePeriodicPrefab: true));
            return simulation;
        }

        private static MobSimulation CreateThunderBirdSimulation()
        {
            SimulationWorld world = new SimulationWorld(19);
            MobSimulation simulation = new MobSimulation(world, new NavigationGrid(20, 10));
            simulation.Register(world.Spawn(10, At(3, 3)).Id, new MobDefinition(
                (Fix64)2, (Fix64)20, (Fix64)7, 20, 1, 4, 100, (Fix64)0.5m,
                MobArchetype.ThunderBird));
            simulation.Register(world.Spawn(20, At(6, 3)).Id, Definition(1, 20));
            return simulation;
        }

        private static MobSimulation CreateThreeMobSimulation(out MobAgentState self, out MobAgentState lowId, out MobAgentState highId)
        {
            SimulationWorld world = new SimulationWorld(123);
            MobSimulation simulation = new MobSimulation(world, new NavigationGrid(12, 12, new[] { new GridPosition(4, 5) }));
            MobDefinition definition = Definition(attackRange: 1, cooldownFrames: 5);
            self = simulation.Register(world.Spawn(10, At(5, 5)).Id, definition);
            lowId = simulation.Register(world.Spawn(20, At(3, 5)).Id, definition);
            highId = simulation.Register(world.Spawn(20, At(7, 5)).Id, definition);
            return simulation;
        }

        private static MobDefinition Definition(int attackRange, int cooldownFrames) =>
            new MobDefinition((Fix64)2, (Fix64)20, (Fix64)attackRange, 10, cooldownFrames, 4);

        private static SimVector2 At(int x, int y) => new SimVector2((Fix64)x + (Fix64)0.5m, (Fix64)y + (Fix64)0.5m);
    }
}
