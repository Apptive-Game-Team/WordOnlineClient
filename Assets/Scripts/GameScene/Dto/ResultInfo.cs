using System;

namespace GameScene.Dto
{
    [Serializable]
    public class ResultInfo : ServerMessage
    {
        public string leftPlayer;
        public string rightPlayer;
        public short lastLeftPlayerMmr;
        public short lastRightPlayerMmr;
        public short newLeftPlayerMmr;
        public short newRightPlayerMmr;
    }
}
