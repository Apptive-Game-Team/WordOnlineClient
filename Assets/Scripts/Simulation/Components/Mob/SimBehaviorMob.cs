using System;
using System.Collections.Generic;

namespace Simulation.Core
{
    /// <summary>
    /// State-machine mob with Idle → Move → Attack states.
    /// Mirrors server's BehaviorMob.java.
    /// </summary>
    public class SimBehaviorMob : SimMob
    {
        private readonly SimPathFinder _pathFinder = new();
        private readonly SimDetector _detector;
        private SimGameObject _target;
        private Fix64 _targetRadius;
        private SimRigidBody _rigidBody;
        public Stat AttackInterval { get; }
        protected Fix64 AttackRange;

        private Func<SimGameObject, bool> _behavior;
        private Action _stateUpdate;
        private Action _stateExit;

        // State-specific fields
        private Fix64 _timer;
        private List<SimVector2> _path;

        public SimBehaviorMob(int maxHp, int speed, int targetMask,
            Fix64 attackInterval, Fix64 attackRange, Func<SimGameObject, bool> behavior)
            : base(maxHp, speed)
        {
            _detector = new SimDetector(targetMask);
            AttackInterval = new Stat(attackInterval);
            AttackRange = attackRange;
            _behavior = behavior;
        }

        public void SetBehavior(Func<SimGameObject, bool> behavior) => _behavior = behavior;

        public override void Start()
        {
            _rigidBody = GameObject.GetComponent<SimRigidBody>();
            EnterIdle();
        }

        public override void Update()
        {
            base.Update(); // delayed attacks
            _stateUpdate?.Invoke();
        }

        public override void OnDestroy()
        {
            _stateExit?.Invoke();
        }

        public override void OnDeath() => GameObject.Destroy();

        public void SetStun(Fix64 duration)
        {
            ExitCurrentState();
            _timer = Fix64.Zero;
            _stateUpdate = () =>
            {
                _timer = _timer + SimGameConfig.DeltaTime;
                if (_timer > duration) EnterIdle();
            };
            _stateExit = null;
        }

        public void SetIdle() => EnterIdle();

        // ── Idle ──
        private void EnterIdle()
        {
            ExitCurrentState();
            _timer = Fix64.FromInt(SimDetector.DETECTING_INTERVAL) + Fix64.One;
            _stateUpdate = IdleUpdate;
            _stateExit = null;
        }

        private void IdleUpdate()
        {
            _timer = _timer + SimGameConfig.DeltaTime;
            if (_timer > Fix64.FromInt(SimDetector.DETECTING_INTERVAL))
            {
                _target = _detector.Detect(GameObject, World);
                if (_target != null)
                {
                    var cc = _target.GetComponent<SimCircleCollider>();
                    _targetRadius = cc != null ? cc.Radius : Fix64.Zero;
                    EnterMove();
                    return;
                }
                _timer = Fix64.Zero;
            }
        }

        // ── Move ──
        private void EnterMove()
        {
            ExitCurrentState();
            if (_target == null)
            {
                EnterIdle();
                return;
            }
            _path = _pathFinder.FindPath(GameObject.Position.ToVector2(), _target.Position.ToVector2());
            _timer = Fix64.Zero;
            _stateUpdate = MoveUpdate;
            _stateExit = null;
        }

        private void MoveUpdate()
        {
            if (_target.IsDestroyed || _path == null || _path.Count == 0)
            {
                EnterIdle();
                return;
            }

            SimVector2 curPos = GameObject.Position.ToVector2();

            if (curPos.Distance(_path[0]) < SimPathFinder.REACH_THRESHOLD)
            {
                _path.RemoveAt(0);
                if (_path.Count == 0)
                {
                    EnterIdle();
                    return;
                }
            }

            Fix64 distToTarget = GameObject.Position.Distance(_target.Position) - _targetRadius;
            Fix64 threshold = AttackRange - Fix64.FromDouble(0.1);
            if (distToTarget <= threshold)
            {
                EnterAttack();
                return;
            }

            _timer = _timer + SimGameConfig.DeltaTime;
            if (_timer > Fix64.FromInt(SimDetector.DETECTING_INTERVAL))
            {
                var newTarget = _detector.Detect(GameObject, World);
                if (newTarget != null && newTarget != _target)
                {
                    _target = newTarget;
                    var cc = newTarget.GetComponent<SimCircleCollider>();
                    _targetRadius = cc != null ? cc.Radius : Fix64.Zero;
                    _path = _pathFinder.FindPath(GameObject.Position.ToVector2(), _target.Position.ToVector2());
                    if (_path.Count == 0) return;
                }
                _timer = Fix64.Zero;
            }

            if (_path.Count == 0) return;
            SimVector2 dir = (_path[0] - curPos).Normalized;
            SimVector2 vel = dir * Speed.Total;

            if (_rigidBody == null)
            {
                EnterIdle();
                return;
            }
            _rigidBody.AddVelocity(vel.ToVector3());
        }

        // ── Attack ──
        private void EnterAttack()
        {
            ExitCurrentState();
            _timer = Fix64.Zero;
            _stateUpdate = AttackUpdate;
            _stateExit = null;
        }

        private void AttackUpdate()
        {
            if (_target.IsDestroyed)
            {
                EnterIdle();
                return;
            }
            _timer = _timer + SimGameConfig.DeltaTime;

            Fix64 distToTarget = GameObject.Position.Distance(_target.Position) - _targetRadius;
            if (distToTarget > AttackRange)
            {
                EnterMove();
            }
            else if (_timer > AttackInterval.Total)
            {
                _timer = Fix64.Zero;
                if (_behavior == null || !_behavior(_target))
                {
                    EnterIdle();
                }
            }
        }

        private void ExitCurrentState()
        {
            _stateExit?.Invoke();
        }
    }
}
