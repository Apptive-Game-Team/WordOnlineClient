using Script.Global;
using UnityEngine.SceneManagement;

namespace Script.GameScene
{
    public class GameEndEventController : LocalSingletonObject<GameEndEventController>
    {
        public void TriggerGameEnd()
        {
            Invoke(nameof(GameEnd), 2f);
        }
        
        private void GameEnd()
        {
            SceneManager.LoadScene("ResultScene");
        }
    }
}