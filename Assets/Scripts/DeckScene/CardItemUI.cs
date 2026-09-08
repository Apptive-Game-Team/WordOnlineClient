using Data;
using Data.Localization;
using Data.Magic;
using GameScene.Card;
using Global;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DeckScene
{
    public class CardItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI cardManaText;
        [SerializeField] private TextMeshProUGUI cardCountText;
        [SerializeField] private CardImageMapper cardImageMapper;

        // [SerializeField] private Sprite typeSprite;
        // [SerializeField] private Sprite magicSprite;

        [SerializeField] private GameObject lockRoot;
        [SerializeField] private TextMeshProUGUI unlockConditionText;
        [SerializeField] private TextMeshProUGUI unlockProgressText;
        [SerializeField] private Image cardArtImage;

        private static readonly Color LockedColor = new Color(0f, 0f, 0f, 0.85f);
        private System.Action<CardItemUI> onPointerEnter;
        private System.Action onPointerExit;

        public void Init(string cName, int count)
        {
            Init(cName, count, true, null, null);
        }

        public async void Init(string cName, int count, bool unlocked, string unlockText, string progressText)
        {
            if (cardArtImage == null)
                cardArtImage = transform.GetChild(2).GetComponent<Image>();

            CombinedMagicData magic = LocalCombinedMagicData.GetCombinedMagicData(cName);

            // 카드 앞면은 마법마다 다른 아트다. cardImageMapper 에는 원소 아이콘만 남아 있다.
            // TODO(#577): 카드에 원소 아이콘을 함께 붙이려면 cardImageMapper.GetElementImage 를 쓴다.
            cardArtImage.sprite = magic != null ? magic.GetSprite() : null;

            cardManaText.text = CardManaCost.Of(magic).ToString();

            var bg = GetComponent<Image>();

            // TODO(#580): 카드 이름 번역표가 Magic 표로 합쳐지면 표 이름을 "Magic" 으로 옮긴다.
            cardNameText.text = await LocaleUtils.GetStringAsync("Card", magic?.localizationKey ?? cName);

            if (unlocked)
            {
                cardCountText.text = $" X {count}";
                if (lockRoot != null) lockRoot.SetActive(false);
                if (unlockConditionText != null) unlockConditionText.text = "";
                if (unlockProgressText != null) unlockProgressText.text = "";
                cardArtImage.color = Color.white;
                bg.color = Color.white;
            }
            else
            {
                cardCountText.text = "";
                if (lockRoot != null) lockRoot.SetActive(true);
                if (unlockConditionText != null) unlockConditionText.text = unlockText ?? "";
                if (unlockProgressText != null) unlockProgressText.text = progressText ?? "";
                cardArtImage.color = LockedColor;
                bg.color = LockedColor;
            }
        }

        public void BindHover(System.Action<CardItemUI> onEnter, System.Action onExit)
        {
            onPointerEnter = onEnter;
            onPointerExit = onExit;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            onPointerEnter?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            onPointerExit?.Invoke();
        }
    }
}
