using System;
using System.Collections.Generic;

namespace GameScene.Simulation.Protocol
{
    public static class LockstepVersions
    {
        public const int Protocol = 1;
        public const string Simulation = "bepuphysics1int-9237daa";
        public const string Config = "lockstep-config-v1";
    }

    public enum LockstepMessageType
    {
        Unknown,
        SessionStart,
        ConfirmedFrame,
        Abort
    }

    [Serializable]
    public sealed class ClientReadyMessage
    {
        public int protocolVersion;
        public string simulationVersion;
        public string configVersion;
    }

    [Serializable]
    public sealed class LockstepSessionStartMessage
    {
        public string type;
        public int protocolVersion;
        public string simulationVersion;
        public string configVersion;
        public long rngSeed;
        public int initialFrame;
        public string sessionType;
        public long leftUserId;
        public long rightUserId;
        public string[] leftCards;
        public string[] rightCards;
        public BootstrapEventMessage[] bootstrapEvents;
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
}
