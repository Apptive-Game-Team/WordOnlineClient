using GameScene.Simulation.Resources;

namespace GameScene.Simulation.Rendering
{
    public interface ISimulationPlayerUi
    {
        void Render(PlayerResourceSnapshot snapshot);
    }
}
