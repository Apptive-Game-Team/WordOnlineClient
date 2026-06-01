using Global;
using UnityEngine.SceneManagement;

namespace TutorialScene
{
    public class GlobalTutorialManager : SingletonObject<GlobalTutorialManager>
    {
        private const string LobbySceneName = "LobbyScene";
        private const string BattleTutorialSceneName = "InteractiveTutorialScene";

        private ITutorialState currentState;

        public OnboardingProgress CurrentProgress { get; private set; } = OnboardingProgress.Battle;

        private void Start()
        {
            if (CurrentProgress == OnboardingProgress.Battle)
            {
                CompleteTask(new BattleState());
            }
        }

        public void CompleteTask(ITutorialState nextState)
        {
            if (currentState != null)
            {
                currentState.Exit();
            }
            currentState = nextState;
            currentState.Enter();
        }

        public void StartOnboardingTutorial()
        {
            SetProgress(OnboardingProgress.Battle);
            LoadSceneIfNeeded(BattleTutorialSceneName);
            CompleteTask(new BattleState());
        }

        public void SkipOnboardingTutorial()
        {
            SetProgress(OnboardingProgress.Skipped);
            CompleteOnboarding();
            LoadSceneIfNeeded(LobbySceneName);
        }

        public void SetProgress(OnboardingProgress progress)
        {
            CurrentProgress = progress;
        }

        public void Update()
        {
            if (currentState != null)
            {
                currentState.Update();
            }
        }

        internal void LoadSceneIfNeeded(string sceneName)
        {
            if (SceneManager.GetActiveScene().name == sceneName)
            {
                return;
            }

            SceneManager.LoadScene(sceneName);
        }

        private void CompleteOnboarding()
        {
            if (currentState != null)
            {
                currentState.Exit();
                currentState = null;
            }

            Destroy(gameObject);
        }

        public void FinishOnboarding()
        {
            SetProgress(OnboardingProgress.Completed);
            CompleteOnboarding();
        }

        public static OnboardingProgress GetCurrentProgress()
        {
            return Instance != null ? Instance.CurrentProgress : OnboardingProgress.None;
        }
    }
}
