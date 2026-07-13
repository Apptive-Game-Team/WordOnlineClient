using System;
using System.Collections.Generic;
using FixMath.NET;
using GameScene.Simulation.Core;
using GameScene.Simulation.Physics;

namespace GameScene.Simulation.Magic
{
    public readonly struct MagicCastCommand
    {
        public long UserId { get; }
        public int RequestId { get; }
        public int MagicId { get; }
        public SimVector2 Target { get; }

        public MagicCastCommand(long userId, int requestId, int magicId, SimVector2 target)
        { UserId = userId; RequestId = requestId; MagicId = magicId; Target = target; }
    }

    public sealed class MagicCombatState
    {
        private readonly SortedDictionary<string, int> statuses =
            new SortedDictionary<string, int>(StringComparer.Ordinal);
        public int EntityId { get; }
        public int Health { get; private set; } = 100;
        public IReadOnlyDictionary<string, int> Statuses => statuses;
        internal MagicCombatState(int entityId, int health)
        {
            if (health < 1) throw new ArgumentOutOfRangeException(nameof(health));
            EntityId = entityId; Health = health;
        }
        internal void Damage(int amount) { Health = Math.Max(0, Health - amount); }
        internal void ApplyStatus(string id, int frames)
        {
            if (string.IsNullOrEmpty(id) || frames <= 0) return;
            if (!statuses.TryGetValue(id, out int current) || current < frames) statuses[id] = frames;
        }
        internal void Step()
        {
            List<string> keys = new List<string>(statuses.Keys);
            for (int index = 0; index < keys.Count; index++)
            {
                int remaining = statuses[keys[index]] - 1;
                if (remaining <= 0) statuses.Remove(keys[index]); else statuses[keys[index]] = remaining;
            }
        }
        internal void WriteState(CanonicalStateWriter writer)
        {
            writer.WriteInt32(EntityId); writer.WriteInt32(Health); writer.WriteInt32(statuses.Count);
            foreach (KeyValuePair<string, int> pair in statuses)
            { writer.WriteString(pair.Key); writer.WriteInt32(pair.Value); }
        }
    }

    public sealed class MagicSimulation
    {
        private sealed class Instance
        {
            public int EntityId;
            public long OwnerUserId;
            public MagicDefinition Definition;
            public int RemainingFrames;
        }

        private readonly long sessionSeed;
        private readonly SimulationWorld world;
        private readonly ProductionMagicCatalog catalog;
        private readonly List<Instance> instances = new List<Instance>();
        private readonly SortedDictionary<int, MagicCombatState> combatants =
            new SortedDictionary<int, MagicCombatState>();

        public SimulationWorld World => world;
        public IReadOnlyDictionary<int, MagicCombatState> Combatants => combatants;

        public MagicSimulation(long sessionSeed, SimulationWorld world, ProductionMagicCatalog catalog)
        {
            this.sessionSeed = sessionSeed;
            this.world = world ?? throw new ArgumentNullException(nameof(world));
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            RegisterCombatants();
        }

        public void RegisterCombatant(int entityId, int health = 100)
        {
            if (entityId < 0 || entityId >= world.Entities.Count || world.Entities[entityId].IsDestroyed)
                throw new InvalidOperationException("Cannot register missing combatant: " + entityId);
            if (!combatants.ContainsKey(entityId)) combatants.Add(entityId, new MagicCombatState(entityId, health));
        }

        public void Step(IReadOnlyList<MagicCastCommand> casts)
        {
            if (casts == null) throw new ArgumentNullException(nameof(casts));
            RegisterCombatants();
            List<MagicCastCommand> ordered = new List<MagicCastCommand>(casts);
            ordered.Sort((left, right) =>
            {
                int user = left.UserId.CompareTo(right.UserId);
                return user != 0 ? user : left.RequestId.CompareTo(right.RequestId);
            });
            for (int index = 1; index < ordered.Count; index++)
                if (ordered[index - 1].UserId == ordered[index].UserId &&
                    ordered[index - 1].RequestId == ordered[index].RequestId)
                    throw new InvalidOperationException("Duplicate magic cast request: " + ordered[index].RequestId);
            for (int index = 0; index < ordered.Count; index++) Cast(ordered[index]);

            world.Step(Array.Empty<SimulationInput>());
            ApplyProjectileHits();
            TickInstances();
            foreach (MagicCombatState state in combatants.Values) state.Step();
        }

        public ulong CalculateStateHash()
        {
            CanonicalStateWriter writer = new CanonicalStateWriter();
            writer.WriteUInt64(world.CalculateStateHash());
            writer.WriteInt64(sessionSeed);
            writer.WriteInt32(combatants.Count);
            foreach (MagicCombatState state in combatants.Values) state.WriteState(writer);
            instances.Sort((left, right) => left.EntityId.CompareTo(right.EntityId));
            writer.WriteInt32(instances.Count);
            for (int index = 0; index < instances.Count; index++)
            {
                Instance instance = instances[index];
                writer.WriteInt32(instance.EntityId); writer.WriteInt64(instance.OwnerUserId);
                writer.WriteInt32(instance.Definition.Id); writer.WriteInt32(instance.RemainingFrames);
            }
            return writer.Hash;
        }

        private void Cast(MagicCastCommand command)
        {
            MagicDefinition definition = catalog.GetRequired(command.MagicId);
            SimulationEntity caster = FindCaster(command.UserId);
            SimVector2 target = ClampTarget(caster.Position, command.Target, definition.Range);
            DeterministicRandom random = new DeterministicRandom(DeriveSeed(command, definition.Id));
            for (int index = 0; index < definition.SpawnCount; index++)
            {
                SimVector2 position = definition.Family == MagicBehaviorFamily.Projectile
                    ? caster.Position : AddStableOffset(target, random, definition.SpawnCount > 1);
                SimulationEntity entity = world.Spawn(command.UserId, position);
                if (definition.Family == MagicBehaviorFamily.Projectile)
                    entity.SetVelocity(Direction(caster.Position, target) * definition.Speed);
                instances.Add(new Instance
                {
                    EntityId = entity.Id, OwnerUserId = command.UserId,
                    Definition = definition, RemainingFrames = definition.LifetimeFrames
                });
            }
            if (definition.Family == MagicBehaviorFamily.Area)
                ApplyArea(command.UserId, target, definition);
        }

        private void ApplyArea(long ownerUserId, SimVector2 target, MagicDefinition definition)
        {
            Fix64 radiusSquared = definition.Radius * definition.Radius;
            foreach (KeyValuePair<int, MagicCombatState> pair in combatants)
            {
                SimulationEntity entity = world.Entities[pair.Key];
                if (entity.IsDestroyed || entity.OwnerUserId == ownerUserId) continue;
                SimVector2 delta = new SimVector2(entity.Position.X - target.X, entity.Position.Y - target.Y);
                if (delta.X * delta.X + delta.Y * delta.Y > radiusSquared) continue;
                pair.Value.Damage(definition.Damage);
                pair.Value.ApplyStatus(definition.StatusId, definition.StatusFrames);
            }
        }

        private void ApplyProjectileHits()
        {
            for (int eventIndex = 0; eventIndex < world.CollisionEvents.Count; eventIndex++)
            {
                SimulationCollisionEvent collision = world.CollisionEvents[eventIndex];
                if (collision.Type != SimulationCollisionEventType.Enter) continue;
                HitProjectile(collision.EntityA, collision.EntityB);
                HitProjectile(collision.EntityB, collision.EntityA);
            }
        }

        private void HitProjectile(int projectileId, int targetId)
        {
            Instance instance = FindInstance(projectileId);
            if (instance == null || instance.Definition.Family != MagicBehaviorFamily.Projectile) return;
            if (!combatants.TryGetValue(targetId, out MagicCombatState target) ||
                world.Entities[targetId].OwnerUserId == instance.OwnerUserId) return;
            target.Damage(instance.Definition.Damage);
            target.ApplyStatus(instance.Definition.StatusId, instance.Definition.StatusFrames);
            world.Destroy(projectileId); instance.RemainingFrames = 0;
        }

        private void TickInstances()
        {
            instances.Sort((left, right) => left.EntityId.CompareTo(right.EntityId));
            for (int index = instances.Count - 1; index >= 0; index--)
            {
                Instance instance = instances[index];
                instance.RemainingFrames--;
                if (instance.RemainingFrames > 0 && !world.Entities[instance.EntityId].IsDestroyed) continue;
                world.Destroy(instance.EntityId);
                instances.RemoveAt(index);
            }
        }

        private void RegisterCombatants()
        {
            for (int index = 0; index < world.Entities.Count; index++)
            {
                SimulationEntity entity = world.Entities[index];
                if (!entity.IsDestroyed && entity.PrefabId == Objects.SimulationPrefabRegistry.PlayerPrefabId &&
                    !combatants.ContainsKey(entity.Id)) combatants.Add(entity.Id, new MagicCombatState(entity.Id, 100));
            }
        }

        private SimulationEntity FindCaster(long userId)
        {
            for (int index = 0; index < world.Entities.Count; index++)
            {
                SimulationEntity entity = world.Entities[index];
                if (!entity.IsDestroyed && entity.OwnerUserId == userId &&
                    entity.PrefabId == Objects.SimulationPrefabRegistry.PlayerPrefabId) return entity;
            }
            throw new InvalidOperationException("Magic caster not found: " + userId);
        }

        private Instance FindInstance(int entityId)
        {
            for (int index = 0; index < instances.Count; index++)
                if (instances[index].EntityId == entityId) return instances[index];
            return null;
        }

        private long DeriveSeed(MagicCastCommand command, int magicId)
        {
            ulong hash = 14695981039346656037UL;
            hash = Mix(hash, unchecked((ulong)sessionSeed)); hash = Mix(hash, unchecked((ulong)command.UserId));
            hash = Mix(hash, unchecked((uint)command.RequestId)); hash = Mix(hash, unchecked((uint)magicId));
            return unchecked((long)hash);
        }

        private static ulong Mix(ulong hash, ulong value)
        {
            for (int index = 0; index < 8; index++) hash = unchecked((hash ^ (byte)(value >> (index * 8))) * 1099511628211UL);
            return hash;
        }

        private static SimVector2 AddStableOffset(SimVector2 value, DeterministicRandom random, bool enabled)
        {
            if (!enabled) return value;
            Fix64 x = (Fix64)(random.NextInt(2001) - 1000) / (Fix64)1000;
            Fix64 y = (Fix64)(random.NextInt(2001) - 1000) / (Fix64)1000;
            return new SimVector2(value.X + x, value.Y + y);
        }

        private static SimVector2 Direction(SimVector2 origin, SimVector2 target)
        {
            Fix64 x = target.X - origin.X; Fix64 y = target.Y - origin.Y;
            Fix64 length = Fix64.Sqrt(x * x + y * y);
            return length == Fix64.Zero ? SimVector2.Zero : new SimVector2(x / length, y / length);
        }

        private static SimVector2 ClampTarget(SimVector2 origin, SimVector2 target, Fix64 range)
        {
            Fix64 x = Clamp(target.X, Fix64.Zero, (Fix64)18);
            Fix64 y = Clamp(target.Y, Fix64.Zero, (Fix64)10);
            SimVector2 bounded = new SimVector2(x, y);
            SimVector2 direction = Direction(origin, bounded);
            Fix64 dx = bounded.X - origin.X; Fix64 dy = bounded.Y - origin.Y;
            Fix64 distance = Fix64.Sqrt(dx * dx + dy * dy);
            return distance <= range ? bounded : origin + direction * range;
        }

        private static Fix64 Clamp(Fix64 value, Fix64 minimum, Fix64 maximum)
        {
            if (value < minimum) return minimum;
            return value > maximum ? maximum : value;
        }
    }
}
