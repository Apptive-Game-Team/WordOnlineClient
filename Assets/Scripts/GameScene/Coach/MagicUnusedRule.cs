using Coach;
using GameScene.Card;
using Global.Coach;
using UnityEngine;

namespace GameScene.Coach
{
    /// <summary>
    /// 한동안 카드를 한 장도 쓰지 않았다. 카드를 쓰면 그 프레임에 이 규칙이 거짓이 되고
    /// 스케줄러가 타이머를 초기화하므로, dwell 타이머가 곧 마지막 카드 이후 경과 시간이 된다.
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
                helper = UnityEngine.Object.FindObjectOfType<MagicHelperUI>();
            }

            return helper;
        }

        private void OnCardUsed()
        {
            cardUsedSinceLastCheck = true;
        }
    }
}
