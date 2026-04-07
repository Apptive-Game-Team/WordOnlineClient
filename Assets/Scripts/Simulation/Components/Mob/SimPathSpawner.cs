namespace Simulation.Core
{
    /// <summary>
    /// Spawns a prefab at the object's position periodically as it moves.
    /// </summary>
    public class SimPathSpawner : SimComponent
    {
        private readonly PrefabType _prefabType;
        private readonly Fix64 _interval;
        private readonly SimVector3 _offset;
        private Fix64 _timer;

        public SimPathSpawner(PrefabType prefabType, Fix64 interval, SimVector3 offset = default)
        {
            _prefabType = prefabType;
            _interval = interval;
            _offset = offset;
        }

        public override void Start() { }
        public override void OnDestroy() { }

        public override void Update()
        {
            _timer = _timer + SimGameConfig.DeltaTime;
            if (_timer < _interval) return;
            _timer = Fix64.Zero;

            new SimGameObject(GameObject.Master, _prefabType, GameObject.Position + _offset, World);
        }
    }
}
