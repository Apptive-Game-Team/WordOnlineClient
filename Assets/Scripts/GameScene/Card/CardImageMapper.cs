using System;
using System.Collections.Generic;
using Scripts.Data;
using UnityEngine;

namespace Scripts.GameScene.Card
{
    [CreateAssetMenu(fileName = "CardImageMapper", menuName = "ScriptableObjects/CardImageMapper", order = 1)]
    public class CardImageMapper : ScriptableObject
    {
        [Serializable]
        public class CardImageMapping
        {
            public CardType CardType;
            public Sprite cardImage;
        }
        
        [SerializeField]
        private List<CardImageMapping> cardImageMappings;
        
        
        public Sprite GetCardImage(CardType cardType)
        {
            foreach (var mapping in cardImageMappings)
            {
                if (mapping.CardType == cardType)
                {
                    return mapping.cardImage;
                }
            }
            return null;
        }
        public Sprite GetCardImage(string cardType)
        {
            foreach (var mapping in cardImageMappings)
            {
                if (mapping.CardType.ToString() == cardType)
                {
                    return mapping.cardImage;
                }
            }
            return null;
        }
    }
}