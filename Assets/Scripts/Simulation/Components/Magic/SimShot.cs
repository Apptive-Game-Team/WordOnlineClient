using System.Collections.Generic;

namespace Simulation.Core
{
    /// <summary>
    /// Projectile that moves in a direction and explodes on collision.
    /// </summary>
    public class SimShot : SimComponent, ISimCollidable
    {
        private static readonly Fix64 IMPACT_RADIUS = Fix64.One;

        protected Fix64 Speed;
        public SimVector3 Direction { get; private set; }
        protected readonly int Damage;
        private bool _explosionTriggered;

        public SimShot(int damage, Fix64 speed)
        {
            Damage = damage;
            Speed = speed;
        }

        public void SetTarget(SimVector3 targetPos)
        {
            Direction = (targetPos - GameObject.Position).Normalized;
        }

        public void SetDirection(SimVector3 direction)
        {
            Direction = direction.Normalized;
        }

        public override void Start() { }
        public override void OnDestroy() { }

        public override void Update()
        {
            if (Direction.SqrMagnitude < Fix64.Epsilon) return;
            GameObject.SetPosition(
                GameObject.Position + Direction * Speed * SimGameConfig.DeltaTime
            );
        }

        public virtual void OnCollision(SimGameObject other)
        {
            if (_explosionTriggered) return;
            var mob = other.GetComponent<SimMob>();
            if (mob == null) return;

            _explosionTriggered = true;
            Direction = SimVector3.Zero;
            GameObject.SetStatus(SimStatus.Attack);

            var hits = World.OverlapSphereAll(GameObject, IMPACT_RADIUS);
            var attackInfo = new AttackInfo(Damage, GameObject.Element.Total());
            foreach (var hit in hits)
            {
                if (hit == GameObject || hit.Master == GameObject.Master) continue;
                var target = hit.GetComponent<SimMob>();
                if (target == null) continue;
                hit.SetStatus(SimStatus.Damaged);
                target.OnDamaged(attackInfo);
            }
            GameObject.Destroy();
        }
    }
}
