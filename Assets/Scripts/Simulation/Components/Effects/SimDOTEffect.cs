namespace Simulation.Core
{
    /// <summary>
    /// Damage/heal over time. Positive totalAmount = damage, negative = heal.
    /// </summary>
    public class SimDOTEffect : SimStatusEffect
    {
        private Fix64 _curTick;
        private readonly Fix64 _tickInterval;
        private readonly int _unit; // +1 or -1
        private readonly ElementType _elementType;

        public SimDOTEffect(Fix64 duration, int totalAmount, ElementType elementType, SimStatusEffectKey key)
            : base(duration, key)
        {
            _elementType = elementType;
            int totalUnits = totalAmount < 0 ? -totalAmount : totalAmount;
            if (totalUnits == 0) totalUnits = 1;
            _tickInterval = InitialDuration / Fix64.FromInt(totalUnits);
            _unit = totalAmount >= 0 ? 1 : -1;
        }

        public override void Start() { }

        public override void Update()
        {
            base.Update();
            _curTick = _curTick + SimGameConfig.DeltaTime;
            int ticks = SimMath.FloorToInt(_curTick / _tickInterval);
            if (ticks <= 0) return;
            _curTick = _curTick - Fix64.FromInt(ticks) * _tickInterval;

            var mob = GameObject.GetComponent<SimMob>();
            mob?.ApplyDamage(new AttackInfo(_unit * ticks, _elementType));
        }

        public override void OnAttacked(ElementType attackType) { }
    }
}
