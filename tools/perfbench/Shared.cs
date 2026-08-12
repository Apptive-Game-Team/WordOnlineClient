using System;
using System.Collections.Generic;

namespace Bench
{
    // ── 실제 프로젝트 타입 (Unity 의존 제거, 필드 동일) ──────────────────────
    public enum CardType
    {
        Dummy, Shoot, Drop, Build, Spawn, Explode,
        Fire, Water, Lightning, Rock, Nature, Wind,
    }

    [Serializable]
    public class GameParameterData
    {
        public string gameObjectName;
        public string paramName;
        public float value;
    }

    public class CombinedMagicData
    {
        public long id;
        public string serverName;
        public string localizationKey;
        public string textLocalizationKey;
        public string resourceName;
        public CardType castType;
        public List<CardType> recipe;
    }

    // ── 테스트 데이터 생성 ───────────────────────────────────────────────────
    // 규모 근거:
    //  - 파라미터 테이블: 런타임 오브젝트 타입 ~94종(사운드 카탈로그 94행) × 파라미터 3~5개
    //    + 매직별 magic_id 행 → 400행으로 설정
    //  - 조합 마법 수 M: 아트 문서 기준 67종 → 60으로 설정
    public static class BenchData
    {
        public const int ParamRows = 400;
        public const int MagicCount = 60;

        public static List<GameParameterData> Table;
        public static List<CombinedMagicData> Magics;

        static readonly string[] ParamNames = { "hp", "damage", "duration", "radius", "range", "speed", "count" };

        public static void Build()
        {
            var rng = new Random(42);
            Table = new List<GameParameterData>(ParamRows);
            Magics = new List<CombinedMagicData>(MagicCount);

            var castTypes = new[] { CardType.Shoot, CardType.Drop, CardType.Build, CardType.Spawn, CardType.Explode };
            var elements = new[] { CardType.Fire, CardType.Water, CardType.Lightning, CardType.Rock, CardType.Nature, CardType.Wind };

            for (int m = 0; m < MagicCount; m++)
            {
                string objName = $"magic_object_{m}";
                var magic = new CombinedMagicData
                {
                    id = 1000 + m,
                    serverName = objName,
                    resourceName = $"MagicObject{m}",
                    localizationKey = $"magic.{m}",
                    castType = castTypes[m % castTypes.Length],
                    recipe = new List<CardType>
                    {
                        castTypes[m % castTypes.Length],
                        elements[m % elements.Length],
                        elements[(m * 7 + 3) % elements.Length],
                    },
                };
                Magics.Add(magic);

                // magic_id 행 (실데이터의 주 해석 경로)
                Table.Add(new GameParameterData { gameObjectName = objName, paramName = "magic_id", value = magic.id });

                // 오브젝트별 파라미터 행
                int paramCount = 3 + m % 3;
                for (int p = 0; p < paramCount && Table.Count < ParamRows; p++)
                {
                    Table.Add(new GameParameterData
                    {
                        gameObjectName = objName,
                        paramName = ParamNames[(m + p) % ParamNames.Length],
                        value = (float)(rng.NextDouble() * 10.0),
                    });
                }
            }

            // range가 없는 매직도 있으므로 cast-type family 행 추가 (실코드의 family 폴백 경로)
            foreach (var fam in new[] { "spawn", "drop", "explode", "build", "shoot" })
            {
                Table.Add(new GameParameterData { gameObjectName = fam, paramName = "range", value = 5f });
            }

            // 나머지는 무관 행으로 채워 400행 확보
            int filler = 0;
            while (Table.Count < ParamRows)
            {
                Table.Add(new GameParameterData
                {
                    gameObjectName = $"creature_{filler}",
                    paramName = ParamNames[filler % ParamNames.Length],
                    value = filler,
                });
                filler++;
            }

            // 모든 매직에 range 행이 있도록 보장하지는 않는다 — family 폴백이 실제로 뛰는 경우 포함
        }
    }
}
