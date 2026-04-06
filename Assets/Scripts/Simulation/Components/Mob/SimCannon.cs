using System.Collections.Generic;

namespace Simulation.Core
{
    /// <summary>
    /// Stationary mob that periodically attacks the closest enemy with AOE damage.
    /// Mirrors server's Cannon (TimedBehaviorMob).
    /// </summary>
    public class SimCannon : SimMob
    {
        private static readonly Fix64 AOE_RADIUS = Fix64.One;

        private readonly int _damage;
        private readonly int _targetMask;
        private readonly Fix64 _attackInterval;
        private readonly Fix64 _attackRange;
        private Fix64 _timer;
        private readonly SimDetector _detector;

        public SimCannon(int maxHp, int damage, int targetMask,
            Fix64 attackInterval, Fix64 attackRange)
            : base(maxHp, 0)
        {
            _damage = damage;
            _targetMask = targetMask;
            _attackInterval = attackInterval;
            _attackRange = attackRange;
            _detector = new SimDetector(targetMask);
        }

        public override void Start() { }
        public override void OnDestroy() { }
        public override void OnDeath() => GameObject.Destroy();

        public override void Update()
        {
            base.Update();
            _timer = _timer + SimGameConfig.DeltaTime;
            if (_timer < _attackInterval) return;

            var target = _detector.Detect(GameObject, World);
            if (target == null) return;

            Fix64 distance = GameObject.Position.Distance(target.Position);
            if (distance > _attackRange) return;

            _timer = Fix64.Zero;

            // AOE damage at target position
            var victims = World.OverlapSphereAll(target, AOE_RADIUS);
            var attackInfo = new AttackInfo(_damage, GameObject.Element.Total());
            foreach (var victim in victims)
            {
                var mob = victim.GetComponent<SimMob>();
                mob?.OnDamaged(attackInfo);
            }
            GameObject.SetStatus(SimStatus.Attack);
        }
    }
}
