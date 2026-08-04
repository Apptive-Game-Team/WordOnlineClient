using System;
using FixMath.NET;
using GameScene.Simulation.Core;
using GameScene.Simulation.Objects;
using GameScene.Simulation.Protocol;

namespace GameScene.Simulation.Pve
{
    public static class ProductionPveScenarioCatalog
    {
        public static PveScenarioCatalog Create(int timeLimitFrames = 20 * 60 * 5)
        {
            if (timeLimitFrames <= 0) throw new ArgumentOutOfRangeException(nameof(timeLimitFrames));
            return new PveScenarioCatalog(new[]
            {
                Scenario(11, timeLimitFrames, SimulationPrefabRegistry.PveNatureSlimeNestPrefabId,
                    "enemy_boss_1", 14, 5, "pve_1_1_intro", "pve_1_1_enemy_line"),
                new PveScenarioDefinition(12, LockstepVersions.Config, timeLimitFrames, new[]
                {
                    Spawn(0, SimulationPrefabRegistry.PveNatureSlimeNestPrefabId,
                        "nature_nest", 14, 7, true),
                    Spawn(1, SimulationPrefabRegistry.PveWaterSlimeNestPrefabId,
                        "water_nest", 14, 3, true)
                }, Scripts("water_nest", "pve_1_2_intro", "pve_1_2_enemy_line")),
                Scenario(13, timeLimitFrames, SimulationPrefabRegistry.PveVineColonyPrefabId,
                    "pve_vine_colony", 14, 5, "pve_1_3_intro", "pve_1_3_enemy_line"),
                Scenario(14, timeLimitFrames, SimulationPrefabRegistry.PveVineWitchPrefabId,
                    "pve_vine_witch", 14, 5, "pve_1_4_intro", "pve_1_4_enemy_line")
            });
        }

        private static PveScenarioDefinition Scenario(long id, int timeLimitFrames,
            string prefabId, string installerId,
            int x, int y, string introKey, string enemyLineKey) =>
            new PveScenarioDefinition(id, LockstepVersions.Config, timeLimitFrames,
                new[] { Spawn(0, prefabId, installerId, x, y, true) },
                Scripts(installerId, introKey, enemyLineKey));
        private static PveSpawnEvent Spawn(int sequence, string prefabId, string installerId,
            int x, int y, bool objective) =>
            new PveSpawnEvent(0, sequence, prefabId, -1,
                new SimVector2((Fix64)x, (Fix64)y), objective, installerId);

        private static PveScriptEventDefinition[] Scripts(string speaker,
            string introKey, string enemyLineKey) => new[]
        {
            new PveScriptEventDefinition("intro", 10, speaker, introKey,
                StageLine(introKey), ObjectiveLine(introKey)),
            new PveScriptEventDefinition("enemyLine", 20, speaker, enemyLineKey,
                "Burn it all down!")
        };

        private static string StageLine(string introKey) => "Stage " + introKey.Substring(4, 3).Replace('_', '-');

        private static string ObjectiveLine(string introKey) => introKey == "pve_1_3_intro"
            ? "Destroy the vine colony!"
            : introKey == "pve_1_4_intro" ? "Destroy the vine witch!" : "Destroy the enemy nest!";
    }
}
