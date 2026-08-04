using System.Collections.Generic;
using GameScene.Simulation.Core;

namespace GameScene.Simulation.Rendering
{
    public interface ISimulationEntityView
    {
        void ApplySimulationState(string status, IReadOnlyList<string> effects,
            IReadOnlyList<SimulationGaugeSnapshot> gauges, string master);

        void ApplyLocalEffects(IReadOnlyList<string> effects);
    }
}
