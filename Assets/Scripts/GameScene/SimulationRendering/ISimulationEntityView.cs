using System.Collections.Generic;
using GameScene.Simulation.Core;

namespace GameScene.Simulation.Rendering
{
    public interface ISimulationEntityView
    {
        void ApplySimulationState(string status, IReadOnlyList<string> effects,
            IReadOnlyList<SimulationGaugeSnapshot> gauges, string master);

        void ApplyLocalEffects(IReadOnlyList<string> effects);

        /// <summary>
        /// Hands the view to its prefab presentation components. Called once, after the first
        /// snapshot has configured the view, so nothing subscribes to a half-built object.
        /// </summary>
        void BindListeners();

        /// <summary>Called once, right after <see cref="BindListeners"/>, on a real spawn.</summary>
        void NotifySpawned();

        /// <summary>Called on every snapshot in which the simulation moved this entity.</summary>
        void NotifyMoved();
    }
}
