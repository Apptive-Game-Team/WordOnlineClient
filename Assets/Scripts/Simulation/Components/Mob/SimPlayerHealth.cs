namespace Simulation.Core
{
    /// <summary>
    /// Player health component. When HP reaches 0, that player loses.
    /// </summary>
    public class SimPlayerHealth : SimMob
    {
        private const int MAX_HEALTH = 100;

        public SimPlayerHealth() : base(MAX_HEALTH, 0) { }

        public override void Start() { }
        public override void OnDestroy() { }

        public override void OnDeath()
        {
            // Signal game over — SimWorld checks for this
            World.MarkUpdated(GameObject);
            GameObject.Destroy();
        }
    }
}
