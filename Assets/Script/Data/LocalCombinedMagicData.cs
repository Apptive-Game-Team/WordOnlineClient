using System.Collections.Generic;
using UnityEngine;

namespace Script.Data
{
    public class CombinedMagicData
    {
        public string magicName;
        public List<CardType> recipe;
        public string spriteName;
        
        public Sprite GetSprite()
        {
            return Resources.Load<Sprite>(spriteName);
        }
    }
    
    public static class LocalCombinedMagicData
    {
        public static List<CombinedMagicData> dataList = new()
        {
            new (){magicName = "Fire Slime Swarm", recipe = new () { CardType.Spawn , CardType.Fire}, spriteName = "Game/slime/fire_slime"},
            new (){magicName = "Water Slime Swarm", recipe = new () { CardType.Spawn , CardType.Water}, spriteName = "Game/slime/water_slime"},
            new (){magicName = "Lightning Slime Swarm", recipe = new () { CardType.Spawn , CardType.Lightning}, spriteName = "Game/slime/lightning_slime"},
            new (){magicName = "Nature Slime Swarm", recipe = new () { CardType.Spawn , CardType.Nature}, spriteName = "Game/slime/nature_slime"},
            new (){magicName = "Rock Slime Swarm", recipe = new () { CardType.Spawn , CardType.Rock}, spriteName = "Game/slime/rock_slime"},
            new (){magicName = "Wind Slime Swarm", recipe = new () { CardType.Spawn , CardType.Rock}, spriteName = "Game/slime/wind_slime"},
            new (){magicName = "Fire Shot", recipe = new () { CardType.Shoot , CardType.Fire}, spriteName = "Game/shoot/fire_shoot"},
            new (){magicName = "Water Shot", recipe = new () { CardType.Shoot , CardType.Water}, spriteName = "Game/shoot/water_shoot"},
            new (){magicName = "Lightning Shot", recipe = new () { CardType.Shoot , CardType.Lightning}, spriteName = "Game/shoot/lightning_shoot"},
            new (){magicName = "Nature Shot", recipe = new () { CardType.Shoot , CardType.Nature}, spriteName = "Game/shoot/nature_shoot"},
            new (){magicName = "Rock Shot", recipe = new () { CardType.Shoot , CardType.Rock}, spriteName = "Game/shoot/rock_shoot"},
            new (){magicName = "Wind Shot", recipe = new () { CardType.Shoot , CardType.Wind}, spriteName = "Game/shoot/wind_shoot"},
            new (){magicName = "Fire Slime Nest", recipe = new () { CardType.Build , CardType.Fire}, spriteName = "Game/slime_nest/fire_slime_nest"},
            new (){magicName = "Water Slime Nest", recipe = new () { CardType.Build , CardType.Water}, spriteName = "Game/slime_nest/water_slime_nest"},
            new (){magicName = "Lightning Slime Nest", recipe = new () { CardType.Build , CardType.Lightning}, spriteName = "Game/slime_nest/lightning_slime_nest"},
            new (){magicName = "Nature Slime Nest", recipe = new () { CardType.Build , CardType.Nature}, spriteName = "Game/slime_nest/nature_slime_nest"},
            new (){magicName = "Rock Slime Nest", recipe = new () { CardType.Build , CardType.Rock}, spriteName = "Game/slime_nest/rock_slime_nest"},
            new (){magicName = "Wind Slime Nest", recipe = new () { CardType.Build , CardType.Wind}, spriteName = "Game/slime_nest/wind_slime_nest"},
            new (){magicName = "Fire Explosion", recipe = new () { CardType.Build , CardType.Fire}, spriteName = "Game/explode/fire_explode"},
            new (){magicName = "Water Explosion", recipe = new () { CardType.Build , CardType.Water}, spriteName = "Game/explode/water_explode"},
            new (){magicName = "Lightning Explosion", recipe = new () { CardType.Build , CardType.Lightning}, spriteName = "Game/explode/lightning_explode"},
            new (){magicName = "Nature Explosion", recipe = new () { CardType.Build , CardType.Nature}, spriteName = "Game/explode/nature_explode"},
            new (){magicName = "Rock Explosion", recipe = new () { CardType.Build , CardType.Rock}, spriteName = "Game/explode/rock_explode"},
            new (){magicName = "Wind Explosion", recipe = new () { CardType.Build , CardType.Wind}, spriteName = "Game/explode/wind_explode"},
            new (){magicName = "Cannon", recipe = new () { CardType.Build , CardType.Shoot, CardType.Rock}, spriteName = "Game/build/ground_cannon"},
            new (){magicName = "Tower", recipe = new () { CardType.Build , CardType.Explode, CardType.Rock}, spriteName = "Game/build/anti_air_turret"},
            new (){magicName = "Mana Well", recipe = new () { CardType.Build , CardType.Lightning, CardType.Nature}, spriteName = "Game/build/mana_well"},
            new (){magicName = "Aqua Archer", recipe = new () { CardType.Spawn , CardType.Shoot, CardType.Water}, spriteName = "Game/spawn/aqua_archer"},
            new (){magicName = "Rock Golem", recipe = new () { CardType.Spawn , CardType.Rock, CardType.Rock}, spriteName = "Game/spawn/rock_golem"},
            new (){magicName = "Storm Rider", recipe = new () { CardType.Spawn , CardType.Water, CardType.Lightning}, spriteName = "Game/spawn/storm_rider"},
            new (){magicName = "Thunder Spirit", recipe = new () { CardType.Spawn , CardType.Lightning, CardType.Wind}, spriteName = "Game/spawn/thunder_spirit"},
            new (){magicName = "Fire Spirit", recipe = new () { CardType.Spawn , CardType.Wind, CardType.Fire}, spriteName = "Game/spawn/fire_spirit"},
        };
        
        public static CombinedMagicData GetCombinedMagicData(string name)
        {
            return dataList.Find(x => x.magicName == name);
        }
    }
}