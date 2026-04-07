using System.Collections.Generic;

namespace Simulation.Core
{
    /// <summary>
    /// Moves toward center, sucks in and rotates enemies.
    /// </summary>
    public class SimTornado : SimComponent, ISimCollidable
    {
        private static readonly Fix64 ANGULAR_SPEED = Fix64.FromDouble(6.28318530718); // 360 deg/s in radians
        private static readonly Fix64 LIFT_SPEED = Fix64.FromInt(8);
        private static readonly Fix64 HEIGHT = Fix64.FromInt(5);
        private static readonly Fix64 MIN_ORBIT = Fix64.FromDouble(0.1);

        private SimVector3 _direction;
        private readonly int _damage;
        private readonly Fix64 _speed;
        private readonly Fix64 _radius;

        private readonly List<SimGameObject> _victims = new();
        private readonly Dictionary<int, Fix64> _angles = new();
        private readonly Dictionary<int, Fix64> _orbits = new();

        public SimTornado(Fix64 speed, int damage, Fix64 radius, Fix64 duration, Fix64 attackInterval)
        {
            _damage = damage;
            _speed = speed;
            _radius = radius;
            SetTarget(new SimVector3(SimGameConfig.X_MID, SimGameConfig.Y_MID, 0));
        }

        public void SetTarget(SimVector3 targetPos)
        {
            // direction is set lazily since GameObject isn't assigned yet in constructor
            _direction = targetPos.Normalized; // placeholder; recalculated in Start
        }

        public override void Start()
        {
            var target = new SimVector3(Fix64.FromInt(SimGameConfig.X_MID), Fix64.FromInt(SimGameConfig.Y_MID), Fix64.Zero);
            _direction = (target - GameObject.Position).Normalized;
        }

        public override void OnDestroy() { }

        public override void Update()
        {
            if (_direction.SqrMagnitude < Fix64.Epsilon) return;

            // Move tornado
            GameObject.SetPosition(
                GameObject.Position + _direction * _speed * SimGameConfig.DeltaTime
            );

            // Rotate victims
            var center = GameObject.Position;
            for (int i = _victims.Count - 1; i >= 0; i--)
            {
                var victim = _victims[i];
                if (victim == null || victim.IsDestroyed)
                {
                    _victims.RemoveAt(i);
                    continue;
                }

                int id = victim.Id;
                if (!_angles.ContainsKey(id) || !_orbits.ContainsKey(id)) continue;

                Fix64 angle = _angles[id] - ANGULAR_SPEED * SimGameConfig.DeltaTime;
                Fix64 r = _orbits[id];

                Fix64 newX = center.X + SimMath.Cos(angle) * r;
                Fix64 newY = center.Y + SimMath.Sin(angle) * r;

                var vp = victim.Position;
                Fix64 newZ = vp.Z < HEIGHT
                    ? SimMath.Min(vp.Z + LIFT_SPEED * SimGameConfig.DeltaTime, HEIGHT)
                    : vp.Z;

                victim.SetPosition(new SimVector3(newX, newY, newZ));
                _angles[id] = angle;
            }
        }

        public void OnCollision(SimGameObject other)
        {
            var mob = other.GetComponent<SimMob>();
            if (mob == null) return;

            if (!_victims.Contains(other))
            {
                _victims.Add(other);
                var center = GameObject.Position;
                var op = other.Position;

                // atan2 in fixed-point: use the lookup-based approach
                Fix64 dy = center.Y - op.Y;
                Fix64 dx = center.X - op.X;
                Fix64 angle = Atan2(dy, dx);
                Fix64 orbit = SimMath.Max(MIN_ORBIT, center.Distance(op));

                _angles[other.Id] = angle;
                _orbits[other.Id] = orbit;
            }
        }

        private static Fix64 Atan2(Fix64 y, Fix64 x)
        {
            // Simple atan2 approximation using the relation:
            // atan2(y,x) ≈ atan(y/x) with quadrant correction
            // For our purposes, a rough approximation is fine
            if (x == Fix64.Zero && y == Fix64.Zero) return Fix64.Zero;

            Fix64 absX = SimMath.Abs(x);
            Fix64 absY = SimMath.Abs(y);
            Fix64 minV = SimMath.Min(absX, absY);
            Fix64 maxV = SimMath.Max(absX, absY);

            // atan(a) ≈ a * (pi/4) for small a (better: polynomial)
            Fix64 a = minV / maxV;
            // Polynomial: atan(a) ≈ a - a^3/3 + a^5/5 (truncated to a for speed)
            Fix64 r = a * Fix64.FromDouble(0.7853981633974); // pi/4 approximation

            if (absY > absX) r = Fix64.HalfPi - r;
            if (x < Fix64.Zero) r = Fix64.Pi - r;
            if (y < Fix64.Zero) r = -r;
            return r;
        }
    }
}
