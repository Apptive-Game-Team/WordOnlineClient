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

        /// <summary>1이면 직선 조준, 0이면 원 조준.</summary>
        public int aimShape;
    }
}
