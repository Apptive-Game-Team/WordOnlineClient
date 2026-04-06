using System.Collections.Generic;

namespace Simulation.Core
{
    /// <summary>
    /// Stationary mob that periodically heals friendly units in range.
    /// </summary>
    public class SimTotem : SimMob
    {
        private Fix64 _timer;
        private readonly AttackInfo _healInfo;
        private readonly Fix64 _healCooldown;
        private readonly Fix64 _healRange;

        public SimTotem(int maxHp, int healAmount, Fix64 attackInterval, Fix64 range)
            : base(maxHp, 0)
        {
            _healInfo = new AttackInfo(healAmount, ElementType.NATURE);
            _healCooldown = attackInterval;
            _healRange = range;
        }

        public override void Start() { }
        public override void OnDestroy() { }
        public override void OnDeath() => GameObject.Destroy();

        public override void Update()
        {
            base.Update();
            _timer = _timer + SimGameConfig.DeltaTime;
            if (_timer >= _healCooldown)
            {
                _timer = Fix64.Zero;
                var nearby = World.OverlapSphereAll(GameObject, _healRange);
                foreach (var obj in nearby)
                {
                    if (obj.Master != GameObject.Master) continue;
                    var mob = obj.GetComponent<SimMob>();
                    mob?.ApplyDamage(_healInfo);
                }
                GameObject.SetStatus(SimStatus.Attack);
            }
        }
    }
}
