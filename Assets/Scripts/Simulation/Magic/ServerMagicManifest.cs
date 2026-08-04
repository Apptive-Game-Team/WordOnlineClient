using System;
using System.Collections.Generic;

namespace GameScene.Simulation.Magic
{
    public sealed class ServerMagicDescriptor
    {
        public int StableId { get; }
        public string Name { get; }
        public MagicBehaviorFamily Family { get; }
        public string RootPrefabId { get; }
        public MagicExecutionKind ExecutionKind { get; }

        public ServerMagicDescriptor(int stableId, string name, MagicBehaviorFamily family, string rootPrefabId,
            MagicExecutionKind? executionKind = null)
        {
            StableId = stableId;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Family = family;
            RootPrefabId = rootPrefabId ?? throw new ArgumentNullException(nameof(rootPrefabId));
            ExecutionKind = executionKind ?? DefaultExecution(name, family);
        }


        private static MagicExecutionKind DefaultExecution(string name, MagicBehaviorFamily family)
        {
            if (name == "vine_toss") return MagicExecutionKind.VineLine;
            if (name == "vine_fan") return MagicExecutionKind.VineFan;
            if (name == "lightning_drop") return MagicExecutionKind.LightningColumn;
            if (name == "frenzy_totem") return MagicExecutionKind.FrenzyDrop;
            if (name == "rallying_torch") return MagicExecutionKind.RallyingTorch;
            if (name == "leafair") return MagicExecutionKind.LeafairDrop;
            if (name == "meteor_shower") return MagicExecutionKind.MeteorShower;
            if (name == "chicken_commando") return MagicExecutionKind.AirSpawn;
            if (name == "overgrowth") return MagicExecutionKind.Overgrowth;
            if (name == "razor_gale") return MagicExecutionKind.PeriodicArea;
            if (name == "sand_storm") return MagicExecutionKind.PersistentArea;
            if (name == "tornado_strike") return MagicExecutionKind.MovingTornado;
            if (name == "chain_lightning") return MagicExecutionKind.ChainShot;
            if (name == "rock_rolling") return MagicExecutionKind.RollingRock;
            if (name == "tide_call") return MagicExecutionKind.PushShot;
            if (name == "will_o_wisp") return MagicExecutionKind.MindControlShot;
            if (name == "wind_blade") return MagicExecutionKind.PiercingShot;
            if (family == MagicBehaviorFamily.Spawn && name.Contains("swarm"))
                return MagicExecutionKind.SwarmSpawn;
            switch (family)
            {
                case MagicBehaviorFamily.Spawn: return MagicExecutionKind.SingleSpawn;
                case MagicBehaviorFamily.Projectile: return MagicExecutionKind.Shot;
                case MagicBehaviorFamily.Area: return MagicExecutionKind.Explosion;
                case MagicBehaviorFamily.Build: return MagicExecutionKind.Build;
                case MagicBehaviorFamily.Drop: return MagicExecutionKind.DropImpact;
                default: throw new ArgumentOutOfRangeException(nameof(family));
            }
        }
    }

    public static class ServerMagicManifest
    {
        public static IReadOnlyList<ServerMagicDescriptor> All { get; } = new[]
        {
            new ServerMagicDescriptor(1, "aqua_archer", MagicBehaviorFamily.Spawn, "AquaArcher"),
            new ServerMagicDescriptor(2, "bubble_generator", MagicBehaviorFamily.Build, "BubbleGenerator"),
            new ServerMagicDescriptor(3, "bubble_spirit", MagicBehaviorFamily.Spawn, "BubbleSpirit"),
            new ServerMagicDescriptor(4, "cannon", MagicBehaviorFamily.Build, "GroundCannon"),
            new ServerMagicDescriptor(5, "chain_lightning", MagicBehaviorFamily.Projectile, "ChainLightning"),
            new ServerMagicDescriptor(6, "chicken_commando", MagicBehaviorFamily.Drop, "ChickenCommando"),
            new ServerMagicDescriptor(7, "cloud_dragon", MagicBehaviorFamily.Spawn, "CloudDragon"),
            new ServerMagicDescriptor(8, "crater", MagicBehaviorFamily.Build, "Crater"),
            new ServerMagicDescriptor(9, "dimension_toad", MagicBehaviorFamily.Spawn, "DimensionToad"),
            new ServerMagicDescriptor(10, "electric_tower", MagicBehaviorFamily.Build, "ElectricTower"),
            new ServerMagicDescriptor(11, "ember_spirit_swarm", MagicBehaviorFamily.Spawn, "EmberSpirit"),
            new ServerMagicDescriptor(12, "fire_drop", MagicBehaviorFamily.Drop, "FireDrop"),
            new ServerMagicDescriptor(13, "fire_lord_spirit", MagicBehaviorFamily.Spawn, "FireLordSpirit"),
            new ServerMagicDescriptor(14, "fire_shot", MagicBehaviorFamily.Projectile, "FireShot"),
            new ServerMagicDescriptor(15, "fire_slime_nest", MagicBehaviorFamily.Build, "FireSummon"),
            new ServerMagicDescriptor(16, "fire_spirit", MagicBehaviorFamily.Spawn, "FireSpirit"),
            new ServerMagicDescriptor(17, "frenzy_totem", MagicBehaviorFamily.Drop, "FrenzyTotem"),
            new ServerMagicDescriptor(18, "healing_totem", MagicBehaviorFamily.Build, "HealingTotem"),
            new ServerMagicDescriptor(19, "leafair", MagicBehaviorFamily.Drop, "Leafair"),
            new ServerMagicDescriptor(20, "life_tree", MagicBehaviorFamily.Build, "LifeTree"),
            new ServerMagicDescriptor(21, "lightning_drop", MagicBehaviorFamily.Drop, "LightningDrop"),
            new ServerMagicDescriptor(22, "lightning_explosion", MagicBehaviorFamily.Area, "ElectricExplode"),
            new ServerMagicDescriptor(23, "lightning_shot", MagicBehaviorFamily.Projectile, "ElectricShot"),
            new ServerMagicDescriptor(24, "lightning_slime_nest", MagicBehaviorFamily.Build, "ElectricSummon"),
            new ServerMagicDescriptor(25, "magma_explosion", MagicBehaviorFamily.Area, "MagmaExplosion"),
            new ServerMagicDescriptor(26, "magma_spirit", MagicBehaviorFamily.Spawn, "MagmaSpirit"),
            new ServerMagicDescriptor(27, "mana_well", MagicBehaviorFamily.Build, "ManaWell"),
            new ServerMagicDescriptor(28, "meteor_shower", MagicBehaviorFamily.Drop, "MeteorShower"),
            new ServerMagicDescriptor(29, "mini_rock_swarm", MagicBehaviorFamily.Spawn, "MiniRock"),
            new ServerMagicDescriptor(30, "nature_drop", MagicBehaviorFamily.Drop, "NatureDrop"),
            new ServerMagicDescriptor(31, "nature_slime_nest", MagicBehaviorFamily.Build, "PveNatureSlimeNest"),
            new ServerMagicDescriptor(32, "overgrowth", MagicBehaviorFamily.Area, "Overgrowth"),
            new ServerMagicDescriptor(33, "pve_water_slime_nest", MagicBehaviorFamily.Build, "PveWaterSlimeNest"),
            new ServerMagicDescriptor(34, "rallying_torch", MagicBehaviorFamily.Drop, "RallyingTorch"),
            new ServerMagicDescriptor(35, "razor_gale", MagicBehaviorFamily.Area, "RazorGale"),
            new ServerMagicDescriptor(36, "rock_drop", MagicBehaviorFamily.Drop, "RockDrop"),
            new ServerMagicDescriptor(37, "rock_golem", MagicBehaviorFamily.Spawn, "RockGolem"),
            new ServerMagicDescriptor(38, "rock_mage", MagicBehaviorFamily.Spawn, "RockMage"),
            new ServerMagicDescriptor(39, "rock_rolling", MagicBehaviorFamily.Projectile, "RockRolling"),
            new ServerMagicDescriptor(40, "rock_slime_nest", MagicBehaviorFamily.Build, "RockSummon"),
            new ServerMagicDescriptor(41, "rock_turret", MagicBehaviorFamily.Build, "RockTurret"),
            new ServerMagicDescriptor(42, "sand_storm", MagicBehaviorFamily.Area, "SandStorm"),
            new ServerMagicDescriptor(43, "seed_spirit_swarm", MagicBehaviorFamily.Spawn, "SeedSpirit"),
            new ServerMagicDescriptor(44, "shock_overload", MagicBehaviorFamily.Area, "ShockOverload"),
            new ServerMagicDescriptor(45, "storm_rider", MagicBehaviorFamily.Spawn, "StormRider"),
            new ServerMagicDescriptor(46, "thunder_bird_swarm", MagicBehaviorFamily.Spawn, "ThunderBird"),
            new ServerMagicDescriptor(47, "thunder_spirit", MagicBehaviorFamily.Spawn, "ThunderSpirit"),
            new ServerMagicDescriptor(48, "tide_call", MagicBehaviorFamily.Projectile, "TideCall"),
            new ServerMagicDescriptor(49, "tornado_strike", MagicBehaviorFamily.Spawn, "TornadoStrike"),
            new ServerMagicDescriptor(50, "tower", MagicBehaviorFamily.Build, "GroundTower"),
            new ServerMagicDescriptor(51, "towerback", MagicBehaviorFamily.Build, "Towerback"),
            new ServerMagicDescriptor(52, "tree_golem", MagicBehaviorFamily.Spawn, "TreeGolem"),
            new ServerMagicDescriptor(53, "vine_colony", MagicBehaviorFamily.Build, "VineColony"),
            new ServerMagicDescriptor(54, "vine_fan", MagicBehaviorFamily.Projectile, "Vine"),
            new ServerMagicDescriptor(55, "vine_spirit", MagicBehaviorFamily.Spawn, "VineSpirit"),
            new ServerMagicDescriptor(56, "vine_toss", MagicBehaviorFamily.Projectile, "Vine"),
            new ServerMagicDescriptor(57, "water_explosion", MagicBehaviorFamily.Area, "WaterExplosion"),
            new ServerMagicDescriptor(58, "water_shot", MagicBehaviorFamily.Projectile, "WaterShot"),
            new ServerMagicDescriptor(59, "water_slime_swarm", MagicBehaviorFamily.Spawn, "WaterSlime"),
            new ServerMagicDescriptor(60, "will_o_wisp", MagicBehaviorFamily.Projectile, "WillOWisp"),
            new ServerMagicDescriptor(61, "wind_blade", MagicBehaviorFamily.Projectile, "WindBlade"),
            new ServerMagicDescriptor(62, "wind_drop", MagicBehaviorFamily.Drop, "WindDrop"),
            new ServerMagicDescriptor(63, "wind_explosion", MagicBehaviorFamily.Area, "WindExplode"),
            new ServerMagicDescriptor(64, "wind_slime_nest", MagicBehaviorFamily.Build, "WindSummon"),
            new ServerMagicDescriptor(65, "wind_spirit", MagicBehaviorFamily.Spawn, "WindSpirit"),
            new ServerMagicDescriptor(66, "wind_totem", MagicBehaviorFamily.Build, "WindTotem"),
            new ServerMagicDescriptor(67, "zap_mouse", MagicBehaviorFamily.Spawn, "ZapMouse"),
        };
    }
}
