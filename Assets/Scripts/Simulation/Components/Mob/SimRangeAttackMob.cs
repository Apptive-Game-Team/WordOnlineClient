namespace Simulation.Core
{
    /// <summary>
    /// Ranged attack mob — moves toward enemy and spawns a projectile prefab to attack.
    /// Mirrors server's ProjectileRangeAttackMob.
    /// </summary>
    public class SimRangeAttackMob : SimBehaviorMob
    {
        public SimRangeAttackMob(int maxHp, int speed, int targetMask,
            int damage, Fix64 attackInterval, Fix64 attackRange,
            PrefabType projectilePrefab)
            : base(maxHp, speed, targetMask, attackInterval, attackRange, null)
        {
            SetBehavior(target =>
            {
                // Spawn projectile at our position, aimed at target
                var proj = new SimGameObject(GameObject.Master, projectilePrefab, GameObject.Position, World);
                var shot = proj.GetComponent<SimShot>();
                if (shot != null)
                {
                    var dir = (target.Position - GameObject.Position).Normalized;
                    shot.SetDirection(dir);
                }
                GameObject.SetStatus(SimStatus.Attack);
                return true;
            });
        }
    }
}
