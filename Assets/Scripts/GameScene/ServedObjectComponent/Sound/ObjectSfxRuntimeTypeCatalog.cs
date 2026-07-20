using System.Collections.Generic;

namespace GameScene.ServedObjectComponent.Sound
{
    public enum RuntimeSfxArchetype
    {
        Silent,
        Creature,
        Building,
        Transient
    }

    public enum RuntimeSfxElement
    {
        Neutral,
        Fire,
        Water,
        Nature,
        Lightning,
        Rock,
        Wind
    }

    public static class ObjectSfxRuntimeTypeCatalog
    {
        private static readonly HashSet<string> Buildings = new()
        {
            "BubbleGenerator", "Crater", "ElectricTower", "FrenzyTotem", "GroundCannon",
            "GroundTower", "HealingTotem", "LifeTree", "ManaWell", "PveNatureSlimeNest",
            "PveVineColony", "PveWaterSlimeNest", "RallyingTorch", "RockTurret", "Vine",
            "VineColony", "WindTotem", "FireRune", "LightningRune", "NatureRune", "RockRune",
            "WaterRune", "WindRune"
        };

        private static readonly HashSet<string> Transients = new()
        {
            "ChainLightning", "CraterEmber", "ElectricExplode", "ElectricField", "ElectricShot",
            "FireDrop", "FireExplode", "FireField", "FireShot", "LeafExplode", "LeafField",
            "LeafShot", "Leafair", "LightningDrop", "MagmaExplosion", "MagmaFist", "MeteorDrop",
            "MeteorShower", "MiniRock", "NatureDrop", "Overgrowth", "RainCloud", "RazorGale",
            "RockDrop", "RockExplode", "RockRolling", "SandStorm", "ShockOverload", "TideCall",
            "TornadoStrike", "WaterExplode", "WaterExplosion", "WaterField", "WaterShot",
            "WindBlade", "WindDrop", "WindExplode"
        };

        private static readonly HashSet<string> Silent = new()
        {
            "ServedObjectHpBar", "Towerback"
        };

        private static readonly HashSet<string> Creatures = new()
        {
            "AquaArcher", "BubbleSpirit", "ChickenCommando", "CloudDragon", "DimensionToad",
            "ElectricSlime", "EmberSpirit", "FireChildSpirit", "FireLordSpirit", "FireSlime",
            "FireSpirit", "FireTadpole", "LeafSlime", "LightningTadpole", "MagmaSpirit", "Player",
            "PveVineWitch", "RockGolem", "RockMage", "RockSlime", "SeedSpirit", "StormRider",
            "ThunderBird", "ThunderSpirit", "TreeGolem", "VineSpirit", "WaterSlime", "WillOWisp",
            "WindSlime", "WindSpirit", "ZapMouse"
        };

        private static readonly HashSet<string> Fire = new()
        {
            "CraterEmber", "EmberSpirit", "FireChildSpirit", "FireDrop", "FireExplode", "FireField",
            "FireLordSpirit", "FireRune", "FireShot", "FireSlime", "FireSpirit", "FireTadpole",
            "MagmaExplosion", "MagmaFist", "MagmaSpirit", "MeteorDrop", "MeteorShower", "RallyingTorch"
        };

        private static readonly HashSet<string> Water = new()
        {
            "AquaArcher", "BubbleGenerator", "BubbleSpirit", "PveWaterSlimeNest", "RainCloud", "TideCall",
            "WaterExplode", "WaterExplosion", "WaterField", "WaterRune", "WaterShot", "WaterSlime"
        };

        private static readonly HashSet<string> Nature = new()
        {
            "LeafExplode", "LeafField", "LeafShot", "LeafSlime", "Leafair", "LifeTree", "NatureDrop",
            "NatureRune", "Overgrowth", "PveNatureSlimeNest", "PveVineColony", "PveVineWitch", "SeedSpirit",
            "TreeGolem", "Vine", "VineColony", "VineSpirit", "WillOWisp"
        };

        private static readonly HashSet<string> Lightning = new()
        {
            "ChainLightning", "ElectricExplode", "ElectricField", "ElectricShot", "ElectricSlime",
            "ElectricTower", "LightningDrop", "LightningRune", "LightningTadpole", "ShockOverload",
            "StormRider", "ThunderBird", "ThunderSpirit", "ZapMouse"
        };

        private static readonly HashSet<string> Rock = new()
        {
            "Crater", "DimensionToad", "GroundCannon", "GroundTower", "MiniRock", "RockDrop", "RockExplode",
            "RockGolem", "RockMage", "RockRolling", "RockRune", "RockSlime", "RockTurret"
        };

        private static readonly HashSet<string> Wind = new()
        {
            "CloudDragon", "RazorGale", "SandStorm", "TornadoStrike", "WindBlade", "WindDrop", "WindExplode",
            "WindRune", "WindSlime", "WindSpirit", "WindTotem"
        };

        public static RuntimeSfxArchetype Resolve(string runtimeType)
        {
            if (string.IsNullOrEmpty(runtimeType) || Silent.Contains(runtimeType))
            {
                return RuntimeSfxArchetype.Silent;
            }

            if (Buildings.Contains(runtimeType))
            {
                return RuntimeSfxArchetype.Building;
            }

            if (Transients.Contains(runtimeType))
            {
                return RuntimeSfxArchetype.Transient;
            }

            return Creatures.Contains(runtimeType)
                ? RuntimeSfxArchetype.Creature
                : RuntimeSfxArchetype.Silent;
        }

        public static RuntimeSfxElement ResolveElement(string runtimeType)
        {
            if (Fire.Contains(runtimeType)) return RuntimeSfxElement.Fire;
            if (Water.Contains(runtimeType)) return RuntimeSfxElement.Water;
            if (Nature.Contains(runtimeType)) return RuntimeSfxElement.Nature;
            if (Lightning.Contains(runtimeType)) return RuntimeSfxElement.Lightning;
            if (Rock.Contains(runtimeType)) return RuntimeSfxElement.Rock;
            if (Wind.Contains(runtimeType)) return RuntimeSfxElement.Wind;
            return RuntimeSfxElement.Neutral;
        }
    }
}
