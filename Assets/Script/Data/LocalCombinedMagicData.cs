using System.Collections.Generic;

namespace Script.Data
{
    public class CombinedMagicData
    {
        public string magicName;
        public List<CardType> recipe;
        public string spriteName;
    }
    
    public static class LocalCombinedMagicData
    {
        public static List<CombinedMagicData> dataList = new()
        {
            new (){magicName = "Fire Slime Swarm", recipe = new () { CardType.Spawn , CardType.Fire}, spriteName = "FireSlime"},
            new (){magicName = "Water Slime Swarm", recipe = new () { CardType.Spawn , CardType.Water}, spriteName = "WaterSlime"},
            new (){magicName = "Lightning Slime Swarm", recipe = new () { CardType.Spawn , CardType.Lightning}, spriteName = "LightningSlime"},
            new (){magicName = "Nature Slime Swarm", recipe = new () { CardType.Spawn , CardType.Nature}, spriteName = "NatureSlime"},
            new (){magicName = "Rock Slime Swarm", recipe = new () { CardType.Spawn , CardType.Rock}, spriteName = "RockSlime"},
            new (){magicName = "Wind Slime Swarm", recipe = new () { CardType.Spawn , CardType.Rock}, spriteName = "WindSlime"},
            new (){magicName = "Fire Shot", recipe = new () { CardType.Shoot , CardType.Fire}, spriteName = "FireShot"},
            new (){magicName = "Water Shot", recipe = new () { CardType.Shoot , CardType.Water}, spriteName = "WaterShot"},
            new (){magicName = "Lightning Shot", recipe = new () { CardType.Shoot , CardType.Lightning}, spriteName = "LightningShot"},
            new (){magicName = "Nature Shot", recipe = new () { CardType.Shoot , CardType.Nature}, spriteName = "NatureShot"},
            new (){magicName = "Rock Shot", recipe = new () { CardType.Shoot , CardType.Rock}, spriteName = "RockShot"},
            new (){magicName = "Wind Shot", recipe = new () { CardType.Shoot , CardType.Wind}, spriteName = "WindShot"},
            new (){magicName = "Fire Slime Nest", recipe = new () { CardType.Build , CardType.Fire}, spriteName = "FireSlimeNest"},
            new (){magicName = "Water Slime Nest", recipe = new () { CardType.Build , CardType.Water}, spriteName = "WaterSlimeNest"},
            new (){magicName = "Lightning Slime Nest", recipe = new () { CardType.Build , CardType.Lightning}, spriteName = "LightningSlimeNest"},
            new (){magicName = "Nature Slime Nest", recipe = new () { CardType.Build , CardType.Nature}, spriteName = "NatureSlimeNest"},
            new (){magicName = "Rock Slime Nest", recipe = new () { CardType.Build , CardType.Rock}, spriteName = "RockSlimeNest"},
            new (){magicName = "Wind Slime Nest", recipe = new () { CardType.Build , CardType.Wind}, spriteName = "WindSlimeNest"},
            new (){magicName = "Fire Explosion", recipe = new () { CardType.Build , CardType.Fire}, spriteName = "FireExplode"},
            new (){magicName = "Water Explosion", recipe = new () { CardType.Build , CardType.Water}, spriteName = "WaterExplode"},
            new (){magicName = "Lightning Explosion", recipe = new () { CardType.Build , CardType.Lightning}, spriteName = "LightningExplode"},
            new (){magicName = "Nature Explosion", recipe = new () { CardType.Build , CardType.Nature}, spriteName = "NatureExplode"},
            new (){magicName = "Rock Explosion", recipe = new () { CardType.Build , CardType.Rock}, spriteName = "RockExplode"},
            new (){magicName = "Wind Explosion", recipe = new () { CardType.Build , CardType.Wind}, spriteName = "WindExplode"},
            new (){magicName = "Cannon", recipe = new () { CardType.Build , CardType.Shoot, CardType.Rock}, spriteName = "Cannon"},
            new (){magicName = "Tower", recipe = new () { CardType.Build , CardType.Explode, CardType.Rock}, spriteName = "Tower"},
            new (){magicName = "Mana Well", recipe = new () { CardType.Build , CardType.Lightning, CardType.Nature}, spriteName = "ManaWell"},
            new (){magicName = "Aqua Archer", recipe = new () { CardType.Spawn , CardType.Shoot, CardType.Water}, spriteName = "AquaArcher"},
            new (){magicName = "Rock Golem", recipe = new () { CardType.Spawn , CardType.Rock, CardType.Rock}, spriteName = "RockGolem"},
            new (){magicName = "Storm Rider", recipe = new () { CardType.Spawn , CardType.Water, CardType.Lightning}, spriteName = "StormRider"},
            new (){magicName = "Thunder Spirit", recipe = new () { CardType.Spawn , CardType.Lightning, CardType.Wind}, spriteName = "ThunderSpirit"},
            new (){magicName = "Fire Spirit", recipe = new () { CardType.Spawn , CardType.Wind, CardType.Fire}, spriteName = "FireSpirit"},
        };
        
        public static CombinedMagicData GetCombinedMagicData(string name)
        {
            return dataList.Find(x => x.magicName == name);
        }
    }
}