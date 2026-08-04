using System;
using System.Collections.Generic;
using Data.GameConfig;
using FixMath.NET;
using GameScene.Simulation.Mob;

namespace GameScene.Lockstep
{
    public static class ProductionMobDefinitionResolverFactory
    {
        public static ProductionMobDefinitionResolver Create()
        {
            IReadOnlyList<GameParameterData> cached = ParametersDataSource.GetCachedParameters();
            if (cached == null || cached.Count == 0)
                throw new InvalidOperationException("Versioned game parameters must be loaded before mob simulation starts");
            List<MobParameterValue> values = new List<MobParameterValue>(cached.Count);
            for (int index = 0; index < cached.Count; index++)
            {
                GameParameterData value = cached[index];
                values.Add(new MobParameterValue(value.gameObjectName, value.paramName, (Fix64)value.value));
            }
            return new ProductionMobDefinitionResolver(values);
        }
    }
}
