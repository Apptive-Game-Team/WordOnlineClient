namespace Scripts.Data.Deck
{
    [System.Serializable]
    public class DeckResponseDto {
        public long id;
        public string name;
        public CardDto[] cards;
    }
}