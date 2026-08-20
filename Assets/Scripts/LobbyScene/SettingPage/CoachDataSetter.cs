using System;
using Data.Coach;
using Data.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LobbyScene.SettingPage
{
    /// <summary>
    /// 로비 설정 페이지의 훈수 on/off 스위치를 연결한다. 볼륨 슬라이더와 달리 드래그 중
    /// 연달아 발생할 일이 없어서, 저장을 모으지 않고 바로 쓴다.
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
        /// 현재 상태를 글자로 적는다. 스위치 색도 같이 바뀌지만 색만으로는 애매하고,
        /// 색각 이상 유저는 알 수 없다.
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
