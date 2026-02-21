namespace CustomizeScene.dto
{
    [System.Serializable]
    public class DetailDecorationDto
    {
        public long decorationId;
        public string name;
        public bool isEquipped;
        public bool unlocked;
        public string unlockText;
        public string progressText;
    }
}