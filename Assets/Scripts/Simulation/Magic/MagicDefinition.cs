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

    public sealed class MagicDefinition
    {
        public int Id { get; }
        public string Name { get; }
        public MagicBehaviorFamily Family { get; }
        public int SpawnCount { get; }
        public int LifetimeFrames { get; }
        public int Damage { get; }
        public Fix64 Range { get; }
        public Fix64 Radius { get; }
        public Fix64 Speed { get; }
        public string StatusId { get; }
        public int StatusFrames { get; }

        public MagicDefinition(int id, string name, MagicBehaviorFamily family,
            int spawnCount, int lifetimeFrames, int damage, Fix64 range,
            Fix64 radius, Fix64 speed, string statusId = null, int statusFrames = 0)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Magic name is required", nameof(name));
            if (spawnCount < 1 || lifetimeFrames < 1 || damage < 0 || range < Fix64.Zero
                || radius < Fix64.Zero || speed < Fix64.Zero || statusFrames < 0)
                throw new ArgumentOutOfRangeException(nameof(spawnCount));
            Id = id; Name = name; Family = family; SpawnCount = spawnCount;
            LifetimeFrames = lifetimeFrames; Damage = damage; Range = range;
            Radius = radius; Speed = speed; StatusId = statusId; StatusFrames = statusFrames;
        }
    }
}
