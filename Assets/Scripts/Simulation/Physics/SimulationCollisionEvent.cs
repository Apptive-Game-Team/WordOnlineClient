using GameScene.Simulation.Core;

namespace GameScene.Simulation.Physics
{
    public enum SimulationCollisionEventType : byte
    {
        Enter = 1,
        Exit = 2
    }

    public readonly struct SimulationCollisionEvent
    {
        public int FrameNumber { get; }
        public int EntityA { get; }
        public int EntityB { get; }
        public SimulationCollisionEventType Type { get; }
        public bool IsTrigger { get; }

        public SimulationCollisionEvent(
            int frameNumber,
            int entityA,
            int entityB,
            SimulationCollisionEventType type,
            bool isTrigger)
        {
            FrameNumber = frameNumber;
            EntityA = entityA < entityB ? entityA : entityB;
            EntityB = entityA < entityB ? entityB : entityA;
            Type = type;
            IsTrigger = isTrigger;
        }

        internal void WriteState(CanonicalStateWriter writer)
        {
            writer.WriteInt32(FrameNumber);
            writer.WriteInt32(EntityA);
            writer.WriteInt32(EntityB);
            writer.WriteInt32((int)Type);
            writer.WriteBoolean(IsTrigger);
        }
    }
}
