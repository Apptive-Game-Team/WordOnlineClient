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

        private bool completed;

        // 두 번 불려도 안전해야 한다. 이 오브젝트는 스스로를 파괴하므로, 두 번째 호출은
        // 파괴된 오브젝트에 Destroy를 걸어 MissingReferenceException을 낸다. 진행이 이미
        // 끝났다는 사실 자체는 오류가 아니라 그냥 할 일이 없는 상태다.
        private void CompleteOnboarding()
        {
            if (completed)
            {
                return;
            }

            completed = true;

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
