using Script.Data.Deck;
using UnityEngine;

namespace Script.DeckScene
{
    public class DeckSceneContext : MonoBehaviour
    {
        public static CardDto[] OwnedCards
        {
            get; set;
        }

        public static DeckResponseDto CurrentDeck
        {
            get; set;
        }
    }
}
