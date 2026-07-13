using System;
using FixMath.NET;
using GameScene.Simulation.Core;

namespace GameScene.Simulation.Mob
{
    public enum MobBehaviorState : byte { Idle = 0, Chasing = 1, Attacking = 2, Dead = 3 }

    public sealed class MobDefinition
    {
        public Fix64 Speed { get; }
        public Fix64 DetectionRange { get; }
        public Fix64 AttackRange { get; }
        public int Damage { get; }
        public int AttackCooldownFrames { get; }
        public int RepathIntervalFrames { get; }
        public int MaxHealth { get; }

        public MobDefinition(Fix64 speed, Fix64 detectionRange, Fix64 attackRange, int damage,
            int attackCooldownFrames, int repathIntervalFrames, int maxHealth = 100)
        {
            if (speed < Fix64.Zero) throw new ArgumentOutOfRangeException(nameof(speed));
            if (detectionRange < Fix64.Zero) throw new ArgumentOutOfRangeException(nameof(detectionRange));
            if (attackRange < Fix64.Zero) throw new ArgumentOutOfRangeException(nameof(attackRange));
            if (damage < 0) throw new ArgumentOutOfRangeException(nameof(damage));
            if (attackCooldownFrames <= 0) throw new ArgumentOutOfRangeException(nameof(attackCooldownFrames));
            if (repathIntervalFrames <= 0) throw new ArgumentOutOfRangeException(nameof(repathIntervalFrames));
            if (maxHealth <= 0) throw new ArgumentOutOfRangeException(nameof(maxHealth));
            Speed = speed; DetectionRange = detectionRange; AttackRange = attackRange; Damage = damage;
            AttackCooldownFrames = attackCooldownFrames; RepathIntervalFrames = repathIntervalFrames; MaxHealth = maxHealth;
        }

        internal void WriteState(CanonicalStateWriter writer)
        {
            writer.WriteFixed64(Speed); writer.WriteFixed64(DetectionRange); writer.WriteFixed64(AttackRange);
            writer.WriteInt32(Damage); writer.WriteInt32(AttackCooldownFrames);
            writer.WriteInt32(RepathIntervalFrames); writer.WriteInt32(MaxHealth);
        }
    }
}
