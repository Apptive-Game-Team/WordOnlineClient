namespace Global.Coach
{
    /// <summary>
    /// Implemented by rules that subscribe to static game events. The director
    /// calls these so subscriptions never outlive the scene that made them.
    /// </summary>
    public interface ICoachRuleLifecycle
    {
        void Initialize();

        void Dispose();
    }
}
