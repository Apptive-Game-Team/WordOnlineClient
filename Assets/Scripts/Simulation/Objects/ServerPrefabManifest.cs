using System.Collections.Generic;

namespace GameScene.Simulation.Objects
{
    public static class ServerPrefabManifest
    {
        // Mirrors Game server PrefabType. Order is part of deterministic catalog contract.
        public static IReadOnlyList<string> All { get; } = new[]
        {
            "FireShot", "FireExplode", "FireField", "FireSlime", "FireSummon",
            "WaterShot", "WaterExplode", "WaterField", "WaterSlime",
            "RockRolling", "RockExplode", "RockSlime", "RockSummon",
            "ElectricShot", "ElectricExplode", "ElectricSlime", "ElectricSummon", "ElectricField",
            "VineToss", "LeafExplode", "LeafField", "LeafSlime",
            "WindBlade", "WindExplode", "WindSlime", "WindSummon",
            "GroundCannon", "GroundTower", "ManaWell", "AquaArcher", "RockGolem",
            "StormRider", "FireSpirit", "ThunderSpirit", "MagmaSpirit", "HealingTotem",
            "SandStorm", "ChainLightning", "TornadoStrike", "MeteorShower", "TideCall",
            "FireDrop", "NatureDrop", "RockDrop", "LightningDrop", "WindDrop", "RallyingTotem",
            "FireRune", "WaterRune", "NatureRune", "RockRune", "LightningRune", "WindRune",
            "TreeGolem", "VineSpirit", "ThunderBird", "CloudDragon", "RockMage", "VineColony",
            "Vine", "MagmaFist", "MeteorDrop", "CraterEmber",
            "PveNatureSlimeNest", "PveWaterSlimeNest", "PveVineColony", "PveVineWitch",
            "EmberSpirit", "SeedSpirit", "ZapMouse", "WindSpirit", "MiniRock",
            "LifeTree", "RockTurret", "WindTotem", "MagmaExplosion", "WaterExplosion",
            "RazorGale", "ShockOverload", "WillOWisp", "FrenzyTotem", "Leafair",
            "ChickenCommando", "Overgrowth", "Crater", "BubbleGenerator", "ElectricTower",
            "FireLordSpirit", "BubbleSpirit", "Towerback", "FireChildSpirit", "DimensionToad",
            "FireTadpole", "LightningTadpole", "Wall", "Player"
        };
    }
}
