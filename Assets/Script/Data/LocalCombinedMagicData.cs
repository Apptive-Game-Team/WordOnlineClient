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
            new (){magicName = "Wind Slime Swarm", recipe = new () { CardType.Spawn , CardType.Rock}, spritePath = "Game/spawn/wind_slime"},
            new (){magicName = "Fire Shot", recipe = new () { CardType.Shoot , CardType.Fire}, spritePath = "Game/shoot/fire_shoot"},
            new (){magicName = "Water Shot", recipe = new () { CardType.Shoot , CardType.Water}, spritePath = "Game/shoot/water_shoot"},
            new (){magicName = "Lightning Shot", recipe = new () { CardType.Shoot , CardType.Lightning}, spritePath = "Game/shoot/lightning_shoot"},
            new (){magicName = "Nature Shot", recipe = new () { CardType.Shoot , CardType.Nature}, spritePath = "Game/shoot/leaf_shoot"},
            new (){magicName = "Rock Shot", recipe = new () { CardType.Shoot , CardType.Rock}, spritePath = "Game/shoot/rock_shoot"},
            new (){magicName = "Wind Shot", recipe = new () { CardType.Shoot , CardType.Wind}, spritePath = "Game/shoot/wind_shoot"},
            new (){magicName = "Fire Slime Nest", recipe = new () { CardType.Build , CardType.Fire}, spritePath = "Game/build/fire_slime_nest"},
            new (){magicName = "Water Slime Nest", recipe = new () { CardType.Build , CardType.Water}, spritePath = "Game/build/water_slime_nest"},
            new (){magicName = "Lightning Slime Nest", recipe = new () { CardType.Build , CardType.Lightning}, spritePath = "Game/build/lightning_slime_nest"},
            new (){magicName = "Nature Slime Nest", recipe = new () { CardType.Build , CardType.Nature}, spritePath = "Game/build/leaf_slime_nest"},
            new (){magicName = "Rock Slime Nest", recipe = new () { CardType.Build , CardType.Rock}, spritePath = "Game/build/rock_slime_nest"},
            new (){magicName = "Wind Slime Nest", recipe = new () { CardType.Build , CardType.Wind}, spritePath = "Game/build/wind_slime_nest"},
            new (){magicName = "Fire Explosion", recipe = new () { CardType.Build , CardType.Fire}, spritePath = "Game/explode/fire_explode"},
            new (){magicName = "Water Explosion", recipe = new () { CardType.Build , CardType.Water}, spritePath = "Game/explode/water_explode"},
            new (){magicName = "Lightning Explosion", recipe = new () { CardType.Build , CardType.Lightning}, spritePath = "Game/explode/lightning_explode"},
            new (){magicName = "Nature Explosion", recipe = new () { CardType.Build , CardType.Nature}, spritePath = "Game/explode/leaf_explode"},
            new (){magicName = "Rock Explosion", recipe = new () { CardType.Build , CardType.Rock}, spritePath = "Game/explode/rock_explode"},
            new (){magicName = "Wind Explosion", recipe = new () { CardType.Build , CardType.Wind}, spritePath = "Game/explode/wind_explode"},
            new (){magicName = "Cannon", recipe = new () { CardType.Build , CardType.Shoot, CardType.Rock}, spritePath = "Game/build/ground_cannon"},
            new (){magicName = "Tower", recipe = new () { CardType.Build , CardType.Explode, CardType.Rock}, spritePath = "Game/build/anti_air_turret"},
            new (){magicName = "Mana Well", recipe = new () { CardType.Build , CardType.Lightning, CardType.Nature}, spritePath = "Game/build/mana_well"},
            new (){magicName = "Aqua Archer", recipe = new () { CardType.Spawn , CardType.Shoot, CardType.Water}, spritePath = "Game/spawn/aqua_archer"},
            new (){magicName = "Rock Golem", recipe = new () { CardType.Spawn , CardType.Rock, CardType.Rock}, spritePath = "Game/spawn/rock_golem"},
            new (){magicName = "Storm Rider", recipe = new () { CardType.Spawn , CardType.Water, CardType.Lightning}, spritePath = "Game/spawn/storm_rider"},
            new (){magicName = "Thunder Spirit", recipe = new () { CardType.Spawn , CardType.Lightning, CardType.Wind}, spritePath = "Game/spawn/thunder_spirit"},
            new (){magicName = "Fire Spirit", recipe = new () { CardType.Spawn , CardType.Wind, CardType.Fire}, spritePath = "Game/spawn/fire_spirit"},
        };
        
        public static CombinedMagicData GetCombinedMagicData(string name)
        {
            return dataList.Find(x => x.magicName == name);
        }
    }
}