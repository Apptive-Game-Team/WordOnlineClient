using System;
using System.Collections.Generic;

namespace WordOnline.Simulation.Core
{
    /// <summary>Minimal deterministic vertical slice; gameplay systems build on this boundary.</summary>
    public sealed class SimulationWorld
    {
        public static readonly Fixed64 FixedDeltaTime = Fixed64.FromRatio(1, 20);

        private readonly List<SimulationEntity> entities = new List<SimulationEntity>();
        private readonly DeterministicRandom random;
        private int nextEntityId;

        public int FrameNumber { get; private set; }
        public IReadOnlyList<SimulationEntity> Entities => entities;
        public ulong RandomDrawCount => random.DrawCount;

        public SimulationWorld(long seed)
        {
            random = new DeterministicRandom(seed);
        }

        public SimulationEntity Spawn(long ownerUserId, SimVector2 position)
        {
            SimulationEntity entity = new SimulationEntity(nextEntityId++, ownerUserId, position);
            entities.Add(entity);
            return entity;
        }

        public void Step(IReadOnlyList<SimulationInput> confirmedInputs)
        {
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

            // Entity list is append-only inside a frame and IDs are monotonic, so this is ID order.
            for (int index = 0; index < entities.Count; index++)
            {
                SimulationEntity entity = entities[index];
                if (!entity.IsDestroyed)
                {
                    entity.Integrate(FixedDeltaTime);
                }
            }

            FrameNumber = checked(FrameNumber + 1);
        }

        public uint DrawRandomUInt32() => random.NextUInt32();

        public ulong CalculateStateHash()
        {
            CanonicalStateWriter writer = new CanonicalStateWriter();
            writer.WriteInt32(FrameNumber);
            writer.WriteInt32(nextEntityId);
            random.WriteState(writer);
            writer.WriteInt32(entities.Count);
            for (int index = 0; index < entities.Count; index++)
            {
                entities[index].WriteState(writer);
            }

            return writer.Hash;
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
                    FindRequired(input.EntityId).Destroy();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(input), input.Type, "Unknown input type");
            }
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
