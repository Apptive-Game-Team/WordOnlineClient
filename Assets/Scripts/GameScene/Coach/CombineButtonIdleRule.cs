using Coach;
using UnityEngine;

namespace GameScene.Coach
{
    /// <summary>
    /// Cards are selected but the cast button has not been pressed. The player
    /// has done the hard half and stopped one step short.
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
