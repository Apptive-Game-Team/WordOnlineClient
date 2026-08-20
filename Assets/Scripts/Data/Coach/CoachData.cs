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

        /// <summary>Following a hint this many times retires it permanently.</summary>
        public const int RetireAfterSatisfiedCount = 3;

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

        public static bool IsRetired(string ruleId)
        {
            return GetSatisfiedCount(ruleId) >= RetireAfterSatisfiedCount;
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

        public static void ResetSatisfied(string ruleId)
        {
            PlayerPrefs.DeleteKey(SatisfiedKeyPrefix + ruleId);
            PlayerPrefs.Save();
        }
    }
}
