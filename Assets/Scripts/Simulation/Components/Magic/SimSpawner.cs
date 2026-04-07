namespace Simulation.Core
{
    /// <summary>
    /// Periodically spawns prefab instances. Optionally destructible (takes damage each spawn).
    /// </summary>
    public class SimSpawner : SimMob
    {
        public static readonly Fix64 DEFAULT_SPAWN_INTERVAL = Fix64.FromInt(2);
        private static readonly Fix64 DEFAULT_BURST_SPACING = Fix64.Half;

        private Fix64 _counter;
        private bool _isRunning;
        private readonly PrefabType _prefabType;
        private readonly Fix64 _spawnInterval;
        private readonly bool _destructible;
        private readonly int _spawnCount;

        public SimSpawner(int maxHp, PrefabType prefabType)
            : this(maxHp, prefabType, DEFAULT_SPAWN_INTERVAL, true, 1) { }

        public SimSpawner(int maxHp, PrefabType prefabType, Fix64 spawnInterval, bool destructible, int spawnCount)
            : base(maxHp, 0)
        {
            _prefabType = prefabType;
            _spawnInterval = spawnInterval < Fix64.FromDouble(0.1) ? Fix64.FromDouble(0.1) : spawnInterval;
            _destructible = destructible;
            _spawnCount = spawnCount < 1 ? 1 : spawnCount;
            _isRunning = true;
        }

        public override void Start() { }
        public override void OnDestroy() { }

        public override void Update()
        {
            base.Update();
            if (!_isRunning) return;
            _counter = _counter + SimGameConfig.DeltaTime;
            if (_counter >= _spawnInterval)
            {
                _counter = Fix64.Zero;
                SpawnPrefabs();
                if (_destructible)
                    OnDamaged(new AttackInfo(1, ElementType.NONE));
            }
        }

        public override void ApplyDamage(AttackInfo attackInfo)
        {
            if (!_destructible) return;
            Hp -= attackInfo.Damage;
            World.MarkUpdated(GameObject);
            if (Hp <= 0) GameObject.Destroy();
        }

        public override void OnDeath() { }

        private void SpawnPrefabs()
        {
            if (_spawnCount == 1)
            {
                new SimGameObject(GameObject, _prefabType);
                return;
            }

            Fix64 centerOffset = Fix64.FromInt(_spawnCount - 1) / Fix64.Two;
            for (int i = 0; i < _spawnCount; i++)
            {
                Fix64 offsetX = (Fix64.FromInt(i) - centerOffset) * DEFAULT_BURST_SPACING;
                var basePos = GameObject.Position;
                var spawnPos = new SimVector3(basePos.X + offsetX, basePos.Y, basePos.Z);
                new SimGameObject(GameObject.Master, _prefabType, spawnPos, World);
            }
        }
    }
}
