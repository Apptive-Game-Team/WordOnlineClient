using DeckScene;
using Global;
using TMPro;
using UnityEngine;

namespace LobbyScene.Matching
{
    public sealed class MatchingPlayerInfoUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private TMP_Text deckNameText;

        private void OnEnable()
        {
            playerNameText.text = SceneContext.User != null ? SceneContext.User.name : string.Empty;
            deckNameText.text = DeckSceneContext.CurrentDeck != null ? DeckSceneContext.CurrentDeck.name : string.Empty;
        }
    }
}
