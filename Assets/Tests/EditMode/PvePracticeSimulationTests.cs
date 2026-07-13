using FixMath.NET;
using GameScene.Simulation.Core;
using GameScene.Simulation.Objects;
using GameScene.Simulation.Protocol;
using GameScene.Simulation.Pve;
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
        public void PracticeBotDecisionIsNormalStableFrameInput()
        {
            SimulationWorld firstWorld = PlayerWorld(); SimulationWorld secondWorld = PlayerWorld();
            PracticeBot first = new PracticeBot(77, -3, 5, "Water", "Fire");
            PracticeBot second = new PracticeBot(77, -3, 5, "Fire", "Water");

            Assert.That(first.Decide(1, firstWorld), Is.Null);
            ConfirmedInputMessage left = first.Decide(5, firstWorld); ConfirmedInputMessage right = second.Decide(5, secondWorld);
            Assert.That(left.userId, Is.EqualTo(-3)); Assert.That(left.input.type, Is.EqualTo("USE_CARD"));
            Assert.That(right.input.cards, Is.EqualTo(left.input.cards)); Assert.That(right.input.position.x, Is.EqualTo(left.input.position.x));
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

        private static PveScenarioCatalog Catalog() => new PveScenarioCatalog(new[]
        {
            new PveScenarioDefinition(1, LockstepVersions.Config, 100, new[]
            { new PveSpawnEvent(0, 0, SimulationPrefabRegistry.DefaultPrefabId, -1, At(7, 5), true) })
        });

        private static LockstepSessionStartMessage StartMessage() => new LockstepSessionStartMessage
        {
            sessionType = "PVE", configVersion = LockstepVersions.Config, rngSeed = 123, leftUserId = 10, rightUserId = -1,
            bootstrapEvents = new[]
            {
                new BootstrapEventMessage { sequence = 0, type = "SPAWN_PLAYER", master = "LeftPlayer", position = Position(2, 5) },
                new BootstrapEventMessage { sequence = 1, type = "START_PVE_SCENARIO", scenarioId = 1 }
            }
        };

        private static SimulationWorld PlayerWorld() { SimulationWorld world = new SimulationWorld(77); world.Spawn(10, At(2, 5)); return world; }
        private static SimVector2 At(int x, int y) => new SimVector2((Fix64)x, (Fix64)y);
        private static ProtocolVector3 Position(int x, int z) => new ProtocolVector3 { x = x, z = z };
    }
}
