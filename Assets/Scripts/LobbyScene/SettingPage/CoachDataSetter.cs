using System;
using Data.Coach;
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
        [SerializeField] private Toggle coachToggle;

        public static event Action OnCoachDataChanged;

        private void Awake()
        {
            if (coachToggle == null)
            {
                return;
            }

            coachToggle.SetIsOnWithoutNotify(CoachData.Enabled);
            coachToggle.onValueChanged.AddListener(OnToggleChanged);
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
            OnCoachDataChanged?.Invoke();
        }
    }
}
