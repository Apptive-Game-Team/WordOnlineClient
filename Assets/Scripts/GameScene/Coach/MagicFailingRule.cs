using Coach;
using GameScene.Card;
using Global.Coach;
using UnityEngine;

namespace GameScene.Coach
{
    /// <summary>
    /// 시전이 계속 실패한다. 보통 어떤 카드가 조합되는지 아직 모른다는 뜻이다. 추천 목록을
    /// 가리키고 실제로 되는 카드에 테두리를 둘러 준다.
    /// </summary>
    public class MagicFailingRule : GameCoachRule, ICoachRuleLifecycle
    {
        private const int FailStreakThreshold = 3;

        private MagicHelperUI helper;
        private int failStreak;

        public override CoachRuleId Id => CoachRuleId.MagicFailing;

        public override string MessageKey => "coach.magicFailing";

        public override int Priority => 3;

        /// <summary>연속 실패를 세는 것 자체가 대기라서 dwell을 따로 두지 않는다.</summary>
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
