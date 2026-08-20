using System.Collections.Generic;
using Global.Coach;
using UnityEngine;

namespace LobbyScene.Coach
{
    /// <summary>Adds the lobby rules to the director beside it.</summary>
    [RequireComponent(typeof(CoachDirector))]
    public class LobbyCoachRuleProvider : MonoBehaviour, ICoachRuleProvider
    {
        public IEnumerable<ICoachRule> CreateRules()
        {
            return new ICoachRule[]
            {
                new LobbyIdleRule()
            };
        }
    }
}
