using Coach;
using UnityEngine;

namespace GameScene.Coach
{
    /// <summary>
    /// 카드는 골랐는데 주문 버튼을 누르지 않았다. 어려운 절반을 해 놓고 한 걸음 앞에서
    /// 멈춘 상태다.
    /// </summary>
    public class CombineButtonIdleRule : GameCoachRule
    {
        private MagicCombineButton combineButton;

        public override CoachRuleId Id => CoachRuleId.CombineButtonIdle;

        public override string MessageKey => "coach.combineButton";

        public override int Priority => 2;

        public override float DwellSeconds => 8f;

        public override int MaxShowsPerSession => 3;

        public override bool IsActive()
        {
            if (!TryGetInput(out Card.CardInputSender sender))
            {
                return false;
            }

            return sender.CanSelectField
                   && !sender.IsFieldSelectMode()
                   && !sender.IsWaitingInputResponse();
        }

        public override Transform[] ResolveTargets()
        {
            if (combineButton == null)
            {
                combineButton = UnityEngine.Object.FindObjectOfType<MagicCombineButton>();
            }

            return combineButton != null ? new[] { combineButton.transform } : null;
        }
    }
}
