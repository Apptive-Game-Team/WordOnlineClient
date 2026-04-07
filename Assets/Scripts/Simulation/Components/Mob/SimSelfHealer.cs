namespace Simulation.Core
{
    /// <summary>
    /// Periodically heals the parent mob.
    /// </summary>
    public class SimSelfHealer : SimComponent
    {
        private readonly int _healAmount;
        private readonly Fix64 _interval;
        private readonly ElementType _element;
        private Fix64 _timer;

        public SimSelfHealer(int healAmount, Fix64 interval, ElementType element)
        {
            _healAmount = healAmount;
            _interval = interval;
            _element = element;
        }

        public override void Start() { }
        public override void OnDestroy() { }

        public override void Update()
        {
            _timer = _timer + SimGameConfig.DeltaTime;
            if (_timer < _interval) return;
            _timer = Fix64.Zero;

            var mob = GameObject.GetComponent<SimMob>();
            if (mob == null) return;
            mob.ApplyDamage(new AttackInfo(-_healAmount, _element));
        }
    }
}
