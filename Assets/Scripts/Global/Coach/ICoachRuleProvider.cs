using System.Collections.Generic;

namespace Global.Coach
{
    /// <summary>
    /// 한 씬의 규칙을 공급한다. <see cref="CoachDirector"/>와 같은 오브젝트에 붙으며,
    /// 덕분에 director는 씬을 몰라도 된다. 씬은 provider를 붙이는 것으로 훈수에 참여한다.
    /// </summary>
    public interface ICoachRuleProvider
    {
        IEnumerable<ICoachRule> CreateRules();
    }
}
