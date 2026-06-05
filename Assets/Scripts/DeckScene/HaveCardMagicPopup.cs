using System.Collections.Generic;
using Data;
using Data.Magic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeckScene
{
    public class HaveCardMagicPopup : MonoBehaviour
    {
        private const float AnchorOffset = 12f;

        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Transform itemRoot;

        private void Awake()
        {
            panelRoot ??= gameObject;
            Hide();
        }

        public void Show(IReadOnlyList<CombinedMagicData> magics)
        {
            Show(magics, null);
        }

        public void Show(IReadOnlyList<CombinedMagicData> magics, RectTransform anchor)
        {
            panelRoot ??= gameObject;
            ClearItems();

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
            if (panelRoot.transform is RectTransform panelRect)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
            }

            PlaceNextTo(anchor);
        }

        public void Hide()
        {
            GameObject root = panelRoot != null ? panelRoot : gameObject;
            root.SetActive(false);
        }

        private void AddMagicItem(CombinedMagicData magic)
        {
            if (itemRoot == null)
            {
                Debug.LogWarning("[HaveCardMagicPopup] itemRoot is not assigned.");
                return;
            }

            DeckMagicPopupItem item = DeckMagicPopupItem.Create(itemRoot);
            item.Bind(magic, null, null);
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
            text.text = "None";
            text.fontSize = 18f;
            text.alignment = TextAlignmentOptions.Center;
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

        private void PlaceNextTo(RectTransform anchor)
        {
            if (anchor == null || panelRoot == null)
            {
                return;
            }

            RectTransform panelRect = panelRoot.transform as RectTransform;
            if (panelRect == null)
            {
                return;
            }

            var corners = new Vector3[4];
            anchor.GetWorldCorners(corners);
            Vector3 rightCenter = (corners[2] + corners[3]) * 0.5f;
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(GetCanvasCamera(panelRect), rightCenter);
            screenPoint.x += AnchorOffset;

            panelRect.pivot = new Vector2(0f, 0.5f);
            RectTransform parentRect = panelRect.parent as RectTransform;
            if (parentRect == null)
            {
                panelRect.position = rightCenter + (anchor.right * AnchorOffset);
                return;
            }

            Camera camera = GetCanvasCamera(parentRect);
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(parentRect, screenPoint, camera, out Vector3 worldPoint))
            {
                panelRect.position = worldPoint;
            }
        }

        private static Camera GetCanvasCamera(Component component)
        {
            Canvas canvas = component.GetComponentInParent<Canvas>();
            return canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;
        }
    }
}
