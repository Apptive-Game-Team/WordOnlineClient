using System.Collections.Generic;
using UnityEngine;

namespace Data.Magic
{
    public class CombinedMagicData
    {
        public long id;
        public string localizationKey;
        public string resourceName;
        public List<CardType> recipe;

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
