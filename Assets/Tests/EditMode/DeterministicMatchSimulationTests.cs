using System.Collections.Generic;
using System.Linq;
using FixMath.NET;
using GameScene.Simulation.Core;
using GameScene.Simulation.Magic;
using GameScene.Simulation.Mob;
using GameScene.Simulation.Objects;
using GameScene.Simulation.Protocol;
using GameScene.Simulation.Resources;
using NUnit.Framework;

namespace GameScene.Simulation.Tests
{
    public sealed class DeterministicMatchSimulationTests
    {
        [Test]
        public void ResourceMagicAndMobShareExactlyOneWorldStepPerConfirmedFrame()
        {
            DeterministicMatchSimulation match = Create(out MobAgentState leftMob, out MobAgentState rightMob);
            match.Step(EmptyFrame(1), System.Array.Empty<MagicCastCommand>());
            ConfirmedFrameMessage frame = Frame(2, 10, 7, "Fire", "Spawn");
            MagicCastCommand cast = new MagicCastCommand(10, 7, "fire_spirit", new SimVector2((Fix64)5, (Fix64)5));

            match.Step(frame, new[] { cast });

            Assert.That(match.World.FrameNumber, Is.EqualTo(2));
            Assert.That(match.Resources.FrameNumber, Is.EqualTo(2));
            Assert.That(match.World.Entities.Count, Is.EqualTo(3));
            Assert.That(match.World.Entities[2].PrefabId, Is.EqualTo("FireSpirit"));
            Assert.That(leftMob.State, Is.Not.EqualTo(MobBehaviorState.Dead));
            Assert.That(rightMob.State, Is.Not.EqualTo(MobBehaviorState.Dead));
        }

        [Test]
        public void RejectedResourceInputDoesNotCastMagic()
        {
            DeterministicMatchSimulation match = Create(out _, out _);
            match.Step(EmptyFrame(1), System.Array.Empty<MagicCastCommand>());
            ConfirmedFrameMessage frame = Frame(2, 10, 9, "Water", "Spawn");
            MagicCastCommand cast = new MagicCastCommand(10, 9, "fire_spirit", new SimVector2((Fix64)5, (Fix64)5));

            match.Step(frame, new[] { cast });

            Assert.That(match.Resources.LastCastResults[0].Code, Is.EqualTo(CastResultCode.MissingCard));
            Assert.That(match.World.Entities.Count, Is.EqualTo(2));
        }

        [Test]
        public void SpawnedSummonRegistersAsMobForFollowingFrame()
        {
            DeterministicMatchSimulation match = Create(out _, out _, new FireSpiritResolver());
            match.Step(EmptyFrame(1), System.Array.Empty<MagicCastCommand>());

            match.Step(Frame(2, 10, 10, "Fire", "Spawn"), new[]
            {
                new MagicCastCommand(10, 10, "fire_spirit", new SimVector2((Fix64)5, (Fix64)5))
            });

            Assert.That(match.Mobs.Agents.ContainsKey(2), Is.True);
            Assert.That(match.Mobs.Agents[2].State, Is.EqualTo(MobBehaviorState.Idle));

            match.Step(EmptyFrame(3), System.Array.Empty<MagicCastCommand>());
            Assert.That(match.Mobs.Agents[2].TargetEntityId, Is.EqualTo(1));
        }

        [Test]
        public void DelayedMagicAndMobHealthSynchronizeOnExplosionFrame()
        {
            DeterministicMatchSimulation match = Create(out _, out _);
            match.Step(EmptyFrame(1), System.Array.Empty<MagicCastCommand>());

            match.Step(Frame(2, 10, 11, "Fire", "Spawn"), new[]
            {
                new MagicCastCommand(10, 11, "magma_explosion", new SimVector2((Fix64)8, (Fix64)5))
            });
            for (int frame = 3; frame <= 12; frame++)
                match.Step(EmptyFrame(frame), System.Array.Empty<MagicCastCommand>());

            Assert.That(match.Magic.Combatants[1].Health, Is.EqualTo(90));
            Assert.That(match.Mobs.Agents[1].Health, Is.EqualTo(90));
            Assert.That(match.Resources.Right.Health, Is.EqualTo(90));
        }

        [Test]
        public void MatchSnapshotCarriesServerStatusEffectsAndHpGauge()
        {
            DeterministicMatchSimulation match = Create(out _, out _);
            match.Step(Frame(1, 10, 12, "Fire", "Spawn"), new[]
            {
                new MagicCastCommand(10, 12, "fire_spirit", new SimVector2((Fix64)5, (Fix64)5))
            });
            match.Magic.ApplyExternalStatus(1, "burn", 60);

            SimulationSnapshot snapshot = match.CreateSnapshot();

            Assert.That(snapshot.Entities[0].Status, Is.EqualTo("Attack"));
            Assert.That(snapshot.Entities[0].Effects, Contains.Item("FireAttackAura"));
            Assert.That(snapshot.Entities[1].Effects, Contains.Item("Burn"));
            Assert.That(snapshot.Entities[1].Gauges[0].Category, Is.EqualTo("HP"));
            Assert.That(snapshot.Entities[1].Gauges[0].Value, Is.EqualTo(100));
        }

        [Test]
        public void AttackAuraExpiresAfterServerPointThreeSecondDuration()
        {
            DeterministicMatchSimulation match = Create(out _, out _);
            match.Step(Frame(1, 10, 12, "Fire", "Spawn"), new[]
            {
                new MagicCastCommand(10, 12, "fire_spirit", new SimVector2((Fix64)5, (Fix64)5))
            });
            Assert.That(match.CreateSnapshot().Entities[0].Effects,
                Contains.Item("FireAttackAura"));

            for (int frame = 2; frame <= 7; frame++)
                match.Step(EmptyFrame(frame), System.Array.Empty<MagicCastCommand>());

            Assert.That(match.CreateSnapshot().Entities[0].Effects,
                Does.Not.Contain("FireAttackAura"));
        }

        [Test]
        public void ConfirmedCardSelectionProjectsRemoteIdleAuraAndHashesIt()
        {
            DeterministicMatchSimulation first = Create(out _, out _);
            DeterministicMatchSimulation second = Create(out _, out _);
            ConfirmedFrameMessage selected = new ConfirmedFrameMessage
            {
                frameNum = 1,
                inputs = new[]
                {
                    new ConfirmedInputMessage
                    {
                        userId = 20,
                        input = new FrameInputMessage
                        {
                            sequence = 0,
                            type = "setCardSelection",
                            cards = new[] { "Water", "Spawn" }
                        }
                    }
                }
            };

            first.Step(selected, System.Array.Empty<MagicCastCommand>());
            second.Step(selected, System.Array.Empty<MagicCastCommand>());

            Assert.That(first.CreateSnapshot().Entities[1].Effects,
                Contains.Item("WaterIdleAura"));
            Assert.That(second.CalculateStateHash(), Is.EqualTo(first.CalculateStateHash()));
        }

        [Test]
        public void EveryServerMagicRunsDeterministicallyThroughFullProductionMatchPipeline()
        {
            IReadOnlyList<MagicParameterValue> magicParameters =
                MagicSimulationTests.CompleteMagicParameters();
            IReadOnlyList<MobParameterValue> mobParameters =
                ProductionMobDefinitionResolverTests.Parameters();
            foreach (ServerMagicDescriptor descriptor in ServerMagicManifest.All)
            {
                DeterministicMatchSimulation first = CreateProduction(magicParameters, mobParameters);
                DeterministicMatchSimulation second = CreateProduction(magicParameters, mobParameters);
                for (int frame = 1; frame <= 80; frame++)
                {
                    long userId = descriptor.StableId % 2 == 0 ? 10 : 20;
                    ConfirmedFrameMessage confirmed = frame == 1
                        ? Frame(frame, userId, descriptor.StableId)
                        : EmptyFrame(frame);
                    IReadOnlyList<MagicCastCommand> casts = frame == 1
                        ? new[] { new MagicCastCommand(userId, descriptor.StableId,
                            descriptor.Name, new SimVector2((Fix64)5, (Fix64)5)) }
                        : System.Array.Empty<MagicCastCommand>();
                    first.Step(confirmed, casts);
                    second.Step(confirmed, casts);
                    Assert.That(second.CalculateStateHash(), Is.EqualTo(first.CalculateStateHash()),
                        descriptor.Name + " frame " + frame);
                    if (first.Resources.Result != SimulationResult.Ongoing) break;
                }
            }
        }

        [Test]
        public void EveryServerPrefabRunsDeterministicallyThroughProductionLifecycle()
        {
            IReadOnlyList<MagicParameterValue> magicParameters =
                MagicSimulationTests.CompleteMagicParameters();
            IReadOnlyList<MobParameterValue> mobParameters =
                ProductionMobDefinitionResolverTests.Parameters();
            foreach (string prefabId in ServerPrefabManifest.All)
            {
                DeterministicMatchSimulation first = CreateProductionWithPrefab(
                    magicParameters, mobParameters, prefabId);
                DeterministicMatchSimulation second = CreateProductionWithPrefab(
                    magicParameters, mobParameters, prefabId);

                Assert.That(first.World.Entities.Any(entity => entity.PrefabId == prefabId), Is.True,
                    prefabId + " spawn");
                for (int frame = 1; frame <= 40; frame++)
                {
                    ConfirmedFrameMessage confirmed = EmptyFrame(frame);
                    first.Step(confirmed, System.Array.Empty<MagicCastCommand>());
                    second.Step(confirmed, System.Array.Empty<MagicCastCommand>());
                    Assert.That(second.CalculateStateHash(), Is.EqualTo(first.CalculateStateHash()),
                        prefabId + " frame " + frame);
                    if (first.Resources.Result != SimulationResult.Ongoing) break;
                }
            }
        }

        private static DeterministicMatchSimulation Create(out MobAgentState leftMob, out MobAgentState rightMob,
            IMobDefinitionResolver resolver = null)
        {
            SimulationWorld world = new SimulationWorld(123, SimulationPrefabRegistry.CreateProduction());
            SimulationEntity left = world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 10,
                new SimVector2((Fix64)2, (Fix64)5));
            SimulationEntity right = world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 20,
                new SimVector2((Fix64)8, (Fix64)5));
            ResourceSimulation resources = new ResourceSimulation(
                new DeterministicGameRules(10, 6, 1, 10, 1, 100,
                    new Dictionary<string, int> { ["Fire"] = 1, ["Spawn"] = 1, ["Water"] = 1 },
                    new[] { new[] { "Fire", "Spawn" } }),
                10, 20, new[] { "Fire", "Spawn" }, new[] { "Fire", "Spawn" });
            MagicSimulation magic = new MagicSimulation(456, world, ProductionMagicCatalog.Create());
            MobSimulation mobs = new MobSimulation(world, new NavigationGrid(18, 10));
            MobDefinition definition = new MobDefinition((Fix64)2, (Fix64)20, (Fix64)1, 10, 4, 4);
            leftMob = mobs.Register(left.Id, definition);
            rightMob = mobs.Register(right.Id, definition);
            return new DeterministicMatchSimulation(resources, magic, mobs, null, resolver);
        }

        private static DeterministicMatchSimulation CreateProduction(
            IReadOnlyList<MagicParameterValue> magicParameters,
            IReadOnlyList<MobParameterValue> mobParameters)
        {
            List<SimulationPrefabParameterValue> prefabParameters = magicParameters.Select(value =>
                new SimulationPrefabParameterValue(value.ObjectName, value.ParameterName, value.Value)).ToList();
            SimulationWorld world = new SimulationWorld(123,
                SimulationPrefabRegistry.CreateProduction(prefabParameters));
            SimulationEntity left = world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 10,
                new SimVector2((Fix64)2, (Fix64)5));
            SimulationEntity right = world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 20,
                new SimVector2((Fix64)8, (Fix64)5));
            ResourceSimulation resources = new ResourceSimulation(
                new DeterministicGameRules(0, 1, 1, 0, 1, 200,
                    new Dictionary<string, int>(), new[] { System.Array.Empty<string>() }),
                10, 20, System.Array.Empty<string>(), System.Array.Empty<string>());
            MagicSimulation magic = new MagicSimulation(456, world,
                ProductionMagicCatalog.Create(magicParameters));
            MobSimulation mobs = new MobSimulation(world, new NavigationGrid(18, 10));
            ProductionMobDefinitionResolver resolver = new ProductionMobDefinitionResolver(mobParameters);
            MobDefinition player = new MobDefinition((Fix64)2, (Fix64)20, (Fix64)1,
                2, 20, 4, 100, (Fix64)0.5m);
            mobs.Register(left.Id, player);
            mobs.Register(right.Id, player);
            return new DeterministicMatchSimulation(resources, magic, mobs, null, resolver);
        }

        private static DeterministicMatchSimulation CreateProductionWithPrefab(
            IReadOnlyList<MagicParameterValue> magicParameters,
            IReadOnlyList<MobParameterValue> mobParameters,
            string prefabId)
        {
            DeterministicMatchSimulation match = CreateProduction(magicParameters, mobParameters);
            match.World.Spawn(prefabId, 10, new SimVector2((Fix64)5, (Fix64)5));
            match.Mobs.RegisterWorldEntities(new ProductionMobDefinitionResolver(mobParameters));
            return match;
        }

        private sealed class FireSpiritResolver : IMobDefinitionResolver
        {
            public bool TryResolve(string prefabId, out MobDefinition definition)
            {
                if (prefabId == "FireSpirit")
                {
                    definition = new MobDefinition((Fix64)2, (Fix64)20, (Fix64)1, 10, 4, 4);
                    return true;
                }
                definition = null;
                return false;
            }
        }

        private static ConfirmedFrameMessage Frame(int frameNumber, long userId, int requestId, params string[] cards) =>
            new ConfirmedFrameMessage
            {
                frameNum = frameNumber,
                inputs = new[]
                {
                    new ConfirmedInputMessage
                    {
                        userId = userId,
                        input = new FrameInputMessage { id = requestId, type = "useMagic", cards = cards }
                    }
                }
            };

        private static ConfirmedFrameMessage EmptyFrame(int frameNumber) =>
            new ConfirmedFrameMessage { frameNum = frameNumber, inputs = System.Array.Empty<ConfirmedInputMessage>() };
    }
}
