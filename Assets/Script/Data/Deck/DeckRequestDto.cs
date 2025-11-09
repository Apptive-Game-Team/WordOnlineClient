using System;

namespace Script.Data.Deck
{
    [Serializable]
    public class DeckRequestDto
    {
        public string   name;
        public long[]   cardIds;
    }
}