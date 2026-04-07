using System;
using System.Collections.Generic;

namespace Data.GameConfig
{
    [Serializable]
    public class GameConfigData
    {
        public string version;
        public List<MagicRecipeData> magicRecipes;
        public GameParametersData parameters;
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

    /// <summary>
    /// Flat struct for game balance parameters. JsonUtility does not support
    /// Dictionary, so DB rows are mapped to named fields. Field names match
    /// the pattern {gameObject}_{parameter} in snake_case.
    /// </summary>
    [Serializable]
    public class GameParametersData
    {
        // player
        public int player_max_hp = 100;
        public int player_max_card_num = 6;
        public int player_max_mana = 10;
        public float player_mana_charge_interval = 0.2f;
        public int player_mana_charge_value = 1;

        // world
        public int world_fps = 20;
        public float world_gravity = 4f;
        public int world_x_bound = 15;
        public int world_y_bound = 10;

        // slime
        public float slime_hp = 8f;
        public float slime_damage = 3f;
        public float slime_radius = 0.5f;
        public float slime_speed = 0.8f;
        public float slime_mass = 1f;

        // shoot
        public float shoot_damage = 10f;
        public float shoot_radius = 0.5f;

        // explode
        public float explode_damage = 8f;
        public float explode_radius = 0.5f;

        // field
        public float field_duration = 3f;

        // per-card mana costs
        public int fire_mana_cost = 2;
        public int water_mana_cost = 2;
        public int lightning_mana_cost = 2;
        public int rock_mana_cost = 2;
        public int nature_mana_cost = 2;
        public int wind_mana_cost = 2;
        public int shoot_card_mana_cost = 1;
        public int build_mana_cost = 1;
        public int spawn_mana_cost = 1;
        public int explode_card_mana_cost = 1;
        public int drop_mana_cost = 1;
    }
}
