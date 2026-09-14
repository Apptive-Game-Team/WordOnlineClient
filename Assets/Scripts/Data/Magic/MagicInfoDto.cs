using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Data.Magic
{
    [Serializable]
    public class MagicInfoDto : IMagicRecipeSource
    {
        public long id;
        public string name;
        public string text;
        public string castType;
        public List<string> cards;

        /// <summary>
        /// 조준 표시를 적은 jsonb document. 서버가 <c>/api/data/magics</c> 항목 안에 그대로 실어 보낸다.
        /// 옛 서버나 이 필드가 없는 마법에서는 null 이다.
        /// </summary>
        public MagicIndicatorDocument indicator;

        // 아래는 전부 위 필드를 그대로 읽는 property 다. [JsonIgnore] 가 없으면 Json.NET 이 getter 도
        // 직렬화해서 PlayerPrefs cache 에 name 과 Name 이 나란히 들어간다. indicator 는 document 통째로
        // 두 번 실린다. MagicInfoResponse 가 같은 이유로 자기 interface property 에 [JsonIgnore] 를 달고 있다.

        [JsonIgnore]
        public long Id => id;

        [JsonIgnore]
        public string Name => name;

        [JsonIgnore]
        public string Text => text;

        [JsonIgnore]
        public string CastType => castType;

        [JsonIgnore]
        public IReadOnlyList<string> Cards => cards;

        [JsonIgnore]
        public MagicIndicatorDocument Indicator => indicator;
    }
}
