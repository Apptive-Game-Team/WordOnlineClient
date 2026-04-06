namespace Simulation.Core
{
    /// <summary>
    /// Periodically applies an effect to all nearby objects in radius.
    /// </summary>
    public class SimAreaEffectProvider : SimComponent
    {
        private readonly Fix64 _interval;
        private readonly Fix64 _radius;
        private readonly Effect _effect;
        private Fix64 _counter;

        public SimAreaEffectProvider(Fix64 interval, Fix64 radius, Effect effect)
        {
            _interval = interval;
            _radius = radius;
            _effect = effect;
        }

        public override void Start() { }
        public override void OnDestroy() { }

        public override void Update()
        {
            _counter = _counter + SimGameConfig.DeltaTime;
            if (_counter < _interval) return;
            _counter = Fix64.Zero;

            var targets = World.OverlapSphereAll(GameObject, _radius);
            foreach (var target in targets)
            {
                var receiver = target.GetComponent<SimEffectReceiver>();
                receiver?.OnReceive(_effect);
            }
        }
    }
}
