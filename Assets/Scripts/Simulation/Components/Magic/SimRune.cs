namespace Simulation.Core
{
    /// <summary>
    /// Static trap that explodes on collision.
    /// </summary>
    public class SimRune : SimComponent, ISimCollidable
    {
        private readonly Fix64 _radius;
        private readonly int _damage;

        public SimRune(Fix64 radius, int damage)
        {
            _radius = radius;
            _damage = damage;
        }

        public override void Start() { }
        public override void Update() { }
        public override void OnDestroy() { }

        public void OnCollision(SimGameObject other)
        {
            var attackInfo = new AttackInfo(_damage, GameObject.Element.Total());
            var targets = World.OverlapSphereAll(GameObject, _radius);
            foreach (var target in targets)
            {
                var mob = target.GetComponent<SimMob>();
                mob?.OnDamaged(attackInfo);
            }
            GameObject.Destroy();
        }
    }
}
