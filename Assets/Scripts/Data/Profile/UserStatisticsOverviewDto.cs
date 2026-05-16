using System;

namespace Data.Profile
{
    [Serializable]
    public class UserStatisticsOverviewDto
    {
        public int totalGameNum;
        public int totalWinNum;

        public int TotalGameNum => totalGameNum;
        public int TotalWinNum => totalWinNum;
        public int TotalLoseNum => Math.Max(0, totalGameNum - totalWinNum);
        public float WinRate => totalGameNum <= 0 ? 0f : (float)totalWinNum / totalGameNum;
    }
}
