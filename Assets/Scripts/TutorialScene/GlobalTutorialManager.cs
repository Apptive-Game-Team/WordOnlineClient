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

        public static bool IsDeckOnboardingActive()
        {
            switch (GetCurrentProgress())
            {
                case OnboardingProgress.Deck_EnterScene:
                case OnboardingProgress.Deck_ExplainCard:
                case OnboardingProgress.Deck_ExplainDeck:
                case OnboardingProgress.Deck_SelectCreateDeck:
                case OnboardingProgress.Deck_CreateDeck:
                case OnboardingProgress.Deck_SaveDeck:
                case OnboardingProgress.Deck_ReturnToLobby:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsMagicBookOnboardingActive()
        {
            switch (GetCurrentProgress())
            {
                case OnboardingProgress.MagicBook_EnterScene:
                case OnboardingProgress.MagicBook_ExplainMagicBook:
                case OnboardingProgress.MagicBook_SelectMagic:
                case OnboardingProgress.MagicBook_ExplainMagicInfo:
                case OnboardingProgress.MagicBook_OpenElementChart:
                case OnboardingProgress.MagicBook_ReturnToLobby:
                    return true;
                default:
                    return false;
            }
        }
    }
}
