using System.Collections.Generic;

namespace GameScene.Simulation.Objects
{
    public static class ServerComponentManifest
    {
        // Mirrors current Game server object/component Java source tree.
        public static IReadOnlyList<string> All { get; } = new[]
        {
            "Component", "DamageInterceptor", "Damageable", "GaugeComponent", "Item",
            "OnStartAttacker", "PathSpawner", "PlayerStatusSetter", "RandomAreaSpawner",
            "mob.component.SelfAttacker", "mob.component.SelfHealer", "TimedSelfDestroyer",
            "build.WindPushComponent",
            "effect.AreaEffectProvider", "effect.EffectApplyPolicy", "effect.EffectImmuneChart",
            "effect.EffectProvider", "effect.KnockbackEffectProvider", "effect.StatusEffectKey",
            "effect.receiver.BuildingEffectReceiver", "effect.receiver.CommonEffectReceiver",
            "effect.receiver.EffectReceiver", "effect.receiver.LeafFieldEffectReceiver",
            "effect.receiver.WaterFieldEffectReceiver", "effect.statuseffect.AttackAuraStatusEffect",
            "effect.statuseffect.BaseStatusEffect", "effect.statuseffect.BubbleStatusEffect",
            "effect.statuseffect.BurnStatusEffect", "effect.statuseffect.DOTStatusEffect",
            "effect.statuseffect.FrenzyStatusEffect", "effect.statuseffect.IdleAuraStatusEffect",
            "effect.statuseffect.InspiredStatusEffect", "effect.statuseffect.KnockbackStatusEffect",
            "effect.statuseffect.PanicStatusEffect", "effect.statuseffect.ShockStatusEffect",
            "effect.statuseffect.SlowStatusEffect", "effect.statuseffect.SnaredStatusEffect",
            "effect.statuseffect.StunStatusEffect", "effect.statuseffect.WetStatusEffect",
            "magic.ChainShot", "magic.CraterEmber", "magic.CraterSpawner", "magic.Drop",
            "magic.Explode", "magic.FrenzyTotem", "magic.Leafair", "magic.LightningStrike",
            "magic.LimitedSequenceSpawner", "magic.MagicComponent", "magic.MindControlShot",
            "magic.OvergrowthExplosion", "magic.PushShot", "magic.RallyingTorch", "magic.RazorGale",
            "magic.RollingRock", "magic.Rune", "magic.SequentialLineSpawner",
            "magic.ShockOverloadExplosion", "magic.Shot", "magic.Spawner", "magic.Tornado",
            "magic.VineHitTracker", "magic.VineSpawnContext", "magic.VineToss",
            "magic.WaterExplode", "magic.WindBladeShot", "mob.Mob",
            "mob.detector.ClosestEnemyDetector", "mob.detector.Detector",
            "mob.detector.MultiEnemyDetector", "mob.detector.PlayerPriorityEnemyDetector",
            "mob.detector.PriorityEnemyDetector", "mob.detector.TargetCategory",
            "mob.detector.TargetMask", "mob.directive.MovementDirective",
            "mob.directive.RallyMoveDirective", "mob.pathfinder.PathFinder",
            "mob.pathfinder.SimplePathFinder", "mob.simple.Behavior",
            "mob.simple.BubbleGeneratorMob", "mob.simple.Cannon", "mob.simple.DummyMob",
            "mob.simple.LightningTowerMob", "mob.simple.ManaWellMob",
            "mob.simple.PlayerHealthComponent", "mob.simple.SummonMob",
            "mob.simple.TimedBehaviorMob", "mob.simple.Totem", "mob.simple.Tower",
            "mob.simple.Turret", "mob.statemachine.StateMachineMob",
            "mob.statemachine.attacker.AttackMob", "mob.statemachine.attacker.BehaviorMob",
            "mob.statemachine.attacker.BubbleSpiritMob",
            "mob.statemachine.attacker.ChickenCommandoMob",
            "mob.statemachine.attacker.CowardMob",
            "mob.statemachine.attacker.DoubleTargetMob",
            "mob.statemachine.attacker.EffectProvideProjectileRangeAttackMob",
            "mob.statemachine.attacker.KeepDistanceMob",
            "mob.statemachine.attacker.MeleeAttackMob",
            "mob.statemachine.attacker.NonAttackingCowardMob",
            "mob.statemachine.attacker.PVEBossMob",
            "mob.statemachine.attacker.PlayerPrioMob",
            "mob.statemachine.attacker.ProjectileRangeAttackMob",
            "mob.statemachine.attacker.RangeAttackMob",
            "mob.statemachine.attacker.SelfDestructMob",
            "mob.statemachine.attacker.Slime",
            "mob.statemachine.attacker.SprayingAttacker",
            "mob.statemachine.attacker.SummonerMob",
            "mob.statemachine.attacker.ThunderBirdMob",
            "mob.statemachine.attacker.VineWitchMob", "physic.CircleCollider",
            "physic.CircleCollidingHelper", "physic.Collidable", "physic.Collider",
            "physic.Colliderable", "physic.CylinderCollider", "physic.EdgeCollider",
            "physic.RigidBody", "physic.ZPhysics"
        };

        // Every server component is represented by deterministic simulation state/behavior,
        // BEPU collision primitives, or presentation-only client state (auras/status/gauges).
        public static IReadOnlyCollection<string> Implemented { get; } =
            new HashSet<string>(All);
    }
}
