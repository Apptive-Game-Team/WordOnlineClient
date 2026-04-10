using Data.Adventures;
using Data.Adventures.Domain;
using Global;
using UnityEngine;
using UnityEngine.UI;

namespace Adventures.Scenarios
{
    public class ScenarioButton : AdventureButtonBase
    {
        private static readonly Color FinishedColor = new Color(0.30f, 0.80f, 0.45f);
        private static readonly Color ActiveColor = new Color(1.00f, 0.75f, 0.20f);
        private static readonly Color InactiveColor = new Color(0.55f, 0.60f, 0.70f);
        private static readonly Color DebugColor = new Color(0.70f, 0.18f, 0.18f, 0.95f);
        private static readonly Color DebugInactiveColor = new Color(0.35f, 0.35f, 0.40f, 0.95f);

        private Image icon;
        private Button debugButton;
        private Image debugButtonImage;
        private Scenario _scenario;

        public long Id => _scenario.Id;

        protected override void Awake()
        {
            base.Awake();
            icon = GetComponent<Image>();
            CreateDebugButton();
        }

        protected override void OnClickButton()
        {
            if (_scenario.State == State.INACTIVE) return;

            AdventureStoryOverlayUI.Play(_scenario, () => AdventureViewModel.Instance.PlayPVE(Id));
        }

        public void Setup(Scenario scenario)
        {
            _scenario = scenario;

            switch (_scenario.State)
            {
                case State.FINISHED:
                    icon.color = FinishedColor;
                    break;
                case State.ACTIVE:
                    icon.color = ActiveColor;
                    break;
                case State.INACTIVE:
                    icon.color = InactiveColor;
                    break;
            }

            if (debugButton != null)
            {
                debugButtonImage.color = scenario.State == State.INACTIVE
                    ? DebugInactiveColor
                    : DebugColor;
            }
        }

        private void CreateDebugButton()
        {
            var buttonObject = new GameObject("DebugButton");
            buttonObject.transform.SetParent(transform, false);
            buttonObject.transform.SetAsLastSibling();

            var rectTransform = buttonObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(1f, 1f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.pivot = new Vector2(1f, 1f);
            rectTransform.anchoredPosition = new Vector2(8f, 8f);
            rectTransform.sizeDelta = new Vector2(24f, 24f);

            buttonObject.AddComponent<CanvasRenderer>();
            debugButtonImage = buttonObject.AddComponent<Image>();
            debugButtonImage.color = DebugColor;
            buttonObject.AddComponent<AdminOnly>();

            debugButton = buttonObject.AddComponent<Button>();
            debugButton.targetGraphic = debugButtonImage;
            debugButton.onClick.AddListener(OnClickDebugButton);

            var labelObject = new GameObject("Label");
            labelObject.transform.SetParent(buttonObject.transform, false);

            var labelRect = labelObject.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            labelObject.AddComponent<CanvasRenderer>();
            var text = labelObject.AddComponent<Text>();
            text.text = "D";
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 10;
            text.resizeTextMaxSize = 18;
            text.raycastTarget = false;
        }

        private void OnClickDebugButton()
        {
            if (_scenario == null) return;

            AdventureViewModel.Instance.PlayPveDebug(Id);
        }
    }
}
