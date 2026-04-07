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
            // Default direction: toward center of map
            if (_direction.SqrMagnitude < Fix64.Epsilon)
            {
                var center = new SimVector3(Fix64.FromInt(SimGameConfig.X_MID), Fix64.FromInt(SimGameConfig.Y_MID), Fix64.Zero);
                _direction = (center - GameObject.Position).Normalized;
            }
        }

        public override void Update()
        {
            if (_direction.SqrMagnitude < Fix64.Epsilon) return;

            var newPos = GameObject.Position + _direction * _speed * SimGameConfig.DeltaTime;
            GameObject.SetPosition(newPos);

            if (SimGameConfig.IsOutOfBounds(newPos))
                GameObject.Destroy();
        }

        public override void OnDestroy() { }

        public void OnCollision(SimGameObject other)
        {
            if (other.Master == GameObject.Master) return;
            var mob = other.GetComponent<SimMob>();
            if (mob == null) return;
            mob.OnDamaged(new AttackInfo(_damage, GameObject.Element.Total()));
        }
    }
}
