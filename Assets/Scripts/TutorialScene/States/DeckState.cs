namespace TutorialScene
{
    public class DeckState : ITutorialState
    {
        public void Enter()
        {
            GlobalTutorialManager.Instance.SetProgress(OnboardingProgress.Deck_EnterScene);
            GlobalTutorialManager.Instance.LoadSceneIfNeeded(GlobalTutorialManager.GetSceneName(OnboardingProgress.Deck_EnterScene));
        }

        public void Update()
        {
        }

        public void Exit()
        {
        }
    }

    public class MagicBookState : ITutorialState
    {
        public void Enter()
        {
            GlobalTutorialManager.Instance.SetProgress(OnboardingProgress.MagicBook_EnterScene);
            GlobalTutorialManager.Instance.LoadSceneIfNeeded(GlobalTutorialManager.GetSceneName(OnboardingProgress.MagicBook_EnterScene));
        }

        public void Update()
        {
        }

        public void Exit()
        {
        }
    }

    public class LobbyCommonState : ITutorialState
    {
        public void Enter()
        {
            GlobalTutorialManager.Instance.SetProgress(OnboardingProgress.Common_SelectDeck);
            GlobalTutorialManager.Instance.LoadSceneIfNeeded(GlobalTutorialManager.GetSceneName(OnboardingProgress.Common_SelectDeck));
        }

        public void Update()
        {
        }

        public void Exit()
        {
        }
    }
}
