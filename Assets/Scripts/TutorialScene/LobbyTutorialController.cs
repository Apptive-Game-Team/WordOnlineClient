using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Localization;
using Global;
using Global.Button;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TutorialScene
{
    public class OnboardingPromptController : MonoBehaviour
    {
        private const string OnboardingTable = "Onboarding";

        [SerializeField] private GameObject promptPanel;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;

        private async void Start()
        {
            if (GlobalTutorialManager.Instance == null ||
                GlobalTutorialManager.Instance.IsCompleted ||
                GlobalTutorialManager.Instance.CurrentProgress != OnboardingProgress.None)
            {
                SetPromptActive(false);
                return;
            }

            SetPromptActive(true);
            if (titleText != null)
            {
                titleText.text = await LocaleUtils.GetStringAsync(OnboardingTable, "onboarding.tutorialPrompt.title");
            }

            if (messageText != null)
            {
                messageText.text = await LocaleUtils.GetStringAsync(OnboardingTable, "onboarding.tutorialPrompt.message");
            }

            if (yesButton != null)
            {
                yesButton.onClick.AddListener(StartTutorial);
            }

            if (noButton != null)
            {
                noButton.onClick.AddListener(SkipTutorial);
            }
        }

        private void OnDestroy()
        {
            if (yesButton != null)
            {
                yesButton.onClick.RemoveListener(StartTutorial);
            }

            if (noButton != null)
            {
                noButton.onClick.RemoveListener(SkipTutorial);
            }
        }

        private void StartTutorial()
        {
            SetPromptActive(false);
            GlobalTutorialManager.Instance.StartOnboardingTutorial();
        }

        private void SkipTutorial()
        {
            SetPromptActive(false);
            GlobalTutorialManager.Instance.SkipOnboardingTutorial();
        }

        private void SetPromptActive(bool isActive)
        {
            if (promptPanel != null)
            {
                promptPanel.SetActive(isActive);
            }
        }
    }

    public enum OnboardingStepCompletion
    {
        NextButton,
        TargetClick,
        External
    }

    [Serializable]
    public class OnboardingTutorialStepBinding
    {
        public OnboardingProgress progress;
        public string titleKey;
        public string messageKey;
        public RectTransform highlightTarget;
        public ButtonBase buttonBase;
        public Button button;
        public OnboardingStepCompletion completion = OnboardingStepCompletion.NextButton;
    }

    public abstract class SceneTutorialController : LocalSingletonObject<SceneTutorialController>
    {
        private const string OnboardingTable = "Onboarding";

        [Header("Tutorial UI")]
        [SerializeField] private GameObject tutorialMask;
        [SerializeField] private GameObject tutorialPanel;
        [SerializeField] private RectTransform highlightFrame;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button doneButton;

        [Header("Steps")]
        [SerializeField] private List<OnboardingTutorialStepBinding> steps = new List<OnboardingTutorialStepBinding>();

        private readonly Dictionary<Selectable, bool> previousInteractableBySelectable = new Dictionary<Selectable, bool>();
        private OnboardingTutorialStepBinding activeStep;

        protected override void Awake()
        {
            base.Awake();
            HideTutorial();
        }

        protected virtual void Start()
        {
            RefreshStep();
        }

        protected virtual void OnDestroy()
        {
            UnbindActiveStep();
            RestoreInteractables();
        }

        public void RefreshStep()
        {
            if (GlobalTutorialManager.Instance == null || !GlobalTutorialManager.Instance.IsRunning)
            {
                HideTutorial();
                return;
            }

            if (!TryShowProgress(GlobalTutorialManager.Instance.CurrentProgress))
            {
                HideTutorial();
            }
        }

        public virtual bool TryShowProgress(OnboardingProgress progress)
        {
            OnboardingTutorialStepBinding step = steps.Find(candidate => candidate.progress == progress);
            if (step == null)
            {
                return false;
            }

            ShowStep(step);
            return true;
        }

        protected OnboardingTutorialStepBinding CreateDefaultStep(
            OnboardingProgress progress,
            OnboardingStepCompletion completion = OnboardingStepCompletion.NextButton,
            RectTransform target = null,
            ButtonBase buttonBase = null,
            Button button = null)
        {
            return new OnboardingTutorialStepBinding
            {
                progress = progress,
                titleKey = GetTitleKey(progress),
                messageKey = GetMessageKey(progress),
                highlightTarget = target,
                buttonBase = buttonBase,
                button = button,
                completion = completion
            };
        }

        protected void CompleteActiveStep()
        {
            if (activeStep == null || GlobalTutorialManager.Instance == null)
            {
                return;
            }

            OnboardingProgress completedProgress = activeStep.progress;
            UnbindActiveStep();
            GlobalTutorialManager.Instance.AdvanceFrom(completedProgress);
            RefreshStep();
        }

        protected async void ShowStep(OnboardingTutorialStepBinding step)
        {
            UnbindActiveStep();
            activeStep = step;

            SetActive(tutorialMask, true);
            SetActive(tutorialPanel, true);
            SetActive(nextButton != null ? nextButton.gameObject : null, step.completion == OnboardingStepCompletion.NextButton);
            SetActive(doneButton != null ? doneButton.gameObject : null, false);

            await SetLocalizedText(titleText, step.titleKey);
            await SetLocalizedText(messageText, step.messageKey);

            MoveHighlight(step.highlightTarget);
            RestrictInteraction(step);
            BindStepCompletion(step);
        }

        private void BindStepCompletion(OnboardingTutorialStepBinding step)
        {
            if (nextButton != null)
            {
                nextButton.onClick.RemoveListener(CompleteActiveStep);
                if (step.completion == OnboardingStepCompletion.NextButton)
                {
                    nextButton.onClick.AddListener(CompleteActiveStep);
                }
            }

            if (doneButton != null)
            {
                doneButton.onClick.RemoveListener(CompleteActiveStep);
            }

            if (step.completion != OnboardingStepCompletion.TargetClick)
            {
                return;
            }

            if (step.buttonBase != null)
            {
                step.buttonBase.OnClick += CompleteActiveStep;
            }
            else if (step.button != null)
            {
                step.button.onClick.AddListener(CompleteActiveStep);
            }
        }

        private void UnbindActiveStep()
        {
            if (activeStep == null)
            {
                return;
            }

            if (activeStep.buttonBase != null)
            {
                activeStep.buttonBase.OnClick -= CompleteActiveStep;
            }

            if (activeStep.button != null)
            {
                activeStep.button.onClick.RemoveListener(CompleteActiveStep);
            }

            if (nextButton != null)
            {
                nextButton.onClick.RemoveListener(CompleteActiveStep);
            }

            if (doneButton != null)
            {
                doneButton.onClick.RemoveListener(CompleteActiveStep);
            }

            activeStep = null;
        }

        private void MoveHighlight(RectTransform target)
        {
            if (highlightFrame == null)
            {
                return;
            }

            if (target == null)
            {
                highlightFrame.gameObject.SetActive(false);
                return;
            }

            highlightFrame.gameObject.SetActive(true);
            highlightFrame.SetParent(target.parent, false);
            highlightFrame.SetAsLastSibling();
            highlightFrame.anchorMin = target.anchorMin;
            highlightFrame.anchorMax = target.anchorMax;
            highlightFrame.pivot = target.pivot;
            highlightFrame.anchoredPosition = target.anchoredPosition;
            highlightFrame.sizeDelta = target.sizeDelta + new Vector2(16f, 16f);
        }

        private void RestrictInteraction(OnboardingTutorialStepBinding step)
        {
            RestoreInteractables();

            if (step.completion == OnboardingStepCompletion.External)
            {
                SetMaskRaycastBlocking(false);
                return;
            }

            if (step.completion != OnboardingStepCompletion.TargetClick)
            {
                SetMaskRaycastBlocking(true);
                return;
            }

            SetMaskRaycastBlocking(false);
            Selectable allowedSelectable = GetTargetSelectable(step);
            foreach (Selectable selectable in FindObjectsOfType<Selectable>(true))
            {
                if (selectable == allowedSelectable || selectable == nextButton || selectable == doneButton)
                {
                    continue;
                }

                previousInteractableBySelectable[selectable] = selectable.interactable;
                selectable.interactable = false;
            }
        }

        private void RestoreInteractables()
        {
            foreach (KeyValuePair<Selectable, bool> pair in previousInteractableBySelectable)
            {
                if (pair.Key != null)
                {
                    pair.Key.interactable = pair.Value;
                }
            }

            previousInteractableBySelectable.Clear();
            SetMaskRaycastBlocking(false);
        }

        private void SetMaskRaycastBlocking(bool blocksRaycasts)
        {
            if (tutorialMask == null)
            {
                return;
            }

            CanvasGroup canvasGroup = tutorialMask.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = tutorialMask.AddComponent<CanvasGroup>();
            }

            canvasGroup.blocksRaycasts = blocksRaycasts;
            canvasGroup.interactable = blocksRaycasts;
        }

        private Selectable GetTargetSelectable(OnboardingTutorialStepBinding step)
        {
            if (step.button != null)
            {
                return step.button;
            }

            return step.buttonBase != null ? step.buttonBase.GetComponent<Selectable>() : null;
        }

        private void HideTutorial()
        {
            UnbindActiveStep();
            RestoreInteractables();
            SetActive(tutorialMask, false);
            SetActive(tutorialPanel, false);
            SetActive(highlightFrame != null ? highlightFrame.gameObject : null, false);
        }

        private static void SetActive(GameObject target, bool isActive)
        {
            if (target != null)
            {
                target.SetActive(isActive);
            }
        }

        private static async Task SetLocalizedText(TMP_Text text, string key)
        {
            if (text == null || string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            string localized = await LocaleUtils.GetStringAsync(OnboardingTable, key);
            text.text = string.IsNullOrWhiteSpace(localized) ? key : localized;
        }

        private static string GetTitleKey(OnboardingProgress progress)
        {
            switch (progress)
            {
                case OnboardingProgress.Deck_EnterScene:
                case OnboardingProgress.Deck_ExplainCard:
                case OnboardingProgress.Deck_ExplainDeck:
                case OnboardingProgress.Deck_CreateDeck:
                case OnboardingProgress.Deck_ReturnToLobby:
                    return "onboarding.deck.title";
                case OnboardingProgress.MagicBook_EnterScene:
                case OnboardingProgress.MagicBook_ExplainMagicBook:
                case OnboardingProgress.MagicBook_SelectMagic:
                case OnboardingProgress.MagicBook_ExplainMagicInfo:
                case OnboardingProgress.MagicBook_ReturnToLobby:
                    return "onboarding.magicBook.title";
                case OnboardingProgress.Common_SelectDeck:
                case OnboardingProgress.Common_ExplainMatch:
                case OnboardingProgress.Common_ExplainBot:
                case OnboardingProgress.Common_ExplainAdventure:
                    return "onboarding.matching.title";
                case OnboardingProgress.Common_ExplainMenu:
                    return "onboarding.menu.title";
                default:
                    return "onboarding.tutorialPrompt.title";
            }
        }

        private static string GetMessageKey(OnboardingProgress progress)
        {
            switch (progress)
            {
                case OnboardingProgress.Deck_EnterScene:
                    return "onboarding.deck.cardTypes";
                case OnboardingProgress.Deck_ExplainCard:
                    return "onboarding.deck.cardTypes";
                case OnboardingProgress.Deck_ExplainDeck:
                    return "onboarding.deck.rules";
                case OnboardingProgress.Deck_CreateDeck:
                    return "onboarding.deck.createDeck";
                case OnboardingProgress.Deck_ReturnToLobby:
                    return "onboarding.deck.returnToLobby";
                case OnboardingProgress.MagicBook_EnterScene:
                    return "onboarding.magicBook.description";
                case OnboardingProgress.MagicBook_ExplainMagicBook:
                    return "onboarding.magicBook.description";
                case OnboardingProgress.MagicBook_SelectMagic:
                    return "onboarding.magicBook.grassSeedSlime";
                case OnboardingProgress.MagicBook_ExplainMagicInfo:
                    return "onboarding.magicBook.elementChart";
                case OnboardingProgress.MagicBook_ReturnToLobby:
                    return "onboarding.magicBook.returnToLobby";
                case OnboardingProgress.Common_SelectDeck:
                    return "onboarding.matching.deckDropdown";
                case OnboardingProgress.Common_ExplainMatch:
                    return "onboarding.matching.matchPlayers";
                case OnboardingProgress.Common_ExplainBot:
                    return "onboarding.matching.matchBot";
                case OnboardingProgress.Common_ExplainAdventure:
                    return "onboarding.matching.enjoyAdventure";
                case OnboardingProgress.Common_ExplainMenu:
                    return "onboarding.menu.optionsProfile";
                case OnboardingProgress.Common_Complete:
                    return "onboarding.complete";
                default:
                    return "onboarding.tutorialPrompt.message";
            }
        }
    }

    public class LobbyTutorialController : SceneTutorialController
    {
        [Header("Lobby Targets")]
        [SerializeField] private ButtonBase deckButton;
        [SerializeField] private ButtonBase magicBookButton;
        [SerializeField] private ButtonBase matchButton;
        [SerializeField] private ButtonBase botButton;
        [SerializeField] private ButtonBase adventureButton;
        [SerializeField] private ButtonBase menuButton;
        [SerializeField] private TMP_Dropdown deckDropdown;

        protected override void Start()
        {
            base.Start();
            HookLegacyTargets();
        }

        public override bool TryShowProgress(OnboardingProgress progress)
        {
            if (base.TryShowProgress(progress))
            {
                return true;
            }

            OnboardingTutorialStepBinding step = CreateLobbyDefaultStep(progress);
            if (step == null)
            {
                return false;
            }

            ShowStep(step);
            return true;
        }

        private void HookLegacyTargets()
        {
            BindButtonProgress(deckButton, OnboardingProgress.Deck_EnterScene);
            BindButtonProgress(magicBookButton, OnboardingProgress.MagicBook_EnterScene);
            BindButtonProgress(matchButton, OnboardingProgress.Common_ExplainMatch);
            BindButtonProgress(botButton, OnboardingProgress.Common_ExplainBot);
            BindButtonProgress(adventureButton, OnboardingProgress.Common_ExplainAdventure);
            BindButtonProgress(menuButton, OnboardingProgress.Common_ExplainMenu);

            if (deckDropdown != null)
            {
                deckDropdown.onValueChanged.AddListener(_ => CompleteIfCurrent(OnboardingProgress.Common_SelectDeck));
            }
        }

        private static void BindButtonProgress(ButtonBase button, OnboardingProgress progress)
        {
            if (button != null)
            {
                button.OnClick += () => CompleteIfCurrent(progress);
            }
        }

        private static void CompleteIfCurrent(OnboardingProgress progress)
        {
            if (GlobalTutorialManager.Instance == null ||
                GlobalTutorialManager.Instance.CurrentProgress != progress)
            {
                return;
            }

            GlobalTutorialManager.Instance.AdvanceFrom(progress);
        }

        private OnboardingTutorialStepBinding CreateLobbyDefaultStep(OnboardingProgress progress)
        {
            switch (progress)
            {
                case OnboardingProgress.Deck_EnterScene:
                    return CreateTargetStep(progress, deckButton);
                case OnboardingProgress.Deck_ReturnToLobby:
                    return CreateDefaultStep(progress);
                case OnboardingProgress.MagicBook_EnterScene:
                    return CreateTargetStep(progress, magicBookButton);
                case OnboardingProgress.MagicBook_ReturnToLobby:
                    return CreateDefaultStep(progress);
                case OnboardingProgress.Common_SelectDeck:
                    return CreateDefaultStep(progress, OnboardingStepCompletion.NextButton, GetRectTransform(deckDropdown));
                case OnboardingProgress.Common_ExplainMatch:
                    return CreateTargetStep(progress, matchButton);
                case OnboardingProgress.Common_ExplainBot:
                    return CreateTargetStep(progress, botButton);
                case OnboardingProgress.Common_ExplainAdventure:
                    return CreateTargetStep(progress, adventureButton);
                case OnboardingProgress.Common_ExplainMenu:
                    return CreateTargetStep(progress, menuButton);
                case OnboardingProgress.Common_Complete:
                    return CreateDefaultStep(progress);
                default:
                    return null;
            }
        }

        private OnboardingTutorialStepBinding CreateTargetStep(OnboardingProgress progress, ButtonBase button)
        {
            return CreateDefaultStep(
                progress,
                button != null ? OnboardingStepCompletion.TargetClick : OnboardingStepCompletion.NextButton,
                GetRectTransform(button),
                button);
        }

        private static RectTransform GetRectTransform(Component component)
        {
            return component != null ? component.GetComponent<RectTransform>() : null;
        }
    }

    public class DeckTutorialController : SceneTutorialController
    {
        public override bool TryShowProgress(OnboardingProgress progress)
        {
            if (base.TryShowProgress(progress))
            {
                return true;
            }

            switch (progress)
            {
                case OnboardingProgress.Deck_ExplainCard:
                case OnboardingProgress.Deck_ExplainDeck:
                    ShowStep(CreateDefaultStep(progress));
                    return true;
                case OnboardingProgress.Deck_CreateDeck:
                    ShowStep(CreateDefaultStep(progress, OnboardingStepCompletion.External));
                    return true;
                default:
                    return false;
            }
        }

        public void NotifyDeckCardCountChanged(int cardCount)
        {
            if (GlobalTutorialManager.Instance == null ||
                GlobalTutorialManager.Instance.CurrentProgress != OnboardingProgress.Deck_CreateDeck)
            {
                return;
            }

            RefreshStep();
        }

        public void NotifyDeckSaved()
        {
            if (GlobalTutorialManager.Instance != null)
            {
                GlobalTutorialManager.Instance.NotifyDeckSaved();
            }
        }
    }

    public class MagicBookTutorialController : SceneTutorialController
    {
        public override bool TryShowProgress(OnboardingProgress progress)
        {
            if (base.TryShowProgress(progress))
            {
                return true;
            }

            switch (progress)
            {
                case OnboardingProgress.MagicBook_ExplainMagicBook:
                case OnboardingProgress.MagicBook_ExplainMagicInfo:
                case OnboardingProgress.MagicBook_ReturnToLobby:
                    ShowStep(CreateDefaultStep(progress));
                    return true;
                case OnboardingProgress.MagicBook_SelectMagic:
                    ShowStep(CreateDefaultStep(progress, OnboardingStepCompletion.External));
                    return true;
                default:
                    return false;
            }
        }

        public void NotifyMagicSelected()
        {
            if (GlobalTutorialManager.Instance != null &&
                GlobalTutorialManager.Instance.NotifyMagicSelected())
            {
                RefreshStep();
            }
        }

        public void ReturnToLobby()
        {
            if (GlobalTutorialManager.Instance == null ||
                GlobalTutorialManager.Instance.CurrentProgress != OnboardingProgress.MagicBook_ReturnToLobby)
            {
                return;
            }

            GlobalTutorialManager.Instance.AdvanceFrom(OnboardingProgress.MagicBook_ReturnToLobby);
        }
    }
}
