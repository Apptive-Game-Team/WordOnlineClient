using System.Linq;
using Data;
using Data.Magic;
using GameScene.Card;
using UnityEngine;

namespace DeckScene
{
    /// <summary>
    /// 카드 앞면과 원소 아이콘을 찾는다. 카드 앞면은 마법별 아트이고,
    /// 원소 아이콘만 <see cref="CardImageMapper"/> 에 남아 있다.
    /// </summary>
    public static class DeckCardSpriteResolver
    {
        private const string CardImageMapperEditorPath = "Assets/Art/Images/UI/Card/CardImageMapper.asset";

        private static CardImageMapper cachedMapper;

        public static Sprite GetMagicSprite(CombinedMagicData magic)
        {
            return magic?.GetSprite();
        }

        public static Sprite GetMagicSprite(string magicName)
        {
            return LocalCombinedMagicData.TryGetByName(magicName, out CombinedMagicData magic)
                ? magic.GetSprite()
                : null;
        }

        public static Sprite GetElementSprite(ElementType element)
        {
            CardImageMapper mapper = ResolveMapper();
            return mapper != null ? mapper.GetElementImage(element) : null;
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
