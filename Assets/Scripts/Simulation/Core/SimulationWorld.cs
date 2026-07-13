using System;
using System.Collections.Generic;
using BEPUphysics;
using BEPUutilities;
using FixMath.NET;
using GameScene.Simulation.Objects;
using GameScene.Simulation.Protocol;
using GameScene.Simulation.Physics;

namespace GameScene.Simulation.Core
{
    /// <summary>Minimal deterministic vertical slice; gameplay systems build on this boundary.</summary>
    public sealed class SimulationWorld
    {
        public static readonly Fix64 FixedDeltaTime = (Fix64)(1m / 20m);

        private readonly List<SimulationEntity> entities = new List<SimulationEntity>();
        private readonly DeterministicRandom random;
        private readonly Space physicsSpace;
        private readonly SimulationPrefabRegistry prefabRegistry;
        private readonly SimulationBounds bounds;
        private readonly List<SimulationCollisionEvent> collisionEvents = new List<SimulationCollisionEvent>();
        private int nextEntityId;

        public int FrameNumber { get; private set; }
        public IReadOnlyList<SimulationEntity> Entities => entities;
        public ulong RandomDrawCount => random.DrawCount;
        public IReadOnlyList<SimulationCollisionEvent> CollisionEvents => collisionEvents;

        public SimulationWorld(long seed) : this(seed, SimulationPrefabRegistry.CreateCore(), SimulationBounds.Default)
        {
        }

        public SimulationWorld(long seed, SimulationPrefabRegistry prefabRegistry)
            : this(seed, prefabRegistry, SimulationBounds.Default)
        {
        }

        public SimulationWorld(
            long seed,
            SimulationPrefabRegistry prefabRegistry,
            SimulationBounds bounds)
        {
            this.prefabRegistry = prefabRegistry ?? throw new ArgumentNullException(nameof(prefabRegistry));
            this.bounds = bounds;
            random = new DeterministicRandom(seed);
            physicsSpace = new Space(null);
            physicsSpace.TimeStepSettings.TimeStepDuration = FixedDeltaTime;
            physicsSpace.ForceUpdater.Gravity = Vector3.Zero;
        }

        public SimulationEntity Spawn(long ownerUserId, SimVector2 position)
        {
            return Spawn(SimulationPrefabRegistry.DefaultPrefabId, ownerUserId, position);
        }

        public SimulationEntity Spawn(string prefabId, long ownerUserId, SimVector2 position)
        {
            return SpawnWithId(nextEntityId, prefabId, ownerUserId, position);
        }

        public SimulationEntity SpawnWithId(int entityId, string prefabId, long ownerUserId, SimVector2 position)
        {
            SimulationPrefabDefinition prefab = prefabRegistry.GetRequired(prefabId);
            if (entityId < nextEntityId)
            {
                SimulationEntity existing = FindRequired(entityId);
                if (!existing.IsDestroyed &&
                    existing.OwnerUserId == ownerUserId &&
                    string.Equals(existing.PrefabId, prefabId, StringComparison.Ordinal) &&
                    existing.Position.Equals(position))
                    return existing;
                throw new InvalidOperationException("Conflicting simulation spawn for entity: " + entityId);
            }
            if (entityId != nextEntityId)
                throw new InvalidOperationException("Simulation entity ID must be next expected ID " + nextEntityId + ": " + entityId);

            SimulationEntity entity = new SimulationEntity(entityId, ownerUserId, position, prefab, FrameNumber);
            entities.Add(entity);
            nextEntityId = checked(nextEntityId + 1);
            entity.Body.CollisionInformation.Tag = entityId;
            entity.Body.CollisionInformation.Events.InitialCollisionDetected +=
                (sender, other, pair) => RecordCollision(entityId, other.Tag, SimulationCollisionEventType.Enter);
            entity.Body.CollisionInformation.Events.CollisionEnded +=
                (sender, other, pair) => RecordCollision(entityId, other.Tag, SimulationCollisionEventType.Exit);
            physicsSpace.Add(entity.Body);
            return entity;
        }

        public void ApplyBootstrap(LockstepSessionStartMessage sessionStart)
        {
            if (sessionStart == null) throw new ArgumentNullException(nameof(sessionStart));
            IReadOnlyList<BootstrapEventMessage> bootstrapEvents = sessionStart.bootstrapEvents;
            if (bootstrapEvents == null) throw new ArgumentNullException(nameof(bootstrapEvents));
            if (FrameNumber != 0 || entities.Count != 0)
                throw new InvalidOperationException("Bootstrap can only be applied to an empty initial world");

            List<BootstrapEventMessage> orderedEvents = new List<BootstrapEventMessage>(bootstrapEvents.Count);
            for (int index = 0; index < bootstrapEvents.Count; index++)
            {
                if (bootstrapEvents[index] == null) throw new ArgumentException("Bootstrap event cannot be null");
                orderedEvents.Add(bootstrapEvents[index]);
            }
            orderedEvents.Sort((left, right) => left.sequence.CompareTo(right.sequence));
            for (int index = 1; index < orderedEvents.Count; index++)
            {
                if (orderedEvents[index - 1].sequence == orderedEvents[index].sequence)
                    throw new InvalidOperationException("Duplicate bootstrap sequence: " + orderedEvents[index].sequence);
            }
            for (int index = 0; index < orderedEvents.Count; index++)
                ValidateBootstrapEvent(orderedEvents[index], sessionStart);
            for (int index = 0; index < orderedEvents.Count; index++)
                ApplyBootstrapEvent(orderedEvents[index], sessionStart);
        }

        public bool Destroy(int entityId)
        {
            SimulationEntity entity = FindRequired(entityId);
            if (entity.IsDestroyed) return false;
            physicsSpace.Remove(entity.Body);
            entity.Destroy(FrameNumber);
            return true;
        }

        public void Step(IReadOnlyList<SimulationInput> confirmedInputs)
        {
            if (confirmedInputs == null) throw new ArgumentNullException(nameof(confirmedInputs));
            collisionEvents.Clear();
            List<SimulationInput> orderedInputs = new List<SimulationInput>(confirmedInputs.Count);
            for (int index = 0; index < confirmedInputs.Count; index++)
            {
                orderedInputs.Add(confirmedInputs[index]);
            }

            orderedInputs.Sort(CompareInputs);
            for (int index = 1; index < orderedInputs.Count; index++)
            {
                SimulationInput previous = orderedInputs[index - 1];
                SimulationInput current = orderedInputs[index];
                if (previous.UserId == current.UserId && previous.Sequence == current.Sequence)
                {
                    throw new InvalidOperationException(
                        "Duplicate confirmed input sequence for user " + current.UserId + ": " + current.Sequence);
                }
            }

            for (int index = 0; index < orderedInputs.Count; index++)
            {
                Apply(orderedInputs[index]);
            }

            // Single-threaded fixed BEPU step. Passing null to Space prevents a parallel looper.
            physicsSpace.Update();

            for (int index = 0; index < entities.Count; index++)
            {
                if (!entities[index].IsDestroyed) entities[index].ConstrainTo(bounds.Min, bounds.Max);
            }
            NormalizeCollisionEvents();

            FrameNumber = checked(FrameNumber + 1);
        }

        public uint DrawRandomUInt32() => random.NextUInt32();

        public ulong CalculateStateHash()
        {
            CanonicalStateWriter writer = new CanonicalStateWriter();
            writer.WriteInt32(FrameNumber);
            writer.WriteInt32(nextEntityId);
            random.WriteState(writer);
            bounds.WriteState(writer);
            writer.WriteInt32(entities.Count);
            for (int index = 0; index < entities.Count; index++)
            {
                entities[index].WriteState(writer);
            }
            writer.WriteInt32(collisionEvents.Count);
            for (int index = 0; index < collisionEvents.Count; index++) collisionEvents[index].WriteState(writer);

            return writer.Hash;
        }

        private void RecordCollision(
            int entityId,
            object otherTag,
            SimulationCollisionEventType type)
        {
            if (!(otherTag is int otherEntityId) || entityId == otherEntityId) return;
            bool isTrigger = entities[entityId].IsTrigger || entities[otherEntityId].IsTrigger;
            collisionEvents.Add(new SimulationCollisionEvent(
                FrameNumber, entityId, otherEntityId, type, isTrigger));
        }

        private void NormalizeCollisionEvents()
        {
            collisionEvents.Sort((left, right) =>
            {
                int a = left.EntityA.CompareTo(right.EntityA);
                if (a != 0) return a;
                int b = left.EntityB.CompareTo(right.EntityB);
                if (b != 0) return b;
                return left.Type.CompareTo(right.Type);
            });
            for (int index = collisionEvents.Count - 1; index > 0; index--)
            {
                SimulationCollisionEvent left = collisionEvents[index - 1];
                SimulationCollisionEvent right = collisionEvents[index];
                if (left.EntityA == right.EntityA && left.EntityB == right.EntityB && left.Type == right.Type)
                    collisionEvents.RemoveAt(index);
            }
        }

        private void Apply(SimulationInput input)
        {
            switch (input.Type)
            {
                case SimulationInputType.SetVelocity:
                    FindRequired(input.EntityId).SetVelocity(input.Value);
                    break;
                case SimulationInputType.Spawn:
                    Spawn(input.UserId, input.Value);
                    break;
                case SimulationInputType.Destroy:
                    Destroy(input.EntityId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(input), input.Type, "Unknown input type");
            }
        }

        private void ApplyBootstrapEvent(BootstrapEventMessage bootstrapEvent, LockstepSessionStartMessage sessionStart)
        {
            Spawn(
                SimulationPrefabRegistry.PlayerPrefabId,
                BootstrapOwnerUserId(bootstrapEvent, sessionStart),
                new SimVector2((Fix64)(decimal)bootstrapEvent.position.x, (Fix64)(decimal)bootstrapEvent.position.z));
        }

        private void ValidateBootstrapEvent(
            BootstrapEventMessage bootstrapEvent,
            LockstepSessionStartMessage sessionStart)
        {
            if (!string.Equals(bootstrapEvent.type, "SPAWN_PLAYER", StringComparison.Ordinal))
                throw new InvalidOperationException("Unsupported bootstrap event: " + bootstrapEvent.type);
            if (bootstrapEvent.position == null)
                throw new InvalidOperationException("Bootstrap position is required for sequence " + bootstrapEvent.sequence);
            BootstrapOwnerUserId(bootstrapEvent, sessionStart);
            prefabRegistry.GetRequired(SimulationPrefabRegistry.PlayerPrefabId);
        }

        private static long BootstrapOwnerUserId(
            BootstrapEventMessage bootstrapEvent,
            LockstepSessionStartMessage sessionStart)
        {
            if (string.Equals(bootstrapEvent.master, "LeftPlayer", StringComparison.Ordinal))
                return sessionStart.leftUserId;
            if (string.Equals(bootstrapEvent.master, "RightPlayer", StringComparison.Ordinal))
                return sessionStart.rightUserId;
            throw new InvalidOperationException("Unsupported bootstrap master: " + bootstrapEvent.master);
        }

        private SimulationEntity FindRequired(int entityId)
        {
            if (entityId < 0 || entityId >= entities.Count || entities[entityId].Id != entityId)
            {
                throw new InvalidOperationException("Unknown simulation entity: " + entityId);
            }

            return entities[entityId];
        }

        private static int CompareInputs(SimulationInput left, SimulationInput right)
        {
            int userOrder = left.UserId.CompareTo(right.UserId);
            if (userOrder != 0)
            {
                return userOrder;
            }

            int sequenceOrder = left.Sequence.CompareTo(right.Sequence);
            if (sequenceOrder != 0)
            {
                return sequenceOrder;
            }

            // Duplicate sequence numbers are invalid at protocol validation. This tie-breaker
            // still prevents arrival order from becoming simulation order.
            int typeOrder = left.Type.CompareTo(right.Type);
            if (typeOrder != 0)
            {
                return typeOrder;
            }

            return left.EntityId.CompareTo(right.EntityId);
        }
    }
}
