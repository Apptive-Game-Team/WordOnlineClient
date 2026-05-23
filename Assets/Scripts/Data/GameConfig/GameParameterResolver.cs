using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Data.Magic;

namespace Data.GameConfig
{
    public static class GameParameterResolver
    {
        private static readonly string[] DisplayParamOrder =
        {
            "hp",
            "damage",
            "duration",
            "radius",
            "attack_range",
            "attack_interval",
            "speed",
            "count",
            "quantity",
        };

        private static readonly HashSet<string> HiddenParams = new()
        {
            "magic_id",
            "mass"
        };

        public static string GetMagicDisplayStats(CombinedMagicData magic)
        {
            if (magic == null)
            {
                return string.Empty;
            }

            var parameters = ParametersDataSource.GetCachedParameters();
            if (parameters == null || parameters.Count == 0)
            {
                return string.Empty;
            }

            var objectNames = GetDisplayObjectNamesForMagic(parameters, magic);
            if (objectNames.Count == 0)
            {
                return string.Empty;
            }

            var blocks = new List<string>();
            foreach (var objectName in objectNames)
            {
                var stats = parameters
                    .Where(parameter => IsSameName(parameter.gameObjectName, objectName))
                    .Where(parameter => !HiddenParams.Contains(parameter.paramName))
                    .GroupBy(parameter => parameter.paramName)
                    .Select(group => group.First())
                    .OrderBy(parameter => GetDisplayOrder(parameter.paramName))
                    .ThenBy(parameter => parameter.paramName)
                    .ToList();

                if (stats.Count == 0)
                {
                    continue;
                }

                var lines = stats
                    .Select(parameter => $"{ToDisplayName(parameter.paramName)}: {FormatValue(parameter.value)}");

                blocks.Add(string.Join("\n", lines));
            }

            return string.Join("\n\n", blocks);
        }

        private static List<string> GetDisplayObjectNamesForMagic(
            IReadOnlyList<GameParameterData> parameters,
            CombinedMagicData magic)
        {
            var objectNames = GetObjectNamesForMagicByName(parameters, magic);
            if (objectNames.Count > 0)
            {
                return objectNames;
            }

            if (!TryGetMagicFamilyObjectName(magic, out var familyObjectName))
            {
                return objectNames;
            }

            return parameters.Any(parameter => IsSameName(parameter.gameObjectName, familyObjectName))
                ? new List<string> { familyObjectName }
                : objectNames;
        }

        private static List<string> GetObjectNamesForMagicByName(
            IReadOnlyList<GameParameterData> parameters,
            CombinedMagicData magic)
        {
            var candidates = new[]
            {
                magic.serverName,
                ToSnakeCase(magic.resourceName),
                ToSnakeCase(magic.localizationKey),
                magic.localizationKey
            };

            return candidates
                .Where(candidate => !string.IsNullOrWhiteSpace(candidate))
                .Where(candidate => parameters.Any(parameter => IsSameName(parameter.gameObjectName, candidate)))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static bool TryGetMagicParameter(CombinedMagicData magic, string paramName, out float value)
        {
            value = 0f;
            if (magic == null || string.IsNullOrWhiteSpace(paramName))
            {
                return false;
            }

            var parameters = ParametersDataSource.GetCachedParameters();
            if (parameters == null || parameters.Count == 0)
            {
                return false;
            }

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
            if (magic?.recipe == null)
            {
                return false;
            }

            if (magic.recipe.Contains(CardType.Shoot))
            {
                objectName = "shoot";
                return true;
            }

            if (magic.recipe.Contains(CardType.Explode))
            {
                objectName = "explode";
                return true;
            }

            if (magic.recipe.Contains(CardType.Drop))
            {
                objectName = "drop";
                return true;
            }

            if (magic.recipe.Contains(CardType.Spawn))
            {
                objectName = "spawn";
                return true;
            }

            if (magic.recipe.Contains(CardType.Build))
            {
                objectName = "build";
                return true;
            }

            return false;
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

        private static int GetDisplayOrder(string paramName)
        {
            var index = Array.IndexOf(DisplayParamOrder, paramName);
            return index >= 0 ? index : DisplayParamOrder.Length;
        }

        private static bool IsSameName(string left, string right)
        {
            return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
        }

        private static string ToDisplayName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(value.Replace("_", " "));
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

        private static string FormatValue(float value)
        {
            return Math.Abs(value - Math.Round(value)) < 0.001f
                ? ((int)Math.Round(value)).ToString(CultureInfo.InvariantCulture)
                : value.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }
}
