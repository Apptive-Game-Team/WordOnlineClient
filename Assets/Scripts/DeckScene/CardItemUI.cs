using Data;
using Data.Localization;
using GameScene.Card;
using Global;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeckScene
{
    public class CardItemUI : MonoBehaviour
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

        public void Init(string cName, int count)
        {
            Init(cName, count, true, null, null);
        }

        public async void Init(string cName, int count, bool unlocked, string unlockText, string progressText)
        {
            if (cardArtImage == null)
                cardArtImage = transform.GetChild(2).GetComponent<Image>();

            cardArtImage.sprite = cardImageMapper.GetCardImage(cName);

            MagicData magicData = LocalMagicData.GetMagicData(cName);
            cardManaText.text = magicData.mana.ToString();

            var bg = GetComponent<Image>();
            switch (magicData.type)
            {
                // case "type": bg.sprite = typeSprite; break;
                // case "magic": bg.sprite = magicSprite; break;
                default: WDebug.LogError($"Unknown magic type: {magicData.type}"); break;
            }

            cardNameText.text = await LocaleUtils.GetStringAsync("Card", cName);

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
    }
}
