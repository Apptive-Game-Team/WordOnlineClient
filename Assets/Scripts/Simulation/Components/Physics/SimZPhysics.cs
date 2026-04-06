using System.Collections.Generic;
using Unity.VisualScripting;

namespace Simulation.Core
{
    public class SimZPhysics : SimComponent
    {
        private Fix64 _zVelocity;
        public Fix64 GroundZ { get; set; } = Fix64.Zero;
        public Fix64 Gravity { get; set; } = SimGameConfig.GRAVITY_ACCEL;
        private readonly Fix64 _floatingVelocity = SimGameConfig.DEFAULT_FLOATING_VELOCITY;

        private readonly Fix64 _hoverZ;
        private readonly bool _isHover;
        private readonly HashSet<object> _hoverLocks = new(ReferenceEqualityComparer.Instance);
        public Fix64 FallThreshold { get; set; } = SimGameConfig.FALL_THRESHOLD;
        public Fix64 FallDamageVelocityUnit { get; set; } = SimGameConfig.FALL_THRESHOLD_VELOCITY;

        public SimZPhysics()
        {
            _isHover = false;
            _hoverZ = Fix64.Zero;
        }

        public SimZPhysics(Fix64 hoverZ)
        {
            _hoverZ = hoverZ;
            _isHover = true;
        }

        public void AddImpulseZ(Fix64 force) => _zVelocity = _zVelocity + force;

        public bool CanHover() => _isHover && _hoverLocks.Count == 0;
        public void LockHover(object obj) => _hoverLocks.Add(obj);
        public void UnlockHover(object obj) => _hoverLocks.Remove(obj);

        public void ApplyHover()
        {
            var p = GameObject.Position;
            Fix64 curZ = p.Z;
            if (curZ != _hoverZ)
            {
                Fix64 deltaZ = _floatingVelocity * SimGameConfig.DeltaTime;
                Fix64 diff = SimMath.Abs(curZ - _hoverZ);
                if (diff < deltaZ)
                {
                    GameObject.SetPosition(new SimVector3(p.X, p.Y, _hoverZ));
                }
                else
                {
                    Fix64 direction = _hoverZ > curZ ? Fix64.One : Fix64.MinusOne;
                    GameObject.SetPosition(new SimVector3(p.X, p.Y, curZ + deltaZ * direction));
                }
            }
        }

        public void ApplyZForce()
        {
            Fix64 dt = SimGameConfig.DeltaTime;
            _zVelocity = _zVelocity - Gravity * dt;

            var p = GameObject.Position;
            Fix64 curZ = p.Z;
            Fix64 z = curZ + _zVelocity * dt;

            if (z < GroundZ)
            {
                z = GroundZ;
                if (_zVelocity < Fix64.Zero && SimMath.Abs(curZ - GroundZ) > FallThreshold)
                {
                    int fallDamage = SimMath.FloorToInt(SimMath.Abs(_zVelocity) / FallDamageVelocityUnit);
                    // Fall damage is applied via Mob component — handled in SimPhysicSystem
                    var mob = GameObject.GetComponent<SimComponent>(); // Mob lookup deferred to task #9
                }
                _zVelocity = Fix64.Zero;
            }
            if (curZ != z) GameObject.SetPosition(new SimVector3(p.X, p.Y, z));
        }

        public override void Start() { }
        public override void Update() { }
        public override void OnDestroy() { }
    }
}
