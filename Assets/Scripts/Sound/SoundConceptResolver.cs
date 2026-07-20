using System.Collections.Generic;

namespace Sound
{
    public enum SoundConcept
    {
        Generic,
        Fire,
        Water,
        Nature,
        Lightning,
        Rock,
        Wind
    }

    public static class SoundConceptResolver
    {
        private static readonly Dictionary<string, SoundConcept> PrefabConcepts = new Dictionary<string, SoundConcept>
        {
            { "Crater", SoundConcept.Fire },
            { "CraterEmber", SoundConcept.Fire },
            { "EmberSpirit", SoundConcept.Fire },
            { "FireChildSpirit", SoundConcept.Fire },
            { "FireDrop", SoundConcept.Fire },
            { "FireExplode", SoundConcept.Fire },
            { "FireField", SoundConcept.Fire },
            { "FireLordSpirit", SoundConcept.Fire },
            { "FireRune", SoundConcept.Fire },
            { "FireShot", SoundConcept.Fire },
            { "FireSlime", SoundConcept.Fire },
            { "FireSpirit", SoundConcept.Fire },
            { "FireTadpole", SoundConcept.Fire },
            { "MagmaExplosion", SoundConcept.Fire },
            { "MagmaFist", SoundConcept.Fire },
            { "MagmaSpirit", SoundConcept.Fire },
            { "MeteorDrop", SoundConcept.Fire },
            { "MeteorShower", SoundConcept.Fire },
            { "RallyingTorch", SoundConcept.Fire },

            { "AquaArcher", SoundConcept.Water },
            { "BubbleGenerator", SoundConcept.Water },
            { "BubbleSpirit", SoundConcept.Water },
            { "PveWaterSlimeNest", SoundConcept.Water },
            { "RainCloud", SoundConcept.Water },
            { "TideCall", SoundConcept.Water },
            { "WaterExplode", SoundConcept.Water },
            { "WaterExplosion", SoundConcept.Water },
            { "WaterField", SoundConcept.Water },
            { "WaterRune", SoundConcept.Water },
            { "WaterShot", SoundConcept.Water },
            { "WaterSlime", SoundConcept.Water },

            { "LeafExplode", SoundConcept.Nature },
            { "LeafField", SoundConcept.Nature },
            { "LeafShot", SoundConcept.Nature },
            { "LeafSlime", SoundConcept.Nature },
            { "Leafair", SoundConcept.Nature },
            { "LifeTree", SoundConcept.Nature },
            { "NatureDrop", SoundConcept.Nature },
            { "NatureRune", SoundConcept.Nature },
            { "Overgrowth", SoundConcept.Nature },
            { "PveNatureSlimeNest", SoundConcept.Nature },
            { "PveVineColony", SoundConcept.Nature },
            { "PveVineWitch", SoundConcept.Nature },
            { "SeedSpirit", SoundConcept.Nature },
            { "TreeGolem", SoundConcept.Nature },
            { "Vine", SoundConcept.Nature },
            { "VineColony", SoundConcept.Nature },
            { "VineSpirit", SoundConcept.Nature },
            { "WillOWisp", SoundConcept.Nature },

            { "ChainLightning", SoundConcept.Lightning },
            { "ElectricExplode", SoundConcept.Lightning },
            { "ElectricField", SoundConcept.Lightning },
            { "ElectricShot", SoundConcept.Lightning },
            { "ElectricSlime", SoundConcept.Lightning },
            { "ElectricTower", SoundConcept.Lightning },
            { "LightningDrop", SoundConcept.Lightning },
            { "LightningRune", SoundConcept.Lightning },
            { "LightningTadpole", SoundConcept.Lightning },
            { "ShockOverload", SoundConcept.Lightning },
            { "StormRider", SoundConcept.Lightning },
            { "ThunderBird", SoundConcept.Lightning },
            { "ThunderSpirit", SoundConcept.Lightning },
            { "ZapMouse", SoundConcept.Lightning },

            { "GroundCannon", SoundConcept.Rock },
            { "GroundTower", SoundConcept.Rock },
            { "MiniRock", SoundConcept.Rock },
            { "RockDrop", SoundConcept.Rock },
            { "RockExplode", SoundConcept.Rock },
            { "RockGolem", SoundConcept.Rock },
            { "RockMage", SoundConcept.Rock },
            { "RockRolling", SoundConcept.Rock },
            { "RockRune", SoundConcept.Rock },
            { "RockSlime", SoundConcept.Rock },
            { "RockTurret", SoundConcept.Rock },
            { "SandStorm", SoundConcept.Rock },

            { "CloudDragon", SoundConcept.Wind },
            { "RazorGale", SoundConcept.Wind },
            { "TornadoStrike", SoundConcept.Wind },
            { "WindBlade", SoundConcept.Wind },
            { "WindDrop", SoundConcept.Wind },
            { "WindExplode", SoundConcept.Wind },
            { "WindRune", SoundConcept.Wind },
            { "WindSlime", SoundConcept.Wind },
            { "WindSpirit", SoundConcept.Wind },
            { "WindTotem", SoundConcept.Wind },

            { "ChickenCommando", SoundConcept.Generic },
            { "DimensionToad", SoundConcept.Generic },
            { "FrenzyTotem", SoundConcept.Generic },
            { "HealingTotem", SoundConcept.Generic },
            { "ManaWell", SoundConcept.Generic },
            { "Player", SoundConcept.Generic },
            { "ServedObjectHpBar", SoundConcept.Generic },
            { "Towerback", SoundConcept.Generic }
        };

        public static SoundConcept Resolve(string prefabType)
        {
            if (string.IsNullOrEmpty(prefabType)) return SoundConcept.Generic;
            return PrefabConcepts.TryGetValue(prefabType, out SoundConcept concept)
                ? concept
                : SoundConcept.Generic;
        }
    }
}
