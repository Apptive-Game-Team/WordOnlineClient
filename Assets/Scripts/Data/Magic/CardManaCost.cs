using System;
using System.Collections.Generic;
using Data.GameConfig;
using UnityEngine;

namespace Data.Magic
{
    /// <summary>
    /// 카드 한 장이 먹는 마나. 서버는 <c>PlayerData.useCards</c>에서 카드마다
    /// parameters의 (카드 이름 소문자, "mana_cost") 값을 차감하므로 여기서도 같은 값을 읽는다.
    /// 파라미터가 아직 도착하지 않았을 때만 <see cref="LocalMagicData"/>의 내장 값으로 떨어진다.
    /// 내장 표는 서버와 어긋날 수 있다(Spawn은 내장 20, 서버 15).
    /// </summary>
    public static class CardManaCost
    {
        private const string ManaCostParamName = "mana_cost";

        // 카드를 고를 때마다 파라미터 전체를 훑지 않도록 카드 이름별 결과를 담아 둔다.
        private static readonly Dictionary<string, int> Cache = new Dictionary<string, int>();
        private static IReadOnlyList<GameParameterData> cachedParameterSource;

        public static int Of(CardType cardType)
        {
            return Of(cardType.ToString());
        }

        public static int Of(string cardName)
        {
            if (string.IsNullOrEmpty(cardName))
            {
                return 0;
            }

            var parameters = ParametersDataSource.GetCachedParameters();
            InvalidateCacheIfSourceChanged(parameters);

            if (Cache.TryGetValue(cardName, out int cached))
            {
                return cached;
            }

            int cost = Resolve(parameters, cardName);
            Cache[cardName] = cost;
            return cost;
        }

        /// <summary>레시피 한 벌이 먹는 마나. 서버도 카드마다 따로 차감하므로 단순 합이다.</summary>
        public static int SumOf(IList<CardType> recipe)
        {
            if (recipe == null)
            {
                return 0;
            }

            int total = 0;
            for (int i = 0; i < recipe.Count; i++)
            {
                total += Of(recipe[i]);
            }

            return total;
        }

        /// <summary>
        /// ParametersDataSource는 갱신될 때마다 새 List를 배정하므로, 참조가 바뀌면 캐시를 버린다.
        /// 파라미터가 도착하기 전에 담아 둔 내장 값도 이때 함께 버려진다.
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

        private static int Resolve(IReadOnlyList<GameParameterData> parameters, string cardName)
        {
            if (parameters != null)
            {
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

                    if (!string.Equals(parameter.gameObjectName, cardName, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    return Mathf.RoundToInt(parameter.value);
                }
            }

            MagicData localData = LocalMagicData.GetMagicData(cardName);
            return localData != null ? localData.mana : 0;
        }
    }
}
