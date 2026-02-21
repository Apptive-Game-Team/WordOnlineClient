using Scripts.Data.Deck;
using UnityEngine;

namespace Scripts.DeckScene
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
