using System;
using System.Collections.Generic;
using Data.GameConfig;
using FixMath.NET;
using GameScene.Simulation.Magic;
using GameScene.Simulation.Objects;

namespace GameScene.Lockstep
{
    public static class ProductionMagicCatalogFactory
    {
        public static ProductionMagicCatalog Create()
        {
            IReadOnlyList<GameParameterData> cached = ParametersDataSource.GetCachedParameters();
            if (cached == null || cached.Count == 0)
                throw new InvalidOperationException(
                    "Versioned game parameters must be loaded before magic simulation starts");
            List<MagicParameterValue> values = new List<MagicParameterValue>(cached.Count);
            for (int index = 0; index < cached.Count; index++)
            {
                GameParameterData value = cached[index];
                values.Add(new MagicParameterValue(value.gameObjectName, value.paramName, (Fix64)value.value));
            }
            return ProductionMagicCatalog.Create(values);
        }
    }

    public static class ProductionSimulationPrefabRegistryFactory
    {
        public static SimulationPrefabRegistry Create()
        {
            IReadOnlyList<GameParameterData> cached = ParametersDataSource.GetCachedParameters();
            if (cached == null || cached.Count == 0)
                throw new InvalidOperationException(
                    "Versioned game parameters must be loaded before prefab physics starts");
            List<SimulationPrefabParameterValue> values =
                new List<SimulationPrefabParameterValue>(cached.Count);
            for (int index = 0; index < cached.Count; index++)
            {
                GameParameterData value = cached[index];
                values.Add(new SimulationPrefabParameterValue(value.gameObjectName, value.paramName,
                    (Fix64)value.value));
            }
            return SimulationPrefabRegistry.CreateProduction(values);
        }
    }
}
