using System;
using System.Collections.Generic;
using FixMath.NET;
using GameScene.Simulation.Physics;

namespace GameScene.Simulation.Objects
{
    public sealed class SimulationPrefabDefinition
    {
        private readonly string[] componentIds;

        public string Id { get; }
        public IReadOnlyList<string> ComponentIds => componentIds;
        public SimulationPhysicsDefinition Physics { get; }
        public SimulationElement Elements { get; }

        public SimulationPrefabDefinition(string id, params string[] componentIds)
            : this(id, new SimulationPhysicsDefinition((Fix64)0.5m, Fix64.One),
                SimulationElement.None, componentIds)
        {
        }

        public SimulationPrefabDefinition(
            string id,
            SimulationPhysicsDefinition physics,
            params string[] componentIds)
            : this(id, physics, SimulationElement.None, componentIds)
        {
        }

        public SimulationPrefabDefinition(
            string id,
            SimulationPhysicsDefinition physics,
            SimulationElement elements,
            params string[] componentIds)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Prefab ID is required", nameof(id));
            if (componentIds == null) throw new ArgumentNullException(nameof(componentIds));
            Physics = physics ?? throw new ArgumentNullException(nameof(physics));
            Elements = elements;

            Id = id;
            this.componentIds = new string[componentIds.Length];
            for (int index = 0; index < componentIds.Length; index++)
            {
                string componentId = componentIds[index];
                if (string.IsNullOrWhiteSpace(componentId))
                    throw new ArgumentException("Component ID is required at index " + index, nameof(componentIds));
                for (int previous = 0; previous < index; previous++)
                {
                    if (string.Equals(this.componentIds[previous], componentId, StringComparison.Ordinal))
                        throw new ArgumentException("Duplicate component ID: " + componentId, nameof(componentIds));
                }
                this.componentIds[index] = componentId;
            }
        }
    }
}
