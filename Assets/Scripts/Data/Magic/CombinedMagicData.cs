using System.Collections.Generic;
using UnityEngine;

namespace Data.Magic
{
    public class CombinedMagicData
    {
        public long id;
        public string serverName;
        public string localizationKey;
        public string textLocalizationKey;
        public string resourceName;
        public CardType castType;
        public List<CardType> recipe;

        /// <summary>
        /// 조준 표시를 적은 document. 서버가 주지 않았으면 null 이고, 그때는
        /// <see cref="GameScene.MagicIndicatorResolver"/> 가 <see cref="castType"/> 으로 갈라지는
        /// 이 변경 전 표시를 그린다.
        /// </summary>
        public MagicIndicatorDocument indicator;

        private const string SpriteResourceRoot = "Game/sprites";
        
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
