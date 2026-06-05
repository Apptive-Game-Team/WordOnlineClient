
using UnityEngine.SceneManagement;

namespace TutorialScene
{
    public class BattleState : ITutorialState
    {

        public void Enter()
        {
            if (BattleTutorialManager.Instance != null)
            {
                BattleTutorialManager.Instance.OnEnd += OnTutorialEnd;
                return;
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public void Update()
        {
            
        }

        public void Exit()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (BattleTutorialManager.Instance != null)
            {
                BattleTutorialManager.Instance.OnEnd -= OnTutorialEnd;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (BattleTutorialManager.Instance == null)
            {
                return;
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
            BattleTutorialManager.Instance.OnEnd += OnTutorialEnd;
        }

        private void OnTutorialEnd()
        {
            GlobalTutorialManager.Instance.CompleteTask(new DeckState());
        }
    }
}
