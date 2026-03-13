using Data;
using Data.Localization;
using Data.Magic;
using Data.Util;
using GameScene.Card;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MagicBookScene
{
    public class MagicInfo : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Transform cardsParent;
        [SerializeField] private Image magicImage;
        
        [SerializeField] private CardImageMapper mapper;

        public async void Init(CombinedMagicData data)
        {
            magicImage.sprite = data.GetSprite();
            foreach (Transform child in cardsParent)
            {
                Destroy(child.gameObject);
            }
            foreach (CardType cardType in data.recipe)
            {
                var cardObj = new GameObject(cardType.ToString());
                var img = cardObj.AddComponent<Image>();
                img.preserveAspect = true;
                img.sprite = mapper.GetCardImage(cardType);
                img.rectTransform.sizeDelta = new Vector2(50, 50);
                cardObj.transform.SetParent(cardsParent, false);
            }

            nameText.text = await LocaleUtils.GetStringAsync("Magic", StringUtils.ToCamelCase(data.magicName));
        }
    }
}
