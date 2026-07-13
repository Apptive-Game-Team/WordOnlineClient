using System;
using GameScene.Simulation.Protocol;
using UnityEngine;

namespace GameScene.Lockstep
{
    public static class LockstepProtocolJson
    {
        [Serializable]
        private sealed class MessageEnvelope { public string type; }

        public static LockstepMessageType ReadType(string json)
        {
            MessageEnvelope envelope = JsonUtility.FromJson<MessageEnvelope>(json);
            return envelope?.type switch
            {
                "lockstepSessionStart" => LockstepMessageType.SessionStart,
                "confirmedFrame" => LockstepMessageType.ConfirmedFrame,
                "lockstepAbort" => LockstepMessageType.Abort,
                _ => LockstepMessageType.Unknown
            };
        }

        public static string SerializeReady()
        {
            return JsonUtility.ToJson(new ClientReadyMessage
            {
                protocolVersion = LockstepVersions.Protocol,
                simulationVersion = LockstepVersions.Simulation,
                configVersion = LockstepVersions.Config
            });
        }

        public static string SerializeSubmission(FrameSubmissionMessage submission) =>
            JsonUtility.ToJson(submission ?? throw new ArgumentNullException(nameof(submission)));

        public static LockstepSessionStartMessage DeserializeSessionStart(string json) =>
            JsonUtility.FromJson<LockstepSessionStartMessage>(json);

        public static ConfirmedFrameMessage DeserializeConfirmedFrame(string json) =>
            JsonUtility.FromJson<ConfirmedFrameMessage>(json);

        public static LockstepAbortMessage DeserializeAbort(string json) =>
            JsonUtility.FromJson<LockstepAbortMessage>(json);
    }
}
