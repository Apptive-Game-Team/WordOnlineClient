using System;

namespace Scripts.Data.Deck
{
    [Serializable]
    public class DeckRequestDto
    {
        public string   name;
        public long[]   cardIds;
    }
}