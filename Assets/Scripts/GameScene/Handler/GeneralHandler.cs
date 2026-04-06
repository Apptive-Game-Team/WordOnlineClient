using System.Collections.Generic;
using Data;
using GameScene.Dto;
using Simulation.Bridge;
using UnityEngine;

namespace GameScene.Handler
{
    public class GeneralHandler : IFrameInfoHandler<string>
    {
        private readonly DeltaFrameHandler deltaFrameHandler = new DeltaFrameHandler();
        private readonly ResultHandler resultHandler = new ResultHandler();
        private readonly SyncFrameHandler syncFrameHandler = new SyncFrameHandler();
        private readonly MagicValidHandler magicValidHandler = new MagicValidHandler();

        public void Handler(string json)
        {
            TypeChecker infotype = JsonUtility.FromJson<TypeChecker>(json);
            switch (infotype.type)
            {
                // ── Lockstep messages ──
                case "sessionStart":
                    LockstepDriver.Instance?.HandleSessionStart(json);
                    break;
                case "confirmedFrame":
                    LockstepDriver.Instance?.HandleConfirmedFrame(json);
                    break;

                // ── Legacy server-authoritative messages ──
                case "frame":
                    FrameInfoDto info = JsonUtility.FromJson<FrameInfoDto>(json);
                    deltaFrameHandler.Handler(info);
                    break;
                case "sync":
                    SyncFrameInfo syncFrameInfo = JsonUtility.FromJson<SyncFrameInfo>(json);
                    syncFrameHandler.Handler(syncFrameInfo);
                    break;
                case "magicValid":
                    MagicValidInfo magicValid = JsonUtility.FromJson<MagicValidInfo>(json);
                    //vaildCheck
                    magicValidHandler.Handler(magicValid);
                    return;
                case "result":
                    ResultInfo result = JsonUtility.FromJson<ResultInfo>(json);
                    resultHandler.Handler(result);
                    break;
                case "pveScript":
                case "pveScriptEvent":
                    HandlePveScriptEvent(json);
                    break;
                default:
                    TryHandleUntypedPveScriptEvent(json);
                    break;
            }
        }

        private static void HandlePveScriptEvent(string json)
        {
            PveScriptPayload payload = JsonUtility.FromJson<PveScriptPayload>(json);
            ShowPveScriptPayload(payload);
        }

        private static void TryHandleUntypedPveScriptEvent(string json)
        {
            PveScriptPayload payload = JsonUtility.FromJson<PveScriptPayload>(json);
            if (payload == null)
            {
                return;
            }

            bool hasKey = !string.IsNullOrWhiteSpace(payload.key);
            bool hasLines = payload.lines != null && payload.lines.Count > 0;
            if (!hasKey && !hasLines)
            {
                return;
            }

            ShowPveScriptPayload(payload);
        }

        private static void ShowPveScriptPayload(PveScriptPayload payload)
        {
            if (payload == null)
            {
                return;
            }

            if (payload.lines != null && payload.lines.Count > 0)
            {
                foreach (string line in payload.lines)
                {
                    PveDialoguePresenter.ShowLine(payload.speakerObjectId, line);
                }

                return;
            }

            if (!string.IsNullOrWhiteSpace(payload.key))
            {
                PveDialoguePresenter.ShowLine(payload.speakerObjectId, payload.key);
            }
        }

        [System.Serializable]
        private class PveScriptPayload
        {
            public string type;
            public string key;
            public int speakerObjectId;
            public List<string> lines;
        }
    }
}
