using Data;
using GameScene.Card;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MagicBookScene
{
    // Draws the elemental effectiveness table from element icons and multiplier labels.
    // It replaces the baked elementChart.png so the numbers can follow the server chart
    // without a new art pass.
    public class ElementChartView : MonoBehaviour
    {
        // Mirrors com.wordonline.server.game.domain.magic.ElementType on the game server.
        // The declaration order matters: it is the order the server uses to index its chart.
        private enum ElementSlot
        {
            None,
            Fire,
            Water,
            Nature,
            Lightning,
            Rock,
            Wind,
        }

        // Row is the attacking element, column is the defending element.
        // Mirrors ElementalChart.CHART on the game server.
        private static readonly float[,] Multipliers =
        {
            //             None  Fire  Water Nature Lightning Rock  Wind
            /* None      */ { 1f, 1f, 1f, 1f, 1f, 1f, 1f },
            /* Fire      */ { 1f, 1f, 0.5f, 2f, 1f, 1f, 1f },
            /* Water     */ { 1f, 2f, 1f, 0.5f, 1f, 1f, 1f },
            /* Nature    */ { 1f, 0.5f, 2f, 1f, 2f, 0.5f, 1f },
            /* Lightning */ { 1f, 1f, 1f, 1f, 1f, 1f, 1f },
            /* Rock      */ { 1f, 0.5f, 1.5f, 1.5f, 0.5f, 1.5f, 0.5f },
            /* Wind      */ { 1f, 1f, 1f, 1f, 2f, 2f, 1f },
        };

        // None has no card art; its header cell is left empty.
        private static readonly CardType[] SlotCardTypes =
        {
            CardType.Dummy,
            CardType.Fire,
            CardType.Water,
            CardType.Nature,
            CardType.Lightning,
            CardType.Rock,
            CardType.Wind,
        };

        private const int ElementCount = 7;
        private const int LineCount = ElementCount + 1;
        private const float ChartPadding = 34f;
        private const float CellSpacing = 6f;
        private const float CellCornerScale = 6f;
        private const float IconInset = 8f;
        private const float MultiplierFontSize = 30f;
        private const float AxisFontSize = 18f;

        private static readonly Color CellColor = new Color(0.973f, 0.925f, 0.839f);
        private static readonly Color NeutralTextColor = new Color(0.478f, 0.384f, 0.282f);
        private static readonly Color StrongTextColor = new Color(0.753f, 0.337f, 0.184f);
        private static readonly Color WeakTextColor = new Color(0.290f, 0.494f, 0.659f);

        [SerializeField] private CardImageMapper cardImageMapper;
        [SerializeField] private TMP_FontAsset fontAsset;
        [SerializeField] private Sprite cellBackground;

        private bool built;

        private void OnEnable()
        {
            Build();
        }

        private void Build()
        {
            if (built)
            {
                return;
            }

            built = true;

            RectTransform grid = CreateGrid();
            CreateAxisCell(grid);

            for (int defender = 0; defender < ElementCount; defender++)
            {
                CreateIconCell(grid, (ElementSlot)defender);
            }

            for (int attacker = 0; attacker < ElementCount; attacker++)
            {
                CreateIconCell(grid, (ElementSlot)attacker);
                for (int defender = 0; defender < ElementCount; defender++)
                {
                    CreateMultiplierCell(grid, Multipliers[attacker, defender]);
                }
            }
        }

        private RectTransform CreateGrid()
        {
            var gridObject = new GameObject("ChartGrid", typeof(RectTransform), typeof(GridLayoutGroup));
            var gridRect = gridObject.GetComponent<RectTransform>();
            gridRect.SetParent(transform, false);
            gridRect.anchorMin = Vector2.zero;
            gridRect.anchorMax = Vector2.one;
            gridRect.offsetMin = new Vector2(ChartPadding, ChartPadding);
            gridRect.offsetMax = new Vector2(-ChartPadding, -ChartPadding);

            var grid = gridObject.GetComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = LineCount;
            grid.spacing = new Vector2(CellSpacing, CellSpacing);
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.cellSize = CalculateCellSize();
            return gridRect;
        }

        private Vector2 CalculateCellSize()
        {
            Rect panel = ((RectTransform)transform).rect;
            float totalSpacing = CellSpacing * (LineCount - 1);
            float width = (panel.width - ChartPadding * 2f - totalSpacing) / LineCount;
            float height = (panel.height - ChartPadding * 2f - totalSpacing) / LineCount;
            return new Vector2(Mathf.Max(width, 1f), Mathf.Max(height, 1f));
        }

        // The top-left corner names the two axes: rows attack, columns defend.
        private void CreateAxisCell(RectTransform grid)
        {
            GameObject cell = CreateCell(grid, "AxisCell");
            TMP_Text defenderLabel = CreateLabel(cell, "DefenderAxis", "DEF", AxisFontSize, NeutralTextColor);
            defenderLabel.alignment = TextAlignmentOptions.TopRight;
            TMP_Text attackerLabel = CreateLabel(cell, "AttackerAxis", "ATK", AxisFontSize, NeutralTextColor);
            attackerLabel.alignment = TextAlignmentOptions.BottomLeft;
        }

        private void CreateIconCell(RectTransform grid, ElementSlot slot)
        {
            GameObject cell = CreateCell(grid, slot + "Cell");
            Sprite sprite = ResolveIcon(slot);
            if (sprite == null)
            {
                // None has no card art. Leave the cell empty instead of drawing an
                // untextured Image, which shows up as a white box.
                return;
            }

            var iconObject = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.SetParent(cell.transform, false);
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(IconInset, IconInset);
            iconRect.offsetMax = new Vector2(-IconInset, -IconInset);

            var icon = iconObject.GetComponent<Image>();
            icon.sprite = sprite;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
        }

        private void CreateMultiplierCell(RectTransform grid, float multiplier)
        {
            GameObject cell = CreateCell(grid, "MultiplierCell");
            CreateLabel(cell, "Multiplier", FormatMultiplier(multiplier), MultiplierFontSize, ResolveTextColor(multiplier));
        }

        private GameObject CreateCell(RectTransform grid, string name)
        {
            var cellObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            cellObject.transform.SetParent(grid, false);

            var background = cellObject.GetComponent<Image>();
            // Same reason as the icon cells: an Image with no sprite draws a white box.
            background.enabled = cellBackground != null;
            background.sprite = cellBackground;
            background.type = Image.Type.Sliced;
            background.pixelsPerUnitMultiplier = CellCornerScale;
            background.color = CellColor;
            background.raycastTarget = false;
            return cellObject;
        }

        private TMP_Text CreateLabel(GameObject cell, string name, string value, float fontSize, Color color)
        {
            var labelObject = new GameObject(name, typeof(RectTransform));
            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.SetParent(cell.transform, false);
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var label = labelObject.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null)
            {
                label.font = fontAsset;
            }

            label.text = value;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            return label;
        }

        // Returns null when the slot has no card art, which is the case for None.
        private Sprite ResolveIcon(ElementSlot slot)
        {
            CardType cardType = SlotCardTypes[(int)slot];
            return cardImageMapper != null
                ? cardImageMapper.GetCardImage(cardType)
                : DeckScene.DeckCardSpriteResolver.GetCardSprite(cardType);
        }

        private static string FormatMultiplier(float multiplier)
        {
            return multiplier.ToString("0.##") + "x";
        }

        private static Color ResolveTextColor(float multiplier)
        {
            if (multiplier > 1f)
            {
                return StrongTextColor;
            }

            if (multiplier < 1f)
            {
                return WeakTextColor;
            }

            return NeutralTextColor;
        }
    }
}
