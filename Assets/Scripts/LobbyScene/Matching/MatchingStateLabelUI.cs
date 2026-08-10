using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace LobbyScene.Matching
{
    /// <summary>
    /// Shows the ticket state behind the matching page, so waiting, allocating and
    /// reconnecting are distinguishable instead of collapsing into one "matching" label.
    /// </summary>
    public sealed class MatchingStateLabelUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private LocalizedString queuedLabel;
        [SerializeField] private LocalizedString allocatingLabel;
        [SerializeField] private LocalizedString matchedLabel;
        [SerializeField] private LocalizedString reconnectingLabel;
        [SerializeField] private LocalizedString cancelPendingLabel;

        private void OnEnable()
        {
            LobbySceneViewModel.Instance.TicketState.OnStateChange += Render;
            Render(LobbySceneViewModel.Instance.TicketState.Data);
        }

        private void OnDisable()
        {
            if (LobbySceneViewModel.Instance == null) return;
            LobbySceneViewModel.Instance.TicketState.OnStateChange -= Render;
        }

        private void Render(MatchTicketState state)
        {
            LocalizedString label = ResolveLabel(state);
            text.text = label == null || label.IsEmpty ? string.Empty : label.GetLocalizedString();
        }

        private LocalizedString ResolveLabel(MatchTicketState state)
        {
            switch (state)
            {
                case MatchTicketState.Queued:
                    return queuedLabel;
                case MatchTicketState.Allocating:
                    return allocatingLabel;
                case MatchTicketState.Matched:
                    return matchedLabel;
                case MatchTicketState.Reconnecting:
                    return reconnectingLabel;
                case MatchTicketState.CancelPending:
                    return cancelPendingLabel;
                default:
                    return null;
            }
        }
    }
}
