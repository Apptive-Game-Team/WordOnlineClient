using System;
using System.Collections.Generic;

namespace Simulation.Core
{
    /// <summary>
    /// Initializes SimGameObjects with components based on PrefabType.
    /// Replaces server's PrefabProvider + PrefabInitializer pattern.
    /// Parameters come from SessionStartDto at game init.
    /// </summary>
    public static class SimPrefabFactory
    {
        private static Dictionary<string, Dictionary<string, Fix64>> _params;

        public static void SetParameters(Dictionary<string, Dictionary<string, Fix64>> parameters)
        {
            _params = parameters;
        }

        private static Fix64 P(string group, string key)
        {
            if (_params != null && _params.TryGetValue(group, out var g) && g.TryGetValue(key, out var v))
                return v;
            return Fix64.One; // fallback
        }

        private static int PI(string group, string key) => P(group, key).ToInt();

        /// <summary>
        /// Initialize a game object with components for its PrefabType.
        /// Called by SimGameObject.Start() after construction.
        /// </summary>
        public static void Initialize(SimGameObject obj)
        {
            switch (obj.Type)
            {
                case PrefabType.Player: InitPlayer(obj); break;

                // Slimes
                case PrefabType.FireSlime: InitSlime(obj, ElementType.FIRE); break;
                case PrefabType.WaterSlime: InitSlime(obj, ElementType.WATER); break;
                case PrefabType.LeafSlime: InitSlime(obj, ElementType.NATURE); break;
                case PrefabType.RockSlime: InitSlime(obj, ElementType.ROCK); break;
                case PrefabType.ElectricSlime: InitSlime(obj, ElementType.LIGHTNING); break;
                case PrefabType.WindSlime: InitSlime(obj, ElementType.WIND); break;

                // Shots
                case PrefabType.FireShot: InitShot(obj, ElementType.FIRE, "shoot"); break;
                case PrefabType.WaterShot: InitShot(obj, ElementType.WATER, "shoot"); break;
                case PrefabType.ElectricShot: InitShot(obj, ElementType.LIGHTNING, "shoot"); break;
                case PrefabType.WindBlade: InitShot(obj, ElementType.WIND, "shoot"); break;
                case PrefabType.VineToss: InitShot(obj, ElementType.NATURE, "shoot"); break;
                case PrefabType.RockRolling: InitShot(obj, ElementType.ROCK, "shoot"); break;

                // Explodes
                case PrefabType.FireExplode: InitExplode(obj, ElementType.FIRE); break;
                case PrefabType.WaterExplode: InitWaterExplode(obj); break;
                case PrefabType.LeafExplode: InitExplode(obj, ElementType.NATURE); break;
                case PrefabType.RockExplode: InitExplode(obj, ElementType.ROCK); break;
                case PrefabType.ElectricExplode: InitExplode(obj, ElementType.LIGHTNING); break;
                case PrefabType.WindExplode: InitExplode(obj, ElementType.WIND); break;

                // Drops
                case PrefabType.FireDrop: InitDrop(obj, ElementType.FIRE); break;
                case PrefabType.RainCloud: InitDrop(obj, ElementType.WATER); break;
                case PrefabType.NatureDrop: InitDrop(obj, ElementType.NATURE); break;
                case PrefabType.RockDrop: InitDrop(obj, ElementType.ROCK); break;
                case PrefabType.LightningDrop: InitDrop(obj, ElementType.LIGHTNING); break;
                case PrefabType.WindDrop: InitDrop(obj, ElementType.WIND); break;

                // Fields
                case PrefabType.LeafField: InitField(obj, ElementType.NATURE, Effect.LeafFieldHeal); break;
                case PrefabType.WaterField: InitField(obj, ElementType.WATER, Effect.Wet); break;
                case PrefabType.FireField: InitField(obj, ElementType.FIRE, Effect.Burn); break;
                case PrefabType.ElectricField: InitField(obj, ElementType.LIGHTNING, Effect.Shock); break;

                // Builds
                case PrefabType.LifeTree: InitBuild(obj, ElementType.NATURE, "lifetree"); break;
                case PrefabType.RockTurret: InitBuild(obj, ElementType.ROCK, "rockturret"); break;
                case PrefabType.WindTotem: InitBuild(obj, ElementType.WIND, "windtotem"); break;

                // Runes
                case PrefabType.FireRune: InitRune(obj, ElementType.FIRE); break;
                case PrefabType.WaterRune: InitRune(obj, ElementType.WATER); break;
                case PrefabType.NatureRune: InitRune(obj, ElementType.NATURE); break;
                case PrefabType.RockRune: InitRune(obj, ElementType.ROCK); break;
                case PrefabType.LightningRune: InitRune(obj, ElementType.LIGHTNING); break;
                case PrefabType.WindRune: InitRune(obj, ElementType.WIND); break;

                // Summon nests (spawners)
                case PrefabType.FireSummon: InitSummonNest(obj, ElementType.FIRE, PrefabType.FireSlime); break;
                case PrefabType.RockSummon: InitSummonNest(obj, ElementType.ROCK, PrefabType.RockSlime); break;
                case PrefabType.ElectricSummon: InitSummonNest(obj, ElementType.LIGHTNING, PrefabType.ElectricSlime); break;
                case PrefabType.WindSummon: InitSummonNest(obj, ElementType.WIND, PrefabType.WindSlime); break;

                // Advanced mobs — melee
                case PrefabType.RockGolem: InitMeleeMob(obj, ElementType.ROCK, "rock_golem", SimTargetMask.GROUND); break;
                case PrefabType.StormRider: InitMeleeMob(obj, "storm_rider", SimTargetMask.GROUND, ElementType.LIGHTNING, ElementType.WATER); break;

                // Advanced mobs — ranged
                case PrefabType.AquaArcher: InitRangedMob(obj, "aqua_archer", PrefabType.WaterShot, SimTargetMask.ANY, ElementType.WATER); break;
                case PrefabType.ThunderSpirit: InitRangedMobAerial(obj, "thunder_spirit", PrefabType.ElectricShot, SimTargetMask.GROUND, ElementType.LIGHTNING, ElementType.WIND); break;
                case PrefabType.RockMage: InitRangedMob(obj, "rock_mage", PrefabType.RockRolling, SimTargetMask.ANY, ElementType.ROCK); break;

                // Advanced mobs — special
                case PrefabType.FireSpirit: InitFireSpirit(obj); break;
                case PrefabType.MagmaSpirit: InitMagmaSpirit(obj); break;
                case PrefabType.WindSpirit: InitWindSpirit(obj); break;

                // Stationary structures
                case PrefabType.HealingTotem: InitHealingTotem(obj); break;
                case PrefabType.GroundCannon: InitCannon(obj, "ground_cannon", SimTargetMask.GROUND); break;
                case PrefabType.GroundTower: InitCannon(obj, "ground_tower", SimTargetMask.AIR); break;
                case PrefabType.ManaWell: InitManaWell(obj); break;
                case PrefabType.VineColony: InitVineColony(obj); break;

                // Area effects
                case PrefabType.SandStorm: InitSandStorm(obj); break;
                case PrefabType.TornadoStrike: InitTornadoStrike(obj); break;
                case PrefabType.MeteorShower: InitMeteorShower(obj); break;

                // 3rd tier mobs
                case PrefabType.TreeGolem: InitTreeGolem(obj); break;
                case PrefabType.VineSpirit: InitVineSpirit(obj); break;
                case PrefabType.ThunderBird: InitRangedMobAerial(obj, "thunder_bird", PrefabType.ElectricShot, SimTargetMask.GROUND, ElementType.LIGHTNING); break;
                case PrefabType.CloudDragon: InitCloudDragon(obj); break;

                // Sub-prefabs
                case PrefabType.MagmaFist: InitMagmaFist(obj); break;
                case PrefabType.Vine: InitVine(obj); break;
                case PrefabType.MeteorDrop: InitMeteorDrop(obj); break;

                // Multi-element explodes
                case PrefabType.MagmaExplosion: InitExplode(obj, ElementType.FIRE); break;
                case PrefabType.WaterExplosion: InitWaterExplode(obj); break;

                // Multi-element shots
                case PrefabType.ChainLightning: InitShot(obj, ElementType.LIGHTNING, "shoot"); break;
                case PrefabType.TideCall: InitTideCall(obj); break;

                // Swarm spawns (same as element slimes)
                case PrefabType.EmberSpirit: InitSlime(obj, ElementType.FIRE); break;
                case PrefabType.SeedSpirit: InitSlime(obj, ElementType.NATURE); break;
                case PrefabType.MiniRock: InitSlime(obj, ElementType.ROCK); break;

                // Wall
                case PrefabType.Wall: InitWall(obj); break;

                // Default: just set element based on name analysis
                default: InitGenericMob(obj); break;
            }
        }

        // ── Initializers ──

        private static void InitPlayer(SimGameObject obj)
        {
            obj.AddComponent(new SimCircleCollider(Fix64.One, false));
            obj.SetElement(ElementType.NONE);
            obj.AddComponent(new SimPlayerHealth());
            obj.AddComponent(new SimEffectReceiver());
        }

        private static void InitSlime(SimGameObject obj, ElementType element)
        {
            obj.AddComponent(new SimRigidBody(PI("slime", "mass")));
            obj.AddComponent(new SimZPhysics());
            obj.AddComponent(new SimCircleCollider(P("slime", "radius"), false));
            obj.SetElement(element);
            obj.AddComponent(new SimAttackMob(
                PI("slime", "hp"),
                PI("slime", "speed"),
                SimTargetMask.GROUND,
                PI("slime", "damage"),
                P("slime", "attack_interval"),
                P("slime", "attack_range")
            ));
            obj.AddComponent(new SimEffectReceiver());
            obj.AddComponent(new SimItemCarrier());
        }

        private static void InitShot(SimGameObject obj, ElementType element, string paramGroup)
        {
            obj.AddComponent(new SimCircleCollider(P(paramGroup, "radius"), true));
            obj.SetElement(element);
            obj.AddComponent(new SimShot(PI(paramGroup, "damage"), P(paramGroup, "speed")));
        }

        private static void InitExplode(SimGameObject obj, ElementType element)
        {
            obj.SetElement(element);
            obj.AddComponent(new SimExplode(
                PI("explode", "damage"),
                P("explode", "radius")
            ));
        }

        private static void InitWaterExplode(SimGameObject obj)
        {
            obj.SetElement(ElementType.WATER);
            obj.AddComponent(new SimWaterExplode(
                PI("explode", "damage"),
                P("explode", "radius"),
                P("explode", "z_force")
            ));
        }

        private static void InitDrop(SimGameObject obj, ElementType element)
        {
            obj.AddComponent(new SimCircleCollider(P("drop", "radius"), true));
            obj.SetElement(element);
            obj.AddComponent(new SimDrop(PI("drop", "damage")));
        }

        private static void InitField(SimGameObject obj, ElementType element, Effect effect)
        {
            obj.SetElement(element);
            obj.AddComponent(new SimCircleCollider(P("field", "radius"), true));
            obj.AddComponent(new SimDummyMob(PI("field", "hp")));
            obj.AddComponent(new SimAreaEffectProvider(P("field", "interval"), P("field", "radius"), effect));
        }

        private static void InitBuild(SimGameObject obj, ElementType element, string paramGroup)
        {
            obj.SetElement(element);
            obj.AddComponent(new SimCircleCollider(P(paramGroup, "radius"), false));
            obj.AddComponent(new SimDummyMob(PI(paramGroup, "hp")));
            obj.AddComponent(new SimEffectReceiver());
        }

        private static void InitRune(SimGameObject obj, ElementType element)
        {
            obj.SetElement(element);
            obj.AddComponent(new SimCircleCollider(P("rune", "radius"), true));
            obj.AddComponent(new SimRune(P("rune", "attack_range"), PI("rune", "damage")));
        }

        private static void InitWall(SimGameObject obj)
        {
            obj.SetElement(ElementType.NONE);
            obj.AddComponent(new SimCircleCollider(SimGameConfig.OBSTACLE_RADIUS, false));
            obj.AddComponent(new SimRigidBody(-1)); // infinite mass
        }

        // ── Advanced Initializers ──

        private static void InitSummonNest(SimGameObject obj, ElementType element, PrefabType spawnType)
        {
            obj.AddComponent(new SimCircleCollider(P("build", "radius"), true));
            obj.SetElement(element);
            obj.AddComponent(new SimSpawner(PI("build", "hp"), spawnType));
        }

        private static void InitMeleeMob(SimGameObject obj, ElementType element, string paramGroup, int targetMask)
        {
            obj.AddComponent(new SimRigidBody(PI(paramGroup, "mass")));
            obj.AddComponent(new SimZPhysics());
            obj.AddComponent(new SimCircleCollider(P(paramGroup, "radius"), false));
            obj.SetElement(element);
            obj.AddComponent(new SimAttackMob(
                PI(paramGroup, "hp"),
                PI(paramGroup, "speed"),
                targetMask,
                PI(paramGroup, "damage"),
                P(paramGroup, "attack_interval"),
                P(paramGroup, "attack_range")
            ));
            obj.AddComponent(new SimEffectReceiver());
        }

        private static void InitMeleeMob(SimGameObject obj, string paramGroup, int targetMask, params ElementType[] elements)
        {
            obj.AddComponent(new SimRigidBody(PI(paramGroup, "mass")));
            obj.AddComponent(new SimZPhysics());
            obj.AddComponent(new SimCircleCollider(P(paramGroup, "radius"), false));
            foreach (var e in elements) obj.SetElement(e);
            obj.AddComponent(new SimAttackMob(
                PI(paramGroup, "hp"),
                PI(paramGroup, "speed"),
                targetMask,
                PI(paramGroup, "damage"),
                P(paramGroup, "attack_interval"),
                P(paramGroup, "attack_range")
            ));
            obj.AddComponent(new SimEffectReceiver());
        }

        private static void InitRangedMob(SimGameObject obj, string paramGroup, PrefabType projectile, int targetMask, params ElementType[] elements)
        {
            obj.AddComponent(new SimRigidBody(PI(paramGroup, "mass")));
            obj.AddComponent(new SimZPhysics());
            obj.AddComponent(new SimCircleCollider(P(paramGroup, "radius"), false));
            foreach (var e in elements) obj.SetElement(e);
            obj.AddComponent(new SimRangeAttackMob(
                PI(paramGroup, "hp"),
                PI(paramGroup, "speed"),
                targetMask,
                PI(paramGroup, "damage"),
                P(paramGroup, "attack_interval"),
                P(paramGroup, "attack_range"),
                projectile
            ));
            obj.AddComponent(new SimEffectReceiver());
        }

        private static void InitRangedMobAerial(SimGameObject obj, string paramGroup, PrefabType projectile, int targetMask, params ElementType[] elements)
        {
            obj.AddComponent(new SimRigidBody(PI(paramGroup, "mass")));
            obj.AddComponent(new SimZPhysics(SimGameConfig.AERIAL_MOB_INIT_HEIGHT));
            obj.AddComponent(new SimCircleCollider(P(paramGroup, "radius"), false));
            foreach (var e in elements) obj.SetElement(e);
            obj.AddComponent(new SimRangeAttackMob(
                PI(paramGroup, "hp"),
                PI(paramGroup, "speed"),
                targetMask,
                PI(paramGroup, "damage"),
                P(paramGroup, "attack_interval"),
                P(paramGroup, "attack_range"),
                projectile
            ));
            obj.AddComponent(new SimEffectReceiver());
        }

        private static void InitFireSpirit(SimGameObject obj)
        {
            obj.AddComponent(new SimRigidBody(PI("fire_spirit", "mass")));
            obj.AddComponent(new SimZPhysics());
            obj.AddComponent(new SimCircleCollider(P("fire_spirit", "radius"), false));
            obj.Element.Add(ElementType.FIRE);
            obj.Element.Add(ElementType.WIND);
            obj.AddComponent(new SimAttackMob(
                PI("fire_spirit", "hp"),
                PI("fire_spirit", "speed"),
                SimTargetMask.GROUND,
                PI("fire_spirit", "damage"),
                P("fire_spirit", "attack_interval"),
                P("fire_spirit", "attack_range")
            ));
            obj.AddComponent(new SimEffectReceiver());
            obj.AddComponent(new SimAreaEffectProvider(Fix64.One, P("fire_spirit", "attack_range"), Effect.Burn));
        }

        private static void InitMagmaSpirit(SimGameObject obj)
        {
            obj.AddComponent(new SimRigidBody(PI("fire_spirit", "mass")));
            obj.AddComponent(new SimZPhysics());
            obj.AddComponent(new SimCircleCollider(P("fire_spirit", "radius"), false));
            obj.Element.Add(ElementType.FIRE);
            obj.Element.Add(ElementType.ROCK);
            obj.AddComponent(new SimSummonerMob(
                PI("magma_spirit", "hp"),
                PI("magma_spirit", "speed"),
                SimTargetMask.GROUND,
                P("magma_spirit", "attack_interval"),
                P("magma_spirit", "attack_range"),
                PrefabType.MagmaFist
            ));
            obj.AddComponent(new SimEffectReceiver());
        }

        private static void InitWindSpirit(SimGameObject obj)
        {
            obj.AddComponent(new SimRigidBody(PI("wind_spirit", "mass")));
            obj.AddComponent(new SimZPhysics(SimGameConfig.AERIAL_MOB_INIT_HEIGHT));
            obj.AddComponent(new SimCircleCollider(P("wind_spirit", "radius"), false));
            obj.SetElement(ElementType.WIND);
            obj.AddComponent(new SimAttackMob(
                PI("wind_spirit", "hp"),
                PI("wind_spirit", "speed"),
                SimTargetMask.AIR,
                PI("wind_spirit", "damage"),
                P("wind_spirit", "attack_interval"),
                P("wind_spirit", "attack_range")
            ));
            obj.AddComponent(new SimEffectReceiver());
        }

        private static void InitHealingTotem(SimGameObject obj)
        {
            obj.AddComponent(new SimRigidBody(PI("healing_totem", "mass")));
            obj.AddComponent(new SimCircleCollider(P("healing_totem", "radius"), true));
            obj.Element.Add(ElementType.NATURE);
            obj.Element.Add(ElementType.WATER);
            obj.AddComponent(new SimTotem(
                PI("healing_totem", "hp"),
                PI("healing_totem", "damage"),
                P("healing_totem", "attack_interval"),
                P("healing_totem", "range")
            ));
            obj.AddComponent(new SimTimedSelfDestroyer(P("healing_totem", "duration")));
            obj.AddComponent(new SimEffectReceiver());
        }

        private static void InitCannon(SimGameObject obj, string paramGroup, int targetMask)
        {
            obj.AddComponent(new SimRigidBody(PI(paramGroup, "mass")));
            obj.AddComponent(new SimCircleCollider(P(paramGroup, "radius"), false));
            obj.SetElement(ElementType.ROCK);
            obj.AddComponent(new SimCannon(
                PI(paramGroup, "hp"),
                PI(paramGroup, "damage"),
                targetMask,
                P(paramGroup, "attack_interval"),
                P(paramGroup, "attack_range")
            ));
            obj.AddComponent(new SimEffectReceiver());
        }

        private static void InitManaWell(SimGameObject obj)
        {
            obj.AddComponent(new SimCircleCollider(P("mana_well", "radius"), false));
            obj.Element.Add(ElementType.LIGHTNING);
            obj.Element.Add(ElementType.NATURE);
            obj.AddComponent(new SimDummyMob(PI("mana_well", "hp")));
            obj.AddComponent(new SimEffectReceiver());
        }

        private static void InitVineColony(SimGameObject obj)
        {
            obj.AddComponent(new SimRigidBody(PI("vine_colony", "mass")));
            obj.AddComponent(new SimCircleCollider(P("vine_colony", "radius"), false));
            obj.SetElement(ElementType.NATURE);
            obj.AddComponent(new SimSummonMob(
                PI("vine_colony", "hp"),
                PI("vine_colony", "damage"),
                P("vine_colony", "attack_interval"),
                P("vine_colony", "attack_range"),
                PrefabType.Vine
            ));
            obj.AddComponent(new SimEffectReceiver());
        }

        private static void InitSandStorm(SimGameObject obj)
        {
            obj.AddComponent(new SimCircleCollider(P("sand_storm", "radius"), true));
            obj.Element.Add(ElementType.ROCK);
            obj.Element.Add(ElementType.WIND);
            obj.AddComponent(new SimEffectProvider(Effect.Sandstorm));
            obj.AddComponent(new SimTimedSelfDestroyer(P("sand_storm", "duration")));
        }

        private static void InitTornadoStrike(SimGameObject obj)
        {
            obj.AddComponent(new SimCircleCollider(P("tornado_strike", "radius"), true));
            obj.Element.Add(ElementType.NATURE);
            obj.Element.Add(ElementType.WIND);
            obj.AddComponent(new SimTornado(
                P("tornado_strike", "speed"),
                PI("tornado_strike", "damage"),
                P("tornado_strike", "radius"),
                P("tornado_strike", "duration"),
                P("tornado_strike", "attack_interval")
            ));
            obj.AddComponent(new SimTimedSelfDestroyer(P("tornado_strike", "duration")));
        }

        private static void InitMeteorShower(SimGameObject obj)
        {
            obj.Element.Add(ElementType.FIRE);
            obj.Element.Add(ElementType.ROCK);
            obj.AddComponent(new SimRandomAreaSpawner(
                PrefabType.MeteorDrop,
                P("meteor_shower", "attack_interval"),
                P("meteor_shower", "radius")
            ));
            obj.AddComponent(new SimTimedSelfDestroyer(P("meteor_shower", "duration")));
        }

        private static void InitTreeGolem(SimGameObject obj)
        {
            obj.AddComponent(new SimRigidBody(PI("tree_golem", "mass")));
            obj.AddComponent(new SimZPhysics());
            obj.AddComponent(new SimCircleCollider(P("tree_golem", "radius"), false));
            obj.SetElement(ElementType.NATURE);
            obj.AddComponent(new SimAttackMob(
                PI("tree_golem", "hp"),
                PI("tree_golem", "speed"),
                SimTargetMask.GROUND,
                PI("tree_golem", "damage"),
                P("tree_golem", "attack_interval"),
                P("tree_golem", "attack_range")
            ));
            obj.AddComponent(new SimSelfHealer(
                PI("tree_golem", "heal_amount"),
                P("tree_golem", "heal_interval"),
                ElementType.NATURE
            ));
            obj.AddComponent(new SimPathSpawner(PrefabType.LeafField, Fix64.One));
            obj.AddComponent(new SimEffectReceiver());
        }

        private static void InitVineSpirit(SimGameObject obj)
        {
            obj.AddComponent(new SimRigidBody(PI("vine_spirit", "mass")));
            obj.AddComponent(new SimZPhysics());
            obj.AddComponent(new SimCircleCollider(P("vine_spirit", "radius"), false));
            obj.SetElement(ElementType.NATURE);
            obj.AddComponent(new SimRangeAttackMob(
                PI("vine_spirit", "hp"),
                PI("vine_spirit", "speed"),
                SimTargetMask.ANY,
                PI("vine_spirit", "damage"),
                P("vine_spirit", "attack_interval"),
                P("vine_spirit", "attack_range"),
                PrefabType.VineToss
            ));
            obj.AddComponent(new SimEffectReceiver());
        }

        private static void InitCloudDragon(SimGameObject obj)
        {
            obj.AddComponent(new SimRigidBody(PI("cloud_dragon", "mass")));
            obj.AddComponent(new SimZPhysics(SimGameConfig.AERIAL_MOB_INIT_HEIGHT));
            obj.AddComponent(new SimCircleCollider(P("cloud_dragon", "radius"), false));
            obj.Element.Add(ElementType.WATER);
            obj.Element.Add(ElementType.WIND);
            obj.AddComponent(new SimRangeAttackMob(
                PI("cloud_dragon", "hp"),
                PI("cloud_dragon", "speed"),
                SimTargetMask.ANY,
                PI("cloud_dragon", "damage"),
                P("cloud_dragon", "attack_interval"),
                P("cloud_dragon", "attack_range"),
                PrefabType.WaterShot
            ));
            obj.AddComponent(new SimEffectReceiver());
            obj.AddComponent(new SimAreaEffectProvider(Fix64.One, P("cloud_dragon", "attack_range"), Effect.Wet));
        }

        private static void InitMagmaFist(SimGameObject obj)
        {
            obj.AddComponent(new SimCircleCollider(P("magma_fist", "radius"), true));
            obj.Element.Add(ElementType.FIRE);
            obj.Element.Add(ElementType.ROCK);
            obj.AddComponent(new SimEffectProvider(Effect.Burn));
            obj.AddComponent(new SimTimedSelfDestroyer(P("magma_fist", "duration")));
            obj.AddComponent(new SimOnStartAttacker(P("magma_fist", "radius"), PI("magma_fist", "damage")));
        }

        private static void InitVine(SimGameObject obj)
        {
            obj.AddComponent(new SimCircleCollider(P("vine", "radius"), true));
            obj.SetElement(ElementType.NATURE);
            obj.AddComponent(new SimEffectProvider(Effect.Snared));
            obj.AddComponent(new SimTimedSelfDestroyer(P("vine", "duration")));
            obj.AddComponent(new SimOnStartAttacker(P("vine", "radius"), PI("vine", "damage")));
        }

        private static void InitMeteorDrop(SimGameObject obj)
        {
            obj.AddComponent(new SimCircleCollider(P("meteor_drop", "radius"), true));
            obj.Element.Add(ElementType.FIRE);
            obj.Element.Add(ElementType.ROCK);
            obj.AddComponent(new SimDrop(PI("meteor_drop", "damage")));
        }

        private static void InitTideCall(SimGameObject obj)
        {
            obj.AddComponent(new SimCircleCollider(Fix64.FromInt(2), true));
            obj.SetElement(ElementType.WATER);
            obj.AddComponent(new SimEffectProvider(Effect.Wet));
            obj.AddComponent(new SimPushShot(PI("tide_call", "damage"), P("tide_call", "speed")));
            obj.AddComponent(new SimPathSpawner(PrefabType.WaterField, Fix64.Half));
            obj.AddComponent(new SimTimedSelfDestroyer(Fix64.FromInt(3)));
        }

        private static void InitGenericMob(SimGameObject obj)
        {
            // Fallback for unimplemented prefab types - provides basic stats to avoid errors
            obj.AddComponent(new SimRigidBody(1));
            obj.AddComponent(new SimZPhysics());
            obj.AddComponent(new SimCircleCollider(Fix64.One, false));
            obj.AddComponent(new SimAttackMob(
                10, // hp
                2,  // speed
                SimTargetMask.GROUND,
                1,  // damage
                Fix64.One, // interval
                Fix64.FromInt(2) // range
            ));
            obj.AddComponent(new SimEffectReceiver());
        }
    }
}
