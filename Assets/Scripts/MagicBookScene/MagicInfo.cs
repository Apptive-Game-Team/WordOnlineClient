using System.Collections.Generic;
using System.Threading.Tasks;
using Data;
using Data.GameConfig;
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
        [SerializeField] private TMP_Text statsText;
        
        [SerializeField] private CardImageMapper mapper;
        private MagicPrefabPreview prefabPreview;

        public async void Init(CombinedMagicData data)
        {
            magicImage.sprite = data.GetSprite();
            prefabPreview ??= MagicPrefabPreview.Attach(magicImage);
            prefabPreview?.Show(data);
            foreach (Transform child in cardsParent)
            {
                Destroy(child.gameObject);
            }
            // 조합이 없어졌으므로 재료 카드 줄 대신 마법의 원소 아이콘 하나를 그린다.
            // TODO(#578): 도감 화면이 정리되면 원소·마나·사거리 표시를 다시 설계한다.
            Sprite elementSprite = mapper != null ? mapper.GetElementImage(data.element) : null;
            if (elementSprite != null)
            {
                var cardObj = new GameObject(data.element.ToString());
                var img = cardObj.AddComponent<Image>();
                img.preserveAspect = true;
                img.sprite = elementSprite;
                img.rectTransform.sizeDelta = new Vector2(50, 50);
                cardObj.transform.SetParent(cardsParent, false);
            }

            nameText.text = await LocaleUtils.GetStringAsync("Magic", data.localizationKey);
            if (statsText != null)
            {
                string stats = GameParameterResolver.GetMagicDisplayStats(data);
                string description = await GetMagicBookDescriptionAsync(data);
                statsText.text = AppendText(stats, description);
                statsText.gameObject.SetActive(!string.IsNullOrWhiteSpace(statsText.text));
            }
        }

        private static async Task<string> GetMagicBookDescriptionAsync(CombinedMagicData data)
        {
            foreach (string key in GetMagicBookKeyCandidates(data))
            {
                string description = await LocaleUtils.GetStringAsync("MagicBook", key);
                if (!string.IsNullOrWhiteSpace(description) && description != key)
                {
                    return description;
                }
            }

            return string.Empty;
        }

        private static IEnumerable<string> GetMagicBookKeyCandidates(CombinedMagicData data)
        {
            var yielded = new HashSet<string>();
            TryYield(data.textLocalizationKey, yielded, out string textLocalizationKey);
            if (textLocalizationKey != null)
            {
                yield return textLocalizationKey;
            }

            TryYield(data.serverName, yielded, out string serverName);
            if (serverName != null)
            {
                yield return serverName;
            }

            TryYield(StringUtils.ToSnakeCase(data.localizationKey), yielded, out string snakeLocalizationKey);
            if (snakeLocalizationKey != null)
            {
                yield return snakeLocalizationKey;
            }

            TryYield(data.localizationKey, yielded, out string localizationKey);
            if (localizationKey != null)
            {
                yield return localizationKey;
            }
        }

        private static bool TryYield(string value, ISet<string> yielded, out string result)
        {
            result = null;
            if (string.IsNullOrWhiteSpace(value) || !yielded.Add(value))
            {
                return false;
            }

            result = value;
            return true;
        }

        private static string AppendText(string currentText, string additionalText)
        {
            if (string.IsNullOrWhiteSpace(currentText))
            {
                return additionalText ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(additionalText))
            {
                return currentText;
            }

            return $"{currentText}\n\n{additionalText}";
        }
    }
}
