namespace Simulation.Core
{
    public class SimShockEffect : SimStatusEffect
    {
        private readonly Fix64 _duration;

        public SimShockEffect(Fix64 duration, SimStatusEffectKey key) : base(duration, key)
        {
            _duration = duration;
        }

        public override void Start()
        {
            GameObject.SetEffect(Effect.Shock);
            var zp = GameObject.GetComponent<SimZPhysics>();
            zp?.LockHover(this);
            var behavior = GameObject.GetComponent<SimBehaviorMob>();
            behavior?.SetStun(_duration);
        }

        public override void OnAttacked(ElementType attackType) { }

        public override void Expire()
        {
            var zp = GameObject.GetComponent<SimZPhysics>();
            zp?.UnlockHover(this);
            var behavior = GameObject.GetComponent<SimBehaviorMob>();
            behavior?.SetIdle();
            base.Expire();
        }
    }
}
