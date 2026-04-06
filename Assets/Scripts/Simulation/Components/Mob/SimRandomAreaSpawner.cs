namespace Simulation.Core
{
    /// <summary>
    /// Periodically spawns a prefab at random positions within an area.
    /// Used by MeteorShower.
    /// </summary>
    public class SimRandomAreaSpawner : SimComponent
    {
        private readonly PrefabType _prefabType;
        private readonly Fix64 _interval;
        private readonly Fix64 _radius;
        private Fix64 _timer;

        public SimRandomAreaSpawner(PrefabType prefabType, Fix64 interval, Fix64 radius)
        {
            _prefabType = prefabType;
            _interval = interval;
            _radius = radius;
        }

        public override void Start() { }
        public override void OnDestroy() { }

        public override void Update()
        {
            _timer = _timer + SimGameConfig.DeltaTime;
            if (_timer < _interval) return;
            _timer = Fix64.Zero;

            var center = GameObject.Position;
            var offset = World.Random.RandomUnitVector2() * _radius * World.Random.NextFix();
            var spawnPos = new SimVector3(
                center.X + offset.X,
                center.Y + offset.Y,
                SimGameConfig.DROP_MAGIC_INITIAL_HEIGHT
            );

            new SimGameObject(GameObject.Master, _prefabType, spawnPos, World);
        }
    }
}
