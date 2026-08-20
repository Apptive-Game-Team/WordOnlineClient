using Coach;
using GameScene.Card;
using Global.Coach;
using UnityEngine;

namespace GameScene.Coach
{
    /// <summary>
    /// Casting keeps failing, which usually means the player does not yet know
    /// which cards combine. Points at the suggestion list and outlines the
    /// cards that would actually work.
    /// </summary>
    public class MagicFailingRule : GameCoachRule, ICoachRuleLifecycle
    {
        private const int FailStreakThreshold = 3;

        private MagicHelperUI helper;
        private int failStreak;

        public override CoachRuleId Id => CoachRuleId.MagicFailing;

        public override string MessageKey => "coach.magicFailing";

        public override int Priority => 3;

        /// <summary>The streak itself is the wait, so no extra dwell is needed.</summary>
        public override float DwellSeconds => 0f;

        public override int MaxShowsPerSession => 2;

        public void Initialize()
        {
            CardInputSender.OnMagicFailed += OnMagicFailed;
            CardInputSender.OnMagicSucceeded += OnMagicSucceeded;
        }

        public void Dispose()
        {
            CardInputSender.OnMagicFailed -= OnMagicFailed;
            CardInputSender.OnMagicSucceeded -= OnMagicSucceeded;
        }

        public override bool IsActive()
        {
            return failStreak >= FailStreakThreshold;
        }

        public override Transform[] ResolveTargets()
        {
            MagicHelperUI resolved = ResolveHelper();
            return resolved != null && resolved.SuggestionRoot != null
                ? new[] { resolved.SuggestionRoot }
                : null;
        }

        public override void OnShown()
        {
            MagicHelperUI resolved = ResolveHelper();
            if (resolved != null)
            {
                resolved.TryHighlightTopSuggestion();
            }
        }

        public override void OnHidden()
        {
            MagicHelperUI resolved = ResolveHelper();
            if (resolved != null)
            {
                resolved.ClearHandHighlight();
            }
        }

        private MagicHelperUI ResolveHelper()
        {
            if (helper == null)
            {
                helper = UnityEngine.Object.FindObjectOfType<MagicHelperUI>();
            }

            return helper;
        }

        private void OnMagicFailed()
        {
            failStreak++;
        }

        private void OnMagicSucceeded()
        {
            failStreak = 0;
        }
    }
}
