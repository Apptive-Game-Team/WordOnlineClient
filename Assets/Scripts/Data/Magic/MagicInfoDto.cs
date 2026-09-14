using System;

namespace Data.Magic
{
    /// <summary>
    /// <c>GET /api/data/magics</c> 의 magics 항목 하나.
    /// </summary>
    [Serializable]
    public class MagicInfoDto
    {
        public long id;
        public string name;
        public string text;

        /// <summary>Fire, Water, Lightning, Rock, Nature, Wind, None 중 하나.</summary>
        public string element;

        public int manaCost;

        /// <summary>
        /// 조준 표시를 적은 jsonb document. 서버가 항목 안에 그대로 실어 보낸다.
        /// 옛 서버나 이 필드가 없는 마법에서는 null 이다.
        /// </summary>
        public MagicIndicatorDocument indicator;
    }
}
