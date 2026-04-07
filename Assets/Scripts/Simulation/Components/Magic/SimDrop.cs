namespace Simulation.Core
{
    /// <summary>
    /// Falling projectile that moves down the Z axis and deals damage on collision.
    /// </summary>
    public class SimDrop : SimMob, ISimCollidable
    {
        private static readonly Fix64 SPEED = Fix64.FromInt(5);

        private int _direction;
        private readonly int _damage;

        public SimDrop(int damage) : base(1, 0)
        {
            _direction = -1;
            _damage = damage;
        }

        public override void Start() { }
        public override void OnDestroy() { }

        public override void Update()
        {
            base.Update();
            Fix64 dz = Fix64.FromInt(_direction) * SPEED * SimGameConfig.DeltaTime;
            var p = GameObject.Position;
            GameObject.SetPosition(new SimVector3(p.X, p.Y, p.Z + dz));
            if (GameObject.Position.Z < Fix64.Zero)
                GameObject.Destroy();
        }

        public override void OnDeath()
        {
            _direction = 0;
            GameObject.Destroy();
        }

        public void OnCollision(SimGameObject other)
        {
            var mob = other.GetComponent<SimMob>();
            if (mob == null) return;

            _direction = 0;
            GameObject.SetStatus(SimStatus.Attack);
            other.SetStatus(SimStatus.Damaged);
            mob.OnDamaged(new AttackInfo(_damage, GameObject.Element.Total()));
            GameObject.Destroy();
        }
    }
}
