using System;
using System.Collections.Generic;
using FixMath.NET;
using GameScene.Simulation.Core;
using GameScene.Simulation.Protocol;
using GameScene.Simulation.Resources;
using NUnit.Framework;

namespace GameScene.Simulation.Tests
{
    public sealed class SimulationSnapshotTests
    {
        [Test]
        public void WorldSnapshotIsDetachedFromLaterSimulationSteps()
        {
            SimulationWorld world = new SimulationWorld(1);
            world.Spawn(10, SimVector2.Zero);
            SimulationSnapshot snapshot = world.CreateSnapshot();

            world.Step(new[]
            {
                new SimulationInput(10, 0, SimulationInputType.SetVelocity, 0,
                    new SimVector2((Fix64)5, Fix64.Zero))
            });

            Assert.That(snapshot.FrameNumber, Is.EqualTo(0));
            Assert.That(snapshot.Entities[0].Position.X, Is.EqualTo(Fix64.Zero));
            Assert.That(snapshot.Entities[0].LinearVelocity.X, Is.EqualTo(Fix64.Zero));
            Assert.That(world.CreateSnapshot().Entities[0].Position.X, Is.Not.EqualTo(Fix64.Zero));
        }

        [Test]
        public void PlayerSnapshotCopiesHandAndRejectsUnknownUser()
        {
            ResourceSimulation resources = new ResourceSimulation(
                new DeterministicGameRules(
                    10, 1, 1, 1, 1, 5,
                    new Dictionary<string, int> { ["Fire"] = 1 },
                    new[] { new[] { "Fire" } }),
                10, 20, new[] { "Fire" }, new[] { "Water" });
            resources.Step(new ConfirmedFrameMessage
            {
                frameNum = 1,
                inputs = Array.Empty<ConfirmedInputMessage>()
            });

            PlayerResourceSnapshot snapshot = resources.CreatePlayerSnapshot(10);

            Assert.That(snapshot.Mana, Is.EqualTo(1));
            Assert.That(snapshot.Hand, Is.EqualTo(new[] { "Fire" }));
            Assert.Throws<InvalidOperationException>(() => resources.CreatePlayerSnapshot(999));
        }
    }
}
