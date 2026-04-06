namespace Simulation.Core
{
    /// <summary>
    /// Stationary mob that periodically summons a prefab.
    /// Unlike SimSpawner (which is destructible per spawn), this is a regular timed mob.
    /// Used by VineColony.
    /// </summary>
    public class SimSummonMob : SimMob
    {
        private readonly PrefabType _summonPrefab;
        private readonly Fix64 _summonInterval;
        private readonly Fix64 _summonRange;
        private Fix64 _timer;

        public SimSummonMob(int maxHp, int damage, Fix64 summonInterval, Fix64 summonRange, PrefabType summonPrefab)
            : base(maxHp, 0)
        {
            _summonPrefab = summonPrefab;
            _summonInterval = summonInterval;
            _summonRange = summonRange;
        }

        public override void Start() { }
        public override void OnDestroy() { }
        public override void OnDeath() => GameObject.Destroy();

        public override void Update()
        {
            base.Update();
            _timer = _timer + SimGameConfig.DeltaTime;
            if (_timer < _summonInterval) return;
            _timer = Fix64.Zero;

            // Spawn at a random offset within range
            var offset = World.Random.RandomUnitVector2() * _summonRange * World.Random.NextFix();
            var pos = new SimVector3(
                GameObject.Position.X + offset.X,
                GameObject.Position.Y + offset.Y,
                Fix64.Zero
            );
            new SimGameObject(GameObject.Master, _summonPrefab, pos, World);
            GameObject.SetStatus(SimStatus.Attack);
        }
    }
}
