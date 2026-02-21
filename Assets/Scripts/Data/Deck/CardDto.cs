namespace Scripts.Data.Deck
{
    [System.Serializable]
    public class CardDto
    {
        public long id;
        public string name;
        public string type;
        public int count;
        public bool unlocked;
        public string unlockText;
        public string progressText;
    }
}