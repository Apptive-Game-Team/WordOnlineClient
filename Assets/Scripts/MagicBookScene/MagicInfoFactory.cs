using System.Collections.Generic;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Data;
using Data.Localization;
using Data.Magic;
using Global;
using UnityEngine;

namespace MagicBookScene
{
    public enum MagicBookSortMode
    {
        Name,
        Attribute,
        CardCount,
    }

    public class MagicInfoFactory : MonoBehaviour
    {
        private const string AttributeType = "type";
        private const string ActionType = "magic";

        [SerializeField] private Transform magicInfoParent;
        [SerializeField] private GameObject magicInfoPrefab;
        [SerializeField] private MagicInfo magicInfo;
        
        [SerializeField] private UserMagicApiClient userMagicApiClient;

        public event Action MagicSelected;

        private readonly List<MagicBookEntry> entries = new();
        private MagicBookSortMode sortMode = MagicBookSortMode.Name;
        private CardType? selectedAttribute;
        private CardType? selectedActionType;
        private System.Threading.SynchronizationContext unityContext;
        
        private void Awake()
        {
            unityContext = System.Threading.SynchronizationContext.Current;
            var savedMagicJson = PlayerPrefs.GetString(MagicInfoDataSource.PlayerPrefsKeyName, string.Empty);
            WDebug.Log($"[MagicInfoFactory] Saved magic json: {savedMagicJson}");

            MagicInfoDataSource.Instance.GetMagics(_ =>
            {
                userMagicApiClient.GetUserMagic(response =>
                {
                    LoadEntries(response?.magicIds);
                });
            });
        }

        public void SetSortMode(MagicBookSortMode mode)
        {
            sortMode = mode;
            RenderCurrentView();
        }

        public void SetAttributeFilter(CardType? attribute)
        {
            selectedAttribute = attribute;
            RenderCurrentView();
        }

        public void SetActionTypeFilter(CardType? actionType)
        {
            selectedActionType = actionType;
            RenderCurrentView();
        }
        
        private async void LoadEntries(List<long> userMagicIds = null)
        {
            userMagicIds ??= new List<long>();
            List<MagicBookEntry> loadedEntries = await BuildEntriesAsync(userMagicIds);

            RunOnUnityThread(() =>
            {
                entries.Clear();
                entries.AddRange(loadedEntries);
                RenderCurrentView();
            });
        }
        
        private void OnClickMagicButton(CombinedMagicData data)
        {
            magicInfo.Init(data);
            MagicSelected?.Invoke();
        }

        private async Task<List<MagicBookEntry>> BuildEntriesAsync(List<long> userMagicIds)
        {
            var loadedEntries = new List<MagicBookEntry>();
            foreach (CombinedMagicData data in LocalCombinedMagicData.GetEffectiveDataList())
            {
                string localizedName = await LocaleUtils.GetStringAsync("Magic", data.localizationKey);
                if (string.IsNullOrWhiteSpace(localizedName) || localizedName == data.localizationKey)
                {
                    localizedName = data.serverName;
                }

                loadedEntries.Add(new MagicBookEntry(
                    data,
                    userMagicIds.Contains(data.id),
                    localizedName,
                    GetCardsByType(data, AttributeType),
                    GetCardsByType(data, ActionType)));
            }

            return loadedEntries;
        }

        private void RenderCurrentView()
        {
            ClearMagicInfo();

            foreach (MagicBookEntry entry in GetVisibleEntries())
            {
                CreateMagicInfo(entry.Data, entry.IsOwned);
            }
        }

        private IEnumerable<MagicBookEntry> GetVisibleEntries()
        {
            IEnumerable<MagicBookEntry> visibleEntries = entries.Where(PassesFilters);

            return sortMode switch
            {
                MagicBookSortMode.Attribute => visibleEntries
                    .OrderBy(GetPrimaryAttributeSortValue)
                    .ThenBy(entry => entry.LocalizedName, StringComparer.Create(CultureInfo.CurrentCulture, true)),
                MagicBookSortMode.CardCount => visibleEntries
                    .OrderBy(entry => entry.CardCount)
                    .ThenBy(entry => entry.LocalizedName, StringComparer.Create(CultureInfo.CurrentCulture, true)),
                _ => visibleEntries
                    .OrderBy(entry => entry.LocalizedName, StringComparer.Create(CultureInfo.CurrentCulture, true)),
            };
        }

        private bool PassesFilters(MagicBookEntry entry)
        {
            if (selectedAttribute.HasValue && !entry.Attributes.Contains(selectedAttribute.Value))
            {
                return false;
            }

            if (selectedActionType.HasValue && !entry.ActionTypes.Contains(selectedActionType.Value))
            {
                return false;
            }

            return true;
        }

        private static int GetPrimaryAttributeSortValue(MagicBookEntry entry)
        {
            return entry.Attributes.Count > 0 ? (int)entry.Attributes[0] : int.MaxValue;
        }

        private static List<CardType> GetCardsByType(CombinedMagicData data, string type)
        {
            var cards = new List<CardType>();
            if (data.recipe == null)
            {
                return cards;
            }

            foreach (CardType cardType in data.recipe)
            {
                MagicData magicData = LocalMagicData.GetMagicData(cardType.ToString());
                if (magicData != null && magicData.type == type)
                {
                    cards.Add(cardType);
                }
            }

            return cards;
        }

        private void ClearMagicInfo()
        {
            foreach (Transform child in magicInfoParent)
            {
                Destroy(child.gameObject);
            }
        }

        private void RunOnUnityThread(Action action)
        {
            if (unityContext == null || System.Threading.SynchronizationContext.Current == unityContext)
            {
                action();
                return;
            }

            unityContext.Post(_ => action(), null);
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

        private sealed class MagicBookEntry
        {
            public MagicBookEntry(
                CombinedMagicData data,
                bool isOwned,
                string localizedName,
                List<CardType> attributes,
                List<CardType> actionTypes)
            {
                Data = data;
                IsOwned = isOwned;
                LocalizedName = localizedName ?? string.Empty;
                Attributes = attributes;
                ActionTypes = actionTypes;
            }

            public CombinedMagicData Data { get; }
            public bool IsOwned { get; }
            public string LocalizedName { get; }
            public IReadOnlyList<CardType> Attributes { get; }
            public IReadOnlyList<CardType> ActionTypes { get; }
            public int CardCount => Data.recipe?.Count ?? 0;
        }
    }
}
