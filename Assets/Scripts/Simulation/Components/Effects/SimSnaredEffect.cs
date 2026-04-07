namespace Simulation.Core
{
    public class SimSnaredEffect : SimStatusEffect
    {
        private static readonly Fix64 SLOW_PERCENT = Fix64.Half;
        private readonly int _removalDamage;

        public SimSnaredEffect(Fix64 duration, int removalDamage, SimStatusEffectKey key) : base(duration, key)
        {
            _removalDamage = removalDamage;
        }

        public override void Start()
        {
            GameObject.SetEffect(Effect.Snared);
            var mob = GameObject.GetComponent<SimMob>();
            if (mob != null)
                mob.Speed.ModifierPercent = -SLOW_PERCENT;
            var zp = GameObject.GetComponent<SimZPhysics>();
            zp?.LockHover(this);
            var behavior = GameObject.GetComponent<SimBehaviorMob>();
            if (behavior != null)
                behavior.AttackInterval.ModifierPercent = SLOW_PERCENT;
        }

        public override void OnAttacked(ElementType attackType)
        {
            if (attackType == ElementType.FIRE)
            {
                var mob = GameObject.GetComponent<SimMob>();
                mob?.ApplyDamage(new AttackInfo(_removalDamage, ElementType.FIRE));
                Expire();
            }
        }

        public override void Expire()
        {
            var mob = GameObject.GetComponent<SimMob>();
            if (mob != null)
                mob.Speed.ModifierPercent = Fix64.Zero;
            var zp = GameObject.GetComponent<SimZPhysics>();
            zp?.UnlockHover(this);
            var behavior = GameObject.GetComponent<SimBehaviorMob>();
            if (behavior != null)
                behavior.AttackInterval.ModifierPercent = Fix64.Zero;
            base.Expire();
        }
    }
}
