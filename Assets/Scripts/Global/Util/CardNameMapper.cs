using System;
using System.Collections.Generic;
using Scripts.Data;

namespace Scripts.Global.Util
{
    public class CardNameMapper
    {
        private static readonly Dictionary<string, CardType> NameToType = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Build", CardType.Build }, { "Spawn", CardType.Spawn }, { "Shoot", CardType.Shoot }, { "Explode", CardType.Explode },
            { "Fire", CardType.Fire }, { "Water", CardType.Water }, { "Lightning", CardType.Lightning },
            { "Nature", CardType.Nature }, { "Rock", CardType.Rock }, { "Wind", CardType.Wind },
        };

        public static bool TryMapToCardType(string name, out CardType type)
        {
            if (Enum.TryParse(name, true, out type)) return true;
            return NameToType.TryGetValue(name, out type);
        }
    }
}