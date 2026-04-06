namespace Simulation.Core
{
    public interface ISimDamageable
    {
        void OnDamaged(AttackInfo attackInfo);
        void OnDamaged(AttackInfo attackInfo, Fix64 delay);
    }
}
