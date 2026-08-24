using Coach;
using UnityEngine;

namespace GameScene.Coach
{
    /// <summary>
    /// 마나 바를 아직 한 번도 올리지 않았다. 진행이 막힌 것은 아니라서 시전이 멈춘
    /// 힌트들보다 뒤에 둔다.
    /// </summary>
    public class ManaBarUnopenedRule : GameCoachRule
    {
        private BarController barController;

        public override CoachRuleId Id => CoachRuleId.ManaBarUnopened;

        public override string MessageKey => "coach.manaBar";

        public override int Priority => 5;

        public override float DwellSeconds => 20f;

        public override int MaxShowsPerSession => 2;

        /// <summary>마나 바 버튼이 기본 자리 아래에 있어서 패널을 옆으로 비킨다.</summary>
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
