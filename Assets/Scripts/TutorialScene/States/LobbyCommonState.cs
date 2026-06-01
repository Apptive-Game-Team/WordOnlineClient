using UnityEngine.SceneManagement;

namespace TutorialScene
{
    public class LobbyCommonState : ITutorialState
    {
        private const string LobbySceneName = "LobbyScene";

        private LobbyTutorialController lobbyController;

        public void Enter()
        {
            GlobalTutorialManager.Instance.SetProgress(OnboardingProgress.Common_SelectDeck);
            SceneManager.sceneLoaded += OnSceneLoaded;
            GlobalTutorialManager.Instance.LoadSceneIfNeeded(LobbySceneName);
            TryStartLobbySteps();
        }

        public void Update()
        {
        }

        public void Exit()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (lobbyController != null)
            {
                lobbyController.Hide();
                lobbyController = null;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (GlobalTutorialManager.Instance.CurrentProgress == OnboardingProgress.Common_SelectDeck)
            {
                TryStartLobbySteps();
            }
        }

        private void TryStartLobbySteps()
        {
            lobbyController = LobbyTutorialController.Instance;
            if (lobbyController == null)
            {
                return;
            }

            lobbyController.ShowDeckDropdown(ShowMatchStep);
        }

        private void ShowMatchStep()
        {
            GlobalTutorialManager.Instance.SetProgress(OnboardingProgress.Common_ExplainMatch);
            lobbyController.ShowMatchButton(ShowBotStep);
        }

        private void ShowBotStep()
        {
            GlobalTutorialManager.Instance.SetProgress(OnboardingProgress.Common_ExplainBot);
            lobbyController.ShowBotButton(ShowAdventureStep);
        }

        private void ShowAdventureStep()
        {
            GlobalTutorialManager.Instance.SetProgress(OnboardingProgress.Common_ExplainAdventure);
            lobbyController.ShowAdventureButton(ShowMenuStep);
        }

        private void ShowMenuStep()
        {
            GlobalTutorialManager.Instance.SetProgress(OnboardingProgress.Common_ExplainMenu);
            lobbyController.ShowMenuButton(CompleteCommonState);
        }

        private void CompleteCommonState()
        {
            GlobalTutorialManager.Instance.SetProgress(OnboardingProgress.Common_Complete);
            GlobalTutorialManager.Instance.FinishOnboarding();
        }
    }
}
