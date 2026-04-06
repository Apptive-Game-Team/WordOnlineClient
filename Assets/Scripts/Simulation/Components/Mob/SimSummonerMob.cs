namespace Simulation.Core
{
    /// <summary>
    /// BehaviorMob that summons a prefab at the target's position on attack.
    /// Mirrors server's SummonerMob.
    /// </summary>
    public class SimSummonerMob : SimBehaviorMob
    {
        public SimSummonerMob(int maxHp, int speed, int targetMask,
            Fix64 attackInterval, Fix64 attackRange, PrefabType summonPrefab)
            : base(maxHp, speed, targetMask, attackInterval, attackRange, null)
        {
            SetBehavior(target =>
            {
                new SimGameObject(GameObject.Master, summonPrefab, target.Position, World);
                GameObject.SetStatus(SimStatus.Attack);
                return true;
            });
        }
    }
}
