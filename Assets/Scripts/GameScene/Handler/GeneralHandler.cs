using Data;
using GameScene.Dto;
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
            }
        }
    }
}