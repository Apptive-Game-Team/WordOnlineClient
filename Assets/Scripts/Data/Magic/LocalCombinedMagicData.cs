using System.Collections.Generic;

namespace Data.Magic
{
    public static class LocalCombinedMagicData
    {
        public static List<CombinedMagicData> dataList = new()
        {
            new (){id = 1, magicName = "Ember Spirit Swarm", recipe = new () { CardType.Spawn , CardType.Fire}, spritePath = "Game/spawn/ember_spirit"},
            new (){id = 2, magicName = "Water Slime Swarm", recipe = new () { CardType.Spawn , CardType.Water}, spritePath = "Game/spawn/water_slime"},
            new (){id = 4, magicName = "Seed Spirit Swarm", recipe = new () { CardType.Spawn , CardType.Nature}, spritePath = "Game/spawn/seed_spirit"},
            new (){id = 3, magicName = "Mini Rock Swarm", recipe = new () { CardType.Spawn , CardType.Rock}, spritePath = "Game/spawn/mini_rock"},
            new (){id = 5, magicName = "Wind Spirit", recipe = new () { CardType.Spawn , CardType.Wind}, spritePath = "Game/spawn/wind_spirit"},
            
            new (){id = 7, magicName = "Fire Shot", recipe = new () { CardType.Shoot , CardType.Fire}, spritePath = "Game/shoot/fire_shoot"},
            new (){id = 8, magicName = "Tide Call", recipe = new () { CardType.Shoot , CardType.Water}, spritePath = "Game/shoot/tide_call"},
            new (){id = 9, magicName = "Chain Lightning", recipe = new () { CardType.Shoot , CardType.Lightning}, spritePath = "Game/shoot/chain_lightning"},
            new (){id = 10, magicName = "Vine Toss", recipe = new () { CardType.Shoot , CardType.Nature}, spritePath = "Game/build/vine"},
            new (){id = 11, magicName = "Rock Rolling", recipe = new () { CardType.Shoot , CardType.Rock}, spritePath = "Game/shoot/rock_rolling"},
            new (){id = 12, magicName = "Wind Blade", recipe = new () { CardType.Shoot , CardType.Wind}, spritePath = "Game/shoot/wind_blade"},
            
            new (){id = 16, magicName = "Life Tree", recipe = new () { CardType.Build , CardType.Nature}, spritePath = "Game/build/life_tree"},
            new (){id = 17, magicName = "Rock Turret", recipe = new () { CardType.Build , CardType.Rock}, spritePath = "Game/build/rock_turret"},
            new (){id = 18, magicName = "Wind Totem", recipe = new () { CardType.Build , CardType.Wind}, spritePath = "Game/build/wind_totem"},
            
            new (){id = 19, magicName = "Magma Explosion", recipe = new () { CardType.Explode , CardType.Fire}, spritePath = "Game/explode/magma_explosion"},
            new (){id = 20, magicName = "Water Explosion", recipe = new () { CardType.Explode , CardType.Water}, spritePath = "Game/explode/water_explosion"},
            
            new (){id = 25, magicName = "Cannon", recipe = new () { CardType.Build , CardType.Shoot, CardType.Rock}, spritePath = "Game/build/ground_cannon"},
            new (){id = 26, magicName = "Tower", recipe = new () { CardType.Build , CardType.Explode, CardType.Rock}, spritePath = "Game/build/ground_tower"},
            new (){id = 27, magicName = "Mana Well", recipe = new () { CardType.Build , CardType.Lightning, CardType.Nature}, spritePath = "Game/build/mana_well"},
            new (){id = 28, magicName = "Aqua Archer", recipe = new () { CardType.Spawn , CardType.Shoot, CardType.Water}, spritePath = "Game/spawn/aqua_archer"},
            new (){id = 29, magicName = "Rock Golem", recipe = new () { CardType.Spawn , CardType.Rock, CardType.Rock}, spritePath = "Game/spawn/rock_golem"},
            new (){id = 30, magicName = "Storm Rider", recipe = new () { CardType.Spawn , CardType.Water, CardType.Lightning}, spritePath = "Game/spawn/storm_rider"},
            new (){id = 31, magicName = "Thunder Spirit", recipe = new () { CardType.Spawn , CardType.Lightning, CardType.Wind}, spritePath = "Game/spawn/thunder_spirit"},
            new (){id = 32, magicName = "Fire Spirit", recipe = new () { CardType.Spawn , CardType.Wind, CardType.Fire}, spritePath = "Game/spawn/fire_spirit"},
            
            new (){id = 34, magicName = "Rain Cloud", recipe = new () { CardType.Drop , CardType.Water}, spritePath = "Game/drop/rain_cloud"},
            new (){id = 35, magicName = "Lightning Drop", recipe = new () { CardType.Drop , CardType.Lightning}, spritePath = "Game/drop/lightning_drop"},
            
            new (){id = 39, magicName = "Magma Spirit", recipe = new () { CardType.Spawn , CardType.Explode, CardType.Fire, CardType.Fire, CardType.Rock}, spritePath = "Game/spawn/magma_spirit"},
            new (){id = 40, magicName = "Healing Totem", recipe = new () { CardType.Build , CardType.Nature, CardType.Water}, spritePath = "Game/build/healing_totem"},
            new (){id = 41, magicName = "Sand Storm", recipe = new () { CardType.Explode , CardType.Rock, CardType.Wind}, spritePath = "Game/explode/sand_storm"},
            new (){id = 43, magicName = "Tornado Strike", recipe = new () { CardType.Spawn , CardType.Wind, CardType.Wind, CardType.Wind, CardType.Nature}, spritePath = "Game/spawn/tornado_strike"},
            new (){id = 44, magicName = "Meteor Shower", recipe = new () { CardType.Drop , CardType.Fire, CardType.Fire, CardType.Rock, CardType.Rock}, spritePath = "Game/drop/meteor_drop"},
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