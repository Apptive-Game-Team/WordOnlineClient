using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Data
{
    public class CombinedMagicData
    {
        public string magicName;
        public List<CardType> recipe;
        public string spritePath;
        
        public Sprite GetSprite()
        {
            return Resources.Load<Sprite>(spritePath);
        }
    }
}