using UnityEngine;
using UnityEngine.UI;

namespace GameScene
{
    /// <summary>
    /// 마나 바 위에 "지금 준비한 조합이 먹을 마나"를 덧그린다.
    /// 구간은 언제나 초록 채움의 오른쪽 끝에서 왼쪽으로 자란다. 소모량이 현재 마나 이하면
    /// 그만큼만 주황으로 덮고, 현재 마나보다 크면 시전할 수 없다는 뜻이므로 초록 채움 전체를
    /// 빨갛게 덮는다.
    /// 씬 계층에 오브젝트를 미리 심어 두지 않아도 되도록 덧그리는 이미지는 런타임에 만든다.
    /// </summary>
    public class ManaCostPreview
    {
        /// <summary>소모 가능. "이만큼 빠진다".</summary>
        private static readonly Color AffordableColor = new Color(0.94f, 0.55f, 0.15f, 1f);

        /// <summary>마나 부족. "지금은 못 쓴다".</summary>
        private static readonly Color InsufficientColor = new Color(0.85f, 0.21f, 0.18f, 1f);

        private const string OverlayObjectName = "ManaCostOverlay";

        private readonly Slider slider;

        private RectTransform overlay;
        private Image overlayImage;

        public ManaCostPreview(Slider slider)
        {
            this.slider = slider;
        }

        /// <param name="currentMana">지금 가진 마나. 슬라이더 값과 같은 단위다.</param>
        /// <param name="cost">준비한 조합이 먹을 마나. 0 이하면 덧그리지 않는다.</param>
        public void Render(float currentMana, int cost)
        {
            if (slider == null || slider.fillRect == null || cost <= 0)
            {
                Hide();
                return;
            }

            float valueRange = slider.maxValue - slider.minValue;
            if (valueRange <= 0f)
            {
                Hide();
                return;
            }

            float filledEnd = Mathf.Clamp01((currentMana - slider.minValue) / valueRange);
            bool insufficient = cost > currentMana;
            float costStart = insufficient
                ? 0f
                : Mathf.Clamp01((currentMana - cost - slider.minValue) / valueRange);

            // 마나가 0이면 덮을 초록이 없다.
            if (filledEnd <= costStart)
            {
                Hide();
                return;
            }

            if (!TryEnsureOverlay())
            {
                return;
            }

            overlayImage.color = insufficient ? InsufficientColor : AffordableColor;
            overlay.anchorMin = new Vector2(costStart, 0f);
            overlay.anchorMax = new Vector2(filledEnd, 1f);
            overlay.offsetMin = Vector2.zero;
            overlay.offsetMax = Vector2.zero;

            // Render는 마나가 갱신될 때마다, 즉 매 프레임 불린다. 이미 맞는 상태면 건드리지 않는다.
            if (!overlay.gameObject.activeSelf)
            {
                overlay.gameObject.SetActive(true);
            }
        }

        public void Hide()
        {
            if (overlay != null && overlay.gameObject.activeSelf)
            {
                overlay.gameObject.SetActive(false);
            }
        }

        private bool TryEnsureOverlay()
        {
            if (overlay != null)
            {
                return true;
            }

            // 슬라이더는 채움을 이 부모 안에서 0..normalizedValue 앵커로 늘린다. 같은 부모에
            // 같은 방식으로 앵커를 잡으면 덧그린 구간이 마나 값과 정확히 같은 자리에 놓인다.
            RectTransform fillArea = slider.fillRect.parent as RectTransform;
            if (fillArea == null)
            {
                return false;
            }

            GameObject overlayObject = new GameObject(OverlayObjectName, typeof(RectTransform), typeof(Image));
            overlayObject.layer = fillArea.gameObject.layer;

            overlay = (RectTransform)overlayObject.transform;
            overlay.SetParent(fillArea, false);
            overlay.pivot = new Vector2(0.5f, 0.5f);

            // 채움과 같은 9-slice 스프라이트를 쓰면 구간이 좁을 때 테두리가 뭉개진다.
            // 초록 채움 안쪽만 덮으므로 스프라이트 없이 단색으로 그린다.
            overlayImage = overlayObject.GetComponent<Image>();
            overlayImage.raycastTarget = false;

            // 슬라이더가 채움을 다시 그려도 덧그린 구간이 그 아래로 내려가지 않게 마지막에 둔다.
            // 이후 이 부모의 자식 순서를 바꾸는 코드가 없으므로 만들 때 한 번이면 된다.
            overlay.SetAsLastSibling();
            return true;
        }
    }
}
