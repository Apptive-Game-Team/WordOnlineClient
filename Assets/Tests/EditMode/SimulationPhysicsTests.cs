using System;
using FixMath.NET;
using GameScene.Simulation.Core;
using GameScene.Simulation.Objects;
using GameScene.Simulation.Physics;
using NUnit.Framework;

namespace GameScene.Simulation.Tests
{
    public sealed class SimulationPhysicsTests
    {
        [Test]
        public void BoundsClampBodyAndClearOutboundVelocity()
        {
            SimulationBounds bounds = new SimulationBounds(
                new SimVector2((Fix64)(-2), (Fix64)(-2)),
                new SimVector2((Fix64)2, (Fix64)2));
            SimulationWorld world = new SimulationWorld(1, SimulationPrefabRegistry.CreateCore(), bounds);
            SimulationEntity entity = world.Spawn(1, new SimVector2((Fix64)1.4m, Fix64.Zero));

            world.Step(new[] { Velocity(1, 0, 0, 10, 0) });

            Assert.That(entity.Position.X, Is.EqualTo((Fix64)1.5m));
            Assert.That(entity.Velocity.X, Is.EqualTo(Fix64.Zero));
        }

        [Test]
        public void CollisionEventsAreCanonicalAndDeduplicated()
        {
            SimulationWorld world = new SimulationWorld(1);
            world.Spawn(2, new SimVector2((Fix64)0.4m, Fix64.Zero));
            world.Spawn(1, new SimVector2((Fix64)(-0.4m), Fix64.Zero));

            world.Step(Array.Empty<SimulationInput>());

            Assert.That(world.CollisionEvents.Count, Is.EqualTo(1));
            Assert.That(world.CollisionEvents[0].EntityA, Is.EqualTo(0));
            Assert.That(world.CollisionEvents[0].EntityB, Is.EqualTo(1));
            Assert.That(world.CollisionEvents[0].Type, Is.EqualTo(SimulationCollisionEventType.Enter));
        }

        [Test]
        public void TriggerReportsContactWithoutPhysicalResponse()
        {
            SimulationPhysicsDefinition triggerPhysics = new SimulationPhysicsDefinition(
                (Fix64)0.5m, Fix64.One, true);
            SimulationPrefabRegistry registry = new SimulationPrefabRegistry(new[]
            {
                new SimulationPrefabDefinition("Trigger", triggerPhysics, "Transform", "PhysicsBody")
            });
            SimulationWorld world = new SimulationWorld(1, registry);
            SimulationEntity left = world.Spawn("Trigger", 1, new SimVector2((Fix64)(-0.4m), Fix64.Zero));
            SimulationEntity right = world.Spawn("Trigger", 2, new SimVector2((Fix64)0.4m, Fix64.Zero));

            world.Step(Array.Empty<SimulationInput>());

            Assert.That(world.CollisionEvents.Count, Is.EqualTo(1));
            Assert.That(world.CollisionEvents[0].IsTrigger, Is.True);
            Assert.That(left.Position.X, Is.EqualTo((Fix64)(-0.4m)));
            Assert.That(right.Position.X, Is.EqualTo((Fix64)0.4m));
        }

        [Test]
        public void FullBodyCheckpointIsExposedWithoutUnityTransform()
        {
            SimulationWorld world = new SimulationWorld(1);
            SimulationEntity entity = world.Spawn(1, SimVector2.Zero);
            world.Step(new[] { Velocity(1, 0, 0, 3, 4) });

            Assert.That(entity.Position3D.X, Is.EqualTo(entity.Position.X));
            Assert.That(entity.LinearVelocity.X, Is.EqualTo(entity.Velocity.X));
            Assert.That(entity.AngularVelocity, Is.EqualTo(new SimVector3(Fix64.Zero, Fix64.Zero, Fix64.Zero)));
            Assert.That(entity.Orientation.W, Is.Not.EqualTo(Fix64.Zero));
        }

        [Test]
        public void CrossRunReplayHashesMatchAtEveryPhysicsFrame()
        {
            SimulationWorld first = ReplayWorld();
            SimulationWorld second = ReplayWorld();
            for (int frame = 0; frame < 80; frame++)
            {
                SimulationInput[] inputs = frame == 0
                    ? new[] { Velocity(1, 0, 0, 2, 0), Velocity(2, 0, 1, -2, 0) }
                    : Array.Empty<SimulationInput>();
                first.Step(inputs);
                second.Step(inputs);
                Assert.That(second.CalculateStateHash(), Is.EqualTo(first.CalculateStateHash()), "frame " + frame);
            }
        }

        private static SimulationWorld ReplayWorld()
        {
            SimulationWorld world = new SimulationWorld(55);
            world.Spawn(1, new SimVector2((Fix64)(-2), Fix64.Zero));
            world.Spawn(2, new SimVector2((Fix64)2, Fix64.Zero));
            return world;
        }

        private static SimulationInput Velocity(long userId, int sequence, int entityId, int x, int y)
        {
            return new SimulationInput(
                userId, sequence, SimulationInputType.SetVelocity, entityId,
                new SimVector2((Fix64)x, (Fix64)y));
        }
    }
}
