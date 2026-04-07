using System.Collections.Generic;

namespace Simulation.Core
{
    /// <summary>
    /// Shot that pushes enemies on collision. Used by TideCall.
    /// Moves in a direction, applying knockback to hit enemies.
    /// </summary>
    public class SimPushShot : SimComponent, ISimCollidable
    {
        private readonly int _damage;
        private readonly Fix64 _speed;
        private SimVector3 _direction;
        private readonly List<SimGameObject> _victims = new();

        public SimPushShot(int damage, Fix64 speed)
        {
            _damage = damage;
            _speed = speed;
        }

        public SimVector3 Direction => _direction;

        public void SetDirection(SimVector3 dir)
        {
            _direction = dir.Normalized;
        }

        public override void Start()
        {
            if (_direction.SqrMagnitude < Fix64.Epsilon)
            {
                var center = new SimVector3(Fix64.FromInt(SimGameConfig.X_MID), Fix64.FromInt(SimGameConfig.Y_MID), Fix64.Zero);
                _direction = (center - GameObject.Position).Normalized;
            }
        }

        public override void Update()
        {
            if (_direction.SqrMagnitude < Fix64.Epsilon) return;

            var delta = _direction * _speed * SimGameConfig.DeltaTime;
            var newPos = GameObject.Position + delta;
            GameObject.SetPosition(newPos);

            // Push victims along with the shot
            for (int i = _victims.Count - 1; i >= 0; i--)
            {
                var victim = _victims[i];
                if (victim == null || victim.IsDestroyed || !victim.IsActive)
                {
                    _victims.RemoveAt(i);
                    continue;
                }
                victim.SetPosition(victim.Position + delta);
            }

            if (SimGameConfig.IsOutOfBounds(newPos))
                GameObject.Destroy();
        }

        public override void OnDestroy() { }

        public void OnCollision(SimGameObject other)
        {
            if (other.Master == GameObject.Master) return;
            var mob = other.GetComponent<SimMob>();
            if (mob == null) return;

            // Apply damage once
            if (!_victims.Contains(other))
            {
                _victims.Add(other);
                mob.OnDamaged(new AttackInfo(_damage, GameObject.Element.Total()));
            }
        }
    }
}
