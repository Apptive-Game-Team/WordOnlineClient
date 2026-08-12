// 수정 후 GameParameterResolver.TryGetMagicParameter 경로 — 현재 HEAD에서 그대로 복사.
// 변경점: ParametersDataSource.GetCachedParameters() → BenchData.Table 치환 뿐.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bench.NewImpl
{
    public static class GameParameterResolver
    {
        private static readonly Dictionary<MagicParameterKey, float?> MagicParameterCache = new();
        private static IReadOnlyList<GameParameterData> cachedParameterSource;

        public static void ResetCacheForBench()
        {
            MagicParameterCache.Clear();
            cachedParameterSource = null;
        }

        public static bool TryGetMagicParameter(CombinedMagicData magic, string paramName, out float value)
        {
            value = 0f;
            if (magic == null || string.IsNullOrWhiteSpace(paramName))
            {
                return false;
            }

            var parameters = (IReadOnlyList<GameParameterData>)BenchData.Table;
            if (parameters == null || parameters.Count == 0)
            {
                return false;
            }

            InvalidateCacheIfSourceChanged(parameters);

            var key = new MagicParameterKey(magic, paramName);
            if (MagicParameterCache.TryGetValue(key, out float? cached))
            {
                if (!cached.HasValue)
                {
                    return false;
                }

                value = cached.Value;
                return true;
            }

            bool resolved = ResolveMagicParameter(parameters, magic, paramName, out value);
            MagicParameterCache[key] = resolved ? value : (float?)null;
            return resolved;
        }

        private static void InvalidateCacheIfSourceChanged(IReadOnlyList<GameParameterData> parameters)
        {
            if (ReferenceEquals(cachedParameterSource, parameters))
            {
                return;
            }

            cachedParameterSource = parameters;
            MagicParameterCache.Clear();
        }

        private static bool ResolveMagicParameter(
            IReadOnlyList<GameParameterData> parameters,
            CombinedMagicData magic,
            string paramName,
            out float value)
        {
            value = 0f;
            var objectNames = GetObjectNamesForParameter(parameters, magic, paramName);
            foreach (var objectName in objectNames)
            {
                var parameter = parameters.FirstOrDefault(p =>
                    IsSameName(p.gameObjectName, objectName) &&
                    IsSameName(p.paramName, paramName));
                if (parameter != null)
                {
                    value = parameter.value;
                    return true;
                }
            }

            return false;
        }

        private static List<string> GetObjectNamesForParameter(
            IReadOnlyList<GameParameterData> parameters,
            CombinedMagicData magic,
            string paramName)
        {
            var result = new List<string>();
            if (IsSameName(paramName, "range") && TryGetMagicFamilyObjectName(magic, out var familyObjectName))
            {
                result.Add(familyObjectName);
            }

            result.AddRange(GetObjectNamesForMagic(parameters, magic));
            return result
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Where(name => parameters.Any(parameter => IsSameName(parameter.gameObjectName, name)))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static bool TryGetMagicFamilyObjectName(CombinedMagicData magic, out string objectName)
        {
            objectName = null;
            if (magic == null)
            {
                return false;
            }

            switch (magic.castType)
            {
                case CardType.Spawn:
                    objectName = "spawn";
                    return true;
                case CardType.Drop:
                    objectName = "drop";
                    return true;
                case CardType.Explode:
                    objectName = "explode";
                    return true;
                case CardType.Build:
                    objectName = "build";
                    return true;
                case CardType.Shoot:
                    objectName = "shoot";
                    return true;
                default:
                    return false;
            }
        }

        private static List<string> GetObjectNamesForMagic(
            IReadOnlyList<GameParameterData> parameters,
            CombinedMagicData magic)
        {
            var result = parameters
                .Where(parameter => IsSameName(parameter.paramName, "magic_id"))
                .Where(parameter => Math.Abs(parameter.value - magic.id) < 0.01f)
                .Select(parameter => parameter.gameObjectName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct()
                .ToList();

            if (result.Count > 0)
            {
                return result;
            }

            var candidates = new[]
            {
                magic.serverName,
                ToSnakeCase(magic.resourceName),
                magic.localizationKey
            };

            return candidates
                .Where(candidate => !string.IsNullOrWhiteSpace(candidate))
                .Where(candidate => parameters.Any(parameter => IsSameName(parameter.gameObjectName, candidate)))
                .Distinct()
                .ToList();
        }

        private static bool IsSameName(string left, string right)
        {
            return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
        }

        private static string ToSnakeCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var chars = new List<char>();
            for (var i = 0; i < value.Length; i++)
            {
                var current = value[i];
                if (char.IsUpper(current) && i > 0)
                {
                    chars.Add('_');
                }

                chars.Add(char.ToLowerInvariant(current));
            }

            return new string(chars.ToArray());
        }

        private readonly struct MagicParameterKey : IEquatable<MagicParameterKey>
        {
            private readonly long magicId;
            private readonly CardType castType;
            private readonly string serverName;
            private readonly string resourceName;
            private readonly string localizationKey;
            private readonly string paramName;

            public MagicParameterKey(CombinedMagicData magic, string paramName)
            {
                magicId = magic.id;
                castType = magic.castType;
                serverName = magic.serverName;
                resourceName = magic.resourceName;
                localizationKey = magic.localizationKey;
                this.paramName = paramName;
            }

            public bool Equals(MagicParameterKey other)
            {
                return magicId == other.magicId &&
                       castType == other.castType &&
                       string.Equals(serverName, other.serverName, StringComparison.Ordinal) &&
                       string.Equals(resourceName, other.resourceName, StringComparison.Ordinal) &&
                       string.Equals(localizationKey, other.localizationKey, StringComparison.Ordinal) &&
                       string.Equals(paramName, other.paramName, StringComparison.Ordinal);
            }

            public override bool Equals(object obj) => obj is MagicParameterKey other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = magicId.GetHashCode();
                    hash = hash * 397 ^ (int)castType;
                    hash = hash * 397 ^ (serverName?.GetHashCode() ?? 0);
                    hash = hash * 397 ^ (resourceName?.GetHashCode() ?? 0);
                    hash = hash * 397 ^ (localizationKey?.GetHashCode() ?? 0);
                    hash = hash * 397 ^ (paramName?.GetHashCode() ?? 0);
                    return hash;
                }
            }
        }
    }
}
