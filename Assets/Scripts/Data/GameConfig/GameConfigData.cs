using System;
using System.Collections.Generic;

namespace Data.GameConfig
{
    [Serializable]
    public class GameConfigData
    {
        public string version;
        public List<MagicRecipeData> magicRecipes;
        public List<GameParameterEntry> parameters;
        public List<ElementalChartEntry> elementalChart;
        public List<CardDefinitionData> cards;
    }

    /// <summary>
    /// One balance parameter entry from GET /api/data/config.
    /// Using a list of {group, key, value} tuples so that multi-word game object
    /// names (e.g. "fire_spirit", "healing_totem") remain unambiguous — a flat
    /// map with '_' separator would be ambiguous since names themselves contain '_'.
    /// </summary>
    [Serializable]
    public class GameParameterEntry
    {
        public string group;
        public string key;
        public double value;
    }

    [Serializable]
    public class MagicRecipeData
    {
        public long id;
        public string name;
        public List<string> cards;
    }

    [Serializable]
    public class ElementalChartEntry
    {
        public string attacker;
        public string defender;
        public float multiplier;
    }

    [Serializable]
    public class CardDefinitionData
    {
        public long id;
        public string name;
        public string cardType; // "Type" or "Magic"
    }
}
