using System;
using System.Collections.Generic;

namespace Data.GameConfig
{
    [Serializable]
    public class GameConfigData
    {
        public string version;
        public List<MagicRecipeData> magicRecipes;
        public List<GameParameterData> parameters;
        public List<ElementalChartEntry> elementalChart;
        public List<CardDefinitionData> cards;
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

    [Serializable]
    public class GameParameterData
    {
        public string gameObjectName;
        public string paramName;
        public float value;
    }
}
