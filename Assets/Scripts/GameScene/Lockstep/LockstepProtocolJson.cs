using System;
using Data.GameConfig;
using Data.Magic;
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
            return ReadRawType(json) switch
            {
                "lockstepSessionStart" => LockstepMessageType.SessionStart,
                "confirmedFrame" => LockstepMessageType.ConfirmedFrame,
                "lockstepAbort" => LockstepMessageType.Abort,
                "result" => LockstepMessageType.Result,
                _ => LockstepMessageType.Unknown
            };
        }

        public static string ReadRawType(string json) =>
            JsonUtility.FromJson<MessageEnvelope>(json)?.type;

        public static string SerializeReady()
        {
            string parameterDataVersion = ParametersDataSource.GetCachedVersion();
            string magicDataVersion = MagicInfoDataSource.GetCachedVersion();
            if (string.IsNullOrWhiteSpace(parameterDataVersion) ||
                string.IsNullOrWhiteSpace(magicDataVersion))
                throw new InvalidOperationException(
                    "Versioned parameter and magic data must be loaded before lockstep ready");
            return JsonUtility.ToJson(new ClientReadyMessage
            {
                protocolVersion = LockstepVersions.Protocol,
                simulationVersion = LockstepVersions.Simulation,
                configVersion = LockstepVersions.Config,
                parameterDataVersion = parameterDataVersion,
                magicDataVersion = magicDataVersion
            });
        }

        public static string SerializeSubmission(FrameSubmissionMessage submission) =>
            JsonUtility.ToJson(submission ?? throw new ArgumentNullException(nameof(submission)));

        public static string SerializeResult(LockstepResultSubmissionMessage result) =>
            JsonUtility.ToJson(result ?? throw new ArgumentNullException(nameof(result)));

        public static LockstepSessionStartMessage DeserializeSessionStart(string json) =>
            JsonUtility.FromJson<LockstepSessionStartMessage>(json);

        public static ConfirmedFrameMessage DeserializeConfirmedFrame(string json) =>
            JsonUtility.FromJson<ConfirmedFrameMessage>(json);

        public static LockstepAbortMessage DeserializeAbort(string json) =>
            JsonUtility.FromJson<LockstepAbortMessage>(json);

        public static Data.ResultInfo DeserializeResult(string json) =>
            JsonUtility.FromJson<Data.ResultInfo>(json);
    }
}
