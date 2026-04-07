namespace Simulation.Core
{
    /// <summary>
    /// Stationary mob that takes damage but doesn't move.
    /// </summary>
    public class SimDummyMob : SimMob
    {
        public SimDummyMob(int maxHp) : base(maxHp, 0) { }

        public override void Start() { }
        public override void OnDestroy() { }
        public override void OnDeath() => GameObject.Destroy();
    }
}
