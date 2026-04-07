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
                var targetPos = target.Position;
                // Safety check to keep summon within map bounds
                var spawnPos = new SimVector3(
                    SimMath.Clamp(targetPos.X, Fix64.Zero, Fix64.FromInt(SimGameConfig.WIDTH)),
                    SimMath.Clamp(targetPos.Y, Fix64.Zero, Fix64.FromInt(SimGameConfig.HEIGHT)),
                    targetPos.Z
                );

                new SimGameObject(GameObject.Master, summonPrefab, spawnPos, World);
                GameObject.SetStatus(SimStatus.Attack);
                return true;
            });
        }
    }
}
