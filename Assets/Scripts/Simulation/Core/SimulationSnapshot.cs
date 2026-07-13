using System;
using System.Collections.Generic;

namespace GameScene.Simulation.Core
{
    public readonly struct SimulationEntitySnapshot
    {
        public int Id { get; }
        public long OwnerUserId { get; }
        public string PrefabId { get; }
        public bool IsDestroyed { get; }
        public SimVector3 Position { get; }
        public SimQuaternion Orientation { get; }
        public SimVector3 LinearVelocity { get; }
        public SimVector3 AngularVelocity { get; }

        internal SimulationEntitySnapshot(SimulationEntity entity)
        {
            Id = entity.Id;
            OwnerUserId = entity.OwnerUserId;
            PrefabId = entity.PrefabId;
            IsDestroyed = entity.IsDestroyed;
            Position = entity.Position3D;
            Orientation = entity.Orientation;
            LinearVelocity = entity.LinearVelocity;
            AngularVelocity = entity.AngularVelocity;
        }
    }

    public sealed class SimulationSnapshot
    {
        private readonly SimulationEntitySnapshot[] entities;

        public int FrameNumber { get; }
        public IReadOnlyList<SimulationEntitySnapshot> Entities => entities;

        internal SimulationSnapshot(int frameNumber, IReadOnlyList<SimulationEntity> source)
        {
            FrameNumber = frameNumber;
            entities = new SimulationEntitySnapshot[source.Count];
            for (int index = 0; index < source.Count; index++)
                entities[index] = new SimulationEntitySnapshot(source[index]);
        }
    }
}
