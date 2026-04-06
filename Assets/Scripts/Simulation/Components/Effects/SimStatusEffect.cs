namespace Simulation.Core
{
    /// <summary>
    /// Base status effect with duration. Mirrors server's BaseStatusEffect.
    /// </summary>
    public abstract class SimStatusEffect : SimComponent
    {
        protected Fix64 InitialDuration;
        protected Fix64 Remaining;
        public SimStatusEffectKey Key { get; }

        protected SimStatusEffect(Fix64 duration, SimStatusEffectKey key)
        {
            InitialDuration = duration;
            Remaining = duration;
            Key = key;
        }

        public void ResetDuration() => Remaining = InitialDuration;

        public override void Update()
        {
            Remaining = Remaining - SimGameConfig.DeltaTime;
            if (Remaining <= Fix64.Zero)
                Expire();
        }

        public abstract void OnAttacked(ElementType attackType);

        public virtual void Expire()
        {
            GameObject.SetEffect(Effect.None);
            GameObject.RemoveComponent(this);
        }

        public void Refresh(Fix64 duration)
        {
            InitialDuration = duration;
            Remaining = duration;
            Start();
        }

        public void Extend(Fix64 duration)
        {
            Remaining = Remaining + duration;
        }

        public override void OnDestroy() { }
    }
}
