namespace Simulation.Core
{
    /// <summary>
    /// On collision, applies an effect to the other object's EffectReceiver.
    /// </summary>
    public class SimEffectProvider : SimComponent, ISimCollidable
    {
        private readonly Effect _effect;

        public SimEffectProvider(Effect effect) { _effect = effect; }

        public void OnCollision(SimGameObject other)
        {
            var receiver = other.GetComponent<SimEffectReceiver>();
            receiver?.OnReceive(_effect);
        }

        public override void Start() { }
        public override void Update() { }
        public override void OnDestroy() { }
    }
}
