using System;
using System.Collections.Generic;
using System.Linq;
using Data.GameConfig;
using Global;

namespace Simulation.Core
{
    /// <summary>
    /// Maps card combinations to magic spells.
    /// Built dynamically from versioned server recipe data (GameConfigData.magicRecipes).
    /// Falls back to hardcoded defaults if server data unavailable.
    /// </summary>
    public class SimMagicRegistry
    {
        private readonly Dictionary<string, SimMagic> _recipeMap = new();

        /// <summary>
        /// Register a magic recipe. Cards are sorted to create a canonical key.
        /// </summary>
        public void Register(List<SimCardType> cards, SimMagic magic)
        {
            string key = MakeKey(cards);
            _recipeMap[key] = magic;
        }

        /// <summary>
        /// Look up a magic by its card combination. Returns null if not found.
        /// </summary>
        public SimMagic Parse(List<SimCardType> cards)
        {
            string key = MakeKey(cards);
            return _recipeMap.TryGetValue(key, out var magic) ? magic : null;
        }

        private static string MakeKey(List<SimCardType> cards)
        {
            var sorted = new List<SimCardType>(cards);
            sorted.Sort();
            return string.Join(",", sorted);
        }

        /// <summary>
        /// Build registry from server-provided versioned recipe data.
        /// Each recipe has a name (server bean name) and cards (card type strings).
        /// Falls back to BuildDefault() if recipes is null/empty.
        /// </summary>
        public static SimMagicRegistry BuildFromServerData(List<MagicRecipeData> recipes)
        {
            if (recipes == null || recipes.Count == 0)
            {
                WDebug.LogWarning("[SimMagicRegistry] No server recipes, using hardcoded defaults");
                return BuildDefault();
            }

            var registry = new SimMagicRegistry();

            foreach (var recipe in recipes)
            {
                // Map magic name to SimMagic instance
                var simMagic = ResolveSimMagic(recipe.name);
                if (simMagic == null)
                {
                    WDebug.LogWarning($"[SimMagicRegistry] Unknown magic name: {recipe.name}");
                    continue;
                }

                simMagic.Id = recipe.id;

                // Convert card strings to SimCardType
                var simCards = new List<SimCardType>();
                bool valid = true;
                foreach (var cardName in recipe.cards)
                {
                    if (Enum.TryParse<SimCardType>(cardName, true, out var ct))
                        simCards.Add(ct);
                    else
                    {
                        WDebug.LogWarning($"[SimMagicRegistry] Unknown card type '{cardName}' in recipe '{recipe.name}'");
                        valid = false;
                        break;
                    }
                }

                if (!valid) continue;
                registry.Register(simCards, simMagic);
            }

            WDebug.Log($"[SimMagicRegistry] Built {registry._recipeMap.Count} recipes from server data");
            return registry;
        }

        /// <summary>
        /// Maps server magic bean name to the corresponding SimMagic instance.
        /// </summary>
        private static SimMagic ResolveSimMagic(string name)
        {
            return name switch
            {
                // Swarm spawns (spawn multiple small units)
                "fire_slime_swarm" or "ember_spirit_swarm" => new SimSwarmSpawnMagic(PrefabType.EmberSpirit),
                "water_slime_swarm" => new SimSwarmSpawnMagic(PrefabType.WaterSlime),
                "lightning_slime_swarm" => new SimSwarmSpawnMagic(PrefabType.ElectricSlime),
                "seed_spirit_swarm" => new SimSwarmSpawnMagic(PrefabType.SeedSpirit),
                "mini_rock_swarm" => new SimSwarmSpawnMagic(PrefabType.MiniRock),
                "wind_slime_swarm" => new SimSwarmSpawnMagic(PrefabType.WindSlime),
                "thunder_bird_swarm" => new SimSwarmSpawnMagic(PrefabType.ThunderBird),

                // Shots
                "fire_shot" => new SimShotMagic(PrefabType.FireShot),
                "water_shot" => new SimShotMagic(PrefabType.WaterShot),
                "lightning_shot" => new SimShotMagic(PrefabType.ElectricShot),
                "vine_toss" or "nature_shot" => new SimShotMagic(PrefabType.VineToss),
                "rock_rolling" or "rock_shot" => new SimShotMagic(PrefabType.RockRolling),
                "wind_blade" or "wind_shot" => new SimShotMagic(PrefabType.WindBlade),
                "chain_lightning" => new SimShotMagic(PrefabType.ChainLightning),
                "tide_call" => new SimShotMagic(PrefabType.TideCall),

                // Explodes
                "fire_explosion" => new SimExplodeMagic(PrefabType.FireExplode),
                "water_explosion" => new SimExplodeMagic(PrefabType.WaterExplosion),
                "lightning_explosion" => new SimExplodeMagic(PrefabType.ElectricExplode),
                "nature_explosion" => new SimExplodeMagic(PrefabType.LeafExplode),
                "rock_explosion" => new SimExplodeMagic(PrefabType.RockExplode),
                "wind_explosion" => new SimExplodeMagic(PrefabType.WindExplode),
                "magma_explosion" => new SimExplodeMagic(PrefabType.MagmaExplosion),
                "sand_storm" => new SimExplodeMagic(PrefabType.SandStorm),
                "tornado_strike" => new SimExplodeMagic(PrefabType.TornadoStrike),

                // Drops
                "fire_drop" => new SimDropMagic(PrefabType.FireDrop),
                "rain_cloud" => new SimDropMagic(PrefabType.RainCloud),
                "nature_drop" => new SimDropMagic(PrefabType.NatureDrop),
                "rock_drop" => new SimDropMagic(PrefabType.RockDrop),
                "lightning_drop" => new SimDropMagic(PrefabType.LightningDrop),
                "wind_drop" => new SimDropMagic(PrefabType.WindDrop),
                "meteor_shower" => new SimDropMagic(PrefabType.MeteorShower),

                // Builds (slime nests / structures)
                "fire_slime_nest" => new SimBuildMagic(PrefabType.FireSummon),
                "water_slime_nest" => new SimBuildMagic(PrefabType.WaterSlime), // PvE only
                "lightning_slime_nest" => new SimBuildMagic(PrefabType.ElectricSummon),
                "nature_slime_nest" => new SimBuildMagic(PrefabType.PveNatureSlimeNest),
                "rock_slime_nest" => new SimBuildMagic(PrefabType.RockSummon),
                "wind_slime_nest" => new SimBuildMagic(PrefabType.WindSummon),
                "life_tree" => new SimBuildMagic(PrefabType.LifeTree),
                "rock_turret" => new SimBuildMagic(PrefabType.RockTurret),
                "wind_totem" => new SimBuildMagic(PrefabType.WindTotem),
                "cannon" => new SimBuildMagic(PrefabType.GroundCannon),
                "tower" => new SimBuildMagic(PrefabType.GroundTower),
                "mana_well" => new SimBuildMagic(PrefabType.ManaWell),
                "healing_totem" => new SimBuildMagic(PrefabType.HealingTotem),
                "vine_colony" => new SimBuildMagic(PrefabType.VineColony),

                // Single spawns (advanced mobs)
                "aqua_archer" => new SimSpawnMagic(PrefabType.AquaArcher),
                "rock_golem" => new SimSpawnMagic(PrefabType.RockGolem),
                "storm_rider" => new SimSpawnMagic(PrefabType.StormRider),
                "thunder_spirit" => new SimSpawnMagic(PrefabType.ThunderSpirit),
                "fire_spirit" => new SimSpawnMagic(PrefabType.FireSpirit),
                "magma_spirit" => new SimSpawnMagic(PrefabType.MagmaSpirit),
                "wind_spirit" => new SimSpawnMagic(PrefabType.WindSpirit),
                "rock_mage" => new SimSpawnMagic(PrefabType.RockMage),
                "cloud_dragon" => new SimSpawnMagic(PrefabType.CloudDragon),
                "tree_golem" => new SimSpawnMagic(PrefabType.TreeGolem),
                "vine_spirit" => new SimSpawnMagic(PrefabType.VineSpirit),

                _ => null
            };
        }

        /// <summary>
        /// Hardcoded fallback registry. Used when server recipe data is unavailable.
        /// </summary>
        public static SimMagicRegistry BuildDefault()
        {
            var registry = new SimMagicRegistry();

            // Shot magics (Shoot + Element)
            registry.Register(new() { SimCardType.Shoot, SimCardType.Fire }, new SimShotMagic(PrefabType.FireShot));
            registry.Register(new() { SimCardType.Shoot, SimCardType.Water }, new SimShotMagic(PrefabType.TideCall));
            registry.Register(new() { SimCardType.Shoot, SimCardType.Lightning }, new SimShotMagic(PrefabType.ChainLightning));
            registry.Register(new() { SimCardType.Shoot, SimCardType.Rock }, new SimShotMagic(PrefabType.RockRolling));
            registry.Register(new() { SimCardType.Shoot, SimCardType.Nature }, new SimShotMagic(PrefabType.VineToss));
            registry.Register(new() { SimCardType.Shoot, SimCardType.Wind }, new SimShotMagic(PrefabType.WindBlade));

            // Explode magics (Explode + Element)
            registry.Register(new() { SimCardType.Explode, SimCardType.Fire }, new SimExplodeMagic(PrefabType.MagmaExplosion));
            registry.Register(new() { SimCardType.Explode, SimCardType.Water }, new SimExplodeMagic(PrefabType.WaterExplosion));
            registry.Register(new() { SimCardType.Explode, SimCardType.Nature }, new SimExplodeMagic(PrefabType.LeafExplode));
            registry.Register(new() { SimCardType.Explode, SimCardType.Rock }, new SimExplodeMagic(PrefabType.RockExplode));
            registry.Register(new() { SimCardType.Explode, SimCardType.Lightning }, new SimExplodeMagic(PrefabType.ElectricExplode));
            registry.Register(new() { SimCardType.Explode, SimCardType.Wind }, new SimExplodeMagic(PrefabType.WindExplode));

            // Drop magics (Drop + Element)
            registry.Register(new() { SimCardType.Drop, SimCardType.Fire }, new SimDropMagic(PrefabType.FireDrop));
            registry.Register(new() { SimCardType.Drop, SimCardType.Water }, new SimDropMagic(PrefabType.RainCloud));
            registry.Register(new() { SimCardType.Drop, SimCardType.Nature }, new SimDropMagic(PrefabType.NatureDrop));
            registry.Register(new() { SimCardType.Drop, SimCardType.Rock }, new SimDropMagic(PrefabType.RockDrop));
            registry.Register(new() { SimCardType.Drop, SimCardType.Lightning }, new SimDropMagic(PrefabType.LightningDrop));
            registry.Register(new() { SimCardType.Drop, SimCardType.Wind }, new SimDropMagic(PrefabType.WindDrop));

            // Spawn magics (Spawn + Element) — swarm spawns from LocalCombinedMagicData
            registry.Register(new() { SimCardType.Spawn, SimCardType.Fire }, new SimSwarmSpawnMagic(PrefabType.EmberSpirit));
            registry.Register(new() { SimCardType.Spawn, SimCardType.Water }, new SimSwarmSpawnMagic(PrefabType.WaterSlime));
            registry.Register(new() { SimCardType.Spawn, SimCardType.Nature }, new SimSwarmSpawnMagic(PrefabType.SeedSpirit));
            registry.Register(new() { SimCardType.Spawn, SimCardType.Rock }, new SimSwarmSpawnMagic(PrefabType.MiniRock));
            registry.Register(new() { SimCardType.Spawn, SimCardType.Wind }, new SimSpawnMagic(PrefabType.WindSpirit));

            // Build magics (Build + Element)
            registry.Register(new() { SimCardType.Build, SimCardType.Nature }, new SimBuildMagic(PrefabType.LifeTree));
            registry.Register(new() { SimCardType.Build, SimCardType.Rock }, new SimBuildMagic(PrefabType.RockTurret));
            registry.Register(new() { SimCardType.Build, SimCardType.Wind }, new SimBuildMagic(PrefabType.WindTotem));

            // Advanced (from LocalCombinedMagicData)
            registry.Register(new() { SimCardType.Build, SimCardType.Shoot, SimCardType.Rock }, new SimBuildMagic(PrefabType.GroundCannon));
            registry.Register(new() { SimCardType.Build, SimCardType.Explode, SimCardType.Rock }, new SimBuildMagic(PrefabType.GroundTower));
            registry.Register(new() { SimCardType.Build, SimCardType.Lightning, SimCardType.Nature }, new SimBuildMagic(PrefabType.ManaWell));
            registry.Register(new() { SimCardType.Build, SimCardType.Nature, SimCardType.Water }, new SimBuildMagic(PrefabType.HealingTotem));
            registry.Register(new() { SimCardType.Build, SimCardType.Nature, SimCardType.Nature }, new SimBuildMagic(PrefabType.VineColony));

            registry.Register(new() { SimCardType.Spawn, SimCardType.Shoot, SimCardType.Water }, new SimSpawnMagic(PrefabType.AquaArcher));
            registry.Register(new() { SimCardType.Spawn, SimCardType.Rock, SimCardType.Rock }, new SimSpawnMagic(PrefabType.RockGolem));
            registry.Register(new() { SimCardType.Spawn, SimCardType.Water, SimCardType.Lightning }, new SimSpawnMagic(PrefabType.StormRider));
            registry.Register(new() { SimCardType.Spawn, SimCardType.Lightning, SimCardType.Wind }, new SimSpawnMagic(PrefabType.ThunderSpirit));
            registry.Register(new() { SimCardType.Spawn, SimCardType.Wind, SimCardType.Fire }, new SimSpawnMagic(PrefabType.FireSpirit));

            registry.Register(new() { SimCardType.Explode, SimCardType.Rock, SimCardType.Wind }, new SimExplodeMagic(PrefabType.SandStorm));

            // 3rd tier
            registry.Register(new() { SimCardType.Spawn, SimCardType.Water, SimCardType.Wind }, new SimSpawnMagic(PrefabType.CloudDragon));
            registry.Register(new() { SimCardType.Spawn, SimCardType.Drop, SimCardType.Lightning }, new SimSwarmSpawnMagic(PrefabType.ThunderBird));
            registry.Register(new() { SimCardType.Spawn, SimCardType.Nature, SimCardType.Nature }, new SimSpawnMagic(PrefabType.TreeGolem));
            registry.Register(new() { SimCardType.Spawn, SimCardType.Shoot, SimCardType.Nature }, new SimSpawnMagic(PrefabType.VineSpirit));
            registry.Register(new() { SimCardType.Spawn, SimCardType.Shoot, SimCardType.Rock }, new SimSpawnMagic(PrefabType.RockMage));

            // Complex (5 card) — from LocalCombinedMagicData
            registry.Register(new() { SimCardType.Spawn, SimCardType.Explode, SimCardType.Fire, SimCardType.Fire, SimCardType.Rock }, new SimSpawnMagic(PrefabType.MagmaSpirit));
            registry.Register(new() { SimCardType.Spawn, SimCardType.Wind, SimCardType.Wind, SimCardType.Wind, SimCardType.Nature }, new SimExplodeMagic(PrefabType.TornadoStrike));
            registry.Register(new() { SimCardType.Drop, SimCardType.Fire, SimCardType.Fire, SimCardType.Rock, SimCardType.Rock }, new SimDropMagic(PrefabType.MeteorShower));

            return registry;
        }
    }
}
