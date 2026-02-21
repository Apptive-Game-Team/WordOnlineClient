using Data.Deck;
using UnityEngine;

namespace DeckScene
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
