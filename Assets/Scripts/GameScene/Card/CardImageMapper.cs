using System;
using System.Collections.Generic;
using Data;
using UnityEngine;

namespace GameScene.Card
{
    /// <summary>
    /// 원소 아이콘 표. 카드 앞면은 마법마다 다른 아트를
    /// <see cref="Data.Magic.CombinedMagicData.GetSprite"/> 로 읽으므로 이 표에는 원소 아이콘만 남는다.
    /// 원소 상성표와 도감 필터, 카드의 원소 표시가 읽는다.
    /// </summary>
    [CreateAssetMenu(fileName = "CardImageMapper", menuName = "ScriptableObjects/CardImageMapper", order = 1)]
    public class CardImageMapper : ScriptableObject
    {
        [Serializable]
        public class CardImageMapping
        {
            public ElementType elementType;
            public Sprite cardImage;
        }

        [SerializeField]
        private List<CardImageMapping> cardImageMappings;

        /// <summary>원소 아이콘. None 처럼 아트가 없는 원소는 null 이다.</summary>
        public Sprite GetElementImage(ElementType elementType)
        {
            if (cardImageMappings == null)
            {
                return null;
            }

            foreach (var mapping in cardImageMappings)
            {
                if (mapping.elementType == elementType)
                {
                    return mapping.cardImage;
                }
            }

            return null;
        }

        public Sprite GetElementImage(string elementName)
        {
            return Enum.TryParse(elementName, true, out ElementType elementType)
                ? GetElementImage(elementType)
                : null;
        }
    }
}
