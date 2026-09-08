using UnityEngine;

namespace Data.Magic
{
    /// <summary>
    /// 마법 카드 한 장. 카드 한 장이 곧 마법 하나이므로 조합(recipe)도 시전 종류(castType)도 없다.
    /// 마나 비용과 조준 모양은 서버가 <c>/api/data/magics</c> 로 준 값을 그대로 담는다.
    /// </summary>
    public class CombinedMagicData
    {
        /// <summary>조준선을 직선으로 그리는 <see cref="aimShape"/> 값.</summary>
        public const int AimShapeLine = 1;

        /// <summary>조준선을 원으로 그리는 <see cref="aimShape"/> 값.</summary>
        public const int AimShapeCircle = 0;

        public long id;
        public string serverName;
        public string localizationKey;
        public string textLocalizationKey;
        public string resourceName;
        public ElementType element;
        public int manaCost;

        /// <summary>조준 표시 모양. 1이면 직선, 0이면 원.</summary>
        public int aimShape;

        public bool IsLineAim => aimShape == AimShapeLine;

        private const string SpriteResourceRoot = "Game/sprites";

        /// <summary>카드 앞면이자 도감 아이콘. 마법마다 다른 아트를 이름으로 찾는다.</summary>
        public Sprite GetSprite()
        {
            if (string.IsNullOrEmpty(resourceName))
            {
                return null;
            }

            return Resources.Load<Sprite>($"{SpriteResourceRoot}/{resourceName}");
        }
    }
}
