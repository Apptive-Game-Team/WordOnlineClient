using System.Collections.Generic;
using Global.Coach;
using UnityEngine;

namespace GameScene.Coach
{
    /// <summary>
    /// Adds the in-game rules to the director beside it. A scene opts into
    /// coaching by carrying this component, so no rule list is hard-coded.
    /// </summary>
    [RequireComponent(typeof(CoachDirector))]
    public class GameCoachRuleProvider : MonoBehaviour, ICoachRuleProvider
    {
        public IEnumerable<ICoachRule> CreateRules()
        {
            return new ICoachRule[]
            {
                new FieldSelectIdleRule(),
                new CombineButtonIdleRule(),
                new MagicFailingRule(),
                new MagicUnusedRule(),
                new ManaBarUnopenedRule()
            };
        }
    }
}
