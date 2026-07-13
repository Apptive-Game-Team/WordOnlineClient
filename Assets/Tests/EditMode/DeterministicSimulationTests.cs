using System;
using System.Collections.Generic;
using NUnit.Framework;
using WordOnline.Simulation.Core;

namespace WordOnline.Simulation.Tests
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

            Assert.That(left.CalculateStateHash(), Is.EqualTo(0x63C7C0A7E94932E7UL));
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
        public void FixedPointOverflowFailsInsteadOfWrapping()
        {
            Fixed64 maximum = Fixed64.FromRaw(long.MaxValue);
            Assert.Throws<OverflowException>(() => Ignore(maximum + Fixed64.One));
        }

        private static SimulationWorld CreateWorld(long seed)
        {
            SimulationWorld world = new SimulationWorld(seed);
            world.Spawn(10, new SimVector2(Fixed64.FromInt(-4), Fixed64.Zero));
            world.Spawn(20, new SimVector2(Fixed64.FromInt(4), Fixed64.Zero));
            return world;
        }

        private static IReadOnlyList<SimulationInput> FixtureInputs(int frame)
        {
            if (frame % 20 == 0)
            {
                return new[]
                {
                    Velocity(20, frame, 1, -1, 0),
                    Velocity(10, frame, 0, 1, 0)
                };
            }

            if (frame == 40)
            {
                return new[]
                {
                    new SimulationInput(10, frame, SimulationInputType.Spawn, -1,
                        new SimVector2(Fixed64.FromInt(-2), Fixed64.FromInt(1)))
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
                new SimVector2(Fixed64.FromInt(x), Fixed64.FromInt(y)));
        }

        private static void Ignore(Fixed64 value)
        {
        }
    }
}
