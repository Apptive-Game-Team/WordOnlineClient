namespace Simulation.Core
{
    /// <summary>
    /// A modifiable stat value with a percentage modifier.
    /// total = originalStat * (1 + modifierPercent)
    /// </summary>
    public class Stat
    {
        public Fix64 OriginalStat;
        public Fix64 ModifierPercent;

        public Stat(Fix64 original)
        {
            OriginalStat = original;
            ModifierPercent = Fix64.Zero;
        }

        public Stat(int original)
        {
            OriginalStat = Fix64.FromInt(original);
            ModifierPercent = Fix64.Zero;
        }

        public Fix64 Total => OriginalStat * (Fix64.One + ModifierPercent);
        public int FloorValue => SimMath.FloorToInt(Total);

        public void AddPercent(Fix64 delta) => ModifierPercent = ModifierPercent + delta;
    }
}
