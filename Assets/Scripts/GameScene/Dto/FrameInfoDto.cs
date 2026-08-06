namespace GameScene.Dto
{
    [System.Serializable]
    public class FrameInfoDto : ServerMessage
    {
        public int remainingTime;

        public int updatedMana;
        public int leftPlayerHp;
        public int rightPlayerHp;
        public CardInfo cards;
        public ObjectsInfo objects;
    }
}
