using System.Collections.Generic;
using UnityEngine;

namespace Script.Data
{
    public class CombinedMagicData
    {
        public string magicName;
        public List<CardType> recipe;
        public string spritePath;
        
        public Sprite GetSprite()
        {
            return Resources.Load<Sprite>(spritePath);
        }
    }
    
    public static class LocalCombinedMagicData
    {
        public static List<CombinedMagicData> dataList = new()
        {
            new (){magicName = "Fire Slime Swarm", recipe = new () { CardType.Spawn , CardType.Fire}, spritePath = "Game/spawn/fire_slime"},
            new (){magicName = "Water Slime Swarm", recipe = new () { CardType.Spawn , CardType.Water}, spritePath = "Game/spawn/water_slime"},
            new (){magicName = "Lightning Slime Swarm", recipe = new () { CardType.Spawn , CardType.Lightning}, spritePath = "Game/spawn/lightning_slime"},
            new (){magicName = "Nature Slime Swarm", recipe = new () { CardType.Spawn , CardType.Nature}, spritePath = "Game/spawn/leaf_slime"},
            new (){magicName = "Rock Slime Swarm", recipe = new () { CardType.Spawn , CardType.Rock}, spritePath = "Game/spawn/rock_slime"},
            new (){magicName = "Wind Slime Swarm", recipe = new () { CardType.Spawn , CardType.Wind}, spritePath = "Game/spawn/wind_slime"},
            new (){magicName = "Fire Shot", recipe = new () { CardType.Shoot , CardType.Fire}, spritePath = "Game/shoot/fire_shoot"},
            new (){magicName = "Water Shot", recipe = new () { CardType.Shoot , CardType.Water}, spritePath = "Game/shoot/water_shoot"},
            new (){magicName = "Lightning Shot", recipe = new () { CardType.Shoot , CardType.Lightning}, spritePath = "Game/shoot/lightning_shoot"},
            new (){magicName = "Nature Shot", recipe = new () { CardType.Shoot , CardType.Nature}, spritePath = "Game/shoot/leaf_shoot"},
            new (){magicName = "Rock Shot", recipe = new () { CardType.Shoot , CardType.Rock}, spritePath = "Game/shoot/rock_shoot"},
            new (){magicName = "Wind Shot", recipe = new () { CardType.Shoot , CardType.Wind}, spritePath = "Game/shoot/wind_shoot"},
            new (){magicName = "Fire Rune", recipe = new () { CardType.Build , CardType.Fire}, spritePath = "Game/rune/fire_rune"},
            new (){magicName = "Water Rune", recipe = new () { CardType.Build , CardType.Water}, spritePath = "Game/rune/water_rune"},
            new (){magicName = "Lightning Rune", recipe = new () { CardType.Build , CardType.Lightning}, spritePath = "Game/rune/lightning_rune"},
            new (){magicName = "Nature Rune", recipe = new () { CardType.Build , CardType.Nature}, spritePath = "Game/rune/leaf_rune"},
            new (){magicName = "Rock Rune", recipe = new () { CardType.Build , CardType.Rock}, spritePath = "Game/rune/rock_rune"},
            new (){magicName = "Wind Rune", recipe = new () { CardType.Build , CardType.Wind}, spritePath = "Game/rune/wind_rune"},
            new (){magicName = "Fire Explosion", recipe = new () { CardType.Explode , CardType.Fire}, spritePath = "Game/explode/fire_explode"},
            new (){magicName = "Water Explosion", recipe = new () { CardType.Explode , CardType.Water}, spritePath = "Game/explode/water_explode"},
            new (){magicName = "Lightning Explosion", recipe = new () { CardType.Explode , CardType.Lightning}, spritePath = "Game/explode/lightning_explode"},
            new (){magicName = "Nature Explosion", recipe = new () { CardType.Explode , CardType.Nature}, spritePath = "Game/explode/leaf_explode"},
            new (){magicName = "Rock Explosion", recipe = new () { CardType.Explode , CardType.Rock}, spritePath = "Game/explode/rock_explode"},
            new (){magicName = "Wind Explosion", recipe = new () { CardType.Explode , CardType.Wind}, spritePath = "Game/explode/wind_explode"},
            new (){magicName = "Cannon", recipe = new () { CardType.Build , CardType.Shoot, CardType.Rock}, spritePath = "Game/build/ground_cannon"},
            new (){magicName = "Tower", recipe = new () { CardType.Build , CardType.Explode, CardType.Rock}, spritePath = "Game/build/ground_tower"},
            new (){magicName = "Mana Well", recipe = new () { CardType.Build , CardType.Lightning, CardType.Nature}, spritePath = "Game/build/mana_well"},
            new (){magicName = "Aqua Archer", recipe = new () { CardType.Spawn , CardType.Shoot, CardType.Water}, spritePath = "Game/spawn/aqua_archer"},
            new (){magicName = "Rock Golem", recipe = new () { CardType.Spawn , CardType.Rock, CardType.Rock}, spritePath = "Game/spawn/rock_golem"},
            new (){magicName = "Storm Rider", recipe = new () { CardType.Spawn , CardType.Water, CardType.Lightning}, spritePath = "Game/spawn/storm_rider"},
            new (){magicName = "Thunder Spirit", recipe = new () { CardType.Spawn , CardType.Lightning, CardType.Wind}, spritePath = "Game/spawn/thunder_spirit"},
            new (){magicName = "Fire Spirit", recipe = new () { CardType.Spawn , CardType.Wind, CardType.Fire}, spritePath = "Game/spawn/fire_spirit"},
            new (){magicName = "Fire Drop", recipe = new () { CardType.Drop , CardType.Fire}, spritePath = "Game/drop/fire_drop"},
            new (){magicName = "Water Drop", recipe = new () { CardType.Drop , CardType.Water}, spritePath = "Game/drop/water_drop"},
            new (){magicName = "Lightning Drop", recipe = new () { CardType.Drop , CardType.Lightning}, spritePath = "Game/drop/lightning_drop"},
            new (){magicName = "Nature Drop", recipe = new () { CardType.Drop , CardType.Nature}, spritePath = "Game/drop/leaf_drop"},
            new (){magicName = "Rock Drop", recipe = new () { CardType.Drop , CardType.Rock}, spritePath = "Game/drop/rock_drop"},
            new (){magicName = "Wind Drop", recipe = new () { CardType.Drop , CardType.Wind}, spritePath = "Game/drop/wind_drop"},
            new (){magicName = "Magma Spirit", recipe = new () { CardType.Spawn , CardType.Explode, CardType.Fire, CardType.Fire, CardType.Rock}, spritePath = "Game/spawn/magma_spirit"},
            new (){magicName = "Healing Totem", recipe = new () { CardType.Build , CardType.Nature, CardType.Water}, spritePath = "Game/build/healing_totem"},
            new (){magicName = "Sand Storm", recipe = new () { CardType.Explode , CardType.Rock, CardType.Wind}, spritePath = "Game/explode/sand_storm"},
            new (){magicName = "Chain Lightning", recipe = new () { CardType.Shoot , CardType.Lightning, CardType.Lightning, CardType.Lightning}, spritePath = "Game/shoot/chain_lightning"},
            new (){magicName = "Tornado Strike", recipe = new () { CardType.Spawn , CardType.Wind, CardType.Wind, CardType.Wind, CardType.Nature}, spritePath = "Game/spawn/tornado_strike"},
            new (){magicName = "Meteor Shower", recipe = new () { CardType.Drop , CardType.Fire, CardType.Fire, CardType.Rock, CardType.Rock}, spritePath = "Game/drop/meteor_shower"},
            new (){magicName = "Tide Call", recipe = new () { CardType.Shoot , CardType.Water, CardType.Nature}, spritePath = "Game/shoot/tide_call"},
             // 3차 상위 마법
            new (){magicName = "Cloud Dragon", recipe = new () { CardType.Spawn , CardType.Water, CardType.Wind}, spritePath = "Game/spawn/cloud_dragon"},
            new (){magicName = "Thunder Bird Swarm", recipe = new () { CardType.Spawn , CardType.Drop, CardType.Lightning}, spritePath = "Game/spawn/thunder_bird"},
            new (){magicName = "Tree Golem", recipe = new () { CardType.Spawn , CardType.Nature, CardType.Nature}, spritePath = "Game/spawn/tree_golem"},
            new (){magicName = "Vine Spirit", recipe = new () { CardType.Spawn , CardType.Shoot, CardType.Nature}, spritePath = "Game/spawn/vine_spirit"},
            
            new (){magicName = "Vine Colony", recipe = new () { CardType.Build , CardType.Nature, CardType.Nature}, spritePath = "Game/build/vine_colony"},
        };
        
        public static CombinedMagicData GetCombinedMagicData(string name)
        {
            return dataList.Find(x => x.magicName == name);
        }


        public static bool TryGetByRecipe(IList<CardType> recipe, out CombinedMagicData match)
            => CombinedMagicResolver.TryResolve(recipe, out match);
    }
}