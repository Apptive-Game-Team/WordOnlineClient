namespace Simulation.Core
{
    public class SimWetEffect : SimStatusEffect
    {
        public SimWetEffect(Fix64 duration, SimStatusEffectKey key) : base(duration, key) { }

        public override void Start()
        {
            GameObject.SetEffect(Effect.Wet);
            GameObject.AddTempElement(ElementType.WATER, this);
            var burn = GameObject.GetComponent<SimBurnEffect>();
            if (burn != null)
            {
                burn.Expire();
                Expire();
            }
        }

        public override void OnAttacked(ElementType attackType) { }

        public override void Expire()
        {
            GameObject.RemoveTempElement(ElementType.WATER, this);
            base.Expire();
        }
    }
}
