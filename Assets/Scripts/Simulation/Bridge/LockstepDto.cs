using System;
using System.Collections.Generic;

namespace Simulation.Bridge
{
    /// <summary>
    /// DTOs for lockstep protocol messages from the server.
    /// Deserialized from JSON via JsonUtility.
    /// </summary>

    [Serializable]
    public class SessionStartDto
    {
        public string type; // "sessionStart"
        public int frameNum;
        public long rngSeed;
        public long leftUserId;
        public long rightUserId;
        public List<string> leftCards;
        public List<string> rightCards;
        // parameters is a nested map — parsed separately
        public string parametersJson;
    }

    [Serializable]
    public class ConfirmedFrameDto
    {
        public string type; // "confirmedFrame"
        public int frameNum;
        public List<PlayerInput> inputs;
    }

    [Serializable]
    public class PlayerInput
    {
        public long userId;
        public List<string> cards;
        public int id;
        public float x, y, z;
    }
}
