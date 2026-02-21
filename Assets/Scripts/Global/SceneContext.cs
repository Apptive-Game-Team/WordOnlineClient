using Scripts.Data;

namespace Scripts.Global
{
    public class SceneContext : SingletonObject<SceneContext>
    {
    
        public static string JwtToken
        {
            get; set;
        }
    
        public static long UserID => _user.id;

        private static User _user;

        public static User User
        {
            get => _user;
            set
            {
                WDebug.Log("Setting User: " + value.name + ", ID: " + value.id);
                _user = value;
            }
        }

        public static string Me
        {
            get
            {
                if (UserID == MatchInfo.leftUser.id)
                    return "LeftPlayer";
                else if (UserID == MatchInfo.rightUser.id)
                    return "RightPlayer";
                return "None";
            }
        }

        public static MatchedInfoDto MatchInfo
        {
            get; set;
        }

        public static ResultInfo MatchResult
        {
            get; set;
        }
    
        public static string SelectedDeck
        {
            get; set;
        }
        public static string OwnedCards
        {
            get; set;
        }
    
        public static void ClearContext()
        {
            JwtToken = null;
            _user = null;
            MatchInfo = null;
            MatchResult = null;
            SelectedDeck = null;
            OwnedCards = null;
            GuestContext.ClearGuestInfo();
        }
    }
}
