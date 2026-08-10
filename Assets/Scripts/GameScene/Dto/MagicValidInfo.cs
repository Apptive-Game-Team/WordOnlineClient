namespace GameScene.Dto
{
    [System.Serializable]
    public class MagicValidInfo : ServerMessage
    {
        public string message;
        public bool valid;
        public string resultCode;
        public int updatedMana;
        public int id;
        public long magicId;
    }
}
