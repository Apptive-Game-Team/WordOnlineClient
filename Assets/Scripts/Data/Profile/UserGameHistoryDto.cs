using System;

namespace Data.Profile
{
    [Serializable]
    public class UserGameHistoryDto
    {
        public long opponentId;
        public long opponent_id;
        public long opponentUserId;
        public long opponent_user_id;
        public string result;

        public long OpponentId
        {
            get
            {
                if (opponentId != 0)
                {
                    return opponentId;
                }

                if (opponent_id != 0)
                {
                    return opponent_id;
                }

                if (opponentUserId != 0)
                {
                    return opponentUserId;
                }

                return opponent_user_id;
            }
        }

        public bool IsWin => string.Equals(result, "win", StringComparison.OrdinalIgnoreCase);
    }
}
