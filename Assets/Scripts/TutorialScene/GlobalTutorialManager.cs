using Global;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TutorialScene
{
    public class GlobalTutorialManager : SingletonObject<GlobalTutorialManager>
    {
        private const string ProgressPrefsKey = "OnboardingTutorialProgress";
        private const string CompletedPrefsKey = "OnboardingTutorialCompleted";
        private const string LobbySceneName = "LobbyScene";
        private const string BattleTutorialSceneName = "TutorialScene";
        private const string DeckSceneName = "ManageDeckScene";
        private const string MagicBookSceneName = "MagicBookScene";

        private ITutorialState currentState;

        public OnboardingProgress CurrentProgress { get; private set; } = OnboardingProgress.None;
        public bool IsCompleted => PlayerPrefs.GetInt(CompletedPrefsKey, 0) == 1;
        public bool IsRunning => !IsCompleted && CurrentProgress != OnboardingProgress.None;

        protected override void Awake()
        {
            base.Awake();
            CurrentProgress = LoadProgress();
        }

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
            PlayerPrefs.SetInt(CompletedPrefsKey, 0);
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
            PlayerPrefs.SetString(ProgressPrefsKey, progress.ToString());
            PlayerPrefs.Save();
        }

        public void Advance()
        {
            AdvanceFrom(CurrentProgress);
        }

        public void AdvanceFrom(OnboardingProgress progress)
        {
            if (progress != CurrentProgress)
            {
                return;
            }

            switch (progress)
            {
                case OnboardingProgress.Battle:
                    CompleteTask(new DeckState());
                    break;
                case OnboardingProgress.Deck_EnterScene:
                    SetProgress(OnboardingProgress.Deck_ExplainCard);
                    break;
                case OnboardingProgress.Deck_ExplainCard:
                    SetProgress(OnboardingProgress.Deck_ExplainDeck);
                    break;
                case OnboardingProgress.Deck_ExplainDeck:
                    SetProgress(OnboardingProgress.Deck_CreateDeck);
                    break;
                case OnboardingProgress.Deck_CreateDeck:
                    SetProgress(OnboardingProgress.Deck_ReturnToLobby);
                    break;
                case OnboardingProgress.Deck_ReturnToLobby:
                    CompleteTask(new MagicBookState());
                    break;
                case OnboardingProgress.MagicBook_EnterScene:
                    SetProgress(OnboardingProgress.MagicBook_ExplainMagicBook);
                    break;
                case OnboardingProgress.MagicBook_ExplainMagicBook:
                    SetProgress(OnboardingProgress.MagicBook_SelectMagic);
                    break;
                case OnboardingProgress.MagicBook_SelectMagic:
                    SetProgress(OnboardingProgress.MagicBook_ExplainMagicInfo);
                    break;
                case OnboardingProgress.MagicBook_ExplainMagicInfo:
                    SetProgress(OnboardingProgress.MagicBook_ReturnToLobby);
                    break;
                case OnboardingProgress.MagicBook_ReturnToLobby:
                    CompleteTask(new LobbyCommonState());
                    break;
                case OnboardingProgress.Common_SelectDeck:
                    SetProgress(OnboardingProgress.Common_ExplainMatch);
                    break;
                case OnboardingProgress.Common_ExplainMatch:
                    SetProgress(OnboardingProgress.Common_ExplainBot);
                    break;
                case OnboardingProgress.Common_ExplainBot:
                    SetProgress(OnboardingProgress.Common_ExplainAdventure);
                    break;
                case OnboardingProgress.Common_ExplainAdventure:
                    SetProgress(OnboardingProgress.Common_ExplainMenu);
                    break;
                case OnboardingProgress.Common_ExplainMenu:
                    SetProgress(OnboardingProgress.Common_Complete);
                    break;
                case OnboardingProgress.Common_Complete:
                    CompleteOnboarding();
                    break;
            }
        }

        public bool NotifyDeckSaved()
        {
            if (CurrentProgress != OnboardingProgress.Deck_CreateDeck)
            {
                return false;
            }

            SetProgress(OnboardingProgress.Deck_ReturnToLobby);
            LoadSceneIfNeeded(LobbySceneName);
            return true;
        }

        public bool NotifyMagicSelected()
        {
            if (CurrentProgress != OnboardingProgress.MagicBook_SelectMagic)
            {
                return false;
            }

            SetProgress(OnboardingProgress.MagicBook_ExplainMagicInfo);
            return true;
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
            SetProgress(OnboardingProgress.Completed);
            PlayerPrefs.SetInt(CompletedPrefsKey, 1);
            PlayerPrefs.Save();

            if (currentState != null)
            {
                currentState.Exit();
                currentState = null;
            }
        }

        private static OnboardingProgress LoadProgress()
        {
            string savedProgress = PlayerPrefs.GetString(ProgressPrefsKey, OnboardingProgress.None.ToString());
            return System.Enum.TryParse(savedProgress, out OnboardingProgress progress)
                ? progress
                : OnboardingProgress.None;
        }

        public static string GetSceneName(OnboardingProgress progress)
        {
            switch (progress)
            {
                case OnboardingProgress.Battle:
                    return BattleTutorialSceneName;
                case OnboardingProgress.Deck_ExplainCard:
                case OnboardingProgress.Deck_ExplainDeck:
                case OnboardingProgress.Deck_CreateDeck:
                    return DeckSceneName;
                case OnboardingProgress.MagicBook_ExplainMagicBook:
                case OnboardingProgress.MagicBook_SelectMagic:
                case OnboardingProgress.MagicBook_ExplainMagicInfo:
                    return MagicBookSceneName;
                default:
                    return LobbySceneName;
            }
        }
    }
}
