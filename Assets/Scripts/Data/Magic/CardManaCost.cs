using System;
using System.Collections.Generic;
using Data.GameConfig;
using UnityEngine;

namespace Data.Magic
{
    /// <summary>
    /// 마법 카드 한 장이 먹는 마나. 서버는 마법 이름으로 parameters 의 (마법 이름, "mana_cost") 값을
    /// 차감하므로 여기서도 같은 값을 먼저 읽는다. parameters 가 아직 도착하지 않았으면
    /// <c>/api/data/magics</c> 가 준 <see cref="CombinedMagicData.manaCost"/> 로 떨어진다.
    /// </summary>
    public static class CardManaCost
    {
        private const string ManaCostParamName = "mana_cost";

        // 카드를 고를 때마다 파라미터 전체를 훑지 않도록 마법 이름별 결과를 담아 둔다.
        private static readonly Dictionary<string, int> Cache = new Dictionary<string, int>();
        private static IReadOnlyList<GameParameterData> cachedParameterSource;

        public static int Of(CombinedMagicData magic)
        {
            if (magic == null)
            {
                return 0;
            }

            var parameters = ParametersDataSource.GetCachedParameters();
            InvalidateCacheIfSourceChanged(parameters);

            string magicName = magic.serverName;
            if (!string.IsNullOrEmpty(magicName) && Cache.TryGetValue(magicName, out int cached))
            {
                return cached;
            }

            int cost = TryResolveFromParameters(parameters, magicName, out int resolved)
                ? resolved
                : magic.manaCost;

            if (!string.IsNullOrEmpty(magicName))
            {
                Cache[magicName] = cost;
            }

            return cost;
        }

        public static int Of(string magicName)
        {
            if (string.IsNullOrEmpty(magicName))
            {
                return 0;
            }

            if (LocalCombinedMagicData.TryGetByName(magicName, out CombinedMagicData magic))
            {
                return Of(magic);
            }

            var parameters = ParametersDataSource.GetCachedParameters();
            InvalidateCacheIfSourceChanged(parameters);
            return TryResolveFromParameters(parameters, magicName, out int resolved) ? resolved : 0;
        }

        /// <summary>
        /// ParametersDataSource는 갱신될 때마다 새 List를 배정하므로, 참조가 바뀌면 캐시를 버린다.
        /// 파라미터가 도착하기 전에 담아 둔 값도 이때 함께 버려진다.
        /// </summary>
        private static void InvalidateCacheIfSourceChanged(IReadOnlyList<GameParameterData> parameters)
        {
            if (ReferenceEquals(cachedParameterSource, parameters))
            {
                return;
            }

            cachedParameterSource = parameters;
            Cache.Clear();
        }

        private static bool TryResolveFromParameters(
            IReadOnlyList<GameParameterData> parameters,
            string magicName,
            out int value)
        {
            value = 0;
            if (parameters == null || string.IsNullOrEmpty(magicName))
            {
                return false;
            }

            for (int i = 0; i < parameters.Count; i++)
            {
                GameParameterData parameter = parameters[i];
                if (parameter == null)
                {
                    continue;
                }

                if (!string.Equals(parameter.paramName, ManaCostParamName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!string.Equals(parameter.gameObjectName, magicName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                value = Mathf.RoundToInt(parameter.value);
                return true;
            }

            return false;
        }
    }
}
