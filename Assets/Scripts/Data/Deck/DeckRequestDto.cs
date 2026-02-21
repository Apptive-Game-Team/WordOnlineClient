using System;

namespace Data.Deck
{
    [Serializable]
    public class DeckRequestDto
    {
        public string   name;
        public long[]   cardIds;
    }
}