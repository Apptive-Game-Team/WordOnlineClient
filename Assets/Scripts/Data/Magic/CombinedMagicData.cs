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
