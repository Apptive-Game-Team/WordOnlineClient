namespace Global.Coach
{
    /// <summary>
    /// 정적 게임 이벤트를 구독하는 규칙이 구현한다. 구독이 그것을 만든 씬보다 오래
    /// 살아남지 않도록 director가 직접 호출한다.
    /// </summary>
    public interface ICoachRuleLifecycle
    {
        void Initialize();

        void Dispose();
    }
}
