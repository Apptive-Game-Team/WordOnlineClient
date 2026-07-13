using System;
using System.Linq;
using FixMath.NET;
using GameScene.Simulation.Core;
using GameScene.Simulation.Mob;
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
