using System;
using System.Collections.Generic;
using FixMath.NET;
using GameScene.Simulation.Core;
using GameScene.Simulation.Objects;
using GameScene.Simulation.Protocol;
using GameScene.Simulation.Pve;
using GameScene.Simulation.Replay;
using GameScene.Simulation.Resources;
using NUnit.Framework;

namespace GameScene.Simulation.Tests
{
    public sealed class LockstepE2EReplayTests
    {
        [Test]
        public void PvpIndependentWorldsMatchEveryConfirmedCheckpoint()
        {
            SimulationWorld primary = PvpWorld(42); SimulationWorld replica = PvpWorld(42);
            LockstepReplayVerifier verifier = new LockstepReplayVerifier(primary, replica);
            for (int frame = 0; frame < 120; frame++)
            {
                SimulationInput[] inputs =
                {
                    Velocity(10, frame, 0, frame < 60 ? 1 : -1),
                    Velocity(20, frame, 1, frame < 60 ? -1 : 1)
                };
                Assert.That(verifier.Step(inputs), Is.True, "frame " + frame);
            }
            Assert.That(verifier.Artifact.Checkpoints.Count, Is.EqualTo(120));
            Assert.That(verifier.Artifact.Failures, Is.Empty);
        }

        [Test]
        public void HashMismatchRetainsFrameAndBothHashes()
        {
            LockstepReplayVerifier verifier = new LockstepReplayVerifier(PvpWorld(1), PvpWorld(2));
            Assert.That(verifier.Step(Array.Empty<SimulationInput>()), Is.False);
            Assert.That(verifier.Artifact.Failures[0].Reason, Is.EqualTo(LockstepFailureReason.HashMismatch));
            Assert.That(verifier.Artifact.Failures[0].Frame, Is.EqualTo(1));
            Assert.That(verifier.Artifact.Failures[0].Message, Does.Contain("primary=").And.Contain("replica="));
        }

        [Test]
        public void TimeoutDisconnectAndVersionRejectionRetainDiagnostics()
        {
            LockstepSessionGuard timeout = new LockstepSessionGuard(Start("PVP"), 3);
            timeout.TickWithoutFrame(); timeout.TickWithoutFrame(); timeout.TickWithoutFrame();
            Assert.That(timeout.Failure.Reason, Is.EqualTo(LockstepFailureReason.Timeout));
            Assert.That(timeout.Failure.Frame, Is.EqualTo(0));

            LockstepSessionGuard disconnected = new LockstepSessionGuard(Start("PVP"), 3);
            disconnected.ConfirmFrame(0); disconnected.Disconnect("socket closed");
            Assert.That(disconnected.Failure.Reason, Is.EqualTo(LockstepFailureReason.Disconnected));
            Assert.That(disconnected.Failure.Frame, Is.EqualTo(1));

            LockstepSessionStartMessage incompatible = Start("PVP"); incompatible.simulationVersion = "wrong-engine";
            LockstepSessionGuard version = new LockstepSessionGuard(incompatible, 3);
            Assert.That(version.Failure.Reason, Is.EqualTo(LockstepFailureReason.VersionMismatch));
            Assert.That(version.Failure.Message, Does.Contain("wrong-engine"));
        }

        [Test]
        public void PveAndPracticeCompleteFromStartToResultWithoutPeerHash()
        {
            LockstepParticipantPolicy participants = new LockstepParticipantPolicy(10, -1);
            Assert.That(participants.RequiresPeerHash, Is.False);

            PveSessionSimulation pve = PveSessionSimulation.Start(PveStart(), PveCatalog());
            Assert.That(pve.State, Is.EqualTo(PveSessionState.Started));
            PracticeBot bot = new PracticeBot(99, -1, 1, "Fire");
            Assert.That(bot.Decide(0, pve.World).input.type, Is.EqualTo("useMagic"));
            pve.World.Destroy(1); pve.Step();
            Assert.That(pve.State, Is.EqualTo(PveSessionState.Won));

            DeterministicGameRules rules = new DeterministicGameRules(100, 6, 1, 10, 1, 3,
                new Dictionary<string, int> { ["Fire"] = 5 }, new[] { new[] { "Fire" } });
            ResourceSimulation practice = new ResourceSimulation(rules, 10, -1,
                new[] { "Fire" }, new[] { "Fire" });
            PracticeBot practiceBot = new PracticeBot(99, -1, 1, "Fire");
            for (int frame = 1; frame <= 3; frame++)
            {
                ConfirmedInputMessage localBotInput = practiceBot.Decide(frame, pve.World);
                practice.Step(new ConfirmedFrameMessage { protocolVersion = LockstepVersions.Protocol,
                    frameNum = frame, hashMatched = true, inputs = new[] { localBotInput } });
            }
            Assert.That(practice.Result, Is.EqualTo(SimulationResult.Draw));
        }

        private static SimulationWorld PvpWorld(long seed)
        {
            SimulationWorld world = new SimulationWorld(seed);
            world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 10, At(3, 5));
            world.Spawn(SimulationPrefabRegistry.PlayerPrefabId, 20, At(15, 5));
            return world;
        }

        private static SimulationInput Velocity(long userId, int sequence, int entityId, int x) =>
            new SimulationInput(userId, sequence, SimulationInputType.SetVelocity, entityId, new SimVector2((Fix64)x, Fix64.Zero));

        private static LockstepSessionStartMessage Start(string type) => new LockstepSessionStartMessage
        { protocolVersion = LockstepVersions.Protocol, simulationVersion = LockstepVersions.Simulation, configVersion = LockstepVersions.Config, initialFrame = 0, sessionType = type };

        private static LockstepSessionStartMessage PveStart()
        {
            LockstepSessionStartMessage start = Start("PVE"); start.rngSeed = 99; start.leftUserId = 10; start.rightUserId = -1;
            start.bootstrapEvents = new[]
            {
                new BootstrapEventMessage { sequence = 0, type = "SPAWN_PLAYER", master = "LeftPlayer", position = new ProtocolVector3 { x = 2, z = 5 } },
                new BootstrapEventMessage { sequence = 1, type = "START_PVE_SCENARIO", scenarioId = 1 }
            };
            return start;
        }

        private static PveScenarioCatalog PveCatalog() => new PveScenarioCatalog(new[]
        {
            new PveScenarioDefinition(1, LockstepVersions.Config, 100, new[]
            { new PveSpawnEvent(0, 0, SimulationPrefabRegistry.DefaultPrefabId, -1, At(7, 5), true) })
        });

        private static SimVector2 At(int x, int y) => new SimVector2((Fix64)x, (Fix64)y);
    }
}
