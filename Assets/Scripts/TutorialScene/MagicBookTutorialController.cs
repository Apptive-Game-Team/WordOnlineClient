using Global.Button;
using MagicBookScene;
using UnityEngine;
using UnityEngine.UI;

namespace TutorialScene
{
    public class MagicBookTutorialController : SceneTutorialController<MagicBookTutorialController>
    {
        [Header("Magic Book Targets")]
        [SerializeField] private MagicInfoFactory magicInfoFactory;
        [SerializeField] private GameObject magicBookArea;
        [SerializeField] private GameObject magicListArea;
        [SerializeField] private GameObject magicDescriptionScrollArea;
        [SerializeField] private GameObject elementChartArea;
        [SerializeField] private Button elementChartButton;
        [SerializeField] private ButtonBase elementChartButtonBase;
        [SerializeField] private ButtonBase returnToLobbyButton;

        public event System.Action MagicSelected;
        public event System.Action ElementChartOpened;
        public event System.Action ReturnToLobbySelected;

        private void OnEnable()
        {
            if (magicInfoFactory == null)
            {
                magicInfoFactory = GetComponent<MagicInfoFactory>();
            }

            if (magicInfoFactory != null)
            {
                magicInfoFactory.MagicSelected += NotifyMagicSelected;
            }

            if (elementChartButton != null)
            {
                elementChartButton.onClick.AddListener(NotifyElementChartOpened);
            }

            if (elementChartButtonBase != null)
            {
                elementChartButtonBase.OnClick += NotifyElementChartOpened;
            }

            if (returnToLobbyButton != null)
            {
                returnToLobbyButton.OnClick += NotifyReturnToLobbySelected;
            }
        }

        private void OnDisable()
        {
            if (magicInfoFactory != null)
            {
                magicInfoFactory.MagicSelected -= NotifyMagicSelected;
            }

            if (elementChartButton != null)
            {
                elementChartButton.onClick.RemoveListener(NotifyElementChartOpened);
            }

            if (elementChartButtonBase != null)
            {
                elementChartButtonBase.OnClick -= NotifyElementChartOpened;
            }

            if (returnToLobbyButton != null)
            {
                returnToLobbyButton.OnClick -= NotifyReturnToLobbySelected;
            }
        }

        public void ShowMagicBook(System.Action onNext)
        {
            Show("onboarding.magicBook.description", magicBookArea != null ? magicBookArea.transform : null, onNext);
        }

        public void ShowMagicSelection()
        {
            Show("onboarding.magicBook.selectAnyMagic", magicListArea != null ? magicListArea.transform : null);
        }

        public void ShowMagicInfo(System.Action onNext)
        {
            Show("onboarding.magicBook.readMagicInfo", magicDescriptionScrollArea != null ? magicDescriptionScrollArea.transform : null, onNext);
        }

        public void ShowElementChart(System.Action onNext)
        {
            Transform target = elementChartButton != null ? elementChartButton.transform : null;
            if (target == null && elementChartButtonBase != null)
            {
                target = elementChartButtonBase.transform;
            }

            Show("onboarding.magicBook.elementChart", target, onNext);
        }

        public void ShowOpenedElementChart(System.Action onNext)
        {
            Show("onboarding.magicBook.chartDescription", elementChartArea != null ? elementChartArea.transform : null, onNext);
        }

        public void ShowReturnToLobby()
        {
            Show(
                "onboarding.magicBook.returnToLobby",
                returnToLobbyButton != null ? returnToLobbyButton.transform : null,
                TutorialPanelSide.Right);
        }

        public void NotifyMagicSelected()
        {
            MagicSelected?.Invoke();
        }

        private void NotifyElementChartOpened()
        {
            ElementChartOpened?.Invoke();
        }

        private void NotifyReturnToLobbySelected()
        {
            ReturnToLobbySelected?.Invoke();
        }
    }
}
