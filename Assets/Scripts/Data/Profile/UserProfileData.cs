using System;

namespace Data.Profile
{
    public class UserProfileData
    {
        public User user;
        public UserStatisticsOverviewDto overview;
        public UserGameHistoryDto[] games;

        public UserProfileData(User user, UserStatisticsOverviewDto overview, UserGameHistoryDto[] games)
        {
            this.user = user;
            this.overview = overview ?? new UserStatisticsOverviewDto();
            this.games = games ?? Array.Empty<UserGameHistoryDto>();
        }
    }
}
