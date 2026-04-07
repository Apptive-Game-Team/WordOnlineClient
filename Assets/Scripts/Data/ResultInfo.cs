using System;

namespace Data
{
    [Serializable]
    public class ResultInfo
    {
        public string type = "result";
        public string leftPlayer;
        public string rightPlayer;
        public short lastLeftPlayerMmr;
        public short lastRightPlayerMmr;
        public short newLeftPlayerMmr;
        public short newRightPlayerMmr;
    }
}
