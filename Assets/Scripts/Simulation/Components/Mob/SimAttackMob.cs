namespace Simulation.Core
{
    /// <summary>
    /// Melee attack mob — moves toward enemy and deals direct damage.
    /// </summary>
    public class SimAttackMob : SimBehaviorMob
    {
        public SimAttackMob(int maxHp, int speed, int targetMask,
            int damage, Fix64 attackInterval, Fix64 attackRange)
            : base(maxHp, speed, targetMask, attackInterval, attackRange, null)
        {
            SetBehavior(target =>
            {
                var damageable = target.GetComponent<SimMob>();
                if (damageable == null) return false;
                damageable.OnDamaged(new AttackInfo(damage, GameObject.Element.Total()));
                GameObject.SetStatus(SimStatus.Attack);
                return true;
            });
        }
    }
}
