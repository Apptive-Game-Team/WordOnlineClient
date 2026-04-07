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
        // parameters is a nested map — parsed separately from GameDataManager.Config
        public string sessionType;              // "PVP" | "PVE" | "PRACTICE"
        public List<InitialObjectDto> initialObjects;   // PVE only; null for PVP/Practice
        public List<PveEventDto> scenarioEvents;        // PVE only; null for PVP/Practice
    }

    /// <summary>
    /// One object to spawn in SimWorld at session start (PVE only).
    /// installerId maps to a runtime object ID used by scenario events.
    /// </summary>
    [Serializable]
    public class InitialObjectDto
    {
        public string installerId;
        public string prefabType;
        public string master;
        public float x, y, z;
    }

    /// <summary>
    /// One dialogue event that fires when the simulation reaches a given frame (PVE only).
    /// </summary>
    [Serializable]
    public class PveEventDto
    {
        public int frameNum;
        public string speakerInstallerId;
        public string key;
        public List<string> lines;
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
