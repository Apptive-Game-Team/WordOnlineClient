using System;
using System.Collections.Generic;
using FixMath.NET;
using GameScene.Simulation.Protocol;
using GameScene.Simulation.Resources;
using NUnit.Framework;

namespace GameScene.Simulation.Tests
{
    public class ResourceSimulationTests
    {
        [Test]
        public void TwinWorldsKeepSameHashAcrossDrawManaCastAndResult()
        {
            ResourceSimulation first = Create(); ResourceSimulation second = Create();
            for (int frame = 1; frame <= 4; frame++)
            {
                ConfirmedFrameMessage input = frame == 2 ? Frame(frame, Cast(10, 7, "Fire")) : Frame(frame);
                first.Step(input); second.Step(input);
                Assert.That(first.CalculateStateHash(), Is.EqualTo(second.CalculateStateHash()));
            }
            Assert.That(first.Result, Is.EqualTo(SimulationResult.Draw));
        }

        [Test]
        public void ValidCastSpendsManaConsumesHandAndReturnsCardToDeck()
        {
            ResourceSimulation simulation = Create();
            simulation.Step(Frame(1));
            Assert.That(simulation.Left.Hand, Is.EqualTo(new[] { "Fire", "Water" }));
            simulation.Step(Frame(2, Cast(10, 12, "Fire")));
            Assert.That(simulation.LastCastResults[0].Code, Is.EqualTo(CastResultCode.Success));
            Assert.That(simulation.Left.Mana, Is.EqualTo(25));
            Assert.That(simulation.Left.Hand, Is.EqualTo(new[] { "Water" }));
        }

        [TestCase(0, 100, SimulationResult.RightWin)]
        [TestCase(100, 0, SimulationResult.LeftWin)]
        [TestCase(0, 0, SimulationResult.Draw)]
        public void CombatHealthDeterminesResult(int leftHealth, int rightHealth, SimulationResult expected)
        {
            ResourceSimulation simulation = Create();

            simulation.ResolveCombatHealth(leftHealth, rightHealth);

            Assert.That(simulation.Left.Health, Is.EqualTo(leftHealth));
            Assert.That(simulation.Right.Health, Is.EqualTo(rightHealth));
            Assert.That(simulation.Result, Is.EqualTo(expected));
        }

        [TestCase("Nature", CastResultCode.MissingCard)]
        [TestCase("Water", CastResultCode.InvalidMagic)]
        public void InvalidCastMatchesServerCardConsumptionPolicy(string card, CastResultCode expected)
        {
            ResourceSimulation simulation = Create(); simulation.Step(Frame(1));
            int mana = simulation.Left.Mana; ulong hash = simulation.CalculateStateHash();
            simulation.Step(Frame(2, Cast(10, 1, card)));
            Assert.That(simulation.LastCastResults[0].Code, Is.EqualTo(expected));
            int chargedMana = Math.Min(100, mana + 10);
            Assert.That(simulation.Left.Mana, Is.EqualTo(expected == CastResultCode.InvalidMagic
                ? chargedMana - 5 : chargedMana));
            Assert.That(simulation.Left.Hand, Does.Contain("Fire"));
            if (expected == CastResultCode.InvalidMagic) Assert.That(simulation.Left.Hand, Does.Not.Contain("Water"));
            Assert.That(simulation.CalculateStateHash(), Is.Not.EqualTo(hash));
        }

        [Test]
        public void ManaChargePercentUsesServerStatFloorSemantics()
        {
            ResourceSimulation simulation = Create();
            simulation.SetManaChargePercent(10, (Fix64)0.8m);
            simulation.SetManaChargePercent(20, (Fix64)1.6m);

            simulation.Step(Frame(1));

            Assert.That(simulation.Left.Mana, Is.EqualTo(28));
            Assert.That(simulation.Right.Mana, Is.EqualTo(36));
        }

        [Test]
        public void FeverDoublesManaAndHalvesCardDrawIntervalAfterStrictThreshold()
        {
            var rules = new DeterministicGameRules(100, 6, 1, 1, 2, 5,
                new Dictionary<string, int>(), new[] { Array.Empty<string>() }, 3);
            ResourceSimulation simulation = new ResourceSimulation(rules, 10, 20,
                new[] { "Fire", "Water", "Nature", "Rock" }, Array.Empty<string>());

            for (int frame = 1; frame <= 4; frame++) simulation.Step(Frame(frame));

            Assert.That(simulation.Left.Mana, Is.EqualTo(6));
            Assert.That(simulation.Left.Hand,
                Is.EqualTo(new[] { "Fire", "Water", "Nature" }));
        }

        [Test]
        public void TimeoutUsesPreTerminalFrameHpInsteadOfAlwaysDrawing()
        {
            var rules = new DeterministicGameRules(10, 6, 1, 0, 20, 2,
                new Dictionary<string, int>(), new[] { Array.Empty<string>() });
            ResourceSimulation simulation = new ResourceSimulation(
                rules, 10, 20, Array.Empty<string>(), Array.Empty<string>());
            simulation.ResolveCombatHealth(90, 100);

            simulation.Step(Frame(1));
            simulation.Step(Frame(2));

            Assert.That(simulation.Result, Is.EqualTo(SimulationResult.RightWin));
        }

        private static ResourceSimulation Create()
        {
            var costs = new Dictionary<string, int> { ["Fire"] = 5, ["Water"] = 5, ["Nature"] = 5 };
            var recipes = new[] { new[] { "Fire" } };
            var rules = new DeterministicGameRules(100, 6, 1, 10, 1, 4, costs, recipes);
            return new ResourceSimulation(rules, 10, 20,
                new[] { "Fire", "Water" }, new[] { "Nature", "Fire" });
        }

        private static ConfirmedInputMessage Cast(long userId, int id, params string[] cards) =>
            new ConfirmedInputMessage { userId = userId, input = new FrameInputMessage
            { sequence = 0, type = "useMagic", id = id, cards = cards } };

        private static ConfirmedFrameMessage Frame(int number, params ConfirmedInputMessage[] inputs) =>
            new ConfirmedFrameMessage { type = "confirmedFrame", protocolVersion = 1,
                frameNum = number, inputs = inputs, hashMatched = true };
    }
}
