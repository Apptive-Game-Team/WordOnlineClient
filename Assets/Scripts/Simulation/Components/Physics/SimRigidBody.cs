namespace Simulation.Core
{
    public class SimRigidBody : SimComponent
    {
        private SimVector3 _velocity = SimVector3.Zero;
        private readonly int _mass;

        public SimRigidBody(int mass)
        {
            _mass = mass;
        }

        public Fix64 InvMass => _mass == -1 ? Fix64.Zero : Fix64.One / Fix64.FromInt(_mass);

        public SimVector3 Velocity => _velocity;

        public void AddVelocity(SimVector3 v)
        {
            _velocity = _velocity + v;
        }

        public void ApplyVelocity()
        {
            GameObject.SetPosition(
                GameObject.Position + _velocity * SimGameConfig.DeltaTime
            );
            _velocity = SimVector3.Zero;
        }

        public override void Start() { }
        public override void Update() { }
        public override void OnDestroy() { }
    }
}
