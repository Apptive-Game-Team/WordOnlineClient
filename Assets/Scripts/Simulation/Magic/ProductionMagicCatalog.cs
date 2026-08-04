using System;
using System.Collections.Generic;
using FixMath.NET;

namespace GameScene.Simulation.Magic
{
    public readonly struct MagicParameterValue
    {
        public string ObjectName { get; }
        public string ParameterName { get; }
        public Fix64 Value { get; }

        public MagicParameterValue(string objectName, string parameterName, Fix64 value)
        {
            ObjectName = objectName ?? throw new ArgumentNullException(nameof(objectName));
            ParameterName = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
            Value = value;
        }
    }

    public sealed class FieldDefinition
    {
        public string PrefabId { get; }
        public Fix64 Radius { get; }
        public int LifetimeFrames { get; }
        public string StatusId { get; }

        public FieldDefinition(string prefabId, Fix64 radius, int lifetimeFrames, string statusId)
        {
            PrefabId = prefabId ?? throw new ArgumentNullException(nameof(prefabId));
            if (radius < Fix64.Zero || lifetimeFrames <= 0)
                throw new ArgumentOutOfRangeException(nameof(radius));
            Radius = radius; LifetimeFrames = lifetimeFrames;
            StatusId = statusId ?? throw new ArgumentNullException(nameof(statusId));
        }
    }

    public enum AuxiliaryPrefabKind : byte
    {
        CraterEmber = 1, Rune = 2, OnStartArea = 3, FallingDrop = 4
    }

    public sealed class AuxiliaryPrefabDefinition
    {
        public string PrefabId { get; }
        public AuxiliaryPrefabKind Kind { get; }
        public Fix64 Radius { get; }
        public Fix64 EffectRadius { get; }
        public int Damage { get; }
        public int LifetimeFrames { get; }
        public string StatusId { get; }

        public AuxiliaryPrefabDefinition(string prefabId, AuxiliaryPrefabKind kind,
            Fix64 radius, Fix64 effectRadius, int damage, int lifetimeFrames = 0,
            string statusId = null)
        {
            PrefabId = prefabId ?? throw new ArgumentNullException(nameof(prefabId));
            if (radius < Fix64.Zero || effectRadius < Fix64.Zero || damage < 0 || lifetimeFrames < 0)
                throw new ArgumentOutOfRangeException(nameof(radius));
            Kind = kind; Radius = radius; EffectRadius = effectRadius; Damage = damage;
            LifetimeFrames = lifetimeFrames; StatusId = statusId;
        }
    }

    public sealed class ProductionMagicCatalog
    {
        private readonly Dictionary<int, MagicDefinition> byId = new Dictionary<int, MagicDefinition>();
        private readonly Dictionary<string, MagicDefinition> byName =
            new Dictionary<string, MagicDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, FieldDefinition> fields =
            new Dictionary<string, FieldDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, AuxiliaryPrefabDefinition> auxiliaryPrefabs =
            new Dictionary<string, AuxiliaryPrefabDefinition>(StringComparer.Ordinal);

        public IReadOnlyCollection<MagicDefinition> Definitions => byId.Values;

        public ProductionMagicCatalog(IEnumerable<MagicDefinition> definitions,
            IEnumerable<FieldDefinition> fieldDefinitions = null,
            IEnumerable<AuxiliaryPrefabDefinition> auxiliaryDefinitions = null)
        {
            if (definitions == null) throw new ArgumentNullException(nameof(definitions));
            foreach (MagicDefinition definition in definitions)
            {
                if (definition == null || byId.ContainsKey(definition.Id) || byName.ContainsKey(definition.Name))
                    throw new InvalidOperationException("Duplicate or null magic definition");
                byId.Add(definition.Id, definition); byName.Add(definition.Name, definition);
            }
            if (fieldDefinitions != null)
                foreach (FieldDefinition definition in fieldDefinitions)
                    if (definition == null || fields.ContainsKey(definition.PrefabId))
                        throw new InvalidOperationException("Duplicate or null field definition");
                    else fields.Add(definition.PrefabId, definition);
            if (auxiliaryDefinitions != null)
                foreach (AuxiliaryPrefabDefinition definition in auxiliaryDefinitions)
                    if (definition == null || auxiliaryPrefabs.ContainsKey(definition.PrefabId))
                        throw new InvalidOperationException("Duplicate or null auxiliary prefab definition");
                    else auxiliaryPrefabs.Add(definition.PrefabId, definition);
        }

        public MagicDefinition GetRequired(int id) => byId.TryGetValue(id, out MagicDefinition value)
            ? value : throw new InvalidOperationException("Missing magic configuration: " + id);

        public MagicDefinition GetRequired(string name) =>
            name != null && byName.TryGetValue(name, out MagicDefinition value)
                ? value : throw new InvalidOperationException("Missing server magic behavior: " + (name ?? "<null>"));

        public bool TryGetField(string prefabId, out FieldDefinition definition) =>
            fields.TryGetValue(prefabId, out definition);

        public bool TryGetAuxiliaryPrefab(string prefabId, out AuxiliaryPrefabDefinition definition) =>
            auxiliaryPrefabs.TryGetValue(prefabId, out definition);

        public static ProductionMagicCatalog Create()
        {
            List<MagicDefinition> values = new List<MagicDefinition>(ServerMagicManifest.All.Count);
            foreach (ServerMagicDescriptor descriptor in ServerMagicManifest.All)
            {
                int count = descriptor.ExecutionKind == MagicExecutionKind.SwarmSpawn ? 3 : 1;
                int secondarySpawnCount = descriptor.ExecutionKind == MagicExecutionKind.Overgrowth ? 3 : 0;
                Fix64 speed = IsMovingShot(descriptor.ExecutionKind) ||
                    descriptor.ExecutionKind == MagicExecutionKind.MovingTornado ? (Fix64)8 : Fix64.Zero;
                int damage = descriptor.ExecutionKind == MagicExecutionKind.Explosion ||
                    descriptor.ExecutionKind == MagicExecutionKind.Shot ||
                    descriptor.ExecutionKind == MagicExecutionKind.VineLine ||
                    descriptor.ExecutionKind == MagicExecutionKind.VineFan ||
                    descriptor.ExecutionKind == MagicExecutionKind.PeriodicArea ||
                    descriptor.ExecutionKind == MagicExecutionKind.MovingTornado ? 10 : 0;
                if (IsMovingShot(descriptor.ExecutionKind) &&
                    descriptor.ExecutionKind != MagicExecutionKind.MindControlShot) damage = 10;
                int lifetime = descriptor.ExecutionKind == MagicExecutionKind.Explosion ? 11 :
                    descriptor.ExecutionKind == MagicExecutionKind.Overgrowth ? 11 :
                    descriptor.ExecutionKind == MagicExecutionKind.VineLine ||
                    descriptor.ExecutionKind == MagicExecutionKind.VineFan ? 40 :
                    IsFallingDrop(descriptor.ExecutionKind) ? FallEndFrames(descriptor.ExecutionKind, 40) :
                    descriptor.ExecutionKind == MagicExecutionKind.PushShot ? 160 :
                    descriptor.ExecutionKind == MagicExecutionKind.LightningColumn ? 2 : 0;
                string statusId = StatusId(descriptor);
                int activationFrames = IsFallingDrop(descriptor.ExecutionKind) ? lifetime : 0;
                int effectFrames = descriptor.ExecutionKind == MagicExecutionKind.RallyingTorch ? 160 :
                    descriptor.ExecutionKind == MagicExecutionKind.MeteorShower ||
                    descriptor.ExecutionKind == MagicExecutionKind.PeriodicArea ||
                    descriptor.ExecutionKind == MagicExecutionKind.PersistentArea ||
                    descriptor.ExecutionKind == MagicExecutionKind.MovingTornado ? 160 : 0;
                int effectIntervalFrames = descriptor.ExecutionKind == MagicExecutionKind.MeteorShower ? 20 : 0;
                if (descriptor.ExecutionKind == MagicExecutionKind.PeriodicArea ||
                    descriptor.ExecutionKind == MagicExecutionKind.MovingTornado)
                    effectIntervalFrames = descriptor.Name == "razor_gale" ? 21 : 20;
                lifetime = checked(lifetime + effectFrames);
                Fix64 effectRadius = descriptor.ExecutionKind == MagicExecutionKind.FrenzyDrop ? (Fix64)4 : (Fix64)1.5m;
                values.Add(new MagicDefinition(
                    descriptor.StableId,
                    descriptor.Name,
                    descriptor.RootPrefabId,
                    descriptor.Family,
                    descriptor.ExecutionKind,
                    count,
                    lifetime,
                    damage,
                    (Fix64)6,
                    (Fix64)1.5m,
                    speed,
                    statusId, StatusFrames(statusId), activationFrames, effectRadius,
                    effectFrames, effectIntervalFrames,
                    descriptor.ExecutionKind == MagicExecutionKind.MeteorShower ? "MeteorDrop" :
                    descriptor.ExecutionKind == MagicExecutionKind.Overgrowth ? "SeedSpirit" :
                    descriptor.ExecutionKind == MagicExecutionKind.PushShot ? "WaterField" : null,
                    secondarySpawnCount,
                    descriptor.ExecutionKind == MagicExecutionKind.ChainShot ? 2 : 0,
                    descriptor.ExecutionKind == MagicExecutionKind.ChainShot ? 5 :
                    descriptor.ExecutionKind == MagicExecutionKind.RollingRock ? 2 : 0,
                    descriptor.ExecutionKind == MagicExecutionKind.PushShot ? (Fix64)0.5m : Fix64.Zero,
                    descriptor.Name == "water_explosion" ? (Fix64)5 : Fix64.Zero));
            }
            if (values.Count != 67)
                throw new InvalidOperationException("Game server magic manifest must contain 67 entries");
            return new ProductionMagicCatalog(values, DefaultFields(), DefaultAuxiliaryPrefabs());
        }

        public static ProductionMagicCatalog Create(IReadOnlyList<MagicParameterValue> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                throw new InvalidOperationException("Versioned game parameters are required for magic simulation");
            List<MagicDefinition> values = new List<MagicDefinition>(ServerMagicManifest.All.Count);
            foreach (ServerMagicDescriptor descriptor in ServerMagicManifest.All)
            {
                string objectName = ParameterObjectName(descriptor.RootPrefabId);
                int count = descriptor.ExecutionKind == MagicExecutionKind.SwarmSpawn
                    ? RequiredInt(parameters, objectName, "quantity") : 1;
                int secondarySpawnCount = descriptor.ExecutionKind == MagicExecutionKind.Overgrowth
                    ? RequiredInt(parameters, objectName, "quantity") : 0;
                Fix64 range = Required(parameters, FamilyObjectName(descriptor.Family), "range");
                int damage = RequiresDamage(descriptor.ExecutionKind)
                    ? RequiredInt(parameters, objectName, "damage") : 0;
                Fix64 radius = RequiresMagicRadius(descriptor.ExecutionKind)
                    ? Required(parameters, objectName, "radius") : Fix64.Zero;
                Fix64 speed = IsMovingShot(descriptor.ExecutionKind) ||
                    descriptor.ExecutionKind == MagicExecutionKind.MovingTornado
                    ? Required(parameters, objectName, "speed") : Fix64.Zero;
                int activationFrames = ActivationFrames(parameters, objectName, descriptor.ExecutionKind);
                int effectFrames = EffectFrames(parameters, objectName, descriptor.ExecutionKind);
                int lifetime = checked(activationFrames + effectFrames);
                if (descriptor.ExecutionKind == MagicExecutionKind.Explosion ||
                    descriptor.ExecutionKind == MagicExecutionKind.LightningColumn ||
                    descriptor.ExecutionKind == MagicExecutionKind.Overgrowth ||
                    descriptor.ExecutionKind == MagicExecutionKind.VineLine ||
                    descriptor.ExecutionKind == MagicExecutionKind.VineFan ||
                    IsMovingShot(descriptor.ExecutionKind))
                    lifetime = LifetimeFrames(parameters, objectName, descriptor.ExecutionKind);
                Fix64 effectRadius = EffectRadius(parameters, objectName, descriptor.ExecutionKind, radius);
                int effectIntervalFrames = descriptor.ExecutionKind == MagicExecutionKind.MeteorShower
                    ? AtLeastElapsedFrames(Required(parameters, objectName, "attack_interval")) : 0;
                if (descriptor.ExecutionKind == MagicExecutionKind.PeriodicArea ||
                    descriptor.ExecutionKind == MagicExecutionKind.MovingTornado)
                    effectIntervalFrames = descriptor.Name == "razor_gale"
                        ? StrictElapsedFrames(Required(parameters, objectName, "attack_interval"))
                        : AtLeastElapsedFrames(Required(parameters, objectName, "attack_interval"));
                string statusId = StatusId(descriptor);
                int statusFrames = descriptor.Name == "shock_overload" ? 160 :
                    descriptor.ExecutionKind == MagicExecutionKind.FrenzyDrop ||
                    descriptor.ExecutionKind == MagicExecutionKind.RallyingTorch
                    ? AtLeastElapsedFrames(Required(parameters, objectName, "buff_duration"))
                    : StatusFrames(statusId);
                values.Add(new MagicDefinition(descriptor.StableId, descriptor.Name,
                    descriptor.RootPrefabId, descriptor.Family, descriptor.ExecutionKind,
                    count, lifetime, damage, range, radius, speed, statusId, statusFrames,
                    activationFrames, effectRadius, effectFrames, effectIntervalFrames,
                    descriptor.ExecutionKind == MagicExecutionKind.MeteorShower ? "MeteorDrop" :
                    descriptor.ExecutionKind == MagicExecutionKind.Overgrowth ? "SeedSpirit" :
                    descriptor.ExecutionKind == MagicExecutionKind.PushShot ? "WaterField" : null,
                    secondarySpawnCount,
                    descriptor.ExecutionKind == MagicExecutionKind.ChainShot
                        ? RequiredInt(parameters, objectName, "min_damage") : 0,
                    descriptor.ExecutionKind == MagicExecutionKind.ChainShot ? 5 :
                    descriptor.ExecutionKind == MagicExecutionKind.RollingRock ? 2 : 0,
                    descriptor.ExecutionKind == MagicExecutionKind.PushShot ? (Fix64)0.5m : Fix64.Zero,
                    descriptor.Name == "water_explosion"
                        ? Required(parameters, objectName, "z_force") : Fix64.Zero));
            }
            return new ProductionMagicCatalog(values, ParameterFields(parameters), ParameterAuxiliaryPrefabs(parameters));
        }

        private static IEnumerable<AuxiliaryPrefabDefinition> DefaultAuxiliaryPrefabs()
        {
            yield return new AuxiliaryPrefabDefinition("CraterEmber", AuxiliaryPrefabKind.CraterEmber,
                (Fix64)0.35m, (Fix64)1.5m, 3);
            string[] runes = { "FireRune", "WaterRune", "NatureRune", "RockRune", "LightningRune", "WindRune" };
            for (int index = 0; index < runes.Length; index++)
                yield return new AuxiliaryPrefabDefinition(runes[index], AuxiliaryPrefabKind.Rune,
                    (Fix64)0.5m, (Fix64)2, 5);
            yield return new AuxiliaryPrefabDefinition("MagmaFist", AuxiliaryPrefabKind.OnStartArea,
                (Fix64)1.5m, (Fix64)1.5m, 5, 40, "burn");
            yield return new AuxiliaryPrefabDefinition("Vine", AuxiliaryPrefabKind.OnStartArea,
                (Fix64)1.5m, (Fix64)1.5m, 5, 40, "snared");
            yield return new AuxiliaryPrefabDefinition("MeteorDrop", AuxiliaryPrefabKind.FallingDrop,
                (Fix64)0.5m, (Fix64)0.5m, 10, 41);
        }

        private static IEnumerable<AuxiliaryPrefabDefinition> ParameterAuxiliaryPrefabs(
            IReadOnlyList<MagicParameterValue> parameters)
        {
            yield return new AuxiliaryPrefabDefinition("CraterEmber", AuxiliaryPrefabKind.CraterEmber,
                Required(parameters, "crater_ember", "radius"),
                Required(parameters, "crater_ember", "attack_range"),
                RequiredInt(parameters, "crater_ember", "damage"));
            Fix64 runeRadius = Optional(parameters, "rune", "radius");
            Fix64 runeRange = Optional(parameters, "rune", "attack_range");
            int runeDamage = OptionalInt(parameters, "rune", "damage");
            string[] runes = { "FireRune", "WaterRune", "NatureRune", "RockRune", "LightningRune", "WindRune" };
            if (runeRadius > Fix64.Zero && runeRange > Fix64.Zero)
                for (int index = 0; index < runes.Length; index++)
                    yield return new AuxiliaryPrefabDefinition(runes[index], AuxiliaryPrefabKind.Rune,
                        runeRadius, runeRange, runeDamage);
            yield return OnStartAuxiliary(parameters, "MagmaFist", "magma_fist", "burn");
            yield return OnStartAuxiliary(parameters, "Vine", "vine", "snared");
            yield return new AuxiliaryPrefabDefinition("MeteorDrop", AuxiliaryPrefabKind.FallingDrop,
                Required(parameters, "meteor_drop", "radius"),
                Required(parameters, "meteor_drop", "radius"),
                RequiredInt(parameters, "meteor_drop", "damage"), 41);
        }

        private static AuxiliaryPrefabDefinition OnStartAuxiliary(
            IReadOnlyList<MagicParameterValue> parameters, string prefabId, string objectName,
            string statusId) => new AuxiliaryPrefabDefinition(prefabId, AuxiliaryPrefabKind.OnStartArea,
                Required(parameters, objectName, "radius"), Required(parameters, objectName, "radius"),
                RequiredInt(parameters, objectName, "damage"),
                AtLeastElapsedFrames(Required(parameters, objectName, "duration")), statusId);

        private static IEnumerable<FieldDefinition> DefaultFields()
        {
            yield return new FieldDefinition("FireField", (Fix64)1.5m, 60, "burn");
            yield return new FieldDefinition("WaterField", (Fix64)1.5m, 60, "wet");
            yield return new FieldDefinition("ElectricField", (Fix64)1.5m, 60, "shock");
            yield return new FieldDefinition("LeafField", (Fix64)1.5m, 60, "snared");
        }

        private static IEnumerable<FieldDefinition> ParameterFields(
            IReadOnlyList<MagicParameterValue> parameters)
        {
            yield return ParameterField(parameters, "FireField", "fire_field", "burn");
            yield return ParameterField(parameters, "WaterField", "water_field", "wet");
            yield return ParameterField(parameters, "ElectricField", "electric_field", "shock");
            yield return ParameterField(parameters, "LeafField", "leaf_field", "snared");
        }

        private static FieldDefinition ParameterField(IReadOnlyList<MagicParameterValue> parameters,
            string prefabId, string objectName, string statusId) => new FieldDefinition(prefabId,
            Required(parameters, objectName, "radius"),
            AtLeastElapsedFrames(Required(parameters, objectName, "duration")), statusId);

        private static int LifetimeFrames(IReadOnlyList<MagicParameterValue> parameters,
            string objectName, MagicExecutionKind executionKind)
        {
            Fix64 duration = Optional(parameters, objectName, "duration");
            if (duration > Fix64.Zero) return AtLeastElapsedFrames(duration);
            if (executionKind == MagicExecutionKind.VineLine ||
                executionKind == MagicExecutionKind.VineFan)
                return AtLeastElapsedFrames(Required(parameters, objectName, "duration"));
            if (executionKind == MagicExecutionKind.Explosion) return 11;
            if (executionKind == MagicExecutionKind.Overgrowth) return 11;
            if (executionKind == MagicExecutionKind.LightningColumn) return 2;
            if (executionKind == MagicExecutionKind.FrenzyDrop)
            {
                Fix64 speed = Required(parameters, objectName, "speed");
                return FallEndFrames(executionKind, AtLeastElapsedFrames((Fix64)10 / speed));
            }
            if (IsFallingDrop(executionKind)) return FallEndFrames(executionKind, 40);
            return 0;
        }

        private static int ActivationFrames(IReadOnlyList<MagicParameterValue> parameters,
            string objectName, MagicExecutionKind executionKind)
        {
            if (executionKind == MagicExecutionKind.FrenzyDrop)
                return FallEndFrames(executionKind,
                    AtLeastElapsedFrames((Fix64)10 / Required(parameters, objectName, "speed")));
            return IsFallingDrop(executionKind) ? FallEndFrames(executionKind, 40) : 0;
        }

        private static int FallEndFrames(MagicExecutionKind executionKind, int travelFrames) =>
            executionKind == MagicExecutionKind.RallyingTorch
                ? travelFrames : checked(travelFrames + 1);

        private static int EffectFrames(IReadOnlyList<MagicParameterValue> parameters,
            string objectName, MagicExecutionKind executionKind)
        {
            if (executionKind == MagicExecutionKind.RallyingTorch ||
                executionKind == MagicExecutionKind.MeteorShower ||
                executionKind == MagicExecutionKind.PeriodicArea ||
                executionKind == MagicExecutionKind.PersistentArea ||
                executionKind == MagicExecutionKind.MovingTornado)
                return AtLeastElapsedFrames(Required(parameters, objectName, "duration"));
            return 0;
        }

        private static Fix64 EffectRadius(IReadOnlyList<MagicParameterValue> parameters,
            string objectName, MagicExecutionKind executionKind, Fix64 radius)
        {
            if (executionKind == MagicExecutionKind.FrenzyDrop)
                return Required(parameters, objectName, "attack_range");
            if (executionKind == MagicExecutionKind.ChainShot)
                return Required(parameters, objectName, "attack_range");
            return radius;
        }

        private static bool IsFallingDrop(MagicExecutionKind executionKind) =>
            executionKind == MagicExecutionKind.DropImpact ||
            executionKind == MagicExecutionKind.FrenzyDrop ||
            executionKind == MagicExecutionKind.LeafairDrop ||
            executionKind == MagicExecutionKind.RallyingTorch;

        private static bool IsMovingShot(MagicExecutionKind executionKind) =>
            executionKind == MagicExecutionKind.Shot ||
            executionKind == MagicExecutionKind.ChainShot ||
            executionKind == MagicExecutionKind.RollingRock ||
            executionKind == MagicExecutionKind.PushShot ||
            executionKind == MagicExecutionKind.MindControlShot ||
            executionKind == MagicExecutionKind.PiercingShot;

        private static bool RequiresMagicRadius(MagicExecutionKind executionKind) =>
            executionKind != MagicExecutionKind.SingleSpawn &&
            executionKind != MagicExecutionKind.SwarmSpawn &&
            executionKind != MagicExecutionKind.AirSpawn &&
            executionKind != MagicExecutionKind.Build;

        private static bool RequiresDamage(MagicExecutionKind executionKind) =>
            executionKind != MagicExecutionKind.SingleSpawn &&
            executionKind != MagicExecutionKind.SwarmSpawn &&
            executionKind != MagicExecutionKind.AirSpawn &&
            executionKind != MagicExecutionKind.Build &&
            executionKind != MagicExecutionKind.FrenzyDrop &&
            executionKind != MagicExecutionKind.RallyingTorch &&
            executionKind != MagicExecutionKind.MeteorShower &&
            executionKind != MagicExecutionKind.Overgrowth &&
            executionKind != MagicExecutionKind.PersistentArea &&
            executionKind != MagicExecutionKind.MindControlShot;

        private static string StatusId(ServerMagicDescriptor descriptor)
        {
            switch (descriptor.Name)
            {
                case "fire_shot":
                case "magma_explosion": return "burn";
                case "water_shot": return "wet";
                case "water_explosion": return "burn";
                case "lightning_shot":
                case "lightning_explosion": return "shock";
                case "shock_overload": return "shock-overload";
                case "chain_lightning": return "shock";
                case "tide_call": return "wet";
                case "vine_toss":
                case "vine_fan": return "snared";
                case "leafair": return "snared";
                case "frenzy_totem": return "frenzy";
                case "rallying_torch": return "inspired";
                case "sand_storm": return "sandstorm";
                case "wind_blade":
                case "wind_explosion": return "knockback";
                default: return null;
            }
        }

        private static int StatusFrames(string statusId)
        {
            switch (statusId)
            {
                case "burn":
                case "wet":
                case "snared": return 60;
                case "shock":
                case "knockback": return 10;
                case "shock-overload": return 160;
                case "frenzy":
                case "inspired": return 200;
                case "sandstorm": return 10;
                default: return 0;
            }
        }

        private static string FamilyObjectName(MagicBehaviorFamily family)
        {
            switch (family)
            {
                case MagicBehaviorFamily.Spawn: return "spawn";
                case MagicBehaviorFamily.Projectile: return "shoot";
                case MagicBehaviorFamily.Area: return "explode";
                case MagicBehaviorFamily.Build: return "build";
                case MagicBehaviorFamily.Drop: return "drop";
                default: throw new ArgumentOutOfRangeException(nameof(family));
            }
        }

        private static string ParameterObjectName(string prefabId)
        {
            if (prefabId == "Leafair") return "leaf_drop";
            if (prefabId == "PveNatureSlimeNest") return "leaf_slime";
            if (prefabId == "PveWaterSlimeNest") return "water_slime";
            List<char> characters = new List<char>(prefabId.Length + 4);
            for (int index = 0; index < prefabId.Length; index++)
            {
                char value = prefabId[index];
                if (index > 0 && char.IsUpper(value) &&
                    (!char.IsUpper(prefabId[index - 1]) ||
                     index + 1 < prefabId.Length && char.IsLower(prefabId[index + 1])))
                    characters.Add('_');
                characters.Add(char.ToLowerInvariant(value));
            }
            return new string(characters.ToArray());
        }

        private static Fix64 Required(IReadOnlyList<MagicParameterValue> parameters,
            string objectName, string parameterName)
        {
            for (int index = 0; index < parameters.Count; index++)
                if (string.Equals(parameters[index].ObjectName, objectName, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(parameters[index].ParameterName, parameterName, StringComparison.OrdinalIgnoreCase))
                    return parameters[index].Value;
            throw new InvalidOperationException("Missing deterministic magic parameter: " +
                objectName + "." + parameterName);
        }

        private static Fix64 Optional(IReadOnlyList<MagicParameterValue> parameters,
            string objectName, string parameterName)
        {
            for (int index = 0; index < parameters.Count; index++)
                if (string.Equals(parameters[index].ObjectName, objectName, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(parameters[index].ParameterName, parameterName, StringComparison.OrdinalIgnoreCase))
                    return parameters[index].Value;
            return Fix64.Zero;
        }

        private static int RequiredInt(IReadOnlyList<MagicParameterValue> parameters,
            string objectName, string parameterName) => ToInt(Required(parameters, objectName, parameterName));

        private static int OptionalInt(IReadOnlyList<MagicParameterValue> parameters,
            string objectName, string parameterName) => ToInt(Optional(parameters, objectName, parameterName));

        private static int ToInt(Fix64 value) => checked((int)(value + (Fix64)0.5m));

        private static int AtLeastElapsedFrames(Fix64 seconds)
        {
            Fix64 frames = seconds * 20;
            int floor = checked((int)Fix64.Floor(frames));
            return frames == (Fix64)floor ? floor : checked(floor + 1);
        }

        private static int StrictElapsedFrames(Fix64 seconds) =>
            checked((int)Fix64.Floor(seconds * 20) + 1);
    }
}
