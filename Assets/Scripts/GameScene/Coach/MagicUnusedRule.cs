using Coach;
using GameScene.Card;
using Global.Coach;
using UnityEngine;

namespace GameScene.Coach
{
    /// <summary>
    /// No card has been played for a while. The dwell timer measures exactly
    /// the gap since the last card, because using one flips this rule inactive
    /// for that frame and the scheduler resets the timer.
    /// </summary>
    public class MagicUnusedRule : GameCoachRule, ICoachRuleLifecycle
    {
        private MagicHelperUI helper;
        private bool cardUsedSinceLastCheck;

        public override CoachRuleId Id => CoachRuleId.MagicUnused;

        public override string MessageKey => "coach.magicUnused";

        public override int Priority => 4;

        public override float DwellSeconds => 25f;

        public override int MaxShowsPerSession => 2;

        public void Initialize()
        {
            CardInputSender.OnCardUsed += OnCardUsed;
        }

        public void Dispose()
        {
            CardInputSender.OnCardUsed -= OnCardUsed;
        }

        public override bool IsActive()
        {
            if (cardUsedSinceLastCheck)
            {
                cardUsedSinceLastCheck = false;
                return false;
            }

            return true;
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
                helper = Object.FindObjectOfType<MagicHelperUI>();
            }

            return helper;
        }

        private void OnCardUsed()
        {
            cardUsedSinceLastCheck = true;
        }
    }
}
