namespace Simulation.Core
{
    /// <summary>
    /// Deals AOE damage to all enemies in radius on Start.
    /// Used by MagmaFist, Vine.
    /// </summary>
    public class SimOnStartAttacker : SimComponent
    {
        private readonly Fix64 _radius;
        private readonly int _damage;

        public SimOnStartAttacker(Fix64 radius, int damage)
        {
            _radius = radius;
            _damage = damage;
        }

        public override void Start()
        {
            var victims = World.OverlapSphereAll(GameObject, _radius);
            var attackInfo = new AttackInfo(_damage, GameObject.Element.Total());
            foreach (var victim in victims)
            {
                if (victim.Master == GameObject.Master) continue;
                var mob = victim.GetComponent<SimMob>();
                mob?.OnDamaged(attackInfo);
            }
        }

        public override void Update() { }
        public override void OnDestroy() { }
    }
}
