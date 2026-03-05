using System.Collections.Generic;
using UnityEngine;

namespace Data.Magic
{
    public class CombinedMagicData
    {
        public long id;
        public string magicName;
        public List<CardType> recipe;
        public string spritePath;
        
        public Sprite GetSprite()
        {
            return Resources.Load<Sprite>(spritePath);
        }
    }
}