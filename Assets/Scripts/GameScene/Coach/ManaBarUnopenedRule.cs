using Coach;
using UnityEngine;

namespace GameScene.Coach
{
    /// <summary>
    /// The mana bar is still tucked away. Nothing blocks the player here, so
    /// this sits below the hints about a stalled cast.
    /// </summary>
    public class ManaBarUnopenedRule : GameCoachRule
    {
        private BarController barController;

        public override CoachRuleId Id => CoachRuleId.ManaBarUnopened;

        public override string MessageKey => "coach.manaBar";

        public override int Priority => 5;

        public override float DwellSeconds => 20f;

        public override int MaxShowsPerSession => 2;

        /// <summary>The mana bar button sits below the primary spot, so move aside.</summary>
        public override bool UseAlternatePlacement => true;

        public override bool IsActive()
        {
            BarController resolved = ResolveBar();
            return resolved != null && !resolved.IsBarOpen;
        }

        public override Transform[] ResolveTargets()
        {
            BarController resolved = ResolveBar();
            if (resolved == null)
            {
                return null;
            }

            Transform button = resolved.ManaBarButtonTransform;
            return button != null ? new[] { button } : null;
        }

        private BarController ResolveBar()
        {
            if (barController == null)
            {
                barController = UnityEngine.Object.FindObjectOfType<BarController>();
            }

            return barController;
        }
    }
}
