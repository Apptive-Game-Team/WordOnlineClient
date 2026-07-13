using GameScene.Simulation.Core;

namespace GameScene.Simulation.Objects
{
    public sealed class SimulationComponent
    {
        public string Id { get; }
        public int SpawnFrame { get; }
        public int DestroyFrame { get; private set; } = -1;
        public bool IsDestroyed => DestroyFrame >= 0;

        internal SimulationComponent(string id, int spawnFrame)
        {
            Id = id;
            SpawnFrame = spawnFrame;
        }

        internal void Destroy(int frameNumber)
        {
            if (!IsDestroyed) DestroyFrame = frameNumber;
        }

        internal void WriteState(CanonicalStateWriter writer)
        {
            writer.WriteString(Id);
            writer.WriteInt32(SpawnFrame);
            writer.WriteInt32(DestroyFrame);
        }
    }
}
