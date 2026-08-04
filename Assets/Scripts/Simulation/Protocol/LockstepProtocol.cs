using System;
using System.Collections.Generic;
using GameScene.Simulation.Resources;

namespace GameScene.Simulation.Protocol
{
    public static class LockstepVersions
    {
        public const int Protocol = 1;
        public const string Simulation = "wordonline-lockstep-simulation-v2-bepuphysics1int-9237daa";
        public const string Config = "wordonline-game-config-v2";
    }

    public enum LockstepMessageType
    {
        Unknown,
        SessionStart,
        ConfirmedFrame,
        Abort,
        Result
    }

    [Serializable]
    public sealed class ClientReadyMessage
    {
        public int protocolVersion;
        public string simulationVersion;
        public string configVersion;
        public string parameterDataVersion;
        public string magicDataVersion;
    }

    [Serializable]
    public sealed class LockstepSessionStartMessage
    {
        public string type;
        public int protocolVersion;
        public string simulationVersion;
        public string configVersion;
        public string parameterDataVersion;
        public string magicDataVersion;
        public long rngSeed;
        public int initialFrame;
        public string sessionType;
        public long leftUserId;
        public long rightUserId;
        public string[] leftCards;
        public string[] rightCards;
        public LockstepBotConfigMessage botConfig;
        public BootstrapEventMessage[] bootstrapEvents;
    }

    [Serializable]
    public sealed class LockstepBotConfigMessage
    {
        public long participantId;
        public int reactionIntervalFrames;
        public int thinkingDelayFrames;
        public string tier;
        public double counterAggression;
    }

    [Serializable]
    public sealed class BootstrapEventMessage
    {
        public int sequence;
        public string type;
        public string master;
        public ProtocolVector3 position;
        public long scenarioId;
    }

    [Serializable]
    public sealed class ConfirmedFrameMessage
    {
        public string type;
        public int protocolVersion;
        public int frameNum;
        public ConfirmedInputMessage[] inputs;
        public bool hashMatched;
    }

    [Serializable]
    public sealed class ConfirmedInputMessage
    {
        public long userId;
        public FrameInputMessage input;
    }

    [Serializable]
    public sealed class FrameInputMessage
    {
        public int sequence;
        public string type;
        public int id;
        public string[] cards;
        public ProtocolVector3 position;
    }

    [Serializable]
    public sealed class FrameSubmissionMessage
    {
        public int protocolVersion;
        public int frameNum;
        public string previousFrameHash;
        public FrameInputMessage[] inputs;
    }

    [Serializable]
    public sealed class LockstepResultSubmissionMessage
    {
        public int protocolVersion;
        public int frameNum;
        public string stateHash;
        public string loser;

        public static LockstepResultSubmissionMessage Create(
            int frameNumber, string postFrameHash, SimulationResult result)
        {
            if (frameNumber < 0) throw new ArgumentOutOfRangeException(nameof(frameNumber));
            if (string.IsNullOrWhiteSpace(postFrameHash))
                throw new ArgumentException("Post-frame hash is required", nameof(postFrameHash));
            string loser;
            switch (result)
            {
                case SimulationResult.LeftWin: loser = "RightPlayer"; break;
                case SimulationResult.RightWin: loser = "LeftPlayer"; break;
                case SimulationResult.Draw: loser = "None"; break;
                default: throw new InvalidOperationException("Ongoing match has no result submission");
            }
            return new LockstepResultSubmissionMessage
            {
                protocolVersion = LockstepVersions.Protocol,
                frameNum = frameNumber,
                stateHash = postFrameHash,
                loser = loser
            };
        }
    }

    [Serializable]
    public sealed class LockstepAbortMessage
    {
        public string type;
        public int frameNum;
        public string reason;
        public long[] participantIds;
    }

    [Serializable]
    public sealed class ProtocolVector3
    {
        public float x;
        public float y;
        public float z;
    }

    public sealed class ConfirmedFrameQueue
    {
        private readonly SortedDictionary<int, ConfirmedFrameMessage> frames =
            new SortedDictionary<int, ConfirmedFrameMessage>();
        private readonly int maximumBufferedFrames;

        public int NextFrame { get; private set; }
        public int Count => frames.Count;

        public ConfirmedFrameQueue(int initialFrame, int maximumBufferedFrames = 32)
        {
            if (initialFrame < 0) throw new ArgumentOutOfRangeException(nameof(initialFrame));
            if (maximumBufferedFrames < 1) throw new ArgumentOutOfRangeException(nameof(maximumBufferedFrames));
            NextFrame = initialFrame;
            this.maximumBufferedFrames = maximumBufferedFrames;
        }

        public bool Enqueue(ConfirmedFrameMessage frame)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));
            if (frame.protocolVersion != LockstepVersions.Protocol)
                throw new InvalidOperationException("Confirmed frame protocol mismatch: " + frame.protocolVersion);
            if (!frame.hashMatched)
                throw new InvalidOperationException("Server confirmed a mismatched frame");
            if (frame.frameNum < NextFrame || frames.ContainsKey(frame.frameNum)) return false;
            if (frame.frameNum >= NextFrame + maximumBufferedFrames)
                throw new InvalidOperationException("Confirmed frame exceeds buffer window: " + frame.frameNum);
            frames.Add(frame.frameNum, frame);
            return true;
        }

        public bool TryDequeue(out ConfirmedFrameMessage frame)
        {
            if (!frames.TryGetValue(NextFrame, out frame)) return false;
            frames.Remove(NextFrame);
            NextFrame = checked(NextFrame + 1);
            return true;
        }
    }

    public sealed class LockstepParticipantPolicy
    {
        private readonly long[] networkParticipantIds;
        public IReadOnlyList<long> NetworkParticipantIds => networkParticipantIds;
        public bool RequiresPeerHash => networkParticipantIds.Length > 1;

        public LockstepParticipantPolicy(params long[] participantIds)
        {
            if (participantIds == null) throw new ArgumentNullException(nameof(participantIds));
            SortedSet<long> network = new SortedSet<long>();
            for (int index = 0; index < participantIds.Length; index++)
                if (participantIds[index] >= 0) network.Add(participantIds[index]);
            networkParticipantIds = new long[network.Count]; network.CopyTo(networkParticipantIds);
            if (networkParticipantIds.Length == 0) throw new InvalidOperationException("Session requires one network participant");
        }

        public bool RequiresNetworkFrame(long participantId) => participantId >= 0 && Array.BinarySearch(networkParticipantIds, participantId) >= 0;
    }

    public static class LockstepVersionValidator
    {
        public static void Validate(LockstepSessionStartMessage start)
        {
            if (start == null) throw new ArgumentNullException(nameof(start));
            if (start.protocolVersion != LockstepVersions.Protocol ||
                !string.Equals(start.simulationVersion, LockstepVersions.Simulation, StringComparison.Ordinal) ||
                !string.Equals(start.configVersion, LockstepVersions.Config, StringComparison.Ordinal) ||
                string.IsNullOrWhiteSpace(start.parameterDataVersion) ||
                string.IsNullOrWhiteSpace(start.magicDataVersion))
                throw new InvalidOperationException(
                    "Lockstep version mismatch: protocol=" + start.protocolVersion +
                    ", simulation=" + (start.simulationVersion ?? "<null>") +
                    ", config=" + (start.configVersion ?? "<null>"));
        }

        public static void ValidateDataVersions(LockstepSessionStartMessage start,
            string parameterDataVersion, string magicDataVersion)
        {
            Validate(start);
            if (!string.Equals(start.parameterDataVersion, parameterDataVersion, StringComparison.Ordinal) ||
                !string.Equals(start.magicDataVersion, magicDataVersion, StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "Lockstep data version mismatch: parameters=" +
                    (start.parameterDataVersion ?? "<null>") + ", magic=" +
                    (start.magicDataVersion ?? "<null>"));
        }
    }

    public sealed class LocalFrameInputBuffer
    {
        private readonly List<FrameInputMessage> pending = new List<FrameInputMessage>();
        private int nextSequence;
        public int Count => pending.Count;

        public int EnqueueMagic(int requestId, IReadOnlyList<string> cards, ProtocolVector3 position)
        {
            if (cards == null || cards.Count == 0) throw new ArgumentException("Magic cards required", nameof(cards));
            string[] copy = new string[cards.Count];
            for (int index = 0; index < cards.Count; index++) copy[index] = cards[index];
            int sequence = nextSequence++;
            pending.Add(new FrameInputMessage { sequence = sequence, type = "useMagic", id = requestId,
                cards = copy, position = position ?? throw new ArgumentNullException(nameof(position)) });
            return sequence;
        }

        public int EnqueueCardSelection(IReadOnlyList<string> cards)
        {
            int count = cards?.Count ?? 0;
            string[] copy = new string[count];
            for (int index = 0; index < count; index++) copy[index] = cards[index];
            int sequence = nextSequence++;
            pending.Add(new FrameInputMessage
            {
                sequence = sequence,
                type = "setCardSelection",
                id = 0,
                cards = copy
            });
            return sequence;
        }

        public FrameInputMessage[] Drain()
        {
            FrameInputMessage[] values = pending.ToArray(); pending.Clear(); return values;
        }
    }
}
