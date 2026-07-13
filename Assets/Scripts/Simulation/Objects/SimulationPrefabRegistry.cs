using System;
using System.Collections.Generic;
using GameScene.Simulation.Magic;

namespace GameScene.Simulation.Objects
{
    public sealed class SimulationPrefabRegistry
    {
        public const string DefaultPrefabId = "Default";
        public const string PlayerPrefabId = "Player";
        public const string PveNatureSlimeNestPrefabId = "PveNatureSlimeNest";
        public const string PveWaterSlimeNestPrefabId = "PveWaterSlimeNest";
        public const string PveVineColonyPrefabId = "PveVineColony";
        public const string PveVineWitchPrefabId = "PveVineWitch";

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

        public static SimulationPrefabRegistry CreateProduction()
        {
            List<SimulationPrefabDefinition> values = new List<SimulationPrefabDefinition>
            {
                new SimulationPrefabDefinition(DefaultPrefabId, "Transform", "PhysicsBody"),
                new SimulationPrefabDefinition(PlayerPrefabId, "Transform", "PhysicsBody", "PlayerState"),
                new SimulationPrefabDefinition(PveNatureSlimeNestPrefabId, "Transform", "PhysicsBody", "MobState"),
                new SimulationPrefabDefinition(PveWaterSlimeNestPrefabId, "Transform", "PhysicsBody", "MobState"),
                new SimulationPrefabDefinition(PveVineColonyPrefabId, "Transform", "PhysicsBody", "MobState"),
                new SimulationPrefabDefinition(PveVineWitchPrefabId, "Transform", "PhysicsBody", "MobState")
            };
            foreach (MagicDefinition definition in ProductionMagicCatalog.Create().Definitions)
                values.Add(new SimulationPrefabDefinition(definition.Name, "Transform", "PhysicsBody", "MagicState"));
            return new SimulationPrefabRegistry(values);
        }
    }
}
