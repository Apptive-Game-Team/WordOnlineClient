namespace Simulation.Core
{
    /// <summary>
    /// Abstract base collider. Attached to a SimGameObject for collision detection.
    /// </summary>
    public abstract class SimCollider : SimComponent
    {
        public bool IsTrigger { get; }

        protected SimCollider(bool isTrigger)
        {
            IsTrigger = isTrigger;
        }

        public SimVector3 Position => GameObject.Position;

        public abstract bool IsCollidingWith(SimCollider other);
        public abstract SimVector3 GetDisplacement(SimCollider other);

        public override void Start() { }
        public override void Update() { }
        public override void OnDestroy() { }
    }
}
