using System;
using System.Collections.Generic;
using FixMath.NET;
using GameScene.Simulation.Physics;
using GameScene.Simulation.Magic;

namespace GameScene.Simulation.Objects
{
    public readonly struct SimulationPrefabParameterValue
    {
        public string ObjectName { get; }
        public string ParameterName { get; }
        public Fix64 Value { get; }
        public SimulationPrefabParameterValue(string objectName, string parameterName, Fix64 value)
        { ObjectName = objectName; ParameterName = parameterName; Value = value; }
    }

    public sealed class SimulationPrefabRegistry
    {
        public const string DefaultPrefabId = "Default";
        public const string PlayerPrefabId = "Player";
        public const string PveNatureSlimeNestPrefabId = "PveNatureSlimeNest";
        public const string PveWaterSlimeNestPrefabId = "PveWaterSlimeNest";
        public const string PveVineColonyPrefabId = "PveVineColony";
        public const string PveVineWitchPrefabId = "PveVineWitch";
        public const string FireFieldPrefabId = "FireField";
        public const string WaterFieldPrefabId = "WaterField";
        public const string ElectricFieldPrefabId = "ElectricField";
        public const string LeafFieldPrefabId = "LeafField";
        public const string CommonEffectReceiverComponentId = "CommonEffectReceiver";
        public const string BuildingEffectReceiverComponentId = "BuildingEffectReceiver";
        public const string BehaviorMobComponentId = "BehaviorMob";
        public const string RigidBodyComponentId = "RigidBody";
        public const string ZPhysicsComponentId = "ZPhysics";

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
            return CreateProduction(null);
        }

        public static SimulationPrefabRegistry CreateProduction(
            IReadOnlyList<SimulationPrefabParameterValue> parameters)
        {
            List<SimulationPrefabDefinition> values = new List<SimulationPrefabDefinition>
            {
                new SimulationPrefabDefinition(DefaultPrefabId, "Transform", "PhysicsBody"),
                CreateServerPrefab(PlayerPrefabId, parameters, "PlayerState"),
                CreateServerPrefab(PveNatureSlimeNestPrefabId, parameters, "MobState"),
                CreateServerPrefab(PveWaterSlimeNestPrefabId, parameters, "MobState"),
                CreateServerPrefab(PveVineColonyPrefabId, parameters, "MobState"),
                CreateServerPrefab(PveVineWitchPrefabId, parameters, "MobState"),
                CreateServerPrefab(FireFieldPrefabId, parameters, "FieldState"),
                CreateServerPrefab(WaterFieldPrefabId, parameters, "FieldState"),
                CreateServerPrefab(ElectricFieldPrefabId, parameters, "FieldState"),
                CreateServerPrefab(LeafFieldPrefabId, parameters, "FieldState")
            };
            foreach (MagicDefinition definition in ProductionMagicCatalog.Create().Definitions)
                if (!values.Exists(value => string.Equals(value.Id, definition.PrefabId, StringComparison.Ordinal)))
                    values.Add(CreateServerPrefab(definition.PrefabId, parameters, "MagicState"));
            foreach (string prefabId in ServerPrefabManifest.All)
                if (!values.Exists(value => string.Equals(value.Id, prefabId, StringComparison.Ordinal)))
                    values.Add(CreateServerPrefab(prefabId, parameters, "ServerPrefabState"));
            return new SimulationPrefabRegistry(values);
        }

        private static SimulationPrefabDefinition CreateServerPrefab(string prefabId,
            IReadOnlyList<SimulationPrefabParameterValue> parameters, string stateComponent)
        {
            string objectName = ParameterObjectName(prefabId);
            Fix64 radius = prefabId == PlayerPrefabId ? Fix64.One :
                Find(parameters, objectName, "radius");
            bool collisionEnabled = parameters == null || radius > Fix64.Zero;
            Fix64 configuredMass = Find(parameters, objectName, "mass");
            bool isTrigger = IsTrigger(prefabId);
            bool isStatic = parameters != null && !isTrigger && configuredMass <= Fix64.Zero;
            Fix64 mass = configuredMass;
            if (radius <= Fix64.Zero) radius = parameters == null ? (Fix64)0.5m : (Fix64)0.05m;
            if (mass <= Fix64.Zero) mass = Fix64.One;
            string receiver = EffectReceiverComponent(prefabId);
            List<string> components = new List<string>
                { "Transform", "PhysicsBody", stateComponent };
            if (receiver != null) components.Add(receiver);
            if (IsBehaviorMob(prefabId)) components.Add(BehaviorMobComponentId);
            if (HasServerRigidBody(prefabId)) components.Add(RigidBodyComponentId);
            if (HasServerZPhysics(prefabId)) components.Add(ZPhysicsComponentId);
            return new SimulationPrefabDefinition(prefabId,
                new SimulationPhysicsDefinition(radius, mass, isTrigger, collisionEnabled, isStatic),
                ElementsFor(prefabId), components.ToArray());
        }

        private static bool IsBehaviorMob(string prefabId)
        {
            switch (prefabId)
            {
                case PveNatureSlimeNestPrefabId: case PveWaterSlimeNestPrefabId:
                case PveVineColonyPrefabId: case PveVineWitchPrefabId:
                case "EmberSpirit": case "SeedSpirit": case "LeafSlime":
                case "WaterSlime": case "FireSlime": case "ElectricSlime":
                case "RockSlime": case "WindSlime": case "MiniRock":
                case "ZapMouse": case "ChickenCommando": case "WindSpirit":
                case "FireChildSpirit": case "LightningTadpole": case "MagmaSpirit":
                case "FireTadpole": case "RockMage": case "BubbleSpirit":
                case "ThunderSpirit": case "StormRider": case "DimensionToad":
                case "AquaArcher": case "TreeGolem": case "VineSpirit":
                case "Towerback": case "CloudDragon": case "RockGolem":
                case "ThunderBird": case "FireSpirit":
                    return true;
                default:
                    return false;
            }
        }

        private static bool HasServerRigidBody(string prefabId)
        {
            switch (prefabId)
            {
                case PveNatureSlimeNestPrefabId: case PveWaterSlimeNestPrefabId:
                case PveVineColonyPrefabId: case PveVineWitchPrefabId:
                case "AquaArcher": case "BubbleGenerator": case "BubbleSpirit":
                case "ChickenCommando": case "CloudDragon": case "Crater":
                case "DimensionToad": case "ElectricSlime": case "ElectricTower":
                case "EmberSpirit": case "FireChildSpirit": case "FireLordSpirit":
                case "FireSlime": case "FireSpirit": case "FireTadpole":
                case "GroundCannon": case "GroundTower": case "HealingTotem":
                case "LeafSlime": case "LifeTree": case "LightningTadpole":
                case "MagmaSpirit": case "MiniRock": case "RockGolem":
                case "RockMage": case "RockSlime": case "RockTurret":
                case "SeedSpirit": case "StormRider": case "ThunderBird":
                case "ThunderSpirit": case "Towerback": case "TreeGolem":
                case "VineColony": case "VineSpirit": case "WaterSlime":
                case "WindSlime": case "WindSpirit": case "WindTotem":
                case "ZapMouse":
                    return true;
                default:
                    return false;
            }
        }

        private static bool HasServerZPhysics(string prefabId)
        {
            switch (prefabId)
            {
                case PveNatureSlimeNestPrefabId: case PveWaterSlimeNestPrefabId:
                case PveVineColonyPrefabId: case PveVineWitchPrefabId:
                case "AquaArcher": case "BubbleSpirit": case "ChickenCommando":
                case "CloudDragon": case "DimensionToad": case "ElectricSlime":
                case "EmberSpirit": case "FireChildSpirit": case "FireLordSpirit":
                case "FireSlime": case "FireSpirit": case "FireTadpole":
                case "LeafSlime": case "LightningTadpole": case "MagmaSpirit":
                case "MiniRock": case "RockGolem": case "RockMage":
                case "RockSlime": case "SeedSpirit": case "StormRider":
                case "ThunderBird": case "ThunderSpirit": case "Towerback":
                case "TreeGolem": case "VineSpirit": case "WaterSlime":
                case "WindSlime": case "WindSpirit": case "ZapMouse":
                    return true;
                default:
                    return false;
            }
        }

        private static string EffectReceiverComponent(string prefabId)
        {
            if (prefabId == "BubbleGenerator" || prefabId == "Crater" || prefabId == "ElectricTower")
                return BuildingEffectReceiverComponentId;
            if (prefabId == "LeafField") return "LeafFieldEffectReceiver";
            if (prefabId == "WaterField") return "WaterFieldEffectReceiver";
            switch (prefabId)
            {
                case "Player":
                case "AquaArcher": case "BubbleSpirit": case "ChickenCommando":
                case "CloudDragon": case "DimensionToad": case "ElectricSlime":
                case "EmberSpirit": case "FireChildSpirit": case "FireLordSpirit":
                case "FireSlime": case "FireSpirit": case "FireTadpole":
                case "GroundCannon": case "GroundTower": case "HealingTotem":
                case "LeafSlime": case "LifeTree": case "LightningTadpole":
                case "MagmaSpirit": case "ManaWell": case "MiniRock":
                case "RockGolem": case "RockMage": case "RockSlime":
                case "RockTurret": case "SeedSpirit": case "StormRider":
                case "ThunderBird": case "ThunderSpirit": case "Towerback":
                case "TreeGolem": case "VineColony": case "VineSpirit":
                case "WaterSlime": case "WindSlime": case "WindSpirit":
                case "WindTotem": case "ZapMouse":
                    return CommonEffectReceiverComponentId;
                default:
                    return null;
            }
        }

        internal static SimulationElement ElementsFor(string prefabId)
        {
            switch (prefabId)
            {
                case "FireShot": case "MagmaExplosion": case "FireExplode": case "FireDrop":
                case "FireField": case "FireSummon": case "FireSlime": case "FireChildSpirit":
                case "FireTadpole": case "Crater": case "CraterEmber": case "FireRune":
                case "EmberSpirit": case "WaterExplosion":
                    return SimulationElement.Fire;
                case "WaterShot": case "WaterExplode": case "TideCall": case "WaterDrop": case "WaterField":
                case "WaterSlime": case "AquaArcher": case "PveWaterSlimeNest": case "WaterRune":
                case "BubbleGenerator":
                    return SimulationElement.Water;
                case "ElectricShot": case "ElectricExplode": case "ShockOverload":
                case "LightningDrop": case "ElectricField": case "ElectricSummon":
                case "ElectricSlime": case "ElectricTower": case "ZapMouse":
                case "LightningTadpole": case "ThunderBird": case "LightningRune":
                case "ChainLightning": return SimulationElement.Lightning;
                case "Vine": case "Overgrowth": case "NatureDrop": case "LeafExplode": case "LeafField":
                case "LeafSlime": case "SeedSpirit": case "LifeTree": case "VineColony":
                case "VineSpirit": case "NatureRune": case "PveNatureSlimeNest":
                case "TreeGolem": case "PveVineColony": case "PveVineWitch": case "Leafair":
                    return SimulationElement.Nature;
                case "RockRolling": case "RockExplode": case "RockDrop": case "RockSummon": case "RockSlime":
                case "RockTurret": case "RockGolem": case "RockMage": case "MiniRock":
                case "GroundCannon": case "GroundTower": case "Towerback": case "RockRune":
                case "WindTotem": return SimulationElement.Rock;
                case "WindBlade": case "WindExplode": case "RazorGale": case "WindDrop":
                case "WindField": case "WindSummon": case "WindSlime": case "WindSpirit":
                case "ChickenCommando": case "WindRune": return SimulationElement.Wind;
                case "MagmaFist": case "MagmaSpirit": case "MeteorDrop": case "MeteorShower":
                    return SimulationElement.Fire | SimulationElement.Rock;
                case "DimensionToad": return SimulationElement.Fire | SimulationElement.Lightning;
                case "HealingTotem": return SimulationElement.Nature | SimulationElement.Water;
                case "RallyingTotem": case "FireLordSpirit": case "FireSpirit":
                    return SimulationElement.Fire | SimulationElement.Wind;
                case "SandStorm": return SimulationElement.Rock | SimulationElement.Wind;
                case "ManaWell": return SimulationElement.Lightning | SimulationElement.Nature;
                case "ThunderSpirit": return SimulationElement.Lightning | SimulationElement.Wind;
                case "CloudDragon": case "BubbleSpirit":
                    return SimulationElement.Water | SimulationElement.Wind;
                case "StormRider": return SimulationElement.Lightning | SimulationElement.Water;
                case "TornadoStrike": return SimulationElement.Nature | SimulationElement.Wind;
                default: return SimulationElement.None;
            }
        }

        private static Fix64 Find(IReadOnlyList<SimulationPrefabParameterValue> parameters,
            string objectName, string parameterName)
        {
            if (parameters == null) return Fix64.Zero;
            for (int index = 0; index < parameters.Count; index++)
                if (string.Equals(parameters[index].ObjectName, objectName, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(parameters[index].ParameterName, parameterName, StringComparison.OrdinalIgnoreCase))
                    return parameters[index].Value;
            return Fix64.Zero;
        }

        private static string ParameterObjectName(string prefabId)
        {
            if (prefabId == "Leafair") return "leaf_drop";
            List<char> value = new List<char>(prefabId.Length + 4);
            for (int index = 0; index < prefabId.Length; index++)
            {
                char current = prefabId[index];
                if (index > 0 && char.IsUpper(current) &&
                    (!char.IsUpper(prefabId[index - 1]) ||
                     index + 1 < prefabId.Length && char.IsLower(prefabId[index + 1]))) value.Add('_');
                value.Add(char.ToLowerInvariant(current));
            }
            return new string(value.ToArray());
        }

        private static bool IsTrigger(string prefabId)
        {
            switch (prefabId)
            {
                case "FireShot": case "WaterShot": case "ElectricShot": case "ChainLightning":
                case "RockRolling": case "TideCall": case "WillOWisp": case "WindBlade":
                case "FireExplode": case "WaterExplode": case "RockExplode": case "ElectricExplode":
                case "LeafExplode": case "WindExplode": case "MagmaExplosion": case "WaterExplosion":
                case "ShockOverload": case "RazorGale": case "Overgrowth":
                case "FireField": case "WaterField": case "ElectricField": case "LeafField":
                case "FireDrop": case "NatureDrop": case "RockDrop": case "LightningDrop":
                case "WindDrop": case "FrenzyTotem": case "Leafair": case "RallyingTotem":
                case "SandStorm": case "TornadoStrike": case "MeteorShower":
                case "FireRune": case "WaterRune": case "NatureRune": case "RockRune":
                case "LightningRune": case "WindRune": case "Vine": case "MagmaFist":
                case "MeteorDrop": case "CraterEmber": return true;
                default: return false;
            }
        }
    }
}
