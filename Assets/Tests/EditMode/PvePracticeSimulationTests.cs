using System.Collections.Generic;
using FixMath.NET;
using GameScene.Simulation.Core;
using GameScene.Simulation.Magic;
using GameScene.Simulation.Objects;
using GameScene.Simulation.Protocol;
using GameScene.Simulation.Pve;
using GameScene.Simulation.Resources;
using NUnit.Framework;

namespace GameScene.Simulation.Tests
{
    public sealed class PvePracticeSimulationTests
    {
        [Test]
        public void NegativeBotNeverEntersNetworkQuorum()
        {
            LockstepParticipantPolicy solo = new LockstepParticipantPolicy(10, -1);
            Assert.That(solo.NetworkParticipantIds, Is.EqualTo(new long[] { 10 }));
            Assert.That(solo.RequiresNetworkFrame(-1), Is.False);
            Assert.That(solo.RequiresPeerHash, Is.False);

            LockstepParticipantPolicy pvp = new LockstepParticipantPolicy(10, 20);
            Assert.That(pvp.RequiresPeerHash, Is.True);
        }

        [Test]
        public void StartScenarioRejectsDifferentConfigVersion()
        {
            PveScenarioCatalog catalog = Catalog(); LockstepSessionStartMessage start = StartMessage();
            start.configVersion = "other";
            Assert.Throws<System.InvalidOperationException>(() => PveSessionSimulation.Start(start, catalog));
        }

        [Test]
        public void ProductionScenarioUsesInjectedVersionedGameDuration()
        {
            PveScenarioDefinition scenario = ProductionPveScenarioCatalog.Create(1234)
                .GetRequired(11, LockstepVersions.Config);

            Assert.That(scenario.TimeLimitFrames, Is.EqualTo(1234));
        }

        [Test]
        public void PracticeBotDecisionIsNormalStableFrameInput()
        {
            SimulationWorld firstWorld = PlayerWorld(); SimulationWorld secondWorld = PlayerWorld();
            PracticeBot first = new PracticeBot(77, -3, 5, "Water", "Fire");
            PracticeBot second = new PracticeBot(77, -3, 5, "Fire", "Water");

            Assert.That(first.Decide(1, firstWorld), Is.Null);
            ConfirmedInputMessage left = first.Decide(5, firstWorld); ConfirmedInputMessage right = second.Decide(5, secondWorld);
            Assert.That(left.userId, Is.EqualTo(-3)); Assert.That(left.input.type, Is.EqualTo("useMagic"));
            Assert.That(right.input.cards, Is.EqualTo(left.input.cards)); Assert.That(right.input.position.x, Is.EqualTo(left.input.position.x));
        }

        [Test]
        public void PracticeBotChoosesOnlyCompleteRecipePresentInCurrentHand()
        {
            PracticeBot bot = new PracticeBot(77, -3, 5,
                new[] { new[] { "Fire", "Spawn" }, new[] { "Water", "Shoot" } },
                "Fire", "Spawn", "Water", "Shoot");
            SimulationWorld world = PlayerWorld();
            world.Spawn(10, At(10, 5));

            ConfirmedInputMessage input = bot.Decide(5, world,
                new[] { "Water", "Shoot" });

            Assert.That(input.input.cards, Is.EqualTo(new[] { "Water", "Shoot" }));
            Assert.That(input.input.id, Is.EqualTo(input.input.sequence));
        }

        [Test]
        public void PracticeBotUsesDeterministicThinkingDelayAndManaWait()
        {
            PracticeBot bot = new PracticeBot(77, -3, 4, 2,
                new[] { new PracticeBotRecipe(new[] { "Fire", "Spawn" }, (Fix64)8, 5, false) },
                new[] { "Fire", "Spawn" });
            SimulationWorld world = PlayerWorld();

            Assert.That(bot.Decide(4, world, new[] { "Fire", "Spawn" }, 4), Is.Null);
            Assert.That(bot.Decide(8, world, new[] { "Fire", "Spawn" }, 5), Is.Null);
            Assert.That(bot.Decide(9, world, new[] { "Fire", "Spawn" }, 5), Is.Null);
            Assert.That(bot.Decide(10, world, new[] { "Fire", "Spawn" }, 5), Is.Not.Null);
        }

        [Test]
        public void RightSidePracticeBotPlacesTowardArenaWhenNoEnemyExists()
        {
            PracticeBot bot = new PracticeBot(77, -3, 1, 0,
                new[] { new PracticeBotRecipe(new[] { "Fire", "Build" }, (Fix64)4, 0, false) },
                new[] { "Fire", "Build" });
            SimulationWorld world = new SimulationWorld(1, SimulationPrefabRegistry.CreateProduction());
            world.Spawn("Player", -3, At(17, 5));

            ConfirmedInputMessage input = bot.Decide(1, world, new[] { "Fire", "Build" }, 0);

            Assert.That(input.input.position.x, Is.LessThan(17f));
        }

        [Test]
        public void PracticeBotPendingDecisionAndRngParticipateInHash()
        {
            PracticeBot first = new PracticeBot(77, -3, 4, 2,
                new[] { new PracticeBotRecipe(new[] { "Fire", "Spawn" }, (Fix64)8, 5, false) },
                new[] { "Fire", "Spawn" });
            PracticeBot second = new PracticeBot(77, -3, 4, 2,
                new[] { new PracticeBotRecipe(new[] { "Fire", "Spawn" }, (Fix64)8, 5, false) },
                new[] { "Fire", "Spawn" });
            SimulationWorld firstWorld = PlayerWorld();
            SimulationWorld secondWorld = PlayerWorld();

            first.Decide(4, firstWorld, new[] { "Fire", "Spawn" }, 5);
            second.Decide(4, secondWorld, new[] { "Fire", "Spawn" }, 5);

            Assert.That(second.CalculateStateHash(), Is.EqualTo(first.CalculateStateHash()));
            first.Decide(6, firstWorld, new[] { "Fire", "Spawn" }, 5);
            Assert.That(second.CalculateStateHash(), Is.Not.EqualTo(first.CalculateStateHash()));
        }

        [Test]
        public void PracticeBotCyclesAffordableCardWhenNoRecipeCanBeMade()
        {
            PracticeBot bot = new PracticeBot(77, -3, 1, 0,
                new[] { new PracticeBotRecipe(new[] { "Fire", "Shoot" }, (Fix64)8, 5, true) },
                new[] { "Fire", "Shoot", "Water" },
                new Dictionary<string, int> { ["Water"] = 1 });

            ConfirmedInputMessage input = bot.Decide(1, PlayerWorld(), new[] { "Water" }, 1);

            Assert.That(input.input.cards, Is.EqualTo(new[] { "Water" }));
        }

        [Test]
        public void PracticeBotOffensiveRecipeTargetsNearestEnemyInRange()
        {
            SimulationWorld world = PlayerWorld();
            world.Spawn(10, At(10, 5));
            PracticeBot bot = new PracticeBot(77, -3, 1, 0,
                new[] { new PracticeBotRecipe(new[] { "Fire", "Shoot" }, (Fix64)8, 2, true) },
                new[] { "Fire", "Shoot" });

            ConfirmedInputMessage input = bot.Decide(
                1, world, new[] { "Fire", "Shoot" }, 2);

            Assert.That(input.input.position.x, Is.EqualTo(10f));
        }

        [Test]
        public void CounterAggressionPrefersElementalCounterRecipe()
        {
            SimulationWorld world = PlayerWorld();
            world.Spawn(SimulationPrefabRegistry.PveNatureSlimeNestPrefabId, 10, At(10, 5));
            PracticeBot neutral = new PracticeBot(77, -3, 1, 0,
                new[]
                {
                    new PracticeBotRecipe(new[] { "Fire", "Shoot" }, (Fix64)8, 5, true),
                    new PracticeBotRecipe(new[] { "Water", "Shoot" }, (Fix64)8, 1, true)
                }, new[] { "Fire", "Shoot", "Water", "Shoot" }, null, null, "ELITE", 0);
            PracticeBot counter = new PracticeBot(77, -3, 1, 0,
                new[]
                {
                    new PracticeBotRecipe(new[] { "Fire", "Shoot" }, (Fix64)8, 5, true),
                    new PracticeBotRecipe(new[] { "Water", "Shoot" }, (Fix64)8, 1, true)
                }, new[] { "Fire", "Shoot", "Water", "Shoot" }, null, null, "ELITE", 1);

            Assert.That(neutral.Decide(1, world, mana: 10).input.cards,
                Is.EqualTo(new[] { "Water", "Shoot" }));
            Assert.That(counter.Decide(1, world, mana: 10).input.cards,
                Is.EqualTo(new[] { "Fire", "Shoot" }));
        }

        [Test]
        public void TierChangesCandidateQualityButRemainsSeedDeterministic()
        {
            PracticeBotRecipe[] recipes =
            {
                new PracticeBotRecipe(new[] { "Fire", "Build" }, (Fix64)4, 1, false),
                new PracticeBotRecipe(new[] { "Water", "Build" }, (Fix64)4, 2, false),
                new PracticeBotRecipe(new[] { "Nature", "Build" }, (Fix64)4, 3, false),
                new PracticeBotRecipe(new[] { "Rock", "Build" }, (Fix64)4, 4, false)
            };
            string[] deck = { "Fire", "Water", "Nature", "Rock", "Build", "Build", "Build", "Build" };
            bool foundLowerQualityChoice = false;
            for (int seed = 1; seed <= 32; seed++)
            {
                PracticeBot first = new PracticeBot(seed, -3, 1, 0, recipes, deck, null, null, "INTRO", 0);
                PracticeBot replay = new PracticeBot(seed, -3, 1, 0, recipes, deck, null, null, "INTRO", 0);
                PracticeBot elite = new PracticeBot(seed, -3, 1, 0, recipes, deck, null, null, "ELITE", 0);
                ConfirmedInputMessage introInput = first.Decide(1, PlayerWorld(), mana: 10);
                ConfirmedInputMessage replayInput = replay.Decide(1, PlayerWorld(), mana: 10);
                ConfirmedInputMessage eliteInput = elite.Decide(1, PlayerWorld(), mana: 10);
                Assert.That(replayInput.input.cards, Is.EqualTo(introInput.input.cards));
                Assert.That(replayInput.input.position.x, Is.EqualTo(introInput.input.position.x));
                if (!System.Linq.Enumerable.SequenceEqual(introInput.input.cards, eliteInput.input.cards))
                    foundLowerQualityChoice = true;
            }
            Assert.That(foundLowerQualityChoice, Is.True);
        }

        [Test]
        public void StartPlayResultGoldenReplayMatchesEveryHash()
        {
            PveSessionSimulation first = PveSessionSimulation.Start(StartMessage(), Catalog());
            PveSessionSimulation second = PveSessionSimulation.Start(StartMessage(), Catalog());
            Assert.That(second.State, Is.EqualTo(PveSessionState.Started));
            Assert.That(second.CalculateStateHash(), Is.EqualTo(first.CalculateStateHash()));
            for (int frame = 0; frame < 3; frame++)
            { first.Step(); second.Step(); Assert.That(second.CalculateStateHash(), Is.EqualTo(first.CalculateStateHash()), "play frame " + frame); }
            first.World.Destroy(1); second.World.Destroy(1); first.Step(); second.Step();
            Assert.That(first.State, Is.EqualTo(PveSessionState.Won));
            Assert.That(second.CalculateStateHash(), Is.EqualTo(first.CalculateStateHash()));
        }

        [Test]
        public void ProductionScenarioEmitsServerScriptEventWithInstalledSpeakerId()
        {
            LockstepSessionStartMessage start = StartMessage();
            start.bootstrapEvents[1].scenarioId = 14;
            PveSessionSimulation simulation = PveSessionSimulation.Start(
                start, ProductionPveScenarioCatalog.Create());

            for (int frame = 0; frame < 10; frame++) simulation.Step();

            Assert.That(simulation.FiredThisFrame.Count, Is.EqualTo(1));
            Assert.That(simulation.FiredThisFrame[0].Id, Is.EqualTo("intro"));
            Assert.That(simulation.FiredThisFrame[0].SpeakerEntityId, Is.EqualTo(1));
            Assert.That(simulation.FiredThisFrame[0].MessageKey, Is.EqualTo("pve_1_4_intro"));
            Assert.That(simulation.FiredThisFrame[0].Lines,
                Is.EqualTo(new[] { "Stage 1-4", "Destroy the vine witch!" }));
        }

        [Test]
        public void PveLosesBeforeObjectiveCheckWhenLeftPlayerDies()
        {
            PveSessionSimulation simulation = PveSessionSimulation.Start(StartMessage(), Catalog());
            simulation.World.Destroy(1);

            simulation.ResolveAfterWorldStep(true);

            Assert.That(simulation.State, Is.EqualTo(PveSessionState.Lost));
        }

        [Test]
        public void MatchResultMapsPveObjectiveOutcomeToRelaySides()
        {
            PveSessionSimulation pve = PveSessionSimulation.Start(StartMessage(), Catalog());
            ResourceSimulation resources = new ResourceSimulation(
                new DeterministicGameRules(0, 1, 1, 0, 1, 100,
                    new Dictionary<string, int>(), new[] { System.Array.Empty<string>() }),
                10, -1, System.Array.Empty<string>(), System.Array.Empty<string>());
            DeterministicMatchSimulation match = new DeterministicMatchSimulation(
                resources, new MagicSimulation(123, pve.World, ProductionMagicCatalog.Create()),
                pve: pve);

            pve.World.Destroy(1);
            pve.ResolveAfterWorldStep();

            Assert.That(match.Result, Is.EqualTo(SimulationResult.LeftWin));
        }

        private static PveScenarioCatalog Catalog() => new PveScenarioCatalog(new[]
        {
            new PveScenarioDefinition(1, LockstepVersions.Config, 100, new[]
            { new PveSpawnEvent(0, 0, SimulationPrefabRegistry.DefaultPrefabId, -1, At(7, 5), true) })
        });

        private static LockstepSessionStartMessage StartMessage() => new LockstepSessionStartMessage
        {
            sessionType = "PVE", configVersion = LockstepVersions.Config,
            parameterDataVersion = "parameters-1", magicDataVersion = "magic-1",
            rngSeed = 123, leftUserId = 10, rightUserId = -1,
            bootstrapEvents = new[]
            {
                new BootstrapEventMessage { sequence = 0, type = "SPAWN_PLAYER", master = "LeftPlayer", position = Position(2, 5) },
                new BootstrapEventMessage { sequence = 1, type = "START_PVE_SCENARIO", scenarioId = 1 }
            }
        };

        private static SimulationWorld PlayerWorld()
        {
            SimulationWorld world = new SimulationWorld(77, SimulationPrefabRegistry.CreateProduction());
            world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, -3, At(16, 5));
            world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 10, At(2, 5));
            return world;
        }
        private static SimVector2 At(int x, int y) => new SimVector2((Fix64)x, (Fix64)y);
        private static ProtocolVector3 Position(int x, int z) => new ProtocolVector3 { x = x, z = z };
    }
}
