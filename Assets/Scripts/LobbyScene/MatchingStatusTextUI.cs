using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Scripts.LobbyScene
{
    public class MatchingStatusTextUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        private static MatchingStatusTextUI Instance;
    
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            text.text = "";
            LobbySceneViewModel.Instance.CurrentState.OnStateChange += OnStatusChanged;
        }
    
        private void OnDestroy()
        {
            if (LobbySceneViewModel.Instance != null)
            {
                LobbySceneViewModel.Instance.CurrentState.OnStateChange -= OnStatusChanged;
            }
        }
    
        public static void SetMatchingStatusText(LocalizedString localizedString)
        {
            if (Instance != null)
            {
                localizedString.GetLocalizedStringAsync().Completed += handle =>
                {
                    Instance.text.text = handle.Result;
                };
            }
        }

        public static void SetMatchingStatusText(string status)
        {
            if (Instance != null)
            {
                Instance.text.text = status;
            }
        }
    
        private void OnStatusChanged(LobbySceneViewModel.LobbyState state)
        {
            switch (state)
            {
                case LobbySceneViewModel.LobbyState.Idle:
                    text.text = "";
                    break;
            }
        }
    }
}
