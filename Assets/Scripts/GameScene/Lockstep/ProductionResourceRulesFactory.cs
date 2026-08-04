using System;
using System.Collections.Generic;
using Data.GameConfig;
using Data.Magic;
using GameScene.Simulation.Resources;

namespace GameScene.Lockstep
{
    public static class ProductionResourceRulesFactory
    {
        private const int FramesPerSecond = 20;
        private static readonly string[] CardNames =
        {
            "Shoot", "Build", "Spawn", "Explode", "Drop",
            "Fire", "Water", "Lightning", "Rock", "Nature", "Wind"
        };

        public static DeterministicGameRules Create()
        {
            IReadOnlyList<GameParameterData> parameters = ParametersDataSource.GetCachedParameters();
            if (parameters == null || parameters.Count == 0)
                throw new InvalidOperationException("Versioned game parameters must be loaded before lockstep starts");

            Dictionary<string, int> manaCosts = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int index = 0; index < CardNames.Length; index++)
                manaCosts.Add(CardNames[index], RequiredInt(parameters, CardNames[index].ToLowerInvariant(), "mana_cost"));

            List<IEnumerable<string>> recipes = new List<IEnumerable<string>>();
            foreach (CombinedMagicData magic in LocalCombinedMagicData.GetEffectiveDataList())
            {
                List<string> cards = new List<string>();
                for (int index = 0; index < magic.recipe.Count; index++) cards.Add(magic.recipe[index].ToString());
                recipes.Add(cards);
            }
            if (recipes.Count == 0) throw new InvalidOperationException("Versioned magic recipes must be loaded before lockstep starts");

            int durationMilliseconds = RequiredInt(parameters, "game", "duration");
            int durationFrames = checked((durationMilliseconds * FramesPerSecond + 999) / 1000);
            int feverMilliseconds = RequiredInt(parameters, "game", "fever_duration");
            int feverFrames = checked((feverMilliseconds * FramesPerSecond + 999) / 1000);
            return new DeterministicGameRules(
                RequiredInt(parameters, "player", "max_mana"),
                6,
                4,
                1,
                FramesPerSecond,
                durationFrames,
                manaCosts,
                recipes,
                feverFrames);
        }

        private static int RequiredInt(IReadOnlyList<GameParameterData> parameters, string objectName, string parameterName)
        {
            for (int index = 0; index < parameters.Count; index++)
            {
                GameParameterData value = parameters[index];
                if (string.Equals(value.gameObjectName, objectName, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(value.paramName, parameterName, StringComparison.OrdinalIgnoreCase))
                    return checked((int)Math.Round(value.value, MidpointRounding.AwayFromZero));
            }
            throw new InvalidOperationException("Missing deterministic parameter: " + objectName + "." + parameterName);
        }
    }
}
