using System.Collections.Generic;
using Data.Util;

namespace Data.Magic
{
    public static class LocalCombinedMagicData
    {
        /// <summary>
        /// Returns the effective recipe list.
        /// When a versioned server payload is available, recipes come from the server
        /// (authoritative) and server name is used for both localization and sprite resource lookup.
        /// When the payload is unavailable, returns an empty list instead of using local recipe fallbacks.
        /// </summary>
        public static List<CombinedMagicData> GetEffectiveDataList()
        {
            var versionedMagics = MagicInfoDataSource.GetCachedMagics();
            if (versionedMagics != null && versionedMagics.Count > 0)
            {
                return BuildCombinedMagicData(versionedMagics);
            }

            return emptyDataList;
        }

        public static CombinedMagicData GetCombinedMagicData(string name)
        {
            return GetEffectiveDataList().Find(x => x.localizationKey == name);
        }

        public static bool TryGetByRecipe(IList<CardType> recipe, out CombinedMagicData match)
        {
            if (recipe == null)
            {
                match = null;
                return false;
            }

            foreach (var data in GetEffectiveDataList())
            {
                if (AreSameMultiset(data.recipe, recipe))
                {
                    match = data;
                    return true;
                }
            }

            match = null;
            return false;
        }

        private static List<CombinedMagicData> BuildCombinedMagicData<T>(IReadOnlyList<T> source)
            where T : IMagicRecipeSource
        {
            var result = new List<CombinedMagicData>(source.Count);
            foreach (var serverRecipe in source)
            {
                var recipe = new List<CardType>(serverRecipe.Cards.Count);
                var valid = true;
                foreach (var cardName in serverRecipe.Cards)
                {
                    if (System.Enum.TryParse(cardName, true, out CardType cardType))
                    {
                        recipe.Add(cardType);
                    }
                    else
                    {
                        valid = false;
                        break;
                    }
                }

                if (!valid)
                {
                    continue;
                }

                result.Add(new CombinedMagicData
                {
                    id = serverRecipe.Id,
                    localizationKey = StringUtils.ToCamelCase(serverRecipe.Name),
                    resourceName = StringUtils.ToPascalCase(serverRecipe.Name),
                    recipe = recipe,
                });
            }

            return result;
        }

        private static readonly List<CombinedMagicData> emptyDataList = new();

        private static bool AreSameMultiset(IList<CardType> a, IList<CardType> b)
        {
            if (a == null || b == null) return false;
            if (a.Count != b.Count) return false;

            var counts = new Dictionary<CardType, int>();
            foreach (var x in a)
            {
                counts.TryGetValue(x, out var count);
                counts[x] = count + 1;
            }

            foreach (var y in b)
            {
                if (!counts.TryGetValue(y, out var count) || count == 0)
                    return false;

                counts[y] = count - 1;
            }

            return true;
        }
    }
}
