using System.Collections.Generic;
using Data;
using Data.Localization;
using Data.Magic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeckScene
{
    public class DeckInfoMagicPopup : MonoBehaviour
    {
        private const float DetailAnchorOffset = 12f;
        private const float DetailRecipeIconSize = 42f;
        private const float DetailRecipeIconSpacing = 4f;

        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Transform itemRoot;
        [SerializeField] private TMP_Text detailNameText;
        [SerializeField] private Transform detailRecipeIconRoot;

        private int detailRequestId;

        private void Awake()
        {
            panelRoot ??= gameObject;
            Hide();
        }

        public void Show(IReadOnlyList<CombinedMagicData> magics)
        {
            panelRoot ??= gameObject;
            ClearItems();
            ClearDetail();

            if (magics == null || magics.Count == 0)
            {
                AddEmptyItem();
            }
            else
            {
                foreach (CombinedMagicData magic in magics)
                {
                    AddMagicItem(magic);
                }
            }

            panelRoot.SetActive(true);
        }

        public void Hide()
        {
            GameObject root = panelRoot != null ? panelRoot : gameObject;
            root.SetActive(false);
        }

        public bool IsVisible()
        {
            GameObject root = panelRoot != null ? panelRoot : gameObject;
            return root.activeSelf;
        }

        private void AddMagicItem(CombinedMagicData magic)
        {
            if (itemRoot == null)
            {
                Debug.LogWarning("[DeckInfoMagicPopup] itemRoot is not assigned.");
                return;
            }

            DeckMagicPopupItem item = DeckMagicPopupItem.Create(itemRoot);
            item.Bind(magic, ShowDetail, ClearDetail);
        }

        private void AddEmptyItem()
        {
            if (itemRoot == null)
            {
                return;
            }

            GameObject textObject = new GameObject("EmptyMagicText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(itemRoot, false);
            var text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = "가능한 마법 없음";
            text.fontSize = 18f;
            text.alignment = TextAlignmentOptions.Center;
        }

        private async void ShowDetail(CombinedMagicData magic, RectTransform anchor)
        {
            if (magic == null)
            {
                return;
            }

            int requestId = ++detailRequestId;
            PlaceDetailNextTo(anchor);

            string localizedName = await LocaleUtils.GetStringAsync("Magic", magic.localizationKey);
            if (requestId != detailRequestId)
            {
                return;
            }

            if (detailNameText != null)
            {
                detailNameText.text = string.IsNullOrWhiteSpace(localizedName)
                    ? magic.localizationKey
                    : localizedName;
            }

            RenderRecipeIcons(detailRecipeIconRoot, magic.recipe, 42f);
            PlaceDetailNextTo(anchor);
        }

        private void ClearDetail()
        {
            detailRequestId++;

            if (detailNameText != null)
            {
                detailNameText.text = string.Empty;
            }

            ClearChildren(detailRecipeIconRoot);

        }

        private void PlaceDetailNextTo(RectTransform anchor)
        {
            RectTransform detailRect = GetDetailRoot();
            if (anchor == null || detailRect == null)
            {
                return;
            }

            var corners = new Vector3[4];
            anchor.GetWorldCorners(corners);
            Vector3 rightCenter = (corners[2] + corners[3]) * 0.5f;
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(GetCanvasCamera(detailRect), rightCenter);
            screenPoint.x += DetailAnchorOffset;

            detailRect.pivot = new Vector2(0f, 0.5f);
            RectTransform parentRect = detailRect.parent as RectTransform;
            if (parentRect == null)
            {
                detailRect.position = rightCenter + (anchor.right * DetailAnchorOffset);
                return;
            }

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, GetCanvasCamera(parentRect), out Vector2 localPoint))
            {
                detailRect.anchoredPosition = ClampToParent(detailRect, parentRect, localPoint);
            }
        }

        private RectTransform GetDetailRoot()
        {
            if (detailNameText != null && detailNameText.transform.parent is RectTransform nameParent)
            {
                return nameParent;
            }

            if (detailRecipeIconRoot != null && detailRecipeIconRoot.parent is RectTransform recipeParent)
            {
                return recipeParent;
            }

            return null;
        }

        private void ClearItems()
        {
            if (itemRoot == null)
            {
                return;
            }

            foreach (Transform child in itemRoot)
            {
                Destroy(child.gameObject);
            }
        }

        private static void RenderRecipeIcons(Transform root, IEnumerable<CardType> recipe, float iconSize)
        {
            if (root == null)
            {
                return;
            }

            EnsureGridLayout(root, iconSize);
            ClearChildren(root);
            if (recipe == null)
            {
                return;
            }

            int count = 0;
            foreach (CardType cardType in recipe)
            {
                Image icon = CreateImage(root, "RecipeIcon", iconSize);
                icon.sprite = DeckCardSpriteResolver.GetCardSprite(cardType);
                icon.preserveAspect = true;
                count++;
            }

            ResizeRecipeRoot(root, count, iconSize);
            if (root is RectTransform rectTransform)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            }
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

        private static void EnsureGridLayout(Transform root, float iconSize)
        {
            var horizontalLayout = root.GetComponent<HorizontalLayoutGroup>();
            if (horizontalLayout != null)
            {
                Destroy(horizontalLayout);
            }

            var gridLayout = root.GetComponent<GridLayoutGroup>();
            if (gridLayout == null)
            {
                gridLayout = root.gameObject.AddComponent<GridLayoutGroup>();
            }

            gridLayout.cellSize = new Vector2(iconSize, iconSize);
            gridLayout.spacing = new Vector2(DetailRecipeIconSpacing, 0f);
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            gridLayout.constraintCount = 1;
        }

        private static void ResizeRecipeRoot(Transform root, int iconCount, float iconSize)
        {
            if (root is not RectTransform rectTransform)
            {
                return;
            }

            float width = iconCount <= 0
                ? 0f
                : (iconSize * iconCount) + (DetailRecipeIconSpacing * (iconCount - 1));

            rectTransform.sizeDelta = new Vector2(width, iconSize);
            var layout = root.GetComponent<LayoutElement>() ?? root.gameObject.AddComponent<LayoutElement>();
            layout.minWidth = width;
            layout.preferredWidth = width;
            layout.minHeight = iconSize;
            layout.preferredHeight = iconSize;
        }

        private static void ClearChildren(Transform root)
        {
            if (root == null)
            {
                return;
            }

            foreach (Transform child in root)
            {
                Destroy(child.gameObject);
            }
        }

        private static Camera GetCanvasCamera(Component component)
        {
            Canvas canvas = component.GetComponentInParent<Canvas>();
            return canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;
        }

        private static Vector2 ClampToParent(RectTransform rect, RectTransform parent, Vector2 anchoredPosition)
        {
            Rect parentRect = parent.rect;
            Rect rectRect = rect.rect;

            float minX = parentRect.xMin;
            float maxX = parentRect.xMax - rectRect.width;
            float minY = parentRect.yMin + (rectRect.height * 0.5f);
            float maxY = parentRect.yMax - (rectRect.height * 0.5f);

            anchoredPosition.x = Mathf.Clamp(anchoredPosition.x, minX, Mathf.Max(minX, maxX));
            anchoredPosition.y = Mathf.Clamp(anchoredPosition.y, minY, Mathf.Max(minY, maxY));
            return anchoredPosition;
        }
    }
}
