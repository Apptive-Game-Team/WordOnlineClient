using FixMath.NET;
using GameScene.Simulation.Core;
using GameScene.Simulation.Objects;
using GameScene.Simulation.Protocol;

namespace GameScene.Simulation.Pve
{
    public static class ProductionPveScenarioCatalog
    {
        public static PveScenarioCatalog Create() => new PveScenarioCatalog(new[]
        {
            Scenario(11, SimulationPrefabRegistry.PveNatureSlimeNestPrefabId, 14, 5),
            new PveScenarioDefinition(12, LockstepVersions.Config, 20 * 60 * 5, new[]
            {
                Spawn(0, SimulationPrefabRegistry.PveNatureSlimeNestPrefabId, 14, 7, true),
                Spawn(1, SimulationPrefabRegistry.PveWaterSlimeNestPrefabId, 14, 3, true)
            }),
            Scenario(13, SimulationPrefabRegistry.PveVineColonyPrefabId, 14, 5),
            Scenario(14, SimulationPrefabRegistry.PveVineWitchPrefabId, 14, 5)
        });

        private static PveScenarioDefinition Scenario(long id, string prefabId, int x, int y) =>
            new PveScenarioDefinition(id, LockstepVersions.Config, 20 * 60 * 5,
                new[] { Spawn(0, prefabId, x, y, true) });
        private static PveSpawnEvent Spawn(int sequence, string prefabId, int x, int y, bool objective) =>
            new PveSpawnEvent(0, sequence, prefabId, -1,
                new SimVector2((Fix64)x, (Fix64)y), objective);
    }
}
