namespace Simulation.Core
{
    /// <summary>
    /// Timed area explosion. Damages all objects in radius after delay.
    /// </summary>
    public class SimExplode : SimComponent
    {
        public static readonly Fix64 EXPLODE_DELAY = Fix64.Half;
        public static readonly Fix64 EXPLODE_RADIUS = Fix64.FromInt(3);

        protected readonly int Damage;
        protected readonly Fix64 Radius;
        protected readonly Fix64 Delay;
        protected Fix64 Counter;
        protected bool IsRunning;
        protected AttackInfo ExplodeAttackInfo;

        public SimExplode(int damage) : this(damage, EXPLODE_RADIUS, EXPLODE_DELAY) { }
        public SimExplode(int damage, Fix64 radius) : this(damage, radius, EXPLODE_DELAY) { }

        public SimExplode(int damage, Fix64 radius, Fix64 delay)
        {
            Damage = damage;
            Radius = radius;
            Delay = delay;
            IsRunning = true;
        }

        public override void Start()
        {
            ExplodeAttackInfo = new AttackInfo(Damage, GameObject.Element.Total());
        }

        public override void OnDestroy() { }

        public override void Update()
        {
            if (!IsRunning) return;
            Counter = Counter + SimGameConfig.DeltaTime;
            if (Counter < Delay) return;

            var hits = World.OverlapSphereAll(GameObject, Radius);
            foreach (var other in hits)
            {
                if (other == GameObject) continue;
                HandleGameObject(other);
            }
            IsRunning = false;
            GameObject.Destroy();
        }

        protected virtual void HandleGameObject(SimGameObject target)
        {
            var mob = target.GetComponent<SimMob>();
            if (mob == null) return;
            target.SetStatus(SimStatus.Damaged);
            mob.OnDamaged(ExplodeAttackInfo);
        }
    }
}
