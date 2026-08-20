using Coach;

namespace GameScene.Coach
{
    /// <summary>
    /// The player confirmed a spell and the game is waiting for a cast
    /// position, but nothing is being clicked. Play is fully stalled here, so
    /// this outranks every other hint.
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
