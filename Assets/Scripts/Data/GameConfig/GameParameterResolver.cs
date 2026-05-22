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
            "damage",
            "hp",
            "duration",
            "radius",
            "range",
            "attack_range",
            "attack_interval",
            "speed",
            "count",
            "quantity",
            "mana_cost"
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

            var parameters = GameDataManager.Config?.parameters;
            if (parameters == null || parameters.Count == 0)
            {
                return string.Empty;
            }

            var objectNames = GetObjectNamesForMagic(parameters, magic);
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

                blocks.Add($"{ToDisplayName(objectName)}\n{string.Join("\n", lines)}");
            }

            return string.Join("\n\n", blocks);
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
