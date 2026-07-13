using System;
using System.Collections.Generic;

namespace GameScene.Simulation.Objects
{
    public sealed class SimulationPrefabRegistry
    {
        public const string DefaultPrefabId = "Default";
        public const string PlayerPrefabId = "Player";

        private readonly Dictionary<string, SimulationPrefabDefinition> definitions;

        public SimulationPrefabRegistry(IEnumerable<SimulationPrefabDefinition> definitions)
        {
            if (definitions == null) throw new ArgumentNullException(nameof(definitions));
            this.definitions = new Dictionary<string, SimulationPrefabDefinition>(StringComparer.Ordinal);
            foreach (SimulationPrefabDefinition definition in definitions)
            {
                if (definition == null) throw new ArgumentException("Prefab definition cannot be null", nameof(definitions));
                if (this.definitions.ContainsKey(definition.Id))
                    throw new InvalidOperationException("Duplicate simulation prefab: " + definition.Id);
                this.definitions.Add(definition.Id, definition);
            }
        }

        public SimulationPrefabDefinition GetRequired(string prefabId)
        {
            if (prefabId == null || !definitions.TryGetValue(prefabId, out SimulationPrefabDefinition definition))
                throw new InvalidOperationException("Missing simulation prefab configuration: " + (prefabId ?? "<null>"));
            return definition;
        }

        public static SimulationPrefabRegistry CreateCore()
        {
            return new SimulationPrefabRegistry(new[]
            {
                new SimulationPrefabDefinition(DefaultPrefabId, "Transform", "PhysicsBody"),
                new SimulationPrefabDefinition(PlayerPrefabId, "Transform", "PhysicsBody", "PlayerState")
            });
        }
    }
}
