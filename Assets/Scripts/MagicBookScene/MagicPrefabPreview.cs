using Data.Magic;
using GameScene.Object;
using GameScene.PopupBook;
using GameScene.ServedObjectComponent;
using GameScene.ServedObjectComponent.Sound;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MagicBookScene
{
    public sealed class MagicPrefabPreview : MonoBehaviour, IPointerClickHandler
    {
        private const int PreviewLayer = 31;
        private const int TextureSize = 512;
        private const float BoundsPadding = 1.3f;
        private const float ModalCanvasRatio = 0.82f;
        private const int RoundedCornerRadius = 12;
        private static readonly Vector3 PreviewOrigin = new Vector3(10000f, 10000f, 0f);
        private static Sprite roundedUiSprite;

        private Image fallbackImage;
        private CombinedMagicData currentData;
        private GameObject currentPrefab;
        private GameObject modalRoot;
        private RectTransform modalPanel;
        private RawImage previewImage;
        private Camera previewCamera;
        private RenderTexture renderTexture;
        private GameObject previewObject;
        private ServedObject servedObject;

        public static MagicPrefabPreview Attach(Image targetImage)
        {
            if (targetImage == null)
            {
                return null;
            }

            MagicPrefabPreview preview = targetImage.GetComponent<MagicPrefabPreview>();
            if (preview == null)
            {
                preview = targetImage.gameObject.AddComponent<MagicPrefabPreview>();
            }

            preview.Initialize(targetImage);
            return preview;
        }

        public bool Show(CombinedMagicData data)
        {
            CloseModal();
            currentData = data;
            currentPrefab = ResolvePreviewPrefab(data);
            return currentPrefab != null;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (currentPrefab != null)
            {
                OpenModal();
            }
        }

        public void PlayAttack()
        {
            servedObject?.PlayAttackPresentation();
        }

        private void Initialize(Image targetImage)
        {
            fallbackImage = targetImage;
            fallbackImage.raycastTarget = true;
        }

        private static GameObject ResolvePreviewPrefab(CombinedMagicData data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.resourceName))
            {
                return null;
            }

            GameObject prefab = Resources.Load<GameObject>($"Prefabs/{data.resourceName}");
            if (prefab == null ||
                prefab.GetComponent<ServedObject>() == null ||
                prefab.GetComponentInChildren<SpriteRenderer>(true) == null)
            {
                return null;
            }

            return prefab;
        }

        private void OpenModal()
        {
            if (currentData == null || currentPrefab == null)
            {
                return;
            }

            EnsureModalSurface();
            if (modalRoot == null)
            {
                return;
            }

            ClearPreviewObject();
            ResizeModalPanel();
            modalRoot.SetActive(true);
            modalRoot.transform.SetAsLastSibling();

            string prefabName = currentData.resourceName;
            previewObject = Instantiate(currentPrefab);
            previewObject.name = $"{currentPrefab.name} (Magic Book Preview)";
            SetLayerRecursively(previewObject.transform, PreviewLayer);
            DisableWorldSpaceUi(previewObject);

            servedObject = previewObject.GetComponent<ServedObject>();
            ServedObjectSfxController.Attach(servedObject, prefabName, true);
            servedObject.BindListeners();

            FitCamera();
            PopupBookVisualPresenter popupPresenter = PopupBookVisualPresenter.Attach(servedObject);
            servedObject.NotifySpawned();
            popupPresenter?.PlaySpawnPresentation(
                SpawnPresentationTypeCatalog.IsBuilding(prefabName));
        }

        private void EnsureModalSurface()
        {
            if (modalRoot != null)
            {
                return;
            }

            Canvas rootCanvas = fallbackImage.canvas != null
                ? fallbackImage.canvas.rootCanvas
                : FindObjectOfType<Canvas>();
            if (rootCanvas == null)
            {
                return;
            }

            int uiLayer = fallbackImage.gameObject.layer;
            modalRoot = CreateUiObject(
                "MagicPrefabPreviewModal",
                rootCanvas.transform,
                uiLayer,
                typeof(Image),
                typeof(Button));
            RectTransform rootRect = modalRoot.GetComponent<RectTransform>();
            Stretch(rootRect);

            Image backdrop = modalRoot.GetComponent<Image>();
            backdrop.color = new Color(0f, 0f, 0f, 0.72f);
            Button backdropButton = modalRoot.GetComponent<Button>();
            backdropButton.targetGraphic = backdrop;
            backdropButton.transition = UnityEngine.UI.Selectable.Transition.None;
            backdropButton.onClick.AddListener(CloseModal);

            GameObject panelObject = CreateUiObject(
                "PreviewPanel",
                modalRoot.transform,
                uiLayer,
                typeof(Image),
                typeof(Button),
                typeof(Shadow),
                typeof(Outline));
            modalPanel = panelObject.GetComponent<RectTransform>();
            modalPanel.anchorMin = new Vector2(0.5f, 0.5f);
            modalPanel.anchorMax = new Vector2(0.5f, 0.5f);
            modalPanel.pivot = new Vector2(0.5f, 0.5f);
            modalPanel.anchoredPosition = Vector2.zero;

            Image panelImage = panelObject.GetComponent<Image>();
            ApplyRoundedSprite(panelImage);
            panelImage.color = new Color(0.075f, 0.085f, 0.12f, 0.98f);
            Button panelBlocker = panelObject.GetComponent<Button>();
            panelBlocker.targetGraphic = panelImage;
            panelBlocker.transition = UnityEngine.UI.Selectable.Transition.None;

            Shadow panelShadow = panelObject.GetComponent<Shadow>();
            panelShadow.effectColor = new Color(0f, 0f, 0f, 0.55f);
            panelShadow.effectDistance = new Vector2(0f, -10f);
            panelShadow.useGraphicAlpha = true;

            Outline panelOutline = panelObject.GetComponent<Outline>();
            panelOutline.effectColor = new Color(0.42f, 0.48f, 0.62f, 0.7f);
            panelOutline.effectDistance = new Vector2(2f, -2f);
            panelOutline.useGraphicAlpha = true;

            GameObject previewFrame = CreateUiObject(
                "PreviewFrame",
                panelObject.transform,
                uiLayer,
                typeof(Image),
                typeof(Mask));
            RectTransform frameRect = previewFrame.GetComponent<RectTransform>();
            frameRect.anchorMin = new Vector2(0.055f, 0.055f);
            frameRect.anchorMax = new Vector2(0.945f, 0.945f);
            frameRect.offsetMin = Vector2.zero;
            frameRect.offsetMax = Vector2.zero;

            Image frameImage = previewFrame.GetComponent<Image>();
            ApplyRoundedSprite(frameImage);
            frameImage.color = new Color(0.025f, 0.03f, 0.045f, 1f);
            previewFrame.GetComponent<Mask>().showMaskGraphic = true;

            GameObject previewObject = CreateUiObject(
                "PrefabPreview",
                previewFrame.transform,
                uiLayer,
                typeof(RawImage),
                typeof(Button));
            RectTransform previewRect = previewObject.GetComponent<RectTransform>();
            Stretch(previewRect);
            previewRect.offsetMin = new Vector2(10f, 10f);
            previewRect.offsetMax = new Vector2(-10f, -10f);

            previewImage = previewObject.GetComponent<RawImage>();
            previewImage.color = Color.white;
            Button previewButton = previewObject.GetComponent<Button>();
            previewButton.targetGraphic = previewImage;
            previewButton.transition = UnityEngine.UI.Selectable.Transition.None;
            previewButton.onClick.AddListener(PlayAttack);

            CreateCloseButton(panelObject.transform, uiLayer);
            EnsureRenderSurface();
            modalRoot.SetActive(false);
        }

        private void CreateCloseButton(Transform parent, int uiLayer)
        {
            GameObject closeObject = CreateUiObject(
                "Close",
                parent,
                uiLayer,
                typeof(Image),
                typeof(Button));
            RectTransform closeRect = closeObject.GetComponent<RectTransform>();
            closeRect.anchorMin = Vector2.one;
            closeRect.anchorMax = Vector2.one;
            closeRect.pivot = new Vector2(0.5f, 0.5f);
            closeRect.anchoredPosition = new Vector2(-34f, -34f);
            closeRect.sizeDelta = new Vector2(48f, 48f);

            Image closeImage = closeObject.GetComponent<Image>();
            ApplyRoundedSprite(closeImage);
            closeImage.color = new Color(0.2f, 0.23f, 0.32f, 1f);
            Button closeButton = closeObject.GetComponent<Button>();
            closeButton.targetGraphic = closeImage;
            closeButton.onClick.AddListener(CloseModal);

            GameObject labelObject = CreateUiObject(
                "Label",
                closeObject.transform,
                uiLayer,
                typeof(Text));
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            Stretch(labelRect);

            Text label = labelObject.GetComponent<Text>();
            label.text = "\u00d7";
            label.alignment = TextAnchor.MiddleCenter;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 34;
            label.color = Color.white;
            label.raycastTarget = false;
        }

        private void EnsureRenderSurface()
        {
            renderTexture = new RenderTexture(TextureSize, TextureSize, 16, RenderTextureFormat.ARGB32)
            {
                name = "MagicBookPrefabPreview",
                antiAliasing = 2,
                filterMode = FilterMode.Bilinear,
            };
            renderTexture.Create();
            previewImage.texture = renderTexture;

            GameObject cameraObject = new GameObject("MagicBookPrefabPreviewCamera");
            cameraObject.transform.SetParent(modalRoot.transform, false);
            previewCamera = cameraObject.AddComponent<Camera>();
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = Color.clear;
            previewCamera.cullingMask = 1 << PreviewLayer;
            previewCamera.orthographic = true;
            previewCamera.nearClipPlane = 0.1f;
            previewCamera.farClipPlane = 100f;
            previewCamera.targetTexture = renderTexture;
        }

        private void ResizeModalPanel()
        {
            RectTransform canvasRect = modalRoot.transform.parent as RectTransform;
            float width = canvasRect != null ? canvasRect.rect.width : 800f;
            float height = canvasRect != null ? canvasRect.rect.height : 800f;
            float side = Mathf.Max(320f, Mathf.Min(width, height) * ModalCanvasRatio);
            modalPanel.sizeDelta = new Vector2(side, side);
        }

        private void FitCamera()
        {
            SpriteRenderer[] renderers = previewObject.GetComponentsInChildren<SpriteRenderer>(true);
            Bounds bounds = renderers[0].bounds;
            for (int index = 1; index < renderers.Length; index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            previewObject.transform.position += PreviewOrigin - bounds.center;
            float halfSize = Mathf.Max(bounds.extents.y, bounds.extents.x);
            previewCamera.orthographicSize = Mathf.Max(0.5f, halfSize * BoundsPadding);
            previewCamera.transform.position = PreviewOrigin + Vector3.back * 10f;
        }

        private void CloseModal()
        {
            ClearPreviewObject();
            if (modalRoot != null)
            {
                modalRoot.SetActive(false);
            }
        }

        private void ClearPreviewObject()
        {
            servedObject = null;
            if (previewObject == null)
            {
                return;
            }

            previewObject.SetActive(false);
            Destroy(previewObject);
            previewObject = null;
        }

        private static void DisableWorldSpaceUi(GameObject root)
        {
            foreach (Canvas canvas in root.GetComponentsInChildren<Canvas>(true))
            {
                canvas.enabled = false;
            }
        }

        private static void SetLayerRecursively(Transform root, int layer)
        {
            root.gameObject.layer = layer;
            foreach (Transform child in root)
            {
                SetLayerRecursively(child, layer);
            }
        }

        private static GameObject CreateUiObject(
            string objectName,
            Transform parent,
            int layer,
            params System.Type[] components)
        {
            GameObject uiObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer));
            uiObject.layer = layer;
            uiObject.transform.SetParent(parent, false);
            foreach (System.Type component in components)
            {
                uiObject.AddComponent(component);
            }

            return uiObject;
        }

        private static void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private static void ApplyRoundedSprite(Image image)
        {
            if (roundedUiSprite == null)
            {
                roundedUiSprite = CreateRoundedSprite();
            }

            image.sprite = roundedUiSprite;
            image.type = Image.Type.Sliced;
        }

        /// <summary>
        /// Draws the 9-sliced rounded rectangle the modal panels use.
        /// <para>
        /// This used to ask for Unity's own <c>UI/Skin/UISprite.psd</c>, but that sprite lives in
        /// <c>unity_builtin_extra</c>, which <see cref="Resources.GetBuiltinResource{T}"/> does not
        /// search — only the editor-only <c>AssetDatabase.GetBuiltinExtraResource</c> reaches it.
        /// The call therefore always failed and the panels rendered as hard rectangles. Drawing the
        /// corners here keeps the modal self-contained and works in a WebGL build.
        /// </para>
        /// </summary>
        private static Sprite CreateRoundedSprite()
        {
            const int size = RoundedCornerRadius * 2 + 2;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "MagicPreviewRoundedPanel",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    pixels[y * size + x] = new Color(1f, 1f, 1f, CornerCoverage(x, y, size));
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(RoundedCornerRadius, RoundedCornerRadius, RoundedCornerRadius, RoundedCornerRadius));
            sprite.name = "MagicPreviewRoundedPanel";
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }

        /// <summary>
        /// Alpha for one pixel: fully opaque inside the rounded body, feathered across one pixel at
        /// the corner arc so the edge does not stair-step.
        /// </summary>
        private static float CornerCoverage(int x, int y, int size)
        {
            float pixelX = x + 0.5f;
            float pixelY = y + 0.5f;
            float centerX = pixelX < size * 0.5f ? RoundedCornerRadius : size - RoundedCornerRadius;
            float centerY = pixelY < size * 0.5f ? RoundedCornerRadius : size - RoundedCornerRadius;

            bool insideHorizontalBand = pixelX >= RoundedCornerRadius && pixelX <= size - RoundedCornerRadius;
            bool insideVerticalBand = pixelY >= RoundedCornerRadius && pixelY <= size - RoundedCornerRadius;
            if (insideHorizontalBand || insideVerticalBand)
            {
                return 1f;
            }

            float distance = Vector2.Distance(new Vector2(pixelX, pixelY), new Vector2(centerX, centerY));
            return Mathf.Clamp01(RoundedCornerRadius - distance + 0.5f);
        }

        private void Update()
        {
            if (modalRoot != null && modalRoot.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            {
                CloseModal();
            }
        }

        private void OnDestroy()
        {
            CloseModal();
            if (renderTexture != null)
            {
                renderTexture.Release();
                Destroy(renderTexture);
            }

            if (modalRoot != null)
            {
                Destroy(modalRoot);
            }
        }
    }
}
