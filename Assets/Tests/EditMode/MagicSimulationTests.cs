using System;
using System.Collections.Generic;
using System.Linq;
using FixMath.NET;
using GameScene.Simulation.Core;
using GameScene.Simulation.Magic;
using GameScene.Simulation.Objects;
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
            int[] expected = Enumerable.Range(1, 68).Where(value => value != 34).ToArray();

            Assert.That(ids, Is.EqualTo(expected));
            Assert.That(catalog.Definitions.Select(value => value.Name).Distinct().Count(), Is.EqualTo(67));
        }

        [Test]
        public void SpawnFamilyUsesStablePerCastRandomStream()
        {
            MagicSimulation ascending = Create();
            MagicSimulation descending = Create();
            MagicCastCommand left = Cast(10, 4, 1, 5, 5);
            MagicCastCommand right = Cast(20, 2, 1, 7, 5);

            ascending.Step(new[] { left, right });
            descending.Step(new[] { right, left });

            Assert.That(descending.CalculateStateHash(), Is.EqualTo(ascending.CalculateStateHash()));
            Assert.That(ascending.World.Entities.Count, Is.EqualTo(8));
        }

        [Test]
        public void AreaFamilyAppliesDamageAndTimedStatusInEntityOrder()
        {
            MagicSimulation simulation = Create();

            simulation.Step(new[] { Cast(10, 1, 19, 7, 5) });

            Assert.That(simulation.Combatants[1].Health, Is.EqualTo(90));
            Assert.That(simulation.Combatants[1].Statuses["area-hit"], Is.EqualTo(19));
            Assert.That(simulation.Combatants[0].Health, Is.EqualTo(100));
        }

        [Test]
        public void ProjectileFamilyDamagesEnemyAndTimedFamiliesExpire()
        {
            MagicSimulation projectile = Create();
            projectile.Step(new[] { Cast(10, 1, 7, 7, 5) });
            for (int frame = 0; frame < 20; frame++) projectile.Step(Array.Empty<MagicCastCommand>());
            Assert.That(projectile.Combatants[1].Health, Is.LessThan(100));

            MagicSimulation build = Create();
            build.Step(new[] { Cast(10, 1, 14, 5, 5), Cast(20, 1, 24, 7, 5) });
            int spawnedCount = build.World.Entities.Count;
            for (int frame = 0; frame < 40; frame++) build.Step(Array.Empty<MagicCastCommand>());
            Assert.That(build.World.Entities.Count, Is.EqualTo(spawnedCount));
            Assert.That(build.World.Entities.Skip(2).All(entity => entity.IsDestroyed), Is.True);
        }

        [Test]
        public void FullMagicReplayProducesSameHashAtEveryFrame()
        {
            MagicSimulation first = Create();
            MagicSimulation second = Create();
            for (int frame = 0; frame < 80; frame++)
            {
                IReadOnlyList<MagicCastCommand> casts = frame == 0
                    ? new[] { Cast(20, 9, 19, 5, 5), Cast(10, 3, 1, 5, 5), Cast(10, 4, 7, 7, 5) }
                    : Array.Empty<MagicCastCommand>();
                first.Step(casts); second.Step(casts);
                Assert.That(second.CalculateStateHash(), Is.EqualTo(first.CalculateStateHash()), "frame " + frame);
            }
        }

        private static MagicSimulation Create()
        {
            SimulationWorld world = new SimulationWorld(123);
            world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 10, new SimVector2((Fix64)2, (Fix64)5));
            world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 20, new SimVector2((Fix64)7, (Fix64)5));
            return new MagicSimulation(987654321, world, ProductionMagicCatalog.Create());
        }

        private static MagicCastCommand Cast(long userId, int requestId, int magicId, int x, int y) =>
            new MagicCastCommand(userId, requestId, magicId, new SimVector2((Fix64)x, (Fix64)y));
    }
}
