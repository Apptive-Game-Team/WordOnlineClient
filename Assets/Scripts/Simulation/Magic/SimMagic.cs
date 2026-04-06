using System.Collections.Generic;

namespace Simulation.Core
{
    /// <summary>
    /// Base class for all magic spells. Each spell creates game objects when cast.
    /// </summary>
    public abstract class SimMagic
    {
        public long Id;
        public readonly SimCardType MagicType;

        protected SimMagic(SimCardType magicType)
        {
            MagicType = magicType;
        }

        public abstract void Run(SimWorld world, Master master, SimVector3 position);

        public virtual void Run(SimWorld world, Master master, SimVector3 castOrigin, SimVector3 position)
        {
            Run(world, master, position);
        }
    }

    // ── Concrete magic categories ──

    /// <summary>Shot magic: creates projectile at player position, aimed at target.</summary>
    public class SimShotMagic : SimMagic
    {
        private readonly PrefabType _prefabType;

        public SimShotMagic(PrefabType prefabType) : base(SimCardType.Shoot)
        {
            _prefabType = prefabType;
        }

        public override void Run(SimWorld world, Master master, SimVector3 position)
        {
            var playerPos = SimGameConfig.GetPlayerPosition(master);
            var obj = new SimGameObject(master, _prefabType, playerPos, world);
            // Shot.SetTarget will be called after Start via the system
        }
    }

    /// <summary>Explode magic: creates explosion at target position.</summary>
    public class SimExplodeMagic : SimMagic
    {
        private readonly PrefabType _prefabType;

        public SimExplodeMagic(PrefabType prefabType) : base(SimCardType.Explode)
        {
            _prefabType = prefabType;
        }

        public override void Run(SimWorld world, Master master, SimVector3 position)
        {
            new SimGameObject(master, _prefabType, position, world);
        }
    }

    /// <summary>Drop magic: creates falling object at target XY with initial height.</summary>
    public class SimDropMagic : SimMagic
    {
        private readonly PrefabType _prefabType;

        public SimDropMagic(PrefabType prefabType) : base(SimCardType.Drop)
        {
            _prefabType = prefabType;
        }

        public override void Run(SimWorld world, Master master, SimVector3 position)
        {
            var dropPos = new SimVector3(position.X, position.Y, SimGameConfig.DROP_MAGIC_INITIAL_HEIGHT);
            new SimGameObject(master, _prefabType, dropPos, world);
        }
    }

    /// <summary>Spawn magic: creates entity at target position.</summary>
    public class SimSpawnMagic : SimMagic
    {
        private readonly PrefabType _prefabType;

        public SimSpawnMagic(PrefabType prefabType) : base(SimCardType.Spawn)
        {
            _prefabType = prefabType;
        }

        public override void Run(SimWorld world, Master master, SimVector3 position)
        {
            new SimGameObject(master, _prefabType, position, world);
        }
    }

    /// <summary>Build magic: creates structure at target position.</summary>
    public class SimBuildMagic : SimMagic
    {
        private readonly PrefabType _prefabType;

        public SimBuildMagic(PrefabType prefabType) : base(SimCardType.Build)
        {
            _prefabType = prefabType;
        }

        public override void Run(SimWorld world, Master master, SimVector3 position)
        {
            new SimGameObject(master, _prefabType, position, world);
        }
    }

    /// <summary>Swarm spawn magic: creates N small entities scattered around target.</summary>
    public class SimSwarmSpawnMagic : SimMagic
    {
        private readonly PrefabType _prefabType;
        private readonly int _count;

        public SimSwarmSpawnMagic(PrefabType prefabType, int count = 3) : base(SimCardType.Spawn)
        {
            _prefabType = prefabType;
            _count = count;
        }

        public override void Run(SimWorld world, Master master, SimVector3 position)
        {
            for (int i = 0; i < _count; i++)
            {
                var offset = world.Random.RandomUnitVector2() * Fix64.One;
                var spawnPos = new SimVector3(position.X + offset.X, position.Y + offset.Y, Fix64.Zero);
                new SimGameObject(master, _prefabType, spawnPos, world);
            }
        }
    }
}
