using System.Collections.Generic;
using System;
using System.Linq;
using Data;
using Data.Magic;
using Global;
using UnityEngine;

namespace MagicBookScene
{
    public class MagicInfoFactory : MonoBehaviour
    {
        private static readonly HashSet<CardType> AttributeTypes = new()
        {
            CardType.Fire,
            CardType.Lightning,
            CardType.Nature,
            CardType.Rock,
            CardType.Water,
            CardType.Wind,
        };

        [SerializeField] private Transform magicInfoParent;
        [SerializeField] private GameObject magicInfoPrefab;
        [SerializeField] private MagicInfo magicInfo;
        
        [SerializeField] private UserMagicApiClient userMagicApiClient;

        public event Action MagicSelected;
        
        private void Awake()
        {
            var savedMagicJson = PlayerPrefs.GetString(MagicInfoDataSource.PlayerPrefsKeyName, string.Empty);
            WDebug.Log($"[MagicInfoFactory] Saved magic json: {savedMagicJson}");

            MagicInfoDataSource.Instance.GetMagics(_ =>
            {
                userMagicApiClient.GetUserMagic(response =>
                {
                    CreateAllMagicInfo(response?.magicIds);
                });
            });
        }
        
        private void CreateAllMagicInfo(List<long> userMagicIds = null)
        {
            userMagicIds ??= new List<long>();
            foreach (var data in GetSortedMagicData())
            {
                bool active = userMagicIds.Contains(data.id);
                CreateMagicInfo(data, active);
            }
        }
        
        private void OnClickMagicButton(CombinedMagicData data)
        {
            magicInfo.Init(data);
            MagicSelected?.Invoke();
        }
        
        private void CreateMagicInfo(CombinedMagicData data, bool active = true)
        {
            var magicInfoObj = Instantiate(magicInfoPrefab, magicInfoParent);
            
            var magicButton = magicInfoObj.GetComponent<MagicButton>();
            magicButton.Init(data);
            magicButton.SetActive(active);
            
            if (active)
            {
                magicButton.OnClick += OnClickMagicButton;
            }
        }

        private static IEnumerable<CombinedMagicData> GetSortedMagicData()
        {
            return LocalCombinedMagicData.GetEffectiveDataList()
                .OrderBy(GetAttributeSortKey, StringComparer.OrdinalIgnoreCase)
                .ThenBy(data => data.recipe?.Count ?? int.MaxValue)
                .ThenBy(GetNameSortKey, StringComparer.OrdinalIgnoreCase);
        }

        private static string GetAttributeSortKey(CombinedMagicData data)
        {
            if (data?.recipe == null)
            {
                return "zzzzzzzz";
            }

            string attribute = data.recipe
                .Where(AttributeTypes.Contains)
                .Select(cardType => cardType.ToString())
                .OrderBy(cardType => cardType, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();

            return string.IsNullOrWhiteSpace(attribute) ? "zzzzzzzz" : attribute;
        }

        private static string GetNameSortKey(CombinedMagicData data)
        {
            if (!string.IsNullOrWhiteSpace(data?.serverName))
            {
                return data.serverName;
            }

            return data?.localizationKey ?? string.Empty;
        }
    }
}
