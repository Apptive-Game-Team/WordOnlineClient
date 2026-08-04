using System;
using FixMath.NET;
using GameScene.Simulation.Core;

namespace GameScene.Simulation.Mob
{
    public enum MobArchetype : byte
    {
        Melee = 1,
        Slime = 2,
        ProjectileRange = 3,
        Dummy = 4,
        Totem = 5,
        Turret = 6,
        Tower = 7,
        LightningTower = 8,
        Summoner = 9,
        SelfDestruct = 10,
        KeepDistance = 11,
        PlayerPriorityMelee = 12,
        DoubleTarget = 13,
        Spraying = 14,
        Coward = 15,
        NonAttackingCoward = 16,
        ThunderBird = 17,
        BubbleSplash = 18,
        BubbleGenerator = 19,
        StationarySummoner = 20,
        DualAttack = 21,
        PveBoss = 22
    }

    [Flags]
    public enum MobTargetLayer : byte { Ground = 1, Air = 2, Any = Ground | Air }

    public interface IMobDefinitionResolver
    {
        bool TryResolve(string prefabId, out MobDefinition definition);
    }

    public enum MobBehaviorState : byte
    {
        Idle = 0, Chasing = 1, Attacking = 2, Dead = 3, Floating = 4, Diving = 5
    }

    public sealed class MobDefinition
    {
        public Fix64 Speed { get; }
        public Fix64 DetectionRange { get; }
        public Fix64 AttackRange { get; }
        public int Damage { get; }
        public int AttackCooldownFrames { get; }
        public int RepathIntervalFrames { get; }
        public int MaxHealth { get; }
        public Fix64 ColliderRadius { get; }
        public MobArchetype Archetype { get; }
        public string PathSpawnPrefabId { get; }
        public Fix64 PathSpawnDistance { get; }
        public string DeathSpawnPrefabId { get; }
        public int SelfHealAmount { get; }
        public int SelfHealIntervalFrames { get; }
        public int AreaHealthDelta { get; }
        public Fix64 AreaEffectRange { get; }
        public int AreaEffectIntervalFrames { get; }
        public int LifetimeFrames { get; }
        public int AttackDelayFrames { get; }
        public Fix64 SplashRadius { get; }
        public int ChainDamage { get; }
        public int ChainCount { get; }
        public Fix64 ChainRadius { get; }
        public string SummonPrefabId { get; }
        public Fix64 ExplosionRadius { get; }
        public string PeriodicSpawnPrefabId { get; }
        public string SecondaryPeriodicSpawnPrefabId { get; }
        public int PeriodicSpawnIntervalFrames { get; }
        public int PeriodicSpawnLimit { get; }
        public int PanicDurationFrames { get; }
        public int PanicCooldownFrames { get; }
        public Fix64 PanicSpeedMultiplier { get; }
        public bool IsAerial { get; }
        public MobTargetLayer TargetLayer { get; }
        public Fix64 ProjectileSpeed { get; }
        public Fix64 ManaChargePercent { get; }
        public int SecondaryDamage { get; }
        public Fix64 SecondaryAttackRange { get; }
        public int SecondaryAttackCooldownFrames { get; }
        public int SecondaryAttackDelayFrames { get; }
        public string TertiaryPeriodicSpawnPrefabId { get; }
        public int PeriodicSpawnBatchCount { get; }
        public bool SelectSinglePeriodicPrefab { get; }
        public string PrimaryBossMagicName { get; }
        public string SecondaryBossMagicName { get; }
        public string RageBossMagicName { get; }
        public int PeriodicSelfDamage { get; }
        public Fix64 PushForce { get; }
        public Fix64 PushRangeX { get; }
        public Fix64 PushRangeY { get; }
        public int SpawnDelayFrames { get; }
        public string AttackStatusId { get; }
        public string AuraStatusId { get; }
        public Fix64 AuraRange { get; }
        public int AuraIntervalFrames { get; }
        public Fix64 InitialHeight { get; }
        public Fix64 Gravity { get; }

        public MobDefinition(Fix64 speed, Fix64 detectionRange, Fix64 attackRange, int damage,
            int attackCooldownFrames, int repathIntervalFrames, int maxHealth = 100,
            Fix64 colliderRadius = default, MobArchetype archetype = MobArchetype.Melee,
            string pathSpawnPrefabId = null, Fix64 pathSpawnDistance = default,
            string deathSpawnPrefabId = null, int selfHealAmount = 0, int selfHealIntervalFrames = 0,
            int areaHealthDelta = 0, Fix64 areaEffectRange = default,
            int areaEffectIntervalFrames = 0, int lifetimeFrames = 0,
            int attackDelayFrames = 0, Fix64 splashRadius = default,
            int chainDamage = 0, int chainCount = 0, Fix64 chainRadius = default,
            string summonPrefabId = null, Fix64 explosionRadius = default,
            string periodicSpawnPrefabId = null, int periodicSpawnIntervalFrames = 0,
            int periodicSpawnLimit = 0, int panicDurationFrames = 0,
            int panicCooldownFrames = 0, Fix64 panicSpeedMultiplier = default,
            string secondaryPeriodicSpawnPrefabId = null, bool isAerial = false,
            MobTargetLayer targetLayer = MobTargetLayer.Any, Fix64 projectileSpeed = default,
            Fix64 manaChargePercent = default, int secondaryDamage = 0,
            Fix64 secondaryAttackRange = default, int secondaryAttackCooldownFrames = 0,
            int secondaryAttackDelayFrames = 0, string tertiaryPeriodicSpawnPrefabId = null,
            int periodicSpawnBatchCount = 1, bool selectSinglePeriodicPrefab = false,
            string primaryBossMagicName = null, string secondaryBossMagicName = null,
            string rageBossMagicName = null, int periodicSelfDamage = 0,
            Fix64 pushForce = default, Fix64 pushRangeX = default, Fix64 pushRangeY = default,
            int spawnDelayFrames = 0, string attackStatusId = null,
            string auraStatusId = null, Fix64 auraRange = default, int auraIntervalFrames = 0,
            Fix64 initialHeight = default, Fix64 gravity = default)
        {
            if (speed < Fix64.Zero) throw new ArgumentOutOfRangeException(nameof(speed));
            if (detectionRange < Fix64.Zero) throw new ArgumentOutOfRangeException(nameof(detectionRange));
            if (attackRange < Fix64.Zero) throw new ArgumentOutOfRangeException(nameof(attackRange));
            if (damage < 0) throw new ArgumentOutOfRangeException(nameof(damage));
            if (attackCooldownFrames <= 0) throw new ArgumentOutOfRangeException(nameof(attackCooldownFrames));
            if (repathIntervalFrames <= 0) throw new ArgumentOutOfRangeException(nameof(repathIntervalFrames));
            if (maxHealth <= 0) throw new ArgumentOutOfRangeException(nameof(maxHealth));
            if (colliderRadius < Fix64.Zero) throw new ArgumentOutOfRangeException(nameof(colliderRadius));
            if (pathSpawnDistance < Fix64.Zero || selfHealAmount < 0 || selfHealIntervalFrames < 0)
                throw new ArgumentOutOfRangeException(nameof(pathSpawnDistance));
            if (areaEffectRange < Fix64.Zero || areaEffectIntervalFrames < 0 || lifetimeFrames < 0 ||
                attackDelayFrames < 0 || splashRadius < Fix64.Zero)
                throw new ArgumentOutOfRangeException(nameof(areaEffectRange));
            if (chainDamage < 0 || chainCount < 0 || chainRadius < Fix64.Zero)
                throw new ArgumentOutOfRangeException(nameof(chainDamage));
            if (explosionRadius < Fix64.Zero) throw new ArgumentOutOfRangeException(nameof(explosionRadius));
            if (periodicSpawnIntervalFrames < 0 || periodicSpawnLimit < 0 || panicDurationFrames < 0 ||
                panicCooldownFrames < 0 || panicSpeedMultiplier < Fix64.Zero || projectileSpeed < Fix64.Zero ||
                manaChargePercent < Fix64.Zero)
                throw new ArgumentOutOfRangeException(nameof(periodicSpawnIntervalFrames));
            if (secondaryDamage < 0 || secondaryAttackRange < Fix64.Zero ||
                secondaryAttackCooldownFrames < 0 || secondaryAttackDelayFrames < 0)
                throw new ArgumentOutOfRangeException(nameof(secondaryDamage));
            if (periodicSpawnBatchCount < 1) throw new ArgumentOutOfRangeException(nameof(periodicSpawnBatchCount));
            if (periodicSelfDamage < 0) throw new ArgumentOutOfRangeException(nameof(periodicSelfDamage));
            if (pushForce < Fix64.Zero || pushRangeX < Fix64.Zero || pushRangeY < Fix64.Zero)
                throw new ArgumentOutOfRangeException(nameof(pushForce));
            if (spawnDelayFrames < 0) throw new ArgumentOutOfRangeException(nameof(spawnDelayFrames));
            if (auraRange < Fix64.Zero || auraIntervalFrames < 0)
                throw new ArgumentOutOfRangeException(nameof(auraRange));
            if ((auraStatusId == null) != (auraRange == Fix64.Zero) ||
                (auraStatusId == null) != (auraIntervalFrames == 0))
                throw new ArgumentException("Aura status, range, and interval must be configured together");
            if (initialHeight < Fix64.Zero || gravity < Fix64.Zero)
                throw new ArgumentOutOfRangeException(nameof(initialHeight));
            if ((secondaryDamage == 0) != (secondaryAttackRange == Fix64.Zero) ||
                (secondaryDamage == 0) != (secondaryAttackCooldownFrames == 0))
                throw new ArgumentException("Secondary attack damage, range, and cooldown must be configured together");
            if ((chainDamage == 0) != (chainCount == 0) || (chainDamage == 0) != (chainRadius == Fix64.Zero))
                throw new ArgumentException("Chain damage, count, and radius must be configured together");
            if (archetype == MobArchetype.Summoner && string.IsNullOrWhiteSpace(summonPrefabId))
                throw new ArgumentException("Summoner prefab is required", nameof(summonPrefabId));
            if (archetype == MobArchetype.SelfDestruct && explosionRadius <= Fix64.Zero)
                throw new ArgumentException("Self-destruct explosion radius is required", nameof(explosionRadius));
            if ((periodicSpawnPrefabId == null) != (periodicSpawnIntervalFrames == 0))
                throw new ArgumentException("Periodic spawn prefab and interval must be configured together");
            if (secondaryPeriodicSpawnPrefabId != null && periodicSpawnPrefabId == null)
                throw new ArgumentException("Primary periodic spawn prefab is required");
            if (tertiaryPeriodicSpawnPrefabId != null && secondaryPeriodicSpawnPrefabId == null)
                throw new ArgumentException("Secondary periodic spawn prefab is required");
            if (secondaryBossMagicName != null && primaryBossMagicName == null)
                throw new ArgumentException("Primary boss magic is required");
            if (rageBossMagicName != null && primaryBossMagicName == null)
                throw new ArgumentException("Primary boss magic is required for rage behavior");
            bool coward = archetype == MobArchetype.Coward || archetype == MobArchetype.NonAttackingCoward;
            if (coward && (panicDurationFrames == 0 || panicCooldownFrames == 0 || panicSpeedMultiplier == Fix64.Zero))
                throw new ArgumentException("Coward panic duration, cooldown, and speed multiplier are required");
            if ((pathSpawnPrefabId == null) != (pathSpawnDistance == Fix64.Zero))
                throw new ArgumentException("Path spawn prefab and distance must be configured together");
            if ((selfHealAmount == 0) != (selfHealIntervalFrames == 0))
                throw new ArgumentException("Self heal amount and interval must be configured together");
            if ((areaHealthDelta == 0) != (areaEffectIntervalFrames == 0) ||
                (areaHealthDelta == 0) != (areaEffectRange == Fix64.Zero))
                throw new ArgumentException("Area health effect amount, range, and interval must be configured together");
            Speed = speed; DetectionRange = detectionRange; AttackRange = attackRange; Damage = damage;
            AttackCooldownFrames = attackCooldownFrames; RepathIntervalFrames = repathIntervalFrames; MaxHealth = maxHealth;
            ColliderRadius = colliderRadius == Fix64.Zero ? (Fix64)0.5m : colliderRadius;
            Archetype = archetype;
            PathSpawnPrefabId = pathSpawnPrefabId;
            PathSpawnDistance = pathSpawnDistance;
            DeathSpawnPrefabId = deathSpawnPrefabId;
            SelfHealAmount = selfHealAmount;
            SelfHealIntervalFrames = selfHealIntervalFrames;
            AreaHealthDelta = areaHealthDelta;
            AreaEffectRange = areaEffectRange;
            AreaEffectIntervalFrames = areaEffectIntervalFrames;
            LifetimeFrames = lifetimeFrames;
            AttackDelayFrames = attackDelayFrames;
            SplashRadius = splashRadius;
            ChainDamage = chainDamage; ChainCount = chainCount; ChainRadius = chainRadius;
            SummonPrefabId = summonPrefabId; ExplosionRadius = explosionRadius;
            PeriodicSpawnPrefabId = periodicSpawnPrefabId;
            SecondaryPeriodicSpawnPrefabId = secondaryPeriodicSpawnPrefabId;
            PeriodicSpawnIntervalFrames = periodicSpawnIntervalFrames;
            PeriodicSpawnLimit = periodicSpawnLimit;
            PanicDurationFrames = panicDurationFrames;
            PanicCooldownFrames = panicCooldownFrames;
            PanicSpeedMultiplier = panicSpeedMultiplier;
            IsAerial = isAerial; TargetLayer = targetLayer; ProjectileSpeed = projectileSpeed;
            ManaChargePercent = manaChargePercent;
            SecondaryDamage = secondaryDamage; SecondaryAttackRange = secondaryAttackRange;
            SecondaryAttackCooldownFrames = secondaryAttackCooldownFrames;
            SecondaryAttackDelayFrames = secondaryAttackDelayFrames;
            TertiaryPeriodicSpawnPrefabId = tertiaryPeriodicSpawnPrefabId;
            PeriodicSpawnBatchCount = periodicSpawnBatchCount;
            SelectSinglePeriodicPrefab = selectSinglePeriodicPrefab;
            PrimaryBossMagicName = primaryBossMagicName;
            SecondaryBossMagicName = secondaryBossMagicName;
            RageBossMagicName = rageBossMagicName;
            PeriodicSelfDamage = periodicSelfDamage;
            PushForce = pushForce; PushRangeX = pushRangeX; PushRangeY = pushRangeY;
            SpawnDelayFrames = spawnDelayFrames;
            AttackStatusId = attackStatusId;
            AuraStatusId = auraStatusId; AuraRange = auraRange; AuraIntervalFrames = auraIntervalFrames;
            InitialHeight = initialHeight; Gravity = gravity == Fix64.Zero ? (Fix64)4 : gravity;
        }

        internal void WriteState(CanonicalStateWriter writer)
        {
            writer.WriteFixed64(Speed); writer.WriteFixed64(DetectionRange); writer.WriteFixed64(AttackRange);
            writer.WriteInt32(Damage); writer.WriteInt32(AttackCooldownFrames);
            writer.WriteInt32(RepathIntervalFrames); writer.WriteInt32(MaxHealth);
            writer.WriteFixed64(ColliderRadius);
            writer.WriteInt32((int)Archetype);
            writer.WriteString(PathSpawnPrefabId); writer.WriteFixed64(PathSpawnDistance);
            writer.WriteString(DeathSpawnPrefabId); writer.WriteInt32(SelfHealAmount);
            writer.WriteInt32(SelfHealIntervalFrames);
            writer.WriteInt32(AreaHealthDelta); writer.WriteFixed64(AreaEffectRange);
            writer.WriteInt32(AreaEffectIntervalFrames); writer.WriteInt32(LifetimeFrames);
            writer.WriteInt32(AttackDelayFrames); writer.WriteFixed64(SplashRadius);
            writer.WriteInt32(ChainDamage); writer.WriteInt32(ChainCount); writer.WriteFixed64(ChainRadius);
            writer.WriteString(SummonPrefabId); writer.WriteFixed64(ExplosionRadius);
            writer.WriteString(PeriodicSpawnPrefabId); writer.WriteString(SecondaryPeriodicSpawnPrefabId);
            writer.WriteInt32(PeriodicSpawnIntervalFrames);
            writer.WriteInt32(PeriodicSpawnLimit);
            writer.WriteInt32(PanicDurationFrames); writer.WriteInt32(PanicCooldownFrames);
            writer.WriteFixed64(PanicSpeedMultiplier);
            writer.WriteBoolean(IsAerial); writer.WriteInt32((int)TargetLayer);
            writer.WriteFixed64(ProjectileSpeed);
            writer.WriteFixed64(ManaChargePercent);
            writer.WriteInt32(SecondaryDamage); writer.WriteFixed64(SecondaryAttackRange);
            writer.WriteInt32(SecondaryAttackCooldownFrames); writer.WriteInt32(SecondaryAttackDelayFrames);
            writer.WriteString(TertiaryPeriodicSpawnPrefabId); writer.WriteInt32(PeriodicSpawnBatchCount);
            writer.WriteBoolean(SelectSinglePeriodicPrefab);
            writer.WriteString(PrimaryBossMagicName); writer.WriteString(SecondaryBossMagicName);
            writer.WriteString(RageBossMagicName);
            writer.WriteInt32(PeriodicSelfDamage);
            writer.WriteFixed64(PushForce); writer.WriteFixed64(PushRangeX); writer.WriteFixed64(PushRangeY);
            writer.WriteInt32(SpawnDelayFrames);
            writer.WriteString(AttackStatusId);
            writer.WriteString(AuraStatusId); writer.WriteFixed64(AuraRange);
            writer.WriteInt32(AuraIntervalFrames);
            writer.WriteFixed64(InitialHeight); writer.WriteFixed64(Gravity);
        }
    }
}
