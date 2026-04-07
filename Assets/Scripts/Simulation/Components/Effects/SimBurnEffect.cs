namespace Simulation.Core
{
    public class SimBurnEffect : SimStatusEffect
    {
        public SimBurnEffect(Fix64 duration, SimStatusEffectKey key) : base(duration, key) { }

        public override void Start()
        {
            GameObject.SetEffect(Effect.Burn);
            var wet = GameObject.GetComponent<SimWetEffect>();
            if (wet != null)
            {
                wet.Expire();
                Expire();
            }
        }

        public override void OnAttacked(ElementType attackType) { }
    }
}
