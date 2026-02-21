using Scripts.Global;
using UnityEngine.SceneManagement;

namespace Scripts.GameScene
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