using UnityEngine;

namespace Data.Coach
{
    /// <summary>
    /// 훈수 시스템이 기기에 남기는 상태. 유저가 켜고 끄는 스위치와, 힌트를 영구히
    /// 은퇴시키는 규칙별 카운터를 담는다.
    /// </summary>
    public static class CoachData
    {
        private const string EnabledKey = "coach.enabled";
        private const string SatisfiedKeyPrefix = "coach.satisfied.";
        private const string DismissedKeyPrefix = "coach.dismissed.";

        /// <summary>힌트를 이만큼 따르면 영구히 은퇴시킨다.</summary>
        public const int RetireAfterSatisfiedCount = 3;

        /// <summary>
        /// 힌트를 직접 닫은 횟수가 이만큼이면 은퇴시킨다. 따라서 은퇴하는 기준보다
        /// 낮은데, 직접 닫는 것은 원하지 않는다고 대놓고 말하는 것이기 때문이다.
        /// </summary>
        public const int RetireAfterDismissedCount = 2;

        private static bool enabled = true;

        static CoachData()
        {
            Load();
        }

        public static bool Enabled
        {
            get => enabled;
            set => enabled = value;
        }

        public static void Load()
        {
            enabled = PlayerPrefs.GetInt(EnabledKey, 1) != 0;
        }

        public static void Save()
        {
            PlayerPrefs.SetInt(EnabledKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static int GetSatisfiedCount(string ruleId)
        {
            return PlayerPrefs.GetInt(SatisfiedKeyPrefix + ruleId, 0);
        }

        public static int GetDismissedCount(string ruleId)
        {
            return PlayerPrefs.GetInt(DismissedKeyPrefix + ruleId, 0);
        }

        public static bool IsRetired(string ruleId)
        {
            return GetSatisfiedCount(ruleId) >= RetireAfterSatisfiedCount
                   || GetDismissedCount(ruleId) >= RetireAfterDismissedCount;
        }

        /// <summary>
        /// 힌트가 시킨 행동을 유저가 실제로 했다고 기록한다.
        /// <see cref="RetireAfterSatisfiedCount"/>에 닿으면 그 힌트는 더 뜨지 않는다.
        /// 이미 아는 것을 계속 알려 주지 않기 위함이다.
        /// </summary>
        public static void IncreaseSatisfied(string ruleId)
        {
            int next = GetSatisfiedCount(ruleId) + 1;
            if (next > RetireAfterSatisfiedCount)
            {
                return;
            }

            PlayerPrefs.SetInt(SatisfiedKeyPrefix + ruleId, next);
            PlayerPrefs.Save();
        }

        /// <summary>유저가 힌트를 직접 닫았다고 기록한다.</summary>
        public static void IncreaseDismissed(string ruleId)
        {
            int next = GetDismissedCount(ruleId) + 1;
            if (next > RetireAfterDismissedCount)
            {
                return;
            }

            PlayerPrefs.SetInt(DismissedKeyPrefix + ruleId, next);
            PlayerPrefs.Save();
        }

        public static void ResetSatisfied(string ruleId)
        {
            PlayerPrefs.DeleteKey(SatisfiedKeyPrefix + ruleId);
            PlayerPrefs.DeleteKey(DismissedKeyPrefix + ruleId);
            PlayerPrefs.Save();
        }
    }
}
