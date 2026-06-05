using System;
using Data;
using Data.Magic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DeckScene
{
    public class DeckMagicPopupItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private const float MagicIconSize = 48f;
        private const float RecipeIconSize = 32f;
        private const float RecipeIconSpacing = 3f;

        private Image magicIconImage;
        private RectTransform recipeIconRoot;

        private CombinedMagicData magic;
        private Action<CombinedMagicData, RectTransform> onHoverEnter;
        private Action onHoverExit;

        public static DeckMagicPopupItem Create(Transform root)
        {
            GameObject rowObject = new GameObject("MagicItem", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement), typeof(DeckMagicPopupItem));
            rowObject.transform.SetParent(root, false);

            var layout = rowObject.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 6f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var rowLayout = rowObject.GetComponent<LayoutElement>();
            rowLayout.minHeight = MagicIconSize;
            rowLayout.preferredHeight = MagicIconSize;

            Image magicIcon = CreateImage(rowObject.transform, "MagicIcon", MagicIconSize);
            RectTransform recipeIconRoot = CreateRecipeIconRoot(rowObject.transform);

            DeckMagicPopupItem item = rowObject.GetComponent<DeckMagicPopupItem>();
            item.SetViews(magicIcon, recipeIconRoot);
            return item;
        }

        public void SetViews(Image magicIcon, RectTransform recipeRoot)
        {
            magicIconImage = magicIcon;
            recipeIconRoot = recipeRoot;
        }

        public void Bind(CombinedMagicData data, Action<CombinedMagicData, RectTransform> hoverEnter, Action hoverExit)
        {
            magic = data;
            onHoverEnter = hoverEnter;
            onHoverExit = hoverExit;

            if (magicIconImage != null)
            {
                magicIconImage.sprite = magic.GetSprite();
                magicIconImage.preserveAspect = true;
            }

            RenderRecipeIcons();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            onHoverEnter?.Invoke(magic, transform as RectTransform);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            onHoverExit?.Invoke();
        }

        private void RenderRecipeIcons()
        {
            if (recipeIconRoot == null)
            {
                return;
            }

            foreach (Transform child in recipeIconRoot)
            {
                Destroy(child.gameObject);
            }

            if (magic.recipe == null)
            {
                return;
            }

            foreach (CardType cardType in magic.recipe)
            {
                Image icon = CreateRecipeIcon(recipeIconRoot);
                icon.sprite = DeckCardSpriteResolver.GetCardSprite(cardType);
                icon.preserveAspect = true;
            }

            ResizeRecipeRoot(magic.recipe.Count);
            LayoutRebuilder.ForceRebuildLayoutImmediate(recipeIconRoot);
            if (transform is RectTransform rectTransform)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            }
        }

        private static Image CreateRecipeIcon(Transform root)
        {
            return CreateImage(root, "RecipeIcon", RecipeIconSize);
        }

        private static Image CreateImage(Transform root, string name, float size)
        {
            var iconObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
            iconObject.transform.SetParent(root, false);
            iconObject.GetComponent<RectTransform>().sizeDelta = new Vector2(size, size);
            var layout = iconObject.GetComponent<LayoutElement>();
            layout.minWidth = size;
            layout.minHeight = size;
            layout.preferredWidth = size;
            layout.preferredHeight = size;
            return iconObject.GetComponent<Image>();
        }

        private static RectTransform CreateRecipeIconRoot(Transform root)
        {
            GameObject recipeObject = new GameObject("RecipeIcons", typeof(RectTransform), typeof(GridLayoutGroup), typeof(LayoutElement));
            recipeObject.transform.SetParent(root, false);

            var layout = recipeObject.GetComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(RecipeIconSize, RecipeIconSize);
            layout.spacing = new Vector2(RecipeIconSpacing, 0f);
            layout.startAxis = GridLayoutGroup.Axis.Horizontal;
            layout.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            layout.constraintCount = 1;

            return recipeObject.GetComponent<RectTransform>();
        }

        private void ResizeRecipeRoot(int iconCount)
        {
            if (recipeIconRoot == null)
            {
                return;
            }

            float width = iconCount <= 0
                ? 0f
                : (RecipeIconSize * iconCount) + (RecipeIconSpacing * (iconCount - 1));

            recipeIconRoot.sizeDelta = new Vector2(width, RecipeIconSize);
            var layout = recipeIconRoot.GetComponent<LayoutElement>();
            if (layout != null)
            {
                layout.minWidth = width;
                layout.preferredWidth = width;
                layout.minHeight = RecipeIconSize;
                layout.preferredHeight = RecipeIconSize;
            }
        }
    }
}
