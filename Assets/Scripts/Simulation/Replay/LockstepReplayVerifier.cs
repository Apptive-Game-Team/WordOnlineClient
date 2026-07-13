using System;
using System.Collections.Generic;
using GameScene.Simulation.Core;
using GameScene.Simulation.Protocol;

namespace GameScene.Simulation.Replay
{
    public enum LockstepFailureReason : byte
    {
        None = 0,
        Timeout = 1,
        HashMismatch = 2,
        Disconnected = 3,
        VersionMismatch = 4
    }

    public readonly struct ReplayCheckpoint
    {
        public int Frame { get; }
        public ulong PrimaryHash { get; }
        public ulong ReplicaHash { get; }
        public ReplayCheckpoint(int frame, ulong primaryHash, ulong replicaHash)
        { Frame = frame; PrimaryHash = primaryHash; ReplicaHash = replicaHash; }
    }

    public sealed class LockstepFailureDiagnostic
    {
        public LockstepFailureReason Reason { get; }
        public int Frame { get; }
        public string Message { get; }
        public LockstepFailureDiagnostic(LockstepFailureReason reason, int frame, string message)
        { Reason = reason; Frame = frame; Message = message ?? string.Empty; }
    }

    public sealed class LockstepReplayArtifact
    {
        private readonly List<ReplayCheckpoint> checkpoints = new List<ReplayCheckpoint>();
        private readonly List<LockstepFailureDiagnostic> failures = new List<LockstepFailureDiagnostic>();
        public IReadOnlyList<ReplayCheckpoint> Checkpoints => checkpoints;
        public IReadOnlyList<LockstepFailureDiagnostic> Failures => failures;
        internal void Add(ReplayCheckpoint checkpoint) => checkpoints.Add(checkpoint);
        internal void Add(LockstepFailureDiagnostic failure) => failures.Add(failure);
    }

    public sealed class LockstepReplayVerifier
    {
        private readonly SimulationWorld primary;
        private readonly SimulationWorld replica;
        public LockstepReplayArtifact Artifact { get; } = new LockstepReplayArtifact();

        public LockstepReplayVerifier(SimulationWorld primary, SimulationWorld replica)
        { this.primary = primary ?? throw new ArgumentNullException(nameof(primary)); this.replica = replica ?? throw new ArgumentNullException(nameof(replica)); }

        public bool Step(IReadOnlyList<SimulationInput> confirmedInputs)
        {
            primary.Step(confirmedInputs); replica.Step(confirmedInputs);
            ulong primaryHash = primary.CalculateStateHash(); ulong replicaHash = replica.CalculateStateHash();
            Artifact.Add(new ReplayCheckpoint(primary.FrameNumber, primaryHash, replicaHash));
            if (primaryHash == replicaHash) return true;
            Artifact.Add(new LockstepFailureDiagnostic(LockstepFailureReason.HashMismatch, primary.FrameNumber,
                "hash mismatch primary=" + primaryHash.ToString("X16") + " replica=" + replicaHash.ToString("X16")));
            return false;
        }
    }

    public sealed class LockstepSessionGuard
    {
        private readonly int timeoutTicks;
        private int ticksWithoutFrame;
        public int CurrentFrame { get; private set; }
        public LockstepFailureDiagnostic Failure { get; private set; }
        public bool IsFailed => Failure != null;

        public LockstepSessionGuard(LockstepSessionStartMessage start, int timeoutTicks)
        {
            if (timeoutTicks <= 0) throw new ArgumentOutOfRangeException(nameof(timeoutTicks));
            this.timeoutTicks = timeoutTicks;
            try { LockstepVersionValidator.Validate(start); CurrentFrame = start.initialFrame; }
            catch (InvalidOperationException exception) { Fail(LockstepFailureReason.VersionMismatch, exception.Message); }
        }

        public void ConfirmFrame(int frameNumber)
        {
            if (IsFailed) return;
            if (frameNumber != CurrentFrame) throw new InvalidOperationException("Expected frame " + CurrentFrame + ", received " + frameNumber);
            CurrentFrame = checked(CurrentFrame + 1); ticksWithoutFrame = 0;
        }

        public void TickWithoutFrame()
        {
            if (IsFailed) return;
            ticksWithoutFrame++;
            if (ticksWithoutFrame >= timeoutTicks) Fail(LockstepFailureReason.Timeout, "confirmed frame timeout after " + ticksWithoutFrame + " ticks");
        }

        public void Disconnect(string message) { if (!IsFailed) Fail(LockstepFailureReason.Disconnected, message ?? "disconnected"); }
        private void Fail(LockstepFailureReason reason, string message) => Failure = new LockstepFailureDiagnostic(reason, CurrentFrame, message);
    }
}
