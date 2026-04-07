namespace Simulation.Core
{
    public class SimKnockbackEffect : SimStatusEffect
    {
        private static readonly Fix64 KNOCKBACK_DURATION = Fix64.Half;
        private static readonly Fix64 KNOCKBACK_POWER = Fix64.Two;
        private static readonly Fix64 KNOCKBACK_POWER_Z = Fix64.FromInt(10);
        private static readonly Fix64 PROX_MIN = Fix64.FromDouble(0.2);

        private readonly SimVector3 _dir;
        private readonly Fix64 _proximity;
        private Fix64 _moved;

        public SimKnockbackEffect(SimVector3 dir, Fix64 proximity, SimStatusEffectKey key)
            : base(KNOCKBACK_DURATION, key)
        {
            _dir = dir;
            _proximity = SimMath.Max(proximity, PROX_MIN);
        }

        public override void Start()
        {
            GameObject.SetEffect(Effect.Knockback);
            var zp = GameObject.GetComponent<SimZPhysics>();
            zp?.AddImpulseZ(KNOCKBACK_POWER_Z * _proximity);
            var behavior = GameObject.GetComponent<SimBehaviorMob>();
            behavior?.SetStun(KNOCKBACK_DURATION);
        }

        public override void Update()
        {
            Fix64 dt = SimGameConfig.DeltaTime;
            Fix64 speed = KNOCKBACK_POWER / KNOCKBACK_DURATION;
            Fix64 step = speed * dt;

            Fix64 remain = KNOCKBACK_POWER - _moved;
            if (step > remain) step = remain;
            if (step <= Fix64.Zero)
            {
                Expire();
                return;
            }

            var rb = GameObject.GetComponent<SimRigidBody>();
            rb?.AddVelocity(_dir * step * _proximity);
            _moved = _moved + step;
        }

        public override void OnAttacked(ElementType attackType) { }

        public override void Expire()
        {
            var mob = GameObject.GetComponent<SimMob>();
            if (mob != null)
            {
                mob.Speed.ModifierPercent = Fix64.Zero;
                var zp = GameObject.GetComponent<SimZPhysics>();
                zp?.AddImpulseZ(-(KNOCKBACK_POWER_Z * _proximity));
            }
            base.Expire();
        }
    }
}
