using Coach;

namespace GameScene.Coach
{
    /// <summary>
    /// 마법을 확정해서 시전 위치를 기다리는데 아무 데도 클릭하지 않는다. 진행이 완전히
    /// 멈춘 상태라 다른 어떤 힌트보다 우선한다.
    /// </summary>
    public class FieldSelectIdleRule : GameCoachRule
    {
        public override CoachRuleId Id => CoachRuleId.FieldSelectIdle;

        public override string MessageKey => "coach.fieldSelect";

        public override int Priority => 1;

        public override float DwellSeconds => 6f;

        public override int MaxShowsPerSession => 3;

        public override bool IsActive()
        {
            return TryGetInput(out Card.CardInputSender sender) && sender.IsFieldSelectMode();
        }
    }
}
