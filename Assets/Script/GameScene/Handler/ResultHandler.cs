using UnityEngine.SceneManagement;

namespace Script.GameScene.Handler
{
    public class ResultHandler : IFrameInfoHandler<ResultInfo>
    {
        public void Handler(ResultInfo data)
        {
            SceneContext.MatchResult = data;
            SceneManager.LoadScene("ResultScene");
        }
    }
}