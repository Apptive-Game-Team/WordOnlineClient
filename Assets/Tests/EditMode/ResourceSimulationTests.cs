using System;
using System.Collections.Generic;
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
            Assert.That(simulation.Left.Hand, Is.EqualTo(new[] { "Fire" }));
            simulation.Step(Frame(2, Cast(10, 12, "Fire")));
            Assert.That(simulation.LastCastResults[0].Code, Is.EqualTo(CastResultCode.Success));
            Assert.That(simulation.Left.Mana, Is.EqualTo(15));
            Assert.That(simulation.Left.Hand, Is.EqualTo(new[] { "Water" }));
        }

        [TestCase("Nature", CastResultCode.MissingCard)]
        [TestCase("Water", CastResultCode.InvalidMagic)]
        public void InvalidCastDoesNotPartiallyMutateResources(string card, CastResultCode expected)
        {
            ResourceSimulation simulation = Create(); simulation.Step(Frame(1));
            int mana = simulation.Left.Mana; ulong hash = simulation.CalculateStateHash();
            simulation.Step(Frame(2, Cast(10, 1, card)));
            Assert.That(simulation.LastCastResults[0].Code, Is.EqualTo(expected));
            Assert.That(simulation.Left.Mana, Is.EqualTo(Math.Min(100, mana + 10)));
            Assert.That(simulation.Left.Hand, Does.Contain("Fire"));
            Assert.That(simulation.CalculateStateHash(), Is.Not.EqualTo(hash));
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
