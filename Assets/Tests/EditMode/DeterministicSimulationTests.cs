using System;
using System.Collections.Generic;
using NUnit.Framework;
using GameScene.Simulation.Core;
using FixMath.NET;

namespace GameScene.Simulation.Tests
{
    public sealed class DeterministicSimulationTests
    {
        [Test]
        public void SameSeedAndConfirmedInputsProduceSameHashAtEveryFrame()
        {
            SimulationWorld left = CreateWorld(9123456789L);
            SimulationWorld right = CreateWorld(9123456789L);

            for (int frame = 0; frame < 120; frame++)
            {
                IReadOnlyList<SimulationInput> inputs = FixtureInputs(frame);
                left.Step(inputs);
                right.Step(inputs);
                Assert.That(right.CalculateStateHash(), Is.EqualTo(left.CalculateStateHash()), "frame " + frame);
            }

            Assert.That(left.CalculateStateHash(), Is.EqualTo(0x0E5CF22FA41895DBUL));

        }

        [Test]
        public void ArrivalOrderDoesNotChangeCanonicalInputApplication()
        {
            SimulationWorld ascending = CreateWorld(42);
            SimulationWorld shuffled = CreateWorld(42);
            List<SimulationInput> inputs = new List<SimulationInput>
            {
                Velocity(20, 1, 1, 2, 0),
                Velocity(10, 1, 0, 1, 0),
                Velocity(10, 2, 0, 3, 0)
            };

            ascending.Step(inputs);
            inputs.Reverse();
            shuffled.Step(inputs);

            Assert.That(shuffled.CalculateStateHash(), Is.EqualTo(ascending.CalculateStateHash()));
        }

        [Test]
        public void WorldLocalEntityIdsDoNotInterfereAcrossWorlds()
        {
            SimulationWorld first = new SimulationWorld(1);
            SimulationWorld second = new SimulationWorld(1);

            Assert.That(first.Spawn(10, SimVector2.Zero).Id, Is.EqualTo(0));
            Assert.That(second.Spawn(10, SimVector2.Zero).Id, Is.EqualTo(0));
            Assert.That(first.CalculateStateHash(), Is.EqualTo(second.CalculateStateHash()));
        }

        [Test]
        public void RandomDrawCountAndStateAreHashed()
        {
            SimulationWorld untouched = CreateWorld(99);
            SimulationWorld advanced = CreateWorld(99);

            advanced.DrawRandomUInt32();

            Assert.That(advanced.RandomDrawCount, Is.EqualTo(1));
            Assert.That(advanced.CalculateStateHash(), Is.Not.EqualTo(untouched.CalculateStateHash()));
        }

        [Test]
        public void DuplicateUserSequenceIsRejectedBeforeSimulationAdvances()
        {
            SimulationWorld world = CreateWorld(42);
            SimulationInput[] inputs =
            {
                Velocity(10, 7, 0, 1, 0),
                Velocity(10, 7, 0, 2, 0)
            };

            Assert.Throws<InvalidOperationException>(() => world.Step(inputs));
            Assert.That(world.FrameNumber, Is.EqualTo(0));
        }

        [Test]
        public void PhysicsCollisionProducesSameFinalPositionAndVelocity()
        {
            SimulationWorld left = CreateWorld(777);
            SimulationWorld right = CreateWorld(777);

            for (int frame = 0; frame < 100; frame++)
            {
                SimulationInput[] inputs = frame == 0
                    ? new[] { Velocity(10, 0, 0, 2, 0), Velocity(20, 0, 1, -2, 0) }
                    : Array.Empty<SimulationInput>();
                left.Step(inputs);
                right.Step(inputs);
            }

            Assert.That(right.Entities[0].Position, Is.EqualTo(left.Entities[0].Position));
            Assert.That(right.Entities[0].Velocity, Is.EqualTo(left.Entities[0].Velocity));
            Assert.That(right.CalculateStateHash(), Is.EqualTo(left.CalculateStateHash()));
        }

        private static SimulationWorld CreateWorld(long seed)
        {
            SimulationWorld world = new SimulationWorld(seed);
            world.Spawn(10, new SimVector2((Fix64)(-4), Fix64.Zero));
            world.Spawn(20, new SimVector2((Fix64)4, Fix64.Zero));
            return world;
        }

        private static IReadOnlyList<SimulationInput> FixtureInputs(int frame)
        {
            if (frame == 40)
            {
                return new[]
                {
                    new SimulationInput(10, frame, SimulationInputType.Spawn, -1,
                        new SimVector2((Fix64)(-2), Fix64.One))
                };
            }

            if (frame % 20 == 0)
            {
                return new[]
                {
                    Velocity(20, frame, 1, -1, 0),
                    Velocity(10, frame, 0, 1, 0)
                };
            }

            return Array.Empty<SimulationInput>();
        }

        private static SimulationInput Velocity(long userId, int sequence, int entityId, int x, int y)
        {
            return new SimulationInput(
                userId,
                sequence,
                SimulationInputType.SetVelocity,
                entityId,
                new SimVector2((Fix64)x, (Fix64)y));
        }
    }
}
