using System;
using GameScene.Simulation.Core;
using GameScene.Simulation.Objects;
using GameScene.Simulation.Protocol;
using NUnit.Framework;

namespace GameScene.Simulation.Tests
{
    public sealed class SimulationObjectLifecycleTests
    {
        [Test]
        public void BootstrapSequenceDeterminesObjectIdsAndHash()
        {
            SimulationWorld ascending = new SimulationWorld(7);
            SimulationWorld descending = new SimulationWorld(7);
            BootstrapEventMessage left = Player(10, -4);
            BootstrapEventMessage right = Player(20, 4);

            ascending.ApplyBootstrap(Session(left, right));
            descending.ApplyBootstrap(Session(right, left));

            Assert.That(descending.Entities[0].Position, Is.EqualTo(ascending.Entities[0].Position));
            Assert.That(descending.Entities[1].Position, Is.EqualTo(ascending.Entities[1].Position));
            Assert.That(descending.CalculateStateHash(), Is.EqualTo(ascending.CalculateStateHash()));
        }

        [Test]
        public void DuplicateBootstrapSequenceFailsBeforeSpawning()
        {
            SimulationWorld world = new SimulationWorld(7);
            Assert.Throws<InvalidOperationException>(() =>
                world.ApplyBootstrap(Session(Player(10, -4), Player(10, 4))));
            Assert.That(world.Entities, Is.Empty);
        }

        [Test]
        public void DestroyIsIdempotentAndDestroysComponentsInDefinitionOrder()
        {
            SimulationWorld world = new SimulationWorld(7);
            SimulationEntity entity = world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 11, SimVector2.Zero);

            Assert.That(world.Destroy(entity.Id), Is.True);
            ulong destroyedHash = world.CalculateStateHash();
            Assert.That(world.Destroy(entity.Id), Is.False);

            Assert.That(world.CalculateStateHash(), Is.EqualTo(destroyedHash));
            Assert.That(entity.DestroyFrame, Is.EqualTo(0));
            Assert.That(entity.Components[0].Id, Is.EqualTo("Transform"));
            Assert.That(entity.Components[1].Id, Is.EqualTo("PhysicsBody"));
            Assert.That(entity.Components[2].Id, Is.EqualTo("PlayerState"));
            Assert.That(entity.Components[2].IsDestroyed, Is.True);
        }

        [Test]
        public void SpawnWithSameIdAndDefinitionIsIdempotent()
        {
            SimulationWorld world = new SimulationWorld(7);
            SimulationEntity first = world.SpawnWithId(0, SimulationPrefabRegistry.PlayerPrefabId, 11, SimVector2.Zero);
            ulong spawnedHash = world.CalculateStateHash();

            SimulationEntity replay = world.SpawnWithId(0, SimulationPrefabRegistry.PlayerPrefabId, 11, SimVector2.Zero);

            Assert.That(replay, Is.SameAs(first));
            Assert.That(world.Entities.Count, Is.EqualTo(1));
            Assert.That(world.CalculateStateHash(), Is.EqualTo(spawnedHash));
            Assert.Throws<InvalidOperationException>(() =>
                world.SpawnWithId(0, SimulationPrefabRegistry.DefaultPrefabId, 11, SimVector2.Zero));
        }

        [Test]
        public void MissingPrefabFailsBeforeAllocatingObjectId()
        {
            SimulationWorld world = new SimulationWorld(7);
            Assert.Throws<InvalidOperationException>(() => world.Spawn("Missing", 11, SimVector2.Zero));
            Assert.That(world.Spawn(11, SimVector2.Zero).Id, Is.EqualTo(0));
        }

        [Test]
        public void LifecycleGoldenHashIsStable()
        {
            SimulationWorld world = new SimulationWorld(19);
            world.ApplyBootstrap(Session(Player(2, 3), Player(1, -3)));
            world.Destroy(0);

            Assert.That(world.CalculateStateHash(), Is.EqualTo(0x6105ABA9BF4C4B9BUL));
        }

        private static BootstrapEventMessage Player(int sequence, float x)
        {
            return new BootstrapEventMessage
            {
                sequence = sequence,
                type = "SPAWN_PLAYER",
                master = x < 0 ? "LeftPlayer" : "RightPlayer",
                position = new ProtocolVector3 { x = x, y = 0, z = 0 }
            };
        }

        private static LockstepSessionStartMessage Session(params BootstrapEventMessage[] events)
        {
            return new LockstepSessionStartMessage
            {
                leftUserId = 11,
                rightUserId = 22,
                bootstrapEvents = events
            };
        }
    }
}
