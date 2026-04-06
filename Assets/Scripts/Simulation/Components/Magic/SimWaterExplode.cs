namespace Simulation.Core
{
    /// <summary>
    /// Explosion that also launches targets upward (Z impulse).
    /// </summary>
    public class SimWaterExplode : SimExplode
    {
        private readonly Fix64 _zForce;

        public SimWaterExplode(int damage, Fix64 radius, Fix64 zForce) : base(damage, radius)
        {
            _zForce = zForce;
        }

        protected override void HandleGameObject(SimGameObject target)
        {
            base.HandleGameObject(target);
            var zp = target.GetComponent<SimZPhysics>();
            zp?.AddImpulseZ(_zForce);
        }
    }
}
