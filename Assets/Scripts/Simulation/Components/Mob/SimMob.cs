using System.Collections.Generic;

namespace Simulation.Core
{
    /// <summary>
    /// Base mob component. Has HP, speed, takes damage with elemental multipliers.
    /// Mirrors server's Mob.java.
    /// </summary>
    public abstract class SimMob : SimComponent, ISimDamageable
    {
        public int Hp { get; protected set; }
        public int MaxHp { get; protected set; }
        public Stat Speed { get; protected set; }

        private readonly List<(AttackInfo info, Fix64 delay)> _delayedAttacks = new();

        protected SimMob(int maxHp, int speed)
        {
            MaxHp = maxHp;
            Hp = maxHp;
            Speed = new Stat(speed);
        }

        protected SimMob(int maxHp, Fix64 speed)
        {
            MaxHp = maxHp;
            Hp = maxHp;
            Speed = new Stat(speed);
        }

        public void OnDamaged(AttackInfo attackInfo)
        {
            // Trigger status effects
            var effects = GameObject.GetComponents<SimStatusEffect>();
            foreach (var effect in effects)
                foreach (var elem in attackInfo.Elements)
                    effect.OnAttacked(elem);

            ApplyDamage(attackInfo);
        }

        public void OnDamaged(AttackInfo attackInfo, Fix64 delay)
        {
            _delayedAttacks.Add((attackInfo, delay));
        }

        public virtual void ApplyDamage(AttackInfo attackInfo)
        {
            Fix64 multiplier = ElementalChart.ComputePairwiseProductMultiplier(
                attackInfo.Elements, GameObject.Element.Total());
            int damage = SimMath.FloorToInt(Fix64.FromInt(attackInfo.Damage) * multiplier);
            Hp -= damage;
            World.MarkUpdated(GameObject);

            if (Hp <= 0 && !GameObject.IsDestroyed)
                OnDeath();
            else if (Hp > MaxHp)
                Hp = MaxHp;
        }

        public override void Update()
        {
            HandleDelayedAttacks();
        }

        private void HandleDelayedAttacks()
        {
            for (int i = _delayedAttacks.Count - 1; i >= 0; i--)
            {
                var (info, delay) = _delayedAttacks[i];
                Fix64 newDelay = delay - SimGameConfig.DeltaTime;
                if (newDelay <= Fix64.Zero)
                {
                    OnDamaged(info);
                    _delayedAttacks.RemoveAt(i);
                }
                else
                {
                    _delayedAttacks[i] = (info, newDelay);
                }
            }
        }

        public abstract void OnDeath();
    }
}
