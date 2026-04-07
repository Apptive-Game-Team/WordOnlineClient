namespace Simulation.Core
{
    /// <summary>
    /// Finds the closest enemy game object matching a target mask.
    /// </summary>
    public class SimDetector
    {
        public const int DETECTING_INTERVAL = 1; // seconds (fixed-point frames)

        private readonly int _targetMask;

        public SimDetector(int targetMask)
        {
            _targetMask = targetMask;
        }

        public SimGameObject Detect(SimGameObject self, SimWorld world)
        {
            SimGameObject closest = null;
            Fix64 closestDist = Fix64.FromInt(9999);

            foreach (var target in world.GetGameObjects())
            {
                if (!target.IsActive) continue;
                if (target.Master == self.Master) continue;
                if (!target.HasComponent<SimMob>()) continue;
                if ((SimTargetMask.Of(target) & _targetMask) == 0) continue;

                Fix64 dist = self.Position.Distance(target.Position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = target;
                }
            }
            return closest;
        }
    }
}
