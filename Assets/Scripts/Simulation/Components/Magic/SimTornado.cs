using System.Collections.Generic;

namespace Simulation.Core
{
    /// <summary>
    /// Moves toward center, sucks in and rotates enemies.
    /// </summary>
    public class SimTornado : SimComponent, ISimCollidable
    {
        private static readonly Fix64 ANGULAR_SPEED = Fix64.FromDouble(3.14159265359); // 180 deg/s in radians
        private static readonly Fix64 LIFT_SPEED = Fix64.FromInt(5);
        private static readonly Fix64 HEIGHT = Fix64.FromInt(3);
        private static readonly Fix64 MIN_ORBIT = Fix64.FromDouble(0.1);

        private SimVector3 _direction;
        private readonly int _damage;
        private readonly Fix64 _speed;
        private readonly Fix64 _radius;
        private readonly Fix64 _attackInterval;
        private Fix64 _attackTimer;

        private readonly List<SimGameObject> _victims = new();
        private readonly Dictionary<int, Fix64> _angles = new();
        private readonly Dictionary<int, Fix64> _orbits = new();

        public SimTornado(Fix64 speed, int damage, Fix64 radius, Fix64 duration, Fix64 attackInterval)
        {
            _damage = damage;
            _speed = speed;
            _radius = radius;
            _attackInterval = attackInterval;
            SetTarget(new SimVector3(Fix64.FromInt(SimGameConfig.X_MID), Fix64.FromInt(SimGameConfig.Y_MID), Fix64.Zero));
        }

        public void SetTarget(SimVector3 targetPos)
        {
            _direction = (targetPos - GameObject.Position).Normalized;
        }

        public override void Start()
        {
            var target = new SimVector3(Fix64.FromInt(SimGameConfig.X_MID), Fix64.FromInt(SimGameConfig.Y_MID), Fix64.Zero);
            _direction = (target - GameObject.Position).Normalized;
        }

        public override void OnDestroy() { }

        public override void Update()
        {
            // Move tornado toward center
            GameObject.SetPosition(
                GameObject.Position + _direction * _speed * SimGameConfig.DeltaTime
            );

            // 1. Rotate victims
            var center = GameObject.Position;
            for (int i = _victims.Count - 1; i >= 0; i--)
            {
                var victim = _victims[i];
                if (victim == null || victim.IsDestroyed || !victim.IsActive)
                {
                    _victims.RemoveAt(i);
                    continue;
                }

                int id = victim.Id;
                if (!_angles.ContainsKey(id) || !_orbits.ContainsKey(id)) continue;

                // Orbit rotation
                Fix64 angle = _angles[id] + ANGULAR_SPEED * SimGameConfig.DeltaTime;
                Fix64 r = _orbits[id];

                Fix64 newX = center.X + SimMath.Cos(angle) * r;
                Fix64 newY = center.Y + SimMath.Sin(angle) * r;

                // Lift up
                var vp = victim.Position;
                Fix64 newZ = vp.Z < HEIGHT
                    ? SimMath.Min(vp.Z + LIFT_SPEED * SimGameConfig.DeltaTime, HEIGHT)
                    : vp.Z;

                victim.SetPosition(new SimVector3(newX, newY, newZ));
                _angles[id] = angle;
            }

            // 2. Periodic damage to all victims
            _attackTimer += SimGameConfig.DeltaTime;
            if (_attackTimer >= _attackInterval)
            {
                _attackTimer = Fix64.Zero;
                foreach (var victim in _victims)
                {
                    var mob = victim.GetComponent<SimMob>();
                    mob?.OnDamaged(new AttackInfo(_damage, GameObject.Element.Total()));
                }
            }
        }

        public void OnCollision(SimGameObject other)
        {
            if (other.Master == GameObject.Master) return;
            var mob = other.GetComponent<SimMob>();
            if (mob == null) return;

            if (!_victims.Contains(other))
            {
                _victims.Add(other);
                var center = GameObject.Position;
                var op = other.Position;

                Fix64 dy = op.Y - center.Y;
                Fix64 dx = op.X - center.X;
                Fix64 angle = Atan2(dy, dx);
                Fix64 orbit = SimMath.Max(MIN_ORBIT, center.Distance(op));

                _angles[other.Id] = angle;
                _orbits[other.Id] = orbit;
            }
        }

        private static Fix64 Atan2(Fix64 y, Fix64 x)
        {
            if (x == Fix64.Zero && y == Fix64.Zero) return Fix64.Zero;

            Fix64 absX = SimMath.Abs(x);
            Fix64 absY = SimMath.Abs(y);
            Fix64 minV = SimMath.Min(absX, absY);
            Fix64 maxV = SimMath.Max(absX, absY);

            Fix64 a = minV / maxV;
            // More precise atan(a) approximation: a - a^3/3 + a^5/5
            Fix64 a2 = a * a;
            Fix64 r = a * (Fix64.One - a2 / Fix64.FromDouble(3) + a2 * a2 / Fix64.FromInt(5));
            r = r * Fix64.FromDouble(0.78539816339); // Correcting factor to make atan(1) = pi/4

            if (absY > absX) r = Fix64.HalfPi - r;
            if (x < Fix64.Zero) r = Fix64.Pi - r;
            if (y < Fix64.Zero) r = -r;
            return r;
        }
    }
}
