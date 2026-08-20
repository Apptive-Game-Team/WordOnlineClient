using UnityEngine;

namespace Data.Coach
{
    /// <summary>
    /// Persistent state for the coach hint system: the player-facing on/off
    /// switch and the per-rule mastery counters that retire a hint for good.
    /// </summary>
    public static class CoachData
    {
        private const string EnabledKey = "coach.enabled";
        private const string SatisfiedKeyPrefix = "coach.satisfied.";
        private const string DismissedKeyPrefix = "coach.dismissed.";

        /// <summary>Following a hint this many times retires it permanently.</summary>
        public const int RetireAfterSatisfiedCount = 3;

        /// <summary>
        /// Closing a hint by hand this many times retires it. The bar is lower
        /// than for following one, because closing it says outright that the
        /// player does not want it.
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
        /// Records that the player performed the action a hint asked for. Once
        /// this reaches <see cref="RetireAfterSatisfiedCount"/> the hint stops
        /// appearing, so the player is not nagged about something they know.
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

        /// <summary>Records that the player closed a hint by hand.</summary>
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
