namespace WordOnline.Simulation.Core
{
    public enum SimulationInputType : byte
    {
        SetVelocity = 1,
        Spawn = 2,
        Destroy = 3
    }

    public readonly struct SimulationInput
    {
        public long UserId { get; }
        public int Sequence { get; }
        public SimulationInputType Type { get; }
        public int EntityId { get; }
        public SimVector2 Value { get; }

        public SimulationInput(
            long userId,
            int sequence,
            SimulationInputType type,
            int entityId,
            SimVector2 value)
        {
            UserId = userId;
            Sequence = sequence;
            Type = type;
            EntityId = entityId;
            Value = value;
        }
    }
}
