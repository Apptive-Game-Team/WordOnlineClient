using System;
using System.Collections.Generic;

namespace GameScene.Simulation.Core
{
    public readonly struct SimulationGaugeSnapshot
    {
        public int Value { get; }
        public int MaxValue { get; }
        public string Category { get; }
        public SimulationGaugeSnapshot(int value, int maxValue, string category)
        { Value = value; MaxValue = maxValue; Category = category; }
    }

    public sealed class SimulationEntityPresentation
    {
        private readonly string[] effects;
        private readonly SimulationGaugeSnapshot[] gauges;
        public string Status { get; }
        public IReadOnlyList<string> Effects => effects;
        public IReadOnlyList<SimulationGaugeSnapshot> Gauges => gauges;
        public SimulationEntityPresentation(string status, IEnumerable<string> effects = null,
            IEnumerable<SimulationGaugeSnapshot> gauges = null)
        {
            Status = status ?? "Idle";
            this.effects = new List<string>(effects ?? Array.Empty<string>()).ToArray();
            this.gauges = new List<SimulationGaugeSnapshot>(gauges ??
                Array.Empty<SimulationGaugeSnapshot>()).ToArray();
        }
    }

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
        public string Status { get; }
        public IReadOnlyList<string> Effects { get; }
        public IReadOnlyList<SimulationGaugeSnapshot> Gauges { get; }

        internal SimulationEntitySnapshot(SimulationEntity entity,
            SimulationEntityPresentation presentation)
        {
            Id = entity.Id;
            OwnerUserId = entity.OwnerUserId;
            PrefabId = entity.PrefabId;
            IsDestroyed = entity.IsDestroyed;
            Position = entity.Position3D;
            Orientation = entity.Orientation;
            LinearVelocity = entity.LinearVelocity;
            AngularVelocity = entity.AngularVelocity;
            Status = entity.IsDestroyed ? "Destroyed" : presentation?.Status ?? "Idle";
            Effects = presentation?.Effects ?? Array.Empty<string>();
            Gauges = presentation?.Gauges ?? Array.Empty<SimulationGaugeSnapshot>();
        }
    }

    public sealed class SimulationSnapshot
    {
        private readonly SimulationEntitySnapshot[] entities;

        public int FrameNumber { get; }
        public IReadOnlyList<SimulationEntitySnapshot> Entities => entities;

        internal SimulationSnapshot(int frameNumber, IReadOnlyList<SimulationEntity> source)
            : this(frameNumber, source, null)
        {
        }

        internal SimulationSnapshot(int frameNumber, IReadOnlyList<SimulationEntity> source,
            IReadOnlyDictionary<int, SimulationEntityPresentation> presentation)
        {
            FrameNumber = frameNumber;
            entities = new SimulationEntitySnapshot[source.Count];
            for (int index = 0; index < source.Count; index++)
            {
                SimulationEntityPresentation state = null;
                presentation?.TryGetValue(source[index].Id, out state);
                entities[index] = new SimulationEntitySnapshot(source[index], state);
            }
        }
    }
}
