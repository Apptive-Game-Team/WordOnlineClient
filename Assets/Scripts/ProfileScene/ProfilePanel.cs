using Data;
using Data.Profile;
using TMPro;
using UnityEngine;

namespace ProfileScene
{
    public class ProfilePanel : MonoBehaviour
    {
        [SerializeField] TMP_Text nameText;
        [SerializeField] TMP_Text emailText;
        [SerializeField] TMP_Text mmrText;
        [SerializeField] TMP_Text totalGamesText;
        [SerializeField] TMP_Text totalWinText;

        private void Render(UserProfileData profile)
        {
            if (profile == null)
            {
                return;
            }

            if (profile.user != null)
            {
                SetUser(profile.user);
            }

            UserStatisticsOverviewDto overview = profile.overview ?? new UserStatisticsOverviewDto();
            SetOverview(overview);
        }

        public void SetOverview(UserStatisticsOverviewDto overview)
        {
            totalGamesText.text = $"{overview.TotalGameNum}";
            totalWinText.text = $"{overview.TotalWinNum}";
        }

        public void SetUser(User user)
        {
            nameText.text = user.name;
            emailText.text = user.email;
            mmrText.text = $"{user.mmr}";
        }
    }
}