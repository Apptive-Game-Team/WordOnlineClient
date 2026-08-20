using System;
using Data.Coach;
using Data.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LobbyScene.SettingPage
{
    /// <summary>
    /// Binds the coach hint on/off switch in the lobby settings page. Unlike
    /// the volume sliders this cannot fire repeatedly while dragging, so the
    /// write goes straight through instead of being coalesced.
    /// </summary>
    public class CoachDataSetter : MonoBehaviour
    {
        private const string LobbyUiTable = "LobbyUI";
        private const string TitleKey = "CoachHint";
        private const string OnKey = "CoachHintOn";
        private const string OffKey = "CoachHintOff";

        [SerializeField] private Toggle coachToggle;

        [SerializeField] private TMP_Text titleLabel;

        /// <summary>
        /// Spells out the current state in words. The switch also changes color,
        /// but color alone reads as ambiguous and leaves colorblind players guessing.
        /// </summary>
        [SerializeField] private TMP_Text stateLabel;

        public static event Action OnCoachDataChanged;

        private void Awake()
        {
            if (coachToggle == null)
            {
                return;
            }

            coachToggle.SetIsOnWithoutNotify(CoachData.Enabled);
            coachToggle.onValueChanged.AddListener(OnToggleChanged);

            SetLocalizedText(titleLabel, TitleKey);
            RefreshStateLabel(CoachData.Enabled);
        }

        private void OnDestroy()
        {
            if (coachToggle != null)
            {
                coachToggle.onValueChanged.RemoveListener(OnToggleChanged);
            }
        }

        private void OnToggleChanged(bool value)
        {
            CoachData.Enabled = value;
            CoachData.Save();
            RefreshStateLabel(value);
            OnCoachDataChanged?.Invoke();
        }

        private void RefreshStateLabel(bool enabled)
        {
            SetLocalizedText(stateLabel, enabled ? OnKey : OffKey);
        }

        private static async void SetLocalizedText(TMP_Text target, string key)
        {
            if (target == null)
            {
                return;
            }

            string localized = await LocaleUtils.GetStringAsync(LobbyUiTable, key);

            // 비동기로 돌아오는 사이에 페이지가 닫혔을 수 있다.
            if (target == null)
            {
                return;
            }

            target.text = string.IsNullOrWhiteSpace(localized) ? key : localized;
        }
    }
}
