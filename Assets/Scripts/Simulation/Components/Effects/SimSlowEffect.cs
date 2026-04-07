namespace Simulation.Core
{
    public class SimSlowEffect : SimStatusEffect
    {
        private readonly Fix64 _amount;

        public SimSlowEffect(Fix64 duration, Fix64 amount, SimStatusEffectKey key) : base(duration, key)
        {
            _amount = amount;
        }

        public override void Start()
        {
            var mob = GameObject.GetComponent<SimMob>();
            if (mob != null)
                mob.Speed.ModifierPercent = -_amount;
        }

        public override void OnAttacked(ElementType attackType) { }

        public override void Expire()
        {
            var mob = GameObject.GetComponent<SimMob>();
            if (mob != null)
                mob.Speed.ModifierPercent = Fix64.Zero;
            base.Expire();
        }
    }
}
