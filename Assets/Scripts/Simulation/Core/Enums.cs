namespace Simulation.Core
{
    public enum Master { LeftPlayer, RightPlayer, None }

    public enum SimStatus { Initializing, Idle, Move, Attack, Destroyed, Damaged, Hindered }

    public enum Effect { None, Burn, Wet, Shock, Snared, Knockback, LeafFieldHeal, Sandstorm }

    public enum ElementType { NONE, FIRE, WATER, NATURE, LIGHTNING, ROCK, WIND }

    public enum PrefabType
    {
        Player, Wall,
        // Single-element spawns
        FireSlime, WaterSlime, LeafSlime, RockSlime, ElectricSlime, WindSlime,
        // Shots
        FireShot, WaterShot, ElectricShot, RockRolling, VineToss, WindBlade,
        // Explodes
        FireExplode, WaterExplode, LeafExplode, RockExplode, ElectricExplode, WindExplode,
        // Drops
        FireDrop, RainCloud, NatureDrop, RockDrop, LightningDrop, WindDrop,
        // Fields
        LeafField, WaterField, FireField, ElectricField,
        // Builds
        LifeTree, RockTurret, WindTotem,
        // Advanced spawns (Summon = slime nests)
        FireSummon, RockSummon, ElectricSummon, WindSummon,
        AquaArcher, RockGolem, StormRider, ThunderSpirit, FireSpirit,
        // Runes
        FireRune, WaterRune, NatureRune, RockRune, LightningRune, WindRune,
        // 3rd tier
        TreeGolem, VineSpirit, ThunderBird, CloudDragon,
        MagmaSpirit, HealingTotem, SandStorm, TornadoStrike, MeteorDrop,
        // Special
        GroundCannon, GroundTower, ManaWell,
        RockMage, VineColony, Vine, MagmaFist,
        WindSpirit,
        // 2-card swarm spawns (spawns multiple smaller units)
        EmberSpirit, SeedSpirit, MiniRock,
        // Multi-element shots
        ChainLightning, TideCall,
        // Multi-element explodes
        MagmaExplosion, WaterExplosion,
        // Multi-element drop
        MeteorShower,
        // PvE
        PveNatureSlimeNest, PveWaterSlimeNest, PveVineColony, PveVineWitch,
    }
}
