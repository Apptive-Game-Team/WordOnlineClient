namespace Simulation.Core
{
    /// <summary>
    /// Destroys its game object after a set duration.
    /// </summary>
    public class SimTimedSelfDestroyer : SimComponent
    {
        private readonly Fix64 _timeToLive;
        private Fix64 _elapsed;

        public SimTimedSelfDestroyer(Fix64 timeToLive)
        {
            _timeToLive = timeToLive;
        }

        public override void Start() { }
        public override void OnDestroy() { }

        public override void Update()
        {
            _elapsed = _elapsed + SimGameConfig.DeltaTime;
            if (_elapsed >= _timeToLive)
                GameObject.Destroy();
        }
    }
}
