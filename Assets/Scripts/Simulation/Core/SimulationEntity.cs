namespace GameScene.Simulation.Core
{
    public sealed class SimulationEntity
    {
        public int Id { get; }
        public long OwnerUserId { get; }
        public SimVector2 Position { get; private set; }
        public SimVector2 Velocity { get; private set; }
        public bool IsDestroyed { get; private set; }

        internal SimulationEntity(int id, long ownerUserId, SimVector2 position)
        {
            Id = id;
            OwnerUserId = ownerUserId;
            Position = position;
            Velocity = SimVector2.Zero;
        }

        internal void SetVelocity(SimVector2 velocity) => Velocity = velocity;
        internal void Integrate(Fixed64 deltaTime) => Position += Velocity * deltaTime;
        internal void Destroy() => IsDestroyed = true;

        internal void WriteState(CanonicalStateWriter writer)
        {
            writer.WriteInt32(Id);
            writer.WriteInt64(OwnerUserId);
            writer.WriteFixed64(Position.X);
            writer.WriteFixed64(Position.Y);
            writer.WriteFixed64(Velocity.X);
            writer.WriteFixed64(Velocity.Y);
            writer.WriteBoolean(IsDestroyed);
        }
    }
}
