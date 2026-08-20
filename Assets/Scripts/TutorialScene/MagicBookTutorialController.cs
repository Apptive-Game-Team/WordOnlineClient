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
            // 유닛 미리보기 모달은 이 안내와 같은 자리를 덮는다. 플레이어가 앞 단계에서 열어
            // 두었으면 원소표 버튼도 안내도 그 뒤에 가려지므로, 짚어 주기 전에 걷어낸다.
            CloseMagicPreviews();

            Transform target = elementChartButton != null ? elementChartButton.transform : null;
            if (target == null && elementChartButtonBase != null)
            {
                target = elementChartButtonBase.transform;
            }

            Show("onboarding.magicBook.elementChart", target, onNext);
        }

        // 미리보기는 마법 정보 칸마다 붙으므로 정해진 하나를 들고 있을 수 없다. 씬에 있는 것을
        // 모두 닫는다. 이미 닫힌 것에 걸어도 아무 일도 일어나지 않는다.
        private static void CloseMagicPreviews()
        {
            MagicPrefabPreview[] previews = FindObjectsOfType<MagicPrefabPreview>(true);
            foreach (MagicPrefabPreview preview in previews)
            {
                preview.Close();
            }
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
