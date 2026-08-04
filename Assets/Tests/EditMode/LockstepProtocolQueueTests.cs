using System;
using GameScene.Simulation.Protocol;
using GameScene.Simulation.Resources;
using NUnit.Framework;

namespace GameScene.Simulation.Tests
{
    public class LockstepProtocolQueueTests
    {
        [Test]
        public void HoldsGapThenDequeuesInFrameOrder()
        {
            ConfirmedFrameQueue queue = new ConfirmedFrameQueue(10);
            Assert.That(queue.Enqueue(Frame(11)), Is.True);
            Assert.That(queue.TryDequeue(out _), Is.False);
            Assert.That(queue.Enqueue(Frame(10)), Is.True);

            Assert.That(queue.TryDequeue(out ConfirmedFrameMessage first), Is.True);
            Assert.That(first.frameNum, Is.EqualTo(10));
            Assert.That(queue.TryDequeue(out ConfirmedFrameMessage second), Is.True);
            Assert.That(second.frameNum, Is.EqualTo(11));
            Assert.That(queue.NextFrame, Is.EqualTo(12));
        }

        [Test]
        public void IgnoresDuplicateAndStaleFrames()
        {
            ConfirmedFrameQueue queue = new ConfirmedFrameQueue(1);
            Assert.That(queue.Enqueue(Frame(1)), Is.True);
            Assert.That(queue.Enqueue(Frame(1)), Is.False);
            Assert.That(queue.TryDequeue(out _), Is.True);
            Assert.That(queue.Enqueue(Frame(1)), Is.False);
        }

        [Test]
        public void RejectsProtocolMismatchHashMismatchAndFarFutureFrame()
        {
            ConfirmedFrameQueue queue = new ConfirmedFrameQueue(1, 2);
            ConfirmedFrameMessage wrongProtocol = Frame(1);
            wrongProtocol.protocolVersion = 2;
            ConfirmedFrameMessage hashMismatch = Frame(1);
            hashMismatch.hashMatched = false;

            Assert.Throws<InvalidOperationException>(() => queue.Enqueue(wrongProtocol));
            Assert.Throws<InvalidOperationException>(() => queue.Enqueue(hashMismatch));
            Assert.Throws<InvalidOperationException>(() => queue.Enqueue(Frame(3)));
        }

        [Test]
        public void VersionsMatchReviewedServerContract()
        {
            Assert.That(LockstepVersions.Protocol, Is.EqualTo(1));
            Assert.That(LockstepVersions.Simulation,
                Is.EqualTo("wordonline-lockstep-simulation-v2-bepuphysics1int-9237daa"));
            Assert.That(LockstepVersions.Config, Is.EqualTo("wordonline-game-config-v2"));
        }

        [Test]
        public void LocalInputsDrainOnceInStableSequenceOrder()
        {
            LocalFrameInputBuffer buffer = new LocalFrameInputBuffer();
            buffer.EnqueueMagic(31, new[] { "FIRE", "AIR" }, new ProtocolVector3 { x = 2, z = 4 });
            buffer.EnqueueMagic(32, new[] { "WATER" }, new ProtocolVector3 { x = 6, z = 8 });
            buffer.EnqueueCardSelection(new[] { "Fire", "Spawn" });

            FrameInputMessage[] drained = buffer.Drain();

            Assert.That(drained, Has.Length.EqualTo(3));
            Assert.That(drained[0].id, Is.EqualTo(31));
            Assert.That(drained[0].sequence, Is.EqualTo(0));
            Assert.That(drained[1].id, Is.EqualTo(32));
            Assert.That(drained[1].sequence, Is.EqualTo(1));
            Assert.That(drained[2].sequence, Is.EqualTo(2));
            Assert.That(drained[2].type, Is.EqualTo("setCardSelection"));
            Assert.That(drained[2].cards, Is.EqualTo(new[] { "Fire", "Spawn" }));
            Assert.That(buffer.Drain(), Is.Empty);
        }

        [TestCase(SimulationResult.LeftWin, "RightPlayer")]
        [TestCase(SimulationResult.RightWin, "LeftPlayer")]
        [TestCase(SimulationResult.Draw, "None")]
        public void TerminalResultMapsToServerMasterContract(
            SimulationResult result, string expectedLoser)
        {
            LockstepResultSubmissionMessage message =
                LockstepResultSubmissionMessage.Create(42, "AABBCC", result);

            Assert.That(message.protocolVersion, Is.EqualTo(LockstepVersions.Protocol));
            Assert.That(message.frameNum, Is.EqualTo(42));
            Assert.That(message.stateHash, Is.EqualTo("AABBCC"));
            Assert.That(message.loser, Is.EqualTo(expectedLoser));
        }

        private static ConfirmedFrameMessage Frame(int number)
        {
            return new ConfirmedFrameMessage
            {
                type = "confirmedFrame",
                protocolVersion = LockstepVersions.Protocol,
                frameNum = number,
                inputs = Array.Empty<ConfirmedInputMessage>(),
                hashMatched = true
            };
        }
    }
}
