namespace Simulation.Core
{
    /// <summary>
    /// On collision, applies knockback effect based on direction and proximity.
    /// </summary>
    public class SimKnockbackProvider : SimComponent, ISimCollidable
    {
        public override void Start() { }
        public override void Update() { }
        public override void OnDestroy() { }

        public void OnCollision(SimGameObject other)
        {
            var receiver = other.GetComponent<SimEffectReceiver>();
            if (receiver == null) return;

            SimVector3 dir = GetDirection(other);
            Fix64 prox = GetProximity(other);
            receiver.OnReceiveKnockback(dir, prox);
        }

        private SimVector3 GetDirection(SimGameObject target)
        {
            var shot = GameObject.GetComponent<SimShot>();
            if (shot != null) return shot.Direction;

            // Explode: direction from center to target
            return (target.Position - GameObject.Position).Normalized;
        }

        private Fix64 GetProximity(SimGameObject target)
        {
            Fix64 dist = GameObject.Position.Distance(target.Position);
            var explode = GameObject.GetComponent<SimExplode>();
            if (explode != null)
                return dist / SimExplode.EXPLODE_RADIUS;
            return dist / Fix64.One;
        }
    }
}
