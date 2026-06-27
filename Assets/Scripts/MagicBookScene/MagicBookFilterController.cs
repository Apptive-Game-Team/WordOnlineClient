using System;
using System.Collections.Generic;
using Data;
using Data.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace MagicBookScene
{
    public class MagicBookFilterController : MonoBehaviour
    {
        private const string MagicBookTable = "MagicBook";
        private const string CardTable = "Card";

        [SerializeField] private MagicInfoFactory magicInfoFactory;
        [SerializeField] private TMP_Text sortLabel;
        [SerializeField] private TMP_Dropdown sortDropdown;
        [SerializeField] private TMP_Text attributeLabel;
        [SerializeField] private TMP_Dropdown attributeDropdown;
        [SerializeField] private TMP_Text actionTypeLabel;
        [SerializeField] private TMP_Dropdown actionTypeDropdown;
        [SerializeField] private SortButtonBinding[] sortButtons;
        [SerializeField] private FilterButtonBinding[] attributeButtons;
        [SerializeField] private FilterButtonBinding[] actionTypeButtons;
        [SerializeField] private Color selectedColor = new(0.78f, 0.78f, 0.78f, 1f);

        private readonly MagicBookSortMode[] sortOptions =
        {
            MagicBookSortMode.Name,
            MagicBookSortMode.Attribute,
            MagicBookSortMode.CardCount,
        };

        private readonly CardType?[] attributeOptions =
        {
            null,
            CardType.Fire,
            CardType.Water,
            CardType.Nature,
            CardType.Lightning,
            CardType.Rock,
            CardType.Wind,
        };

        private readonly CardType?[] actionTypeOptions =
        {
            null,
            CardType.Shoot,
            CardType.Drop,
            CardType.Build,
            CardType.Spawn,
            CardType.Explode,
        };

        private MagicBookSortMode selectedSortMode = MagicBookSortMode.Name;
        private CardType? selectedAttribute;
        private CardType? selectedActionType;
        private int localizationRefreshVersion;

        private void Awake()
        {
            if (magicInfoFactory == null)
            {
                magicInfoFactory = FindObjectOfType<MagicInfoFactory>();
            }

            BindDropdowns();
            BindFallbackButtons();
            RefreshLocalizedText();
        }

        private void OnEnable()
        {
            LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
        }

        private void OnDisable()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
        }

        private void OnDestroy()
        {
            UnbindDropdowns();
            UnbindFallbackButtons();
        }

        private void OnSelectedLocaleChanged(Locale locale)
        {
            RefreshLocalizedText();
        }

        private void BindDropdowns()
        {
            if (sortDropdown != null)
            {
                sortDropdown.onValueChanged.AddListener(OnSortChanged);
            }

            if (attributeDropdown != null)
            {
                attributeDropdown.onValueChanged.AddListener(OnAttributeChanged);
            }

            if (actionTypeDropdown != null)
            {
                actionTypeDropdown.onValueChanged.AddListener(OnActionTypeChanged);
            }
        }

        private void BindFallbackButtons()
        {
            if (sortDropdown != null || attributeDropdown != null || actionTypeDropdown != null)
            {
                return;
            }

            BindSortButtons();
            BindFilterButtons(attributeButtons, SelectAttribute);
            BindFilterButtons(actionTypeButtons, SelectActionType);
            ApplyFallbackSelectionState();
        }

        private void UnbindFallbackButtons()
        {
            UnbindSortButtons();
            UnbindFilterButtons(attributeButtons);
            UnbindFilterButtons(actionTypeButtons);
        }

        private void BindSortButtons()
        {
            if (sortButtons == null)
            {
                return;
            }

            foreach (SortButtonBinding binding in sortButtons)
            {
                if (binding.button == null)
                {
                    continue;
                }

                MagicBookSortMode mode = binding.mode;
                binding.clickAction = () => SelectSortMode(mode);
                binding.button.onClick.AddListener(binding.clickAction);
            }
        }

        private void UnbindSortButtons()
        {
            if (sortButtons == null)
            {
                return;
            }

            foreach (SortButtonBinding binding in sortButtons)
            {
                if (binding.button != null && binding.clickAction != null)
                {
                    binding.button.onClick.RemoveListener(binding.clickAction);
                    binding.clickAction = null;
                }
            }
        }

        private void BindFilterButtons(FilterButtonBinding[] buttons, Action<FilterButtonBinding> onClick)
        {
            if (buttons == null)
            {
                return;
            }

            foreach (FilterButtonBinding binding in buttons)
            {
                if (binding.button == null)
                {
                    continue;
                }

                FilterButtonBinding captured = binding;
                binding.clickAction = () => onClick(captured);
                binding.button.onClick.AddListener(binding.clickAction);
            }
        }

        private void UnbindFilterButtons(FilterButtonBinding[] buttons)
        {
            if (buttons == null)
            {
                return;
            }

            foreach (FilterButtonBinding binding in buttons)
            {
                if (binding.button != null && binding.clickAction != null)
                {
                    binding.button.onClick.RemoveListener(binding.clickAction);
                    binding.clickAction = null;
                }
            }
        }

        private void UnbindDropdowns()
        {
            if (sortDropdown != null)
            {
                sortDropdown.onValueChanged.RemoveListener(OnSortChanged);
            }

            if (attributeDropdown != null)
            {
                attributeDropdown.onValueChanged.RemoveListener(OnAttributeChanged);
            }

            if (actionTypeDropdown != null)
            {
                actionTypeDropdown.onValueChanged.RemoveListener(OnActionTypeChanged);
            }
        }

        private async void RefreshLocalizedText()
        {
            int refreshVersion = ++localizationRefreshVersion;

            if (sortLabel != null)
            {
                sortLabel.text = await GetMagicBookText("filter.sort", "정렬");
            }

            if (refreshVersion != localizationRefreshVersion)
            {
                return;
            }

            if (attributeLabel != null)
            {
                attributeLabel.text = await GetMagicBookText("filter.attribute", "속성");
            }

            if (refreshVersion != localizationRefreshVersion)
            {
                return;
            }

            if (actionTypeLabel != null)
            {
                actionTypeLabel.text = await GetMagicBookText("filter.actionType", "타입");
            }

            if (refreshVersion != localizationRefreshVersion)
            {
                return;
            }

            await PopulateSortDropdown(refreshVersion);
            await PopulateCardDropdown(attributeDropdown, attributeOptions, refreshVersion);
            await PopulateCardDropdown(actionTypeDropdown, actionTypeOptions, refreshVersion);
        }

        private async System.Threading.Tasks.Task PopulateSortDropdown(int refreshVersion)
        {
            if (sortDropdown == null)
            {
                return;
            }

            var options = new List<string>
            {
                await GetMagicBookText("filter.name", "이름"),
                await GetMagicBookText("filter.attributeSort", "속성"),
                await GetMagicBookText("filter.cardCount", "카드 수"),
            };

            if (refreshVersion == localizationRefreshVersion)
            {
                SetOptions(sortDropdown, options, sortDropdown.value);
            }
        }

        private async System.Threading.Tasks.Task PopulateCardDropdown(TMP_Dropdown dropdown, CardType?[] cardOptions, int refreshVersion)
        {
            if (dropdown == null)
            {
                return;
            }

            var options = new List<string> { await GetMagicBookText("filter.all", "전체") };
            for (int i = 1; i < cardOptions.Length; i++)
            {
                CardType cardType = cardOptions[i].Value;
                options.Add(await GetCardText(cardType));
            }

            if (refreshVersion == localizationRefreshVersion)
            {
                SetOptions(dropdown, options, dropdown.value);
            }
        }

        private static void SetOptions(TMP_Dropdown dropdown, List<string> options, int selectedIndex)
        {
            selectedIndex = Mathf.Clamp(selectedIndex, 0, options.Count - 1);
            dropdown.ClearOptions();
            dropdown.AddOptions(options);
            dropdown.SetValueWithoutNotify(selectedIndex);
            dropdown.RefreshShownValue();
        }

        private void OnSortChanged(int index)
        {
            if (index < 0 || index >= sortOptions.Length)
            {
                return;
            }

            selectedSortMode = sortOptions[index];
            magicInfoFactory?.SetSortMode(selectedSortMode);
        }

        private void SelectSortMode(MagicBookSortMode mode)
        {
            selectedSortMode = mode;
            magicInfoFactory?.SetSortMode(mode);
            ApplyFallbackSelectionState();
        }

        private void OnAttributeChanged(int index)
        {
            if (index < 0 || index >= attributeOptions.Length)
            {
                return;
            }

            selectedAttribute = attributeOptions[index];
            magicInfoFactory?.SetAttributeFilter(selectedAttribute);
        }

        private void SelectAttribute(FilterButtonBinding binding)
        {
            selectedAttribute = binding.isAll ? null : binding.cardType;
            magicInfoFactory?.SetAttributeFilter(selectedAttribute);
            ApplyFallbackSelectionState();
        }

        private void OnActionTypeChanged(int index)
        {
            if (index < 0 || index >= actionTypeOptions.Length)
            {
                return;
            }

            selectedActionType = actionTypeOptions[index];
            magicInfoFactory?.SetActionTypeFilter(selectedActionType);
        }

        private void SelectActionType(FilterButtonBinding binding)
        {
            selectedActionType = binding.isAll ? null : binding.cardType;
            magicInfoFactory?.SetActionTypeFilter(selectedActionType);
            ApplyFallbackSelectionState();
        }

        private void ApplyFallbackSelectionState()
        {
            if (sortButtons != null)
            {
                foreach (SortButtonBinding binding in sortButtons)
                {
                    SetSelected(binding.button, binding.mode == selectedSortMode);
                }
            }

            ApplyFilterSelectionState(attributeButtons, selectedAttribute);
            ApplyFilterSelectionState(actionTypeButtons, selectedActionType);
        }

        private void ApplyFilterSelectionState(FilterButtonBinding[] buttons, CardType? selectedCardType)
        {
            if (buttons == null)
            {
                return;
            }

            foreach (FilterButtonBinding binding in buttons)
            {
                bool selected = binding.isAll ? !selectedCardType.HasValue : selectedCardType == binding.cardType;
                SetSelected(binding.button, selected);
            }
        }

        private void SetSelected(Button button, bool selected)
        {
            if (button == null)
            {
                return;
            }

            Graphic graphic = button.targetGraphic != null ? button.targetGraphic : button.GetComponent<Graphic>();
            if (graphic == null)
            {
                return;
            }

            graphic.color = selected ? selectedColor : Color.white;
        }

        private static async System.Threading.Tasks.Task<string> GetMagicBookText(string key, string fallback)
        {
            string text = await LocaleUtils.GetStringAsync(MagicBookTable, key);
            return IsMissingLocalization(text, key) ? fallback : text;
        }

        private static async System.Threading.Tasks.Task<string> GetCardText(CardType cardType)
        {
            string key = cardType.ToString();
            string text = await LocaleUtils.GetStringAsync(CardTable, key);
            return IsMissingLocalization(text, key) ? key : text;
        }

        private static bool IsMissingLocalization(string text, string key)
        {
            return string.IsNullOrWhiteSpace(text) || text == key;
        }
    }

    [Serializable]
    public class SortButtonBinding
    {
        public Button button;
        public MagicBookSortMode mode;
        [NonSerialized] public UnityAction clickAction;
    }

    [Serializable]
    public class FilterButtonBinding
    {
        public Button button;
        public bool isAll;
        public CardType cardType;
        [NonSerialized] public UnityAction clickAction;
    }
}
