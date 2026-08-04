using System;
using FixMath.NET;

namespace GameScene.Simulation.Magic
{
    public enum MagicBehaviorFamily : byte
    {
        Spawn = 1,
        Projectile = 2,
        Area = 3,
        Build = 4,
        Drop = 5
    }

    public enum MagicExecutionKind : byte
    {
        SingleSpawn = 1,
        SwarmSpawn = 2,
        Shot = 3,
        Explosion = 4,
        Build = 5,
        Drop = 6,
        VineLine = 7,
        VineFan = 8,
        DropImpact = 9,
        LightningColumn = 10,
        FrenzyDrop = 11,
        RallyingTorch = 12,
        LeafairDrop = 13,
        MeteorShower = 14,
        AirSpawn = 15,
        Overgrowth = 16,
        PeriodicArea = 17,
        MovingTornado = 18,
        PersistentArea = 19,
        ChainShot = 20,
        RollingRock = 21,
        PushShot = 22,
        MindControlShot = 23,
        PiercingShot = 24
    }

    public sealed class MagicDefinition
    {
        public int Id { get; }
        public string Name { get; }
        public string PrefabId { get; }
        public MagicBehaviorFamily Family { get; }
        public MagicExecutionKind ExecutionKind { get; }
        public int SpawnCount { get; }
        public int LifetimeFrames { get; }
        public int Damage { get; }
        public Fix64 Range { get; }
        public Fix64 Radius { get; }
        public Fix64 Speed { get; }
        public string StatusId { get; }
        public int StatusFrames { get; }
        public int ActivationFrames { get; }
        public Fix64 EffectRadius { get; }
        public int EffectFrames { get; }
        public int EffectIntervalFrames { get; }
        public string SecondaryPrefabId { get; }
        public int SecondarySpawnCount { get; }
        public int MinimumDamage { get; }
        public int MaximumHits { get; }
        public Fix64 PathSpawnDistance { get; }
        public Fix64 VerticalImpulse { get; }

        public MagicDefinition(int id, string name, string prefabId, MagicBehaviorFamily family,
            int spawnCount, int lifetimeFrames, int damage, Fix64 range,
            Fix64 radius, Fix64 speed, string statusId = null, int statusFrames = 0)
            : this(id, name, prefabId, family, DefaultExecution(family), spawnCount,
                lifetimeFrames, damage, range, radius, speed, statusId, statusFrames)
        {
        }

        public MagicDefinition(int id, string name, string prefabId, MagicBehaviorFamily family,
            MagicExecutionKind executionKind, int spawnCount, int lifetimeFrames, int damage, Fix64 range,
            Fix64 radius, Fix64 speed, string statusId = null, int statusFrames = 0,
            int activationFrames = 0, Fix64 effectRadius = default, int effectFrames = 0,
            int effectIntervalFrames = 0, string secondaryPrefabId = null,
            int secondarySpawnCount = 0, int minimumDamage = 0, int maximumHits = 0,
            Fix64 pathSpawnDistance = default, Fix64 verticalImpulse = default)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Magic name is required", nameof(name));
            if (string.IsNullOrWhiteSpace(prefabId)) throw new ArgumentException("Prefab ID is required", nameof(prefabId));
            if (spawnCount < 1 || lifetimeFrames < 0 || damage < 0 || range < Fix64.Zero
                || radius < Fix64.Zero || speed < Fix64.Zero || statusFrames < 0 ||
                activationFrames < 0 || effectRadius < Fix64.Zero || effectFrames < 0 ||
                effectIntervalFrames < 0 || secondarySpawnCount < 0 || minimumDamage < 0 ||
                maximumHits < 0 || pathSpawnDistance < Fix64.Zero || verticalImpulse < Fix64.Zero)
                throw new ArgumentOutOfRangeException(nameof(spawnCount));
            Id = id; Name = name; PrefabId = prefabId; Family = family;
            ExecutionKind = executionKind; SpawnCount = spawnCount;
            LifetimeFrames = lifetimeFrames; Damage = damage; Range = range;
            Radius = radius; Speed = speed; StatusId = statusId; StatusFrames = statusFrames;
            ActivationFrames = activationFrames; EffectRadius = effectRadius;
            EffectFrames = effectFrames; EffectIntervalFrames = effectIntervalFrames;
            SecondaryPrefabId = secondaryPrefabId;
            SecondarySpawnCount = secondarySpawnCount;
            MinimumDamage = minimumDamage; MaximumHits = maximumHits;
            PathSpawnDistance = pathSpawnDistance;
            VerticalImpulse = verticalImpulse;
        }

        private static MagicExecutionKind DefaultExecution(MagicBehaviorFamily family)
        {
            switch (family)
            {
                case MagicBehaviorFamily.Spawn: return MagicExecutionKind.SingleSpawn;
                case MagicBehaviorFamily.Projectile: return MagicExecutionKind.Shot;
                case MagicBehaviorFamily.Area: return MagicExecutionKind.Explosion;
                case MagicBehaviorFamily.Build: return MagicExecutionKind.Build;
                case MagicBehaviorFamily.Drop: return MagicExecutionKind.DropImpact;
                default: throw new ArgumentOutOfRangeException(nameof(family));
            }
        }
    }
}
