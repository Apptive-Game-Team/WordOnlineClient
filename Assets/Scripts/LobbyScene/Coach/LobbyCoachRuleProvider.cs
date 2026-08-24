using System.Collections.Generic;
using Global.Coach;
using UnityEngine;

namespace LobbyScene.Coach
{
    /// <summary>옆에 붙은 director에 로비 규칙을 넣어 준다.</summary>
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
