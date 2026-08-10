using UnityEngine;

namespace LobbyScene.Matching
{
    /// <summary>
    /// Full screen page shown while a match ticket is alive.
    /// The page itself only handles visibility; leaving it is a normal cancel
    /// request, which the coordinator ignores once the ticket is no longer cancelable.
    /// </summary>
    public sealed class MatchingPage : MonoBehaviour
    {
        [SerializeField] private GameObject content;

        private void Start()
        {
            LobbySceneViewModel.Instance.CurrentState.OnStateChange += ApplyLobbyState;
            ApplyLobbyState(LobbySceneViewModel.Instance.CurrentState.Data);
        }

        private void OnDestroy()
        {
            if (LobbySceneViewModel.Instance == null) return;
            LobbySceneViewModel.Instance.CurrentState.OnStateChange -= ApplyLobbyState;
        }

        private void ApplyLobbyState(LobbySceneViewModel.LobbyState state)
        {
            content.SetActive(state == LobbySceneViewModel.LobbyState.Matching);
        }
    }
}
