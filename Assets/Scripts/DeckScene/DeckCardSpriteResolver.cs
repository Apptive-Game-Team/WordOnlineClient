using System.Linq;
using Data;
using GameScene.Card;
using UnityEngine;

namespace DeckScene
{
    public static class DeckCardSpriteResolver
    {
        private const string CardImageMapperEditorPath = "Assets/Art/Images/UI/Card/CardImageMapper.asset";

        private static CardImageMapper cachedMapper;

        public static Sprite GetCardSprite(CardType cardType)
        {
            CardImageMapper mapper = ResolveMapper();
            return mapper != null ? mapper.GetCardImage(cardType) : null;
        }

        private static CardImageMapper ResolveMapper()
        {
            if (cachedMapper != null)
            {
                return cachedMapper;
            }

            cachedMapper = Resources.FindObjectsOfTypeAll<CardImageMapper>().FirstOrDefault();
            if (cachedMapper != null)
            {
                return cachedMapper;
            }

#if UNITY_EDITOR
            cachedMapper = UnityEditor.AssetDatabase.LoadAssetAtPath<CardImageMapper>(CardImageMapperEditorPath);
#endif
            return cachedMapper;
        }
    }
}
