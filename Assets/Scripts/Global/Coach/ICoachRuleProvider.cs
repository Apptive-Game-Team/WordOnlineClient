using System.Collections.Generic;

namespace Global.Coach
{
    /// <summary>
    /// Supplies the rules for one scene. Sitting beside
    /// <see cref="CoachDirector"/> on the same object keeps the director itself
    /// scene-agnostic: a scene opts into coaching by adding a provider.
    /// </summary>
    public interface ICoachRuleProvider
    {
        IEnumerable<ICoachRule> CreateRules();
    }
}
