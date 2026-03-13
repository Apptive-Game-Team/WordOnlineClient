using System.Collections.Generic;

namespace Data.Magic
{
    public static class LocalCombinedMagicData
    {
        public static List<CombinedMagicData> dataList = new()
        {
            new (){id = 1, magicName = "Fire Slime Swarm", recipe = new () { CardType.Spawn , CardType.Fire}, spritePath = "Game/spawn/fire_slime"},
            new (){id = 2, magicName = "Water Slime Swarm", recipe = new () { CardType.Spawn , CardType.Water}, spritePath = "Game/spawn/water_slime"},
            new (){id = 3, magicName = "Lightning Slime Swarm", recipe = new () { CardType.Spawn , CardType.Lightning}, spritePath = "Game/spawn/lightning_slime"},
            new (){id = 4, magicName = "Nature Slime Swarm", recipe = new () { CardType.Spawn , CardType.Nature}, spritePath = "Game/spawn/leaf_slime"},
            new (){id = 5, magicName = "Rock Slime Swarm", recipe = new () { CardType.Spawn , CardType.Rock}, spritePath = "Game/spawn/rock_slime"},
            new (){id = 6, magicName = "Wind Slime Swarm", recipe = new () { CardType.Spawn , CardType.Wind}, spritePath = "Game/spawn/wind_slime"},
            new (){id = 7, magicName = "Fire Shot", recipe = new () { CardType.Shoot , CardType.Fire}, spritePath = "Game/shoot/fire_shoot"},
            new (){id = 8, magicName = "Water Shot", recipe = new () { CardType.Shoot , CardType.Water}, spritePath = "Game/shoot/water_shoot"},
            new (){id = 9, magicName = "Lightning Shot", recipe = new () { CardType.Shoot , CardType.Lightning}, spritePath = "Game/shoot/lightning_shoot"},
            new (){id = 10, magicName = "Nature Shot", recipe = new () { CardType.Shoot , CardType.Nature}, spritePath = "Game/shoot/leaf_shoot"},
            new (){id = 11, magicName = "Rock Shot", recipe = new () { CardType.Shoot , CardType.Rock}, spritePath = "Game/shoot/rock_shoot"},
            new (){id = 12, magicName = "Wind Shot", recipe = new () { CardType.Shoot , CardType.Wind}, spritePath = "Game/shoot/wind_shoot"},
            new (){id = 13, magicName = "Fire Rune", recipe = new () { CardType.Build , CardType.Fire}, spritePath = "Game/rune/fire_rune"},
            new (){id = 14, magicName = "Water Rune", recipe = new () { CardType.Build , CardType.Water}, spritePath = "Game/rune/water_rune"},
            new (){id = 15, magicName = "Lightning Rune", recipe = new () { CardType.Build , CardType.Lightning}, spritePath = "Game/rune/lightning_rune"},
            new (){id = 16, magicName = "Nature Rune", recipe = new () { CardType.Build , CardType.Nature}, spritePath = "Game/rune/nature_rune"},
            new (){id = 17, magicName = "Rock Rune", recipe = new () { CardType.Build , CardType.Rock}, spritePath = "Game/rune/rock_rune"},
            new (){id = 18, magicName = "Wind Rune", recipe = new () { CardType.Build , CardType.Wind}, spritePath = "Game/rune/wind_rune"},
            new (){id = 19, magicName = "Fire Explosion", recipe = new () { CardType.Explode , CardType.Fire}, spritePath = "Game/explode/fire_explode"},
            new (){id = 20, magicName = "Water Explosion", recipe = new () { CardType.Explode , CardType.Water}, spritePath = "Game/explode/water_explode"},
            new (){id = 21, magicName = "Lightning Explosion", recipe = new () { CardType.Explode , CardType.Lightning}, spritePath = "Game/explode/lightning_explode"},
            new (){id = 22, magicName = "Nature Explosion", recipe = new () { CardType.Explode , CardType.Nature}, spritePath = "Game/explode/leaf_explode"},
            new (){id = 23, magicName = "Rock Explosion", recipe = new () { CardType.Explode , CardType.Rock}, spritePath = "Game/explode/rock_explode"},
            new (){id = 24, magicName = "Wind Explosion", recipe = new () { CardType.Explode , CardType.Wind}, spritePath = "Game/explode/wind_explode"},
            new (){id = 25, magicName = "Cannon", recipe = new () { CardType.Build , CardType.Shoot, CardType.Rock}, spritePath = "Game/build/ground_cannon"},
            new (){id = 26, magicName = "Tower", recipe = new () { CardType.Build , CardType.Explode, CardType.Rock}, spritePath = "Game/build/ground_tower"},
            new (){id = 27, magicName = "Mana Well", recipe = new () { CardType.Build , CardType.Lightning, CardType.Nature}, spritePath = "Game/build/mana_well"},
            new (){id = 28, magicName = "Aqua Archer", recipe = new () { CardType.Spawn , CardType.Shoot, CardType.Water}, spritePath = "Game/spawn/aqua_archer"},
            new (){id = 29, magicName = "Rock Golem", recipe = new () { CardType.Spawn , CardType.Rock, CardType.Rock}, spritePath = "Game/spawn/rock_golem"},
            new (){id = 30, magicName = "Storm Rider", recipe = new () { CardType.Spawn , CardType.Water, CardType.Lightning}, spritePath = "Game/spawn/storm_rider"},
            new (){id = 31, magicName = "Thunder Spirit", recipe = new () { CardType.Spawn , CardType.Lightning, CardType.Wind}, spritePath = "Game/spawn/thunder_spirit"},
            new (){id = 32, magicName = "Fire Spirit", recipe = new () { CardType.Spawn , CardType.Wind, CardType.Fire}, spritePath = "Game/spawn/fire_spirit"},
            new (){id = 33, magicName = "Fire Drop", recipe = new () { CardType.Drop , CardType.Fire}, spritePath = "Game/drop/fire_drop"},
            new (){id = 34, magicName = "Water Drop", recipe = new () { CardType.Drop , CardType.Water}, spritePath = "Game/drop/water_drop"},
            new (){id = 35, magicName = "Lightning Drop", recipe = new () { CardType.Drop , CardType.Lightning}, spritePath = "Game/drop/lightning_drop"},
            new (){id = 36, magicName = "Nature Drop", recipe = new () { CardType.Drop , CardType.Nature}, spritePath = "Game/drop/leaf_drop"},
            new (){id = 37, magicName = "Rock Drop", recipe = new () { CardType.Drop , CardType.Rock}, spritePath = "Game/drop/rock_drop"},
            new (){id = 38, magicName = "Wind Drop", recipe = new () { CardType.Drop , CardType.Wind}, spritePath = "Game/drop/wind_drop"},
            new (){id = 39, magicName = "Magma Spirit", recipe = new () { CardType.Spawn , CardType.Explode, CardType.Fire, CardType.Fire, CardType.Rock}, spritePath = "Game/spawn/magma_spirit"},
            new (){id = 40, magicName = "Healing Totem", recipe = new () { CardType.Build , CardType.Nature, CardType.Water}, spritePath = "Game/build/healing_totem"},
            new (){id = 41, magicName = "Sand Storm", recipe = new () { CardType.Explode , CardType.Rock, CardType.Wind}, spritePath = "Game/explode/sand_storm"},
            new (){id = 42, magicName = "Chain Lightning", recipe = new () { CardType.Shoot , CardType.Lightning, CardType.Lightning, CardType.Lightning}, spritePath = "Game/shoot/chain_lightning"},
            new (){id = 43, magicName = "Tornado Strike", recipe = new () { CardType.Spawn , CardType.Wind, CardType.Wind, CardType.Wind, CardType.Nature}, spritePath = "Game/spawn/tornado_strike"},
            new (){id = 44, magicName = "Meteor Shower", recipe = new () { CardType.Drop , CardType.Fire, CardType.Fire, CardType.Rock, CardType.Rock}, spritePath = "Game/drop/meteor_drop"},
            new (){id = 45, magicName = "Tide Call", recipe = new () { CardType.Shoot , CardType.Water, CardType.Nature}, spritePath = "Game/shoot/tide_call"},
             // 3차 상위 마법
            new (){id = 46, magicName = "Cloud Dragon", recipe = new () { CardType.Spawn , CardType.Water, CardType.Wind}, spritePath = "Game/spawn/cloud_dragon"},
            new (){id = 47, magicName = "Thunder Bird Swarm", recipe = new () { CardType.Spawn , CardType.Drop, CardType.Lightning}, spritePath = "Game/spawn/thunder_bird"},
            new (){id = 48, magicName = "Tree Golem", recipe = new () { CardType.Spawn , CardType.Nature, CardType.Nature}, spritePath = "Game/spawn/tree_golem"},
            new (){id = 49, magicName = "Vine Spirit", recipe = new () { CardType.Spawn , CardType.Shoot, CardType.Nature}, spritePath = "Game/spawn/vine_spirit"},
              // 베타 전 마지막 상위 마법 
            new (){id = 50, magicName = "Vine Colony", recipe = new () { CardType.Build , CardType.Nature, CardType.Nature}, spritePath = "Game/build/vine_colony"},
            new (){id = 51, magicName = "Rock Mage", recipe = new () { CardType.Spawn , CardType.Shoot, CardType.Rock}, spritePath = "Game/spawn/rock_mage"},
        };
        
        public static CombinedMagicData GetCombinedMagicData(string name)
        {
            return dataList.Find(x => x.magicName == name);
        }


        // public static bool TryGetByRecipe(IList<CardType> recipe, out CombinedMagicData match)
        //     => CombinedMagicResolver.TryResolve(recipe, out match);
    }
}